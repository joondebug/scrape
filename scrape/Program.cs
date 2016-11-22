using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Data;
using HtmlAgilityPack;

namespace scrape
{
    class Program
    {

        static void Main(string[] searchTerms)
        {
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            string urlToLoad;

            int pagesToScrape = 1;
            int resultsPerPage = 200; //MAX

            DataTable scrapeData = InitializeTable();

            for (int x = 1; x <= pagesToScrape; x++)
            {
                //url to scrape
                urlToLoad = "http://slickdeals.net/forums/forumdisplay.php?f=9&page=" + x.ToString() + "&pp=" + resultsPerPage.ToString();
                HttpWebRequest request = HttpWebRequest.Create(urlToLoad) as HttpWebRequest;
                request.Method = "GET";

                /* Sart browser signature */
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                /* Sart browser signature */

                Console.WriteLine(request.RequestUri.AbsoluteUri);
                WebResponse response = request.GetResponse();

                htmlDoc.Load(response.GetResponseStream(), true);
                if (htmlDoc.DocumentNode != null)
                {
                    var articleNodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"threadslist\"]/tbody[2]/tr");

                    if (articleNodes != null && articleNodes.Any())
                    {
                        foreach (var articleNode in articleNodes)
                        {
                            var titleNode = articleNode.SelectSingleNode("td[3]/div/a");
                            //Console.WriteLine(WebUtility.HtmlDecode(titleNode.InnerText.Trim()));
                            //Console.WriteLine(WebUtility.HtmlDecode("https://slickdeals.net" + titleNode.Attributes["href"].Value));
                            //Console.WriteLine("--------------------------------------------------------------------------");

                            scrapeData.Rows.Add(WebUtility.HtmlDecode(titleNode.InnerText.Trim()), WebUtility.HtmlDecode("https://slickdeals.net" + titleNode.Attributes["href"].Value));
                        }
                    }
                }

            }

            //string[] searchTerms = new string[] { "surface", "apple", "samsung" };

            FilterData(scrapeData, searchTerms);

            Console.ReadLine(); 
        }

        public static DataTable InitializeTable()
        {

            DataTable table = new DataTable();
            table.Columns.Add("Title", typeof(string));
            table.Columns.Add("Link", typeof(string));

            return table;
        }

        public static DataTable FilterData(DataTable scrapeData, string[] searchTerms)
        {
            DataTable filteredData = new DataTable();
            DataTable tempData = new DataTable();


            for (int i = 0; i < searchTerms.Length; i++)
            {

                if(scrapeData.Select("Title LIKE '%" + searchTerms[i] + "%'").Length > 0)
                    tempData = scrapeData.Select("Title LIKE '%" + searchTerms[i] + "%'").CopyToDataTable();
                
                if(tempData != null)
                filteredData.Merge(tempData);
            }

            if (filteredData.Rows.Count > 0)
            {
                foreach (DataRow dr in filteredData.Rows)
                {
                    Console.WriteLine(dr["Title"] + "\r\n" + dr["Link"]);
                    Console.WriteLine("--------------------------------------------------------------------------");
                    Console.WriteLine("--------------------------------------------------------------------------");
                }
            }
            else
            {
                Console.WriteLine("No results found.");
            }

            

            return filteredData;
        }
    }
}
