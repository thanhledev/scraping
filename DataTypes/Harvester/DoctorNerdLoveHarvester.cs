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
    public class DoctorNerdLoveHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = new DateTime();

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='hfeed']");

                if (mainContent != null)
                {
                    var articles = (from artl in mainContent.Descendants()
                                    where artl.Name == "div" && artl.Attributes["class"] != null && artl.Attributes["class"].Value.Contains("type-post status-publish")
                                    select artl).ToList();

                    if (articles.Count > 0)
                    {
                        foreach (var article in articles)
                        {
                            href = article.Descendants("h2").First().Descendants("a").First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;

                            var date = (from span in article.Descendants()
                                        where span.Name == "span" && span.Attributes["class"] != null && span.Attributes["class"].Value == "date published time"
                                        select span).First().Attributes["title"].Value;

                            date = date.Replace("T", " ").Replace("+00:00", "").Trim();

                            if (!IsContains(result, href))
                                result.Add(new HarvestLink(href, DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", null), link));
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
