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
    public class AllInOneSEO : ISEOPlugin
    {
        private static string _titleMetaKey = "_aioseop_title";
        private static string _descMetaKey = "_aioseop_description";
        private static string _keywordsMetaKey = "_aioseop_keywords";

        private string allInOneSEOTitle;
        private string allInOneSEODescription;
        private string allInOneSEOKeywordMeta;
        private List<string> keywords = new List<string>();
        Random rnd = new Random();

        public void SetupSEOFactors(string title, string desc, string metaKeywords, string addition, List<string> chosenKeywords)
        {
            allInOneSEOTitle = title;
            allInOneSEODescription = desc;
            allInOneSEOKeywordMeta = metaKeywords;
            keywords = chosenKeywords;
        }

        public CustomField[] createSEOFactors()
        {
            if (keywords.Count > 0)
            {
                var cfs = new CustomField[]
                {
                    new CustomField()
                    {
                        Key = _titleMetaKey,
                        Value = keywords[rnd.Next(keywords.Count)] + " - " + allInOneSEOTitle
                    },
                    new CustomField()
                    {
                        Key = _descMetaKey,
                        Value = keywords[rnd.Next(keywords.Count)] + " - " + allInOneSEODescription
                    },
                    new CustomField()
                    {
                        Key = _keywordsMetaKey,
                        Value = keywords[rnd.Next(keywords.Count)] + " - " + allInOneSEOKeywordMeta
                    }               
                };

                return cfs;
            }
            else
            {
                var cfs = new CustomField[]
                {
                    new CustomField()
                    {
                        Key = _titleMetaKey,
                        Value = allInOneSEOTitle
                    },
                    new CustomField()
                    {
                        Key = _descMetaKey,
                        Value = allInOneSEODescription
                    }                                  
                };

                return cfs;
            }
        }

        public CustomField[] remakeSEOFactors(CustomField[] inputFactors, string keywordSignature, List<string> chosenKeywords, string projName)
        {
            List<CustomField> cfs = new List<CustomField>();

            foreach (var field in inputFactors)
            {
                if (field.Key.Contains(_titleMetaKey) || field.Key.Contains(_descMetaKey) || field.Key.Contains(_keywordsMetaKey))
                {
                    cfs.Add(new CustomField() { Key = field.Key, Value = ReplaceAllAvailablePosition(chosenKeywords, field.Value, keywordSignature) });
                }
            }

            return cfs.ToArray();
        }

        private string ReplaceAllAvailablePosition(List<string> keywords, string document, string keywordTag)
        {
            string returnValue = document;

            while (true)
            {
                int pos = document.IndexOf(keywordTag);
                if (pos < 0)
                    break;

                string chosenKeyword = keywords[rnd.Next(keywords.Count)];
                returnValue = returnValue.Substring(0, pos) + chosenKeyword + returnValue.Substring(pos + keywordTag.Length);
            }

            return returnValue;
        }
    }
}
