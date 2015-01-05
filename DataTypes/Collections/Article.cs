using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
namespace DataTypes.Collections
{
    public class Article
    {
        public string url { get; set; }
        public string harvestUrl { get; set; }
        public string title { get; set; }
        public DateTime downloaded { get; set; }
        public DateTime publish { get; set; }
        public string body { get; set; }
        public string image { get; set; }
        public HtmlDocument doc { get; set; }
        public ArticleType type { get; set; }
        public string shampoo { get; set; }
        public string htmlBody { get; set; }
        public bool isPosted { get; set; }
        public string SEOTitle { get; set; }
        public string SEODescription { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Sentences { get; set; }

        public Article(string link, string hUrl, HtmlDocument document,ArticleType Type)
        {
            url = link;
            hUrl = harvestUrl;
            doc = document;
            type = Type;
            isPosted = false;
        }

        public Article()
        {
            url = string.Empty;
        }

        public Article Clone()
        {
            Article temp = new Article();

            temp.url = this.url;
            temp.harvestUrl = this.harvestUrl;
            temp.title = this.title;
            temp.downloaded = this.downloaded;
            temp.publish = this.publish;
            temp.body = this.body;
            temp.image = this.image;
            temp.doc = this.doc;
            temp.type = this.type;
            temp.shampoo = this.shampoo;
            temp.htmlBody = this.htmlBody;
            temp.isPosted = this.isPosted;
            temp.SEOTitle = this.SEOTitle;
            temp.SEODescription = this.SEODescription;
            temp.Tags = this.Tags;
            temp.Sentences = this.Sentences;

            return temp;
        }
    }
}
