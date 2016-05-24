using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace SearchEngineTools
{

    public class DocStat : IComparable<DocStat>
    {
        [BsonId]
        [BsonElement("_id")]
        public int intId { get; set; }
        [BsonElement("len")]
        public int len { get; set; }
        [BsonElement("tlen")]
        public int tlen { get; set; }
        [BsonIgnore]
        public double rank { get; set; }

        public int CompareTo(DocStat other)
            => -1 * rank.CompareTo(other.rank);
    }

    [DataContract]
    public class Document
    {
        [BsonId]
        public int intId { get; set; } //внутренний id - ключ документа в монге

        [DataMember(Order = 0)]
        public int id { get; set; } // id на рутрекере

        [DataMember(Order = 1)]
        public string title { get; set; }

        [DataMember(Order = 2)]
        public string link { get; set; }

        [DataMember(Order = 3)]
        public string magnet { get; set; }

        [DataMember(Order = 4)]
        public string description { get; set; }

        [DataMember(Order = 5)]
        public double qual { get; set; }

        [DataMember(Order = 6)]
        public double lang { get; set; }

        [DataMember(Order = 7)]
        public double keygen { get; set; }

        [DataMember(Order = 8)]
        [BsonIgnore]
        public double rank { get; set; }

        [DataMember(Order = 9)]
        [BsonIgnore]
        public double cos { get; set; }

        [DataMember(Order = 10)]
        public int pages { get; set; }

        [DataMember(Order = 11)]
        public int len { get; set; }

        [DataMember(Order = 12)]
        public int tlen { get; set; }

        [DataMember(Order = 13)]
        public string category { get; set; }

        [DataMember(Order = 14)]
        public DateTime date { get; set; }

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
                { nameof(category), d.category },
                { nameof(magnet), d.magnet },
                { nameof(date), d.date }
            };
        }

        public static implicit operator Document(JObject d)
        {
            return new Document
            {
                intId = -1,
                rank = -1,
                cos = -1,
                id = d[nameof(id)].Value<int>(),
                title = d[nameof(title)]?.Value<string>(),
                description = d[nameof(description)]?.Value<string>(),
                pages = d[nameof(pages)].Value<int>(),
                link = d[nameof(link)]?.Value<string>(),
                qual = TextFeatureMapper.QualMap(d[nameof(qual)]?.Value<string>()),
                len = d[nameof(len)]?.Value<int>() ?? 0,
                tlen = d[nameof(tlen)]?.Value<int>() ?? 0,
                lang = TextFeatureMapper.LangMap(d[nameof(lang)]?.Value<string>()),
                keygen = TextFeatureMapper.KeygenMap(d[nameof(keygen)]?.Value<string>()),
                magnet = d[nameof(magnet)]?.Value<string>(),
                category = d[nameof(category)]?.Value<string>(),
                date = TextFeatureMapper.IdToDate(d[nameof(id)].Value<int>())
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
