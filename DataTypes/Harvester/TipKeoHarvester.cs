using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Interfaces;
using DataTypes.Collections;

namespace DataTypes
{
    public class TipKeoHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();
            DateTime hrefDate = DateTime.Now.AddMinutes((page * -1) * 60);

            var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='blk']");

            var links = mainContent.Descendants("a").ToList();

            foreach (var lnk in links)
            {
                if (lnk.Attributes["class"] != null)
                {
                    if (lnk.Attributes["class"].Value == "news-it-label" && !IsContains(result, lnk.Attributes["href"].Value))
                    {
                        string hrefValue = lnk.Attributes["href"].Value;

                        if (!hrefValue.Contains("http"))
                            hrefValue = host + hrefValue;

                        result.Add(new HarvestLink(hrefValue, hrefDate, link));
                    }
                }
            }

            return result;
        }

        public int PageNumber(string link)
        {
            if (link.Contains("s_p:"))
            {
                string[] content = link.Split('/');

                return Convert.ToInt32(content.Last().Replace("s_p:", "")) - 1;
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
