using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Collections;
using DataTypes.Enums;

namespace DataTypes.Interfaces
{
    public interface IProxyScraper
    {
        List<SystemProxy> GetProxies(HtmlDocument document, string url);
    }
}
