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
    public class TheThao247Spawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            List<string> result = new List<string>();

            foreach (var cate in source.Categories)
            {
                for (int i = 1; i <= depth; i++)
                {
                    if(i==1)
                        result.Add(source.Url + "/" + cate.Slug + "/");
                    else
                        result.Add(source.Url + "/" + cate.Slug + "/p" + i);
                }
            }

            return result;
        }

        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {
            for (int i = 1; i <= depth; i++)
            {
                if (i == 1)
                    searchPages.Add(source.Url + "/" + chosenCategories.Slug + "/");
                else
                    searchPages.Add(source.Url + "/" + chosenCategories.Slug + "/p" + i);
            }
        }
    }
}
