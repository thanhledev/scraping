using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;

namespace DataTypes.Collections
{
    public class SEORulesCollection
    {
        #region variables

        private List<SEORule> _rules = new List<SEORule>();

        public List<SEORule> Rules
        {
            get { return _rules; }            
        }

        #endregion

        #region Constructors

        public SEORulesCollection()
        {
        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Add site SEO Rule to collection
        /// </summary>
        /// <param name="rule"></param>
        public void AddRule(SEORule rule)
        {
            if (!IsExistedRule(rule))
                _rules.Add(rule);
        }

        /// <summary>
        /// Check whether rule is existed or not
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private bool IsExistedRule(SEORule check)
        {
            foreach (var rule in _rules)            
                if (rule.Host == check.Host)
                    return true;
            return false;
        }

        /// <summary>
        /// Get SEORule of site via its host 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public SEORule GetRule(string host)
        {
            foreach (var rule in _rules)
                if (rule.Host == host)
                    return rule;
            return null;
        }

        /// <summary>
        /// Delete SEORule of site via its host
        /// </summary>
        /// <param name="host"></param>
        public void DeleteRule(string host)
        {
            _rules.RemoveAll(a => a.Host == host);
        }

        /// <summary>
        /// Get CategorySEORule by its Id & Host
        /// </summary>
        /// <param name="host"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public CategorySEORule GetCategorySEORule(string host, long Id)
        {
            foreach (var rule in _rules)
            {
                if (string.Compare(rule.Host, host, true) == 0)
                    return rule.GetCategorySEORule(Id);
            }
            return null;
        }

        #endregion
    }
}
