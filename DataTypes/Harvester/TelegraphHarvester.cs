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
    public class TelegraphHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = new DateTime();

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='oneHalf gutter']");

                if (mainContent != null)
                {
                    var summaries = (from divs in mainContent.Descendants()
                                 where divs.Name == "div" && divs.Attributes["class"] != null && divs.Attributes["class"].Value == "summary"
                                 select divs).ToList();

                    if (summaries.Count > 0)
                    {
                        foreach (var content in summaries)
                        {
                            href = content.Descendants("h3").First().Descendants("a").First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;
                            var date = (from para in content.Descendants()
                                        where para.Name == "p" && para.Attributes["class"] != null && para.Attributes["class"].Value == "dateCC"
                                        select para).First().InnerText;
                            hrefDate = DateTime.ParseExact(date, "dd MMM yyyy", null);

                            if (!IsContains(result, href))
                                result.Add(new HarvestLink(href, hrefDate, link));
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
