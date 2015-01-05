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
    public class TheGuardianScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='box']");

                title = HttpUtility.HtmlDecode(mainContent.SelectSingleNode("//div[@id='article-header']").Descendants("h1").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='box']");

                var dateValue = mainContent.SelectSingleNode("//div[@id='content']").Descendants("time").First().Attributes["datetime"].Value;

                dateValue = dateValue.Replace("BST", "").Replace("EDT", "").Trim().Replace("T", " ").Trim();

                return DateTime.ParseExact(dateValue, "yyyy-MM-dd HH:mm", null);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='box']");

                //find body
                htmlBody = mainContent.SelectSingleNode("//div[@id='article-body-blocks']").InnerHtml;

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='box']");

                //find image
                var imageLocation = mainContent.SelectSingleNode("//div[@id='main-content-picture']");

                if(imageLocation != null)
                {
                    image = imageLocation.Descendants("img").First().Attributes["src"].Value;
                }
                else
                {
                    var htmlBody = mainContent.SelectSingleNode("//div[@id='article-body-blocks']");

                    if (htmlBody != null)
                    {
                        var images = htmlBody.Descendants("img").ToList();

                        if (images.Count > 0)
                            image = images.First().Attributes["src"].Value;
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='box']");

                shampoo = HttpUtility.HtmlDecode(mainContent.SelectSingleNode("//div[@id='stand-first']").InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='box']");

                seoTitle = HttpUtility.HtmlDecode(mainContent.SelectSingleNode("//div[@id='article-header']").Descendants("h1").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='box']");

                description = HttpUtility.HtmlDecode(mainContent.SelectSingleNode("//div[@id='stand-first']").InnerText);
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
                var keywordsLocation = document.DocumentNode.SelectSingleNode("//meta[@name='keywords']");

                List<string> temp = new List<string>();

                if (keywordsLocation != null)
                {
                    var value = keywordsLocation.Attributes["content"].Value;

                    string[] keywords = value.Split(',');

                    foreach (var word in keywords)
                    {
                        tags.Add(word.Trim());
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='box']");

                //find body
                htmlBody = mainContent.SelectSingleNode("//div[@id='article-body-blocks']").InnerHtml;
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
