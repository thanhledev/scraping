using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;
using System.Text.RegularExpressions;

namespace DataTypes
{
    public class MostRelevantInserter : IInsert
    {
        public void InsertToBody(string body, List<string> sentences, List<string> keywords, LinkList linkList, ref List<SEOSentence> replacement, ref int appeared, ref List<string> insertedKeywords)
        {
            //not yet be implemented
        }
    }
}
