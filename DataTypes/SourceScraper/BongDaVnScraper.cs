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
    public class BongDaVnScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            string title = "not-found-title";
            try
            {
                switch (type)
                {
                    case ArticleType.NewsArticle:
                        if (document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_art_title']") != null)
                            title = document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_art_title']").InnerText;
                        break;
                    case ArticleType.GalleryArticle:
                        if (document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_title']") != null)
                            title = document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_title']").InnerText;
                        break;
                    case ArticleType.VideoArticle:
                        break; //not implement yet
                    default:
                        break; //not implement yet
                };
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
                string value = "";

                switch (type)
                {
                    case ArticleType.NewsArticle:
                        value = document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_art_date']").InnerText;
                        break;
                    case ArticleType.GalleryArticle:
                        value = document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_date']").InnerText;
                        break;
                    case ArticleType.VideoArticle:
                        return DateTime.Now; // not be implemented yet
                };
                return DateTime.ParseExact(value, "H:m d/M/yyyy", null);
            }
            catch
            {
                return DateTime.Now;
            }            
        }        

        /// <summary>
        /// GetBody new ways
        /// </summary>
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
                switch (type)
                {
                    case ArticleType.NewsArticle:
                        if (document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_art_pnlNewsContainer']").Descendants("img").Count() > 0)
                        {
                            image = document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_art_pnlNewsContainer']").Descendants("img").First().Attributes["src"].Value;
                            if (!image.Contains("http://"))
                                image = host + image;
                        }
                        break;
                    case ArticleType.GalleryArticle:
                        if (document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_pnlNewsContainer']").Descendants("img").Count() > 0)
                        {
                            image = document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_pnlNewsContainer']").Descendants("img").First().Attributes["src"].Value;
                            if (!image.Contains("http://"))
                                image = host + image;
                        }
                        break;
                    case ArticleType.VideoArticle:
                        return string.Empty; //not implement yet
                    default:
                        return string.Empty;
                };
            }
            catch (Exception ex)
            {

            }
            return image;
        }

        public string GetShampoo(HtmlDocument document, ArticleType type)
        {
            var shampoo = "not-found-shampoo";
            try
            {

                switch (type)
                {
                    case ArticleType.NewsArticle:
                        if (document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_art_pnlNewsContainer']").Descendants("strong").Count() > 0)
                            shampoo = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_art_pnlNewsContainer']").Descendants("strong").First().InnerText);
                        break;
                    case ArticleType.GalleryArticle:
                        if (document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_pnlNewsContainer']").Descendants("strong").Count() > 0)
                            shampoo = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_pnlNewsContainer']").Descendants("strong").First().InnerText);
                        break;
                    case ArticleType.VideoArticle:
                        break; //not implement yet
                    default:
                        break; //not implement yet
                };
            }
            catch (Exception ex)
            {
 
            }
            return shampoo;
        }

        public string GetHtmlBody(HtmlDocument document, ArticleType type, string url)
        {
            string body = "";
            //var host = "http://" + new Uri(url).Host;            
            
            switch (type)
            {
                case ArticleType.NewsArticle:
                    var mainNewsContent = document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_art_pnlNewsContainer']");

                    Dictionary<string, string> replaceNewsImages = new Dictionary<string, string>();

                    var newsImages = mainNewsContent.Descendants("img").ToList();

                    if (newsImages.Count > 0)
                    {
                        foreach (var image in newsImages)
                        {
                            var imgSource = image.Attributes["src"].Value;

                            if (!imgSource.Contains("www.bongda.com.vn") && !imgSource.Contains("www1.bongda.com.vn"))
                            {
                                var oldImageHtml = image.OuterHtml;
                                
                                var newImageSource = "http://www.bongda.com.vn" + imgSource;
                                var newImageWidth = "";
                                var newImageHeight = "";
                                
                                if(image.Attributes["width"] != null)
                                    newImageWidth = "width=\"" + image.Attributes["width"].Value + "\"";
                                if(image.Attributes["height"] != null)
                                    newImageHeight = "height=\"" + image.Attributes["height"].Value + "\"";

                                var newImageHtml = "<img " + newImageWidth + " " + newImageHeight + " src=\"" + newImageSource + "\" alt=\"\" />";

                                replaceNewsImages.Add(oldImageHtml, newImageHtml);
                            }
                        }
                    }

                    body = mainNewsContent.InnerHtml;

                    foreach (var item in replaceNewsImages)
                    {
                        body = body.Replace(item.Key, item.Value);
                    }
            
                    break;
                case ArticleType.GalleryArticle:
                    var mainGalleryContent = document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_pnlNewsContainer']");

                    Dictionary<string, string> replaceGalleryImages = new Dictionary<string, string>();

                    var galleryImages = mainGalleryContent.Descendants("img").ToList();

                    if (galleryImages.Count > 0)
                    {
                        foreach (var image in galleryImages)
                        {
                            var imgSource = image.Attributes["src"].Value;

                            if (!imgSource.Contains("www.bongda.com.vn") && !imgSource.Contains("www1.bongda.com.vn"))
                            {
                                var oldImageHtml = image.OuterHtml;
                                
                                var newImageSource = "http://www.bongda.com.vn" + imgSource;
                                var newImageWidth = "";
                                var newImageHeight = "";
                                
                                if(image.Attributes["width"] != null)
                                    newImageWidth = "width=\"" + image.Attributes["width"].Value + "\"";
                                if(image.Attributes["height"] != null)
                                    newImageHeight = "height=\"" + image.Attributes["height"].Value + "\"";

                                var newImageHtml = "<img " + newImageWidth + " " + newImageHeight + " src=\"" + newImageSource + "\" alt=\"\" />";

                                replaceGalleryImages.Add(oldImageHtml, newImageHtml);
                            }
                        }
                    }

                    body = mainGalleryContent.InnerHtml;

                    foreach (var item in replaceGalleryImages)
                    {
                        body = body.Replace(item.Key, item.Value);
                    }

                    break;
                //case ArticleType.VideoArticle:
                // not be implemented yet    
                default:
                    return string.Empty;
            };
            
            body = HtmlSanitizer.SanitizeHtml(body);
            body = Regex.Replace(body, @"^\s*$[\r\n]*", "", RegexOptions.Multiline);
            body = HttpUtility.HtmlDecode(body);

            return body;
        }

        public string GetSEOTitle(HtmlDocument document, ArticleType type)
        {
            string title = "not-found-seo-title";
            try
            {
                switch (type)
                {
                    case ArticleType.NewsArticle:
                        if (document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_art_title']") != null)
                            title = document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_art_title']").InnerText;
                        break;
                    case ArticleType.GalleryArticle:
                        if (document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_title']") != null)
                            title = document.DocumentNode.SelectSingleNode("//span[@id='ctl00_BD_title']").InnerText;
                        break;
                    case ArticleType.VideoArticle:
                        break; //not implement yet
                    default:
                        break; //not implement yet
                };
            }
            catch (Exception)
            {

            }
            return title;
        }

        public string GetSEODescription(HtmlDocument document, ArticleType type)
        {
            var description = "not-found-description";
            
            switch (type)
            {
                case ArticleType.NewsArticle:
                    if (document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_art_pnlNewsContainer']").Descendants("strong").Count() > 0)
                        description = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_art_pnlNewsContainer']").Descendants("strong").First().InnerText);
                    break;
                case ArticleType.GalleryArticle:
                    if (document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_pnlNewsContainer']").Descendants("strong").Count() > 0)
                        description = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_pnlNewsContainer']").Descendants("strong").First().InnerText);
                    break;
                case ArticleType.VideoArticle:
                    break; //not implement yet
                default:
                    break; //not implement yet
            };

            return description;
        }

        public List<string> GetTags(HtmlDocument document, ArticleType type)
        {
            List<string> articleTags = new List<string>();
            switch (type)
            {
                case ArticleType.NewsArticle:
                    var tagDiv = document.DocumentNode.SelectSingleNode("//div[@id='topNew']");
                    //article tags
                    var articleLinks = tagDiv.Descendants("a").ToList();

                    List<string> temp = new List<string>();

                    foreach (var url in articleLinks)
                    {
                        if (url.Attributes["href"].Value.Contains("tags"))
                            if (url.InnerText.Split().Length <= 4)
                                if (!articleTags.Contains(HttpUtility.HtmlDecode(url.InnerText)))
                                    temp.Add(HttpUtility.HtmlDecode(url.InnerText));
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
                            articleTags.Add(i);
                        }
                    }

                    break;
                case ArticleType.GalleryArticle:
                    break;
                case ArticleType.VideoArticle:
                    break;
            };
            return articleTags;
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            string body = "";

            switch (type)
            {
                case ArticleType.NewsArticle:
                    body = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_art_pnlNewsContainer']").InnerText);
                    break;
                case ArticleType.GalleryArticle:
                    body = HttpUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//div[@id='ctl00_BD_pnlNewsContainer']").InnerText);
                    break;                
            }

            string plainBody = HtmlSanitizer.StripHtml(body);
            plainBody = Regex.Replace(plainBody, @"(\r\n){2,}", "\r\n\r\n"); //remove multiple blank lines
            plainBody = Regex.Replace(plainBody, @"(\n){2,}", "\n");

            List<string> sentences = new List<string>();

            Regex rx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");
            foreach (Match match in rx.Matches(plainBody))
            {
                if(match.Value.Split().Length > 5)
                    sentences.Add(match.Value);
            }

            return sentences;
        }
    }
}
