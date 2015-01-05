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
    public class TipKeoSpawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            List<string> result = new List<string>();
            List<string> temp = new List<string>();

            foreach (var i in source.Categories)
            {
                if(i.Slug != "du-lieu-bong-da")
                    temp.Add(source.Url + "/" + i.Slug);
            }

            foreach (var i in temp)
            {
                for (int j = 1; j <= depth; j++)
                {
                    result.Add(temp + "/s_p:" + j);
                }
            }

            return result;
        }
        
        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {
            List<string> result = new List<string>();
            string pattern = source.Url + "/" + chosenCategories.Slug + "/s_p:";

            for (int i = 1; i <= depth; i++)
            {
                result.Add(pattern + i);
            }

            foreach (var k in result)
                searchPages.Add(k);
        }
    }
}
