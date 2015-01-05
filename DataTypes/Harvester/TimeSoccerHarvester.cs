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
    public class TimeSoccerHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = DateTime.Now;

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='span12 column_container']");

                if (mainContent != null)
                {
                    var articles = (from post in mainContent.Descendants()
                                    where post.Name == "div" && post.Attributes["class"] != null && post.Attributes["class"].Value.Contains("td_mod6 td_mod_wrap")
                                    select post).ToList();
                    if (articles.Count > 0)
                    {
                        foreach (var article in articles)
                        {
                            var header3 = article.Descendants("h3").ToList();

                            if (header3.Count > 0)
                                href = header3.First().Descendants("a").First().Attributes["href"].Value;

                            var datePublished = article.Descendants("time").First().Attributes["datetime"].Value;

                            datePublished = datePublished.Replace("T", " ").Replace("+00:00", "").Trim();

                            hrefDate = DateTime.ParseExact(datePublished, "yyyy-MM-dd HH:mm:ss", null);

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
