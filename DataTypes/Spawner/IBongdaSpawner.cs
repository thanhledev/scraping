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
    public class IBongdaSpawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            List<string> result = new List<string>();

            foreach (var i in source.Categories)
            {
                if (i.Slug.Contains("nhan-dinh"))
                    result.Add(source.Url + "/" + i.Slug + "/24h.html");
                else
                    result.Add(source.Url + "/" + i.Slug);
            }

            return result;
        }

        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {
            if (chosenCategories.Slug.Contains("nhan-dinh"))
                searchPages.Add(source.Url + "/" + chosenCategories.Slug + "/24h.html");
            else
                searchPages.Add(source.Url + "/" + chosenCategories.Slug);
        }
    }
}
