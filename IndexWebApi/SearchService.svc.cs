using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SearchEngineTools;
using System.Configuration;

namespace IndexWebApi
{
    public class SearchService : ISearchService
    {
        private static IndexCore index;
        private static Task start;
        private static Stopwatch startTime;

        static SearchService()
        {
            startTime = new Stopwatch();
            startTime.Start();
            start = new Task(() =>
            {
                index = IndexCore.Deserialize(ConfigurationManager.AppSettings["indexSource"]);
            });
            start.Start();
        }
        public int AddDocuments(string value)
        {
            try
            {
                JObject.Parse(value);
                return 1;
            }
            catch
            {
                return -1;
            }
        }

        public Response Find(string query, string debug, string dist)
        {
            try
            {
                var stat = Status();
                if (stat.errorCode == 0)
                {
                    int distInt;
                    if (!int.TryParse(dist, out distInt))
                        distInt = 10;
                    var timer = Stopwatch.StartNew();
                    var res = index.SearchQuery(query, distInt);
                    res.errorCode = 0;
                    var elapsed = timer.Elapsed;
                    bool d;
                    if (bool.TryParse(debug, out d))
                        if (d)
                        {
                            res.elapsedTime = elapsed;
                            return res;
                        }
                    return res;
                }
                return new Response
                {
                    errorCode = stat.errorCode,
                    errorMessage = "Index not ready, more info at /status"
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    errorCode = -1,
                    errorMessage = ex.ToString()
                };
            }
        }

        public Status Status()
        {
            string stat;
            int errCode;
            if (start.IsFaulted)
            {
                stat = "Failed:\n" + start.Exception;
                errCode = -1;
            }
            else if (start.IsCompleted)
            {
                stat = "Ready";
                errCode = 0;
            }
            else
            {
                stat = "Loading...";
                errCode = 1;
            }
            return new Status
            {
                status = stat,
                runningTime = startTime.Elapsed.ToString(),
                errorCode = errCode
            };
        }
    }
}
