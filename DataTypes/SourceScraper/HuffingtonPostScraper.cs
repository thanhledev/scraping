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
    public class HuffingtonPostScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//article[@class='entry']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//article[@class='entry']");

                var dateValue = mainContent.Descendants().Where(a => a.Name == "time").First().Attributes["datetime"].Value;

                dateValue = dateValue.Replace("T", " ").Replace("-04:00", "").Trim();

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
                var mainContent = document.DocumentNode.SelectSingleNode("//article[@class='entry']");

                var rawBody = mainContent.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value == "mainentrycontent").First();

                List<string> removeParts = new List<string>();

                //remove ads
                var adsLocation1 = rawBody.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "float_left");

                if (adsLocation1.Count() > 0)
                    removeParts.Add(adsLocation1.First().OuterHtml);

                var adsLocation2 = rawBody.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value == "modulous_mid_article");

                if (adsLocation2.Count() > 0)
                    removeParts.Add(adsLocation2.First().OuterHtml);

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
                var mainContent = document.DocumentNode.SelectSingleNode("//article[@class='entry']");                

                var featuredImageLocation = mainContent.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "main-visual group embedded-image");

                if (featuredImageLocation != null)
                {
                    var images = featuredImageLocation.First().Descendants("img").ToList();

                    if (images.Count > 0)
                        image = images.First().Attributes["data-img-path"].Value;
                    else
                    {
                        var rawBody = mainContent.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value == "mainentrycontent");

                        if (rawBody != null)
                        {
                            var images_1 = rawBody.First().Descendants("img").ToList();

                            if (images_1.Count > 0)
                            {
                                image = images_1.First().Attributes["src"].Value;
                            }
                        }
                    }
                }
                else
                {
                    var rawBody = mainContent.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value == "mainentrycontent");

                    if (rawBody != null)
                    {
                        var images = rawBody.First().Descendants("img").ToList();

                        if (images.Count > 0)
                        {
                            image = images.First().Attributes["src"].Value;
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
                var mainContent = document.DocumentNode.SelectSingleNode("//article[@class='entry']");

                shampoo = HttpUtility.HtmlDecode(mainContent.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value == "mainentrycontent").First().Descendants("p").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//article[@class='entry']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//article[@class='entry']");

                description = HttpUtility.HtmlDecode(mainContent.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value == "mainentrycontent").First().Descendants("p").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//article[@class='entry']");

                var rawBody = mainContent.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value == "mainentrycontent").First();

                List<string> removeParts = new List<string>();

                //remove ads
                var adsLocation1 = rawBody.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "float_left");

                if (adsLocation1.Count() > 0)
                    removeParts.Add(adsLocation1.First().OuterHtml);

                var adsLocation2 = rawBody.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value == "modulous_mid_article");

                if (adsLocation2.Count() > 0)
                    removeParts.Add(adsLocation2.First().OuterHtml);

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
