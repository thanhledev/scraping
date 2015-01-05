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
    public class IBongdaHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate;

            if (document.DocumentNode.InnerText != string.Empty)
            {
                if (link.Contains("nhan-dinh"))
                {
                    var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='bound-zone-predictions']");

                    if (mainContent != null)
                    {
                        var contents = mainContent.Descendants("li").ToList();

                        if (contents.Count > 0)
                        {
                            foreach (var content in contents)
                            {
                                href = content.Descendants("a").First().Attributes["href"].Value;

                                if (!href.Contains("http"))
                                    href = host + href;

                                var dateTimeLocation = content.SelectSingleNode("meta[@itemprop='datePublished']");

                                if (dateTimeLocation != null)
                                {
                                    string value = dateTimeLocation.Attributes["content"].Value;

                                    hrefDate = DateTime.ParseExact(value, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                                }
                                else
                                    hrefDate = DateTime.Now;

                                result.Add(new HarvestLink(href, hrefDate, link));
                            }
                        }
                    }
                }
                else
                {
                    var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='content-left-news']");

                    if (mainContent != null)
                    {
                        var contents = mainContent.SelectNodes("//div[@class='news-info']");

                        if(contents.Count > 0)
                        {
                            foreach (var content in contents)
                            {
                                href = content.Descendants("a").First().Attributes["href"].Value;

                                if (!href.Contains("http"))
                                    href = host + href;

                                var dateTimeLocation = content.SelectSingleNode("div[@class='news-date-time']");

                                if (dateTimeLocation != null)
                                {
                                    string value = dateTimeLocation.InnerText.Trim();

                                    hrefDate = DateTime.ParseExact(value, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    hrefDate = DateTime.Now;
                                }

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
