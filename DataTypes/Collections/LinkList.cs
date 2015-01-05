using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Enums;
using System.IO;

namespace DataTypes.Collections
{
    public class LinkList
    {
        #region variable

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _keywordListName;

        public string KeywordListName
        {
            get { return _keywordListName; }
            set { _keywordListName = value; }
        }

        private int _apperanceNumber;

        public int ApperanceNumber
        {
            get { return _apperanceNumber; }
            set { _apperanceNumber = value; }
        }
       
        private List<string> _links = new List<string>();

        public List<string> Links
        {
            get { return _links; }           
        }

        #endregion

        #region Constructor

        public LinkList()
            : this("", "", 0)
        {
        }

        public LinkList(string name, string keywordList, int apperanceNumber)
        {
            _name = name;
            _keywordListName = keywordList;
            _apperanceNumber = apperanceNumber;            
        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Load all links of current link set
        /// </summary>
        /// <param name="content"></param>
        public void LoadLinks(string content)
        {
            _links = new List<string>();
            using (StringReader reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line) && !_links.Contains(line))
                    {
                        _links.Add(line.Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Get all links of current link set
        /// </summary>
        /// <returns>String: Links</returns>
        public string GetLinks()
        {
            string content = "";

            foreach (var i in _links)
            {
                content += i;
                content += "\n";
            }

            return content;
        }
       
        #endregion
    }
}
