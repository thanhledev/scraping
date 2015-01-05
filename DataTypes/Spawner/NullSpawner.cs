﻿using System;
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
    public class NullSpawner : ISpawning
    {
        public List<string> CreateSearchPages(ArticleSource source, int depth)
        {
            return null;
        }

        public void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages)
        {
            //doing nothing
        }
    }
}
