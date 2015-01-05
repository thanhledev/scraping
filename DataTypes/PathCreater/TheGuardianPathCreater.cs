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
    public class TheGuardianPathCreater : IPathCreater
    {
        public void CreateFilePath(string hUrl, string aUrl, string savingFolder, List<ArticleSource> sources, ref string filePath, ref string fileName)
        {
            string hLink = hUrl;

            Uri tmp = new Uri(hLink);
            string host = tmp.Scheme + Uri.SchemeDelimiter + tmp.Host + "/";
            hLink = hLink.Replace(host, "").Trim();
            string[] content = hLink.Split('/');

            if (content.Last() == string.Empty)
                content = content.Take(content.Length - 1).ToArray();

            string[] dirs = Directory.GetDirectories(savingFolder + "\\");

            foreach (var site in dirs)
            {
                if (host.Contains(new DirectoryInfo(site).Name))
                {
                    filePath += site;
                    break;
                }
            }

            for (int i = 0; i < content.Count() - 1; i++)
            {
                if (CheckSlugIsExisted(sources, tmp.Host, content[i]) != null)
                    filePath += "\\" + CheckSlugIsExisted(sources, tmp.Host, content[i]);
            }

            filePath = filePath + "\\" + string.Format("{0}-{1}-{2}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year);

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string[] aContent = aUrl.Split('/');

            if (aContent.Last() == string.Empty)
                aContent = aContent.Take(aContent.Length - 1).ToArray();

            fileName = aContent.Last();

            if (!fileName.Contains('.'))
            {
                fileName += ".html";
            }
        }

        private string CheckSlugIsExisted(List<ArticleSource> sources, string host, string path)
        {
            foreach (var src in sources)
            {
                if (host.Contains(src.Title))
                {
                    foreach (var cate in src.Categories)
                    {
                        if (cate.Slug.ToLower().Contains(path.ToLower()))
                            return cate.Slug;
                    }
                }
            }
            return null;
        }
    }
}
