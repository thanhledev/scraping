using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DataTypes
{
    public class DatingAdviceHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = new DateTime();

            if (document.DocumentNode.InnerText != string.Empty)
            {
                //var recentArticles = document.DocumentNode.SelectSingleNode("//div[@id='recent-articles-and-howto']");

                //if (recentArticles != null)
                //{
                //    var articles = (from divs in recentArticles.Descendants()
                //                    where divs.Name == "div" && divs.Attributes["class"] != null && divs.Attributes["class"].Value.Contains("span-featured")
                //                    select divs).ToList();
                //    if (articles.Count > 0)
                //    {
                //        foreach (var article in articles)
                //        {
                //            href = article.Descendants("a").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "thumbnail").First().Attributes["href"].Value;

                //            if (!href.Contains("http"))
                //                href = host + href;

                //            var dateValue = article.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "overlay").First().InnerText;

                //            result.Add(new HarvestLink(href, DateTime.ParseExact(dateValue, "MMMM d, yyyy", null), link));
                //        }
                //    }
                //}

                //var listArticles = document.DocumentNode.SelectSingleNode("//div[@id='studies-carousel']");

                //if (listArticles != null)
                //{
                //    var articles = (from artl in listArticles.Descendants()
                //                    where artl.Name == "article"
                //                    select artl).ToList();

                //    if (articles.Count > 0)
                //    {
                //        foreach (var article in articles)
                //        {
                //            href = article.Descendants("h2").First().Descendants("a").First().Attributes["href"].Value;

                //            if (!href.Contains("http"))
                //                href = host + href;                            

                //            var date = (from div in article.Descendants()
                //                        where div.Name == "div" && div.Attributes["class"] != null && div.Attributes["class"].Value.Contains("published")
                //                        select div).First().InnerText;

                //            date = date.Replace("Published:", "").Trim();

                //            result.Add(new HarvestLink(href, DateTime.ParseExact(date, "MM/dd/yy", null), link));
                //        }
                //    }
                //}

                if (!link.Contains("studies"))
                {
                    var recentArticles = document.DocumentNode.SelectSingleNode("//div[@id='latest-advice-articles']");

                    if (recentArticles != null)
                    {
                        var articles = (from artl in recentArticles.Descendants("div")
                                        where artl.Attributes["class"] != null && artl.Attributes["class"].Value.Contains("article-container")
                                        select artl).ToList();

                        if (articles.Count > 0)
                        {
                            foreach (var article in articles)
                            {
                                href = article.Descendants("h2").First().Descendants("a").First().Attributes["href"].Value;

                                if (!href.Contains("http"))
                                    href = host + href;

                                if (!IsContains(result, href))
                                {
                                    try
                                    {
                                        var date = (from div in article.Descendants()
                                                    where div.Name == "div" && div.Attributes["class"] != null && div.Attributes["class"].Value.Contains("article-author")
                                                    select div).First().InnerText;

                                        //date = date.Replace("<!-- end .author-avatar -->", "").Trim();

                                        //date = Regex.Replace(date, @"\t|\n|\r", "");

                                        string[] content = date.Split(new string[] { "&bull;" }, StringSplitOptions.RemoveEmptyEntries);

                                        string[] dateContent = content[1].Split('/');

                                        string fullDateTime = dateContent[0].Trim() + "/" + dateContent[1].Trim() + "/" + dateContent[2].Trim();

                                        hrefDate = DateTime.ParseExact(fullDateTime, "M/dd/yyyy", null);

                                        result.Add(new HarvestLink(href, hrefDate, link));
                                    }
                                    catch (Exception)
                                    {
                                        result.Add(new HarvestLink(href, hrefDate, link));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='studies-carousel']");

                    if (mainContent != null)
                    {
                        var recentArticles = mainContent.Descendants("article").ToList();

                        if (recentArticles.Count > 0)
                        {
                            foreach (var article in recentArticles)
                            {
                                var anchors = article.Descendants("a").ToList();

                                if (anchors.Count > 0)
                                {
                                    foreach (var anchor in anchors)
                                    {
                                        if (anchor.Attributes["class"] != null && anchor.Attributes["class"].Value.Contains("read-more"))
                                            href = anchor.Attributes["href"].Value;
                                    }
                                }

                                var dateTimeLocation = (from divs in article.Descendants("div")
                                                        where divs.Attributes["class"] != null && divs.Attributes["class"].Value.Contains("study-published")
                                                        select divs);

                                if (dateTimeLocation != null)
                                {
                                    var dateTimeValue = dateTimeLocation.First().InnerText.Trim();

                                    dateTimeValue = dateTimeValue.Replace("PUBLISHED: ", "");

                                    string[] content = dateTimeValue.Split('/');

                                    string finalDateTime = content[0] + "/" + content[1] + "/" + "20" + content[2];

                                    hrefDate = DateTime.ParseExact(finalDateTime, "M/d/yyyy", null);
                                }

                                if (!IsContains(result, href))
                                    result.Add(new HarvestLink(href, hrefDate, link));
                            }
                        }
                    }
                }
            }

            return result;
        }

        public int PageNumber(string link)
        {
            return 0;
        }

        private bool IsContains(List<HarvestLink> list, string value)
        {
            foreach (var item in list)
            {
                if (item.articleUrl == value)
                    return true;
            }
            return false;
        }
    }
}
