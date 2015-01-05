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
    public class PostSites
    {
        #region variables

        private string _host;

        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

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

        private TimeZoneInfo _timeZone;

        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
        }

        public string Status;

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

        //for collaboration between posting and windows
        public Window _currentWindow; // the window in which contains this action

        //for action
        public Action<Window, Label, int> updateStatus;

        //for labels
        public Label _statusLabel = new Label();

        #endregion

        #region constructors

        public PostSites()
        {

        }

        public PostSites(string host, string username, string password,PostSiteType postSiteType, TimeZoneInfo timeZone)
        {
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

        #region UtilityMethods

        public string GetHostName()
        {
            return new Uri(_host).Host;
        }

        public string GetConnectStatus()
        {
            if (_connect > 0)
                return "Connected!";
            else if (_connect == 0)
                return "Pending!";
            else
                return "Failed!";
        }

        public void SetConnectStatus(int value)
        {
            _connect = value;
        }

        public void InitializeWordpress()
        {
            _wordpress = new WordPressWrapper(_host + "/xmlrpc.php", _username, _decryptPassword);            
        }

        public void LoadCategories()
        {            
            tempList = _wordpress.GetCategories().Where(a => a.Name != "Uncategorized").ToList();
            ReloadCategories(0);
            tempList.Clear();
        }

        private void ReloadCategories(long parentID)
        {
            foreach (var cate in tempList)
            {
                if (cate.ParentCategoryID == parentID)
                {
                    _categories.Add(cate, parentID);
                    ReloadCategories(cate.CategoryID);
                }
            }
        }

        public Category FindPostSiteCategoryByName(string name)
        {
            foreach (var cate in _categories)
            {
                if (cate.Key.Name == name)
                    return cate.Key;
            }
            return null;
        }

        public Category FindPostSiteCategoryByID(long id)
        {
            foreach (var cate in _categories)
            {
                if (cate.Key.CategoryID == id)
                    return cate.Key;
            }
            return null;
        }

        public void ClonePostSites(PostSites newSite)
        {
            Host = newSite.Host;
            Username = newSite.Username;
            EncryptPassword = newSite.EncryptPassword;
            DecryptPassword = newSite.DecryptPassword;
            Chosen = true;
            _connect = 0;
            _type = newSite.Type;
            _timeZone = newSite.TimeZone;

            Status = "";            
            _categories = new Dictionary<Category, long>();
        }

        public string GetCategoryById(long Id)
        {
            string name = "";

            try
            {
                name = _wordpress.GetCategories().Where(a => a.CategoryID == Id).First().Name;
            }
            catch (Exception)
            {                
            }

            return name;
        }

        #endregion
    }
}
