using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearchEngineTools
{
    public class DirectoryStorage : IDocumentStorage
    {
        ConcurrentDictionary<int, int> fileFolderDictionary = new ConcurrentDictionary<int, int>();
        readonly string root;
        int currentFolder;
        int currentFolderCount;
        object locker = new object();

        public DirectoryStorage(string root = "E:\tmp")
        {
            this.root = root;
            var d = Directory.GetDirectories(root);
            currentFolder = d.Select(x => int.Parse(Path.GetDirectoryName(x))).Max();
            currentFolderCount = Directory.GetFiles(string.Concat(root, "/", currentFolder.ToString())).Length;
        }

        public int Add(Document doc)
        {
            return AddRange(new[] { doc });
        }

        public int AddRange(IEnumerable<Document> docs)
        {
            int k = 0;
            Parallel.ForEach(docs, new ParallelOptions { MaxDegreeOfParallelism = 10 },
                (d) =>
                {
                    int target;
                    lock (locker)
                    {
                        if (currentFolderCount >= 1000)
                        {
                            currentFolderCount = 0;
                            currentFolder++;
                        }
                        target = currentFolder;
                    }
                    //File.WriteAllBytes(d);
                    // docid считать, doc2binary написать
                });
            return k;
        }

        public Document Get(int id)
        {
            throw new NotImplementedException();
        }
    }
}
