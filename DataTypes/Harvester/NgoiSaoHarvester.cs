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
    public class NgoiSaoHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();

            string href = "";
            DateTime hrefDate = DateTime.Now;

            if (document.DocumentNode.InnerText != string.Empty)
            {
                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='cate-items']");

                if (mainContent != null)
                {
                    var artilces = (from post in mainContent.Descendants()
                                    where post.Name == "div" && post.Attributes["class"] != null && post.Attributes["class"].Value.Contains("cate-item")
                                    select post).ToList();
                    if (artilces.Count > 0)
                    {
                        foreach (var article in artilces)
                        {
                            href = article.Descendants("a").Where(a=>a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("thumb")).First().Attributes["href"].Value;

                            if (!href.Contains("http"))
                                href = host + "/" + href;

                            var date = article.Descendants("span").Where(a => a.Attributes["class"] != null && a.Attributes["class"].Value.Contains("timer")).First().InnerText;                            

                            result.Add(new HarvestLink(href, DateTime.ParseExact(date.Trim(), "dd/MM/yyyy HH:mm", null), link));
                        }
                    }
                }
            }

            return result;
        }

        public int PageNumber(string link)
        {
            if (link.Contains("trang"))
            {
                string[] content = link.Split('/');

                return Convert.ToInt32(content[content.Length - 2]);
            }
            else
                return 1;
        }
    }
}
