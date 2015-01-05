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
    public class BongDaLivesHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate;
            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='module s2']");

                if (mainContent != null)
                {
                    var contents = mainContent.SelectNodes("//div[@class='box-border-shadow m-bottom listz-news']").ToList();

                    if (contents.Count > 0)
                    {
                        foreach (var content in contents)
                        {
                            //find article link in content
                            var contentLinks = content.Descendants("a");

                            if (contentLinks != null)
                            {
                                foreach (var lnk in contentLinks)
                                {
                                    if (lnk.Attributes["class"] != null)
                                    {
                                        if (lnk.Attributes["class"].Value == "more")
                                        {
                                            href = lnk.Attributes["href"].Value;

                                            if (!href.Contains("http"))
                                                href = host + href;                                            
                                        }
                                    }
                                }
                            }

                            //find article post date in content

                            var contentDate = content.SelectSingleNode("div[@class='info small']");

                            if (contentDate != null)
                            {
                                var contenDateValue = contentDate.InnerText.Trim();

                                string[] values = contenDateValue.Split('|');

                                values[0] = values[0].Trim();
                                values[0] = values[0].Replace("Đăng lúc: ", "");
                                values[0] = values[0].Replace('-', '/');

                                hrefDate = DateTime.ParseExact(values[0], "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                hrefDate = DateTime.Now;
                            }

                            HarvestLink newItem = new HarvestLink(href, hrefDate, link);

                            result.Add(newItem);
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
    }
}
