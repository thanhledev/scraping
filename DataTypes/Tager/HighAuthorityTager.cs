using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using System.Net;
using HtmlAgilityPack;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;

namespace DataTypes
{
    public class HighAuthorityTager : IAuthority
    {
        Random rnd = new Random();

        //with proxy
        public void InsertTags(AuthorityKeywords tagType, AuthorityApperance appearance, ref string htmlDocument, List<string> articleTags, List<string> manualTags, WebProxy proxy, List<string> agents)
        {            
            int real_appearance = 0;
            List<string> chosenTags = new List<string>();
            switch (appearance)
            {
                case AuthorityApperance.UpTo1:
                    real_appearance = rnd.Next(2);
                    break;
                case AuthorityApperance.UpTo2:
                    real_appearance = rnd.Next(3);
                    break;
                case AuthorityApperance.UpTo3:
                    real_appearance = rnd.Next(4);
                    break;
            };

            int retry = 0;

            if (tagType == AuthorityKeywords.ArticleTags)
            {
                if (articleTags.Count > 0)
                {
                    if (articleTags.Count <= real_appearance)
                    {
                        chosenTags = articleTags;
                    }
                    else
                    {
                        while (chosenTags.Count < real_appearance && retry < 5)
                        {
                            string chosenTag = articleTags[rnd.Next(articleTags.Count)];

                            if (Regex.Matches(htmlDocument, chosenTag, RegexOptions.IgnoreCase).Count > 1 && !chosenTags.Contains(chosenTag))
                                chosenTags.Add(chosenTag);
                            else
                                retry++;
                        }
                    }
                }
            }
            else
            {

            }

            foreach (var i in chosenTags)
            {
                TagLink tag;
                retry = 0;
                bool flag = false;
                do
                {
                    int engine = rnd.Next(4);
                    if (engine == 0)
                    {
                        tag = new TagLink(i, GetLinkFromCNN(i, ref flag, proxy, agents));
                    }
                    else if (engine == 1)
                    {
                        tag = new TagLink(i, GetLinkFromBBC(i, ref flag, proxy, agents));
                    }
                    else if (engine == 2)
                    {
                        tag = new TagLink(i, GetLinkFromTelegraph(i, ref flag, proxy, agents));
                    }
                    else
                    {
                        tag = new TagLink(i, GetLinkFromMirrorUk(i, ref flag, proxy, agents));
                    }
                    if (flag)
                        htmlDocument = ReplaceRandom(htmlDocument, tag.Tag, tag.AuthorityLink);
                    else
                        retry++;
                } while (!flag && retry < 3);
            }
        }

        private string ReplaceRandom(string text, string search, string replace)
        {
            List<int> appearance_post = new List<int>();

            CompareInfo sampleInfo = CultureInfo.InvariantCulture.CompareInfo;

            int pos = sampleInfo.IndexOf(text, search, CompareOptions.IgnoreCase);

            while (pos > 0)
            {
                appearance_post.Add(pos);
                pos = sampleInfo.IndexOf(text, search, pos + 1, CompareOptions.IgnoreCase);
            }
            if (appearance_post.Count > 0)
            {                
                pos = appearance_post[rnd.Next(appearance_post.Count)];
                return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
            }
            else
                return text;
        }

        private string GetLinkFromCNN(string tag, ref bool flag, WebProxy proxy, List<string> agents)
        {
            try
            {
                string query = "http://edition.cnn.com/search/?query=" + tag.Replace(" ", "%20") + "&intl=true&sortBy=relevance#&sortBy=relevance";
                List<string> suggestLinks = new List<string>();
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                int pos = 0;                
                //pos = rnd.Next(proxies.Count);
                //WebProxy p = new WebProxy(proxies.ElementAt(pos).Ip, Convert.ToInt32(proxies.ElementAt(pos).Port));
                //p.Credentials = new NetworkCredential(proxies.ElementAt(pos).Username, proxies.ElementAt(pos).Password);

                //WebProxy p = new WebProxy(proxy.Ip, Convert.ToInt32(proxy.Port));
                //if (proxy.Username != null && proxy.Password != null)
                //    p.Credentials = new NetworkCredential(proxy.Username, proxy.Password);

                request.Proxy = proxy;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //we will read data via the response stream
                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@id='mixedresults']");

                var links = searchResults.Descendants("li").Where(a => a.Attributes["class"].Value == "cnnResultTitle cnnResultTitleTopic").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Descendants("a").First().Attributes["href"].Value);

                    suggestLinks.Add(decodeLink);
                }

                flag = true;
                return "<a href=\"" + suggestLinks[rnd.Next(suggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        private string GetLinkFromBBC(string tag, ref bool flag, WebProxy proxy, List<string> agents)
        {
            try
            {
                string query = "http://www.bbc.co.uk/search/sport/?q=" + tag.Replace(" ", "%20") + "&video=on&audio=on&text=on";
                List<string> suggestLinks = new List<string>();
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                int pos = 0;                
                //pos = rnd.Next(proxies.Count);
                //WebProxy p = new WebProxy(proxies.ElementAt(pos).Ip, Convert.ToInt32(proxies.ElementAt(pos).Port));
                //p.Credentials = new NetworkCredential(proxies.ElementAt(pos).Username, proxies.ElementAt(pos).Password);

                //WebProxy p = new WebProxy(proxy.Ip, Convert.ToInt32(proxy.Port));
                //if (proxy.Username != null && proxy.Password != null)
                //    p.Credentials = new NetworkCredential(proxy.Username, proxy.Password);

                request.Proxy = proxy;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //we will read data via the response stream
                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@id='sport-content']");

                var links = searchResults.Descendants("a").Where(a => a.Attributes["class"].Value == "title linktrack-title").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Attributes["href"].Value);

                    suggestLinks.Add(decodeLink);
                }

                flag = true;
                return "<a href=\"" + suggestLinks[rnd.Next(suggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        private string GetLinkFromTelegraph(string tag, ref bool flag, WebProxy proxy, List<string> agents)
        {
            try
            {
                string query = "http://www.telegraph.co.uk/search/?queryText=" + tag.Replace(' ', '+') + "&site=telegraph_sport";
                List<string> suggestLinks = new List<string>();
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                int pos = 0;                
                //pos = rnd.Next(proxies.Count);
                //WebProxy p = new WebProxy(proxies.ElementAt(pos).Ip, Convert.ToInt32(proxies.ElementAt(pos).Port));
                //p.Credentials = new NetworkCredential(proxies.ElementAt(pos).Username, proxies.ElementAt(pos).Password);

                //WebProxy p = new WebProxy(proxy.Ip, Convert.ToInt32(proxy.Port));
                //if (proxy.Username != null && proxy.Password != null)
                //    p.Credentials = new NetworkCredential(proxy.Username, proxy.Password);

                request.Proxy = proxy;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //we will read data via the response stream
                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

                var links = searchResults.SelectNodes("//div[@class='summary']").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Descendants("a").First().Attributes["href"].Value);

                    suggestLinks.Add(decodeLink);
                }

                flag = true;
                return "<a href=\"" + suggestLinks[rnd.Next(suggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        private string GetLinkFromMirrorUk(string tag, ref bool flag, WebProxy proxy, List<string> agents)
        {
            try
            {
                string query = "http://www.mirror.co.uk/search/simple.do?destinationSectionId=219&publicationName=mirror&sortString=publishdate&sortOrder=desc&sectionId=69&articleTypes=+news+opinion+advice&pageNumber=1&pageLength=10&searchString=" + tag.Replace(' ', '+');
                List<string> suggestLinks = new List<string>();
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                int pos = 0;                
                //pos = rnd.Next(proxies.Count);
                //WebProxy p = new WebProxy(proxies.ElementAt(pos).Ip, Convert.ToInt32(proxies.ElementAt(pos).Port));
                //p.Credentials = new NetworkCredential(proxies.ElementAt(pos).Username, proxies.ElementAt(pos).Password);

                //WebProxy p = new WebProxy(proxy.Ip, Convert.ToInt32(proxy.Port));
                //if (proxy.Username != null && proxy.Password != null)
                //    p.Credentials = new NetworkCredential(proxy.Username, proxy.Password);

                request.Proxy = proxy;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //we will read data via the response stream
                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@id='search-result']");

                var links = searchResults.SelectNodes("//div[@class='article ma-teaser clearfix']").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Descendants("a").First().Attributes["href"].Value);

                    suggestLinks.Add(decodeLink);
                }

                flag = true;
                return "<a href=\"" + suggestLinks[rnd.Next(suggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        //without proxy
        public void InsertTags(AuthorityKeywords tagType, AuthorityApperance appearance, ref string htmlDocument, List<string> articleTags, List<string> manualTags, List<string> agents)
        {
            int real_appearance = 0;
            List<string> chosenTags = new List<string>();
            switch (appearance)
            {
                case AuthorityApperance.UpTo1:
                    real_appearance = rnd.Next(2);
                    break;
                case AuthorityApperance.UpTo2:
                    real_appearance = rnd.Next(3);
                    break;
                case AuthorityApperance.UpTo3:
                    real_appearance = rnd.Next(4);
                    break;
            };

            int retry = 0;

            if (tagType == AuthorityKeywords.ArticleTags)
            {
                if (articleTags.Count > 0)
                {
                    if (articleTags.Count <= real_appearance)
                    {
                        chosenTags = articleTags;
                    }
                    else
                    {
                        while (chosenTags.Count < real_appearance && retry < 5)
                        {
                            string chosenTag = articleTags[rnd.Next(articleTags.Count)];

                            if (Regex.Matches(htmlDocument, chosenTag, RegexOptions.IgnoreCase).Count > 1 && !chosenTags.Contains(chosenTag))
                                chosenTags.Add(chosenTag);
                            else
                                retry++;
                        }
                    }
                }
            }
            else
            {

            }

            foreach (var i in chosenTags)
            {
                TagLink tag;
                retry = 0;
                bool flag = false;
                do
                {
                    int engine = rnd.Next(4);
                    if (engine == 0)
                    {
                        tag = new TagLink(i, GetLinkFromCNN(i, ref flag, agents));
                    }
                    else if (engine == 1)
                    {
                        tag = new TagLink(i, GetLinkFromBBC(i, ref flag, agents));
                    }
                    else if (engine == 2)
                    {
                        tag = new TagLink(i, GetLinkFromTelegraph(i, ref flag, agents));
                    }
                    else
                    {
                        tag = new TagLink(i, GetLinkFromMirrorUk(i, ref flag, agents));
                    }
                    if (flag)
                        htmlDocument = ReplaceRandom(htmlDocument, tag.Tag, tag.AuthorityLink);
                    else
                        retry++;
                } while (!flag && retry < 3);
            }
        }

        private string GetLinkFromCNN(string tag, ref bool flag, List<string> agents)
        {
            try
            {
                string query = "http://edition.cnn.com/search/?query=" + tag.Replace(" ", "%20") + "&intl=true&sortBy=relevance#&sortBy=relevance";
                List<string> suggestLinks = new List<string>();
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                int pos = 0;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //we will read data via the response stream
                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@id='mixedresults']");

                var links = searchResults.Descendants("li").Where(a => a.Attributes["class"].Value == "cnnResultTitle cnnResultTitleTopic").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Descendants("a").First().Attributes["href"].Value);

                    suggestLinks.Add(decodeLink);
                }

                flag = true;
                return "<a href=\"" + suggestLinks[rnd.Next(suggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        private string GetLinkFromBBC(string tag, ref bool flag, List<string> agents)
        {
            try
            {
                string query = "http://www.bbc.co.uk/search/sport/?q=" + tag.Replace(" ", "%20") + "&video=on&audio=on&text=on";
                List<string> suggestLinks = new List<string>();
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                int pos = 0;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //we will read data via the response stream
                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@id='sport-content']");

                var links = searchResults.Descendants("a").Where(a => a.Attributes["class"].Value == "title linktrack-title").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Attributes["href"].Value);

                    suggestLinks.Add(decodeLink);
                }

                flag = true;
                return "<a href=\"" + suggestLinks[rnd.Next(suggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        private string GetLinkFromTelegraph(string tag, ref bool flag, List<string> agents)
        {
            try
            {
                string query = "http://www.telegraph.co.uk/search/?queryText=" + tag.Replace(' ', '+') + "&site=telegraph_sport";
                List<string> suggestLinks = new List<string>();
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                int pos = 0;                
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //we will read data via the response stream
                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@class='twoThirds gutter']");

                var links = searchResults.SelectNodes("//div[@class='summary']").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Descendants("a").First().Attributes["href"].Value);

                    suggestLinks.Add(decodeLink);
                }

                flag = true;
                return "<a href=\"" + suggestLinks[rnd.Next(suggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        private string GetLinkFromMirrorUk(string tag, ref bool flag, List<string> agents)
        {
            try
            {
                string query = "http://www.mirror.co.uk/search/simple.do?destinationSectionId=219&publicationName=mirror&sortString=publishdate&sortOrder=desc&sectionId=69&articleTypes=+news+opinion+advice&pageNumber=1&pageLength=10&searchString=" + tag.Replace(' ', '+');
                List<string> suggestLinks = new List<string>();
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                int pos = 0;                
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //we will read data via the response stream
                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@id='search-result']");

                var links = searchResults.SelectNodes("//div[@class='article ma-teaser clearfix']").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Descendants("a").First().Attributes["href"].Value);

                    suggestLinks.Add(decodeLink);
                }

                flag = true;
                return "<a href=\"" + suggestLinks[rnd.Next(suggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }
    }
}
