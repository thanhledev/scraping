using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;
using DataTypes.Collections;
using DataTypes.Enums;
using System.Diagnostics;
using System.Windows.Threading;
using System.IO;
using System.Text.RegularExpressions;
namespace Utilities
{
    public static class Helper
    {
        /// <summary>
        /// CPU & RAM Consumption 
        /// </summary>
        private static Process p = Process.GetCurrentProcess();
        private static PerformanceCounter _cpuCounter = new PerformanceCounter("Process", "% Processor Time", p.ProcessName);
        private static PerformanceCounter _ramCounter = new PerformanceCounter("Process", "Working Set - Private", p.ProcessName);

        public static List<string> ReadAdvancedValues(string path, string node)
        {
            List<string> temp = new List<string>();            
            XDocument doc = XDocument.Load(path);

            var nodes = from n in doc.Descendants(node)
                        select n.Value;
            foreach (var i in nodes)
            {
                temp.Add(i);
            }                                
            return temp;            
        }        

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        //public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        //{
        //    // Confirm parent and childName are valid. 
        //    if (parent == null) return null;

        //    T foundChild = null;

        //    int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        //    for (int i = 0; i < childrenCount; i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(parent, i);
        //        // If the child is not of the request child type child
        //        T childType = child as T;
        //        if (childType == null)
        //        {
        //            // recursively drill down the tree
        //            foundChild = FindChild<T>(child, childName);

        //            // If the child is found, break so we do not overwrite the found child. 
        //            if (foundChild != null) break;
        //        }
        //        else if (!string.IsNullOrEmpty(childName))
        //        {
        //            var frameworkElement = child as FrameworkElement;
        //            // If the child's name is set for search
        //            if (frameworkElement != null && frameworkElement.Name == childName)
        //            {
        //                // if the child's name is of the request name
        //                foundChild = (T)child;
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            // child element found.
        //            foundChild = (T)child;
        //            break;
        //        }
        //    }

        //    return foundChild;
        //}

        public static List<ArticleSource> LoadSources(string path)
        {
            List<ArticleSource> temp = new List<ArticleSource>();

            XDocument doc = XDocument.Load(path);

            var nodes = from n in doc.Descendants("source")
                        select new
                        {
                            Name = n.Element("name").Value,
                            Title = n.Element("title").Value,
                            Url = n.Element("url").Value
                        };

            foreach (var i in nodes)
            {
                ArticleSource newItem = new ArticleSource(i.Name, i.Title, i.Url);
                temp.Add(newItem);
            }

            return temp;
        }

        public static Nullable<T> Parse<T>(string input) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Generic Type 'T' must be an Enum");
            }
            if (!string.IsNullOrEmpty(input))
            {
                if (Enum.GetNames(typeof(T)).Any(e => e == input))
                    return (T)Enum.Parse(typeof(T), input, true);
            }
            return null;
        }

        public static string GetRamConsumption()
        {
            double ram = _ramCounter.NextValue();
            return string.Format("{0:N2} MB", ram / 1024 / 1024);
        }

        public static string GetCPUConsumption()
        {
            //dynamic firstValue = _cpuCounter.NextValue();
            //System.Threading.Thread.Sleep(1000);
            dynamic secondValue = _cpuCounter.NextValue();
            return string.Format("{0:N1} %", secondValue);
        }

        public static SystemProxy GetProxy(string value)
        {
            string[] proxyValues = value.Split(':');
            SystemProxy temp = new SystemProxy();

            switch (proxyValues.Length)
            {
                case 2:
                    temp.Ip = proxyValues[0];
                    temp.Port = proxyValues[1];
                    break;
                case 4:
                    temp.Ip = proxyValues[0];
                    temp.Port = proxyValues[1];
                    temp.Username = proxyValues[2];
                    temp.Password = proxyValues[3];
                    temp.IsPrivate = true;
                    break;
            };

            return temp;
        }

        public static string CreateMessage(string format,string message)
        {
            return DateTime.Now.ToString(format) + " : " + message;
        }

        public static string CreateSearchPageByKeywords(ArticleSource source, string keyword, int depth)
        {
            string result = "";
            keyword = keyword.ToLower().Trim();

            switch (source.Title)
            {
                case "bongda.com.vn":
                    keyword = keyword.Replace(' ', '+');                   
                    break;
                case "thethao247.vn":
                    keyword = keyword.Replace(' ', '-');                    
                    break;
            };

            result = source.Url + source.ScrapeLinkStructure;
            result = result.Replace("{0}", keyword);
            result = result.Replace("{1}", depth.ToString());

            return result;
        }

        //public static List<string> CreateSearchPageByCategories(ArticleSource source, int depth)
        //{
        //    switch (source.Title)
        //    {
        //        case "bongda.com.vn":
        //            break;
        //        case "thethao247.vn":
        //            break;
        //        default:
        //            return null;
        //    };
        //}

        //private List<string> 

        public static string GetFullDomain(string url)
        {
            if (url.Contains("http://"))
                return "http://" + new Uri(url).Host;
            else
                return "https://" + new Uri(url).Host;
        }

        public static string CreateSignalNodeToHarvestLink(string url)
        {
            if(url.Contains("bongda.com.vn"))
                return "//div[@id='ctl00_BD_sResult']";
            if (url.Contains("thethao247.vn"))
                return "//div[@class='tagcat2-tinmoi']";
            return "";
        }

        public static bool CheckExistFolder(string path)
        {
            return Directory.Exists(path);
        }

        public static string convertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            temp = regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');

            return Regex.Replace(temp, @"[^0-9a-zA-Z ]+", "").Trim().Replace(" ", "-");
        }

        public static TimeZoneInfo GetTimeZoneInfo(string timeZoneId)
        {
            foreach (TimeZoneInfo info in TimeZoneInfo.GetSystemTimeZones())
            {
                if (string.Compare(info.Id, timeZoneId, true) == 0)
                    return info;
            }
            return TimeZoneInfo.Utc;
        }

        public static Record ConvertFromArticle(Article article,string filePath, string fileName)
        {
            Record item = new Record();

            item.url = article.url;
            item.downloaded = article.downloaded;
            item.published = article.publish;
            item.isPosted = false;
            item.filePath = filePath + "\\" + fileName;            

            return item;
        }
    }
}
