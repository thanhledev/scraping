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
    public class CelebromanceComHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = new DateTime();

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='span12 column_container']");

                if (mainContent != null)
                {
                    var contents = mainContent.SelectNodes("//div[@class='span4']");

                    if (contents.Count > 0)
                    {
                        foreach (var content in contents)
                        {
                            //get content hrefs
                            var contentHrefs = content.Descendants("a").ToList();

                            if (contentHrefs.Count > 0)
                            {
                                foreach (var i in contentHrefs)
                                {
                                    var value = i.Attributes["href"].Value;

                                    if (!IsContains(result,value))
                                    {
                                        href = value;
                                        break;
                                    }
                                }
                            }

                            //get content datetime
                            var contentInfo = content.Descendants("time").First().Attributes["datetime"].Value;

                            if (contentInfo != string.Empty)
                            {
                                contentInfo = contentInfo.Replace("T", " ").Trim().Replace("+00:00", "").Replace('-', '/').Trim();

                                hrefDate = DateTime.ParseExact(contentInfo, "yyyy/MM/dd HH:mm:ss", null);
                            }

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
