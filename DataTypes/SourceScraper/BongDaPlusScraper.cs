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
    public class BongDaPlusScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                title = document.DocumentNode.SelectSingleNode("//h3[@class='cap']").InnerText;
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
                string value = document.DocumentNode.SelectSingleNode("//div[@class='news_date rgt']").InnerText;

                string[] content = value.Split(',');

                string date = content[1].Replace(" ngày ", "").Replace(" - ", " ").Replace('-', '/').Trim();

                return DateTime.ParseExact(date, "dd/MM/yyyy hh:mm:ss", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        public string GetShampoo(HtmlDocument document, ArticleType type)
        {
            var shampoo = "not-found-shampoo";
            try
            {
                shampoo = document.DocumentNode.SelectSingleNode("//p[@class='summary']").InnerText;
            }
            catch (Exception)
            {
                
            }
            return shampoo;
        }

        public string GetHtmlBody(HtmlDocument document, ArticleType type, string url)
        {
            var htmlBody = "";
            try
            {
                var rawBody = document.DocumentNode.SelectSingleNode("//div[@class='contentbox']");

                List<string> removeParts = new List<string>();

                var articleImageLocation = rawBody.SelectSingleNode("//div[@class='thumbnailsbox']");

                var articleImage = articleImageLocation.Descendants("img").First().Attributes["src"].Value;

                removeParts.Add(articleImageLocation.OuterHtml);

                var relevant_news = rawBody.SelectSingleNode("//ul[@class='cat_ref lst']");

                if (relevant_news != null)
                    removeParts.Add(relevant_news.OuterHtml);

                htmlBody = rawBody.InnerHtml;

                foreach (var part in removeParts)
                {
                    htmlBody = htmlBody.Replace(part, "");
                }

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
                var rawBody = document.DocumentNode.SelectSingleNode("//div[@class='contentbox']");

                var articleImageLocation = rawBody.SelectSingleNode("//div[@class='thumbnailsbox']");

                image = articleImageLocation.Descendants("img").First().Attributes["src"].Value;
            }
            catch (Exception)
            {
                
            }

            return image;
        }

        public string GetSEOTitle(HtmlDocument document, ArticleType type)
        {
            string seoTitle = "not-found-seo-title";
            try
            {
                seoTitle = document.DocumentNode.SelectSingleNode("//h3[@class='cap']").InnerText;
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
                description = document.DocumentNode.SelectSingleNode("//meta[@property='og:description']").Attributes["content"].Value;
            }
            catch (Exception)
            {

            }

            return description;
        }

        public List<string> GetTags(HtmlDocument document, ArticleType type)
        {
            List<string> aTags = new List<string>();
            try
            {
                List<string> temp = new List<string>();

                var articleTagsLocation = document.DocumentNode.SelectSingleNode("//div[@class='news_tags']");

                if (articleTagsLocation != null)
                {
                    var tags = articleTagsLocation.Descendants("a").ToList();

                    foreach (var tag in tags)
                    {
                        temp.Add(tag.InnerText);
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
                        aTags.Add(i);
                    }
                }
            }
            catch (Exception)
            {
                
            }

            return aTags;
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            string plainBody = GetBody(document, type, "");

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
