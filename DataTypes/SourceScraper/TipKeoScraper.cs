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
    public class TipKeoScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                title = document.DocumentNode.SelectSingleNode("//h1[@class='article_name']").InnerText;
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
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@class='article_viewable']");

                List<string> removeParts = new List<string>();

                var qc = rawHtmlBody.SelectSingleNode("//div[@id='qc']");

                if (qc != null)
                    removeParts.Add(qc.OuterHtml);

                removeParts.Add(rawHtmlBody.SelectSingleNode("//h1[@class='article_name']").OuterHtml);

                htmlBody = rawHtmlBody.InnerHtml;

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
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@class='article_viewable']");

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
            return "";
        }

        public string GetSEOTitle(HtmlDocument document, ArticleType type)
        {
            string title = "not-found-seo-title";
            try
            {
                title = document.DocumentNode.SelectSingleNode("//h1[@class='article_name']").InnerText;
            }
            catch (Exception)
            {

            }
            return title;
        }

        public string GetSEODescription(HtmlDocument document, ArticleType type)
        {
            string title = "not-found-description";
            try
            {
                title = document.DocumentNode.SelectSingleNode("//h1[@class='article_name']").InnerText;
            }
            catch (Exception)
            {

            }
            return title;
        }

        public List<string> GetTags(HtmlDocument document, ArticleType type)
        {
            return new List<string>();
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            List<string> sentences = new List<string>();
            try
            {
                var body = document.DocumentNode.SelectSingleNode("//div[@class='article_viewable']");
                var paragraphs = body.Descendants("p");

                Regex rx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");

                foreach (var para in paragraphs)
                {
                    string plainBody = para.InnerText;
                    foreach (Match match in rx.Matches(plainBody))
                    {
                        if(match.Value.Split().Length > 5)
                            sentences.Add(match.Value);
                    }
                }
            }
            catch (Exception)
            {
               
            }           
            return sentences;
        }
    }
}
