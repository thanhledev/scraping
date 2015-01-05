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
    public class CelebromanceComScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                var dateValue = mainContent.Descendants("time").First().Attributes["datetime"].Value;

                dateValue = dateValue.Replace("T", " ").Trim().Replace("+00:00", "").Replace('-', '/').Trim();

                return DateTime.ParseExact(dateValue, "yyyy/MM/dd HH:mm:ss", null);
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

                var rawHtml = mainContent.InnerHtml;

                List<string> removeParts = new List<string>();

                removeParts.Add(mainContent.Descendants("header").First().OuterHtml);

                var ads = mainContent.Descendants("div").Where(a => a.Attributes["style"] != null && a.Attributes["style"].Value.Contains("float:right;margin:10px 0 10px 10px;")).First().OuterHtml;                

                if (ads != null)
                    removeParts.Add(ads);

                var attachment = mainContent.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value.Contains("attachment_"));

                if (attachment != null)
                {
                    removeParts.Add(attachment.First().OuterHtml);
                }

                foreach (var part in removeParts)
                    rawHtml = rawHtml.Replace(part, "");

                htmlBody = HtmlSanitizer.SanitizeHtml(rawHtml);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                image = mainContent.Descendants("img").First().Attributes["src"].Value;
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

                shampoo = HttpUtility.HtmlDecode(mainContent.Descendants("p").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

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
                var tagList = (from metas in document.DocumentNode.Descendants("meta")
                               where metas.Attributes["property"] != null && metas.Attributes["property"].Value.Contains("article:tag")
                               select metas).ToList();

                List<string> temp = new List<string>();

                if (tagList.Count > 0)
                {
                    foreach (var meta in tagList)
                    {
                        temp.Add(meta.Attributes["content"].Value);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//article");

                var rawHtml = mainContent.InnerHtml;

                List<string> removeParts = new List<string>();

                removeParts.Add(mainContent.Descendants("header").First().OuterHtml);

                var ads = mainContent.Descendants("div").Where(a => a.Attributes["style"] != null && a.Attributes["style"].Value.Contains("float:right;margin:10px 0 10px 10px;")).First().OuterHtml;

                if (ads != null)
                    removeParts.Add(ads);

                foreach (var part in removeParts)
                    rawHtml = rawHtml.Replace(part, "");

                htmlBody = HtmlSanitizer.SanitizeHtml(rawHtml);
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
