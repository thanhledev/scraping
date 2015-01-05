using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Enums;

namespace DataTypes.Collections
{
    public class KeywordPackage
    {
        private KeywordList _keywordList;

        public KeywordList KeywordList
        {
            get { return _keywordList; }            
        }

        private LinkList _linkList;

        public LinkList LinkList
        {
            get { return _linkList; }            
        }

        private int _linkAppeared;

        public int LinkAppeared
        {
            get { return _linkAppeared; }
            set { _linkAppeared = value; }
        }

        public KeywordPackage()
            : this(null, null, 0)
        {
        }

        public KeywordPackage(KeywordList kList, LinkList lList, int value)
        {
            _keywordList = kList;
            _linkList = lList;
            _linkAppeared = value;
        }
    }
}
