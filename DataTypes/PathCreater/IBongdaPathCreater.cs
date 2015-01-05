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
    public class IBongdaPathCreater : IPathCreater
    {
        public void CreateFilePath(string hUrl, string aUrl, string savingFolder, List<ArticleSource> sources, ref string filePath, ref string fileName)
        {
            string hLink = hUrl;

            Uri tmp = new Uri(hLink);
            string host = tmp.Scheme + Uri.SchemeDelimiter + tmp.Host + "/";
            var link = hLink.Replace(host, "").Trim();
            var aLink = aUrl.Replace(host, "").Trim();

            string[] content = link.Split('/');

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

            link = "";

            for (int j = 0; j < content.Length - 1; j++)
            {
                if (j < content.Length - 2)
                    link += content[j] + "/";
                else
                    link += content[j];
            }

            if (CheckSlugIsExisted2(sources, tmp.Host, link) != null)
            {
                filePath += "\\" + CheckSlugIsExisted2(sources, tmp.Host, link).Replace('/', '-');
            }

            filePath = filePath + "\\" + string.Format("{0}-{1}-{2}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year);

            string[] aContent = aLink.Split('/');

            fileName = aContent.Last();
        }

        private string CheckSlugIsExisted2(List<ArticleSource> sources, string host, string path)
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
