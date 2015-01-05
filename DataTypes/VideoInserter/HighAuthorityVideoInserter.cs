using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;

namespace DataTypes
{
    public class HighAuthorityVideoInserter : IVideo
    {
        Random rnd = new Random();

        public void InsertVideo(AuthorityKeywords tagType, ref string htmlBody, List<string> articleTags, List<string> manualTags, WebProxy proxy, List<string> agents)
        {            
            int chosen = rnd.Next(3);

            if (tagType == AuthorityKeywords.ArticleTags)
            {
                if (articleTags.Count > 0)
                {
                    //old code
                    //switch (chosen)
                    //{
                    //    case 0:
                    //        InsertYoutubeVideo(ref htmlBody, articleTags, proxy, agents);
                    //        break;
                    //    case 1:
                    //        InsertDailyMotionVideo(ref htmlBody, articleTags, proxy, agents);
                    //        break;
                    //    case 2:
                    //        InsertVimeoVideo(ref htmlBody, articleTags, proxy, agents);
                    //        break;
                    //};

                    InsertYoutubeVideo(ref htmlBody, articleTags, proxy, agents);
                }
            }
            else
            {
                //not yet be implemented
            }
        }
        
        private void InsertYoutubeVideo(ref string htmlBody, List<string> keywords, WebProxy proxy, List<string> agents)
        {            
            string chosenKeyword = keywords[rnd.Next(keywords.Count)];
            List<string> youtubeSuggestVideos = new List<string>();

            string query = "https://www.youtube.com/results?search_query=" + chosenKeyword.Replace(" ", "+");

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                request.Proxy = proxy;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                int pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='results']");

                if (mainContent != null)
                {
                    var titles = mainContent.Descendants("h3");

                    if (titles != null)
                    {
                        foreach (var title in titles)
                        {
                            if (title.Attributes["class"] != null)
                            {
                                if (title.Attributes["class"].Value == "yt-lockup-title")
                                {
                                    var videoId = title.Descendants("a").First().Attributes["href"].Value;

                                    //if (!videoLink.Contains("youtube.com"))
                                    //    videoLink += "https://www.youtube.com" + videoLink;

                                    videoId = videoId.Replace("/watch?v=", "");

                                    youtubeSuggestVideos.Add(videoId);
                                }
                            }
                        }

                        var chosenYoutubeVideo = youtubeSuggestVideos[rnd.Next(youtubeSuggestVideos.Count)];

                        var youtubePlaceholder = "<div style=\"margin-left: auto; margin-right: auto;\"><iframe width=\"560\" height=\"315\" src=\"//www.youtube.com/embed/" + chosenYoutubeVideo + "\" frameborder=\"0\" allowfullscreen></iframe></div>";

                        htmlBody += youtubePlaceholder;
                    }
                }                
            }
            catch (Exception)
            {

            }
        }

        private void InsertDailyMotionVideo(ref string htmlBody, List<string> keywords, WebProxy proxy, List<string> agents)
        {            
            string chosenKeyword = keywords[rnd.Next(keywords.Count)];
            List<string> dailymotionSuggestVideos = new List<string>();

            string query = "http://www.dailymotion.com/us/relevance/search/" + chosenKeyword.Replace(" ", "+") +"/1";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                request.Proxy = proxy;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                int pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='js-list-list']");

                if (mainContent != null)
                {
                    var titles = mainContent.Descendants("//div[@class='title font-lg mrg-btm-sm']");

                    if (titles != null)
                    {
                        foreach (var title in titles)
                        {
                            var link = title.Descendants("a").First().Attributes["href"].Value;

                            string[] args = link.Split('_');

                            link = args[0];

                            dailymotionSuggestVideos.Add(link);
                        }

                        var chosenDailymotionVideo = dailymotionSuggestVideos[rnd.Next(dailymotionSuggestVideos.Count)];

                        var dailymotionPlaceholder = "<div style=\"margin-left: auto; margin-right: auto;\"><iframe frameborder=\"0\" width=\"480\" height=\"270\" src=\"//www.dailymotion.com/embed/" + chosenDailymotionVideo + "?logo=0&info=0\" allowfullscreen></iframe></div>";

                        htmlBody += dailymotionPlaceholder;
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void InsertVimeoVideo(ref string htmlBody, List<string> keywords, WebProxy proxy, List<string> agents)
        {            
            string chosenKeyword = keywords[rnd.Next(keywords.Count)];
            List<string> vimeoSuggestVideos = new List<string>();

            string query = "http://vimeo.com/search?q=" + chosenKeyword.Replace(" ", "+");

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);

                request.Proxy = proxy;
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                int pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var mainContent = document.DocumentNode.SelectSingleNode("//ol[@class='js-browse_list clearfix browse browse_videos browse_videos_thumbnails kane']");

                if (mainContent != null)
                {
                    var links = mainContent.Descendants("a");

                    if (links != null)
                    {
                        foreach (var link in links)
                        {
                            var href = link.Attributes["href"].Value;

                            vimeoSuggestVideos.Add(href);
                        }

                        var chosenVimeoVideo = vimeoSuggestVideos[rnd.Next(vimeoSuggestVideos.Count)];

                        var vimeoPlaceholder = "<div style=\"margin-left: auto; margin-right: auto;\"><iframe src=\"//player.vimeo.com/video/" + chosenVimeoVideo + "\" width=\"500\" height=\"281\" frameborder=\"0\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe></div>";

                        htmlBody += vimeoPlaceholder;
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        public void InsertVideo(AuthorityKeywords tagType, ref string htmlBody, List<string> articleTags, List<string> manualTags, List<string> agents)
        {
            int chosen = rnd.Next(3);

            if (tagType == AuthorityKeywords.ArticleTags)
            {
                if (articleTags.Count > 0)
                {
                    //old code
                    //switch (chosen)
                    //{
                    //    case 0:
                    //        InsertYoutubeVideo(ref htmlBody, articleTags, agents);
                    //        break;
                    //    case 1:
                    //        InsertDailyMotionVideo(ref htmlBody, articleTags, agents);
                    //        break;
                    //    case 2:
                    //        InsertVimeoVideo(ref htmlBody, articleTags, agents);
                    //        break;
                    //};

                    InsertYoutubeVideo(ref htmlBody, articleTags, agents);
                }
            }
            else
            {
                //not yet be implemented
            }
        }

        private void InsertYoutubeVideo(ref string htmlBody, List<string> keywords, List<string> agents)
        {
            string chosenKeyword = keywords[rnd.Next(keywords.Count)];
            List<string> youtubeSuggestVideos = new List<string>();

            string query = "https://www.youtube.com/results?search_query=" + chosenKeyword.Replace(" ", "+");

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
                
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                int pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var mainContent = document.DocumentNode.SelectSingleNode("//div[@id='results']");

                if (mainContent != null)
                {
                    var titles = mainContent.Descendants("h3");

                    if (titles != null)
                    {
                        foreach (var title in titles)
                        {
                            if (title.Attributes["class"] != null)
                            {
                                if (title.Attributes["class"].Value == "yt-lockup-title")
                                {
                                    var videoId = title.Descendants("a").First().Attributes["href"].Value;

                                    //if (!videoLink.Contains("youtube.com"))
                                    //    videoLink += "https://www.youtube.com" + videoLink;

                                    videoId = videoId.Replace("/watch?v=", "");

                                    youtubeSuggestVideos.Add(videoId);
                                }
                            }
                        }

                        var chosenYoutubeVideo = youtubeSuggestVideos[rnd.Next(youtubeSuggestVideos.Count)];

                        var youtubePlaceholder = "<div style=\"margin-left: auto; margin-right: auto;\"><iframe width=\"560\" height=\"315\" src=\"//www.youtube.com/embed/" + chosenYoutubeVideo + "\" frameborder=\"0\" allowfullscreen></iframe></div>";

                        htmlBody += youtubePlaceholder;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void InsertDailyMotionVideo(ref string htmlBody, List<string> keywords, List<string> agents)
        {
            string chosenKeyword = keywords[rnd.Next(keywords.Count)];
            List<string> dailymotionSuggestVideos = new List<string>();

            string query = "http://www.dailymotion.com/us/relevance/search/" + chosenKeyword.Replace(" ", "+") + "/1";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
                
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                int pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var mainContent = document.DocumentNode.SelectSingleNode("//div[@class='js-list-list']");

                if (mainContent != null)
                {
                    var titles = mainContent.Descendants("//div[@class='title font-lg mrg-btm-sm']");

                    if (titles != null)
                    {
                        foreach (var title in titles)
                        {
                            var link = title.Descendants("a").First().Attributes["href"].Value;

                            string[] args = link.Split('_');

                            link = args[0];

                            dailymotionSuggestVideos.Add(link);
                        }

                        var chosenDailymotionVideo = dailymotionSuggestVideos[rnd.Next(dailymotionSuggestVideos.Count)];

                        var dailymotionPlaceholder = "<div style=\"margin-left: auto; margin-right: auto;\"><iframe frameborder=\"0\" width=\"480\" height=\"270\" src=\"//www.dailymotion.com/embed/" + chosenDailymotionVideo + "?logo=0&info=0\" allowfullscreen></iframe></div>";

                        htmlBody += dailymotionPlaceholder;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void InsertVimeoVideo(ref string htmlBody, List<string> keywords, List<string> agents)
        {
            string chosenKeyword = keywords[rnd.Next(keywords.Count)];
            List<string> vimeoSuggestVideos = new List<string>();

            string query = "http://vimeo.com/search?q=" + chosenKeyword.Replace(" ", "+");

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
                
                request.Timeout = 30000;
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml";

                int pos = rnd.Next(agents.Count);
                request.UserAgent = agents[pos];

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var document = new HtmlDocument();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    document.Load(reader.BaseStream);
                }

                var mainContent = document.DocumentNode.SelectSingleNode("//ol[@class='js-browse_list clearfix browse browse_videos browse_videos_thumbnails kane']");

                if (mainContent != null)
                {
                    var links = mainContent.Descendants("a");

                    if (links != null)
                    {
                        foreach (var link in links)
                        {
                            var href = link.Attributes["href"].Value;

                            vimeoSuggestVideos.Add(href);
                        }

                        var chosenVimeoVideo = vimeoSuggestVideos[rnd.Next(vimeoSuggestVideos.Count)];

                        var vimeoPlaceholder = "<div style=\"margin-left: auto; margin-right: auto;\"><iframe src=\"//player.vimeo.com/video/" + chosenVimeoVideo + "\" width=\"500\" height=\"281\" frameborder=\"0\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe></div>";

                        htmlBody += vimeoPlaceholder;
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
