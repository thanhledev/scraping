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
using DataTypes.Enums;
using System.Net;
using System.Web;
using System.IO;

namespace DataTypes.Collections
{
    public class IndexerService
    {
        #region variables

        private string _serviceName;

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }

        private string _serviceAPI;

        public string ServiceAPI
        {
            get { return _serviceAPI; }
            set { _serviceAPI = value; }
        }

        private string _encryptPassword;

        public string EncryptPassword
        {
            get { return _encryptPassword; }
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

        //for collaboration between posting and windows
        public Window _currentWindow; // the window in which contains this action

        //for action
        public Action<Window, Label, bool> updateStatus;

        //for labels
        public Label _statusLabel = new Label();

        #endregion

        #region constructors

        public IndexerService()
        {
        }

        public IndexerService(string name, string api, string password, bool chosen)
        {
            _serviceName = name;
            _serviceAPI = api;
            _encryptPassword = password;
            _decryptPassword = "";
            _chosen = chosen;
        }

        #endregion

        #region utility methods

        public void IndexUrls(List<string> links)
        {
            if (links.Count > 0)
            {
                if (!_serviceName.Contains("instantlinkindexer"))
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_serviceAPI);
                        ASCIIEncoding encoding = new ASCIIEncoding();

                        string postData = "key=" + _decryptPassword;
                        postData += "&links=";

                        foreach (var lnk in links)
                        {
                            postData += lnk + "\n";
                        }

                        byte[] data = encoding.GetBytes(postData);

                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = postData.Length;

                        using (Stream stream = request.GetRequestStream())
                        {
                            stream.Write(data, 0, data.Length);
                        }

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_serviceAPI);
                        ASCIIEncoding encoding = new ASCIIEncoding();

                        string postData = "apikey=" + HttpUtility.HtmlEncode(_decryptPassword);
                        postData += "&cmd=submit";

                        postData += "&urls=" + HttpUtility.HtmlEncode(string.Join("|", links));

                        byte[] data = encoding.GetBytes(postData);

                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = postData.Length;

                        using (Stream stream = request.GetRequestStream())
                        {
                            stream.Write(data, 0, data.Length);
                        }

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        public string GetChosenStatus()
        {
            if (_chosen)
                return "Chosen";
            else
                return "None!";
        }

        public void UpdateService(string api, string ePassword, string dPassword, bool chosen)
        {
            _serviceAPI = api;
            _decryptPassword = dPassword;
            _encryptPassword = ePassword;
            _chosen = chosen;
        }

        #endregion
    }
}
