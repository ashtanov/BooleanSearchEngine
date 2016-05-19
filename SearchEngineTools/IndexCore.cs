using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Set = System.Collections.Generic.SortedSet<int>;
using PositionDict = System.Collections.Generic.SortedDictionary<int, System.Collections.Generic.SortedSet<int>>;
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
        private ConcurrentDictionary<string, int> idf;
        private Dictionary<string, int> wordToInt;
        private Dictionary<int, string> intToWord;
        private int wordsCount;

        public double[] weights = { 1, 0.3 };
        public double k1 = 2;

        public IndexCore() : this(Guid.NewGuid().ToString("N"))
        { }

        public IndexCore(string name)
        {
            indexName = name;
            normalizer = new WordCaseNormalizer();
            index = new Dictionary<string, PositionDict>();
            storage = new MongoStorage();
            idf = new ConcurrentDictionary<string, int>();
            wordToInt = new Dictionary<string, int>();
            intToWord = new Dictionary<int, string>();
            wordsCount = 0;
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
                int mPos = InsertInIndex(doc, 0, (x) => x.title);
                InsertInIndex(doc, 200, (x) => x.description);
            }
        }

        public Response SearchQuery(string query)
        {
            var words = ParseHelper.FindAllWords(query)
                .Select(x => normalizer.NormalizeWord(x))
                .Where(x => index.ContainsKey(x))// исключаем слова, не содержащиеся в словаре
                .ToArray();
            if (words.Length != 0)
            {
                var res = words.Length == 1 ? SearchFull(words) : DistanceSearch(words, 10);
                if (res.Length == 0)
                    res = SearchFull(words);
                var ranked = res
                    .Select(x => new {score = BM25F(x, words), doc = x})
                    .OrderByDescending(x => x.score)
                    .Take(100)
                    .Select((s, i) =>
                    {
                        s.doc.rank = i;
                        return s.doc;
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
                query = words.Select(x => new WordDoc {word = x}).ToArray()
            };
        }

        private double BM25F(Document doc, string[] words)
        {
            double sum = 0;
            foreach (var w in words)
            {
                double tfTitl = 0;
                double tfDesc = 0;
                foreach (var coord in index[w][doc.intId])
                    if (coord < 200)
                        tfTitl += 1;
                    else
                        tfDesc += 1;
                tfTitl = tfTitl / ParseHelper.FindAllWords(doc.title).Count;
                tfDesc = doc.len != 0 ? (tfDesc / doc.len) : 0;
                double tf = tfTitl * weights[0] + tfDesc * weights[1];
                sum += tf / (k1 + tf) * Math.Log((wordsCount + 0.0) / idf[w]);
            }
            return sum;
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
                    if (current[j].docId == res[i][k].docId)
                    {
                        if (current[j].sPos == res[i][k].fPos)
                            intersect.Add(res[i][k]);
                        if (j + 1 < current.Count && current[j + 1].docId == res[i][k].docId)
                            j++;
                        else if (k + 1 < res[i].Count && res[i][k + 1].docId == current[j].docId)
                            k++;
                        else
                        {
                            k++;
                            j++;
                        }
                    }
                    else if (current[j].docId < res[i][k].docId)
                        j++;
                    else
                        k++;
                current = intersect;
            }
            return storage.GetRange(current.Select(x => x.docId).ToList()).ToArray();
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
                //idf
                bw.Write(idf.Count);
                foreach (var kvp in idf)
                {
                    bw.Write(kvp.Key);
                    bw.Write(kvp.Value);
                }

                //TODO: сериализовать idf wordToInt intToWord 
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
                //idf
                int idfCount = br.ReadInt32();
                for (int i = 0; i < idfCount; ++i)
                {
                    var key = br.ReadString();
                    int val = br.ReadInt32();
                    ind.wordsCount += val;
                    ind.idf.TryAdd(key, val);
                }

            }

            //TODO: десериализовать wordToInt intToWord
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
                    {
                        while (i + jumpb < first.Count && first[i + jumpb] < second[k])
                        {
                            i += jumpb;
                        }
                        i++;
                    }
                    else
                        i++;
                }
                else
                {
                    if (k % jumpa == 0)
                    {
                        while (k + jumpa < second.Count && second[k + jumpa] < first[i])
                            k += jumpa;
                        k++;
                    }
                    else
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
                wordsCount++;
                if (wordToInt.ContainsKey(nword) && index.TryGetValue(nword, out tmp))
                {
                    int currentid = wordToInt[nword];
                    idf[nword] += 1;
                    if (tmp.ContainsKey(doc.intId))
                        tmp[doc.intId].Add(wordPos);
                    else
                        tmp.Add(doc.intId, new Set { wordPos });
                }
                else
                {
                    int currentid = wordToInt.Count;
                    wordToInt.Add(nword, currentid);
                    intToWord.Add(currentid, nword);

                    idf.AddOrUpdate(nword, 1, (x, y) => y + 1);
                    index.Add(nword,
                        new PositionDict
                        {
                            { doc.intId, new Set {wordPos} }
                        });
                }
                wordPos++;
            }
            return wordPos;
        }

        public class Coord
        {
            public int docId { get; set; }
            public int fPos { get; set; }
            public int sPos { get; set; }

            public override string ToString()
            {
                return $"id:{docId} {{{fPos} {sPos}}}";
            }

            public override int GetHashCode()
            {
                return docId;
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
                        foreach (var ps in l)
                            answer.Add(new Coord { docId = docId, fPos = posA[fPos], sPos = ps });
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
