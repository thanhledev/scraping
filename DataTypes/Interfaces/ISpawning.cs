using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Enums;
using DataTypes.Collections;
namespace DataTypes.Interfaces
{
    public interface ISpawning
    {
        List<string> CreateSearchPages(ArticleSource source, int depth);

        void CreateSearchPagesLiveFeed(ArticleSource source, int depth, SourceCategory chosenCategories, ref List<string> searchPages);
    }
}
