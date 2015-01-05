using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Collections;

namespace DataTypes.Interfaces
{
    public interface IPathCreater
    {
        void CreateFilePath(string hUrl, string aUrl, string savingFolder, List<ArticleSource> sources, ref string filePath, ref string fileName);
    }
}
