using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Text.RegularExpressions;
using DataTypes.Enums;
using System.Globalization;
using System.Web;
namespace DataTypes
{
    public class TheThao247Scraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                if (document.DocumentNode.SelectSingleNode("//h1[@class='title_detail']") != null)
                    title = document.DocumentNode.SelectSingleNode("//h1[@class='title_detail']").InnerText;
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
                var articleDate = document.DocumentNode.SelectSingleNode("//p[@class='f11 hd-desd']").InnerText;

                string[] values = articleDate.Split('|');
                string[] values1 = values[0].Split(',');
                values1[1] = values1[1].Trim();
                values1[1] = values1[1].Replace("GMT+7", "").Trim();

                return DateTime.ParseExact(values1[1], "dd/MM/yyyy HH:mm", null);
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public string GetImage(HtmlDocument document, ArticleType type, string url)
        {
            var host = "http://" + new Uri(url).Host;
            string image = "not-found-image";
            try
            {
                var imageList = document.DocumentNode.SelectSingleNode("//div[@id='main-detail']").Descendants("img");

                if (imageList.Count() > 0)
                {
                    image = imageList.First().Attributes["src"].Value;
                    if (!image.Contains("http://"))
                        image = host + image;
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
                if (document.DocumentNode.SelectSingleNode("//p[@class='sapo_detail']") != null)
                {
                    shampoo = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//p[@class='sapo_detail']").InnerText);
                }
            }
            catch (Exception)
            {
            
            }           
            return shampoo;
        }

        public string GetBody(HtmlDocument document, ArticleType type, string url)
        {
            var body = GetHtmlBody(document, type, url);

            string plainBody = HtmlSanitizer.StripHtml(body);
            plainBody = Regex.Replace(plainBody, @"(\r\n){2,}", "\r\n\r\n");

            return plainBody;

            //var body = document.DocumentNode.SelectSingleNode("//div[@id='detail_post']").InnerHtml;
            //Dictionary<string, string> ClearImages = new Dictionary<string, string>();
            ////var Images = document.DocumentNode.SelectSingleNode("//div[@id='detail_post']").Descendants("p").Where(a => a.Attributes["style"] != null);

            ////foreach (var img in Images)
            ////{
            ////    if (img.InnerHtml.Trim() != string.Empty)
            ////    {
            ////        var key = img.OuterHtml;
            ////        var value = img.Descendants("img").First().Attributes["src"].Value;

            ////        ClearImages.Add(key, value);
            ////    }
            ////}

            //var Images = document.DocumentNode.SelectSingleNode("//div[@id='detail_post']").Descendants("img");

            //foreach (var img in Images)
            //{
            //    var key = img.OuterHtml;
            //    var value = img.Attributes["src"].Value + "\n";
            //    ClearImages.Add(key, value);
            //}

            //foreach (var i in ClearImages)
            //{
            //    body = body.Replace(i.Key, i.Value);
            //}

            //return HtmlSanitizer.StripHtml(body);
        }

        public string GetHtmlBody(HtmlDocument document, ArticleType type, string url)
        {
            var htmlBody = "";
            try
            {
                var rawHtmlBody = document.DocumentNode.SelectSingleNode("//div[@id='main-detail']");

                Dictionary<string,string> annoyImage = new Dictionary<string,string>();
                Dictionary<string, string> articleAnchors = new Dictionary<string, string>();
                List<string> removeParts = new List<string>();

                var suggestBoxes = rawHtmlBody.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value == "add").ToList();

                if (suggestBoxes.Count > 0)
                {
                    foreach (var box in suggestBoxes)
                        removeParts.Add(box.OuterHtml);
                }

                var memes = rawHtmlBody.Descendants("div").Where(a => a.Attributes["id"] != null && a.Attributes["id"].Value.Contains("Meme_")).ToList();

                if (memes.Count > 0)
                {
                    foreach (var meme in memes)
                        removeParts.Add(meme.OuterHtml);
                }

                var rawDescendants = rawHtmlBody.Descendants().ToList();

                foreach (var item in rawDescendants)
                {
                    var videoAnchor = item.Descendants("a").Where(a => a.Attributes["href"] != null && a.Attributes["href"].Value.Contains("video")).ToList();

                    if (videoAnchor.Count > 0)
                        removeParts.Add(item.OuterHtml);
                }

                foreach (var item in rawDescendants)
                {
                    var itemImages = item.Descendants("img").ToList();

                    if (itemImages.Count > 0)
                    {
                        foreach (var image in itemImages)
                        {
                            if(image.Attributes["style"] != null && image.Attributes["style"].Value == "display: block; margin-left: auto; margin-right: auto;")
                            {
                                var imageHtml = "<img style=\"display: block; margin-left: auto; margin-right: auto;\" src=\"" + image.Attributes["src"].Value + "\" alt=\"" + image.Attributes["alt"].Value + "\" />";

                                annoyImage.Add(item.OuterHtml, imageHtml);
                            }
                        }
                    }
                }

                foreach (var item in annoyImage)
                    htmlBody.Replace(item.Key, item.Value);

                htmlBody = rawHtmlBody.InnerHtml;

                foreach (var part in removeParts)
                    htmlBody = htmlBody.Replace(part, "");

                var rawArticleAnchors = rawHtmlBody.Descendants("a").ToList();

                if (rawArticleAnchors.Count > 0)
                {
                    foreach (var rawAnchor in rawArticleAnchors)
                    {
                        if(!rawAnchor.Attributes["href"].Value.Contains("video"))
                            articleAnchors.Add(rawAnchor.OuterHtml, rawAnchor.InnerText);
                    }
                }

                foreach (var anchor in articleAnchors)
                    htmlBody.Replace(anchor.Key, anchor.Value);

                htmlBody = htmlBody.Replace("h2", "p");

                htmlBody = HtmlSanitizer.SanitizeHtml(htmlBody);
                htmlBody = Regex.Replace(htmlBody, @"^\s*$[\r\n]*", "", RegexOptions.Multiline);
                htmlBody = HttpUtility.HtmlDecode(htmlBody);
            }
            catch (Exception)
            {

            }
            return htmlBody;
        }

        public string GetSEOTitle(HtmlDocument document, ArticleType type)
        {
            try
            {
                return document.DocumentNode.SelectSingleNode("//title").InnerText;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string GetSEODescription(HtmlDocument document, ArticleType type)
        {
            try
            {
                return document.DocumentNode.SelectSingleNode("//meta[@name='description']").Attributes["content"].Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public List<string> GetTags(HtmlDocument document, ArticleType type)
        {
            List<string> articleTags = new List<string>();
            try
            {
                List<string> temp = new List<string>();

                var tagDiv = document.DocumentNode.SelectSingleNode("//div[@class='tagdfds']");

                var plainBody = GetBody(document, type, "");

                var links = tagDiv.Descendants("a").ToList();

                foreach (var link in links)
                {
                    var value = link.InnerText;
                    int appeareance = 0;

                    CompareInfo sampleInfo = CultureInfo.InvariantCulture.CompareInfo;

                    int pos = sampleInfo.IndexOf(plainBody, value, CompareOptions.IgnoreCase);

                    while (pos > 0)
                    {
                        appeareance++;
                        pos = sampleInfo.IndexOf(plainBody, value, pos + 1, CompareOptions.IgnoreCase);
                    }

                    if (appeareance > 0)
                    {
                        articleTags.Add(value);
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
            var body = document.DocumentNode.SelectSingleNode("//div[@id='main-detail']");
            var paragraphs = body.Descendants("p");
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
        }
    }
}
