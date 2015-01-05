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
    public class BongDaPlusSpawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            List<string> result = new List<string>();

            foreach (var i in source.Categories)
            {
                for (int j = 1; j <= depth; j++)
                {
                    result.Add(source.Url + "/" + i.Slug + "/trang-" + j + ".bdplus");
                }
            }

            return result;
        }

        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {
            for (int i = 1; i <= depth; i++)
            {
                searchPages.Add(source.Url + "/" + chosenCategories.Slug + "/trang-" + i + ".bdplus");
            }
        }
    }
}
