using SearchEngineTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Services.Discovery;

namespace IndexWebApi
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ISearchService
    {
        [WebInvoke(Method = "GET",
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "/Find?query={query}")]
        [OperationContract]
        IndDocument[] Find(string query);

        [WebInvoke(Method = "POST",
                    ResponseFormat = WebMessageFormat.Json,
                    RequestFormat = WebMessageFormat.Json,
                    UriTemplate = "/Add")]
        [OperationContract]
        int AddDocuments(string value);
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class IndDocument
    {
        [DataMember]
        public string Title
        {
            get; set;
        }

        [DataMember]
        public string Body
        {
            get; set;
        }

        [DataMember]
        public int Rank
        {
            get; set;
        }

        [DataMember]
        public string Meta
        {
            get; set;
        }

        [DataMember]
        public string Link
        {
            get; set;
        }

        [DataMember]
        public string Magnet
        {
            get; set;
        }

        //public static implicit operator Document(IndDocument doc)
        //{
        //    return new Document
        //    {
        //        body = doc.Body,
        //        link = doc.Link,
        //        magnet = doc.Magnet,
        //        meta = doc.Meta,
        //        rank = doc.Rank,
        //        title = doc.Title
        //    };
        //}

        //public static implicit operator IndDocument(Document doc)
        //{
        //    return new IndDocument
        //    {
        //        Body = doc.body,
        //        Link = doc.link,
        //        Magnet = doc.magnet,
        //        Meta = doc.meta,
        //        Rank = doc.rank,
        //        Title = doc.title
        //    };
        //}
    }
}
