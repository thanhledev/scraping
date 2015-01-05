using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoeBlogs;

namespace DataTypes.Collections
{
    public class VNSPost
    {
        private string _vnsHost;

        public string VnsHost
        {
            get { return _vnsHost; }
            set { _vnsHost = value; }
        }

        private long _vnsCateId;

        public long VnsCateId
        {
            get { return _vnsCateId; }
            set { _vnsCateId = value; }
        }

        private Post _post;

        public Post Post
        {
            get { return _post; }
            set { _post = value; }
        }

        public VNSPost(string vnsHost, long cateId, Post post)
        {
            _vnsHost = vnsHost;
            _vnsCateId = cateId;
            _post = post;
        }
    }
}
