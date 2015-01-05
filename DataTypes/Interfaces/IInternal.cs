using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Collections;
using System.Net;
using JoeBlogs;
namespace DataTypes.Interfaces
{
    public interface IInternal
    {
        void InsertInternalLink(AuthorityKeywords tagType, ref string htmlBody, List<string> articleTags, List<string> manualTags, WordPressWrapper wordpress, List<string> agents);
    }
}
