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
using System.Globalization;
using JoeBlogs;

namespace DataTypes
{
    public class ByTagInternalInserter
    {
        Random rnd = new Random();

        public void InsertInternalLink(AuthorityKeywords tagType, ref string htmlBody, List<string> articleTags, List<string> manualTags, WordPressWrapper wordpress, List<string> agents)
        {
            if (tagType == AuthorityKeywords.ArticleTags)
            {
                if (articleTags.Count > 0)
                {
                    int count = 0;

                    string chosenTag = "";
                    string relatedLink = "";

                    do
                    {
                        chosenTag = articleTags[rnd.Next(articleTags.Count)];

                        int appeareance = 0;

                        CompareInfo sampleInfo = CultureInfo.InvariantCulture.CompareInfo;

                        int pos = sampleInfo.IndexOf(htmlBody, chosenTag, CompareOptions.IgnoreCase);

                        while (pos > 0)
                        {
                            appeareance++;
                            pos = sampleInfo.IndexOf(htmlBody, chosenTag, pos + 1, CompareOptions.IgnoreCase);
                        }

                        if (appeareance > 0)
                            break;
                        else
                        {
                            chosenTag = "";
                            count++;
                        }

                    } while (count < 20);

                    if (chosenTag != "")
                    {
                        string wordpressTag = chosenTag.Trim();

                        List<Tag> relatedTags = wordpress.GetTagsByName(wordpressTag).ToList();

                        if (relatedTags.Count > 0)
                        {
                            Tag chosenWordpressTag = relatedTags[rnd.Next(relatedTags.Count)];

                            List<Post> relatedPostsByChosenWordpressTag = wordpress.GetPostsByTag(20, chosenWordpressTag.Slug).ToList();

                            if (relatedPostsByChosenWordpressTag.Count > 0)
                            {
                                relatedLink = relatedPostsByChosenWordpressTag[rnd.Next(relatedPostsByChosenWordpressTag.Count)].Permalink;

                                string relatedAnchor = "<a href=\"" + relatedLink + "\" target=\"_blank\">" + chosenTag + "</a>";

                                htmlBody = ReplaceRandom(htmlBody, chosenTag, relatedAnchor);
                            }
                        }
                    }
                }
            }
            else
            {
                //not yet be implemented
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
    }
}
