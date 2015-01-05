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
    public class NgoiSaoScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='detail']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='detail']");

                var dateValue = (from divs in mainContent.Descendants()
                                 where divs.Name == "div" && divs.Attributes["class"] != null && divs.Attributes["class"].Value.Contains("ct-cal")
                                 select divs).First().InnerText.Trim();

                return DateTime.ParseExact(dateValue, "dd/MM/yyyy HH:mm", null);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='detail-content']");

                htmlBody = mainContent.InnerHtml;
                
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='detail-content']");

                image = mainContent.Descendants("img").First().Attributes["src"].Value;

                if (!image.Contains("http://"))
                    image = host + image;
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
                shampoo = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//meta[@name='description']").Attributes["content"].Value.Trim());
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='detail']");

                title = HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
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
                description = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//meta[@name='description']").Attributes["content"].Value.Trim());
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
                var allArticleTags = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//meta[@name='news_keywords']").Attributes["content"].Value.Trim());

                //if (mainContent != null)
                //{
                //    var tagLocation = mainContent.SelectSingleNode("//div[@class='post-tags']");

                //    if (tagLocation != null)
                //    {
                //        var tagAnchors = tagLocation.Descendants("a").ToList();

                //        if (tagAnchors.Count > 0)
                //        {
                //            foreach (var anchor in tagAnchors)
                //            {
                //                tags.Add(anchor.InnerText);
                //            }
                //        }
                //    }
                //}

                string[] allTags = allArticleTags.Split(',');

                foreach (var tag in allTags)
                {
                    tags.Add(tag.Trim());
                }
            }
            catch (Exception)
            {

            }

            return tags;
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            var body = GetHtmlBody(document, type, "");

            string plainBody = HtmlSanitizer.StripHtml(body);
            plainBody = Regex.Replace(plainBody, @"(\r\n){2,}", "\r\n\r\n"); //remove multiple blank lines
            plainBody = Regex.Replace(plainBody, @"(\n){2,}", "\n");

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
