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
                index = IndexCore.Deserialize(@"E:\indexFull.idx");
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

        public Response Find(string query, string debug)
        {
            try
            {
                var timer = Stopwatch.StartNew();
                var res = index.SearchQuery(query);
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
            if (start.IsCompleted)
                stat = "Ready";
            else if (start.IsFaulted)
                stat = "Failed:\n" + start.Exception;
            else
                stat = "Loading...";
            return new Status
            {
                status = stat,
                runningTime = startTime.Elapsed.ToString()
            };
        }
    }
}
