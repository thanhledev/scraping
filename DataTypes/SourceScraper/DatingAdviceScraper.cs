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
    public class DatingAdviceScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container main-container']");

                title = HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container main-container']");

                var dateValues = (from div in mainContent.Descendants("td")
                                  where div.Attributes["class"] != null && div.Attributes["class"].Value.Contains("header-author")
                                  select div).First().InnerText.Trim();

                DateTime articlePublished = new DateTime();

                dateValues = dateValues.Replace("<!-- end .author-avatar -->", "").Trim();

                dateValues = Regex.Replace(dateValues, @"\t|\n|\r", "");

                string[] content = dateValues.Split(new string[] { "&bull;" }, StringSplitOptions.RemoveEmptyEntries);

                string[] dateContent = content[1].Split('/');

                string fullDateTime = dateContent[0] + "/" + dateContent[1] + "/" + "20" + dateContent[2];

                articlePublished = DateTime.ParseExact(fullDateTime, "M/dd/yyyy", CultureInfo.InvariantCulture);

                return articlePublished;
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container main-container']");

                htmlBody = mainContent.SelectSingleNode("//div[@class='single-content']").InnerHtml;

                htmlBody = HtmlSanitizer.SanitizeHtml(htmlBody);
                htmlBody = Regex.Replace(htmlBody, @"^\s*$[\r\n]*", "", RegexOptions.Multiline);
                htmlBody = HttpUtility.HtmlDecode(htmlBody);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='image-container']");

                var images = mainContent.Descendants("img").ToList();

                if (images.Count > 0)
                {
                    image = images.First().Attributes["data-lazy-src"].Value;
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container main-container']");

                var htmlBody = mainContent.SelectSingleNode("//div[@class='single-content']");

                shampoo = HttpUtility.HtmlDecode(htmlBody.Descendants("p").First().InnerText);
            }
            catch (Exception ex)
            {

            }
            return shampoo;
        }

        public string GetSEOTitle(HtmlDocument document, ArticleType type)
        {
            string seoTitle = "not-found-seo-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container main-container']");

                seoTitle = HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
            }
            catch (Exception)
            {

            }
            return seoTitle;
        }

        public string GetSEODescription(HtmlDocument document, ArticleType type)
        {
            var description = "not-found-description";

            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container main-container']");

                description = HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
            }
            catch (Exception)
            {

            }

            return description;
        }

        public List<string> GetTags(HtmlDocument document, ArticleType type)
        {
            List<string> tags = new List<string>();

            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container main-container']");

                List<string> temp = new List<string>();

                if (mainContent != null)
                {
                    var tagLocation = mainContent.SelectSingleNode("//div[@class='topics']");

                    if (tagLocation != null)
                    {
                        var tagList = tagLocation.Descendants().ToList();

                        if (tagList != null)
                        {
                            //var tagAnchors = tagList.Descendants("a").ToList();

                            //if (tagAnchors.Count > 0)
                            //{
                            //    foreach (var anchor in tagAnchors)
                            //    {
                            //        tags.Add(anchor.InnerText);
                            //    }
                            //}

                            //string plainBody = GetBody(document, type, "");

                            //foreach (var i in temp)
                            //{
                            //    int appeareance = 0;

                            //    CompareInfo sampleInfo = CultureInfo.InvariantCulture.CompareInfo;

                            //    int pos = sampleInfo.IndexOf(plainBody, i, CompareOptions.IgnoreCase);

                            //    while (pos > 0)
                            //    {
                            //        appeareance++;
                            //        pos = sampleInfo.IndexOf(plainBody, i, pos + 1, CompareOptions.IgnoreCase);
                            //    }

                            //    if (appeareance > 0)
                            //    {
                            //        tags.Add(i);
                            //    }
                            //}

                            foreach (var tag in tagList)
                            {
                                temp.Add(tag.InnerText);
                            }

                            string plainBody = GetBody(document, type, "");

                            foreach (var i in temp)
                            {
                                int appeareance = 0;

                                CompareInfo sampleInfo = CultureInfo.InvariantCulture.CompareInfo;

                                int pos = sampleInfo.IndexOf(plainBody, i, CompareOptions.IgnoreCase);

                                while (pos > 0)
                                {
                                    appeareance++;
                                    pos = sampleInfo.IndexOf(plainBody, i, pos + 1, CompareOptions.IgnoreCase);
                                }

                                if (appeareance > 0)
                                {
                                    tags.Add(i);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return tags;
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            var htmlBody = "";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container main-container']");

                htmlBody = mainContent.SelectSingleNode("//div[@class='single-content']").InnerHtml;
            }
            catch (Exception)
            {

            }

            var plainBody = HtmlSanitizer.StripHtml(htmlBody);

            List<string> sentences = new List<string>();
            Regex rx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");

            foreach (Match match in rx.Matches(plainBody))
            {
                if (match.Value.Split().Length > 5)
                    sentences.Add(match.Value);
            }

            return sentences;
        }
    }
}
