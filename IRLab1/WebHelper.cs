using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IRLab1
{
    public static class WebHelper
    {
        public static string GetLibCompositionText(Uri url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            HtmlDocument doc = new HtmlDocument();
            doc.Load(req.GetResponse().GetResponseStream());
            return doc.DocumentNode.InnerText;
        } 
    }
}
