using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;
using HtmlAgilityPack;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Text.RegularExpressions;
using DataTypes.Enums;
using System.IO;
using System.Web;

namespace DataTypes
{
    public class NullPlugin : ISEOPlugin
    {
        public void SetupSEOFactors(string title, string desc, string metaKeywords, string addition, List<string> chosenKeywords)
        {
            return;
        }

        public CustomField[] createSEOFactors()
        {
            return null;
        }

        public CustomField[] remakeSEOFactors(CustomField[] inputFactors, string keywordSignature, List<string> chosenKeywords, string projName)
        {
            return null;
        }
    }
}
