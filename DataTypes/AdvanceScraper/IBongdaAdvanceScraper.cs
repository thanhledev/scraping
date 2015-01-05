using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;

namespace DataTypes
{
    public class IBongdaAdvanceScraper : IAdvance
    {
        Random rnd = new Random();

        public string GetImageSearchPhrase(HtmlDocument document, WebProxy proxy, List<string> agents)
        {
            string imgUrl = string.Empty;

            try
            {
                var title = document.DocumentNode.SelectSingleNode("//h1[@class='tt-cttin']").InnerText;

                string[] content = title.Split(':');

                var imageName = content[0].Trim().Replace(" ", "+");

                imageName += "+" + DateTime.Now.Year.ToString();

                string query = "https://www.bing.com/images/search?q=" + imageName + "&scope=images";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                request.Proxy = proxy;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                int pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var bingImageDocument = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    bingImageDocument.Load(reader.BaseStream);
                }

                var searchResult = bingImageDocument.DocumentNode.SelectSingleNode("//div[@id='dg_c']");

                List<string> images = new List<string>();
                if (searchResult != null)
                {
                    var results = (from divs in searchResult.Descendants("div")
                                   where divs.Attributes["class"] != null && divs.Attributes["class"].Value.Contains("dg_u")
                                   select divs).ToList();

                    if (results.Count > 0)
                    {
                        results = results.Take(10).ToList();

                        foreach (var result in results)
                        {
                            string value = HttpUtility.HtmlDecode(result.Descendants("a").First().Attributes["m"].Value);

                            string[] parts = value.Split(',');

                            foreach (var part in parts)
                            {
                                if (part.Contains("imgurl"))
                                {
                                    images.Add(part.Replace("imgurl:\"", "").Replace("\"", "").Trim());
                                }
                            }
                        }
                    }

                    imgUrl = images[rnd.Next(images.Count)];
                }
            }
            catch (Exception)
            {
               
            }

            return imgUrl;
        }

        public string GetImageSearchPhrase(HtmlDocument document, List<string> agents)
        {
            string imgUrl = string.Empty;

            try
            {
                var title = document.DocumentNode.SelectSingleNode("//h1[@class='tt-cttin']").InnerText;

                string[] content = title.Split(':');

                var imageName = content[0].Trim().Replace(" ", "+");

                imageName += "+" + DateTime.Now.Year.ToString();

                string query = "https://www.bing.com/images/search?q=" + imageName + "&scope=images";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
                
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                int pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var bingImageDocument = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    bingImageDocument.Load(reader.BaseStream);
                }

                var searchResult = bingImageDocument.DocumentNode.SelectSingleNode("//div[@id='dg_c']");

                List<string> images = new List<string>();
                if (searchResult != null)
                {
                    var results = (from divs in searchResult.Descendants("div")
                                   where divs.Attributes["class"] != null && divs.Attributes["class"].Value.Contains("dg_u")
                                   select divs).ToList();

                    if (results.Count > 0)
                    {
                        results = results.Take(10).ToList();

                        foreach (var result in results)
                        {
                            string value = HttpUtility.HtmlDecode(result.Descendants("a").First().Attributes["m"].Value);

                            string[] parts = value.Split(',');

                            foreach (var part in parts)
                            {
                                if (part.Contains("imgurl"))
                                {
                                    images.Add(part.Replace("imgurl:\"", "").Replace("\"", "").Trim());
                                }
                            }
                        }
                    }

                    imgUrl = images[rnd.Next(images.Count)];
                }
            }
            catch (Exception)
            {

            }

            return imgUrl;
        }
    }
}
