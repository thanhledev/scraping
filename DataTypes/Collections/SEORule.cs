using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;
using DataTypes.Enums;
using DataTypes.Interfaces;

namespace DataTypes.Collections
{
    public class SEORule
    {
        #region variables

        private string _host;

        public string Host
        {
            get { return _host; }            
        }

        private ISEOPlugin _seoPlugin;

        public ISEOPlugin SeoPlugin
        {
            get { return _seoPlugin; }            
        }

        private SEOPluginType? _type;

        public SEOPluginType? Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private List<CategorySEORule> _categoryRules = new List<CategorySEORule>();

        public List<CategorySEORule> CategoryRules
        {
            get { return _categoryRules; }            
        }

        #endregion

        #region Constructors

        public SEORule(string host)
        {
            _host = host;
        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Create SEOPlugin for current host
        /// </summary>
        /// <param name="type"></param>
        public void CreateSEOPlugin(SEOPluginType? type)
        {
            _type = type;
            _seoPlugin = PluginFactory.CreatePlugin(type);            
        }

        /// <summary>
        /// Get CategorySEORule By its Id
        /// </summary>
        /// <param name="Id"></param>
        public CategorySEORule GetCategorySEORule(long Id)
        {
            foreach (var i in _categoryRules)
            {                
                if (i.CategoryID == Id)
                    return i;
            }
            return null;
        }

        ///// <summary>
        ///// Get CategorySEORule by its Id
        ///// </summary>
        ///// <param name="host"></param>
        ///// <param name="Id"></param>
        ///// <returns></returns>
        //public CategorySEORule GetCategorySEORule(string host, long Id)
        //{
        //    foreach (var i in _categoryRules)
        //    {
        //        if (i.CategoryID == Id && string.Compare(host, i.Host, true) == 0)
        //            return i;
        //    }
        //    return null;
        //}

        /// <summary>
        /// Add new CategorySEORule to list
        /// </summary>
        /// <param name="rule"></param>
        public void AddCategorySEORule(CategorySEORule rule)
        {
            _categoryRules.Add(rule);
        }

        /// <summary>
        /// Delete CategorySEORule via its host & Id
        /// </summary>
        /// <param name="host"></param>
        /// <param name="cateID"></param>
        public void DeleteCategorySEORule(string host, long cateID)
        {
            _categoryRules.RemoveAll(a => a.Host == host && a.CategoryID == cateID);
        }

        #endregion
    }
}
