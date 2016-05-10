using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoExtractor
{
    public interface IParser
    {
        JObject ParseDocument(string doc);
    }

    public class RutrackerParser : IParser
    {
        public virtual JObject ParseDocument(string doc)
        {
            var htmlDoc = GetHtmlDoc(doc);
            return ParseBaseInfo(htmlDoc);
        }

        protected HtmlAgilityPack.HtmlDocument GetHtmlDoc(string html)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);
            return htmlDoc;
        }

        protected JObject ParseBaseInfo(HtmlAgilityPack.HtmlDocument htmlDoc)
        {
            JObject obj = new JObject();
            var titleNhtml = htmlDoc.GetElementbyId("topic-title");
            if (titleNhtml != null)
            {
                var body = htmlDoc.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "post_body").First();
                HtmlAgilityPack.HtmlNode torrentBlock;
                string magnet;
                try
                {
                    torrentBlock = body.Descendants("fieldset").First();
                    magnet = torrentBlock.SelectSingleNode("div/a").Attributes["href"].Value;
                }
                catch
                {
                    magnet = null;
                    torrentBlock = null;
                }
                int tbitl = torrentBlock?.InnerText?.Length ?? 0;
                obj.Add("title", titleNhtml.InnerText);
                obj.Add("href", titleNhtml.Attributes["href"].Value);
                obj.Add("body", body.InnerText.Substring(0, body.InnerText.Length - tbitl).Replace("&#10;", "\n"));
                obj.Add("magnet", magnet);
                return obj;
            }
            return null;
        }
    }

    public class RutrackerGamesParser : RutrackerParser
    {
        public static CounterSet<string> cnt = new CounterSet<string>();
        public override JObject ParseDocument(string doc)
        {
            var htmlDoc = GetHtmlDoc(doc);
            JObject obj = ParseBaseInfo(htmlDoc);
            if (obj != null)
            {
                string body = obj["body"].ToString();
                foreach (var m in body.Split('\n').Where(x => x.Contains(":")))
                    cnt.Add(m.Split(':')[0].Trim().ToLower());
                return obj;
            }
            else
                return null;
        }
    }
}
