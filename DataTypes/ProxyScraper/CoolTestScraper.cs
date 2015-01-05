﻿using System;
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
    public class CoolTestScraper : IProxyScraper
    {
        public List<SystemProxy> GetProxies(HtmlDocument document, string url)
        {
            List<SystemProxy> proxies = new List<SystemProxy>();

            var mainContent = document.DocumentNode.SelectSingleNode("//pre");

            string value = mainContent.InnerText;

            string proxyPattern = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b:\d{2,5}";

            MatchCollection collection = Regex.Matches(value, proxyPattern);            
            foreach (Match match in collection)
            {
                if (match.Success)
                {
                    string proxy = match.Groups[0].Value.Trim();

                    string[] args = proxy.Split(':');

                    SystemProxy newItem = new SystemProxy(args[0], args[1], "", "");

                    proxies.Add(newItem);
                }
            }

            return proxies;
        }
    }
}
