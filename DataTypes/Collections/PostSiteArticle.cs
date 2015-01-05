using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;

namespace DataTypes.Collections
{
    public class PostSiteArticle
    {
        private string _siteHost;

        public string SiteHost
        {
            get { return _siteHost; }
            set { _siteHost = value; }
        }

        private long _siteCateId;

        public long SiteCateId
        {
            get { return _siteCateId; }
            set { _siteCateId = value; }
        }

        private Post _post;

        public Post Post
        {
            get { return _post; }
            set { _post = value; }
        }

        public PostSiteArticle(string host, long cateId,Post post)
        {
            _siteHost = host;
            _siteCateId = cateId;
            _post = post;
        }
    }
}
