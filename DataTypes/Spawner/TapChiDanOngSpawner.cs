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
    public class TapChiDanOngSpawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            List<string> result = new List<string>();
            List<string> temp = new List<string>();

            ReverseCategories(ref temp, source.Url + "/categories", null, source);

            foreach (var i in temp)
            {
                for (int j = 1; j <= depth; j++)
                {
                    if (j == 1)
                        result.Add(i);
                    else
                        result.Add(i + "?sort=newest&page=" + j);
                }
            }
            return result;
        }

        private void ReverseCategories(ref List<string> stored, string pattern, SourceCategory parent, ArticleSource source)
        {
            foreach (var cate in source.Categories)
            {
                if (cate.ParentCategory == parent)
                {
                    string value = pattern + "/" + cate.Slug;
                    stored.Add(value + "/");
                    ReverseCategories(ref stored, value, cate, source);
                }
            }
        }

        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {
            List<string> result = new List<string>();
            string pattern = "categories/" + HttpUtility.UrlDecode(chosenCategories.Slug);

            ReverseAgainstCategories(ref pattern, chosenCategories, source);

            pattern = "http://" + chosenCategories.SourceHost + "/" + pattern + "/";

            for (int j = 1; j <= depth; j++)
            {
                if (j == 1)
                    result.Add(pattern);
                else
                    result.Add(pattern + "page/" + j + "/");
            }

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
