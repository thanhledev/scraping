using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Collections;
using System.Net;

namespace DataTypes.Interfaces
{
    public interface IAdvance
    {
        string GetImageSearchPhrase(HtmlDocument document, WebProxy proxy, List<string> agents);
        string GetImageSearchPhrase(HtmlDocument document, List<string> agents);
    }
}
