using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Interfaces;
using DataTypes.Collections;
using System.Globalization;
using System.IO;

namespace DataTypes
{
    public class NullPathCreater : IPathCreater
    {
        public void CreateFilePath(string hUrl, string aUrl, string savingFolder, List<ArticleSource> sources, ref string filePath, ref string fileName)
        {
        }
    }
}
