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
    public class IBongdaScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                title = document.DocumentNode.SelectSingleNode("//h1[@class='tt-cttin']").InnerText;
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
                string value = document.DocumentNode.SelectSingleNode("//p[@class='tt-datetime-nws']").SelectSingleNode("meta[@itemprop='datePublished']").Attributes["content"].Value;

                value = value.Replace('-', '/');

                return DateTime.ParseExact(value, "yyyy/MM/dd hh:mm", CultureInfo.InvariantCulture);
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
                shampoo = document.DocumentNode.SelectSingleNode("//p[@class='td-cttin']").InnerText;
            }
            catch (Exception)
            {
                
            }
            return shampoo;
        }

        public string GetHtmlBody(HtmlDocument document, ArticleType type, string url)
        {
            string htmlBody = "";
            try
            {
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@class='info-ct-news']");

                List<string> removeParts = new List<string>();

                //predict-heading
                var predictHeading = rawHtmlBody.SelectSingleNode("h2[@class='predict-heading']");

                if (predictHeading != null)
                    removeParts.Add(predictHeading.OuterHtml);

                //bang keo
                //var firstTable = rawHtmlBody.Descendants("table").First();

                //if (firstTable != null)
                //    removeParts.Add(firstTable.OuterHtml);

                //ads
                var predictAds = rawHtmlBody.SelectSingleNode("//div[@id='predict-news-zone-text-links']");

                if (predictAds != null)
                    removeParts.Add(predictAds.OuterHtml);

                //sms ads
                var smsAds = rawHtmlBody.SelectSingleNode("div[@id='predict-news-sms-ads']");

                if (smsAds != null)
                    removeParts.Add(smsAds.OuterHtml);
                
                //copy-right
                var copyright = rawHtmlBody.SelectSingleNode("div[@id='predict-copyright']");

                if (copyright != null)
                    removeParts.Add(copyright.OuterHtml);

                //iframes
                var statistics = rawHtmlBody.Descendants("p").ToList();

                foreach (var i in statistics)
                {
                    if (i.InnerText.Contains("Bảng thống kê kèo châu Á 10 trận gần đây của") || i.InnerText.Contains("Bảng thống kê kèo tài xỉu 10 trận gần đây của"))
                        removeParts.Add(i.OuterHtml);
                }

                //author
                var authorDiv = rawHtmlBody.SelectSingleNode("div[@itemprop='author']");

                if (authorDiv != null)
                    removeParts.Add(authorDiv.OuterHtml);

                htmlBody = rawHtmlBody.InnerHtml;

                foreach (var part in removeParts)
                {
                    htmlBody = htmlBody.Replace(part, "");
                }

                //process team images
                Dictionary<string, string> teamImages = new Dictionary<string, string>();

                var imageHrefs = (from anchors in rawHtmlBody.Descendants("a")
                                  where anchors.Attributes["class"] != null && anchors.Attributes["class"].Value.Contains("lable-link-team")
                                  select anchors).ToList();

                if (imageHrefs.Count > 0)
                {
                    foreach (var href in imageHrefs)
                    {
                        teamImages.Add(href.OuterHtml, href.InnerHtml);
                    }
                }

                foreach (var item in teamImages)
                {
                    htmlBody = htmlBody.Replace(item.Key, item.Value);
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

            //try
            //{
            //    var imageLocations = document.DocumentNode.SelectNodes("//p[@style='text-align: center;']");

            //    if (imageLocations != null)
            //    {
            //        foreach (var location in imageLocations)
            //        {
            //            var imgLocation = location.Descendants("img").ToList();

            //            if (imgLocation.Count > 0)
            //            {
            //                image = imgLocation.First().Attributes["src"].Value;
            //                break;
            //            }
            //        }                    
            //    }
            //}
            //catch (Exception)
            //{
                
            //}
            return image;
        }

        public string GetSEOTitle(HtmlDocument document, ArticleType type)
        {
            string seoTitle = "not-found-seo-title";
            try
            {
                seoTitle = document.DocumentNode.SelectSingleNode("//h1[@class='tt-cttin']").InnerText;
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
                description = document.DocumentNode.SelectSingleNode("//h1[@class='tt-cttin']").InnerText;
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

                var tagLocation = document.DocumentNode.SelectSingleNode("//div[@id='zone-news-detail-tags']");

                if (tagLocation != null)
                {
                    var tags = tagLocation.Descendants("a").ToList();

                    if (tags.Count > 0)
                    {
                        foreach (var tag in tags)
                        {
                            aTags.Add(tag.InnerText);
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
