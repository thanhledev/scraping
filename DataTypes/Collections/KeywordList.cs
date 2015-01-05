using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataTypes.Enums;
using System.IO;

namespace DataTypes.Collections
{
    public class KeywordList
    {
        #region Variables

        private string _name;

        public string Name
        {
            get { return _name; }
        }
        private KeywordType _keywordType;

        public KeywordType KeywordType
        {
            get { return _keywordType; }
            set { _keywordType = value; }
        }

        private InsertLinkOptions _insertOpt;

        public InsertLinkOptions InsertOpt
        {
            get { return _insertOpt; }
            set { _insertOpt = value; }
        }

        private List<string> _keywords;

        public List<string> Keywords
        {
            get { return _keywords; }
        }

        #endregion

        #region Constructors

        public KeywordList()
            : this("", KeywordType.Generic, InsertLinkOptions.Random)
        {
            _keywords = new List<string>();
        }

        public KeywordList(string name, KeywordType type, InsertLinkOptions insert)
        {
            _name = name;
            _keywordType = type;
            _insertOpt = insert;
            _keywords = new List<string>();
        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Add keywords by content
        /// </summary>
        /// <param name="content"></param>
        public void AddKeywords(string content)
        {
            using (StringReader reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line) && !_keywords.Contains(line))
                    {
                        _keywords.Add(line.Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Add keyword per word
        /// </summary>
        /// <param name="keyword"></param>
        public void AddPerKeyword(string keyword)
        {
            if (!_keywords.Contains(keyword))
                _keywords.Add(keyword.Trim());
        }

        #endregion
    }
}
