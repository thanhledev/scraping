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
    public class TheThao247PathCreater : IPathCreater
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

            fileName = aUrl.Split('/').Last();

            //if (!fileName.Contains(".html") && !fileName.Contains(".aspx") && !fileName.Contains(".htm"))
            //{
            //    fileName += ".html";
            //}

            if (!fileName.Contains('.'))
            {
                fileName += ".html";
            }
            else
            {
                if (!fileName.Contains(".html") && !fileName.Contains(".aspx") && !fileName.Contains(".htm"))
                {
                    string[] content1 = fileName.Split('.');
                    fileName = content1[0] + ".html";
                }
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
