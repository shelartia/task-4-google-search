using HtmlAgilityPack;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SearchPage.Models.Helpers;
using SearchPage.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace SearchPage.Controllers
{
    public class SearchController : Controller
    {
        
        private List<string> infosearch;
        private MySearcher ms;

        public SearchController()
        {
            infosearch = new List<string>();
            
        }
        //
        // GET: /Search/

        public ActionResult Index(string searchString="")
        {
            ms = new MySearcher(Settings.Default.SiteToSearch);
            
            if (String.IsNullOrEmpty(searchString))
            {
                ViewBag.searchText = "";
                ViewBag.foundResultsCount = "";
                ViewBag.infosearch = infosearch;
                ViewBag.searchTime = "";
            }
            else
            {
                var watch = Stopwatch.StartNew();
                infosearch = ms.Search(searchString);
                watch.Stop();
                ViewBag.searchTime = "Search time: "+watch.ElapsedMilliseconds.ToString()+" ms";
                
                ViewBag.searchText = infosearch[0];
                ViewBag.foundResultsCount = infosearch[1];
                ViewBag.infosearch = infosearch.GetRange(2,infosearch.Count-2);
            }
            
            return View();
        }

        public ActionResult SearchServiceInfo()
        {
            ms = new MySearcher(Settings.Default.SiteToSearch);
            //ViewBag.allLinks = MySearcher.listAllLinks;
            ViewBag.totalPages = "Total indexed pages count: " + ms.GetIndexedPagesCount().ToString();

            return View();
        }

    }
}
