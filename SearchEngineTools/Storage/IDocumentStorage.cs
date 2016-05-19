using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineTools
{
    //return document id
    public interface IDocumentStorage
    {
        int Add(Document doc);
        int AddRange(IEnumerable<Document> docs);
        Task<int> AddAsync(Document doc);
        Task<int> AddRangeAsync(IEnumerable<Document> docs);
        Document Get(int id);
        IList<Document> GetRange(IList<int> id);

    }
}
