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

namespace DataTypes
{
    public class HeavyScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                title = HttpUtility.HtmlDecode(mainContent.SelectSingleNode("//div[@class='entry-header']").Descendants("h1").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                var dateValue = mainContent.Descendants("time").First().InnerText;

                string[] content = dateValue.Split(',');

                dateValue = content[1].Trim() + ", " + content[2].Trim();

                return DateTime.ParseExact(dateValue, "MMMM d, yyyy", null);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                var rawBody = mainContent.SelectSingleNode("//div[@class='entry-content']");

                var galleryImages = (from lis in rawBody.Descendants("li")
                                     where lis.Attributes["class"] != null && lis.Attributes["class"].Value.Contains("carousel-item")
                                     select lis).ToList();

                if (galleryImages.Count > 0)
                {
                    var featuredImageValue = galleryImages.First().Descendants("img").First().Attributes["src"].Value;
                    string[] content = featuredImageValue.Split('?');                    

                    foreach (var img in galleryImages)
                    {
                        var imageValue = img.Descendants("img").First().Attributes["src"].Value;
                        content = imageValue.Split('?');

                        htmlBody += "<p><img src=\"" + content[0] + "\" /></p>";
                    }
                }
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                var rawBody = mainContent.SelectSingleNode("//div[@class='entry-content']");

                var galleryImages = (from lis in rawBody.Descendants("li")
                                     where lis.Attributes["class"] != null && lis.Attributes["class"].Value.Contains("carousel-item")
                                     select lis).ToList();

                if (galleryImages.Count > 0)
                {
                    var featuredImageValue = galleryImages.First().Descendants("img").First().Attributes["src"].Value;
                    string[] content = featuredImageValue.Split('?');
                    image = content[0];                    
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                seoTitle = HttpUtility.HtmlDecode(mainContent.SelectSingleNode("//div[@class='entry-header']").Descendants("h1").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                description = HttpUtility.HtmlDecode(mainContent.SelectSingleNode("//div[@class='entry-header']").Descendants("h1").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                if (mainContent != null)
                {
                    var tagLocation = mainContent.SelectSingleNode("//ul[@class='tags']");

                    if (tagLocation != null)
                    {
                        var tagAnchors = tagLocation.Descendants("a").ToList();

                        if (tagAnchors.Count > 0)
                        {
                            foreach (var anchor in tagAnchors)
                            {
                                tags.Add(anchor.InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                var rawBody = mainContent.SelectSingleNode("//div[@class='entry-content']");

                htmlBody = HtmlSanitizer.SanitizeHtml(rawBody.InnerHtml);
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
