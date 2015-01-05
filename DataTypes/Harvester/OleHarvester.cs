using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Interfaces;
using DataTypes.Collections;

namespace DataTypes
{
    public class OleHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();
            string href = "";
            DateTime hrefDate;

            if (document.DocumentNode.InnerText != null)
            {
                var leadingContent = document.DocumentNode.SelectSingleNode("//div[@class='c-item-leading']");

                if (leadingContent != null)
                {
                    var leadingContentLinks = leadingContent.Descendants("a").ToList();

                    if (leadingContentLinks.Count > 0)
                    {
                        href = leadingContentLinks[0].Attributes["href"].Value;

                        if (!href.Contains("http"))
                            href = host + href;                        
                    }

                    var leadingContentDate = leadingContent.SelectSingleNode("//div[@class='item-created']");

                    if (leadingContentDate != null)
                    {
                        var dateString = leadingContentDate.InnerText.Replace(", Ngày", "");

                        dateString = dateString.Replace('h', ':');

                        hrefDate = DateTime.ParseExact(dateString, "HH:mm dd/MM/yyyy", null);
                    }
                    else
                        hrefDate = DateTime.Now;

                    result.Add(new HarvestLink(href, hrefDate, link));
                }

                var itemPrimaryMainContent = document.DocumentNode.SelectSingleNode("//div[@class='c-item-primary']");

                if (itemPrimaryMainContent != null)
                {
                    var itemPrimaryContents = itemPrimaryMainContent.SelectNodes("//div[@class='uk-width-small-1-4 item-content']").ToList();

                    if (itemPrimaryContents.Count > 0)
                    {
                        foreach (var content in itemPrimaryContents)
                        {
                            href = content.Descendants("a").ToList().First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;

                            var contentDate = content.SelectSingleNode("//div[@class='item-created']");

                            if (contentDate != null)
                            {
                                var dateString = contentDate.InnerText.Replace(", Ngày", "");

                                dateString = dateString.Replace('h', ':');

                                hrefDate = DateTime.ParseExact(dateString, "HH:mm dd/MM/yyyy", null);
                            }
                            else
                                hrefDate = DateTime.Now;

                            result.Add(new HarvestLink(href, hrefDate, link));
                        }
                    }
                }

                var itemSecondaryMainContent = document.DocumentNode.SelectSingleNode("//div[@class='c-item-secondary']");

                if (itemSecondaryMainContent != null)
                {
                    var itemSecondaryContents = itemSecondaryMainContent.SelectNodes("//div[@class='uk-width-small-1-1 item-content']").ToList();

                    if (itemSecondaryContents.Count > 0)
                    {
                        foreach (var content in itemSecondaryContents)
                        {
                            href = content.Descendants("a").ToList().First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;

                            var contentDate = content.SelectSingleNode("//div[@class='item-created']");

                            if (contentDate != null)
                            {
                                var dateString = contentDate.InnerText.Replace(", Ngày", "");

                                dateString = dateString.Replace('h', ':');

                                hrefDate = DateTime.ParseExact(dateString, "HH:mm dd/MM/yyyy", null);
                            }
                            else
                                hrefDate = DateTime.Now;

                            result.Add(new HarvestLink(href, hrefDate, link));
                        }
                    }
                }
            }
            return result;
        }

        public int PageNumber(string link)
        {
            if (link.Contains("start"))
            {
                string[] content = link.Split('?');

                int page = Convert.ToInt32(content[1].Replace("start=", ""));

                return page / 30;
            }
            else
                return 0;
        }
    }
}
