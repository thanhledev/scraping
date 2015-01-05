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
    public class TheGuardianHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = new DateTime();

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//ul[@id='auto-trail-block']");

                if (mainContent != null)
                {
                    var contents = mainContent.Descendants("li").ToList();

                    if (contents.Count > 0)
                    {
                        foreach (var content in contents)
                        {
                            href = content.Descendants("h3").First().Descendants("a").First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;

                            var date = (from span in content.Descendants()
                                        where span.Name == "span" && span.Attributes["class"] != null && span.Attributes["class"].Value == "date"
                                        select span).First().InnerText.Replace(":", "").Trim();

                            if (!IsContains(result, href))
                                result.Add(new HarvestLink(href, DateTime.ParseExact(date, "d MMM yyyy", null), link));
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
                string[] content = link.Split('?');

                string value = content[1].Replace("page=", "").Trim();

                return Convert.ToInt32(value);
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
