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
    public class TapChiDanOngHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = DateTime.Now;

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='CategoryContent']");

                if (mainContent != null)
                {
                    var artilces = (from post in mainContent.Descendants()
                                    where post.Name == "li" && post.Attributes["class"] != null && post.Attributes["class"].Value.Contains("ListView")
                                    select post).ToList();
                    if (artilces.Count > 0)
                    {
                        foreach (var article in artilces)
                        {
                            href = article.Descendants("a").First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;                           

                            result.Add(new HarvestLink(href, hrefDate, link));
                        }
                    }
                }
            }

            return result;
        }

        public int PageNumber(string link)
        {
            if (link.Contains("page"))
            {
                string[] content = link.Split('/');

                return Convert.ToInt32(content[content.Length-1].Replace("?sort=newest&page","").Trim());
            }
            else
                return 1;
        }
    }
}
