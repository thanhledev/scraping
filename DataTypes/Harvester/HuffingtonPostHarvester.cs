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
    public class HuffingtonPostHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = DateTime.Now.AddMinutes((page * -1) * 60);

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var centerContent = document.DocumentNode.SelectSingleNode("//div[@id='center_column']");

                if (centerContent != null)
                {
                    var articleHrefs = centerContent.Descendants("h4").ToList();

                    if (articleHrefs.Count > 0)
                    {
                        foreach (var i in articleHrefs)
                        {
                            href = i.Descendants("a").First().Attributes["href"].Value;

                            if (!IsContains(result, href) && href.Contains("huffingtonpost.com"))
                                result.Add(new HarvestLink(href, hrefDate, link));
                        }
                    }
                }

                var rightContent = document.DocumentNode.SelectSingleNode("//div[@id='right_column_entries']");

                if (rightContent != null)
                {
                    var articleHrefs = rightContent.Descendants("h4").ToList();

                    if (articleHrefs.Count > 0)
                    {
                        foreach (var i in articleHrefs)
                        {
                            href = i.Descendants("a").First().Attributes["href"].Value;

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
