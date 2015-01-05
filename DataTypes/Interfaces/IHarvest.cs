using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Collections;

namespace DataTypes.Interfaces
{
    public interface IHarvest
    {
        List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page);
        int PageNumber(string link);
    }
}
