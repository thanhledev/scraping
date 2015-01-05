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
    public class NullVideoInserter : IVideo
    {
        public void InsertVideo(AuthorityKeywords tagType, ref string htmlBody, List<string> articleTags, List<string> manualTags, WebProxy proxy, List<string> agents)
        {
            //never be implemented
        }

        public void InsertVideo(AuthorityKeywords tagType, ref string htmlBody, List<string> articleTags, List<string> manualTags, List<string> agents)
        {
            //never be implemented
        }
    }
}
