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
        public string link { get; set; }
        public string magnet { get; set; }
        public int len { get; set; }
        public string qual { get; set; }
        public string lang { get; set; }
        public string keygen { get; set; }

        public static implicit operator JObject(Document d)
        {
            return new JObject
            {
                { nameof(intId), d.intId },
                { nameof(id), d.id },
                { nameof(title), d.title },
                { nameof(description), d.description },
                { nameof(pages), d.pages },
                { nameof(link), d.link },
                { nameof(len), d.len },
                { nameof(qual), d.qual },
                { nameof(lang), d.lang },
                { nameof(keygen), d.keygen },
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
            res.link = d[nameof(link)]?.Value<string>();
            res.qual = d[nameof(qual)]?.Value<string>();
            res.len = d[nameof(len)].Value<int>();
            res.lang = d[nameof(lang)]?.Value<string>();
            res.keygen = d[nameof(keygen)]?.Value<string>();
            res.magnet = d[nameof(magnet)]?.Value<string>();
            return res;
        }
    }
}
