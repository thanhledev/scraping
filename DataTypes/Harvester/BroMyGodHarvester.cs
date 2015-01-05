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
    public class BroMyGodHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate;

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='main-content']");

                if (mainContent != null)
                {
                    var artilces = (from post in mainContent.Descendants()
                                    where post.Name == "post" && post.Attributes["class"] != null && post.Attributes["class"].Value.Contains("post type-post status-publish")
                                    select post).ToList();
                    if (artilces.Count > 0)
                    {
                        foreach (var article in artilces)
                        {
                            href = article.Descendants("h2").First().Descendants("a").First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;

                            var date = article.Descendants("div").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("post-meta")).First().InnerText;

                            string[] content = date.Split('/');

                            result.Add(new HarvestLink(href, DateTime.ParseExact(content[1].Trim(), "d MMMM yyyy", null), link));
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
