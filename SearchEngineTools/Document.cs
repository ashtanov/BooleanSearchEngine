using Newtonsoft.Json.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace SearchEngineTools
{
    public class Document
    {
        [BsonId]
        public int Id { get; set; }
        public int extId { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string meta { get; set; }
        public int rank { get; set; }
        public string link { get; set; }
        public string magnet { get; set; }

        public static implicit operator JObject(Document d)
        {
            return new JObject
            {
                { nameof(Id), d.Id },
                { nameof(extId), d.extId },
                { nameof(title), d.title },
                { nameof(body), d.body },
                { nameof(meta), d.meta },
                { nameof(rank), d.rank },
                { nameof(link), d.link },
                { nameof(magnet), d.magnet }
            }; 
        }

        public static implicit operator Document(JObject d)
        {
            var res = new Document();
            res.Id = d[nameof(Id)].Value<int>();
            res.extId = d[nameof(extId)].Value<int>();
            res.title = d[nameof(title)].Value<string>();
            res.body = d[nameof(body)].Value<string>();
            res.meta = d[nameof(meta)].Value<string>();
            res.rank = d[nameof(rank)].Value<int>();
            res.link = d[nameof(link)].Value<string>();
            res.magnet = d[nameof(magnet)].Value<string>();
            return res;
        }
    }
}
