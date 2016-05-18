using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Set = System.Collections.Generic.SortedSet<int>;
using PositionDict = System.Collections.Generic.SortedDictionary<int, System.Collections.Generic.SortedSet<int>>;
using System.Collections.Concurrent;

namespace SearchEngineTools
{
    public class IndexCore
    {

        private string indexName;
        private IWordNormalizer normalizer;
        private IDocumentStorage storage;
        Dictionary<int, PositionDict> index;
        ConcurrentDictionary<int, int> idf;
        private Dictionary<string, int> wordToInt;
        private Dictionary<int, string> intToWord;

        public IndexCore() : this(Guid.NewGuid().ToString("N"))
        { }

        public IndexCore(string name)
        {
            indexName = name;
            normalizer = new WordCaseNormalizer();
            index = new Dictionary<int, PositionDict>();
            storage = new MongoStorage();
            idf = new ConcurrentDictionary<int, int>();
            wordToInt = new Dictionary<string, int>();
            intToWord = new Dictionary<int, string>();
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
                InsertInIndex(doc, mPos + 10, (x) => x.description);
            }
        }

        private int InsertInIndex(Document doc, int posStart, Func<Document, string> field)
        {
            int wordPos = posStart;
            foreach (var word in ParseHelper.FindAllWords(field(doc)))
            {
                string nword = normalizer.NormalizeWord(word);
                PositionDict tmp;

                if (wordToInt.ContainsKey(nword) && index.TryGetValue(wordToInt[nword], out tmp))
                {
                    int currentid = wordToInt[nword];
                    idf[currentid] += 1;
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
                    
                    idf.AddOrUpdate(currentid, 1, (x, y) => y + 1);
                    index.Add(currentid,
                        new PositionDict
                        {
                            { doc.intId, new Set {wordPos} }
                        });
                }
                wordPos++;
            }
            return wordPos;
        }


        private void IntWrite(BinaryWriter bw, int i)
        {
            bw.WriteCompressedInt(i);
        }

        public void Serialize(string filePath)
        {
            using (var bw = new BinaryWriter(new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)))
            {
                bw.Write(indexName);
                bw.Write(index.Count);
                foreach (var t in index)
                {
                    bw.Write(t.Key);
                    IntWrite(bw, t.Value.Count);
                    List<int> docIds = t.Value.Keys.ToList();
                    bw.Write(docIds[0]);
                    List<int> coord1 = t.Value[docIds[0]].ToList();
                    IntWrite(bw, coord1.Count);
                    bw.Write(coord1[0]);
                    for (int k = 1; k < coord1.Count; ++k)
                        IntWrite(bw, coord1[k] - coord1[k - 1]);
                    for (int i = 1; i < docIds.Count; ++i)
                    {
                        IntWrite(bw, docIds[i] - docIds[i - 1]);
                        IntWrite(bw, t.Value[docIds[i]].Count);
                        List<int> coord = t.Value[docIds[i]].ToList();
                        bw.Write(coord[0]);
                        for (int k = 1; k < coord.Count; ++k)
                            IntWrite(bw, coord[k] - coord[k - 1]);
                    }
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
                var count = br.ReadInt32();
                for (var i = 0; i < count; ++i)
                {
                    var word = br.ReadInt32();
                    int scount = br.ReadCompressedInt();
                    int doc0 = br.ReadInt32();
                    int coord0Count = br.ReadCompressedInt();
                    int coord0 = br.ReadInt32();
                    List<int> coord0List = new List<int> { coord0 };
                    for (int k = 1; k < coord0Count; k++)
                    {
                        coord0List.Add(coord0List[k - 1] + br.ReadCompressedInt());
                    }
                    var tmp = new PositionDict { { doc0, new Set(coord0List) } };
                    for (int k = 1; k < scount; ++k)
                    {
                        int docId = br.ReadCompressedInt() + doc0;
                        doc0 = docId;
                        int coordCount = br.ReadCompressedInt();
                        int fCoord = br.ReadInt32();
                        List<int> coordList = new List<int> { fCoord };
                        for (int l = 1; l < coordCount; l++)
                        {
                            coordList.Add(coordList[l - 1] + br.ReadCompressedInt());
                        }
                        tmp.Add(docId, new Set(coordList));
                    }
                    ind.index.Add(word, tmp);
                }
            }
            //TODO: десериализовать idf wordToInt intToWord
            return ind;
        }
    }
}
