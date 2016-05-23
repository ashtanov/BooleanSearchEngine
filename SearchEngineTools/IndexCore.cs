using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Set = SearchEngineTools.CompressedSortedList;
using PositionDict = System.Collections.Generic.SortedDictionary<int, SearchEngineTools.CompressedSortedList>;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

namespace SearchEngineTools
{
    public class IndexCore
    {
        private string indexName;
        private IWordNormalizer normalizer;
        private IDocumentStorage storage;
        private Dictionary<string, PositionDict> index;
        private int docsCount;

        public double[] Weights = { 1, 0.3 };
        public double K1 = 2;

        public IndexCore() : this(Guid.NewGuid().ToString("N"))
        { }

        public IndexCore(string name)
        {
            indexName = name;
            normalizer = new WordCaseNormalizer();
            index = new Dictionary<string, PositionDict>();
            storage = new MongoStorage();
            docsCount = 0;
        }

        public void ClearIndex()
        {
            storage.ClearStorage();
        }

        public void Add(Document d)
        {
            AddRange(new[] { d });
        }

        public void AddRange(IEnumerable<Document> docs)
        {
            storage.AddRange(docs);
            foreach (var doc in docs)
            {
                docsCount++;
                InsertInIndex(doc, 0, (x) => x.title);
                InsertInIndex(doc, 200, (x) => x.description);
            }
        }

        public Response SearchQuery(string query, int distInt)
        {
            var words = ParseHelper.FindAllWords(query)
                .Select(x => normalizer.NormalizeWord(x))
                .Where(x => index.ContainsKey(x))// исключаем слова, не содержащиеся в словаре
                .ToArray();
            if (words.Length != 0)
            {
                Document[] res;
                if (distInt < 1 || words.Length == 1)
                    res = SearchFull(words);
                else
                {
                    res = DistanceSearch(words, distInt);
                    if (res.Length == 0)
                        res = SearchFull(words);
                }

                var ranked = res
                    .Select(x =>
                    {
                        x.rank = BM25F(x, words);
                        return x;
                    }).OrderByDescending(x => x.rank)
                    .Take(100).Select(r =>
                    {
                        r.cos = Cos(r, words);
                        return r;
                    }).ToArray();
                WordDoc[] wdp = new WordDoc[words.Length];
                for (int i = 0; i < wdp.Length; ++i)
                {
                    wdp[i] = new WordDoc
                    {
                        word = words[i],
                        docs = new DocPos[ranked.Length]
                    };
                    for (int k = 0; k < wdp[i].docs.Length; ++k)
                        wdp[i].docs[k] = new DocPos
                        {
                            docId = ranked[k].id,
                            pos = index[words[i]][ranked[k].intId].ToArray()
                        };
                }
                return new Response
                {
                    documents = ranked,
                    query = wdp
                };
            }
            return new Response
            {
                documents = null,
                query = words.Select(x => new WordDoc { word = x }).ToArray(),
                errorCode = 1
            };
        }

        private double BM25F(Document doc, string[] words)
        {
            double sum = 0;
            foreach (var w in words)
            {
                double tfTitl = 0;
                double tfDesc = 0;
                foreach (var coord in index[w][doc.intId].ToList())
                    if (coord < 200)
                        tfTitl += 1;
                    else
                        tfDesc += 1;
                tfTitl = tfTitl / doc.tlen;
                tfDesc = doc.len != 0 ? (tfDesc / doc.len) : 0;
                double tf = tfTitl * Weights[0] + tfDesc * Weights[1];
                sum += tf / (K1 + tf) * CalcIdf(w);
            }
            return sum;
        }

        private double CalcIdf(string w)
        {
            return Math.Log((docsCount + 0.0) / index[w].Count);
        }

        /// <summary>
        /// Косинусная мера похожести запроса и документа
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="queryWords">должно быть нормализованным!</param>
        /// <returns></returns>
        private double Cos(Document doc, string[] queryWords)
        {
            double AB = 0;
            Dictionary<string, double> tfIdfDoc = new Dictionary<string, double>();
            Dictionary<string, double> tfIdfQuery = new Dictionary<string, double>();
            var docWords = ParseHelper.FindAllWords(doc.description + " " + doc.title);
            foreach (var w in docWords
                .Select(x => normalizer.NormalizeWord(x))
                .Distinct())
                tfIdfDoc.Add(w, (index[w][doc.intId].Count / (docWords.Count + 0.0)) * CalcIdf(w));

            foreach (var w in queryWords.GroupBy(x => x))
                tfIdfQuery.Add(w.Key, w.Count() / (queryWords.Length + 0.0) * CalcIdf(w.Key));

            foreach (var w in tfIdfQuery)
                if (tfIdfDoc.ContainsKey(w.Key))
                    AB += tfIdfDoc[w.Key] * w.Value;

            AB = Math.Sqrt(AB);
            var A2 = Math.Sqrt(tfIdfDoc.Sum(x => x.Value * x.Value));
            var B2 = Math.Sqrt(tfIdfQuery.Sum(x => x.Value * x.Value));

            return AB / (A2 * B2);

        }

        private Document[] SearchFull(string[] queryWords)
        {
            List<List<int>> pdList = new List<List<int>>();
            foreach (var word in queryWords)
                pdList.Add(index[word].Keys.ToList());
            pdList = pdList.OrderBy(x => x.Count).ToList();
            IList<int> current = pdList[0];
            for (int i = 1; i < pdList.Count; ++i)
                current = SmartIntersect(current, pdList[i]);
            return storage.GetRange(current).ToArray();
        }

        private Document[] DistanceSearch(string[] queryWords, int distance)
        {
            List<Tuple<string, string>> tmp = new List<Tuple<string, string>>();
            for (int i = 0; i < queryWords.Length; ++i)
                queryWords[i] = normalizer.NormalizeWord(queryWords[i]);
            for (int i = 1; i < queryWords.Length; i++)
                tmp.Add(new Tuple<string, string>(queryWords[i - 1], queryWords[i]));
            List<IList<Coord>> res = new List<IList<Coord>>();
            foreach (var ds in tmp)
            {
                res.Add(
                    DistanceSearch2Docs(
                        index[ds.Item1],
                        index[ds.Item2],
                        distance)
                    );
            }
            IList<Coord> current = res[0];
            for (int i = 1; i < res.Count; ++i)
            {
                List<Coord> intersect = new List<Coord>();
                int j = 0, k = 0;
                while (j < current.Count && k < res[i].Count)
                    if (current[j].DocId == res[i][k].DocId)
                    {
                        if (current[j].SPos == res[i][k].FPos)
                            intersect.Add(res[i][k]);

                        if (j + 1 < current.Count && current[j + 1].DocId == res[i][k].DocId)
                            j++;
                        else if (k + 1 < res[i].Count && res[i][k + 1].DocId == current[j].DocId)
                            k++;
                        else
                        {
                            k++;
                            j++;
                        }
                    }
                    else if (current[j].DocId < res[i][k].DocId)
                        j++;
                    else
                        k++;
                current = intersect;
            }
            return storage.GetRange(current.Select(x => x.DocId).ToList()).ToArray();
        }

        #region De/Serialize
        public void Serialize(string filePath)
        {
            using (var bw = new BinaryWriter(new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)))
            {
                bw.Write(indexName);
                //index
                bw.Write(index.Count);
                foreach (var t in index)
                {
                    bw.Write(t.Key);
                    bw.WriteCompressedInt(t.Value.Count);
                    List<int> docIds = t.Value.Keys.ToList();
                    bw.Write(docIds[0]);
                    List<int> coord1 = t.Value[docIds[0]].ToList();
                    bw.WriteCompressedInt(coord1.Count);
                    bw.Write(coord1[0]);
                    for (int k = 1; k < coord1.Count; ++k)
                        bw.WriteCompressedInt(coord1[k] - coord1[k - 1]);
                    for (int i = 1; i < docIds.Count; ++i)
                    {
                        bw.WriteCompressedInt(docIds[i] - docIds[i - 1]);
                        bw.WriteCompressedInt(t.Value[docIds[i]].Count);
                        List<int> coord = t.Value[docIds[i]].ToList();
                        bw.Write(coord[0]);
                        for (int k = 1; k < coord.Count; ++k)
                            bw.WriteCompressedInt(coord[k] - coord[k - 1]);
                    }
                }
                bw.Write(docsCount);
            }
        }

        public static IndexCore Deserialize(string filePath)
        {
            IndexCore ind;
            using (var br = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                ind = new IndexCore(br.ReadString());
                //index
                var count = br.ReadInt32();
                for (var i = 0; i < count; ++i)
                {
                    var word = br.ReadString();
                    int scount = br.ReadCompressedInt();
                    int doc0 = br.ReadInt32();
                    int coord0Count = br.ReadCompressedInt();
                    int coord0 = br.ReadInt32();
                    List<int> coord0List = new List<int> { coord0 };
                    for (int k = 1; k < coord0Count; k++)
                        coord0List.Add(coord0List[k - 1] + br.ReadCompressedInt());
                    var tmp = new PositionDict { { doc0, new Set(coord0List) } };
                    for (int k = 1; k < scount; ++k)
                    {
                        int docId = br.ReadCompressedInt() + doc0;
                        doc0 = docId;
                        int coordCount = br.ReadCompressedInt();
                        int fCoord = br.ReadInt32();
                        List<int> coordList = new List<int> { fCoord };
                        for (int l = 1; l < coordCount; l++)
                            coordList.Add(coordList[l - 1] + br.ReadCompressedInt());
                        tmp.Add(docId, new Set(coordList));
                    }
                    ind.index.Add(word, tmp);
                }
                ind.docsCount = br.ReadInt32();
            }
            return ind;
        }
        #endregion
        #region Helpers
        private IList<int> SmartIntersect(IList<int> first, IList<int> second)
        {
            int jumpa = (int)Math.Sqrt(first.Count);
            int jumpb = (int)Math.Sqrt(second.Count);
            List<int> result = new List<int>();
            int i = 0, k = 0;
            while (i < first.Count && k < second.Count)
            {
                if (first[i] == second[k])
                {
                    result.Add(first[i]);
                    k++;
                    i++;
                }
                else if (first[i] < second[k])
                {
                    if (i % jumpb == 0)
                        while (i + jumpb < first.Count && first[i + jumpb] < second[k])
                            i += jumpb;
                    i++;
                }
                else
                {
                    if (k % jumpa == 0)
                        while (k + jumpa < second.Count && second[k + jumpa] < first[i])
                            k += jumpa;
                    k++;
                }
            }
            return result;
        }

        private int InsertInIndex(Document doc, int posStart, Func<Document, string> field)
        {
            int wordPos = posStart;
            foreach (var word in ParseHelper.FindAllWords(field(doc)))
            {
                string nword = normalizer.NormalizeWord(word);
                PositionDict tmp;
                if (index.TryGetValue(nword, out tmp))
                    if (tmp.ContainsKey(doc.intId))
                        tmp[doc.intId].Add(wordPos);
                    else
                        tmp.Add(doc.intId, new Set(new[] { wordPos }));
                else
                    index.Add(nword,
                        new PositionDict
                        {
                            { doc.intId, new Set(new []{ wordPos })}
                        });
                wordPos++;
            }
            return wordPos;
        }

        public class Coord
        {
            public int DocId { get; set; }
            public int FPos { get; set; }
            public int SPos { get; set; }

            public override string ToString()
            {
                return $"id:{DocId} {{{FPos} {SPos}}}";
            }

            public override int GetHashCode()
            {
                return DocId;
            }

            public override bool Equals(object obj)
            {
                return GetHashCode() == obj.GetHashCode();
            }
        }

        private IList<Coord> DistanceSearch2Docs(PositionDict a, PositionDict b, int distance)
        {
            int i = 0, k = 0;
            IList<int> first = a.Keys.ToList();
            IList<int> second = b.Keys.ToList();
            int jumpa = (int)Math.Sqrt(first.Count);
            int jumpb = (int)Math.Sqrt(second.Count);
            List<Coord> answer = new List<Coord>();
            while (i < first.Count && k < second.Count)
            {
                if (first[i] == second[k])
                {
                    int docId = first[i];
                    List<int> l = new List<int>();
                    int fPos = 0;
                    int sPos = 0;
                    List<int> posA = a[docId].ToList();
                    while (fPos < posA.Count)
                    {
                        List<int> posB = b[docId].ToList();
                        while (sPos < posB.Count)
                        {
                            if (Math.Abs(posA[fPos] - posB[sPos]) <= distance)
                                l.Add(posB[sPos]);
                            else if (posB[sPos] > posA[fPos])
                                break;
                            sPos++;
                        }
                        while (l.Count != 0 && Math.Abs(l[0] - posA[fPos]) > distance)
                            l.RemoveAt(0);
                        answer.AddRange(l.Select(ps => new Coord {DocId = docId, FPos = posA[fPos], SPos = ps}));
                        fPos++;
                    }
                    k++;
                    i++;
                }
                else if (first[i] < second[k])
                    if (i % jumpb == 0)
                    {
                        while (i + jumpb < first.Count && first[i + jumpb] < second[k])
                            i += jumpb;
                        i++;
                    }
                    else
                        i++;
                else
                    if (k % jumpa == 0)
                {
                    while (k + jumpa < second.Count && second[k + jumpa] < first[i])
                        k += jumpa;
                    k++;
                }
                else
                    k++;
            }
            return answer;
        }
        #endregion
    }
}
