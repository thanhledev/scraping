using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;
using System.Text.RegularExpressions;
using System.Threading;

namespace DataTypes
{
    public class RandomInserter : IInsert
    {
        Random rnd = new Random();

        public void InsertToBody(string body, List<string> sentences, List<string> keywords, LinkList linkList, ref List<SEOSentence> replacement, ref int appeared, ref List<string> insertedKeywords)
        {
            ////break body into sentences
            //List<string> sentences = new List<string>();

            //Regex rx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");
            //foreach (Match match in rx.Matches(body))
            //{
            //    sentences.Add(match.Value);
            //}

            if (sentences.Count > 5)
            {                
                string replaceSentence = RandomSentences(sentences, replacement);

                if (replaceSentence != "")
                {
                    string insertKeyword = RandomKeywords(keywords);

                    if (!insertedKeywords.Contains(insertKeyword))
                        insertedKeywords.Add(insertKeyword);

                    string insertLink = RandomLink(linkList.Links);
                    string insertValue = "";

                    if (linkList.ApperanceNumber > 0)
                    {
                        int fate = rnd.Next(0, 100);
                        if (fate >= 30)
                        {
                            if (appeared < linkList.ApperanceNumber)
                            {
                                insertValue = " <a href=\"" + insertLink + "\" target=\"_blank\">" + insertKeyword + "</a> ";
                                appeared++;
                            }
                            else
                                insertValue = " " + insertKeyword + " ";
                        }
                        else
                            insertValue = " " + insertKeyword + " ";
                    }
                    else
                    {
                        insertValue = " " + insertKeyword + " ";
                    }

                    SEOSentence newItem = new SEOSentence();

                    newItem._originalSentence = replaceSentence;
                    newItem._seoSentence = InsertLinkToSentence(insertValue, replaceSentence);

                    replacement.Add(newItem);
                }
            }
        }

        private string RandomKeywords(List<string> keywords)
        {            
            if (keywords.Count == 1)
                return keywords[0];
            else
            {                
                int pos = rnd.Next(0, keywords.Count);
                return keywords[pos];
            }
        }

        private string RandomLink(List<string> links)
        {            
            if (links.Count == 1)
                return links[0];
            else
            {                
                int pos = rnd.Next(0, links.Count);
                return links[pos];
            }
        }

        private string RandomSentences(List<string> sentences, List<SEOSentence> replacement)
        {
            string value = "";            
            if (sentences.Count == 1)
                return sentences[0];
            else
            {
                int foundTry = 0;
                do
                {
                    int pos = rnd.Next(sentences.Count);
                    value = sentences[pos];
                    if (!IsExistedSEOSentence(value, replacement))
                    {
                        if (value.Split().Length > 5)
                        {
                            break;
                        }
                        else
                        {
                            foundTry++;
                            continue;
                        }
                    }
                    else
                    {
                        foundTry++;
                        continue;
                    }

                } while (foundTry < 15);
                return value;
            }
        }

        private string InsertLinkToSentence(string insertValue, string sentence)
        {
            int chosenPos = rnd.Next(1, sentence.Count(Char.IsWhiteSpace) - 1);

            int pos = sentence.IndexOf(' ');

            int count = 0;
            while (pos >= 0 && count < chosenPos)
            {
                count++;
                pos = sentence.IndexOf(' ', pos + 1);
            }
            if (pos > 0)
                return sentence.Insert(pos, insertValue);
            else
                return sentence;
        }

        private bool IsExistedSEOSentence(string sentence, List<SEOSentence> replacement)
        {
            return replacement.Any(a => a._originalSentence == sentence);
        }
    }
}
