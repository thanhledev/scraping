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
    public class YoastPlugin : ISEOPlugin
    {
        private static string _titleMetaKey = "_yoast_wpseo_title";
        private static string _descMetaKey = "_yoast_wpseo_metadesc";
        private static string _keywordsMetaKey = "_yoast_wpseo_metakeywords";
        private static string _focusKwMetaKey = "_yoast_wpseo_focuskw";

        private string yoastSEOTitle;
        private string yoastSEODescription;
        private string yoastSEOKeywordsMeta;
        private string focusKwValue;
        private List<string> keywords = new List<string>();
        Random rnd = new Random();

        public void SetupSEOFactors(string title, string desc, string metaKeywords, string addition, List<string> chosenKeywords)
        {
            yoastSEOTitle = title;
            yoastSEODescription = desc;
            yoastSEOKeywordsMeta = metaKeywords;
            focusKwValue = addition;
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
                        Value = keywords[rnd.Next(keywords.Count)] + " " + yoastSEOTitle
                    },
                    new CustomField()
                    {
                        Key = _descMetaKey,
                        Value = keywords[rnd.Next(keywords.Count)] + " " + yoastSEODescription
                    },                
                    new CustomField()
                    {
                        Key = _focusKwMetaKey,
                        Value = keywords[rnd.Next(keywords.Count)]
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
                        Value = yoastSEOTitle
                    },
                    new CustomField()
                    {
                        Key = _descMetaKey,
                        Value = yoastSEODescription
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
                if (field.Key.Contains(_titleMetaKey) || field.Key.Contains(_descMetaKey) || field.Key.Contains(_keywordsMetaKey) || field.Key.Contains(_focusKwMetaKey))
                {
                    cfs.Add(new CustomField() { Key = field.Key, Value = ReplaceAllAvailablePosition(chosenKeywords, field.Value, keywordSignature, projName) });
                }
            }

            return cfs.ToArray();
        }

        private string ReplaceAllAvailablePosition(List<string> keywords, string document, string keywordTag, string projName)
        {
            string returnValue = document;

            if (keywords.Count > 0)
            {
                while (true)
                {
                    int pos = returnValue.IndexOf(keywordTag);
                    if (pos < 0)
                        break;

                    string chosenKeyword = keywords[rnd.Next(keywords.Count)];
                    returnValue = returnValue.Substring(0, pos) + chosenKeyword + returnValue.Substring(pos + keywordTag.Length);
                }
            }
            else
            {
                string a = projName;
            }

            return returnValue;
        }
    }
}
