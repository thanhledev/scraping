using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Collections;
using DataTypes.Enums;

namespace DataTypes.Interfaces
{
    public interface IInsert
    {
        void InsertToBody(string body, List<string> sentences, List<string> keywords, LinkList linkList, ref List<SEOSentence> replacement, ref int appeared, ref List<string> insertedKeywords);
    }
}
