using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Text.RegularExpressions;
using DataTypes.Enums;
using System.IO;
using System.Web;

namespace DataTypes
{
    public class NotanH1Scraper : IProxyScraper
    {
        public List<SystemProxy> GetProxies(HtmlDocument document, string url)
        {
            List<SystemProxy> proxies = new List<SystemProxy>();

            var tds = document.DocumentNode.Descendants("td");

            if (tds != null)
            {
                foreach (var cell in tds)
                {
                    if (cell.Attributes["class"] != null)
                    {
                        if (cell.Attributes["class"].Value == "name")
                        {
                            string proxy = cell.InnerText.Trim();

                            string[] args = proxy.Split(':');

                            SystemProxy newItem = new SystemProxy(args[0], args[1], "", "");

                            proxies.Add(newItem);                            
                        }
                    }
                }
            }

            return proxies;
        }
    }
}
