using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;

namespace DataTypes.Collections
{
    public class PostRulesCollection
    {
        #region variables

        private List<PostRule> _rules = new List<PostRule>();

        public List<PostRule> Rules
        {
            get { return _rules; }
        }

        #endregion

        #region constructors

        public PostRulesCollection()
        {            

        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Add new PostRule to collection
        /// </summary>
        /// <param name="rule"></param>
        public void AddRules(PostRule rule)
        {
            if(!IsExistedRule(rule))
                _rules.Add(rule);
        }

        /// <summary>
        /// Check if PostRule is existed or not
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private bool IsExistedRule(PostRule check)
        {
            foreach (var rule in _rules)
                if (rule.SiteHost == check.SiteHost && rule.SiteCategoryID == check.SiteCategoryID && rule.SourceCategory == check.SourceCategory && rule.SourceHost == check.SourceHost)
                    return true;
            return false;
        }

        /// <summary>
        /// Check PostRule is existed or not by Source's category slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public bool CheckRuleExistBySource(string slug)
        {
            foreach (var rule in _rules)
                if (rule.SourceCategory == slug)
                    return true;
            return false;
        }

        /// <summary>
        /// Get list of PostRules by Source's category slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public List<PostRule> GetRulesBySourceCategory(string slug)
        {
            return _rules.GetAllRulesBySourceCategory(slug);
        }

        /// <summary>
        /// Get list of PostRules by PostSite's category Id
        /// </summary>
        /// <param name="cate"></param>
        /// <returns></returns>
        public List<PostRule> GetRulesByPostSiteCategory(long cate)
        {
            return _rules.GetAllRulesBySiteCategory(cate);
        }

        /// <summary>
        /// Get list of PostRules by Host name
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public List<PostRule> GetRulesBySiteHost(string siteHost)
        {
            return _rules.GetAllRulesBySiteHost(siteHost);
        }

        //public List<PostRule> GetRulesByHostAndCategory(string host, long cate)
        //{
        //    List<PostRule> temp = _rules.GetAllRulesByHost(host);

        //    return temp.GetAllRulesBySiteCategory(cate);
        //}

        /// <summary>
        /// Get list of PostRules by host name & categories
        /// </summary>
        /// <param name="host"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public List<PostRule> GetRulesBySiteHostAndCategory(string siteHost, long Id)
        {
            List<PostRule> temp = _rules.GetAllRulesBySiteHost(siteHost);

            return temp.GetAllRulesBySiteCategory(Id);
        }

        /// <summary>
        /// Remove PostRules based on host name
        /// </summary>
        /// <param name="host"></param>
        public void RemoveRules(string siteHost)
        {
            _rules.RemoveAll(a => a.SiteHost == siteHost);
        }

        /// <summary>
        /// Remove PostRules based on host name & category
        /// </summary>
        /// <param name="host"></param>
        /// <param name="cate"></param>
        public void RemoveRules(string siteHost, long cate)
        {
            _rules.RemoveAll(a => a.SiteHost == siteHost && a.SiteCategoryID == cate);
        }

        /// <summary>
        /// Get all post sites
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllPostSites()
        {
            List<string> temp = new List<string>();

            foreach (var i in _rules)
            {
                if (!temp.Contains(i.SiteHost))
                    temp.Add(i.SiteHost);
            }

            return temp;
        }        

        #endregion
    }
}
