using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Set = System.Collections.Generic.List<int>;
using PositionDict = System.Collections.Generic.SortedDictionary<int, System.Collections.Generic.SortedSet<int>>;

namespace SearchEngineTools
{
    public class IndexCore
    { 

        private string indexName;
        private IWordNormalizer normalizer;
        private IDocumentStorage storage;
        Dictionary<string, Set> index;

        public IndexCore() : this(Guid.NewGuid().ToString("N"))
        { }

        public IndexCore(string name)
        {
            indexName = name;
            normalizer = new WordCaseNormalizer();
            index = new Dictionary<string, Set>();
        }

        public void Add(Document d)
        {
            AddRange(new[] { d });
        }

        public void AddRange(IEnumerable<Document> docs)
        {
            storage.AddRange(docs);
        }

        private void IntWrite(BinaryWriter bw, int i)
        {
            bw.WriteCompressedInt(i);
        }

    //    public void Serialize(string filePath)
    //    {
    //        using (var bw = new BinaryWriter(new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)))
    //        {
    //            bw.Write(indexName);
    //            bw.Write(index.Count);
    //            foreach (var t in index)
    //            {
    //                bw.Write(t.Key);
    //                IntWrite(bw, t.Value.Count);
    //                List<int> docIds = t.Value.Keys.ToList();
    //                bw.Write(docIds[0]);
    //                List<int> coord1 = t.Value[docIds[0]].ToList();
    //                IntWrite(bw, coord1.Count);
    //                bw.Write(coord1[0]);
    //                for (int k = 1; k < coord1.Count; ++k)
    //                    IntWrite(bw, coord1[k] - coord1[k - 1]);
    //                for (int i = 1; i < docIds.Count; ++i)
    //                {
    //                    IntWrite(bw, docIds[i] - docIds[i - 1]);
    //                    IntWrite(bw, t.Value[docIds[i]].Count);
    //                    List<int> coord = t.Value[docIds[i]].ToList();
    //                    bw.Write(coord[0]);
    //                    for (int k = 1; k < coord.Count; ++k)
    //                        IntWrite(bw, coord[k] - coord[k - 1]);
    //                }
    //            }
    //        }
    //    }

    //    public static IndexCore Deserialize(string filePath)
    //    {
    //        IndexCore ind;
    //        using (var br = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
    //        {
    //            ind = new IndexCore(br.ReadString());
    //            var count = br.ReadInt32();
    //            for (var i = 0; i < count; ++i)
    //            {
    //                var word = br.ReadString();
    //                int scount = br.ReadInt32();
    //                var tmp = new PositionDict();
    //                for (int k = 0; k < scount; ++k)
    //                {
    //                    int docId = br.ReadInt32();
    //                    int pCount = br.ReadInt32();
    //                    Set s = new Set();
    //                    for (int p = 0; p < pCount; ++p)
    //                    {
    //                        s.Add(br.ReadInt32());
    //                    }
    //                    tmp.Add(docId, s);
    //                }
    //                ind.index.Add(word, tmp);
    //            }
    //        }
    //        return ind;
    //    }
    }
}
