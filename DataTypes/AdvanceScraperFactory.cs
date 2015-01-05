using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Text.RegularExpressions;
using DataTypes.Enums;

namespace DataTypes
{
    public static class AdvanceScraperFactory
    {
        public static IAdvance GetAdvanceScraper(string url)
        {
            if (url.Contains("ibongda.vn"))
                return new IBongdaAdvanceScraper();
            else if (url.Contains("giovangchotso.net"))
                return new GioVangChotSoAdvanceScraper();
            else
                return new NullAdvanceScraper();
        }
    }
}
