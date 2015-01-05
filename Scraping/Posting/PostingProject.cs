using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using DataTypes;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;
using Utilities;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using HtmlAgilityPack;
using JoeBlogs;
using System.Web;
namespace Scraping
{
    public class PostingProject
    {
        #region variables

        // For lockers
        private static readonly object _synclocker = new object(); //using for worker
        private static readonly object _synclocker2 = new object(); //using for worker
        private static readonly object _synclocker3 = new object(); //using for worker
        private static readonly object _stopLocker = new object(); //using for worker 
        private static readonly object _randomlocker = new object(); //using for random
        private static readonly object _finishHarvestLocker = new object(); //using for statistics
        private static readonly object _finishPostLocker = new object(); //using for statistics
        private static readonly object _finishDatabasePostLocker = new object(); //using for statistics
        private static readonly object _finishManagerLocker = new object(); //using for statistics
        private static readonly object _writtingLocker = new object(); //using for write log file
        // For threads
        private int _finishManager = 0;
        private int _finishHarvestWorker = 0; // number of harvest workers have finished its job.
        private int _finishPostWorker = 0; // number of post workers have finished its job.
        private int _finishDatabasePostWorker = 0; // number of post workers have finished its job.
        private int _totalHarvestWorker = 3; //total number of harvest workers.
        private int _totalPostWorker = 0; // total number of post workers.
        private int _totalDatabasePostWorker = 0; // total number of post workers.
        private int _totalManager = 2;
        private Thread[] _harversterWorker = new Thread[3];
        private Thread[] _posterWorker = new Thread[100];
        private Thread[] database_posterWorker = new Thread[100];
        private Thread _harverstManagerWorker;
        private Thread _postManagerWorker;
        private Thread database_postManagerWorker;
        // For containing variables
        public List<string> searchPages = new List<string>();
        private Queue<string> _queueSearchPages = new Queue<string>();
        private Queue<string> _tempSearchPages = new Queue<string>();
        private Queue<DateTime> _wakeUpPoint = new Queue<DateTime>();
        private Queue<DateTime> _postPoint = new Queue<DateTime>();
        private List<HarvestLink> _sourceArticles = new List<HarvestLink>();
        private List<string> _postSites = new List<string>();
        private List<string>[] _chosenArticles = new List<string>[100];
        private static int _minsPerDay = 1440;
        private static string DateTimeFormat = "dd-MM HH:mm:ss";

        private const int MINDELAYINTERVAL = 25000; //minimum delay interval (miliseconds)
        private const int MAXDELAYINTERVAL = 45000; //maximum delay interval (miliseconds)
        private const int MINDELAYINTERVAL1 = 35000; //minimum delay interval (miliseconds)
        private const int MAXDELAYINTERVAL1 = 70000; //maximum delay interval (miliseconds)
        private DriveService service;
        private bool _folderCreated = false;
        private Google.Apis.Drive.v2.Data.File _sharedFolder;

        public List<SourceCategory> chosenCategories = new List<SourceCategory>();

        private Queue<Record>[] database_chosenRecords = new Queue<Record>[100];
        
        //for stop project
        private bool _stopManager = false;
        private bool _stopWorker = false;
        private Thread _stopWatcher;

        //For settings
        private string _projectName;

        public string ProjectName
        {
            get { return _projectName; }
            set { _projectName = value; }
        }

        private string _projectPath;

        public string ProjectPath
        {
            get { return _projectPath; }
            set { _projectPath = value; }
        }

        //for gdrive settings
        private string _googleClientId;

        public string GoogleClientId
        {
            get { return _googleClientId; }
            set { _googleClientId = value; }
        }

        private string _googleClientSecret;

        public string GoogleClientSecret
        {
            get { return _googleClientSecret; }
            set { _googleClientSecret = value; }
        }

        private string _googleApplicationName;

        public string GoogleApplicationName
        {
            get { return _googleApplicationName; }
            set { _googleApplicationName = value; }
        }

        //for rules       
        private List<PostSites> _sites = new List<PostSites>();

        public List<PostSites> Sites
        {
            get { return _sites; }
        }

        private PostRulesCollection _ruleCollection = new PostRulesCollection();

        public PostRulesCollection RuleCollection
        {
            get { return _ruleCollection; }
            set { _ruleCollection = value; }
        }

        private SEORulesCollection _seoRuleCollection = new SEORulesCollection();

        public SEORulesCollection SeoRuleCollection
        {
            get { return _seoRuleCollection; }
            set { _seoRuleCollection = value; }
        }

        private ScheduleRule _scheduleRule = new ScheduleRule();

        public ScheduleRule ScheduleRule
        {
            get { return _scheduleRule; }
            set { _scheduleRule = value; }
        }

        private string _logoPath;

        public string LogoPath
        {
            get { return _logoPath; }
            set { _logoPath = value; }
        }

        private ProjectState _projectState;

        public ProjectState ProjectState
        {
            get { return _projectState; }
            set { _projectState = value; }
        }

        //for collaboration between posting and windows
        public Window _currentWindow; // the window in which contains this action

        //for action
        public Action<Window, Label, ProjectState> updateStatus;
        public Action<Window, Label, string> updateStatistics;


        //for labels
        public Label _statusLabel = new Label();
        public Label _statisticsLabel = new Label();

        #endregion

        #region constructors

        public PostingProject(string name)
            : this(name, "", "", "")
        {

        }

        public PostingProject(string name, string clientId, string clientSecret, string appName)
        {
            _projectName = name;
            _googleClientId = clientId;
            _googleClientSecret = clientSecret;
            _googleApplicationName = appName;
            _projectState = ProjectState.IsReady;
            SetupManager();
        }

        #endregion

        #region actions

        #endregion

        #region public methods

        /// <summary>
        /// Load site to select posting sites of the current project
        /// </summary>
        /// <param name="site"></param>
        public void LoadSite(PostSites site)
        {
            if (!_sites.Contains(site))
                _sites.Add(site);
        }

        /// <summary>
        /// Get post site at position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public PostSites GetSiteAtPos(int pos)
        {
            return _sites[pos];
        }

        /// <summary>
        /// Get post site by its host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public PostSites GetSiteByHost(string host)
        {
            if (_sites.Count > 0)
                return _sites.Where(a => a.Host == host).Single();
            return null;
        }

        /// <summary>
        /// Check site existed or not via its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckSiteExisted(string name)
        {
            foreach (var site in _sites)
                if (site.Host == name)
                    return true;
            return false;
        }

        /// <summary>
        /// Update Google Credentials 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="secret"></param>
        /// <param name="appName"></param>
        public void UpdateGoogleCredentials(string Id, string secret, string appName)
        {
            _googleClientId = Id;
            _googleClientSecret = secret;
            _googleApplicationName = appName;
        }

        /// <summary>
        /// Unused post site by name
        /// </summary>
        /// <param name="name"></param>
        public void RemoveSite(string host)
        {
            PostSites removeSite = _sites.Where(a => a.Host == host).Single();
            _sites.Remove(removeSite);
        }

        #endregion

        #region posting

        public void Start()
        {
            _stopManager = false;
            _stopWorker = false;

            if (_scheduleRule.ScheduleMode == ScheduleMode.LiveFeed)
            {
                //preparation for posting
                CloneSearchPages();
                LoadPostSites();

                //schedule
                CreateWakeUpSchedule();
                CreatePostSchedule();

                //google drive
                CreateDriveService();
                CheckFolderInDrive();

                _harverstManagerWorker.Start();
                _postManagerWorker.Start();
            }
            else
            {
                //preparation for posting                
                LoadPostSites();

                //reload from records
                ReloadArticleFromRecords();

                //schedule
                CreatePostSchedule();

                //google drive
                CreateDriveService();
                CheckFolderInDrive();

                database_postManagerWorker.Start();
            }
            AutoPost.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] has been started!"));
            _projectState = DataTypes.Enums.ProjectState.IsRunning;
            updateStatus.Invoke(_currentWindow, _statusLabel, _projectState);
        }

        public void Stop()
        {
            lock (_stopLocker)
            {
                _stopManager = true;
                _stopWorker = true;
            }
            _stopWatcher = new Thread(StopWatcher_DoWork);
            _stopWatcher.Start();
        }

        /// <summary>
        /// Harvest manager thread DoWork function
        /// </summary>
        private void harvestManager_DoWork()
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
                            goto StopThread;
                    }

                    if (DateTime.Now > wakeUpPoint)
                    {
                        lock (_finishHarvestLocker)
                            _finishHarvestWorker = 0;
                        flag = true;

                        //check if it's already tomorrow
                        if (DateTime.Now.Day > wakeUpPoint.Day && DateTime.Now.Month == wakeUpPoint.Month)
                        {
                            lock (_synclocker2)
                            {
                                _sourceArticles.Clear();
                            }
                        }

                        for (int i = 0; i < _totalHarvestWorker; i++)
                        {
                            int input = i;
                            _harversterWorker[i] = new Thread(harvestWorker_DoWork);
                            _harversterWorker[i].IsBackground = true;                            
                            _harversterWorker[i].Start(input);
                        }
                    }
                    else
                        Thread.Sleep(60000);
                }

            } while (_wakeUpPoint.Count > 0);
            StopThread: if (_stopManager) StopManager();
        }

        /// <summary>
        /// Post manager thread DoWork function
        /// </summary>
        private void postManager_DoWork()
        {
            do
            {
                DateTime wakeUpPoint = _postPoint.Dequeue();
                bool flag = false;

                while (!flag)
                {
                    lock (_stopLocker)
                    {
                        if (_stopManager)
                            goto StopThread;
                    }

                    if (DateTime.Now > wakeUpPoint)
                    {
                        lock (_finishPostLocker)
                            _finishPostWorker = 0;
                        flag = true;
                        for (int i = 0; i < _postSites.Count; i++)
                        {
                            int input = i;
                            if (_chosenArticles[i] == null)
                            {
                                _chosenArticles[i] = new List<string>();
                                ReadPostedLog(input, _postSites[input]);
                            }
                            _posterWorker[i] = new Thread(postWorker_DoWork);
                            _posterWorker[i].IsBackground = true;                            
                            _posterWorker[i].Start(input);
                        }
                    }
                    else
                        Thread.Sleep(60000);
                }
            } while (_postPoint.Count > 0);
            StopThread: if (_stopManager) StopManager();
        }

        /// <summary>
        /// Database post manager DoWork function
        /// </summary>
        private void databasePostManager_DoWork()
        {
            do
            {
                DateTime wakeUpPoint = _postPoint.Dequeue();
                bool flag = false;

                while (!flag)
                {
                    lock (_stopLocker)
                    {
                        if (_stopManager)
                            goto StopThread;
                    }

                    if (DateTime.Now > wakeUpPoint)
                    {
                        lock (_finishDatabasePostLocker)
                            _finishDatabasePostWorker = 0;
                        flag = true;

                        for (int i = 0; i < _postSites.Count; i++)
                        {
                            int input = i;
                            database_posterWorker[i] = new Thread(databasePostWorker_DoWork);
                            database_posterWorker[i].IsBackground = true;                            
                            database_posterWorker[i].Start(input);
                        }
                    }
                    else
                        Thread.Sleep(60000);
                }
            } while (_postPoint.Count > 0);
            StopThread: if (_stopManager) StopManager();
        }

        /// <summary>
        /// Stop function for manager thread
        /// </summary>
        private void StopManager()
        {
            lock (_finishManagerLocker)
            {
                _finishManager++;
            }

            if (_scheduleRule.ScheduleMode == ScheduleMode.LiveFeed)
            {
                if (_finishManager == _totalManager)
                {
                    AutoPost.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "All managers of project [" + _projectName + "] has been stopped!"));
                }
            }
            else
            {
                AutoPost.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "All managers of project [" + _projectName + "] has been stopped!"));
            }
        }

        /// <summary>
        /// Harvester worker DoWork function
        /// </summary>
        /// <param name="Index"></param>
        private void harvestWorker_DoWork(object Index)
        {
            int index = (int)Index;
            string message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Thread[" + index + "]: Begin harvest link!");
            AutoPost.Instance.AddMessages(message);
            WriteLog(message, false, "");
            do
            {
                lock (_stopLocker)
                {
                    if (_stopWorker)
                        goto StopThread;
                }
                string link = string.Empty;
                int retry = 0;
                lock (_synclocker)
                {
                    if (_queueSearchPages.Count > 0)
                    {
                        link = _queueSearchPages.Dequeue();
                        //_tempSearchPages.Enqueue(link);
                    }
                    else
                        break;
                }
                if (link != string.Empty)
                {
                    string domain = "http://" + new Uri(link).Host;
                    while (retry < 3)
                    {
                        try
                        {
                            HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;
                            int pos = 0;
                            Random rand = new Random();

                            if (ProxyHandler.Instance.PostingHarvestLinks != ProxyType.Disabled)
                            {
                                //get proxy
                                WebProxy p;
                                do
                                {
                                    bool result = false;
                                    ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|Harvest|Thread[" + index + "]", ref result);
                                    if (!result)
                                        Thread.Sleep(150);
                                    else
                                    {
                                        p = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|Harvest|Thread[" + index + "]", ProxyHandler.Instance.PostingHarvestLinks);
                                        ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|Harvest|Thread[" + index + "]", ref result);
                                        break;
                                    }
                                } while (true);

                                //prepare the request
                                request.Proxy = p;
                            }
                            request.Timeout = 30000;
                            request.KeepAlive = false;
                            request.ProtocolVersion = HttpVersion.Version10;
                            request.Method = "GET";
                            request.Accept = "text/html,application/xhtml+xml,application/xml";
                            request.ServicePoint.ConnectionLimit = 1;
                            pos = rand.Next(SysSettings.Instance._agents.Count);
                            request.UserAgent = SysSettings.Instance._agents[pos];

                            var document = new HtmlDocument();
                            bool isHarvested = false;
                            using (var response = (HttpWebResponse)request.GetResponse())
                            {
                                using (var responseStream = response.GetResponseStream())
                                {
                                    Encoding encoding = Encoding.UTF8;
                                    using (var reader = new StreamReader(responseStream))
                                    {
                                        try
                                        {
                                            document.Load(reader.BaseStream, encoding);
                                            isHarvested = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Harvest | Thread[" + index + "]: Download failed! Reason : " + ex.Message);
                                            AutoPost.Instance.AddMessages(message);
                                            WriteLog(message, false, "");
                                        }
                                    }
                                    request.Abort();
                                }
                            }

                            if (isHarvested)
                            {
                                IHarvest harvester = HarvesterFactory.CreateHarvester(link);
                                int page = harvester.PageNumber(link);
                                List<HarvestLink> harvestedLinks = harvester.HarvestLinks(document, link, page);
                                if (harvestedLinks.Count > 0)
                                {
                                    foreach (var lnk in harvestedLinks)
                                    {                                        
                                        if (!lnk.articleUrl.Contains("http://"))
                                            lnk.articleUrl = domain + lnk.articleUrl;                                        

                                        lock (_synclocker2)
                                        {
                                            if (!IsExisted(lnk.articleUrl) && !IsViolate(lnk.articleUrl))
                                            {
                                                _sourceArticles.Add(lnk);
                                                message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Harvest | Thread[" + index + "]: Found " + lnk.articleUrl + " and added!");
                                                AutoPost.Instance.AddMessages(message);
                                                WriteLog(message, false, "");
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                            else
                            {
                                retry++;
                                Random retryRand = new Random();
                                Thread.Sleep(retryRand.Next(MINDELAYINTERVAL, MAXDELAYINTERVAL));
                            }
                        }
                        catch (Exception ex)
                        {
                            retry++;
                            message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Harvest | Thread[" + index + "]: Download of " + link + " failed! Retry " + retry + "/3 times. Reason : " + ex.Message);
                            AutoPost.Instance.AddMessages(message);
                            WriteLog(message, false, "");
                            Random retryRand = new Random();
                            Thread.Sleep(retryRand.Next(MINDELAYINTERVAL, MAXDELAYINTERVAL));
                        }
                    }
                }
                Random nextTurn = new Random();
                Thread.Sleep(nextTurn.Next(MINDELAYINTERVAL, MAXDELAYINTERVAL));
            } while (true);
            StopThread: if (_stopWorker) StopWorker(true);
            if (!_stopWorker) harvestWorker_Completed();
        }

        /// <summary>
        /// Handle harvestWorker completed operation
        /// </summary>
        private void harvestWorker_Completed()
        {
            lock (_finishHarvestLocker)
            {
                _finishHarvestWorker++;
            }
            if (_finishHarvestWorker == _totalHarvestWorker)
            {
                string message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Harvest links completely!");
                AutoPost.Instance.AddMessages(message);
                WriteLog(message, false, "");
                _finishHarvestWorker = 0;
                CloneSearchPages();
            }
        }

        /// <summary>
        /// Poster worker DoWork function
        /// </summary>
        /// <param name="Index"></param>
        private void postWorker_DoWork(object Index)
        {
            int index = (int)Index;
            List<PostRule> _currentSitePostRule = _ruleCollection.GetRulesBySiteHost(_postSites[index]);
            List<HarvestLink> _matchLinks = new List<HarvestLink>();
            string chosenSourceArticleLink = "";
            HarvestLink chosenSourceArticle = new HarvestLink("", "");
            string errorMessage = "";
            Random rnd = new Random();

            lock (_synclocker2)
            {
                _matchLinks = getMatchLinks(_sourceArticles, _currentSitePostRule);
            }

            lock (_stopLocker)
            {
                if (_stopWorker)
                    goto StopThread;
            }
            string message = "";
            if (_matchLinks.Count > 0)
            {
                bool foundLink = false;
                int foundTry = 0;
                do
                {
                    int post = rnd.Next(_matchLinks.Count);
                    chosenSourceArticleLink = _matchLinks[post].articleUrl;
                    if (!_chosenArticles[index].Contains(chosenSourceArticleLink))
                    {                        
                        foundLink = true;
                        chosenSourceArticle = _matchLinks[post];
                        break;
                    }
                    else
                    {
                        foundTry++;
                        Thread.Sleep(250);
                    }
                } while (foundTry < 50);

                if (foundLink)
                {
                    lock (_synclocker3)
                    {
                        _chosenArticles[index].Add(chosenSourceArticle.articleUrl);
                    }

                    //try to download article & its images
                    int retry = 0;
                    Article needDownload = new Article();
                    bool isDownloaded = false;

                    while (retry < 3)
                    {
                        try
                        {
                            //download article
                            HttpWebRequest request = WebRequest.Create(chosenSourceArticle.articleUrl) as HttpWebRequest;
                            int pos = 0;

                            if (ProxyHandler.Instance.PostingDownloadArticles != ProxyType.Disabled)
                            {
                                //get proxy
                                WebProxy p;
                                do
                                {
                                    bool result = false;
                                    ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                    if (!result)
                                        Thread.Sleep(150);
                                    else
                                    {
                                        p = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|Post|Thread[" + index + "]", ProxyHandler.Instance.PostingDownloadArticles);
                                        ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                        break;
                                    }
                                } while (true);

                                //implement the proxy
                                request.Proxy = p;
                            }
                            request.Timeout = 30000;
                            request.KeepAlive = false;
                            request.ProtocolVersion = HttpVersion.Version10;
                            request.Method = "GET";
                            request.Accept = "text/html,application/xhtml+xml,application/xml";
                            //request.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                            request.ServicePoint.ConnectionLimit = 1;
                            Random agentrand = new Random();
                            pos = agentrand.Next(SysSettings.Instance._agents.Count);
                            request.UserAgent = SysSettings.Instance._agents[pos];

                            var document = new HtmlDocument();

                            using (var response = (HttpWebResponse)request.GetResponse())
                            {
                                using (var responseStream = response.GetResponseStream())
                                {
                                    Encoding encoding = Encoding.UTF8;
                                    using (var reader = new StreamReader(responseStream))
                                    {
                                        document.Load(reader.BaseStream, encoding);
                                    }
                                    request.Abort();
                                }
                            }

                            needDownload = new Article(chosenSourceArticle.articleUrl, chosenSourceArticle.harvestUrl, document, GetArticleType(chosenSourceArticleLink));
                            IScraper scraper = ScraperFactory.CreateScraper(needDownload);

                            needDownload.title = scraper.GetTitleFor(needDownload.doc, needDownload.type, needDownload.url);
                            needDownload.publish = scraper.GetPublish(needDownload.doc, needDownload.type);
                            needDownload.shampoo = scraper.GetShampoo(needDownload.doc, needDownload.type);
                            needDownload.image = scraper.GetImage(needDownload.doc, needDownload.type, needDownload.url);
                            needDownload.htmlBody = scraper.GetHtmlBody(needDownload.doc, needDownload.type, needDownload.url);
                            needDownload.SEOTitle = scraper.GetSEOTitle(needDownload.doc, needDownload.type);
                            needDownload.SEODescription = scraper.GetSEODescription(needDownload.doc, needDownload.type);
                            needDownload.Tags = scraper.GetTags(needDownload.doc, needDownload.type);
                            needDownload.Sentences = scraper.GetSentences(needDownload.doc, needDownload.type);
                            needDownload.downloaded = DateTime.Now;

                            //remove all black words
                            needDownload.htmlBody = SysSettings.Instance.Article_Scraping_FilterWords.Replace(needDownload.htmlBody);
                            needDownload.shampoo = SysSettings.Instance.Article_Scraping_FilterWords.Replace(needDownload.shampoo);
                            needDownload.SEOTitle = SysSettings.Instance.Article_Scraping_FilterWords.Replace(needDownload.SEOTitle);
                            needDownload.SEODescription = SysSettings.Instance.Article_Scraping_FilterWords.Replace(needDownload.SEODescription);

                            //Download & mixing images & replace them

                            //step 1: filter images
                            List<string> bodyImages = new List<string>();
                            bodyImages = GetImagesInHTMLString(needDownload.htmlBody);

                            //step 2: download & mixing image
                            List<DriveImages> uploadImages = new List<DriveImages>();

                            //foreach (var i in bodyImages)
                            //{
                            //    int imgRetry = 0;
                            //    while (imgRetry < 3)
                            //    {
                            //        try
                            //        {
                            //            System.Drawing.Bitmap tmp = null;
                            //            request = (HttpWebRequest)HttpWebRequest.Create(i);

                            //            if (ProxyHandler.Instance.PostingDownloadArticles != ProxyType.Disabled)
                            //            {
                            //                //get proxy
                            //                WebProxy p;
                            //                do
                            //                {
                            //                    bool result = false;
                            //                    ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                            //                    if (!result)
                            //                        Thread.Sleep(150);
                            //                    else
                            //                    {
                            //                        p = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|Post|Thread[" + index + "]", ProxyHandler.Instance.PostingDownloadArticles);
                            //                        ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                            //                        break;
                            //                    }
                            //                } while (true);

                            //                //implement the proxy
                            //                request.Proxy = p;
                            //            }
                            //            request.Timeout = 30000;
                            //            request.ReadWriteTimeout = 60000;
                            //            request.KeepAlive = false;
                            //            request.ProtocolVersion = HttpVersion.Version10;
                            //            request.Method = "GET";
                            //            request.Accept = "text/html,application/xhtml+xml,application/xml";
                            //            pos = agentrand.Next(SysSettings.Instance._agents.Count);
                            //            request.UserAgent = SysSettings.Instance._agents[pos];

                            //            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            //            Stream stream = response.GetResponseStream();

                            //            tmp = (Bitmap)System.Drawing.Bitmap.FromStream(stream);
                            //            Bitmap remake = ImageHelper.RemakeImage(tmp, _logoPath);
                            //            EncoderParameter parameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)0.5);
                            //            ImageCodecInfo encoder = ImageHelper.getEncoder(ImageHelper.getFormat(i));
                            //            if (encoder != null)
                            //            {
                            //                EncoderParameters encoderParams = new EncoderParameters(1);
                            //                encoderParams.Param[0] = parameter;
                            //                string fileName = System.IO.Path.GetFileNameWithoutExtension(i) + string.Format("{0}-{1}-{2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                            //                fileName += System.IO.Path.GetExtension(i);

                            //                string fullPath = SysSettings.Instance._tempPath + fileName;
                            //                remake.Save(fullPath, encoder, encoderParams);
                            //                uploadImages.Add(new DriveImages(i, fullPath, ""));
                            //            }
                            //            Thread.Sleep(rnd.Next(3000, 5000));
                            //            break;
                            //        }
                            //        catch (Exception ex)
                            //        {
                            //            imgRetry++;
                            //            Thread.Sleep(rnd.Next(3000, 5000));
                            //        }
                            //    }
                            //}

                            //step 3: upload image to drive
                            foreach (var img in uploadImages)
                            {
                                Google.Apis.Drive.v2.Data.File imgUpload = new Google.Apis.Drive.v2.Data.File();
                                imgUpload.Title = System.IO.Path.GetFileNameWithoutExtension(img.SourcePath);
                                imgUpload.MimeType = ImageHelper.getMIMEType(img.SourcePath);
                                imgUpload.Parents = new List<ParentReference>() { new ParentReference() { Id = _sharedFolder.Id } };

                                byte[] byteArray = System.IO.File.ReadAllBytes(img.SourcePath);
                                System.IO.MemoryStream stream = new MemoryStream(byteArray);

                                FilesResource.InsertMediaUpload gRequest = service.Files.Insert(imgUpload, stream, imgUpload.MimeType);
                                gRequest.Upload();

                                Google.Apis.Drive.v2.Data.File uploadedImg = gRequest.ResponseBody;
                                img.DrivePath = "https://drive.google.com/uc?export=download&id=" + uploadedImg.Id;
                            }

                            //step 4: replace old image with new uploaded images
                            foreach (var i in uploadImages)
                            {
                                System.IO.File.Delete(i.SourcePath);
                                needDownload.htmlBody = needDownload.htmlBody.Replace(i.SourceUrl, i.DrivePath);
                            }
                            isDownloaded = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            errorMessage += ex.Message + " | ";
                            retry++;
                            Thread.Sleep(rnd.Next(3000, 5000));
                        }
                    }

                    if (isDownloaded)
                    {
                        message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Download article " + chosenSourceArticle.articleUrl + " completely!");
                        AutoPost.Instance.AddMessages(message);
                        WriteLog(message, false, "");

                        string body = "";

                        foreach (var rule in _currentSitePostRule)
                        {
                            if (chosenSourceArticle.harvestUrl.Contains(HttpUtility.UrlDecode(rule.SourceCategory)) && chosenSourceArticle.harvestUrl.Contains(rule.SourceHost))
                            {
                                message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Begin posting " + needDownload.title + " to website " + rule.SiteHost + " !");
                                AutoPost.Instance.AddMessages(message);
                                WriteLog(message, false, "");

                                //get a copy of download article
                                Article temp = needDownload.Clone();

                                //get site category that article will be published
                                string host = rule.SiteHost;
                                long chosenPostSiteCategoryId = rule.SiteCategoryID;

                                //get category SEO rule for this particular article
                                SEORule chosenSeoRule = _seoRuleCollection.GetRule(host);
                                CategorySEORule chosenRule = _seoRuleCollection.GetCategorySEORule(host, chosenPostSiteCategoryId);

                                if (chosenRule != null)
                                {
                                    //Phase 1: insert SEO rules
                                    string plainBody = temp.body;

                                    List<SEOSentence> replacement = new List<SEOSentence>();
                                    List<string> chosenKeywords = new List<string>();

                                    int primaryKeywordSlots = (int)(chosenRule.TotalKeywords * chosenRule.PrimaryKeywordPercentage) / 100;
                                    int secondaryKeywordSlots = (int)(chosenRule.TotalKeywords * chosenRule.SecondaryKeywordPercentage) / 100;
                                    int genericKeywordSlots = (int)(chosenRule.TotalKeywords * chosenRule.GenericKeywordPercentage) / 100;

                                    int notUsedSlots = chosenRule.TotalKeywords - primaryKeywordSlots - secondaryKeywordSlots - genericKeywordSlots;

                                    if (notUsedSlots > 0)
                                    {
                                        while (notUsedSlots > 0)
                                        {
                                            genericKeywordSlots++;
                                            notUsedSlots--;
                                        }
                                    }

                                    if (primaryKeywordSlots > 0)
                                        InsertKeywordsAndLinks(plainBody, temp.Sentences, ref replacement, chosenRule, KeywordType.Primary, primaryKeywordSlots, ref chosenKeywords);
                                    if (secondaryKeywordSlots > 0)
                                        InsertKeywordsAndLinks(plainBody, temp.Sentences, ref replacement, chosenRule, KeywordType.Secondary, secondaryKeywordSlots, ref chosenKeywords);
                                    if (genericKeywordSlots > 0)
                                        InsertKeywordsAndLinks(plainBody, temp.Sentences, ref replacement, chosenRule, KeywordType.Generic, genericKeywordSlots, ref chosenKeywords);

                                    if (chosenKeywords.Count == 0)
                                        MergeAllKeyword(chosenRule, ref chosenKeywords);

                                    foreach (var rep in replacement)
                                    {
                                        temp.htmlBody = temp.htmlBody.Replace(rep._originalSentence, rep._seoSentence);
                                    }

                                    body = temp.htmlBody;
                                    
                                    if (chosenRule.InsertAuthorityLinks)
                                    {
                                        IAuthority tager = TagFactory.CreateTager(chosenRule.AuthoritySearch);

                                        if (ProxyHandler.Instance.PostingDownloadArticles != ProxyType.Disabled)
                                        {
                                            WebProxy tagProxy;
                                            do
                                            {
                                                bool result = false;
                                                ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                                if (!result)
                                                    Thread.Sleep(150);
                                                else
                                                {
                                                    tagProxy = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|Post|Thread[" + index + "]", ProxyHandler.Instance.PostingDownloadArticles);
                                                    ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                                    break;
                                                }
                                            } while (true);

                                            tager.InsertTags(chosenRule.AuthorityKeywords, chosenRule.AuthorityApperance, ref body, needDownload.Tags, new List<string>() { }, tagProxy, SysSettings.Instance._agents);
                                        }
                                        else
                                            tager.InsertTags(chosenRule.AuthorityKeywords, chosenRule.AuthorityApperance, ref body, needDownload.Tags, new List<string>() { }, SysSettings.Instance._agents);
                                    }

                                    if (chosenRule.InsertVideo)
                                    {
                                        IVideo video = VideoFactory.CreateVideoInserter(chosenRule.VideoSearch);

                                        if (ProxyHandler.Instance.PostingDownloadArticles != ProxyType.Disabled)
                                        {
                                            WebProxy videoProxy;
                                            do
                                            {
                                                bool result = false;
                                                ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                                if (!result)
                                                    Thread.Sleep(150);
                                                else
                                                {
                                                    videoProxy = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|Post|Thread[" + index + "]", ProxyHandler.Instance.PostingDownloadArticles);
                                                    ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                                    break;
                                                }
                                            } while (true);

                                            video.InsertVideo(chosenRule.VideoKeywords, ref body, needDownload.Tags, new List<string>() { }, videoProxy, SysSettings.Instance._agents);
                                        }
                                        else
                                            video.InsertVideo(chosenRule.VideoKeywords, ref body, needDownload.Tags, new List<string>() { }, SysSettings.Instance._agents);
                                    }

                                    //get wordpress wrapper of current site
                                    string chosenHost = _postSites[index];
                                    PostSites chosenSite = GetPostSiteByHost(chosenHost);
                                    chosenSite.InitializeWordpress();

                                    if (chosenRule.InsertInternalLink)
                                    {
                                        ByTagInternalInserter tagInternalInserter = new ByTagInternalInserter();
                                        tagInternalInserter.InsertInternalLink(chosenRule.InternalKeywords, ref body, needDownload.Tags, new List<string>() { }, chosenSite.Wordpress, SysSettings.Instance._agents);
                                    }

                                    temp.htmlBody = body;

                                    //Phase 3: upload article to Wordpress
                                    
                                    //download & upload featured image
                                    MediaObjectInfo featuredImage = new MediaObjectInfo();
                                    bool isFeaturedImageUpload = false;
                                    if (temp.image != "not-found-image")
                                    {
                                        string featuredImagePath = DownloadFeaturedImage(temp.image, _projectName + "|Post|Thread[" + index + "]");

                                        if (featuredImagePath != string.Empty)
                                        {
                                            byte[] imageData = System.IO.File.ReadAllBytes(featuredImagePath);
                                            int fiRetry = 0;

                                            while (fiRetry < 3)
                                            {
                                                try
                                                {
                                                    featuredImage = chosenSite.Wordpress.NewMediaObject(new MediaObject { Bits = imageData, Name = System.IO.Path.GetFileName(featuredImagePath), Type = ImageHelper.getMIMEType(featuredImagePath) });
                                                    isFeaturedImageUpload = true;
                                                    break;
                                                }
                                                catch (Exception ex)
                                                {
                                                    fiRetry++;
                                                    Thread.Sleep(rnd.Next(300, 500));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        IAdvance advanceScraper = AdvanceScraperFactory.GetAdvanceScraper(chosenSourceArticle.articleUrl);

                                        string articleFeaturedImage = "";

                                        if (ProxyHandler.Instance.PostingDownloadArticles != ProxyType.Disabled)
                                        {
                                            WebProxy imageProxy;
                                            do
                                            {
                                                bool result = false;
                                                ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                                if (!result)
                                                    Thread.Sleep(150);
                                                else
                                                {
                                                    imageProxy = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|Post|Thread[" + index + "]", ProxyHandler.Instance.PostingDownloadArticles);
                                                    ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                                    break;
                                                }
                                            } while (true);

                                            articleFeaturedImage = advanceScraper.GetImageSearchPhrase(temp.doc, imageProxy, SysSettings.Instance._agents);
                                        }
                                        else
                                            articleFeaturedImage = advanceScraper.GetImageSearchPhrase(temp.doc, SysSettings.Instance._agents);

                                        string featuredImagePath = DownloadFeaturedImage(articleFeaturedImage, _projectName + "|Post|Thread[" + index + "]");

                                        if (featuredImagePath != string.Empty)
                                        {
                                            byte[] imageData = System.IO.File.ReadAllBytes(featuredImagePath);
                                            int fiRetry = 0;

                                            while (fiRetry < 3)
                                            {
                                                try
                                                {
                                                    featuredImage = chosenSite.Wordpress.NewMediaObject(new MediaObject { Bits = imageData, Name = System.IO.Path.GetFileName(featuredImagePath), Type = ImageHelper.getMIMEType(featuredImagePath) });
                                                    isFeaturedImageUpload = true;
                                                    break;
                                                }
                                                catch (Exception ex)
                                                {
                                                    fiRetry++;
                                                    Thread.Sleep(rnd.Next(300, 500));
                                                }
                                            }
                                        }
                                    }
                                    var post = new Post(); // co rule
                                    post.DateCreated = TimeZoneInfo.ConvertTime(DateTime.Now.AddMinutes(-1), chosenSite.TimeZone);
                                    post.Title = temp.title;
                                    post.Body = temp.htmlBody;
                                    post.Categories = new string[] { chosenSite.GetCategoryById(chosenRule.CategoryID) };
                                    if (isFeaturedImageUpload)
                                        post.PostThumbnail = featuredImage.ID;

                                    chosenSeoRule.SeoPlugin.SetupSEOFactors(temp.SEOTitle, temp.SEODescription, "", "", chosenKeywords);
                                    var cfs = chosenSeoRule.SeoPlugin.createSEOFactors();
                                    post.CustomFields = cfs;

                                    int postRetry = 0;
                                    bool isPosted = false;
                                    int postId = -1;
                                    Post posted = new Post();
                                    while (postRetry < 3)
                                    {
                                        try
                                        {
                                            postId = chosenSite.Wordpress.NewPost(post, true);
                                            posted = chosenSite.Wordpress.GetPost(postId);
                                            isPosted = true;
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            errorMessage += ex.Message + " | ";
                                            postRetry++;
                                            Thread.Sleep(rnd.Next(300, 500));
                                        }
                                    }
                                    if (isPosted)
                                    {
                                        //write log & message
                                        message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Finish posting " + needDownload.title + " to website " + rule.SiteHost + " !");
                                        AutoPost.Instance.AddMessages(message);
                                        WriteLog(message, true, needDownload.url + "|" + rule.SiteHost);

                                        //try to indexed the links
                                        IndexerHandler.Instance.InsertIndexUrls(posted.Permalink);
                                    }
                                    else
                                    {
                                        message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Post failed " + needDownload.title + " to website " + rule.SiteHost + " ! Reason : " + errorMessage);
                                        AutoPost.Instance.AddMessages(message);
                                        WriteLog(message, false, "");
                                    }
                                    Thread.Sleep(5000);
                                }
                                else
                                {
                                    //get wordpress wrapper of current site
                                    string chosenHost = _postSites[index];
                                    PostSites chosenSite = GetPostSiteByHost(chosenHost);
                                    chosenSite.InitializeWordpress();

                                    //download & upload featured image
                                    MediaObjectInfo featuredImage = new MediaObjectInfo();
                                    bool isFeaturedImageUpload = false;
                                    if (temp.image != "not-found-image")
                                    {
                                        string featuredImagePath = DownloadFeaturedImage(temp.image, _projectName + "|Post|Thread[" + index + "]");

                                        if (featuredImagePath != string.Empty)
                                        {
                                            byte[] imageData = System.IO.File.ReadAllBytes(featuredImagePath);
                                            int fiRetry = 0;

                                            while (fiRetry < 3)
                                            {
                                                try
                                                {
                                                    featuredImage = chosenSite.Wordpress.NewMediaObject(new MediaObject { Bits = imageData, Name = System.IO.Path.GetFileName(featuredImagePath), Type = ImageHelper.getMIMEType(featuredImagePath) });
                                                    isFeaturedImageUpload = true;
                                                    break;
                                                }
                                                catch (Exception ex)
                                                {
                                                    fiRetry++;
                                                    Thread.Sleep(rnd.Next(300, 500));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        IAdvance advanceScraper = AdvanceScraperFactory.GetAdvanceScraper(chosenSourceArticle.articleUrl);

                                        string articleFeaturedImage = "";

                                        if (ProxyHandler.Instance.PostingDownloadArticles != ProxyType.Disabled)
                                        {
                                            WebProxy imageProxy;
                                            do
                                            {
                                                bool result = false;
                                                ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                                if (!result)
                                                    Thread.Sleep(150);
                                                else
                                                {
                                                    imageProxy = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|Post|Thread[" + index + "]", ProxyHandler.Instance.PostingDownloadArticles);
                                                    ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|Post|Thread[" + index + "]", ref result);
                                                    break;
                                                }
                                            } while (true);

                                            articleFeaturedImage = advanceScraper.GetImageSearchPhrase(temp.doc, imageProxy, SysSettings.Instance._agents);
                                        }
                                        else
                                            articleFeaturedImage = advanceScraper.GetImageSearchPhrase(temp.doc, SysSettings.Instance._agents);

                                        string featuredImagePath = DownloadFeaturedImage(articleFeaturedImage, _projectName + "|Post|Thread[" + index + "]");

                                        if (featuredImagePath != string.Empty)
                                        {
                                            byte[] imageData = System.IO.File.ReadAllBytes(featuredImagePath);
                                            int fiRetry = 0;

                                            while (fiRetry < 3)
                                            {
                                                try
                                                {
                                                    featuredImage = chosenSite.Wordpress.NewMediaObject(new MediaObject { Bits = imageData, Name = System.IO.Path.GetFileName(featuredImagePath), Type = ImageHelper.getMIMEType(featuredImagePath) });
                                                    isFeaturedImageUpload = true;
                                                    break;
                                                }
                                                catch (Exception ex)
                                                {
                                                    fiRetry++;
                                                    Thread.Sleep(rnd.Next(300, 500));
                                                }
                                            }
                                        }
                                    }
                                    var post = new Post(); // khong co rule
                                    post.DateCreated = TimeZoneInfo.ConvertTime(DateTime.Now.AddMinutes(-1), chosenSite.TimeZone);
                                    post.Title = temp.title;
                                    post.Body = temp.htmlBody;
                                    post.Categories = new string[] { chosenSite.GetCategoryById(chosenPostSiteCategoryId) };
                                    if (isFeaturedImageUpload)
                                        post.PostThumbnail = featuredImage.ID;

                                    //if (temp.Tags.Count > 0)
                                    //    chosenSeoRule.SeoPlugin.SetupSEOFactors(temp.SEOTitle, temp.SEODescription, "", "", temp.Tags);                                    
                                    //else
                                    //    chosenSeoRule.SeoPlugin.SetupSEOFactors(temp.SEOTitle, temp.SEODescription, "", "", new List<string>());
                                    //var cfs = chosenSeoRule.SeoPlugin.createSEOFactors();
                                    //post.CustomFields = cfs;

                                    int postRetry = 0;
                                    bool isPosted = false;
                                    int postId = -1;
                                    Post posted = new Post();
                                    //while (postRetry < 3)
                                    //{
                                    //    try
                                    //    {
                                    //        postId = chosenSite.Wordpress.NewPost(post, true);
                                    //        posted = chosenSite.Wordpress.GetPost(postId);
                                    //        isPosted = true;
                                    //        break;
                                    //    }
                                    //    catch (Exception ex)
                                    //    {
                                    //        errorMessage += ex.Message + " | ";
                                    //        postRetry++;
                                    //        Thread.Sleep(rnd.Next(300, 500));
                                    //    }
                                    //}

                                    try
                                    {
                                        postId = chosenSite.Wordpress.NewPost(post, true);
                                        posted = chosenSite.Wordpress.GetPost(postId);
                                        isPosted = true;
                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        errorMessage += ex.Message + ", StackTrace: " + ex.StackTrace;
                                    }

                                    if (isPosted)
                                    {
                                        //write log & message
                                        message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Finish posting " + needDownload.title + " to website " + rule.SiteHost + " !");
                                        AutoPost.Instance.AddMessages(message);
                                        WriteLog(message, true, needDownload.url + "|" + rule.SiteHost);

                                        //try to indexed the links
                                        IndexerHandler.Instance.InsertIndexUrls(posted.Permalink);
                                    }
                                    else
                                    {
                                        message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Post failed " + needDownload.title + " to website " + rule.SiteHost + " ! Reason : " + errorMessage);
                                        AutoPost.Instance.AddMessages(message);
                                        WriteLog(message, false, "");
                                        WriteLog(message, true, needDownload.url + "|" + rule.SiteHost);
                                    }
                                    Thread.Sleep(5000);
                                }
                            }
                        }
                    }
                    else
                    {
                        message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Download article " + chosenSourceArticle.articleUrl + " failed! Ignore posting this time. Errors : " + errorMessage);
                        AutoPost.Instance.AddMessages(message);
                        WriteLog(message, false, "");
                    }
                    if (!_stopWorker) postWorker_Completed();
                }
                else
                {
                    message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Cannot find any link to post! Ignore posting this time.");
                    AutoPost.Instance.AddMessages(message);
                    WriteLog(message, false, "");
                }
            }
            else
            {
                message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Cannot find any link to post! Ignore posting this time.");
                AutoPost.Instance.AddMessages(message);
                WriteLog(message, false, "");
            }
            
            StopThread: if (_stopWorker) StopWorker(false);
        }

        /// <summary>
        /// Handle postWorker completed operation
        /// </summary>
        private void postWorker_Completed()
        {
            lock (_finishPostLocker)
            {
                _finishPostWorker++;
            }
            if (_finishPostWorker == _totalPostWorker)
            {
                string message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Post articles has been done!");
                AutoPost.Instance.AddMessages(message);
                WriteLog(message, false, "");
            }
        }

        /// <summary>
        /// Database Post worker DoWork function
        /// </summary>
        /// <param name="Index"></param>
        private void databasePostWorker_DoWork(object Index)
        {
            int index = (int)Index;

            List<PostRule> _currentSitePostRule = _ruleCollection.GetRulesBySiteHost(_postSites[index]);
            Record item = database_chosenRecords[index].Dequeue();

            Random rnd = new Random();
            Random proxyrand = new Random();
            string body = "";
            HttpWebRequest request;
            int pos = 0;
            WebProxy p;

            lock (_stopLocker)
            {
                if (_stopWorker)
                    goto StopThread;
            }
            string message = "";
            var document = new HtmlDocument();
            document.Load(item.filePath);

            Article downloadedArticle = new Article(item.url, item.hUrl, document, GetArticleType(item.url));
            IScraper scraper = ScraperFactory.CreateScraper(downloadedArticle);

            downloadedArticle.title = scraper.GetTitleFor(downloadedArticle.doc, downloadedArticle.type, downloadedArticle.url);
            downloadedArticle.publish = scraper.GetPublish(downloadedArticle.doc, downloadedArticle.type);
            downloadedArticle.shampoo = scraper.GetShampoo(downloadedArticle.doc, downloadedArticle.type);
            downloadedArticle.image = scraper.GetImage(downloadedArticle.doc, downloadedArticle.type, downloadedArticle.url);
            downloadedArticle.htmlBody = scraper.GetHtmlBody(downloadedArticle.doc, downloadedArticle.type, downloadedArticle.url);
            downloadedArticle.SEOTitle = scraper.GetSEOTitle(downloadedArticle.doc, downloadedArticle.type);
            downloadedArticle.SEODescription = scraper.GetSEODescription(downloadedArticle.doc, downloadedArticle.type);
            downloadedArticle.Tags = scraper.GetTags(downloadedArticle.doc, downloadedArticle.type);
            downloadedArticle.Sentences = scraper.GetSentences(downloadedArticle.doc, downloadedArticle.type);
            downloadedArticle.downloaded = DateTime.Now;

            //remove all black words            
            downloadedArticle.htmlBody = SysSettings.Instance.Article_Scraping_FilterWords.Replace(downloadedArticle.htmlBody);
            downloadedArticle.shampoo = SysSettings.Instance.Article_Scraping_FilterWords.Replace(downloadedArticle.shampoo);
            downloadedArticle.SEOTitle = SysSettings.Instance.Article_Scraping_FilterWords.Replace(downloadedArticle.SEOTitle);
            downloadedArticle.SEODescription = SysSettings.Instance.Article_Scraping_FilterWords.Replace(downloadedArticle.SEODescription);

            //Download & mixing images & replace them

            //step 1: filter images
            List<string> bodyImages = new List<string>();
            bodyImages = GetImagesInHTMLString(downloadedArticle.htmlBody);

            //step 2: download & mixing image
            List<DriveImages> uploadImages = new List<DriveImages>();

            foreach (var i in bodyImages)
            {
                int imgRetry = 0;
                while (imgRetry < 3)
                {
                    try
                    {
                        System.Drawing.Bitmap tmp = null;
                        request = (HttpWebRequest)HttpWebRequest.Create(i);                        

                        do
                        {
                            bool result = false;
                            ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|DatabasePost|Thread[" + index + "]", ref result);
                            if (!result)
                                Thread.Sleep(150);
                            else
                            {
                                p = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|DatabasePost|Thread[" + index + "]", ProxyHandler.Instance.PostingDownloadArticles);
                                ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|DatabasePost|Thread[" + index + "]", ref result);
                                break;
                            }
                        } while (true);

                        request.Proxy = p;
                        request.Timeout = 30000;
                        request.ReadWriteTimeout = 60000;
                        request.KeepAlive = false;
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream stream = response.GetResponseStream();

                        tmp = (Bitmap)System.Drawing.Bitmap.FromStream(stream);
                        Bitmap remake = ImageHelper.RemakeImage(tmp, _logoPath);
                        EncoderParameter parameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)0.5);
                        ImageCodecInfo encoder = ImageHelper.getEncoder(ImageHelper.getFormat(i));
                        if (encoder != null)
                        {
                            EncoderParameters encoderParams = new EncoderParameters(1);
                            encoderParams.Param[0] = parameter;
                            string fileName = System.IO.Path.GetFileNameWithoutExtension(i) + string.Format("{0}-{1}-{2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                            fileName += System.IO.Path.GetExtension(i);

                            string fullPath = SysSettings.Instance._tempPath + fileName;
                            remake.Save(fullPath, encoder, encoderParams);
                            uploadImages.Add(new DriveImages(i, fullPath, ""));
                        }
                        Thread.Sleep(rnd.Next(3000, 5000));
                        break;
                    }
                    catch (Exception ex)
                    {
                        imgRetry++;
                        Thread.Sleep(rnd.Next(3000, 5000));
                    }
                }
            }

            //step 3: upload image to drive
            foreach (var img in uploadImages)
            {
                Google.Apis.Drive.v2.Data.File imgUpload = new Google.Apis.Drive.v2.Data.File();
                imgUpload.Title = System.IO.Path.GetFileNameWithoutExtension(img.SourcePath);
                imgUpload.MimeType = ImageHelper.getMIMEType(img.SourcePath);
                imgUpload.Parents = new List<ParentReference>() { new ParentReference() { Id = _sharedFolder.Id } };

                byte[] byteArray = System.IO.File.ReadAllBytes(img.SourcePath);
                System.IO.MemoryStream stream = new MemoryStream(byteArray);

                FilesResource.InsertMediaUpload gRequest = service.Files.Insert(imgUpload, stream, imgUpload.MimeType);
                gRequest.Upload();

                Google.Apis.Drive.v2.Data.File uploadedImg = gRequest.ResponseBody;
                img.DrivePath = "https://drive.google.com/uc?export=download&id=" + uploadedImg.Id;
            }

            //step 4: replace old image with new uploaded images
            foreach (var i in uploadImages)
            {
                System.IO.File.Delete(i.SourcePath);
                downloadedArticle.htmlBody = downloadedArticle.htmlBody.Replace(i.SourceUrl, i.DrivePath);
            }

            message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Get article " + downloadedArticle.title + " from database completely!");
            AutoPost.Instance.AddMessages(message);
            WriteLog(message, false, "");

            foreach (var rule in _currentSitePostRule)
            {
                if (item.url.Contains(rule.SourceCategory))
                {
                    message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Begin posting " + downloadedArticle.title + " to website " + rule.SiteHost + " !");
                    AutoPost.Instance.AddMessages(message);
                    WriteLog(message, false, "");

                    //get a copy of download article
                    Article temp = downloadedArticle.Clone();

                    //get site category that article will be published
                    string host = rule.SiteHost;
                    long chosenPostSiteCategoryId = rule.SiteCategoryID;

                    //get category SEO rule for this particular article
                    SEORule chosenSeoRule = _seoRuleCollection.GetRule(host);
                    CategorySEORule chosenRule = _seoRuleCollection.GetCategorySEORule(host, chosenPostSiteCategoryId);

                    //Phase 1: insert SEO rules
                    string plainBody = HtmlSanitizer.StripHtml(temp.htmlBody);
                    plainBody = Regex.Replace(plainBody, @"(\r\n){2,}", "\r\n\r\n"); //remove multiple blank lines

                    List<SEOSentence> replacement = new List<SEOSentence>();                    
                    List<string> chosenKeywords = new List<string>();

                    int primaryKeywordSlots = (int)(chosenRule.TotalKeywords * chosenRule.PrimaryKeywordPercentage) / 100;
                    int secondaryKeywordSlots = (int)(chosenRule.TotalKeywords * chosenRule.SecondaryKeywordPercentage) / 100;
                    int genericKeywordSlots = (int)(chosenRule.TotalKeywords * chosenRule.GenericKeywordPercentage) / 100;

                    int notUsedSlots = chosenRule.TotalKeywords - primaryKeywordSlots - secondaryKeywordSlots - genericKeywordSlots;

                    if (notUsedSlots > 0)
                    {
                        while (notUsedSlots > 0)
                        {
                            genericKeywordSlots++;
                            notUsedSlots--;
                        }
                    }

                    if (primaryKeywordSlots > 0)
                        InsertKeywordsAndLinks(plainBody, temp.Sentences, ref replacement, chosenRule, KeywordType.Primary, primaryKeywordSlots, ref chosenKeywords);
                    if (secondaryKeywordSlots > 0)
                        InsertKeywordsAndLinks(plainBody, temp.Sentences, ref replacement, chosenRule, KeywordType.Secondary, secondaryKeywordSlots, ref chosenKeywords);
                    if (genericKeywordSlots > 0)
                        InsertKeywordsAndLinks(plainBody, temp.Sentences, ref replacement, chosenRule, KeywordType.Generic, genericKeywordSlots, ref chosenKeywords);

                    foreach (var rep in replacement)
                    {
                        temp.htmlBody = temp.htmlBody.Replace(rep._originalSentence, rep._seoSentence);
                    }

                    body = temp.htmlBody;

                    WebProxy tagProxy;
                    do
                    {
                        bool result = false;
                        ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|DatabasePost|Thread[" + index + "]", ref result);
                        if (!result)
                            Thread.Sleep(150);
                        else
                        {
                            tagProxy = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|DatabasePost|Thread[" + index + "]", ProxyHandler.Instance.PostingDownloadArticles);
                            ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|DatabasePost|Thread[" + index + "]", ref result);
                            break;
                        }
                    } while (true);

                    if (chosenRule.InsertAuthorityLinks)
                    {
                        IAuthority tager = TagFactory.CreateTager(chosenRule.AuthoritySearch);
                        tager.InsertTags(chosenRule.AuthorityKeywords, chosenRule.AuthorityApperance, ref body, downloadedArticle.Tags, new List<string>() { }, tagProxy, SysSettings.Instance._agents);
                    }

                    if (chosenRule.InsertVideo)
                    {
                        IVideo video = VideoFactory.CreateVideoInserter(chosenRule.VideoSearch);
                        video.InsertVideo(chosenRule.VideoKeywords, ref body, downloadedArticle.Tags, new List<string>() { }, tagProxy, SysSettings.Instance._agents);
                    }

                    temp.htmlBody = body;                    

                    //Phase 3: upload article to Wordpress

                    //get wordpress wrapper of current site
                    string chosenHost = _postSites[index];
                    PostSites chosenSite = GetPostSiteByHost(chosenHost);
                    chosenSite.InitializeWordpress();

                    //download & upload featured image
                    MediaObjectInfo featuredImage = new MediaObjectInfo();
                    bool isFeaturedImageUpload = false;
                    if (temp.image != "not-found-image")
                    {
                        string featuredImagePath = DownloadFeaturedImage(temp.image, _projectName + "|DatabasePost|Thread[" + index + "]");

                        if (featuredImagePath != string.Empty)
                        {
                            byte[] imageData = System.IO.File.ReadAllBytes(featuredImagePath);
                            int fiRetry = 0;

                            while (fiRetry < 3)
                            {
                                try
                                {
                                    featuredImage = chosenSite.Wordpress.NewMediaObject(new MediaObject { Bits = imageData, Name = System.IO.Path.GetFileName(featuredImagePath), Type = ImageHelper.getMIMEType(featuredImagePath) });
                                    isFeaturedImageUpload = true;
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    fiRetry++;
                                    Thread.Sleep(rnd.Next(300, 500));
                                }
                            }
                        }
                    }
                    var post = new Post();
                    post.DateCreated = TimeZoneInfo.ConvertTime(DateTime.Now.AddMinutes(-1), chosenSite.TimeZone);
                    post.Title = temp.title;
                    post.Body = temp.htmlBody;
                    post.Categories = new string[] { chosenSite.GetCategoryById(chosenRule.CategoryID) };
                    if (isFeaturedImageUpload)
                        post.PostThumbnail = featuredImage.ID;

                    chosenSeoRule.SeoPlugin.SetupSEOFactors(temp.SEOTitle, temp.SEODescription, "", "", chosenKeywords);
                    var cfs = chosenSeoRule.SeoPlugin.createSEOFactors();
                    post.CustomFields = cfs;
                    
                    int postRetry = 0;
                    bool isPosted = false;
                    while (postRetry < 3)
                    {
                        try
                        {
                            chosenSite.Wordpress.NewPost(post, true);
                            isPosted = true;
                            break;
                        }
                        catch (Exception)
                        {
                            postRetry++;
                            Thread.Sleep(rnd.Next(300, 500));
                        }
                    }
                    if (isPosted)
                    {
                        message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Finish posting " + downloadedArticle.title + " to website " + rule.SiteHost + " !");
                        AutoPost.Instance.AddMessages(message);
                        WriteLog(message, true, downloadedArticle.url + "|" + rule.SiteHost);
                    }
                    else
                    {
                        message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Post failed " + downloadedArticle.title + " to website " + rule.SiteHost + " !");
                        AutoPost.Instance.AddMessages(message);
                        WriteLog(message, false, "");
                    }
                    Thread.Sleep(5000);
                }
            }
            StopThread: if (_stopWorker) StopWorker();
            databasePostWorker_Completed();
        }

        /// <summary>
        /// Handle database postWorker completed operation
        /// </summary>
        private void databasePostWorker_Completed()
        {
            lock (_finishDatabasePostLocker)
            {
                _finishDatabasePostWorker++;
            }
            if (_finishDatabasePostWorker == _totalDatabasePostWorker)
            {
                string message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Post articles completely!");
                AutoPost.Instance.AddMessages(message);
                WriteLog(message, false, "");
            }
        }

        /// <summary>
        /// Stop function for worker threads
        /// </summary>
        private void StopWorker(bool flag)
        {
            if (flag)
            {
                lock (_finishHarvestLocker)
                    _finishHarvestWorker++;
            }
            else
            {
                lock (_finishPostLocker)
                    _finishPostWorker++;
            }
        }

        /// <summary>
        /// Stop function for database worker threads
        /// </summary>
        private void StopWorker()
        {
            lock (_finishDatabasePostLocker)
                _finishDatabasePostWorker++;
        }

        /// <summary>
        /// StopWatcher DoWork function
        /// </summary>
        private void StopWatcher_DoWork()
        {
            do
            {
                if (_scheduleRule.ScheduleMode == ScheduleMode.LiveFeed)
                {
                    if (_finishManager == _totalManager && _finishHarvestWorker == _totalHarvestWorker && _finishPostWorker == _totalPostWorker)
                    {
                        _projectState = DataTypes.Enums.ProjectState.IsStopped;
                        updateStatus.Invoke(_currentWindow, _statusLabel, _projectState);
                        AutoPost.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] has been stopped!"));
                        break;
                    }
                }
                else
                {
                    if (_finishManager == 1 && _finishDatabasePostWorker == _totalDatabasePostWorker)
                    {
                        _projectState = DataTypes.Enums.ProjectState.IsStopped;
                        updateStatus.Invoke(_currentWindow, _statusLabel, _projectState);
                        AutoPost.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] has been stopped!"));
                        break;
                    }
                }

                Thread.Sleep(500);
            } while (true);
        }

        #endregion

        #region utility methods

        /// <summary>
        /// Clone List to Queue
        /// </summary>
        private void CloneSearchPages()
        {
            _queueSearchPages.Clear();

            foreach (var i in searchPages)
            {
                _queueSearchPages.Enqueue(i);
            }
        }

        /// <summary>
        /// Load All Post sites
        /// </summary>
        private void LoadPostSites()
        {
            _postSites.Clear();
            _postSites = _ruleCollection.GetAllPostSites();
            _totalPostWorker = _postSites.Count;
            _totalDatabasePostWorker = _postSites.Count;
        }

        /// <summary>
        /// Create WakeUp Schedule for comeback interval
        /// </summary>
        private void CreateWakeUpSchedule()
        {
            Random rnd = new Random();
            int randomInterval = 0;
            _wakeUpPoint.Clear();

            DateTime holdPoint = DateTime.Now;
            DateTime temp = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, holdPoint.Hour, holdPoint.Minute, holdPoint.Second);

            _wakeUpPoint.Enqueue(holdPoint);

            int minComeback = (int)(_scheduleRule.ComebackInterval - (_scheduleRule.ComebackInterval * 0.2));
            int maxComback = (int)(_scheduleRule.ComebackInterval + (_scheduleRule.ComebackInterval * 0.2));

            do
            {
                randomInterval = rnd.Next(minComeback, maxComback);
                temp = temp.AddMinutes(_scheduleRule.ComebackInterval);

                if (temp < holdPoint)
                    continue;
                else
                    _wakeUpPoint.Enqueue(temp);
            } while (temp < holdPoint.AddDays(30));
        }

        /// <summary>
        /// Create Post Schedule from TimeBetweenPost, TimeRange, TimeUnit
        /// </summary>
        private void CreatePostSchedule()
        {
            _postPoint.Clear();

            DateTime holdPoint = DateTime.Now;
            DateTime temp;
            DateTime tomorrow;
            int average_interval = 0;
            int min_interval = 0;
            int max_interval = 0;
            int randomInterval = 0;
            Random rnd = new Random();

            if (_scheduleRule.ScheduleMode == ScheduleMode.LiveFeed)
            {
                switch (_scheduleRule.TimeBetweenPost)
                {
                    case TimeBetweenPost.Automatically:

                        temp = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, 0, 0, 0);
                        tomorrow = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, 0, 0, 0).AddDays(1);

                        if (_scheduleRule.TimeRange == TimeRange.Daily)
                        {
                            average_interval = (int)Math.Ceiling((double)(_minsPerDay / _scheduleRule.NumberOfPosts));
                            min_interval = (int)(average_interval - (average_interval * 30 / 100));
                            max_interval = (int)(average_interval + (average_interval * 30 / 100));

                            do
                            {
                                randomInterval = StaticRandom.RandRange(min_interval, max_interval);
                                temp = temp.AddMinutes(randomInterval);

                                if (temp < holdPoint)
                                    continue;
                                else
                                {
                                    if (temp < tomorrow)
                                        _postPoint.Enqueue(temp);
                                    else
                                    {
                                        tomorrow = tomorrow.AddDays(1);
                                        _postPoint.Enqueue(temp);
                                    }
                                }
                            } while (temp < holdPoint.AddDays(30));
                        }
                        else
                        {
                            int average_postPerDay = (int)Math.Ceiling((double)(_scheduleRule.NumberOfPosts / _scheduleRule.NumberOfDays));
                            average_interval = (int)Math.Ceiling((double)(_minsPerDay / average_postPerDay));
                            min_interval = (int)(average_interval - (average_interval * 30 / 100));
                            max_interval = (int)(average_interval + (average_interval * 30 / 100));

                            do
                            {
                                randomInterval = StaticRandom.RandRange(min_interval, max_interval);
                                temp = temp.AddMinutes(randomInterval);

                                if (temp < holdPoint)
                                    continue;
                                else
                                {
                                    if (temp < tomorrow)
                                        _postPoint.Enqueue(temp);
                                    else
                                    {
                                        tomorrow = tomorrow.AddDays(1);
                                        _postPoint.Enqueue(temp);
                                    }
                                }
                            } while (temp < holdPoint.AddDays(_scheduleRule.NumberOfDays));
                        }
                        break;
                    case TimeBetweenPost.Manually:

                        temp = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, 0, 0, 0);
                        tomorrow = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, 0, 0, 0).AddDays(1);

                        if (_scheduleRule.TimeUnit == TimeUnit.Hour)
                        {
                            min_interval = _scheduleRule.MinInterval * 60;
                            max_interval = _scheduleRule.MaxInterval * 60;
                        }
                        else
                        {
                            min_interval = _scheduleRule.MinInterval;
                            max_interval = _scheduleRule.MaxInterval;
                        }

                        if (_scheduleRule.TimeRange == TimeRange.Daily)
                        {
                            int totalPost = 0;

                            do
                            {
                                randomInterval = StaticRandom.RandRange(min_interval, max_interval);
                                temp = temp.AddMinutes(randomInterval);

                                if (temp < holdPoint)
                                    continue;
                                else
                                {
                                    if (totalPost <= _scheduleRule.NumberOfPosts)
                                    {
                                        if (temp < tomorrow)
                                        {
                                            _postPoint.Enqueue(temp);
                                            totalPost++;
                                        }
                                        else
                                        {
                                            totalPost = 1;
                                            tomorrow = tomorrow.AddDays(1);
                                            _postPoint.Enqueue(temp);
                                        }
                                    }
                                    else
                                    {
                                        totalPost = 0;
                                        temp = tomorrow;
                                        tomorrow = tomorrow.AddDays(1);
                                    }
                                }
                            } while (temp < holdPoint.AddDays(30));
                        }
                        else
                        {
                            int postPerDay = (int)Math.Ceiling((double)(_scheduleRule.NumberOfDays / _scheduleRule.NumberOfPosts));
                            int totalPost = 0;

                            do
                            {
                                randomInterval = StaticRandom.RandRange(min_interval, max_interval);
                                temp = temp.AddMinutes(randomInterval);

                                if (temp < holdPoint)
                                    continue;
                                else
                                {
                                    if (totalPost <= postPerDay)
                                    {
                                        if (temp < tomorrow)
                                        {
                                            _postPoint.Enqueue(temp);
                                            totalPost++;
                                        }
                                        else
                                        {
                                            totalPost = 1;
                                            tomorrow = tomorrow.AddDays(1);
                                            _postPoint.Enqueue(temp);
                                        }
                                    }
                                    else
                                    {
                                        totalPost = 0;
                                        temp = tomorrow;
                                        tomorrow = tomorrow.AddDays(1);
                                    }
                                }
                            } while (temp < holdPoint.AddDays(_scheduleRule.NumberOfDays));
                        }
                        break;
                }
            }
            else
            {
                temp = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, 0, 0, 0);
                tomorrow = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, 0, 0, 0).AddDays(1);

                int average_postPerDay = (int)Math.Ceiling((double)(_scheduleRule.NumberOfPosts / _scheduleRule.NumberOfDays));
                average_interval = (int)Math.Ceiling((double)(_minsPerDay / average_postPerDay));
                min_interval = (int)(average_interval - (average_interval * 30 / 100));
                max_interval = (int)(average_interval + (average_interval * 30 / 100));

                do
                {
                    randomInterval = StaticRandom.RandRange(min_interval, max_interval);
                    temp = temp.AddMinutes(randomInterval);

                    if (temp < holdPoint)
                        continue;
                    else
                    {
                        if (temp < tomorrow)
                            _postPoint.Enqueue(temp);
                        else
                        {
                            tomorrow = tomorrow.AddDays(1);
                            _postPoint.Enqueue(temp);
                        }
                    }
                } while (temp < holdPoint.AddDays(_scheduleRule.NumberOfDays));
            }
        }

        /// <summary>
        /// Reload records by post rules
        /// </summary>
        private void ReloadArticleFromRecords()
        {
            do
            {
                bool result = false;
                RecordHandler.Instance.ConsumeHandler(this, ref result);

                if (!result)
                    Thread.Sleep(250);
                else
                {
                    List<Record> currentRecords = RecordHandler.Instance.GetRecordsByDate(this, _scheduleRule.StartDate, _scheduleRule.EndDate);

                    RecordHandler.Instance.ReleaseHandler(this, ref result);

                    for (int i = 0; i < _postSites.Count; i++)
                    {
                        database_chosenRecords[i] = new Queue<Record>();

                        List<PostRule> currentChosenRules = _ruleCollection.GetRulesBySiteHost(_postSites[i]);

                        foreach (var record in currentRecords)
                        {
                            if (CheckRecordWithPostRule(record, currentChosenRules))
                                database_chosenRecords[i].Enqueue(record);
                        }
                    }
                    break;
                }
            } while (true);
        }

        /// <summary>
        /// Setup postManager & harvestManager DoWork function
        /// </summary>
        private void SetupManager()
        {
            _harverstManagerWorker = new Thread(harvestManager_DoWork);
            _harverstManagerWorker.IsBackground = true;
            _postManagerWorker = new Thread(postManager_DoWork);
            _postManagerWorker.IsBackground = true;
            database_postManagerWorker = new Thread(databasePostManager_DoWork);
            database_postManagerWorker.IsBackground = true;
        }

        /// <summary>
        /// Get list of MatchLinks based on links & rules
        /// </summary>
        /// <param name="links"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        private List<HarvestLink> getMatchLinks(List<HarvestLink> links, List<PostRule> rules)
        {
            List<HarvestLink> temp = new List<HarvestLink>();

            foreach (var link in links)
            {
                foreach (var rule in rules)
                {
                    if (link.harvestUrl.Contains(HttpUtility.UrlDecode(rule.SourceCategory)) && link.harvestUrl.Contains(rule.SourceHost))
                    {
                        temp.Add(link);
                        break;
                    }
                }
            }

            //filter based on limitation
            switch (_scheduleRule.Limitation)
            {
                case Limitation.Lastest10:
                    temp = temp.OrderByDescending(a => a.posted).Take(10).ToList();
                    break;
                case Limitation.Lastest20:
                    temp = temp.OrderByDescending(a => a.posted).Take(20).ToList();
                    break;
                case Limitation.Lastest30:
                    temp = temp.OrderByDescending(a => a.posted).Take(30).ToList();
                    break;
            };

            return temp;
        }

        /// <summary>
        /// Get source of images inside article body
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private List<string> GetImagesInHTMLString(string htmlString)
        {
            List<string> images = new List<string>();
            string pattern = "<img.+?src=[\"'](.+?)[\"'].*?>";

            //Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = Regex.Matches(htmlString, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            for (int i = 0, l = matches.Count; i < l; i++)
            {
                images.Add(matches[i].Groups[1].Value);
            }

            return images;
        }

        /// <summary>
        /// Create drive service
        /// </summary>
        private void CreateDriveService()
        {
            try
            {
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = _googleClientId,
                    ClientSecret = _googleClientSecret,
                },
                new[] { DriveService.Scope.Drive },
                "user", CancellationToken.None).Result;

                // Create the service.
                service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _googleApplicationName,
                });
            }
            catch (Exception e)
            {
                //Console.WriteLine("An error occurred: " + e.Message);
            }
        }

        /// <summary>
        /// Check the shared folder in drive is already existed or not
        /// </summary>
        private void CheckFolderInDrive()
        {
            List<Google.Apis.Drive.v2.Data.File> result = new List<Google.Apis.Drive.v2.Data.File>();
            FilesResource.ListRequest request = service.Files.List();
            request.Q = "mimeType='application/vnd.google-apps.folder' and trashed=false and title='storage-temp'";
            request.MaxResults = 200;

            do
            {
                try
                {
                    FileList files = request.Execute();
                    result.AddRange(files.Items);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception ex)
                {
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            if (result.Count > 0)
            {
                _sharedFolder = result[0];
                _folderCreated = true;
            }

            if (!_folderCreated)
            {
                Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
                body.Title = "storage-temp";
                body.MimeType = "application/vnd.google-apps.folder";

                Google.Apis.Drive.v2.Data.File file = service.Files.Insert(body).Execute();

                Permission permission = new Permission();
                permission.Value = "";
                permission.Type = "anyone";
                permission.Role = "reader";

                service.Permissions.Insert(permission, file.Id).Execute();

                _sharedFolder = file;
            }
        }

        /// <summary>
        /// Download featured image of article
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string DownloadFeaturedImage(string url, string name)
        {
            string fullPath = "";
            Random rnd = new Random();
            int retry = 0;
            while (retry < 3)
            {
                try
                {
                    //System.Drawing.Bitmap temp = null;
                    HttpWebRequest requestFeatured = (HttpWebRequest)HttpWebRequest.Create(url);
                    Random proxyrand = new Random();
                    
                    //get proxy
                    if (ProxyHandler.Instance.PostingDownloadArticles != ProxyType.Disabled)
                    {
                        WebProxy p;
                        do
                        {
                            bool result = false;
                            ProxyHandler.Instance.ConsumeHandler(this, name, ref result);
                            if (!result)
                                Thread.Sleep(150);
                            else
                            {
                                p = ProxyHandler.Instance.GetRandomProxy(this, name, ProxyHandler.Instance.PostingDownloadArticles);
                                ProxyHandler.Instance.ReleaseHandler(this, name, ref result);
                                break;
                            }
                        } while (true);

                        requestFeatured.Proxy = p;
                    }
                    requestFeatured.Timeout = 30000;                    
                    HttpWebResponse responseFeatured = (HttpWebResponse)requestFeatured.GetResponse();
                    using (Stream streamFeatured = responseFeatured.GetResponseStream())
                    {
                        using (var temp = (Bitmap)System.Drawing.Bitmap.FromStream(streamFeatured))
                        {
                            EncoderParameter parameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)0.5);
                            ImageCodecInfo encoder = ImageHelper.getEncoder(ImageHelper.getFormat(url));
                            if (encoder != null)
                            {
                                EncoderParameters encoderParams = new EncoderParameters(1);
                                encoderParams.Param[0] = parameter;
                                string fileName = System.IO.Path.GetFileNameWithoutExtension(url) + string.Format("{0}-{1}-{2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                                //replace special character on file name
                                fileName = fileName.Replace("%", "");

                                fileName += System.IO.Path.GetExtension(url);

                                fullPath = SysSettings.Instance._tempPath + fileName;
                                temp.Save(fullPath, encoder, encoderParams);
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    retry++;
                    Thread.Sleep(rnd.Next(300, 500));
                }
            }
            return fullPath;
        }

        /// <summary>
        /// Get Article Type based on its own url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private ArticleType GetArticleType(string url)
        {
            foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
            {
                if (url.Contains(source.Url))
                {
                    foreach (var type in source.GalleryCategories)
                    {
                        if (url.Contains(type))
                            return ArticleType.GalleryArticle;
                    }
                }
            }
            return ArticleType.NewsArticle;
        }

        /// <summary>
        /// Get PostSite by its host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private PostSites GetPostSiteByHost(string host)
        {
            foreach (var site in SysSettings.Instance.Article_Posting_Sites)
            {
                if (site.Host.Contains(host))
                    return site;
            }
            return null;
        }

        /// <summary>
        /// Check record whether or not it's belonged to one PostRule
        /// </summary>
        /// <param name="item"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        private bool CheckRecordWithPostRule(Record item, List<PostRule> rules)
        {
            foreach (var rule in rules)
            {
                if (item.url.Contains(rule.SourceCategory))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Insert keywords and links into body
        /// </summary>
        /// <param name="body"></param>
        /// <param name="type"></param>
        /// <param name="slot"></param>
        /// <param name="chosenKeywords"></param>
        private void InsertKeywordsAndLinks(string body, List<string> sentences, ref List<SEOSentence> replacement, CategorySEORule rule, KeywordType type, int slot, ref List<string> chosenKeywords)
        {
            int inserted = 0;
            Random rnd = new Random();
            List<KeywordPackage> packages = new List<KeywordPackage>();

            List<KeywordList> kList = rule.GetKeywordListByType(type);
            foreach (var i in kList)
                packages.Add(new KeywordPackage(i, rule.GetLinkListByKeywordListName(i.Name), 0));

            while (inserted < slot)
            {
                //pick random a package
                KeywordPackage random = new KeywordPackage();
                if (packages.Count < 2)
                    random = packages[0];
                else
                    random = packages[rnd.Next(packages.Count)];

                IInsert inserter = InsertFactory.CreateInserter(random.KeywordList.InsertOpt);

                int linkAppeared = random.LinkAppeared;
                inserter.InsertToBody(body, sentences, random.KeywordList.Keywords, random.LinkList, ref replacement, ref linkAppeared, ref chosenKeywords);
                random.LinkAppeared = linkAppeared;

                inserted++;
            }
        }

        /// <summary>
        /// Check if current is existed whether or not 
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private bool IsExisted(string link)
        {
            if (_sourceArticles.Count > 0)
            {
                foreach (var recd in _sourceArticles)
                {
                    if (recd.articleUrl.Contains(link))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if current link is violated with rules whether or not
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool IsViolate(string url)
        {
            foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
            {
                if (url.Contains(source.Url))
                {
                    foreach (var forbidden in source.ForbiddenCategories)
                    {
                        if (url.Contains(forbidden))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Write log to txtfile
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mode"></param>
        private void WriteLog(string message, bool mode, string record)
        {
            lock (_writtingLocker)
            {
                if (mode)
                {
                    using (StreamWriter writer = new StreamWriter(_projectPath + "/logs.txt", true))
                    {
                        writer.WriteLine(message);
                    }

                    using (StreamWriter writer = new StreamWriter(_projectPath + "/posted.txt", true))
                    {
                        writer.WriteLine(record);
                    }
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(_projectPath + "/logs.txt", true))
                    {
                        writer.WriteLine(message);
                    }
                }
            }
        }

        /// <summary>
        /// Read all posted links
        /// </summary>
        /// <param name="path"></param>
        private void ReadPostedLog(int index, string host)
        {
            var lines = System.IO.File.ReadAllLines(_projectPath + "/posted.txt");

            foreach (var line in lines)
            {
                if (line.Contains(host))
                {
                    string[] content = line.Split('|');
                    _chosenArticles[index].Add(content[0]);
                }
            }
        }

        private void MergeAllKeyword(CategorySEORule rule, ref List<string> chosenKeywords)
        {
            List<KeywordList> kList = rule.GetKeywordListByType(KeywordType.Primary);

            foreach (var list in kList)
            {
                foreach (var key in list.Keywords)
                {
                    chosenKeywords.Add(key);
                }
            }

            List<KeywordList> sList = rule.GetKeywordListByType(KeywordType.Secondary);

            foreach (var list in sList)
            {
                foreach (var key in list.Keywords)
                {
                    chosenKeywords.Add(key);
                }
            }

            List<KeywordList> gList = rule.GetKeywordListByType(KeywordType.Generic);

            foreach (var list in gList)
            {
                foreach (var key in list.Keywords)
                {
                    chosenKeywords.Add(key);
                }
            }
        }

        #endregion
    }
}
