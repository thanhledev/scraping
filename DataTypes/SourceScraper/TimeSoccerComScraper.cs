using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Text.RegularExpressions;
using DataTypes.Enums;
using System.IO;
using System.Web;
using System.Globalization;

namespace DataTypes
{
    public class TimeSoccerComScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                title = "Tổng hợp trận đấu " + HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
            }
            catch (Exception)
            {

            }
            return title;
        }

        public DateTime GetPublish(HtmlDocument document, ArticleType type)
        {
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                var dateValue = (from divs in mainContent.Descendants()
                                 where divs.Name == "time" && divs.Attributes["class"] != null && divs.Attributes["class"].Value.Contains("entry-date updated")
                                 select divs).First().Attributes["content"].Value.Trim();

                dateValue = dateValue.Replace("T", " ").Replace("+00:00", "").Trim();

                return DateTime.ParseExact(dateValue, "yyyy-MM-dd HH:mm:ss", null);
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        public string GetHtmlBody(HtmlDocument document, ArticleType type, string url)
        {
            var htmlBody = "";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                htmlBody += "<h3>Thông tin về trận đấu</h3>";

                var matchInformation = mainContent.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("span6 wpb_column column_container")).ToList();

                if (matchInformation.Count == 2)
                {
                    //get match featured image
                    var matchImage = matchInformation[0].Descendants("img").First();

                    htmlBody += "<p><img src=\"" + matchImage.Attributes["src"].Value + "\" alt=\"" + matchImage.Attributes["alt"].Value + "\" /></p>";

                    htmlBody += "<p>";

                    htmlBody += matchInformation[1].InnerHtml;

                    htmlBody += "</p>";
                }

                htmlBody += "<h3>Video tổng hợp trận đấu</h3>";

                var videoScript = mainContent.Descendants("script").Where(a => a.Attributes["src"] != null && a.Attributes["src"].Value.Contains("//cdn.playwire.com/bolt/js/embed.min.js"));

                if (videoScript != null)
                    htmlBody += videoScript.First().OuterHtml;
            }
            catch (Exception)
            {

            }
            return htmlBody;
        }

        public string GetBody(HtmlDocument document, ArticleType type, string url)
        {
            var body = GetHtmlBody(document, type, url);

            string plainBody = HtmlSanitizer.StripHtml(body);
            plainBody = Regex.Replace(plainBody, @"(\r\n){2,}", "\r\n\r\n");

            return plainBody;
        }

        public string GetImage(HtmlDocument document, ArticleType type, string url)
        {
            var host = "http://" + new Uri(url).Host;
            string image = "not-found-image";

            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                var matchInformation = mainContent.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("span6 wpb_column column_container")).ToList();

                if (matchInformation.Count == 2)
                {
                    image = matchInformation.First().Descendants("img").First().Attributes["src"].Value;

                    if (!image.Contains("http://"))
                        image = host + image;
                }


            }
            catch (Exception)
            {

            }

            return image;
        }

        public string GetShampoo(HtmlDocument document, ArticleType type)
        {
            var shampoo = "not-found-shampoo";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                shampoo = "Tổng hợp trận đấu " + HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
            }
            catch (Exception ex)
            {

            }
            return shampoo;
        }

        public string GetSEOTitle(HtmlDocument document, ArticleType type)
        {
            string title = "not-found-seo-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                title = "Tổng hợp trận đấu " + HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
            }
            catch (Exception)
            {

            }
            return title;
        }

        public string GetSEODescription(HtmlDocument document, ArticleType type)
        {
            var description = "not-found-description";

            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                description = "Tổng hợp trận đấu " + HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
            }
            catch (Exception)
            {

            }

            return description;
        }

        public List<string> GetTags(HtmlDocument document, ArticleType type)
        {
            return new List<string>();
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            return new List<string>();
        }
    }
}
