using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class VirtualNewspapersPostRulesCollection
    {
        #region variables

        private List<VirtualNewspapersPostRule> _rules = new List<VirtualNewspapersPostRule>();

        public List<VirtualNewspapersPostRule> Rules
        {
            get { return _rules; }
        }

        #endregion

        #region constructors

        public VirtualNewspapersPostRulesCollection()
        {            

        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Add new VirtualNewspapersPostRule to collection
        /// </summary>
        /// <param name="rule"></param>
        public void AddRules(VirtualNewspapersPostRule rule)
        {
            if (!IsExistedRule(rule))
                _rules.Add(rule);
        }

        /// <summary>
        /// Check if VirtualNewspapersPostRule is existed or not
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private bool IsExistedRule(VirtualNewspapersPostRule check)
        {
            foreach (var rule in _rules)
                if (rule.SiteHost == check.SiteHost && rule.SiteCategoryID == check.SiteCategoryID && rule.VNSCategory == check.VNSCategory && rule.SiteHost == check.SiteHost)
                    return true;
            return false;
        }

        /// <summary>
        /// Check VirtualNewspapersPostRule is existed or not by VNS's category Id
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public bool CheckRuleExistBySource(long Id)
        {
            foreach (var rule in _rules)
                if (rule.VNSCategory == Id)
                    return true;
            return false;
        }

        /// <summary>
        /// Get list of VirtualNewspapersPostRules by Source's category Id
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public List<VirtualNewspapersPostRule> GetRulesBySourceCategory(long Id)
        {
            return _rules.GetAllRulesByVnsCategory(Id);
        }

        /// <summary>
        /// Get list of VirtualNewspapersPostRules by PostSite's category Id
        /// </summary>
        /// <param name="cate"></param>
        /// <returns></returns>
        public List<VirtualNewspapersPostRule> GetRulesByPostSiteCategory(long cate)
        {
            return _rules.GetAllRulesBySiteCategory(cate);
        }

        /// <summary>
        /// Get list of PostRules by Host name
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public List<VirtualNewspapersPostRule> GetRulesBySiteHost(string siteHost)
        {
            return _rules.GetAllRulesBySiteHost(siteHost);
        }

        /// <summary>
        /// Get list of PostRules by host name & categories
        /// </summary>
        /// <param name="host"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public List<VirtualNewspapersPostRule> GetRulesBySiteHostAndCategory(string siteHost, long Id)
        {
            List<VirtualNewspapersPostRule> temp = _rules.GetAllRulesBySiteHost(siteHost);

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

        /// <summary>
        /// Get all rules without duplicated
        /// </summary>
        /// <returns></returns>
        public List<VirtualNewspapersPostRule> GetAllDistinctRulesByVnsHostAndVnsCategories()
        {
            List<VirtualNewspapersPostRule> temp = new List<VirtualNewspapersPostRule>();

            foreach (var i in _rules)
            {
                if (!temp.Any(a => a.VNSHost == i.VNSHost && a.VNSCategory == i.VNSCategory))
                    temp.Add(i);
            }

            return temp;
        }

        #endregion
    }
}
