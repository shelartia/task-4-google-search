using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SearchPage.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchPage.Models.Helpers
{
    public class MySearcher
    {
        
       
        private string baseUrl;
        private static int indexedPagesCount=0;
        //public static List<string> listAllLinks;
        public MySearcher(string _Url)
        {
            baseUrl = _Url;
        }

        public int GetIndexedPagesCount()
        {
            return indexedPagesCount;
        }

        public void ScrapPagesFromSite(int countPages=100)
        {
            
            string url=baseUrl;
            string text;
            List<string> home_links;
            HtmlScrapSite site = new HtmlScrapSite();
            home_links = site.GetLinksFromPage(url,baseUrl);
            text = site.GetTextFromPage(url);
            var page = new Document();

            page.Add(new Field("Link", baseUrl, Field.Store.YES, Field.Index.NOT_ANALYZED));

            page.Add(new Field("Content", text, Field.Store.YES, Field.Index.ANALYZED));

            Lucene.Net.Store.Directory directory = FSDirectory.Open(Settings.Default.IndexDirectory);

            Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);



            var writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);

            writer.AddDocument(page);
            

            page.RemoveField("Link");
            page.RemoveField("Content");

            List<string> next_level_links = new List<string>();
            List<string> current_level_links = new List<string>();
            List<string> page_links = new List<string>();
            List<string> was_searched_links = new List<string>();

            was_searched_links = home_links;
            current_level_links = home_links;


            while(countPages > 0)
            {
                for (int j = 0; j < current_level_links.Count - 1; j++)
                {
                    page_links = site.GetLinksFromPage(current_level_links[j], baseUrl);

                    next_level_links.AddRange(page_links.Except(was_searched_links).ToList());
                    page_links.Clear();

                    text = site.GetTextFromPage(current_level_links[j]);

                    page.Add(new Field("Link", current_level_links[j], Field.Store.YES, Field.Index.NOT_ANALYZED));

                    page.Add(new Field("Content", text, Field.Store.YES, Field.Index.ANALYZED));

                    writer.AddDocument(page);
                    if ((countPages--) <= 0) break;

                    page.RemoveField("Link");
                    page.RemoveField("Content");
                }

                was_searched_links.AddRange(next_level_links.ToList());
                current_level_links = next_level_links.ToList();
                next_level_links.Clear();

            }
            //
            //listAllLinks = was_searched_links;
            indexedPagesCount = writer.NumDocs();
            //
            writer.Optimize();

            writer.Dispose();
            directory.Dispose();
            
        }

        public List<string> Search(string searchword)
        {
            Lucene.Net.Store.Directory directory = FSDirectory.Open(Settings.Default.IndexDirectory);

            Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

            IndexReader indexReader = IndexReader.Open(directory, true);

            Searcher indexSearch = new IndexSearcher(indexReader);

            var queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Content", analyzer);

            var query = queryParser.Parse(searchword);

            List<string> infosearch = new List<string>();
            infosearch.Add("Searching for: " + query.ToString().Replace("Content:",""));

            TopDocs resultDocs = indexSearch.Search(query, indexReader.MaxDoc);

            infosearch.Add("Results Found: " + resultDocs.TotalHits);

            var hits = resultDocs.ScoreDocs;
            foreach (var hit in hits)
            {
                var documentFromSearcher = indexSearch.Doc(hit.Doc);
                infosearch.Add( documentFromSearcher.Get("Link"));
            }

            indexSearch.Dispose();
            directory.Dispose();

            return infosearch;
        }
    }
}