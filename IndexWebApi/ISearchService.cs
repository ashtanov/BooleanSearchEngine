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
                    UriTemplate = "/Find?query={query}&debug={isDebug}")]
        [OperationContract]
        Response Find(string query, string isDebug); //http://192.168.199.10/IWA/SearchService.svc/Find?query=%D0%B4%D0%B6%D0%B5%D0%B9%D0%BC%D1%81%20%D0%B1%D0%BE%D0%BD%D0%B4&debug=true

        [WebInvoke(Method = "GET",
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "/Status")]
        [OperationContract]
        Status Status();

        [WebInvoke(Method = "POST",
                    ResponseFormat = WebMessageFormat.Json,
                    RequestFormat = WebMessageFormat.Json,
                    UriTemplate = "/Add")]
        [OperationContract]
        int AddDocuments(string value);
    }

    [DataContract]
    public class Status
    {
        [DataMember]
        public string status { get; set; }
    }
}
