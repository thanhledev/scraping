using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public static class VirtualNewspapersRulesCollectionExtensionMethods
    {
        public static List<VirtualNewspapersPostRule> GetAllRulesByVnsCategory(this List<VirtualNewspapersPostRule> rules, long Id)
        {
            List<VirtualNewspapersPostRule> tmp = new List<VirtualNewspapersPostRule>();

            foreach (var rule in rules)
            {
                if (rule.VNSCategory == Id)
                    tmp.Add(rule);
            }

            return tmp;
        }

        public static List<VirtualNewspapersPostRule> GetAllRulesBySiteCategory(this List<VirtualNewspapersPostRule> rules, long Id)
        {
            List<VirtualNewspapersPostRule> tmp = new List<VirtualNewspapersPostRule>();

            foreach (var rule in rules)
            {
                if (rule.SiteCategoryID == Id)
                    tmp.Add(rule);
            }

            return tmp;
        }

        public static List<VirtualNewspapersPostRule> GetAllRulesBySiteHost(this List<VirtualNewspapersPostRule> rules, string siteHost)
        {
            List<VirtualNewspapersPostRule> tmp = new List<VirtualNewspapersPostRule>();

            foreach (var rule in rules)
            {
                if (rule.SiteHost == siteHost)
                    tmp.Add(rule);
            }

            return tmp;
        }
    }
}
