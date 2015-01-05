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
    public class SearchEngineTager : IAuthority
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

            if (real_appearance > 0)
            {
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
                        int engine = rnd.Next(2);
                        if (engine == 0)
                        {
                            tag = new TagLink(i, GetLinkFromGoogle(i, ref flag, proxy, agents));
                        }
                        else
                        {
                            tag = new TagLink(i, GetLinkFromBing(i, ref flag, proxy, agents));
                        }
                        if (flag)
                            htmlDocument = ReplaceRandom(htmlDocument, tag.Tag, tag.AuthorityLink);
                        else
                            retry++;
                    } while (!flag && retry < 3);
                }
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

        private string GetLinkFromGoogle(string tag, ref bool flag, WebProxy proxy, List<string> agents)
        {
            try
            {
                string query = "https://www.google.com/search?q=" + tag.Replace(' ', '+') + "&start=10&tbs=qdr:h3&gws_rd=ssl";
                List<string> googleSuggestLinks = new List<string>();
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

                var searchResult = document.DocumentNode.SelectSingleNode("//div[@id='res']");

                var links = searchResult.Descendants("h3").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Descendants("a").First().Attributes["href"].Value);

                    var queryString = string.Join(string.Empty, decodeLink.Split('?').Skip(1));

                    var parsedQuery = HttpUtility.ParseQueryString(queryString);

                    googleSuggestLinks.Add(parsedQuery["q"]);
                }

                flag = true;
                return "<a href=\"" + googleSuggestLinks[rnd.Next(googleSuggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        private string GetLinkFromBing(string tag, ref bool flag, WebProxy proxy, List<string> agents)
        {
            try
            {
                string query = "http://www.bing.com/news/search?q=" + tag.Replace(' ', '+');
                List<string> bingSuggestLinks = new List<string>();

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

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@class='NewsResultSet clearfix']");

                var divTitles = searchResults.SelectNodes("//div[@class='newstitle']");

                foreach (var title in divTitles)
                {
                    bingSuggestLinks.Add(HttpUtility.HtmlDecode(title.Descendants("a").First().Attributes["href"].Value));
                }
                flag = true;
                return "<a href=\"" + bingSuggestLinks[rnd.Next(bingSuggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        //with proxy
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

            if (real_appearance > 0)
            {
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
                        int engine = rnd.Next(2);
                        if (engine == 0)
                        {
                            tag = new TagLink(i, GetLinkFromGoogle(i, ref flag, agents));
                        }
                        else
                        {
                            tag = new TagLink(i, GetLinkFromBing(i, ref flag, agents));
                        }
                        if (flag)
                            htmlDocument = ReplaceRandom(htmlDocument, tag.Tag, tag.AuthorityLink);
                        else
                            retry++;
                    } while (!flag && retry < 3);
                }
            }
        }

        private string GetLinkFromGoogle(string tag, ref bool flag, List<string> agents)
        {
            try
            {
                string query = "https://www.google.com/search?q=" + tag.Replace(' ', '+') + "&start=10&tbs=qdr:h3&gws_rd=ssl";
                List<string> googleSuggestLinks = new List<string>();
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

                var searchResult = document.DocumentNode.SelectSingleNode("//div[@id='res']");

                var links = searchResult.Descendants("h3").ToList();

                foreach (var link in links)
                {
                    var decodeLink = HttpUtility.HtmlDecode(link.Descendants("a").First().Attributes["href"].Value);

                    var queryString = string.Join(string.Empty, decodeLink.Split('?').Skip(1));

                    var parsedQuery = HttpUtility.ParseQueryString(queryString);

                    googleSuggestLinks.Add(parsedQuery["q"]);
                }

                flag = true;
                return "<a href=\"" + googleSuggestLinks[rnd.Next(googleSuggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }

        private string GetLinkFromBing(string tag, ref bool flag, List<string> agents)
        {
            try
            {
                string query = "http://www.bing.com/news/search?q=" + tag.Replace(' ', '+');
                List<string> bingSuggestLinks = new List<string>();

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

                var searchResults = document.DocumentNode.SelectSingleNode("//div[@class='NewsResultSet clearfix']");

                var divTitles = searchResults.SelectNodes("//div[@class='newstitle']");

                foreach (var title in divTitles)
                {
                    bingSuggestLinks.Add(HttpUtility.HtmlDecode(title.Descendants("a").First().Attributes["href"].Value));
                }
                flag = true;
                return "<a href=\"" + bingSuggestLinks[rnd.Next(bingSuggestLinks.Count)] + "\" target=\"_blank\" rel=\"nofollow\">" + tag + "</a>";
            }
            catch (Exception ex)
            {
                flag = false;
                return tag;
            }
        }
    }
}
