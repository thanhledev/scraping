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
    public class EvanmarckatzScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                var articlePost = (from artl in mainContent.Descendants()
                                   where artl.Name == "article" && artl.Attributes["class"] != null && artl.Attributes["class"].Value.Contains("post type-post")
                                   select artl).First();

                title = HttpUtility.HtmlDecode(articlePost.Descendants("h1").First().InnerText);
            }
            catch (Exception)
            {

            }
            return title;
        }

        public DateTime GetPublish(HtmlDocument document, ArticleType type)
        {
            return DateTime.Now;
        }

        public string GetHtmlBody(HtmlDocument document, ArticleType type, string url)
        {
            var htmlBody = "";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");
                
                var rawBody = mainContent.SelectSingleNode("//div[@class='content']");

                List<string> removeParts = new List<string>();

                removeParts.Add(rawBody.SelectSingleNode("//div[@id='sb-container']").OuterHtml);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                var rawBody = mainContent.SelectSingleNode("//div[@class='content']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='primary']");

                var rawBody = mainContent.SelectSingleNode("//div[@class='content']");

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

                var articlePost = (from artl in mainContent.Descendants()
                                   where artl.Name == "article" && artl.Attributes["class"] != null && artl.Attributes["class"].Value.Contains("post type-post")
                                   select artl).First();

                seoTitle = HttpUtility.HtmlDecode(articlePost.Descendants("h1").First().InnerText);
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

                var articlePost = (from artl in mainContent.Descendants()
                                   where artl.Name == "article" && artl.Attributes["class"] != null && artl.Attributes["class"].Value.Contains("post type-post")
                                   select artl).First();

                description = HttpUtility.HtmlDecode(articlePost.Descendants("h1").First().InnerText);
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

                if (tagList.Count > 0)
                {
                    foreach (var meta in tagList)
                    {
                        tags.Add(meta.Attributes["content"].Value);
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

                var rawBody = mainContent.SelectSingleNode("//div[@class='content']");

                List<string> removeParts = new List<string>();

                removeParts.Add(rawBody.SelectSingleNode("//div[@id='sb-container']").OuterHtml);
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
