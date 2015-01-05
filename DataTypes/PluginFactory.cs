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
    public static class PluginFactory
    {
        public static ISEOPlugin CreatePlugin(SEOPluginType? type)
        {
            if (type != null)
            {
                switch (type)
                {
                    case SEOPluginType.AllInOneSEO:
                        return new AllInOneSEO();
                    case SEOPluginType.YoastSEO:
                        return new YoastPlugin();
                    default:
                        return new NullPlugin();
                };
            }
            return new NullPlugin();
        }
    }
}
