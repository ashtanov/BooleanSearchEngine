using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SearchEngineTools
{
    [DataContract]
    public class Document
    {
        [BsonId]
        public int intId { get; set; } //внутренний id - ключ документа в монге
        [DataMember]
        public int id { get; set; } // id на рутрекере
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public int pages { get; set; }
        [DataMember]
        public string link { get; set; }
        [DataMember]
        public string magnet { get; set; }
        [DataMember]
        public int len { get; set; }
        [DataMember]
        public int tlen { get; set; }
        [DataMember]
        public string qual { get; set; }
        [DataMember]
        public string lang { get; set; }
        [DataMember]
        public string keygen { get; set; }
        [DataMember]
        [BsonIgnore]
        [BsonDefaultValue(-1)]
        public int rank { get; set; }

        public override string ToString()
        {
            return $"{intId}: {title}";
        }

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
                { nameof(tlen), d.tlen },
                { nameof(qual), d.qual },
                { nameof(lang), d.lang },
                { nameof(keygen), d.keygen },
                { nameof(magnet), d.magnet }
            };
        }

        public static implicit operator Document(JObject d)
        {
            return new Document
            {
                intId = -1,
                rank = -1,
                id = d[nameof(id)].Value<int>(),
                title = d[nameof(title)]?.Value<string>(),
                description = d[nameof(description)]?.Value<string>(),
                pages = d[nameof(pages)].Value<int>(),
                link = d[nameof(link)]?.Value<string>(),
                qual = d[nameof(qual)]?.Value<string>(),
                len = d[nameof(len)]?.Value<int>() ?? 0,
                tlen = d[nameof(tlen)]?.Value<int>() ?? 0,
                lang = d[nameof(lang)]?.Value<string>(),
                keygen = d[nameof(keygen)]?.Value<string>(),
                magnet = d[nameof(magnet)]?.Value<string>()
            };
        }
    }

    [DataContract]
    public class Response
    {
        [DataMember(Order = 0)]
        public Document[] documents { get; set; }

        [DataMember(Order = 1)]
        public WordDoc[] query { get; set; }

        [DataMember(Order = 2)]
        public int errorCode { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public string errorMessage { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public TimeSpan elapsedTime { get; set; }


    }
    [DataContract]
    public class WordDoc
    {
        [DataMember(Order = 0)]
        public string word { get; set; }
        [DataMember(Order = 1)]
        public DocPos[] docs { get; set; }

    }

    [DataContract]
    public class DocPos
    {
        [DataMember]
        public int docId { get; set; }
        [DataMember]
        public int[] pos { get; set; }
    }
}
