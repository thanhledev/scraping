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
    public class BongDaLivesScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                title = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@class='title1']").SelectSingleNode("//h1").InnerText);
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
                string value = document.DocumentNode.SelectSingleNode("//div[@class='title1']").SelectSingleNode("//span[@class='small']").InnerText;

                value = value.Replace("<!--" + RemoveComment(value) + "-->", "").Trim();

                string[] content = value.Split('-');

                content[1] = content[1].Trim();

                return DateTime.ParseExact(content[1], "dd/MM/yyyy HH:mm", null);
                 
            }
            catch
            {
                return DateTime.Now;
            }
        }

        private string RemoveComment(string value)
        {
            string openTag = "<!--";
            string closedTag = "-->";

            int startIndex = value.IndexOf(openTag) + openTag.Length;
            int length = value.IndexOf(closedTag) - startIndex;

            return value.Substring(startIndex, length);
        }

        public string GetHtmlBody(HtmlDocument document, ArticleType type, string url)
        {
            var htmlBody = "";
            try
            {
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@id='hometext']");

                List<string> removeParts = new List<string>();

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@id='imghome']").OuterHtml);

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@class='gtng']").OuterHtml);

                var sponsorLinks = rawHtmlBody.SelectNodes("//span[@style='font-size:16px;']");

                if (sponsorLinks != null)
                {
                    foreach (var link in sponsorLinks)
                    {
                        if (link.InnerText.Contains("Vip Tip") || link.InnerText.Contains("Card Tip"))
                        {
                            string removePart = link.ParentNode.OuterHtml;

                            removeParts.Add(removePart);
                        }
                    }
                }

                //removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@id='___plusone_0']").OuterHtml);

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@class='box clearfix']").OuterHtml);

                var otherNewsLocations = rawHtmlBody.SelectNodes("//div[@class='other-news']");

                foreach (var location in otherNewsLocations)
                {
                    removeParts.Add(location.OuterHtml);
                }

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@class='g-plusone']").OuterHtml);

                var tables = rawHtmlBody.SelectNodes("//table");

                foreach (var table in tables)
                {
                    removeParts.Add(table.OuterHtml);
                }

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
                var imageLocation = document.DocumentNode.SelectSingleNode("//div[@id='imghome']");

                image = imageLocation.Descendants("a").First().Attributes["href"].Value;

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
                shampoo = document.DocumentNode.SelectSingleNode("//div[@class='gtng']").InnerText;
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
                seoTitle = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@class='title1']").SelectSingleNode("//h1").InnerText);
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
            List<string> tags = new List<string>();

            try
            {
                string title = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@class='title1']").SelectSingleNode("//h1").InnerText);

                if (!title.Contains("Tổng hợp"))
                {
                    string[] content = title.Split(':');

                    content[1] = content[1].Replace(content[1].Substring(0, 3), "");

                    tags.Add(content[1]);

                    string[] temp = content[1].Split(new string[] { "vs" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var i in temp)
                        tags.Add(i);
                }
                else
                {
                    var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='hometext']");

                    if (mainContent != null)
                    {
                        var matchTitles = (from strong in mainContent.Descendants("strong")
                                           where strong.Attributes["style"] != null && strong.Attributes["style"].Value.Contains("font-size: 20px;")
                                           select strong).ToList();

                        if (matchTitles.Count > 0)
                        {
                            foreach (var mTitle in matchTitles)
                            {
                                string value = mTitle.InnerText;

                                string[] content = value.Split(':');

                                content[1] = content[1].Replace(content[1].Substring(0, 3), "");

                                tags.Add(content[1]);

                                string[] temp = content[1].Split(new string[] { "vs" }, StringSplitOptions.RemoveEmptyEntries);

                                foreach (var i in temp)
                                    tags.Add(i);
                            }
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
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@id='hometext']");

                List<string> removeParts = new List<string>();

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@id='imghome']").OuterHtml);

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@class='gtng']").OuterHtml);

                var sponsorLinks = rawHtmlBody.SelectNodes("//span[@style='font-size:16px;']");

                if (sponsorLinks != null)
                {
                    foreach (var lnk in sponsorLinks)
                    {
                        if (lnk.InnerText.Contains("Vip Tip") || lnk.InnerText.Contains("Card Tip"))
                        {
                            string removePart = lnk.ParentNode.OuterHtml;

                            removeParts.Add(removePart);
                        }
                    }
                }

                //removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@id='___plusone_0']").OuterHtml);

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@class='box clearfix']").OuterHtml);

                var otherNewsLocations = rawHtmlBody.SelectNodes("//div[@class='other-news']");

                foreach (var location in otherNewsLocations)
                {
                    removeParts.Add(location.OuterHtml);
                }

                removeParts.Add(rawHtmlBody.SelectSingleNode("//div[@class='g-plusone']").OuterHtml);

                var tables = rawHtmlBody.SelectNodes("//table");

                foreach (var table in tables)
                {
                    removeParts.Add(table.OuterHtml);
                }

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
                return new List<string>();
            }

            var plainBody = HtmlSanitizer.StripHtml(htmlBody);

            List<string> sentences = new List<string>();
            Regex rx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");

            foreach (Match match in rx.Matches(plainBody))
            {
                if (match.Value.Split().Length > 5 && !match.Value.Contains("<!--"))
                    sentences.Add(match.Value);
            }

            return sentences;
        }
    }
}
