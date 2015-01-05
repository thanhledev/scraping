using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;

namespace DataTypes.Collections
{
    public static class RulesCollectionExtensionMethods
    {
        public static List<PostRule> GetAllRulesBySourceCategory(this List<PostRule> rules, string slug)
        {
            List<PostRule> tmp = new List<PostRule>();

            foreach (var rule in rules)
            {
                if (rule.SourceCategory == slug)
                    tmp.Add(rule);
            }

            return tmp;
        }        

        public static List<PostRule> GetAllRulesBySiteCategory(this List<PostRule> rules, long Id)
        {
            List<PostRule> tmp = new List<PostRule>();

            foreach (var rule in rules)
            {
                if (rule.SiteCategoryID == Id)
                    tmp.Add(rule);
            }

            return tmp;
        }

        public static List<PostRule> GetAllRulesBySiteHost(this List<PostRule> rules, string siteHost)
        {
            List<PostRule> tmp = new List<PostRule>();

            foreach (var rule in rules)
            {
                if (rule.SiteHost == siteHost)
                    tmp.Add(rule);
            }

            return tmp;
        }
    }
}
