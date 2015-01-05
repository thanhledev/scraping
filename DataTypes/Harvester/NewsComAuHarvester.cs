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
    public class NewsComAuHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = DateTime.Now.AddMinutes((page * -1) * 60);

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var bigContent = document.DocumentNode.SelectSingleNode("//div[@class='content-item cipos-1 cirpos-1']");

                if (bigContent != null)
                {
                    href = bigContent.Descendants("h4").First().Descendants("a").First().Attributes["href"].Value;

                    if (!href.Contains("http"))
                        href = host + href;

                    if (!IsContains(result, href))
                        result.Add(new HarvestLink(href, hrefDate, link));
                }

                var mainContent1 = document.DocumentNode.SelectSingleNode("//div[@class='item ipos-2 irpos-2']");

                if (mainContent1 != null)
                {
                    var links = mainContent1.Descendants("a").ToList();

                    if (links.Count > 0)
                    {
                        foreach (var lnk in links)
                        {
                            if (lnk.Attributes["class"] != null)
                            {
                                if (lnk.Attributes["class"].Value == "thumb-link")
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

                var mainContent2 = document.DocumentNode.SelectSingleNode("//div[@class='item ipos-3 irpos-1']");

                if (mainContent2 != null)
                {
                    var links = mainContent2.Descendants("a").ToList();

                    if (links.Count > 0)
                    {
                        foreach (var lnk in links)
                        {
                            if (lnk.Attributes["class"] != null)
                            {
                                if (lnk.Attributes["class"].Value == "thumb-link")
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
