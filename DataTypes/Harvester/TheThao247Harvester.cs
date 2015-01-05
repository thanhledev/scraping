using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Text.RegularExpressions;

namespace DataTypes
{
    public class TheThao247Harvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            List<HarvestLink> results = new List<HarvestLink>();
            string href = "";
            DateTime hrefDate = DateTime.Now.AddMinutes((page * -1) * 60);

            var havesterNodes = document.DocumentNode.SelectNodes("//div[@class='tagcat2-tinmoi' or @class='cat-row' or @class='cat-top']");

            if (havesterNodes != null)
            {
                foreach (HtmlNode node in havesterNodes)
                {
                    var havestedLinks = node.Descendants("a").ToList();

                    if (havestedLinks.Count > 0)
                    {
                        foreach (var lnk in havestedLinks)
                        {
                            href = lnk.Attributes["href"].Value;
                            if (href.Contains("html"))
                            {
                                if (!IsContains(results, href))
                                {
                                    results.Add(new HarvestLink(href, hrefDate, link));
                                }
                            }
                        }
                    }
                }
            }

            return results;
        }

        public int PageNumber(string link)
        {
            string[] content = link.Split('/');

            if (Regex.IsMatch(content.Last(), "p([0-9]|[1-9][0-9]|[1-9][0-9][0-9])$"))
            {
                string value = content.Last().Replace("p","");

                return Convert.ToInt32(value) - 1;
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
