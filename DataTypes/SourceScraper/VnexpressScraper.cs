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
    public class VnexpressScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                title = document.DocumentNode.SelectSingleNode("//h1").InnerText;
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
                var articlePublish = document.DocumentNode.SelectSingleNode("//div[@class='block_timer left txt_666']").InnerText;

                string[] preContent = articlePublish.Split(',');

                string[] preDateTimeContent = preContent[1].Trim().Split('|');

                string date = preDateTimeContent[0].Trim();

                string time = preDateTimeContent[1].Trim().Replace("GMT+7", "").Trim();

                return DateTime.ParseExact(date + " " + time, "dd/MM/yyyy HH:mm", null);
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
                switch (type)
                {
                    case ArticleType.NewsArticle:
                        var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@class='fck_detail width_common']");

                        List<string> removeParts = new List<string>();

                        removeParts.Add(rawHtmlBody.SelectSingleNode("p[@style='text-align:right;']").OuterHtml);

                        htmlBody = rawHtmlBody.InnerHtml;

                        foreach (var part in removeParts)
                            htmlBody = htmlBody.Replace(part, "");

                        htmlBody = HtmlSanitizer.SanitizeHtml(htmlBody);
                        htmlBody = Regex.Replace(htmlBody, @"^\s*$[\r\n]*", "", RegexOptions.Multiline);
                        htmlBody = HttpUtility.HtmlDecode(htmlBody);
                        break;
                    case ArticleType.GalleryArticle:

                        var rawHtmlBody1 = document.DocumentNode.SelectSingleNode("//div[@id='article_content']");

                        htmlBody = rawHtmlBody1.InnerHtml;

                        htmlBody = HtmlSanitizer.SanitizeHtml(htmlBody);
                        htmlBody = Regex.Replace(htmlBody, @"^\s*$[\r\n]*", "", RegexOptions.Multiline);
                        htmlBody = HttpUtility.HtmlDecode(htmlBody);

                        break;
                };
                
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
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@class='fck_detail width_common']");

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
                shampoo = document.DocumentNode.SelectSingleNode("//div[@class='short_intro txt_666']").InnerText.Trim();
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
                title = document.DocumentNode.SelectSingleNode("//h1").InnerText;
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
                description = document.DocumentNode.SelectSingleNode("//div[@class='short_intro txt_666']").InnerText.Trim();
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
                var articleTagLocation = document.DocumentNode.SelectSingleNode("//div[@class='block_tag width_common space_bottom_20']");

                var temp = articleTagLocation.Descendants("a").Select(a => a.InnerText).ToList();

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
            switch (type)
            {
                case ArticleType.NewsArticle:
                    var nbody = document.DocumentNode.SelectSingleNode("//div[@class='fck_detail width_common']");

                    var paragraphs = nbody.Descendants("p");
                    List<string> sentences = new List<string>();
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

                    return sentences;
                    
                case ArticleType.GalleryArticle:
                    var gbody = document.DocumentNode.SelectSingleNode("//div[@id='article_content']");

                    var gparagraphs = gbody.Descendants("p");
                    List<string> gsentences = new List<string>();
                    Regex grx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");

                    foreach (var para in gparagraphs)
                    {
                        string plainBody = para.InnerText;
                        foreach (Match match in grx.Matches(plainBody))
                        {
                            if(match.Value.Split().Length > 5)
                                gsentences.Add(match.Value);
                        }
                    }

                    return gsentences;
                    
                default:
                    return new List<string>();
            }
        }
    }
}
