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
    public class DantriScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                title = document.DocumentNode.SelectSingleNode("//h1[@class='fon31 mt2']").InnerText;
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
                var articlePublish = document.DocumentNode.SelectSingleNode("//span[@class='fr fon7 mr2']").InnerText.Trim();

                string[] preContent = articlePublish.Split(',');

                preContent[1] = preContent[1].Trim();

                return DateTime.ParseExact(preContent[1], "dd/MM/yyyy - HH:mm", null);
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
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@class='fon34 mt3 mr2 fon43']");

                List<string> removeParts = new List<string>();

                var authorFootprints = rawHtmlBody.SelectNodes("//p[@align='right' or @style='text-align: right;']");

                if (authorFootprints != null)
                {
                    foreach (var footprint in authorFootprints)
                        removeParts.Add(footprint.OuterHtml);
                }

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@class='news-tag']").OuterHtml);

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
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@class='fon34 mt3 mr2 fon43']");

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
                shampoo = document.DocumentNode.SelectSingleNode("//h2[@class='fon33 mt1']").InnerText.Trim();
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
                seoTitle = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@class='title1']").SelectSingleNode("//h1").InnerText.Trim());
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
                List<string> temp = new List<string>();

                var articleTagsLocation = document.DocumentNode.SelectSingleNode("//span[@class='news-tags-item']");

                if (articleTagsLocation != null)
                {
                    var links = articleTagsLocation.Descendants("a").ToList();

                    if (links != null)
                    {
                        foreach (var link in links)
                        {
                            temp.Add(link.InnerText);
                        }
                    }

                    var plainBody = GetBody(document, type, "");

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
            }
            catch (Exception)
            {

            }
            return articleTags;
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            var body = document.DocumentNode.SelectSingleNode("//div[@class='fon34 mt3 mr2 fon43']");
            var paragraphs = body.Descendants("p");
            List<string> sentences = new List<string>();
            Regex rx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");

            foreach (var para in paragraphs)
            {
                string plainBody = para.InnerText;
                plainBody = Regex.Replace(plainBody, @"\r\n", " ");
                foreach (Match match in rx.Matches(plainBody))
                {
                    if(match.Value.Split().Length > 5)
                        sentences.Add(match.Value);
                }
            }

            return sentences;
        }
    }
}
