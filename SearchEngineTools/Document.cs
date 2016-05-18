using Newtonsoft.Json.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace SearchEngineTools
{
    public class Document
    {
        [BsonId]
        public int intId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int pages { get; set; }
        public int rank { get; set; }
        public string link { get; set; }
        public string magnet { get; set; }

        public static implicit operator JObject(Document d)
        {
            return new JObject
            {
                { nameof(intId), d.intId },
                { nameof(id), d.id },
                { nameof(title), d.title },
                { nameof(description), d.description },
                { nameof(pages), d.pages },
                { nameof(rank), d.rank },
                { nameof(link), d.link },
                { nameof(magnet), d.magnet }
            }; 
        }

        public static implicit operator Document(JObject d)
        {
            var res = new Document();
            res.intId = -1;
            res.id = d[nameof(id)].Value<int>();
            res.title = d[nameof(title)]?.Value<string>();
            res.description = d[nameof(description)]?.Value<string>();
            res.pages = d[nameof(pages)].Value<int>();
            res.rank = -1;
            res.link = d[nameof(link)]?.Value<string>();
            res.magnet = d[nameof(magnet)]?.Value<string>();
            return res;
        }
    }
}
