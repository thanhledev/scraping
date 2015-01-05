using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;

namespace DataTypes.Interfaces
{
    public interface IScraper
    {
        string GetTitleFor(HtmlDocument document, ArticleType type, string url);
        DateTime GetPublish(HtmlDocument document, ArticleType type);
        string GetShampoo(HtmlDocument document, ArticleType type);
        string GetBody(HtmlDocument document,ArticleType type, string url);
        string GetImage(HtmlDocument document, ArticleType type, string url);
        string GetHtmlBody(HtmlDocument document, ArticleType type, string url);
        string GetSEOTitle(HtmlDocument document, ArticleType type);
        string GetSEODescription(HtmlDocument document, ArticleType type);
        List<string> GetTags(HtmlDocument document, ArticleType type);
        List<string> GetSentences(HtmlDocument document, ArticleType type);
    }
}
