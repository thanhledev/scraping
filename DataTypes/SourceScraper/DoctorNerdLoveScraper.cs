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
    public class DoctorNerdLoveScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='content']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='content']");

                var dateValue = (from span in mainContent.Descendants("span")
                                 where span.Attributes["class"] != null && span.Attributes["class"].Value.Contains("date published time")
                                 select span).First().Attributes["title"].Value;

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='content']");

                var rawBody = mainContent.SelectSingleNode("//div[@class='entry-content']");

                List<string> removeParts = new List<string>();

                var relatedPosts = mainContent.SelectSingleNode("//div[@id='wp_rp_first']");

                if (relatedPosts != null)
                    removeParts.Add(relatedPosts.OuterHtml);

                var footnotes = mainContent.SelectSingleNode("//ol[@class='footnotes']");

                if (footnotes != null)
                    removeParts.Add(footnotes.OuterHtml);

                htmlBody = rawBody.InnerHtml;

                foreach (var part in removeParts)
                {
                    htmlBody = htmlBody.Replace(part, "");
                }

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='content']");

                var rawBody = mainContent.SelectSingleNode("//div[@class='entry-content']");

                var images = rawBody.Descendants("img").ToList();

                if (images.Count > 0)
                {
                    foreach (var img in images)
                    {
                        if (img.Attributes["class"] != null && img.Attributes["class"].Value.Contains("wp-image"))
                        {
                            image = images.First().Attributes["src"].Value;
                            break;
                        }
                    }
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='content']");

                var rawBody = mainContent.SelectSingleNode("//div[@class='entry-content']");

                shampoo = rawBody.Descendants("p").First().InnerText;
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='content']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='content']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='content']");

                List<string> temp = new List<string>();

                if (mainContent != null)
                {
                    var tagLocation = mainContent.SelectSingleNode("//div[@class='post-meta']");

                    if (tagLocation != null)
                    {
                        var tagList = (from span in tagLocation.Descendants("span")
                                       where span.Attributes["class"] != null && span.Attributes["class"].Value.Contains("tags")
                                       select span).First();

                        if (tagList != null)
                        {
                            var tagAnchors = tagList.Descendants("a").ToList();

                            if (tagAnchors.Count > 0)
                            {
                                foreach (var anchor in tagAnchors)
                                {
                                    temp.Add(anchor.InnerText);
                                }
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='content']");

                var rawBody = mainContent.SelectSingleNode("//div[@class='entry-content']");

                List<string> removeParts = new List<string>();

                var relatedPosts = mainContent.SelectSingleNode("//div[@id='wp_rp_first']");

                if (relatedPosts != null)
                    removeParts.Add(relatedPosts.OuterHtml);

                var footnotes = mainContent.SelectSingleNode("//ol[@class='footnotes']");

                if (footnotes != null)
                    removeParts.Add(footnotes.OuterHtml);

                htmlBody = rawBody.InnerHtml;

                foreach (var part in removeParts)
                {
                    htmlBody = htmlBody.Replace(part, "");
                }
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
