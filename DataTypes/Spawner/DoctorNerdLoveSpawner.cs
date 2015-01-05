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
    public class DoctorNerdLoveSpawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            List<string> result = new List<string>();

            foreach (var i in source.Categories)
            {
                for (int j = 1; j <= depth; j++)
                {
                    if (j == 1)
                    {
                        result.Add(source.Url + "/category/" + i.Slug + "/");
                    }
                    else
                    {
                        result.Add(source.Url + "/category/" + i.Slug + "/page/" + j + "/");
                    }
                }
            }

            return result;
        }

        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {
            for (int i = 1; i <= depth; i++)
            {
                if (i == 1)
                {
                    searchPages.Add(source.Url + "/category/" + chosenCategories.Slug + "/");
                }
                else
                {
                    searchPages.Add(source.Url + "/category/" + chosenCategories.Slug + "/page/" + i + "/");
                }
            }
        }
    }
}
