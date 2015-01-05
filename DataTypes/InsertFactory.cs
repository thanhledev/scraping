using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;

namespace DataTypes
{
    public static class InsertFactory
    {
        public static IInsert CreateInserter(InsertLinkOptions options)
        {
            switch (options)
            {
                case InsertLinkOptions.Random:
                    return new RandomInserter();
                case InsertLinkOptions.MostRelevant:
                    return new MostRelevantInserter();
                case InsertLinkOptions.LastParagraph:
                    return new LastParagraphInserter();
                case InsertLinkOptions.FirstParagraph:
                    return new FirstParagraphInserter();
                default:
                    return new NullInserter();
            };
        }
    }
}
