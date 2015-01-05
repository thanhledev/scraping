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
    public class NewsComAuScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='story']");

                title = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//title").InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='story']");

                var dateValue = mainContent.Descendants("li").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "date-and-time  last").First().Attributes["title"].Value;

                dateValue = dateValue.Replace("T", " ").Replace("+11:00", "").Trim();

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='story']");

                //find body
                var rawBody = mainContent.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("story-body")).First();

                List<string> removeParts = new List<string>();

                removeParts.Add(rawBody.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("article-media")).First().OuterHtml);

                htmlBody = rawBody.InnerHtml;

                foreach (var part in removeParts)
                    htmlBody = htmlBody.Replace(part, "");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='story']");

                //find image
                var featuredImageLocation = mainContent.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("article-media"));

                if (featuredImageLocation.Count() > 0)
                {
                    image = featuredImageLocation.First().Descendants("img").First().Attributes["src"].Value;
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='story']");

                var shampooLocation = mainContent.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("story-intro"));

                if (shampooLocation.Count() > 0)
                    shampoo = HttpUtility.HtmlDecode(shampooLocation.First().InnerText.Replace("<!-- google_ad_section_start(name=story_introduction, weight=high) -->", "").Replace("<!-- google_ad_section_end(name=story_introduction) -->", "").Trim());
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='story']");

                seoTitle = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//title").InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='story']");

                var shampooLocation = mainContent.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("story-intro"));

                if (shampooLocation.Count() > 0)
                    description = HttpUtility.HtmlDecode(shampooLocation.First().InnerText.Replace("<!-- google_ad_section_start(name=story_introduction, weight=high) -->", "").Replace("<!-- google_ad_section_end(name=story_introduction) -->", "").Trim());
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
            var htmlBody = "";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='story']");

                //find body
                var rawBody = mainContent.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("story-body")).First();

                List<string> removeParts = new List<string>();

                removeParts.Add(rawBody.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("article-media")).First().OuterHtml);

                htmlBody = rawBody.InnerHtml;

                foreach (var part in removeParts)
                    htmlBody = htmlBody.Replace(part, "");
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
