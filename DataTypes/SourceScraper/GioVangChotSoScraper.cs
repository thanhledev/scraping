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
    public class GioVangChotSoScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='detail']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='article-content']");

                var articlePublish = mainContent.Descendants().Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("date")).First().Descendants("a").First().InnerText;

                string date = articlePublish.Replace("Ngày ", "").Trim();

                return DateTime.ParseExact(date, "dd/MM/yyyy", null);
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
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@id='detail']");

                List<string> removeParts = new List<string>();

                removeParts.Add("<p style=\"padding:10px 0px 10px 0px\"><b>Để lọc giải đặc biệt, thống kê và cầu lô tô chính xác nhất soạn tin:");

                removeParts.Add("(Xem nhiều thông tin kết quả xổ số tại");

                removeParts.Add(rawHtmlBody.Descendants("div").First().OuterHtml);

                var removeRelatedHeaders = rawHtmlBody.Descendants("h1").Where(a => a.Attributes["style"] != null && a.Attributes["style"].Value.Contains("font-size: 14px"));

                if (removeRelatedHeaders.Count() > 0)
                {
                    foreach (var header in removeRelatedHeaders)
                        removeParts.Add(header.OuterHtml);
                }

                var removeRelatedLinks = rawHtmlBody.Descendants("ul").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value.Contains("related"));

                if (removeRelatedLinks.Count() > 0)
                {
                    foreach (var relatedLink in removeRelatedLinks)
                        removeParts.Add(relatedLink.OuterHtml);
                }

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

            //try
            //{
            //    var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='main-slider-content']");

            //    image = mainContent.Descendants("img").First().Attributes["src"].Value;

            //    if (!image.Contains("http://"))
            //        image = host + image;
            //}
            //catch (Exception)
            //{

            //}

            return image;
        }

        public string GetShampoo(HtmlDocument document, ArticleType type)
        {
            var shampoo = "not-found-shampoo";
            try
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='detail']");

                shampoo = HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='detail']");

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
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='detail']");

                description = HttpUtility.HtmlDecode(mainContent.Descendants("h1").First().InnerText);
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
