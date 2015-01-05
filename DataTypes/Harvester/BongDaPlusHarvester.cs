using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Globalization;

namespace DataTypes
{
    public class BongDaPlusHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = DateTime.Now.AddMinutes((page * -1) * 60);

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var headLocation = document.DocumentNode.SelectSingleNode("//div[@class='grd5']");

                if (headLocation != null)
                {
                    var flink = headLocation.Descendants("a").First().Attributes["href"].Value;

                    if (!flink.Contains("http"))
                        flink = host + flink;

                    result.Add(new HarvestLink(flink, hrefDate, link));
                }

                var barLocation = document.DocumentNode.SelectSingleNode("//div[@class='mediabar']");

                if (barLocation != null)
                {
                     var bLinks = barLocation.Descendants("a").ToList();

                     if (bLinks.Count > 0)
                     {
                         foreach (var lnk in bLinks)
                         {
                             href = lnk.Attributes["href"].Value;

                             if (!href.Contains("http"))
                                 href = host + href;

                             if (!IsContains(result, href))
                                 result.Add(new HarvestLink(href, hrefDate, link));
                         }
                     }
                }

                var newsLocation = document.DocumentNode.SelectNodes("//*[(self::div[@class='cat_news grd6'] or div[@class='cat_news grdhalf'])]");

                if (newsLocation != null)
                {
                    foreach (var location in newsLocation)
                    {
                        var newsItems = location.SelectNodes("//div[@class='news_item']").ToList();

                        if (newsItems.Count > 0)
                        {
                            foreach (var item in newsItems)
                            {
                                var nLinks = item.Descendants("a").ToList();

                                if (nLinks.Count > 0)
                                {
                                    foreach (var lnk in nLinks)
                                    {
                                        href = lnk.Attributes["href"].Value;

                                        if (!href.Contains("http"))
                                            href = host + href;

                                        if (!IsContains(result, href))
                                            result.Add(new HarvestLink(href, hrefDate, link));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public int PageNumber(string link)
        {
            if (link.Contains("trang"))
            {
                string[] content = link.Split('/');

                string value = content.Last().Replace(".bdplus", "").Replace("trang-","").Trim();

                return Convert.ToInt32(value);
            }
            else
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
