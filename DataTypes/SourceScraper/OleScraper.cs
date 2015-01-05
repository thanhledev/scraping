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
    public class OleScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                title = document.DocumentNode.SelectSingleNode("//h1[@class='item-title']").InnerText;
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
                string value = document.DocumentNode.SelectSingleNode("//div[@class='item-category']").Descendants("div").First().InnerText.Trim();

                string[] preContent = value.Trim().Split('|');

                string[] dateTimeContent = preContent[0].Split(',');

                string time = dateTimeContent[0].Trim();

                string preDate = dateTimeContent[1].Trim();

                string[] datePart = preDate.Split('/');

                string dayValue = datePart[0].Split(' ').Last();

                string monthValue = datePart[1];

                string yearValue = datePart[2];

                string final = time + " " + dayValue + "/" + monthValue + "/" + yearValue;

                return DateTime.ParseExact(final, "HH:mm dd/MM/yyyy", null);
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public string GetHtmlBody(HtmlDocument document, ArticleType type, string url)
        {
            var htmlBody = "";

            try
            {
                htmlBody = HtmlSanitizer.SanitizeHtml(document.DocumentNode.SelectSingleNode("//div[@class='item-fulltext']").InnerHtml);
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
                image = document.DocumentNode.SelectSingleNode("//img[@class='lazy']").Attributes["src"].Value;

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
                shampoo = document.DocumentNode.SelectSingleNode("//h2[@class='item-introtext']").InnerText;
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
                title = document.DocumentNode.SelectSingleNode("//h1[@class='item-title']").InnerText;
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
                description = document.DocumentNode.SelectSingleNode("//meta[@name='description']").Attributes["content"].Value;
            }
            catch (Exception)
            {

            }

            return description;
        }

        public List<string> GetTags(HtmlDocument document, ArticleType type)
        {
            List<string> articleTags = new List<string>();

            try
            {
                var articleTagsLocation = document.DocumentNode.SelectSingleNode("//div[@class='item-tags']");

                var temp = articleTagsLocation.Descendants("a").Select(a => a.InnerText).ToList();

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
                        articleTags.Add(i);
                    }
                }
            }
            catch (Exception)
            {
            
            }

            return articleTags;
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            var body = document.DocumentNode.SelectSingleNode("//div[@class='item-fulltext']").InnerHtml;
            List<string> sentences = new List<string>();
            Regex rx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");

            string plainBody = HtmlSanitizer.StripHtml(body);
            plainBody = Regex.Replace(plainBody, @"(\r\n){2,}", "\r\n\r\n");
            plainBody = Regex.Replace(plainBody, @"(\n){2,}", "\n");

            foreach (Match match in rx.Matches(plainBody))
            {
                if(match.Value.Split().Length > 5)
                    sentences.Add(match.Value);
            }

            return sentences;
        }
    }
}
