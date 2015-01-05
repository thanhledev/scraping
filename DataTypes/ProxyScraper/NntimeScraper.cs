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
    public class NntimeScraper : IProxyScraper
    {
        public List<SystemProxy> GetProxies(HtmlDocument document, string url)
        {
            List<SystemProxy> proxies = new List<SystemProxy>();

            var mainContent = document.DocumentNode.SelectSingleNode("//table[@id='proxylist']");

            //extract port code
            var jsCodes = document.DocumentNode.SelectNodes("//script");
            var portCode = "";

            foreach (var code in jsCodes)
            {
                if (code.InnerText.Contains("o=") || code.InnerText.Contains("h=") || code.InnerText.Contains("l="))
                {
                    portCode = code.InnerText.Trim();
                    break;
                }
            }

            List<PortValueEncrypt> portValues = new List<PortValueEncrypt>();

            LoadEncrypt(portCode, ref portValues);

            var rows = mainContent.Descendants("tr");

            string cellPortCode = "";
            string ipAddress = "";

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    if (row.Attributes["class"] != null)
                    {
                        if (row.Attributes["class"].Value == "odd" || row.Attributes["class"].Value == "even")
                        {
                            var cells = row.Descendants("td").ToList();

                            if (cells != null)
                            {
                                for (int i = 0; i < cells.Count; i++)
                                {
                                    if (i == 1)
                                    {
                                        string rawValue = cells[i].InnerHtml;
                                        ipAddress = HtmlSanitizer.StripHtml(rawValue);
                                        cellPortCode = cells[i].InnerText.Replace(ipAddress, "").Replace("document.write(\":\"+", "").Replace(")", "").Trim();

                                        SystemProxy newItem = new SystemProxy(ipAddress, GetPort(cellPortCode, portValues), "", "");
                                        proxies.Add(newItem);                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return proxies;
        }

        private void LoadEncrypt(string code, ref List<PortValueEncrypt> portValues)
        {
            string[] args = code.Split(';');

            for (int i = 0; i < args.Count(); i++)
            {
                if (args[i] != string.Empty)
                {
                    string[] argValue = args[i].Split('=');

                    PortValueEncrypt newItem = new PortValueEncrypt(argValue[0], argValue[1]);

                    portValues.Add(newItem);
                }
            }
        }

        private string GetValueFromEncrypt(string signal, List<PortValueEncrypt> portValues)
        {
            return portValues.Where(a => a.portSignal == signal).First().portValue;
        }

        private string GetPort(string code, List<PortValueEncrypt> portValues)
        {
            string result = "";
            string[] args = code.Split('+');

            for (int i = 0; i < args.Count(); i++)
            {
                result += GetValueFromEncrypt(args[i], portValues);
            }

            return result;
        }
    }
}
