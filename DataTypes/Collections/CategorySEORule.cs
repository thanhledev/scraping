using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;
using DataTypes.Enums;
using DataTypes.Interfaces;
using System.IO;

namespace DataTypes.Collections
{
    public class CategorySEORule
    {
        #region variables

        private string _host;

        public string Host
        {
            get { return _host; }            
        }

        private long _categoryID;

        public long CategoryID
        {
            get { return _categoryID; }            
        }

        private int _totalKeywords;

        public int TotalKeywords
        {
            get { return _totalKeywords; }            
        }

        private int _primaryKeywordPercentage;

        public int PrimaryKeywordPercentage
        {
            get { return _primaryKeywordPercentage; }            
        }        

        private int _secondaryKeywordPercentage;

        public int SecondaryKeywordPercentage
        {
            get { return _secondaryKeywordPercentage; }            
        }        

        private int _genericKeywordPercentage;

        public int GenericKeywordPercentage
        {
            get { return _genericKeywordPercentage; }
        }

        private List<KeywordList> _keywordList = new List<KeywordList>();

        public List<KeywordList> KeywordList
        {
            get { return _keywordList; }
        }

        private List<LinkList> _categoryLinkList = new List<LinkList>();

        public List<LinkList> CategoryLinkList
        {
            get { return _categoryLinkList; }            
        }

        private bool _insertAuthorityLinks;

        public bool InsertAuthorityLinks
        {
            get { return _insertAuthorityLinks; }
            set { _insertAuthorityLinks = value; }
        }

        private AuthorityKeywords _authorityKeywords;

        public AuthorityKeywords AuthorityKeywords
        {
            get { return _authorityKeywords; }
            set { _authorityKeywords = value; }
        }

        private AuthoritySearchOptions _authoritySearch;

        public AuthoritySearchOptions AuthoritySearch
        {
            get { return _authoritySearch; }
            set { _authoritySearch = value; }
        }

        private AuthorityApperance _authorityApperance;

        public AuthorityApperance AuthorityApperance
        {
            get { return _authorityApperance; }
            set { _authorityApperance = value; }
        }

        private bool _insertVideo;

        public bool InsertVideo
        {
            get { return _insertVideo; }
            set { _insertVideo = value; }
        }

        private AuthorityKeywords _videoKeywords;

        public AuthorityKeywords VideoKeywords
        {
            get { return _videoKeywords; }
            set { _videoKeywords = value; }
        }

        private AuthoritySearchOptions _videoSearch;

        public AuthoritySearchOptions VideoSearch
        {
            get { return _videoSearch; }
            set { _videoSearch = value; }
        }

        private bool _insertInternalLink;

        public bool InsertInternalLink
        {
            get { return _insertInternalLink; }
            set { _insertInternalLink = value; }
        }

        private AuthorityKeywords _internalKeywords;

        public AuthorityKeywords InternalKeywords
        {
            get { return _internalKeywords; }
            set { _internalKeywords = value; }
        }

        #endregion

        #region Constructors

        public CategorySEORule(string host,long Id)
        {
            _host = host;
            _categoryID = Id;
            _totalKeywords = _primaryKeywordPercentage = _secondaryKeywordPercentage = _genericKeywordPercentage = 0;
        }

        #endregion

        #region UtilityMethods
       
        /// <summary>
        /// Change the primary keyword appearance percentage
        /// </summary>
        /// <param name="value"></param>
        public void ChangePrimaryKeywordPercentage(int value)
        {
            _primaryKeywordPercentage = value;
        }        

        /// <summary>
        /// Change the primary keyword appearance percentage
        /// </summary>
        /// <param name="value"></param>
        public void ChangeSecondaryKeywordPercentage(int value)
        {
            _secondaryKeywordPercentage = value;
        }        

        /// <summary>
        /// Change the primary keyword appearance percentage
        /// </summary>
        /// <param name="value"></param>
        public void ChangeGenericKeywordPercentage(int value)
        {
            _genericKeywordPercentage = value;
        }

        /// <summary>
        /// Change the number of total keywords
        /// </summary>
        /// <param name="value"></param>
        public void ChangeTotalKeyword(int value)
        {
            _totalKeywords = value;
        }

        /// <summary>
        /// Check if keywordList already existed 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckKeywordListName(string name)
        {
            return _keywordList.Any(a => string.Compare(a.Name, name, true) == 0);
        }

        /// <summary>
        /// Add new keywordList to current List
        /// </summary>
        /// <param name="link"></param>
        public void AddKeywordList(KeywordList list)
        {
            if (!CheckKeywordListName(list.Name))
                _keywordList.Add(list);
        }

        /// <summary>
        /// Delete keywordList based on its name
        /// </summary>
        /// <param name="name"></param>
        public void DeleteKeywordListByName(string name)
        {
            _keywordList.RemoveAll(a => string.Compare(a.Name, name, true) == 0);
        }

        /// <summary>
        /// Get keywordList by its name
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public KeywordList GetKeywordListByName(string name)
        {
            return _keywordList.Where(a => string.Compare(a.Name, name, true) == 0).First();
        }

        /// <summary>
        /// Get keywordList by keywordType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<KeywordList> GetKeywordListByType(KeywordType type)
        {
            return _keywordList.Where(a => a.KeywordType == type).ToList();
        }

        /// <summary>
        /// Return linklist by its name
        /// </summary>
        /// <param name="name">Name of the linklist</param>
        /// <returns>Linklist has the same name</returns>
        public LinkList GetLinkListByName(string name)
        {
            foreach (var link in _categoryLinkList)
            {
                if (link.Name == name)
                    return link;
            }
            return null;
        }

        /// <summary>
        /// Check if linklist already existed
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <returns>True or false</returns>
        public bool CheckLinkListExisted(string name)
        {
            return _categoryLinkList.Any(a => string.Compare(a.Name,name,true) == 0);
        }

        /// <summary>
        /// Add new linklist to current List LinkList
        /// </summary>
        /// <param name="link"></param>
        public void AddLinkList(LinkList link)
        {
            _categoryLinkList.Add(link);
        }

        /// <summary>
        /// Handler Delete LinkList via its name
        /// </summary>
        /// <param name="name"></param>
        public void DeleteLinkListByName(string name)
        {
            _categoryLinkList.RemoveAll(a => a.Name == name);
        }

        public void DeleteLinkListByKeywordList(string name)
        {
            _categoryLinkList.RemoveAll(a => a.KeywordListName == name);
        }

        /// <summary>
        /// Return linklist by its KeywordListName
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LinkList GetLinkListByKeywordListName(string name)
        {
            LinkList temp = new LinkList();

            foreach (var link in _categoryLinkList)
            {
                if (link.KeywordListName == name)
                    return link;
            }

            return temp;
        }

        #endregion
    }
}
