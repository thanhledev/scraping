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
    public interface IVideo
    {
        void InsertVideo(AuthorityKeywords tagType, ref string htmlBody, List<string> articleTags, List<string> manualTags, WebProxy proxy, List<string> agents);
        void InsertVideo(AuthorityKeywords tagType, ref string htmlBody, List<string> articleTags, List<string> manualTags, List<string> agents);
    }
}
