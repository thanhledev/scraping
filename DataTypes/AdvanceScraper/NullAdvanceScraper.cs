using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;

namespace DataTypes
{
    public class NullAdvanceScraper : IAdvance
    {
        public string GetImageSearchPhrase(HtmlDocument document, WebProxy proxy, List<string> agents)
        {
            return string.Empty;
        }

        public string GetImageSearchPhrase(HtmlDocument document, List<string> agents)
        {
            return string.Empty;
        }
    }
}
