using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SearchPage.Models.Helpers
{
    public class HtmlScrapSite
    {
        public HtmlScrapSite()
        {
            
           
        }

        public string GetTextFromPage(string urlPage)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(urlPage);
            doc.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style")
                .ToList()
                .ForEach(n => n.Remove());
            var inner_text = doc.DocumentNode.SelectNodes("//body//text()").Select(node => node.InnerText);
            StringBuilder output = new StringBuilder();
            foreach (string line in inner_text)
            {

                output.AppendLine(line);

            }
            return HttpUtility.HtmlDecode(output.ToString());
        }

        public List<string> GetLinksFromPage(string urlPage, string urlBase)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(urlPage);
            var linksOnPage = from lnks in doc.DocumentNode.Descendants()
                              where lnks.Name == "a" &&
                                   lnks.Attributes["href"] != null &&
                                   lnks.InnerText.Trim().Length > 0
                              select new
                              {

                                  Url = lnks.Attributes["href"].Value,
                              };

            List<Uri> Uris = new List<Uri>();

            foreach (var link in linksOnPage)
            {
                Uri baseUri = new Uri(urlBase, UriKind.Absolute);
                Uri page = new Uri(baseUri, link.Url.ToString());
                Uris.Add(page);
            }
            List<string> links = new List<string>();
            foreach (var uri in Uris)
            {
                if (!uri.AbsoluteUri.Contains("#") && links.IndexOf(uri.AbsoluteUri) < 0)
                {
                    links.Add(uri.AbsoluteUri);
                }

            }
            return links;
        }

        
    }
}