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
    public class VietGiaiTriHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = DateTime.Now;

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='content-block block-left']");

                if (mainContent != null)
                {
                    var artilces = (from post in mainContent.Descendants()
                                    where post.Name == "div" && post.Attributes["class"] != null && post.Attributes["class"].Value.Contains("post-content-archive") && post.Attributes["id"] != null && post.Attributes["id"].Value.Contains("post-")
                                    select post).ToList();
                    if (artilces.Count > 0)
                    {
                        foreach (var article in artilces)
                        {
                            var header2 = article.Descendants("h2").ToList();

                            if (header2.Count > 0)                            
                                href = header2.First().Descendants("a").First().Attributes["href"].Value;                            
                            else
                            {
                                var header3 = article.Descendants("h3").ToList();

                                if (header3.Count > 0)
                                    href = header3.First().Descendants("a").First().Attributes["href"].Value;
                            }

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

                return Convert.ToInt32(content[content.Length - 1]);
            }
            else
                return 1;
        }
    }
}
