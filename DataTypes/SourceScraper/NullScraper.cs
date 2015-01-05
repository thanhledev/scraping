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
    public class NullScraper : IScraper
    {
        public string GetTitleFor(HtmlDocument document, ArticleType type, string url)
        {
            return null;
        }

        public DateTime GetPublish(HtmlDocument document, ArticleType type)
        {
            return DateTime.Now;
        }

        public string GetBody(HtmlDocument document, ArticleType type, string url)
        {
            return null;
        }

        public string GetShampoo(HtmlDocument document, ArticleType type)
        {
            return null;
        }

        public string GetImage(HtmlDocument document, ArticleType type, string url)
        {
            return null;
        }

        public string GetHtmlBody(HtmlDocument document, ArticleType type, string url)
        {
            return null;
        }

        public string GetSEOTitle(HtmlDocument document, ArticleType type)
        {
            return null;
        }

        public string GetSEODescription(HtmlDocument document, ArticleType type)
        {
            return null;
        }

        public List<string> GetTags(HtmlDocument document, ArticleType type)
        {
            return null;
        }

        public List<string> GetSentences(HtmlDocument document, ArticleType type)
        {
            return null;
        }
    }
}
