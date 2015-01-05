using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Collections
{
    public class VirtualNewspapersPostRule
    {
        #region variables

        private string _vnsHost;

        public string VNSHost
        {
            get { return _vnsHost; }            
        }

        private long _vnsCategory;

        public long VNSCategory
        {
            get { return _vnsCategory; }
        }

        private string _siteHost;

        public string SiteHost
        {
            get { return _siteHost; }
        }

        private long _siteCategoryID;

        public long SiteCategoryID
        {
            get { return _siteCategoryID; }            
        }

        #endregion

        #region Constructors

        public VirtualNewspapersPostRule(string vnsHost, long vnsCate, string siteHost, long siteCateId)
        {
            _vnsHost = vnsHost;
            _vnsCategory = vnsCate;
            _siteHost = siteHost;
            _siteCategoryID = siteCateId;
        }

        #endregion

        #region UtilityMethods
      


        #endregion
    }
}
