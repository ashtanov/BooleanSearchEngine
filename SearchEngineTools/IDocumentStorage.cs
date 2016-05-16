using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineTools
{
    public interface IDocumentStorage
    {
        int Add(Document doc);
        int AddRange(IEnumerable<Document> docs);
        Document Get(int id);
    }
}
