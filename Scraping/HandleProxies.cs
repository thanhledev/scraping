using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections;
using DataTypes.Collections;
using DataTypes.Enums;
using System.Net;

namespace Scraping
{
    public sealed class HandleProxies
    {
        #region variables

        private static HandleProxies _instance = null;
        private static readonly object _locker = new object();
        private static readonly object _locker1 = new object();
        private static string _server = "http://bongda.com.vn"; //google test server https://www.google.com/search?btnG=1&filter=0&start=0&q=jack+the+ripper
        
        private bool _isRunning;
        
        public bool IsRunning
        {
            get { return _isRunning; }
            internal set { _isRunning = value; }
        }

        private BackgroundWorker[] _worker = new BackgroundWorker[20];
        
        public int Total;        

        private List<SystemProxy> _alive;

        public List<SystemProxy> Alive
        {
            get { return _alive; }            
        }

        private ProxyTestType _testType;

        private int _finishWorker;        
        
        /// <summary>
        /// Window & control variables
        /// </summary>
        private Window _currentWD;

        public Window CurrentWD
        {
            get { return _currentWD; }
            set { _currentWD = value; }
        }

        private TextBox _proxyTB = new TextBox();

        public TextBox ProxyTB
        {
            get { return _proxyTB; }
            set { _proxyTB = value; }
        }

        private TextBox _aliveProxy = new TextBox();

        public TextBox AliveProxy
        {
            get { return _aliveProxy; }
            set { _aliveProxy = value; }
        }

        private TextBox _totalProxy = new TextBox();

        public TextBox TotalProxy
        {
            get { return _totalProxy; }
            set { _totalProxy = value; }
        }

        /// <summary>
        /// Action list
        /// </summary>        
        public Action<Window, TextBox, Queue<SystemProxy>> _updateProxyTable;
        public Action<Window, TextBox, TextBox, string, string> _updateStatistics;

        #endregion

        #region Constructors

        public static HandleProxies Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new HandleProxies();
                    }
                }
                return _instance;
            }
        }

        HandleProxies()
        {
            Total = 0;
            _isRunning = false;
            _alive = new List<SystemProxy>();
            _finishWorker = 0;
            for (int i = 0; i < 20; i++)
            {
                _worker[i] = new BackgroundWorker();
                _worker[i].WorkerSupportsCancellation = true;
                _worker[i].DoWork += new DoWorkEventHandler(worker_DoWork);
                _worker[i].RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            }
        }

        #endregion

        #region worker

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Action<Window, TextBox, string> updateStatus = e.Argument as Action<Window, TextBox, string>;

            switch (_testType)
            {
                case ProxyTestType.Public:
                    while (IsRunning)
                    {
                        SystemProxy currentProxy = new SystemProxy();
                        lock (_locker)
                        {
                            if (SysSettings.Instance.Proxies.Count > 0)
                                currentProxy = SysSettings.Instance.Proxies.Dequeue();
                            else
                                break;
                        }
                        if (currentProxy.Ip != null && currentProxy.Port != null)
                        {
                            try
                            {
                                HttpWebRequest request = WebRequest.Create(_server) as HttpWebRequest;
                                WebProxy p = new WebProxy(currentProxy.Ip, Convert.ToInt32(currentProxy.Port));
                                request.Proxy = p;
                                request.Timeout = 12000;
                                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                WebHeaderCollection header = response.Headers;

                                var encoding = ASCIIEncoding.ASCII;
                                string responseText = "";
                                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                                {
                                    responseText = reader.ReadToEnd();
                                }
                                //if (responseText.Contains("schema.org")) //only for google pass
                                if (responseText != string.Empty)
                                {
                                    lock (_locker)
                                    {
                                        FilterProxies(currentProxy);                                        
                                        updateStatus.Invoke(_currentWD, _aliveProxy, _alive.Count.ToString());
                                    }
                                }                                
                            }
                            catch (WebException ex)
                            {                                
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                case ProxyTestType.Private:
                    while (IsRunning)
                    {
                        SystemProxy currentProxy = new SystemProxy();
                        lock (_locker)
                        {
                            if (SysSettings.Instance.PrivateProxies.Count > 0)
                                currentProxy = SysSettings.Instance.PrivateProxies.Dequeue();
                            else
                                break;
                        }
                        if (currentProxy.Ip != null && currentProxy.Port != null)
                        {
                            try
                            {
                                HttpWebRequest request = WebRequest.Create(_server) as HttpWebRequest;
                                WebProxy p = new WebProxy(currentProxy.Ip, Convert.ToInt32(currentProxy.Port));
                                p.Credentials = new NetworkCredential(currentProxy.Username, currentProxy.Password);
                                request.Proxy = p;
                                request.Timeout = 12000;
                                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                WebHeaderCollection header = response.Headers;

                                var encoding = ASCIIEncoding.ASCII;
                                string responseText = "";
                                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                                {
                                    responseText = reader.ReadToEnd();
                                }
                                //if (responseText.Contains("schema.org")) //only for google pass
                                if (responseText != string.Empty)
                                {
                                    lock (_locker)
                                    {
                                        FilterProxies(currentProxy);
                                        updateStatus.Invoke(_currentWD, _aliveProxy, _alive.Count.ToString());
                                    }
                                }
                            }
                            catch (WebException ex)
                            {
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
            };
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lock (_locker1)
            {
                _finishWorker++;
            }
            if (_finishWorker == 20)
            {
                UpdateProxies();
                if (_testType == ProxyTestType.Public)
                {
                    _updateProxyTable.Invoke(_currentWD, _proxyTB, SysSettings.Instance.Proxies);
                    _updateStatistics.Invoke(_currentWD, _totalProxy, _aliveProxy, SysSettings.Instance.Proxies.Count.ToString(), SysSettings.Instance.Proxies.Count.ToString());
                }
                else
                {
                    _updateProxyTable.Invoke(_currentWD, _proxyTB, SysSettings.Instance.PrivateProxies);
                    _updateStatistics.Invoke(_currentWD, _totalProxy, _aliveProxy, SysSettings.Instance.PrivateProxies.Count.ToString(), SysSettings.Instance.PrivateProxies.Count.ToString());
                }
                _isRunning = false;
            }
        }

        #endregion

        #region UtilityMethods

        private void FilterProxies(SystemProxy proxy)
        {
            _alive.Add(proxy);
        }

        private void UpdateProxies()
        {
            switch(_testType)
            {
                case ProxyTestType.Public:
                    SysSettings.Instance.Proxies.Clear();
                    foreach (var i in _alive)
                    {
                        SysSettings.Instance.Proxies.Enqueue(i);
                    }
                    _alive = new List<SystemProxy>();
                    break;
                case ProxyTestType.Private:
                    SysSettings.Instance.PrivateProxies.Clear();
                    foreach (var i in _alive)
                    {
                        SysSettings.Instance.PrivateProxies.Enqueue(i);
                    }
                    _alive = new List<SystemProxy>();                    
                    break;
            }
        }
        
        public void RunHandle(ProxyTestType testType, Window wd, TextBox totalTextBox, TextBox aliveTextBox, TextBox proxyTable, Action<Window, TextBox, string> updateAliveProxy, Action<Window, TextBox, TextBox, string, string> updateStatistics, Action<Window, TextBox, Queue<SystemProxy>> updateProxyTable)
        {
            _testType = testType;
            _currentWD = wd;
            _totalProxy = totalTextBox;
            _aliveProxy = aliveTextBox;
            _proxyTB = proxyTable;
            _updateStatistics = updateStatistics;
            _updateProxyTable = updateProxyTable;
            _isRunning = true;
            for (int i = 0; i < 20; i++)
            {
                _worker[i].RunWorkerAsync(updateAliveProxy);
            }
        }

        public void CancelHandle()
        {
            for (int i = 0; i < 20; i++)
            {
                _worker[i].CancelAsync();
            }
        }

        #endregion
    }
}
