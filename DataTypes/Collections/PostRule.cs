using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;
namespace DataTypes.Collections
{
    public class PostRule
    {
        #region variables

        private string _sourceHost;

        public string SourceHost
        {
            get { return _sourceHost; }            
        }

        private string _sourceCategory;

        public string SourceCategory
        {
            get { return _sourceCategory; }
        }
        
        private long _siteCategoryID;

        public long SiteCategoryID
        {
            get { return _siteCategoryID; }            
        }

        private string _siteHost;

        public string SiteHost
        {
            get { return _siteHost; }            
        }

        #endregion

        #region Constructors

        public PostRule(string sourceHost, string sourceCate, long siteCateId, string siteHost)
        {
            _sourceHost = sourceHost;
            _sourceCategory = sourceCate;
            _siteHost = siteHost;
            _siteCategoryID = siteCateId;
        }

        #endregion

        #region UtilityMethods
      


        #endregion
    }
}
