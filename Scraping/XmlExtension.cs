using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DataTypes.Collections;
using DataTypes.Enums;

namespace Scraping
{
    public static class XmlExtension
    {
        public static IEnumerable<Record> GetRecords(this XmlReader source)
        {
            while (source.Read())
            {
                if (source.NodeType == XmlNodeType.Element && source.Name == "record")
                {
                    yield return new Record
                    {
                        downloaded = Convert.ToDateTime(source.GetAttribute("downloaded")),
                        published = Convert.ToDateTime(source.GetAttribute("published")),
                        url = source.GetAttribute("url"),
                        hUrl = source.GetAttribute("hUrl"),
                        filePath = source.GetAttribute("file"),
                        isPosted = Convert.ToBoolean(source.GetAttribute("isPosted"))
                    };
                }
            }
        }
    }
}
