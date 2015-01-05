using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Text.RegularExpressions;
using DataTypes.Enums;
using System.IO;
using System.Web;

namespace DataTypes
{
    public class VnexpressSpawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            List<string> result = new List<string>();
            List<string> temp = new List<string>();

            ReverseCategories(ref temp, source.Url, null, source);

            foreach (var i in temp)
            {
                if (!i.Contains("photo"))
                {
                    for (int j = 1; j <= depth; j++)
                    {
                        result.Add(i + "/page/" + j + ".html");
                    }
                }
                else
                    result.Add(i);
            }

            return result;
        }

        private void ReverseCategories(ref List<string> stored, string pattern, SourceCategory parent, ArticleSource source)
        {
            foreach (var cate in source.Categories)
            {
                if (cate.ParentCategory == parent)
                {
                    if (cate.Slug == "tin-tuc")
                    {
                        string value = pattern + "/" + cate.Slug;
                        ReverseCategories(ref stored, value, cate, source);
                    }
                    else
                    {
                        string value = pattern + "/" + cate.Slug;
                        stored.Add(value);
                        ReverseCategories(ref stored, value, cate, source);
                    }
                }
            }
        }

        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {
            List<string> result = new List<string>();
            string pattern = chosenCategories.Slug;

            ReverseAgainstCategories(ref pattern, chosenCategories, source);

            pattern = "http://" + chosenCategories.SourceHost + "/" + pattern;

            for (int j = 1; j <= depth; j++)
                result.Add(pattern + "/page/" + j + ".html");

            foreach (var k in result)
                searchPages.Add(k);
        }

        private void ReverseAgainstCategories(ref string pattern, SourceCategory leafCategory, ArticleSource source)
        {
            foreach (var cate in source.Categories)
            {
                if (cate == leafCategory.ParentCategory)
                {
                    pattern = cate.Slug + "/" + pattern;
                    ReverseAgainstCategories(ref pattern, cate, source);
                }
            }
        }
    }
}
