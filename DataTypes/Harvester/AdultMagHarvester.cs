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
    public class AdultMagHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate;

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='masonry-wrapper']");

                if (mainContent != null)
                {
                    var articles = (from artl in mainContent.Descendants()
                                    where artl.Name == "article"
                                    select artl).ToList();
                    if (articles.Count > 0)
                    {
                        foreach (var article in articles)
                        {
                            href = article.Descendants("h2").First().Descendants("a").First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;

                            var date = (from div in article.Descendants()
                                        where div.Name == "div" && div.Attributes["class"] != null && div.Attributes["class"].Value == "meta"
                                        select div).First().InnerText.Trim();

                            string[] content = date.Split(' ');

                            content[1] = content[1].Replace(",", "").Replace("TH", "").Replace("ND", "").Replace("ST", "").Replace("RD", "").Replace("th", "").Replace("nd", "").Replace("st", "").Replace("rd", "").Trim();

                            date = content[0] + " " + content[1] + ", " + content[2];

                            result.Add(new HarvestLink(href, DateTime.ParseExact(date, "MMMM d, yyyy", null), link));
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

                return Convert.ToInt32(content[content.Length - 2]);
            }
            else
                return 1;
        }
    }
}
