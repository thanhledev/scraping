using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using DataTypes.Enums;
using DataTypes.Interfaces;
using DataTypes.Collections;

namespace DataTypes
{
    public class VnexpressHarvester : IHarvest
    {
        public List<HarvestLink> HarvestLinks(HtmlDocument document, string link, int page)
        {
            var host = "http://" + new Uri(link).Host;
            List<HarvestLink> result = new List<HarvestLink>();
            string href = "";
            DateTime hrefDate = DateTime.Now.AddMinutes((page * -1) * 60);

            if (!link.Contains("photo"))
            {
                //big news
                var bigNews = document.DocumentNode.SelectSingleNode("//div[@class='box_hot_news']");

                if (bigNews != null)
                {
                    var bigNewsContent = bigNews.SelectSingleNode("div[@class='title_news']");

                    if (bigNewsContent != null)
                    {
                        href = bigNews.Descendants("a").First().Attributes["href"].Value;

                        result.Add(new HarvestLink(href, hrefDate, link));
                    }                    
                }

                var scrollNews = document.DocumentNode.SelectSingleNode("//div[@class='content_scoller width_common']");

                if (scrollNews != null)
                {
                    var scrollNewsLinks = scrollNews.Descendants("a").ToList();

                    if (scrollNewsLinks != null)
                    {
                        foreach (var lnk in scrollNewsLinks)
                        {
                            if (lnk.Attributes["class"] != null)
                            {
                                if (lnk.Attributes["class"].Value == "txt_link")
                                {
                                    href = lnk.Attributes["href"].Value;

                                    if (!IsContains(result, href))
                                        result.Add(new HarvestLink(href, hrefDate, link));
                                }
                            }
                        }
                    }
                }

                var midNews = document.DocumentNode.SelectSingleNode("//div[@class='block_mid_new']");

                if (midNews != null)
                {
                    var midNewsLinks = midNews.Descendants("a").ToList();

                    if (midNewsLinks != null)
                    {
                        foreach (var lnk in midNewsLinks)
                        {
                            if (lnk.Attributes["class"] != null)
                            {
                                if (lnk.Attributes["class"].Value == "txt_link")
                                {
                                    href = lnk.Attributes["href"].Value;

                                    if (!IsContains(result, href))
                                        result.Add(new HarvestLink(href, hrefDate, link));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //big photos
                var bigPhotos = document.DocumentNode.SelectSingleNode("//div[@id='box_anh_page']");

                if (bigPhotos != null)
                {
                    var bigPhotoLinks = bigPhotos.Descendants("a").ToList();

                    foreach (var lnk in bigPhotoLinks)
                    {
                        href = lnk.Attributes["href"].Value;

                        result.Add(new HarvestLink(href, hrefDate, link));
                    }
                }

                var photoList = document.DocumentNode.SelectSingleNode("//ul[@class='list_photo_trandau']");

                if (photoList != null)
                {
                    var photoListLinks = photoList.Descendants("a").ToList();

                    foreach (var lnk in photoListLinks)
                    {
                        href = lnk.Attributes["href"].Value;
                        if (!IsContains(result, href))
                            result.Add(new HarvestLink(href, hrefDate, link));
                    }
                }
            }

            return result;
        }

        public int PageNumber(string link)
        {
            if (link.Contains("page"))
            {
                string[] content = link.Split('/');

                string value = content.Last().Replace(".html", "");

                return Convert.ToInt32(value) - 1;
            }
            else
                return 0;
        }

        private bool IsContains(List<HarvestLink> list, string value)
        {
            foreach (var item in list)
            {
                if (item.articleUrl == value)
                    return true;
            }
            return false;
        }
    }
}
