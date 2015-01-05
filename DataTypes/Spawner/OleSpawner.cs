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
    public class OleSpawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            List<string> result = new List<string>();
            List<string> temp = new List<string>();

            foreach (var i in source.Categories)
            {
                temp.Add(source.Url + "/" + i.Slug + ".html");
            }

            foreach (var i in temp)
            {
                for (int j = 0; j <= depth; j++)
                {
                    result.Add(temp + "?start=" + j*30);
                }
            }

            return result;
        }

        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {
            List<string> result = new List<string>();
            string pattern = source.Url + "/" + chosenCategories.Slug + ".html?start=";

            for (int i = 0; i <= depth; i++)
            {
                result.Add(pattern + i * 30);
            }

            foreach (var k in result)
                searchPages.Add(k);
        }
    }
}
