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
    public class DantriHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();
            string href = "";
            DateTime hrefDate = DateTime.Now.AddMinutes((page * -1) * 60);

            var links = document.DocumentNode.Descendants("a").ToList();

            if (links != null)
            {
                foreach (var lnk in links)
                {
                    if (lnk.Attributes["class"] != null)
                    {
                        if (lnk.Attributes["class"].Value == "fon44" || lnk.Attributes["class"].Value == "fon6")
                        {
                            href = lnk.Attributes["href"].Value;

                            if (!IsContains(result,href))
                            {
                                if (!href.Contains("http"))
                                    href = host + href;                                

                                result.Add(new HarvestLink(href,hrefDate, link));
                            }
                        }
                    }
                }
            }

            return result;
        }

        public int PageNumber(string link)
        {
            if (link.Contains("trang-"))
            {
                string[] content = link.Split('/');

                string value = content.Last().Replace(".htm", "");

                int page = Convert.ToInt32(value.Replace("trang-", ""));

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
