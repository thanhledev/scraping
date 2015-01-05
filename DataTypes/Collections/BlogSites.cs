using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using DataTypes.Enums;
using JoeBlogs;

namespace DataTypes.Collections
{
    public class BlogSites
    {
        #region variables

        //wordpress.com attributes
        private string _host;

        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

        private TimeZoneInfo _timeZone;

        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
        }

        private List<Category> tempList = new List<Category>();

        private WordPressWrapper _wordpress;

        public WordPressWrapper Wordpress
        {
            get { return _wordpress; }
        }

        private Dictionary<Category, long> _categories = new Dictionary<Category, long>();

        public Dictionary<Category, long> Categories
        {
            get { return _categories; }
            set { _categories = value; }
        }

        //blogger attributes
        private string _blogId;

        public string BlogId
        {
            get { return _blogId; }
            set { _blogId = value; }
        }

        private string _applicationName;

        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        //common attributes
        private string _username;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _encryptPassword;

        public string EncryptPassword
        {
            get { return _encryptPassword; }
            set { _encryptPassword = value; }
        }

        private string _decryptPassword;

        public string DecryptPassword
        {
            get { return _decryptPassword; }
            set { _decryptPassword = value; }
        }

        private bool _chosen;

        public bool Chosen
        {
            get { return _chosen; }
            set { _chosen = value; }
        }

        private int _connect;

        public int Connect
        {
            get { return _connect; }
        }

        private PostSiteType _type;

        public PostSiteType Type
        {
            get { return _type; }
        }

        public string Status;

        //for collaboration between posting and windows
        public Window _currentWindow; // the window in which contains this action

        //for action
        public Action<Window, Label, int> updateStatus;

        //for labels
        public Label _statusLabel = new Label();

        #endregion

        #region constructors

        public BlogSites(string blogId, string applicationName, string username, string password, PostSiteType postSiteType, TimeZoneInfo timeZone)
            :this(blogId,applicationName,"",username,password,postSiteType,timeZone)
        {
            
        }

        public BlogSites(string host, string username, string password, PostSiteType postSiteType, TimeZoneInfo timeZone)
            : this("", "", host, username, password, postSiteType, timeZone)
        {
            
        }

        public BlogSites(string blogId, string applicationName, string host, string username, string password, PostSiteType postSiteType, TimeZoneInfo timeZone)
        {
            BlogId = blogId;
            ApplicationName = applicationName;
            Host = host;
            Username = username;
            EncryptPassword = password;
            DecryptPassword = "";
            Chosen = true;
            _connect = 0;
            _type = postSiteType;
            _timeZone = timeZone;
        }

        #endregion
    }
}
