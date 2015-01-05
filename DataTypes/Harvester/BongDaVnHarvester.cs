using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Net;

namespace DataTypes
{
    public class BongDaVnHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> results = new List<HarvestLink>();
            string href = "";
            DateTime hrefDate = DateTime.Now;

            if (!link.Contains("Thu-Vien-Anh"))
            {
                var harvesterNodes = document.DocumentNode.SelectNodes("//*[(self::div[@id='ctl00_BD_sResult'] or self::table[@class='article'])]");

                if (harvesterNodes != null)
                {
                    foreach (var node in harvesterNodes)
                    {
                        //get rows
                        var rows = node.Descendants("tr").ToList();

                        if (rows.Count > 0)
                        {
                            foreach (var row in rows)
                            {
                                //get article link
                                var articleLink = row.Descendants("a").ToList();

                                if (articleLink.Count > 0)
                                {
                                    foreach (var lnk in articleLink)
                                    {
                                        if (lnk.Attributes["class"] != null)
                                        {
                                            if (lnk.Attributes["class"].Value == "read_more")
                                            {
                                                href = lnk.Attributes["href"].Value;

                                                if (!href.Contains("http"))
                                                    href = host + href;
                                            }
                                        }
                                    }
                                }

                                //get article date
                                var articleDate = row.SelectSingleNode("//sup[@class='date']");

                                if (articleDate != null)
                                {
                                    hrefDate = DateTime.ParseExact(articleDate.InnerText, "H:m d/M/yyyy", null);
                                }
                                else
                                {
                                    hrefDate = DateTime.Now;
                                }

                                if (!IsContains(results, href))
                                {
                                    HarvestLink newItem = new HarvestLink(href, hrefDate, link);

                                    results.Add(newItem);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//table[@id='ctl00_BD_photoNew_lsPhotoView']");

                var tables = mainContent.Descendants("table").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("letsop-photo-item")).ToList();

                if (tables.Count > 0)
                {
                    foreach(var table in tables)
                    {
                        var anchors = table.Descendants("a").ToList();

                        if (anchors.Count > 0)
                        {
                            href = anchors.First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + href;                            
                        }

                        var publishedDate = table.Descendants("span").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("Font_capnhat")).ToList();

                        if (publishedDate.Count > 0)
                        {
                            foreach (var item in publishedDate)
                            {
                                if (item.InnerText.Contains("/"))
                                {
                                    var articleDate = item.InnerText;

                                    hrefDate = DateTime.ParseExact(articleDate, "dd/MM/yyyy", null);

                                    break;
                                }
                            }
                        }

                        if (!IsContains(results, href))
                            results.Add(new HarvestLink(href, hrefDate, link));
                    }
                }                
            }

            return results;
            //var anchors = tableTR.Descendants("a").ToList();

            //if (anchors.Count > 0)
            //{
            //    href = anchors.First().Attributes["href"].Value;

            //    if(!href.Contains("http"))
            //        href = host + href;
            //}

            //var publishedDate = tableTD.Descendants("span").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("Font_capnhat")).ToList();

            //if (publishedDate.Count > 0)
            //{
            //    foreach (var item in publishedDate)
            //    {
            //        if (item.InnerText.Contains("/"))
            //        {
            //            var articleDate = item.InnerText;

            //            hrefDate = DateTime.ParseExact(articleDate, "dd/MM/yyyy", null);
            //        }
            //    }
            //}

            //if (!IsContains(results, href))
            //    results.Add(new HarvestLink(href, hrefDate, link));
        }

        public int PageNumber(string link)
        {
            if (link.Contains("?Page_ID"))
            {
                string[] content = link.Split('?');

                int page = Convert.ToInt32(content[1].Replace("Page_ID=", ""));

                if (page == 1)
                    return 0;
                else
                    return page - 1;
            }
            else
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
