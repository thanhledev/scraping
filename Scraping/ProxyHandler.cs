﻿using System;
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
using System.Net;
using System.IO;
using Utilities;
using HtmlAgilityPack;
using DataTypes;
using DataTypes.Interfaces;
using DataTypes.Collections;
using DataTypes.Enums;

namespace Scraping
{
    public sealed class ProxyHandler
    {
        #region variables

        //for own object
        private static ProxyHandler _instance = null;
        private static readonly object _instanceLocker = new object();
        private static readonly object _loadingLocker = new object();
        private static readonly object _usingLocker = new object();
        private static readonly object _foundLocker = new object();
        private static readonly object _stopLocker = new object();
        private static readonly object _finishProxyManagerLocker = new object();
        Random pRnd = new Random();
        //for working variables
        private static string _defaultValue = "???";
        private bool _autoSearchProxy;

        public bool AutoSearchProxy
        {
            get { return _autoSearchProxy; }
            set { _autoSearchProxy = value; }
        }

        private int _searchProxyInterval;

        public int SearchProxyInterval
        {
            get { return _searchProxyInterval; }
            set { _searchProxyInterval = value; }
        }

        private bool _testProxy;

        public bool TestProxy
        {
            get { return _testProxy; }
            set { _testProxy = value; }
        }

        private bool _checkAnonymous;

        public bool CheckAnonymous
        {
            get { return _checkAnonymous; }
            set { _checkAnonymous = value; }
        }

        private string _checkAnonymousLink;

        public string CheckAnonymousLink
        {
            get { return _checkAnonymousLink; }
            set { _checkAnonymousLink = value; }
        }

        private int _threads;

        public int Threads
        {
            get { return _threads; }
            set { _threads = value; }
        }

        private int _timeOut;

        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        private ProxyType _scrapeHarvestLinks;

        public ProxyType ScrapeHarvestLinks
        {
            get { return _scrapeHarvestLinks; }
            set { _scrapeHarvestLinks = value; }
        }

        private ProxyType _scrapeDownloadArticles;

        public ProxyType ScrapeDownloadArticles
        {
            get { return _scrapeDownloadArticles; }
            set { _scrapeDownloadArticles = value; }
        }

        private ProxyType _postingHarvestLinks;

        public ProxyType PostingHarvestLinks
        {
            get { return _postingHarvestLinks; }
            set { _postingHarvestLinks = value; }
        }

        private ProxyType _postingDownloadArticles;

        public ProxyType PostingDownloadArticles
        {
            get { return _postingDownloadArticles; }
            set { _postingDownloadArticles = value; }
        }

        private ProxyType _vnsDownloadArticles;

        public ProxyType VnsDownloadArticles
        {
            get { return _vnsDownloadArticles; }
            set { _vnsDownloadArticles = value; }
        }

        private static readonly object _dequeueLocker = new object();
        private static readonly object _enqueueLocker = new object();
        private static readonly object _finishLocker = new object();
        private static string _testServer = "http://bongda.com.vn";
        private static string _googleTestServer = "https://www.google.com/search?btnG=1&filter=0&start=0&q=jack+the+ripper";

        private Queue<string> _cloneProxyFeeder = new Queue<string>();
        public List<string> _publicProxyFeeder =
            new List<string>
            {
                "http://aliveproxy.com/high-anonymity-proxy-list/",
                "http://aliveproxy.com/us-proxy-list/",
                "http://aliveproxy.com/ru-proxy-list/",
                "http://aliveproxy.com/jp-proxy-list/",
                "http://aliveproxy.com/ca-proxy-list/",
                "http://aliveproxy.com/fr-proxy-list/",
                "http://aliveproxy.com/gb-proxy-list/",
                "http://aliveproxy.com/de-proxy-list/",
                "http://aliveproxy.com/anonymous-proxy-list/",
                "http://aliveproxy.com/transparent-proxy-list/",
                "http://checkerproxy.net/all_proxy",
                "http://www.cool-tests.com/Azerbaijan-proxy.php",
                "http://www.cool-tests.com/Albania-proxy.php",
                "http://www.cool-tests.com/Algeria-proxy.php",
                "http://www.cool-tests.com/United-States-proxy.php",
                "http://www.cool-tests.com/United-Kingdom-proxy.php",
                "http://www.cool-tests.com/Netherlands-Antilles-proxy.php",
                "http://www.cool-tests.com/United-Arab-Emirates-proxy.php",
                "http://www.cool-tests.com/Argentina-proxy.php",
                "http://www.cool-tests.com/South-Africa-proxy.php",
                "http://www.cool-tests.com/Bangladesh-proxy.php",
                "http://www.cool-tests.com/Belarus-proxy.php",
                "http://www.cool-tests.com/Belgium-proxy.php",
                "http://www.cool-tests.com/Bulgaria-proxy.php",
                "http://www.cool-tests.com/Brazil-proxy.php",
                "http://www.cool-tests.com/Hungary-proxy.php",
                "http://www.cool-tests.com/Venezuela-proxy.php",
                "http://www.cool-tests.com/VietNam-proxy.php",
                "http://www.cool-tests.com/Ghana-proxy.php",
                "http://www.cool-tests.com/Guatemala-proxy.php",
                "http://www.cool-tests.com/Germany-proxy.php",
                "http://www.cool-tests.com/Netherlands-proxy.php",
                "http://www.cool-tests.com/Hong-Kong-proxy.php",
                "http://www.cool-tests.com/Honduras-proxy.php",
                "http://www.cool-tests.com/Greece-proxy.php",
                "http://www.cool-tests.com/Georgia-proxy.php",
                "http://www.cool-tests.com/Denmark-proxy.php",
                "http://www.cool-tests.com/Europe-proxy.php",
                "http://www.cool-tests.com/Egypt-proxy.php",
                "http://www.cool-tests.com/Israel-proxy.php",
                "http://www.cool-tests.com/India-proxy.php",
                "http://www.cool-tests.com/Indonesia-proxy.php",
                "http://www.cool-tests.com/Iraq-proxy.php",
                "http://www.cool-tests.com/Iran-proxy.php",
                "http://www.cool-tests.com/Ireland-proxy.php",
                "http://www.cool-tests.com/Spain-proxy.php",
                "http://www.cool-tests.com/Italy-proxy.php",
                "http://www.cool-tests.com/Kazakhstan-proxy.php",
                "http://www.cool-tests.com/Cambodia-proxy.php",
                "http://www.cool-tests.com/Canada-proxy.php",
                "http://www.cool-tests.com/Kenya-proxy.php",
                "http://www.cool-tests.com/China-proxy.php",
                "http://www.cool-tests.com/Colombia-proxy.php",
                "http://www.cool-tests.com/Costa-Rica-proxy.php",
                "http://www.cool-tests.com/Latvia-proxy.php",
                "http://www.cool-tests.com/Lebanon-proxy.php",
                "http://www.cool-tests.com/Lithuania-proxy.php",
                "http://www.cool-tests.com/Luxembourg-proxy.php",
                "http://www.cool-tests.com/Macedonia-proxy.php",
                "http://www.cool-tests.com/Malaysia-proxy.php",
                "http://www.cool-tests.com/Maldives-proxy.php",
                "http://www.cool-tests.com/Mexico-proxy.php",
                "http://www.cool-tests.com/Moldova-proxy.php",
                "http://www.cool-tests.com/Mongolia-proxy.php",
                "http://www.cool-tests.com/Myanmar-proxy.php",
                "http://www.cool-tests.com/Nepal-proxy.php",
                "http://www.cool-tests.com/Nigeria-proxy.php",
                "http://www.cool-tests.com/New-Zealand-proxy.php",
                "http://www.cool-tests.com/Pakistan-proxy.php",
                "http://www.cool-tests.com/Paraguay-proxy.php",
                "http://www.cool-tests.com/Peru-proxy.php",
                "http://www.cool-tests.com/Poland-proxy.php",
                "http://www.cool-tests.com/Romania-proxy.php",
                "http://www.cool-tests.com/Russian-Federation-proxy.php",
                "http://www.cool-tests.com/El-Salvador-proxy.php",
                "http://www.cool-tests.com/Saudi-Arabia-proxy.php",
                "http://www.cool-tests.com/Serbia-proxy.php",
                "http://www.cool-tests.com/Slovakia-proxy.php",
                "http://www.cool-tests.com/Slovenia-proxy.php",
                "http://www.cool-tests.com/Thailand-proxy.php",
                "http://www.cool-tests.com/Taiwan-proxy.php",
                "http://www.cool-tests.com/Tanzania-proxy.php",
                "http://www.cool-tests.com/Turkey-proxy.php",
                "http://www.cool-tests.com/Uzbekistan-proxy.php",
                "http://www.cool-tests.com/Ukraine-proxy.php",
                "http://www.cool-tests.com/Philippines-proxy.php",
                "http://www.cool-tests.com/Finland-proxy.php",
                "http://www.cool-tests.com/France-proxy.php",
                "http://www.cool-tests.com/Croatia-proxy.php",
                "http://www.cool-tests.com/Chile-proxy.php",
                "http://www.cool-tests.com/Sweden-proxy.php",
                "http://www.cool-tests.com/Switzerland-proxy.php",
                "http://www.cool-tests.com/Ecuador-proxy.php",
                "http://www.cool-tests.com/South-Korea-proxy.php",
                "http://www.cool-tests.com/Japan-proxy.php",
                //"http://fineproxy.org/",
                //"http://www.getproxy.jp/",
                //"http://www.getproxy.jp/default/2",
                //"http://www.getproxy.jp/default/3",
                //"http://www.getproxy.jp/default/4",
                //"http://www.getproxy.jp/default/5",
                //"http://www.google-proxy.net/",
                //"http://www.hotvpn.com/ru/proxies/",
                //"http://www.hotvpn.com/proxies/2/",
                //"http://www.hotvpn.com/proxies/3/",
                //"http://www.hotvpn.com/proxies/4/",
                //"http://www.hotvpn.com/proxies/5/",
                //"http://letushide.com/protocol/https/list_of_free_HTTPS_proxy_servers",
                //"http://letushide.com/protocol/https/2/list_of_free_HTTPS_proxy_servers",
                //"http://letushide.com/protocol/https/3/list_of_free_HTTPS_proxy_servers",
                //"http://letushide.com/protocol/https/4/list_of_free_HTTPS_proxy_servers",
                //"http://letushide.com/protocol/https/5/list_of_free_HTTPS_proxy_servers",
                //"http://letushide.com/protocol/socks/",
                //"http://letushide.com/protocol/socks/2/list_of_free_SOCKS_proxy_servers",
                //"http://letushide.com/protocol/http/",
                //"http://letushide.com/protocol/http/2/list_of_free_HTTP_proxy_servers",
                //"http://letushide.com/protocol/http/3/list_of_free_HTTP_proxy_servers",
                //"http://letushide.com/protocol/http/4/list_of_free_HTTP_proxy_servers",
                //"http://letushide.com/protocol/http/5/list_of_free_HTTP_proxy_servers",
                //"http://notan.h1.ru/hack/xwww/proxy1.html",
                //"http://notan.h1.ru/hack/xwww/proxy2.html",
                //"http://notan.h1.ru/hack/xwww/proxy3.html",
                //"http://proxylist.sakura.ne.jp/index.htm?pages=0",
                //"http://proxylist.sakura.ne.jp/index.htm?pages=1",
                //"http://proxylist.sakura.ne.jp/index.htm?pages=2",
                //"http://nntime.com/",
                //"http://nntime.com/proxy-list-02.htm",
                //"http://nntime.com/proxy-list-03.htm",
                //"http://txt.proxyspy.net/proxy.txt",
                //"http://www.rmccurdy.com/scripts/proxy/good.txt",
                //"http://proxy-ip-list.com/download/free-proxy-list.txt",
                //"http://www.shroomery.org/ythan/proxylist.php/RK=0",
                //"http://50na50.net/no_anonim_http.txt",
                //"http://www.romantic-collection.net/proxy.txt",
                //"http://vmarte.com/proxy/proxy_all.txt",
                //"http://reptv.ru/shy/proxylist.txt",
                //"http://pozitiv.3owl.com/proxy.txt"
            };

        private bool _isTesting;

        public bool IsTesting
        {
            get { return _isTesting; }
            internal set { _isTesting = value; }
        }

        private bool _isScraping;

        public bool IsScraping
        {
            get { return _isScraping; }
            internal set { _isScraping = value; }
        }

        private Thread[] _tester = new Thread[500];
        private Thread[] _scraper = new Thread[500];
        private Thread[] _founder = new Thread[10];

        private Thread _proxyManager;
        private bool _stopManager = false;

        public int Total;
        private List<SystemProxy> _alive;
        private Queue<SystemProxy> _testList = new Queue<SystemProxy>();
        private Queue<SystemProxy> _foundProxies = new Queue<SystemProxy>();
        private Queue<DateTime> _wakeUpPoint = new Queue<DateTime>();

        public List<SystemProxy> Alive
        {
            get { return _alive; }
        }

        private Queue<SystemProxy> _publicProxies = new Queue<SystemProxy>();

        public Queue<SystemProxy> PublicProxies
        {
            get { return _publicProxies; }            
        }

        private Queue<SystemProxy> _privateProxies = new Queue<SystemProxy>();

        public Queue<SystemProxy> PrivateProxies
        {
            get { return _privateProxies; }            
        }

        private ProxyTestType _testType;
        private int _finishWorker;
        private int _finishFounder;
        private int _finishScraper;
        //for interactions with main window
        private Window _currentWD;

        public Window CurrentWD
        {
            get { return _currentWD; }
            set { _currentWD = value; }
        }

        private TextBox _publicProxyTB = new TextBox();

        public TextBox PublicProxyTB
        {
            get { return _publicProxyTB; }
            set { _publicProxyTB = value; }
        }

        private TextBox _publicAliveProxy = new TextBox();

        public TextBox PublicAliveProxy
        {
            get { return _publicAliveProxy; }
            set { _publicAliveProxy = value; }
        }

        private TextBox _publicTotalProxy = new TextBox();

        public TextBox PublicTotalProxy
        {
            get { return _publicTotalProxy; }
            set { _publicTotalProxy = value; }
        }

        private TextBox _privateProxyTB = new TextBox();

        public TextBox PrivateProxyTB
        {
            get { return _privateProxyTB; }
            set { _privateProxyTB = value; }
        }

        private TextBox _privateAliveProxy = new TextBox();

        public TextBox PrivateAliveProxy
        {
            get { return _privateAliveProxy; }
            set { _privateAliveProxy = value; }
        }

        private TextBox _privateTotalProxy = new TextBox();

        public TextBox PrivateTotalProxy
        {
            get { return _privateTotalProxy; }
            set { _privateTotalProxy = value; }
        }

        private TextBox _foundProxy = new TextBox();

        public TextBox FoundProxy
        {
            get { return _foundProxy; }
            set { _foundProxy = value; }
        }

        public Action<Window, TextBox, Queue<SystemProxy>> _updateProxyTable;
        public Action<Window, TextBox, TextBox, string, string> _updateStatistics;
        public Action<Window, TextBox, string> _updateFoundStatistics;
        public Action<Window, TextBox, string> _updateStatus;
        public bool isLoadingCompleted = false;

        //for interaction with other objects
        private object _lockObject = null;
        private string _lockObjectName = "";
        private static readonly object _locker = new object();

        #endregion

        #region constructors

        public static ProxyHandler Instance
        {
            get
            {
                lock (_instanceLocker)
                {
                    if (_instance == null)
                    {
                        _instance = new ProxyHandler();
                    }
                }
                return _instance;
            }
        }

        ProxyHandler()
        {
            Total = 0;
            _isTesting = false;
            _isScraping = false;
            _alive = new List<SystemProxy>();
            _finishWorker = 0;
            _finishFounder = 0;
            _finishScraper = 0;
            if (System.IO.File.Exists(SysSettings._systemIniFile))
            {
                LoadProxySettings();
            }
            if (System.IO.File.Exists(SysSettings._proxycFile))
            {
                LoadProxies();
            }
            if (System.IO.File.Exists(SysSettings._proxypFile))
            {
                LoadPrivateProxies();
            }
            CreateWakeUpSchedule();
            _proxyManager = new Thread(proxyManager_DoWork);
            _proxyManager.IsBackground = true;

            if (_autoSearchProxy)
                _proxyManager.Start();

            isLoadingCompleted = true;
        }

        #endregion

        #region WorkerFunctions

        private void tester_DoWork()
        {            
            switch (_testType)
            {
                case ProxyTestType.Public:
                    do
                    {
                        SystemProxy currentProxy = new SystemProxy();
                        lock (_dequeueLocker)
                        {
                            if (_testList.Count > 0)
                                currentProxy = _testList.Dequeue();
                            else
                                break;
                        }
                        if (currentProxy.Ip != "" && currentProxy.Port != "")
                        {
                            try
                            {
                                HttpWebRequest anonyRequest = WebRequest.Create(_checkAnonymousLink) as HttpWebRequest;
                                WebProxy test = new WebProxy(currentProxy.Ip, Convert.ToInt32(currentProxy.Port));
                                anonyRequest.Proxy = test;
                                anonyRequest.Timeout = _timeOut;
                                anonyRequest.KeepAlive = false;
                                anonyRequest.ReadWriteTimeout = _timeOut;

                                HttpWebResponse anonyResponse = (HttpWebResponse)anonyRequest.GetResponse();
                                Encoding anonyEncoding = Encoding.UTF8;

                                var document = new HtmlDocument();
                                bool isDownload = false;
                                using (var reader = new StreamReader(anonyResponse.GetResponseStream()))
                                {
                                    try
                                    {
                                        document.Load(reader.BaseStream, anonyEncoding);
                                        isDownload = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        
                                    }
                                }
                                if (isDownload)
                                {
                                    if (document.DocumentNode.InnerText.Contains("REQUEST_URI = /azenv.php") && document.DocumentNode.InnerText.Contains("HTTP_HOST = www.proxy-listen.de"))
                                    {
                                        if (_checkAnonymous)
                                        {
                                            if (!document.DocumentNode.InnerText.Contains("HTTP_X_FORWARDED_FOR"))
                                            {
                                                lock (_enqueueLocker)
                                                {
                                                    FilterProxies(currentProxy);
                                                    _updateStatus.Invoke(_currentWD, _publicAliveProxy, _alive.Count.ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            lock (_enqueueLocker)
                                            {
                                                FilterProxies(currentProxy);
                                                _updateStatus.Invoke(_currentWD, _publicAliveProxy, _alive.Count.ToString());
                                            }
                                        }
                                    }
                                }                                
                            }
                            catch (Exception ex)
                            {                                
                            }
                        }
                        else
                        {
                            break;
                        }
                    } while (_testList.Count > 0);
                    tester_DoWorkCompleted();
                    break;
                case ProxyTestType.Private:
                    while (IsTesting)
                    {
                        SystemProxy currentProxy = new SystemProxy();
                        lock (_dequeueLocker)
                        {
                            if (_testList.Count > 0)
                                currentProxy = _testList.Dequeue();
                            else
                                break;
                        }
                        if (currentProxy.Ip != "" && currentProxy.Port != "")
                        {
                            try
                            {
                                HttpWebRequest request = WebRequest.Create(_testServer) as HttpWebRequest;
                                WebProxy p = new WebProxy(currentProxy.Ip, Convert.ToInt32(currentProxy.Port));
                                p.Credentials = new NetworkCredential(currentProxy.Username, currentProxy.Password);
                                request.Proxy = p;
                                request.Timeout = _timeOut;
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
                                    lock (_enqueueLocker)
                                    {
                                        FilterProxies(currentProxy);
                                        _updateStatus.Invoke(_currentWD, _privateAliveProxy, _alive.Count.ToString());
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
                    tester_DoWorkCompleted();
                    break;
            };
        }       

        private void tester_DoWorkCompleted()
        {
            lock (_finishLocker)
            {
                 _finishWorker++;
            }
            //if (_finishWorker >= _threads -1)
            if (_finishWorker == _threads)
            {
                UpdateProxies();
                if (_testType == ProxyTestType.Public)
                {
                    _updateProxyTable.Invoke(_currentWD, _publicProxyTB, _publicProxies);
                    _updateStatistics.Invoke(_currentWD, _publicTotalProxy, _publicAliveProxy, _publicProxies.Count.ToString(), _publicProxies.Count.ToString());
                }
                else
                {
                    _updateProxyTable.Invoke(_currentWD, _privateProxyTB, _privateProxies);
                    _updateStatistics.Invoke(_currentWD, _privateTotalProxy, _privateAliveProxy, _privateProxies.Count.ToString(), _privateProxies.Count.ToString());
                }
                _isTesting = false;
                _finishWorker = 0;
            }
        }

        #endregion

        #region PrivateMethods

        private void CreateTestList()
        {
            lock (_usingLocker)
            {
                if (_testType == ProxyTestType.Public)
                    _testList = CloneProxies(_publicProxies);
                else
                    _testList = CloneProxies(_privateProxies);
            }
        }

        private Queue<SystemProxy> CloneProxies(Queue<SystemProxy> list)
        {
            Queue<SystemProxy> temp = new Queue<SystemProxy>();

            foreach (var i in list)
            {
                temp.Enqueue(i);
            }

            return temp;
        }

        private void FilterProxies(SystemProxy proxy)
        {
            _alive.Add(proxy);
        }

        private void UpdateProxies()
        {
            lock (_usingLocker)
            {
                switch (_testType)
                {
                    case ProxyTestType.Public:
                        _publicProxies.Clear();
                        foreach (var i in _alive)
                        {
                            _publicProxies.Enqueue(i);
                        }
                        _alive = new List<SystemProxy>();
                        break;
                    case ProxyTestType.Private:
                        _privateProxies.Clear();
                        foreach (var i in _alive)
                        {
                            _privateProxies.Enqueue(i);
                        }
                        _alive = new List<SystemProxy>();
                        break;
                }
            }
        }

        private void UpdateProxies(Queue<SystemProxy> proxies)
        {
            lock (_usingLocker)
            {               
                _publicProxies.Clear();
                foreach (var i in proxies)
                {
                    _publicProxies.Enqueue(i);
                }
            }
        }

        public void RunHandle(ProxyTestType testType)
        {
            _testType = testType;
            _isTesting = true;
            CreateTestList();
            for (int i = 0; i < _threads; i++)
            {
                _tester[i] = new Thread(tester_DoWork);
                _tester[i].IsBackground = true;
                _tester[i].Start();
            }
        }

        public void CancelHandle()
        {
            for (int i = 0; i < _threads; i++)
            {
                _tester[i].Abort();
            }
        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Register all actions to proxy's system
        /// </summary>
        /// <param name="updateAliveProxy"></param>
        /// <param name="updateStatistics"></param>
        /// <param name="updateFoundStatistics"></param>
        /// <param name="updateProxyTable"></param>
        public void RegisterActions(Action<Window, TextBox, string> updateAliveProxy, Action<Window, TextBox, TextBox, string, string> updateStatistics, Action<Window, TextBox, string> updateFoundStatistics, Action<Window, TextBox, Queue<SystemProxy>> updateProxyTable)
        {
            _updateStatistics = updateStatistics;
            _updateFoundStatistics = updateFoundStatistics;
            _updateProxyTable = updateProxyTable;
            _updateStatus = updateAliveProxy;
        }

        public void RegisterControls(Window wd, TextBox tbPublicProxyTable, TextBox tbPublicProxyTotal, TextBox tbPublicProxyAlive, TextBox tbPrivateProxyTable, TextBox tbPrivateProxyTotal, TextBox tbPrivateProxyAlive, TextBox tbFoundProxy)
        {
            _currentWD = wd;
            _publicProxyTB = tbPublicProxyTable;
            _publicAliveProxy = tbPublicProxyAlive;
            _publicTotalProxy = tbPublicProxyTotal;
            _privateProxyTB = tbPrivateProxyTable;
            _privateAliveProxy = tbPrivateProxyAlive;
            _privateTotalProxy = tbPrivateProxyTotal;
            _foundProxy = tbFoundProxy;
        }

        /// <summary>
        /// Check whether or not the object & name are equal to lock objects
        /// </summary>
        /// <param name="check"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckLock(object check, string name)
        {
            return _lockObject.Equals(check) && _lockObjectName == name;
        }

        public void ConsumeHandler(object lockObject, string name, ref bool result)
        {
            lock (_locker)
            {
                if (_lockObject != null)
                {
                    if (!CheckLock(lockObject, name))
                        result = false;
                    else
                        result = true;
                }
                else
                {
                    _lockObject = lockObject;
                    _lockObjectName = name;
                    result = true;
                }
            }
        }

        public void ReleaseHandler(object lockObject, string name, ref bool result)
        {
            lock (_locker)
            {
                if (_lockObject == null)
                    result = false;
                else
                {
                    if (!CheckLock(lockObject, name))
                        result = false;
                    else
                    {
                        _lockObject = null;
                        _lockObjectName = "";
                        result = true;
                    }
                }
            }
        }

        public WebProxy GetRandomProxy(object lockObject, string name, ProxyType type)
        {
            if (CheckLock(lockObject, name))
            {
                lock (_usingLocker)
                {                    
                    WebProxy p;
                    int pos = 0;
                    if (type == ProxyType.PublicProxy)
                    {
                        pos = pRnd.Next(_publicProxies.Count);
                        p = new WebProxy(_publicProxies.ElementAt(pos).Ip, Convert.ToInt32(_publicProxies.ElementAt(pos).Port));
                    }
                    else
                    {
                        pos = pRnd.Next(_privateProxies.Count);
                        p = new WebProxy(_privateProxies.ElementAt(pos).Ip, Convert.ToInt32(_privateProxies.ElementAt(pos).Port));
                        p.Credentials = new NetworkCredential(_privateProxies.ElementAt(pos).Username, _privateProxies.ElementAt(pos).Password);
                    }
                    return p;
                }
            }
            else
                return null;
        }        

        public void Initialize()
        {

        }

        public bool IsLoadingCompleled()
        {
            lock (_loadingLocker)
                return isLoadingCompleted;
        }

        #endregion

        #region Save & Load Proxies

        private void LoadProxySettings()
        {
            //load settings
            _autoSearchProxy = Convert.ToBoolean(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "autosearchproxy", _defaultValue));
            _searchProxyInterval = Convert.ToInt32(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "searchproxyinterval", _defaultValue));
            _testProxy = Convert.ToBoolean(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "testproxy", _defaultValue));
            _checkAnonymous = Convert.ToBoolean(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "checkanonymous", _defaultValue));
            _checkAnonymousLink = IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "checkanonymouslink", _defaultValue);
            _threads = Convert.ToInt32(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "threads", _defaultValue));
            _timeOut = Convert.ToInt32(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "timeout", _defaultValue));
            _scrapeHarvestLinks = Utilities.Helper.Parse<ProxyType>(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "scrapeharvestlinks", _defaultValue)) ?? ProxyType.PublicProxy;
            _scrapeDownloadArticles = Utilities.Helper.Parse<ProxyType>(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "scrapedownloadarticles", _defaultValue)) ?? ProxyType.PublicProxy;
            _postingHarvestLinks = Utilities.Helper.Parse<ProxyType>(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "postingharvestlinks", _defaultValue)) ?? ProxyType.PublicProxy;
            _postingDownloadArticles = Utilities.Helper.Parse<ProxyType>(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "postingdownloadarticles", _defaultValue)) ?? ProxyType.PublicProxy;
            _vnsDownloadArticles = Utilities.Helper.Parse<ProxyType>(IniHelper.GetIniFileString(SysSettings._systemIniFile, "proxy", "vnsdownloadarticles", _defaultValue)) ?? ProxyType.PublicProxy;
        }

        private void LoadProxies()
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(SysSettings._proxycFile))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    _publicProxies.Enqueue(Utilities.Helper.GetProxy(line));
                }
            }
        }

        public void LoadProxies(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(fileName))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    _publicProxies.Enqueue(Utilities.Helper.GetProxy(line));
                }
            }
        }

        private void LoadPrivateProxies()
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(SysSettings._proxypFile))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    _privateProxies.Enqueue(Utilities.Helper.GetProxy(line));
                }
            }
        }

        public void LoadPrivateProxies(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(fileName))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    _privateProxies.Enqueue(Utilities.Helper.GetProxy(line));
                }
            }
        }

        public void SaveSystem()
        {
            SaveProxySettings();
            SaveProxies();
        }

        private void SaveProxySettings()
        {
            //write System settings
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "autosearchproxy", _autoSearchProxy.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "searchproxyinterval", _searchProxyInterval.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "testproxy", _testProxy.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "checkanonymous", _checkAnonymous.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "checkanonymouslink", _checkAnonymousLink.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "threads", _threads.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "timeout", _timeOut.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "scrapeharvestlinks", _scrapeHarvestLinks.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "scrapedownloadarticles", _scrapeDownloadArticles.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "postingharvestlinks", _postingHarvestLinks.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "postingdownloadarticles", _postingDownloadArticles.ToString());
            IniHelper.WriteKey(SysSettings._systemIniFile, "proxy", "vnsdownloadarticles", _vnsDownloadArticles.ToString());
        }

        private void SaveProxies()
        {
            using (StreamWriter writer = new StreamWriter(SysSettings._proxycFile, false))
            {
                foreach (var i in _publicProxies)
                {
                    writer.WriteLine(i.ToString());
                }
            }

            using (StreamWriter writer = new StreamWriter(SysSettings._proxypFile, false))
            {
                foreach (var i in _privateProxies)
                {
                    writer.WriteLine(i.ToString());
                }
            }
        }
        
        #endregion

        #region Scrape Proxy

        public void ScrapeProxy(ProxyTestType testType)
        {
            if (!IsTesting && !IsScraping)
            {
                IsScraping = true;
                _testType = testType;                              
                CloneProxyFeeder();
                for (int i = 0; i < 10; i++)
                {
                    _founder[i] = new Thread(Founder_DoWork);
                    _founder[i].IsBackground = true;
                    _founder[i].Start();
                }
            }
        }

        private void CloneProxyFeeder()
        {
            _cloneProxyFeeder.Clear();
            foreach (var feed in _publicProxyFeeder)
            {
                _cloneProxyFeeder.Enqueue(feed);
            }
        }

        private void Founder_DoWork()
        {
            do
            {
                string link = "";
                int retry = 0;
                lock (_foundLocker)
                {
                    if (_cloneProxyFeeder.Count > 0)
                        link = _cloneProxyFeeder.Dequeue();
                    else
                        break;
                }
                if (link != string.Empty)
                {
                    bool IsDownload = false;
                    while (retry < 2)
                    {
                        try
                        {
                            HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;
                            request.Timeout = 30000;
                            request.KeepAlive = false;                            

                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            Encoding encoding = Encoding.UTF8;

                            var document = new HtmlDocument();
                            bool isReaded = false;
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                try
                                {
                                    document.Load(reader.BaseStream, encoding);
                                    isReaded = true;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            if (isReaded)
                            {
                                IProxyScraper pScraper = ProxyScraperFactory.GetProxyScraper(link);
                                List<SystemProxy> proxies = pScraper.GetProxies(document, link);

                                lock (_enqueueLocker)
                                {
                                    PrepareTestProxies(proxies);
                                    _updateFoundStatistics.Invoke(_currentWD, _foundProxy, "Found " + proxies.Count + " proxies at " + link);
                                }
                                IsDownload = true;
                                break;
                            }
                            else                            
                                retry++;
                            
                        }
                        catch (Exception ex)
                        {
                            retry++;
                        }
                    }

                    if (!IsDownload)
                    {
                        _updateFoundStatistics.Invoke(_currentWD, _foundProxy, "Cannot find any proxy at " + link + "!");
                    }
                }
            } while (true);
            Founder_DoWorkComplete();
        }

        private void PrepareTestProxies(List<SystemProxy> proxies)
        {
            foreach (var proxy in proxies)
                _foundProxies.Enqueue(proxy);
        }

        private void Founder_DoWorkComplete()
        {
            lock (_finishLocker)
            {
                _finishFounder++;
            }

            if (_finishFounder == 10)
            {
                if (TestProxy)
                {
                    IsTesting = true;
                    _updateProxyTable.Invoke(_currentWD, _publicProxyTB, _foundProxies);
                    _updateStatistics.Invoke(_currentWD, _publicTotalProxy, _publicAliveProxy, _foundProxies.Count.ToString(), _foundProxies.Count.ToString());
                    
                    for (int i = 0; i < _threads; i++)
                    {
                        _scraper[i] = new Thread(Scraper_DoWork);
                        _scraper[i].IsBackground = true;
                        _scraper[i].Start();
                    }
                }
                else
                {
                    IsScraping = false;
                    UpdateProxies(_foundProxies);
                    CreateTestList();
                    _updateProxyTable.Invoke(_currentWD, _publicProxyTB, _foundProxies);
                    _updateStatistics.Invoke(_currentWD, _publicTotalProxy, _publicAliveProxy, _foundProxies.Count.ToString(), _foundProxies.Count.ToString());
                }
            }
        }

        private void Scraper_DoWork()
        {
            do
            {
                SystemProxy currentProxy = new SystemProxy();
                lock (_dequeueLocker)
                {
                    if (_foundProxies.Count > 0)
                        currentProxy = _foundProxies.Dequeue();
                    else
                        break;
                }
                if (currentProxy.Ip != "" && currentProxy.Port != "")
                {
                    try
                    {
                        HttpWebRequest anonyRequest = WebRequest.Create(_checkAnonymousLink) as HttpWebRequest;
                        WebProxy test = new WebProxy(currentProxy.Ip, Convert.ToInt32(currentProxy.Port));
                        anonyRequest.Proxy = test;
                        anonyRequest.Timeout = _timeOut;
                        anonyRequest.KeepAlive = false;
                        anonyRequest.ReadWriteTimeout = _timeOut;

                        HttpWebResponse anonyResponse = (HttpWebResponse)anonyRequest.GetResponse();
                        Encoding anonyEncoding = Encoding.UTF8;

                        var document = new HtmlDocument();
                        bool isDownload = false;

                        using (var reader = new StreamReader(anonyResponse.GetResponseStream()))
                        {
                            try
                            {
                                document.Load(reader.BaseStream, anonyEncoding);
                                isDownload = true;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        if (isDownload)
                        {
                            if (document.DocumentNode.InnerText.Contains("REQUEST_URI = /azenv.php") && document.DocumentNode.InnerText.Contains("HTTP_HOST = www.proxy-listen.de"))
                            {
                                if (_checkAnonymous)
                                {
                                    if (!document.DocumentNode.InnerText.Contains("HTTP_X_FORWARDED_FOR"))
                                    {
                                        lock (_enqueueLocker)
                                        {
                                            FilterProxies(currentProxy);
                                            _updateStatus.Invoke(_currentWD, _publicAliveProxy, _alive.Count.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    lock (_enqueueLocker)
                                    {
                                        FilterProxies(currentProxy);
                                        _updateStatus.Invoke(_currentWD, _publicAliveProxy, _alive.Count.ToString());
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            } while (_foundProxies.Count > 0);
            Scraper_DoWorkComplete();
        }

        private void Scraper_DoWorkComplete()
        {
            lock (_finishLocker)
            {
                _finishScraper++;
            }

            //if (_finishScraper == _threads - 1)
            if (_finishScraper == _threads)
            {
                IsScraping = false;
                IsTesting = false;
                UpdateProxies();
                _updateProxyTable.Invoke(_currentWD, _publicProxyTB, _publicProxies);
                _updateStatistics.Invoke(_currentWD, _publicTotalProxy, _publicAliveProxy, _publicProxies.Count.ToString(), _publicProxies.Count.ToString());
                _finishScraper = 0;
                _finishFounder = 0;
            }
        }

        #endregion

        #region Manager

        /// <summary>
        /// Create WakeUp Schedule for comeback interval
        /// </summary>
        private void CreateWakeUpSchedule()
        {
            _wakeUpPoint.Clear();
            DateTime holdPoint = DateTime.Now;
            DateTime temp = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, holdPoint.Hour, holdPoint.Minute, holdPoint.Second);
            
            do
            {
                temp = temp.AddMinutes(_searchProxyInterval);
                if (temp < holdPoint)
                    continue;
                else
                    _wakeUpPoint.Enqueue(temp);
            } while (temp < holdPoint.AddDays(30));
        }

        /// <summary>
        /// Handler Manager doWork function
        /// </summary>
        private void proxyManager_DoWork()
        {
            do
            {
                DateTime wakeUpPoint = _wakeUpPoint.Dequeue();
                bool flag = false;

                while (!flag)
                {
                    lock (_stopLocker)
                    {
                        if (_stopManager)
                            goto StopManager;
                    }

                    if (DateTime.Now > wakeUpPoint)
                    {
                        if (!IsTesting && !IsScraping)
                        {
                            flag = true;
                            IsScraping = true;
                            _testType = ProxyTestType.Public;
                            CloneProxyFeeder();
                            for (int i = 0; i < 10; i++)
                            {
                                _founder[i] = new Thread(Founder_DoWork);
                                _founder[i].IsBackground = true;
                                _founder[i].Start();
                            }
                        }
                    }
                    else
                        Thread.Sleep(60000);
                }

            } while (true);
            StopManager: if(_stopManager) StopManager();
        }    

        /// <summary>
        /// Stop function for manager thread
        /// </summary>
        private void StopManager()
        {

        }

        /// <summary>
        /// Switch On/Off
        /// </summary>
        /// <param name="flag"></param>
        public void SwitchManager(bool flag)
        {
            if (_stopManager)
            {
                if (flag)
                {
                    _proxyManager = new Thread(proxyManager_DoWork);
                    _proxyManager.IsBackground = true;
                    _proxyManager.Start();
                }               
            }
            else
            {
                if (!flag)
                {
                    Stop();
                }
            }
        }

        /// <summary>
        /// Stop proxy system
        /// </summary>
        private void Stop()
        {
            lock (_stopLocker)
            {
                _stopManager = true;
            }
        }

        #endregion
    }
}
