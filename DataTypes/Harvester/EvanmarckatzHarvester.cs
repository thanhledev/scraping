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
    public class EvanmarckatzHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = DateTime.Now.AddMinutes((page * -1) * 60);

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//section[@id='primary']");

                if (mainContent != null)
                {
                    var articles = (from artl in mainContent.Descendants()
                                where artl.Name == "article" && artl.Attributes["id"] != null && artl.Attributes["id"].Value.Contains("post-")
                                select artl).ToList();

                    if (articles.Count > 0)
                    {
                        foreach (var article in articles)
                        {
                            href = article.Descendants("h2").First().Descendants("a").First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;

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
            if (link.Contains("page"))
            {
                string[] content = link.Split('/');

                return Convert.ToInt32(content[content.Length - 2]);
            }
            else
                return 1;
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
