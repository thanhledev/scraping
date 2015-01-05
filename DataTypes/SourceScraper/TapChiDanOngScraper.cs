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
    public class TapChiDanOngScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='ProductDetails']");

                title = HttpUtility.HtmlDecode(mainContent.Descendants("h2").First().InnerText);
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
                var articlePublish = document.DocumentNode.SelectSingleNode("//div[@class='slider-meta']").Attributes["datetime"].Value;

                string[] preContent = articlePublish.Split(',');

                string date = preContent[1].Trim().Replace("am", "AM").Replace("pm", "PM").Trim();                        

                return DateTime.ParseExact(date, "dd/MM/yyyy h:mm tt", null);
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
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@class='ProductDescriptionContainer']");

                List<string> removeParts = new List<string>();

                removeParts.Add(rawHtmlBody.SelectSingleNode("div[@style='display: ']").OuterHtml);

                htmlBody = rawHtmlBody.InnerHtml;

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='main-slider-content']");

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
            string seoTitle = "not-found-seo-title";
            try
            {
                seoTitle = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//meta[@property='og:title']").Attributes["content"].Value.Trim());
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
                var allArticleTags = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//meta[@name='keywords']").Attributes["content"].Value.Trim());

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
            return new List<string>();
        }
    }
}
