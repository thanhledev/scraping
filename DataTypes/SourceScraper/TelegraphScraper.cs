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
    public class TelegraphScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

                var dateValue = mainContent.Descendants("p").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "publishedDate").First().InnerText;

                dateValue = dateValue.Replace(" BST", "").Trim().Replace("AM", " AM").Replace("PM", " PM").Trim();

                return DateTime.ParseExact(dateValue, "h:mm tt dd MMM yyyy", null);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

                //find body
                var rawBody = mainContent.SelectSingleNode("//div[@id='mainBodyArea']");

                var bodyParts = (from divs in rawBody.Descendants()
                                 where divs.Name == "div"
                                 select divs).ToList();

                htmlBody = "";

                foreach (var part in bodyParts)
                {
                    if (part.Attributes["class"] != null)
                    {
                        if (part.Attributes["class"].Value.Contains("firstPar") || part.Attributes["class"].Value.Contains("secondPar") || part.Attributes["class"].Value.Contains("thirdPar")
                            || part.Attributes["class"].Value.Contains("fourthPar") || part.Attributes["class"].Value.Contains("fifthPar") || part.Attributes["class"].Value.Contains("body"))
                            htmlBody += part.InnerHtml;
                    }
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

                //find image
                var featuredImageLocation = mainContent.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("ssImg"));

                if (featuredImageLocation.Count() > 0)
                {
                    image = featuredImageLocation.First().Descendants("img").First().Attributes["src"].Value;
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

                shampoo = HttpUtility.HtmlDecode(mainContent.Descendants("h2").First().InnerText.Trim());
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

                description = HttpUtility.HtmlDecode(mainContent.Descendants("h2").First().InnerText.Trim());
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
                        temp.Add(word.Trim());
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

                //find body
                var rawBody = mainContent.SelectSingleNode("//div[@id='mainBodyArea']");

                var bodyParts = (from divs in rawBody.Descendants()
                                 where divs.Name == "div"
                                 select divs).ToList();

                htmlBody = "";

                foreach (var part in bodyParts)
                {
                    if (part.Attributes["class"] != null)
                    {
                        if (part.Attributes["class"].Value.Contains("firstPar") || part.Attributes["class"].Value.Contains("secondPar") || part.Attributes["class"].Value.Contains("thirdPar")
                            || part.Attributes["class"].Value.Contains("fourthPar") || part.Attributes["class"].Value.Contains("fifthPar") || part.Attributes["class"].Value.Contains("body"))
                            htmlBody += part.InnerHtml;
                    }
                }
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
