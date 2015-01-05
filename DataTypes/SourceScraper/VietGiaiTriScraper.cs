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
    public class VietGiaiTriScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='entry clearfix']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='content-block block-left']");

                var dateValue = (from divs in mainContent.Descendants()
                                 where divs.Name == "span" && divs.Attributes["class"] != null && divs.Attributes["class"].Value.Contains("date")
                                 select divs).First().Attributes["content"].Value.Trim();                

                return DateTime.ParseExact(dateValue, "yyyy-dd-MM", null);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='entry clearfix']");

                var rawHtmlBody = mainContent.SelectSingleNode("//div[@itemprop='articleBody']");

                List<string> removeParts = new List<string>();

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@id='abd_vidinpage']").OuterHtml);

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@class='more-post relatedin']").OuterHtml);

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
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@itemprop='articleBody']");

                image = rawHtmlBody.Descendants("img").First().Attributes["src"].Value;

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='entry clearfix']");

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
            return new List<string>();
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            return new List<string>();
        }
    }
}
