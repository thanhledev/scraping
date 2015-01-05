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
    public class BongDaLivesSpawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            List<string> result = new List<string>();

            foreach (var i in source.Categories)
            {
                if(i.Slug != "news")
                    result.Add(source.Url + "/news/" + i.Slug + "/");
            }

            return result;
        }

        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {            
            searchPages.Add(source.Url + "/news/" + chosenCategories.Slug + "/");
        }
    }
}
