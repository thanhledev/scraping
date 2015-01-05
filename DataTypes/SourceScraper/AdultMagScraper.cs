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
    public class AdultMagScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container single-post-container']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container single-post-container']");

                var dateValue = (from time in mainContent.Descendants()
                                 where time.Name == "time"
                                 select time).First().InnerText;

                dateValue = dateValue.Replace("3rd", "3").Replace("1st", "1").Replace("2nd", "2").Replace("th", "").Trim();

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container single-post-container']");

                var rawHtml = mainContent.SelectSingleNode("//div[@class='entry-content']");

                htmlBody = HtmlSanitizer.SanitizeHtml(rawHtml.InnerHtml);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container single-post-container']");

                var slider = mainContent.SelectSingleNode("//div[@class='homepage-slider-wrap row']");

                var figure = mainContent.SelectSingleNode("//figure[@class='post-hero']");

                if (slider != null)
                {
                    image = slider.Descendants("a").First().Attributes["href"].Value;
                }
                if (figure != null)
                {
                    image = figure.Descendants("img").First().Attributes["src"].Value;
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container single-post-container']");

                shampoo = HttpUtility.HtmlDecode(mainContent.Descendants("p").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "deck").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container single-post-container']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container single-post-container']");

                description = HttpUtility.HtmlDecode(mainContent.Descendants("p").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "deck").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='container single-post-container']");

                var rawHtml = mainContent.SelectSingleNode("//div[@class='entry-content']");

                htmlBody = HtmlSanitizer.SanitizeHtml(rawHtml.InnerHtml);
                htmlBody = Regex.Replace(htmlBody, @"^\s*$[\r\n]*", "", RegexOptions.Multiline);
                htmlBody = HttpUtility.HtmlDecode(htmlBody);
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
