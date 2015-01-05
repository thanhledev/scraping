using System;
using System.Collections.Generic;
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
using System.Text.RegularExpressions;

namespace Scraping
{
    public sealed class VNSPostingProject
    {
        #region variables

        // For lockers
        private static readonly object _harvesterSyncLocker = new object(); //using for harvester worker
        private static readonly object _harvesterSyncLocker2 = new object(); //using for harvester worker
        private static readonly object _syncLocker3 = new object(); //using for harvester worker
        private static readonly object _postSyncLocker = new object(); //using for post worker
        private static readonly object _postSyncLocker2 = new object(); //using for post worker
        private static readonly object _postSyncLocker3 = new object(); //using for post worker
        private static readonly object _stopLocker = new object(); //using for worker 
        private static readonly object _randomlocker = new object(); //using for random        
        private static readonly object _finishHarvestLocker = new object(); //using for statistics
        private static readonly object _finishPostLocker = new object(); //using for statistics
        private static readonly object _finishManagerLocker = new object(); //using for statistics
        private static readonly object _writtingLocker = new object(); //using for write log file
        private static readonly object _postedLocker = new object(); //using for write posted file
        private static readonly object _fuckLocker = new object();
        // For threads
        private int _finishManager = 0;
        private int _finishHarvestWorker = 0; // number of harvest workers have finished its job.
        private int _finishPostWorker = 0; // number of post workers have finished its job.        
        private int _totalHarvestWorker = 0; //total number of harvest workers.
        private int _totalPostWorker = 0; // total number of post workers.        
        private int _totalManager = 2;
        private Thread[] _harversterWorker = new Thread[100];
        private Thread[] _posterWorker = new Thread[100];
        private Thread _postManagerWorker;
        private Thread _harverstManagerWorker;

        // For containing variables
        private Queue<DateTime> _wakeUpPoint = new Queue<DateTime>();
        private Queue<DateTime> _postPoint = new Queue<DateTime>();
        private List<string> _postSiteName = new List<string>();
        private Queue<VirtualNewspapersPostRule> _rules = new Queue<VirtualNewspapersPostRule>();
        private List<VNSPost> _vnsPosts = new List<VNSPost>();
        private List<string>[] _chosenArticles = new List<string>[100];
        private static int _minsPerDay = 1440;
        private static string DateTimeFormat = "dd-MM HH:mm:ss";
        
        private const int MINDELAYINTERVAL = 25000; //minimum delay interval (miliseconds)
        private const int MAXDELAYINTERVAL = 45000; //maximum delay interval (miliseconds)
        private const int MINDELAYINTERVAL1 = 35000; //minimum delay interval (miliseconds)
        private const int MAXDELAYINTERVAL1 = 70000; //maximum delay interval (miliseconds)
        private static DriveService service;
        private bool _folderCreated = false;
        private Google.Apis.Drive.v2.Data.File _sharedFolder;

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

        private List<PostSites> _postSites = new List<PostSites>();

        public List<PostSites> PostSites
        {
            get { return _postSites; }
        }        

        private VirtualNewspapersPostRulesCollection _ruleCollection = new VirtualNewspapersPostRulesCollection();

        public VirtualNewspapersPostRulesCollection RuleCollection
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

        private VirtualNewspapersScheduleRule _scheduleRule = new VirtualNewspapersScheduleRule();

        public VirtualNewspapersScheduleRule ScheduleRule
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

        public VNSPostingProject(string name)
            : this(name, "", "", "")
        {

        }

        public VNSPostingProject(string name, string clientId, string clientSecret, string appName)
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
        public void LoadPostSite(PostSites site)
        {
            if (!_postSites.Contains(site))
                _postSites.Add(site);
        }
      
        /// <summary>
        /// Get post site at position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public PostSites GetPostSiteAtPos(int pos)
        {
            return _postSites[pos];
        }

        /// <summary>
        /// Get post site by its host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public PostSites GetPostSiteByHost(string host)
        {
            if (_postSites.Count > 0)
                return _postSites.Where(a => a.Host == host).Single();
            return null;
        }

        /// <summary>
        /// Check site existed or not via its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckPostSiteExisted(string name)
        {
            foreach (var site in _postSites)
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
            PostSites removeSite = _postSites.Where(a => a.Host == host).Single();
            _postSites.Remove(removeSite);
        }

        #endregion

        #region vnsPosting

        public void Start()
        {
            _stopManager = false;
            _stopWorker = false;

            //preparation for posting                
            LoadPostSiteNames();

            //schedule
            CreateWakeUpSchedule();
            CreatePostSchedule();

            //google drive
            CreateDriveService();
            CheckFolderInDrive();

            //start manager
            _harverstManagerWorker.Start();
            _postManagerWorker.Start();

            VirtualNewspapers.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] has been started!"));
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
                        CloneVNSRules();
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
                        for (int i = 0; i < _postSiteName.Count; i++)
                        {
                            int input = i;

                            if (_chosenArticles[input] == null)
                            {
                                _chosenArticles[input] = new List<string>();
                                //read posted log here
                                ReadPostedLog(input, _postSites[input].Host);
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
        /// Stop function for manager thread
        /// </summary>
        private void StopManager()
        {
            lock (_finishManagerLocker)
            {
                _finishManager++;
            }            
        }

        /// <summary>
        /// Harvester worker DoWork function
        /// </summary>
        /// <param name="Index"></param>
        //private void harvestWorker_DoWork(object Index)
        //{
        //    int index = (int)Index;
        //    string message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Harvest | Thread[" + index + "]: Begin harvest VNS posts!");
        //    VirtualNewspapers.Instance.AddMessages(message);
        //    WriteLog(message, false, "");
        //    do
        //    {
        //        lock (_stopLocker)
        //        {
        //            if (_stopWorker)
        //                goto StopThread;
        //        }

        //        int retry = 0;
        //        VirtualNewspapersPostRule currentRule = new VirtualNewspapersPostRule(string.Empty, 0, string.Empty, 0);
        //        lock (_harvesterSyncLocker)
        //        {
        //            if (_rules.Count > 0)
        //                currentRule = _rules.Dequeue();
        //            else
        //                break;
        //        }

        //        if (currentRule.VNSHost != string.Empty)
        //        {
        //            while (retry < 3)
        //            {
        //                try
        //                {
        //                    string currentVnsHost = currentRule.VNSHost;
        //                    long currentVnsCateId = currentRule.VNSCategory;

        //                    foreach (var site in SysSettings.Instance.Article_VNS_Sites)
        //                    {
        //                        if (currentVnsHost == site.Host)
        //                        {
        //                            //List<Post> posts = new List<Post>();
        //                            //lock (_fuckLocker)
        //                            //{
        //                            //    site.InitializeWordpress();
        //                            //    posts = site.getPostsByCategory(currentVnsCateId);
        //                            //}

        //                            List<Post> posts = new List<Post>();
        //                            bool isSuccess = false;
        //                            do
        //                            {
        //                                bool result = false;
        //                                VNSControlHandler.Instance.ConsumeHandler(this, "Project[" + _projectName + "]|Harvest|Thread[" + index + "]", ref result);

        //                                if (!result)
        //                                    Thread.Sleep(500);
        //                                else
        //                                {
        //                                    try
        //                                    {
        //                                        posts = site.getPostsByCategory(currentVnsCateId);
        //                                        VNSControlHandler.Instance.ReleaseHandler(this, "Project[" + _projectName + "]|Harvest|Thread[" + index + "]", ref result);
        //                                        isSuccess = true;
        //                                        break;
        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        VNSControlHandler.Instance.ReleaseHandler(this, "Project[" + _projectName + "]|Harvest|Thread[" + index + "]", ref result);
        //                                        isSuccess = false;
        //                                        break;
        //                                    }                                            
        //                                }
        //                            } while (true);

        //                            if (isSuccess)
        //                            {
        //                                lock (_harvesterSyncLocker2)
        //                                {
        //                                    foreach (var post in posts)
        //                                    {
        //                                        VNSPost newItem = new VNSPost(currentVnsHost, currentVnsCateId, post);
        //                                        _vnsPosts.Add(newItem);
        //                                        message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Harvest | Thread[" + index + "]: Found " + post.Title + " and added!");
        //                                        VirtualNewspapers.Instance.AddMessages(message);
        //                                        WriteLog(message, false, "");
        //                                    }
        //                                }
        //                                break;
        //                            }
        //                            else
        //                                continue;
        //                        }
        //                    }
        //                    break;
        //                }
        //                catch (Exception ex)
        //                {
        //                    retry++;
        //                    message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Harvest | Thread[" + index + "]: Download failed.Retry " + retry + "/3 times. Reason : " + ex.Message);
        //                    VirtualNewspapers.Instance.AddMessages(message);
        //                    WriteLog(message, false, "");
        //                    Random retryRand = new Random();
        //                    Thread.Sleep(retryRand.Next(MINDELAYINTERVAL, MAXDELAYINTERVAL));            
        //                }
        //            }
        //        }
        //        Random nextTurn = new Random();
        //        Thread.Sleep(nextTurn.Next(MINDELAYINTERVAL, MAXDELAYINTERVAL));
        //    } while (true);
        //    StopThread: if (_stopWorker) StopWorker(true);
        //    if (!_stopWorker) harvestWorker_Completed();
        //}

        private void harvestWorker_DoWork(object Index)
        {
            int index = (int)Index;
            string message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Harvest | Thread[" + index + "]: Begin harvest VNS posts!");
            VirtualNewspapers.Instance.AddMessages(message);
            WriteLog(message, false, "");
            do
            {
                lock (_stopLocker)
                {
                    if (_stopWorker)
                        goto StopThread;
                }
                
                VirtualNewspapersPostRule currentRule = new VirtualNewspapersPostRule(string.Empty, 0, string.Empty, 0);
                lock (_harvesterSyncLocker)
                {
                    if (_rules.Count > 0)
                        currentRule = _rules.Dequeue();
                    else
                        break;
                }

                if (currentRule.VNSHost != string.Empty)
                {
                    string currentVnsHost = currentRule.VNSHost;
                    long currentVnsCateId = currentRule.VNSCategory;

                    foreach (var site in SysSettings.Instance.Article_VNS_Sites)
                    {
                        if (currentVnsHost == site.Host)
                        {
                            int retry = 0;

                            while (retry < 3)
                            {
                                try
                                {
                                    List<Post> posts = new List<Post>();
                                    do
                                    {
                                        bool result = false;
                                        VNSHarvestControlHandler.Instance.ConsumeHandler(this, "Project[" + _projectName + "]|Harvest|Thread[" + index + "]", ref result);

                                        if (!result)
                                            Thread.Sleep(500);
                                        else
                                        {
                                            posts = site.getPostsByCategory(currentVnsCateId);
                                            VNSHarvestControlHandler.Instance.ReleaseHandler(this, "Project[" + _projectName + "]|Harvest|Thread[" + index + "]", ref result);
                                            break;
                                        }
                                    } while (true);

                                    lock (_harvesterSyncLocker2)
                                    {
                                        foreach (var post in posts)
                                        {
                                            VNSPost newItem = new VNSPost(currentVnsHost, currentVnsCateId, post);
                                            _vnsPosts.Add(newItem);
                                            message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Harvest | Thread[" + index + "]: Found " + post.Title + " and added!");
                                            VirtualNewspapers.Instance.AddMessages(message);
                                            WriteLog(message, false, "");
                                        }
                                    }
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    bool temp = false;
                                    retry++;
                                    VNSHarvestControlHandler.Instance.ReleaseHandler(this, "Project[" + _projectName + "]|Harvest|Thread[" + index + "]", ref temp);
                                    VirtualNewspapers.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] | Harvest | Thread[" + index + "]: Error " + ex.Message + "!"));
                                }
                            }
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
                string message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Harvest VNS articles completely!");
                VirtualNewspapers.Instance.AddMessages(message);
                WriteLog(message, false, "");
                _finishHarvestWorker = 0;
                CloneVNSRules();
            }
        }

        /// <summary>
        /// Poster worker DoWork function
        /// </summary>
        /// <param name="Index"></param>
        private void postWorker_DoWork(object Index)
        {
            int index = (int)Index;
            List<VirtualNewspapersPostRule> _currentSiteVNSRules = _ruleCollection.GetRulesBySiteHost(_postSiteName[index]);
            List<PostSiteArticle> _matchPosts = new List<PostSiteArticle>();
            List<PostSiteArticle> _chosenPosts = new List<PostSiteArticle>();

            lock (_postSyncLocker)
            {
                _matchPosts = GetMatchPosts(_vnsPosts, _currentSiteVNSRules);
            }

            lock (_stopLocker)
            {
                if (_stopWorker)
                    goto StopThread;
            }
            
            string message = "";
            
            if (_matchPosts.Count > 0)
            {
                foreach (var post in _matchPosts)
                {
                    if (!_chosenArticles[index].Contains(post.Post.Permalink))
                    {
                        _chosenPosts.Add(post);
                    }
                }

                if (_chosenPosts.Count > 0)
                {
                    _chosenPosts = _chosenPosts.OrderBy(a => a.Post.DateCreated).ToList();
                    foreach (var vnsPost in _chosenPosts)
                    {
                        lock (_stopLocker)
                        {
                            if (_stopWorker)
                                goto StopThread;
                        }

                        lock (_syncLocker3)
                        {
                            _chosenArticles[index].Add(vnsPost.Post.Permalink);
                        }

                        //test code 

                        //VirtualNewspapers.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Begin posting " + vnsPost.Post.Title + " to website " + vnsPost.SiteHost + " !"));
                        //VirtualNewspapers.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Posting " + vnsPost.Post.Title + " to website " + vnsPost.SiteHost + " successfully!"));

                        //end test code

                        //origin code

                        string errorMessage = "";
                        HttpWebRequest request;
                        int pos = 0;
                        Random rnd = new Random();

                        VirtualNewspapers.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Begin posting " + vnsPost.Post.Title + " to website " + vnsPost.SiteHost + " !"));

                        //get site category that article will be published
                        string host = vnsPost.SiteHost;
                        long chosenPostSiteCategoryId = vnsPost.SiteCateId;

                        string articleBody = vnsPost.Post.Body;

                        //get wordpress wrapper of current site
                        PostSites chosenSite = GetPostSiteByHost(host);

                        //featured image
                        string featuredImageUrl = GetFeaturedImageUrl(SysSettings.openFeaturedPictureTag, SysSettings.closedFeaturedPictureTag, articleBody);
                        articleBody = articleBody.Replace(SysSettings.openFeaturedPictureTag + featuredImageUrl + SysSettings.closedFeaturedPictureTag, "");

                        bool isFeaturedImageUpload = false;
                        MediaObjectInfo featuredImage = new MediaObjectInfo();
                        if (featuredImageUrl != string.Empty)
                        {
                            string featuredImagePath = DownloadFeaturedImage(featuredImageUrl, _projectName + "|VNS|Thread[" + index + "]");
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
                                        Thread.Sleep(rnd.Next(1000, 5000));
                                    }
                                }
                            }
                        }
                        //Image processing
                        List<string> bodyImages = new List<string>();
                        bodyImages = GetImagesInHTMLString(articleBody);

                        //Phase 1, step 1: download & mixing image
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

                        //            if (ProxyHandler.Instance.VnsDownloadArticles != ProxyType.Disabled)
                        //            {
                        //                //get proxy
                        //                WebProxy p;
                        //                do
                        //                {
                        //                    bool result = false;
                        //                    ProxyHandler.Instance.ConsumeHandler(this, _projectName + "|VNS|Thread[" + index + "]", ref result);
                        //                    if (!result)
                        //                        Thread.Sleep(150);
                        //                    else
                        //                    {
                        //                        p = ProxyHandler.Instance.GetRandomProxy(this, _projectName + "|VNS|Thread[" + index + "]", ProxyHandler.Instance.VnsDownloadArticles);
                        //                        ProxyHandler.Instance.ReleaseHandler(this, _projectName + "|VNS|Thread[" + index + "]", ref result);
                        //                        break;
                        //                    }
                        //                } while (true);

                        //                //implement proxy
                        //                request.Proxy = p;
                        //            }

                        //            request.Timeout = 30000;
                        //            request.ReadWriteTimeout = 60000;
                        //            request.KeepAlive = false;
                        //            request.ProtocolVersion = HttpVersion.Version10;
                        //            request.Method = "GET";
                        //            request.Accept = "text/html,application/xhtml+xml,application/xml";
                        //            pos = rnd.Next(SysSettings.Instance._agents.Count);
                        //            request.UserAgent = SysSettings.Instance._agents[pos];

                        //            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        //            {
                        //                using (Stream imgStream = response.GetResponseStream())
                        //                {
                        //                    tmp = (Bitmap)System.Drawing.Bitmap.FromStream(imgStream);
                        //                    Bitmap remake = ImageHelper.RemakeImage(tmp, _logoPath);
                        //                    EncoderParameter parameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)0.5);
                        //                    ImageCodecInfo encoder = ImageHelper.getEncoder(ImageHelper.getFormat(i));
                        //                    if (encoder != null)
                        //                    {
                        //                        EncoderParameters encoderParams = new EncoderParameters(1);
                        //                        encoderParams.Param[0] = parameter;
                        //                        string fileName = System.IO.Path.GetFileNameWithoutExtension(i) + string.Format("{0}-{1}-{2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        //                        fileName += System.IO.Path.GetExtension(i);

                        //                        string fullPath = SysSettings.Instance._tempPath + fileName;
                        //                        remake.Save(fullPath, encoder, encoderParams);
                        //                        uploadImages.Add(new DriveImages(i, fullPath, ""));
                        //                    }
                        //                    Thread.Sleep(rnd.Next(3000, 5000));
                        //                }
                        //            }
                        //            break;
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            imgRetry++;
                        //            Thread.Sleep(rnd.Next(3000, 5000));
                        //        }
                        //    }
                        //}

                        //Phase 1,step 2: upload image to drive
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

                        //Phase1, step 3: replace old image with new uploaded images
                        foreach (var i in uploadImages)
                        {
                            System.IO.File.Delete(i.SourcePath);
                            articleBody = articleBody.Replace(i.SourceUrl, i.DrivePath);
                        }

                        //Phase 2: Apply SEO 
                        List<string> chosenKeywords = new List<string>();

                        //get category SEO rule for this particular article
                        SEORule chosenSeoRule = _seoRuleCollection.GetRule(host);
                        CategorySEORule chosenRule = _seoRuleCollection.GetCategorySEORule(host, chosenPostSiteCategoryId);

                        if (chosenRule != null)
                        {
                            //Apply SEO to post
                            ApplySEORule(ref articleBody, chosenRule, ref chosenKeywords);
                        }

                        // temporary patch
                        if (chosenKeywords.Count == 0)
                        {
                            chosenKeywords = chosenRule.KeywordList[0].Keywords;
                        }

                        //update the vnsPost
                        vnsPost.Post.Body = articleBody;

                        //Phase 3: upload article to Wordpress                                                
                        var newSitePost = new Post();
                        newSitePost.DateCreated = TimeZoneInfo.ConvertTime(DateTime.Now.AddMinutes(-1), chosenSite.TimeZone);
                        newSitePost.Title = vnsPost.Post.Title;
                        newSitePost.Body = articleBody;
                        newSitePost.Categories = new string[] { chosenSite.GetCategoryById(chosenRule.CategoryID) };
                        if (isFeaturedImageUpload)
                            newSitePost.PostThumbnail = featuredImage.ID;

                        //SEO factors
                        newSitePost.CustomFields = chosenSeoRule.SeoPlugin.remakeSEOFactors(vnsPost.Post.CustomFields, SysSettings._KeywordTag, chosenKeywords, _projectName);

                        int postRetry = 0;
                        bool isPosted = false;
                        int postId = -1;
                        Post posted = new Post();

                        //while (postRetry < 3)
                        //{
                        //    try
                        //    {
                        //        do
                        //        {
                        //            bool result = false;
                        //            VNSPostControlHandler.Instance.ConsumeHandler(this, "Project[" + _projectName + "]|VNS|Thread[" + index + "]", ref result);

                        //            if (!result)
                        //                Thread.Sleep(500);
                        //            else
                        //            {
                        //                postId = chosenSite.Wordpress.NewPost(newSitePost, true);
                        //                posted = chosenSite.Wordpress.GetPost(postId);
                        //                isPosted = true;
                        //                VNSPostControlHandler.Instance.ReleaseHandler(this, "Project[" + _projectName + "]|VNS|Thread[" + index + "]", ref result);
                        //                break;
                        //            }
                        //        } while (true);
                        //        break;
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        bool temp = false;
                        //        errorMessage += ex.Message + " | ";
                        //        postRetry++;
                        //        VNSPostControlHandler.Instance.ReleaseHandler(this, "Project[" + _projectName + "]|VNS|Thread[" + index + "]", ref temp);
                        //    }
                        //}

                        try
                        {
                            do
                            {
                                bool result = false;
                                VNSPostControlHandler.Instance.ConsumeHandler(this, "Project[" + _projectName + "]|VNS|Thread[" + index + "]", ref result);

                                if (!result)
                                    Thread.Sleep(500);
                                else
                                {
                                    postId = chosenSite.Wordpress.NewPost(newSitePost, true);
                                    posted = chosenSite.Wordpress.GetPost(postId);
                                    isPosted = true;
                                    VNSPostControlHandler.Instance.ReleaseHandler(this, "Project[" + _projectName + "]|VNS|Thread[" + index + "]", ref result);
                                    break;
                                }
                            } while (true);
                        }
                        catch (Exception ex)
                        {
                            bool temp = false;
                            errorMessage += ex.Message + " , StackTrace: " + ex.StackTrace;
                            VNSPostControlHandler.Instance.ReleaseHandler(this, "Project[" + _projectName + "]|VNS|Thread[" + index + "]", ref temp);
                        }

                        if (isPosted)
                        {
                            message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "]: Posted " + vnsPost.Post.Title + " to website " + chosenSite.Host + " successfully!");
                            VirtualNewspapers.Instance.AddMessages(message);
                            WriteLog(message, true, vnsPost.Post.Permalink + "|" + chosenSite.Host);

                            //try to indexed the links
                            IndexerHandler.Instance.InsertIndexUrls(posted.Permalink);
                        }
                        else
                        {
                            message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "]: Posted " + vnsPost.Post.Title + " to website " + chosenSite.Host + " failed! Reason : " + errorMessage);
                            VirtualNewspapers.Instance.AddMessages(message);
                            WriteLog(message, false, "");
                            WriteLog(message, true, vnsPost.Post.Permalink + "|" + chosenSite.Host);
                        }
                        Thread.Sleep(5000);

                        //end origin code
                    }
                }
                else
                {
                    message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "]: Cannot find any avaiable article to post! Ignore posting this time.");
                    VirtualNewspapers.Instance.AddMessages(message);
                    WriteLog(message, false, "");
                }
            }
            else
            {
                message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "]: Cannot find any matching article to post! Ignore posting this time.");
                VirtualNewspapers.Instance.AddMessages(message);
                WriteLog(message, false, "");
            }
            StopThread: if (_stopWorker) StopWorker(false);
            if (!_stopWorker) postWorker_Completed();
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
                string message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] : Post articles completely!");
                VirtualNewspapers.Instance.AddMessages(message);
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
        /// StopWatcher DoWork function
        /// </summary>
        private void StopWatcher_DoWork()
        {
            do
            {
                if (_finishManager == _totalManager && _finishHarvestWorker == _totalHarvestWorker && _finishPostWorker == _totalPostWorker)
                {
                    _projectState = DataTypes.Enums.ProjectState.IsStopped;
                    updateStatus.Invoke(_currentWindow, _statusLabel, _projectState);
                    VirtualNewspapers.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "] has been stopped!"));
                    break;
                }

                Thread.Sleep(500);

            } while (true);            
        }

        #endregion

        #region utility methods

        /// <summary>
        /// Clone list of rule to queue
        /// </summary>
        private void CloneVNSRules()
        {
            _rules.Clear();

            foreach (var i in _ruleCollection.GetAllDistinctRulesByVnsHostAndVnsCategories())
            {
                _rules.Enqueue(i);
            }

            _totalHarvestWorker = _rules.Count;
        }

        /// <summary>
        /// Load All Post sites
        /// </summary>
        private void LoadPostSiteNames()
        {
            _postSiteName.Clear();
            _postSiteName = _ruleCollection.GetAllPostSites();
            _totalPostWorker = _postSiteName.Count;
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
                //randomInterval = StaticRandom.RandRange(minComeback, maxComback);
                temp = temp.AddMinutes(_scheduleRule.ComebackInterval);

                if (temp < holdPoint)
                    continue;
                else
                    _wakeUpPoint.Enqueue(temp);
            } while (temp < holdPoint.AddDays(30));            
        }

        /// <summary>
        /// Create Post Schedule from TimeBetweenPost, TimeUnit
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
            //Random rnd = new Random();            

            switch (_scheduleRule.TimeBetweenPost)
            {
                case TimeBetweenPost.Automatically:

                    temp = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, 0, 0, 0);
                    tomorrow = new DateTime(holdPoint.Year, holdPoint.Month, holdPoint.Day, 0, 0, 0).AddDays(1);

                    average_interval = (int)Math.Ceiling((double)(_minsPerDay / _scheduleRule.NumberOfPostsPerDay));
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
                    } while (temp < holdPoint.AddDays(15));
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

                    int totalPost = 0;

                    //for testing only
                    //bool first = false;
                    //if (!first)
                    //{                        
                    //    _postPoint.Enqueue(DateTime.Now.AddMinutes(StaticRandom.RandRange(3, 5)));
                    //    _postPoint.Enqueue(DateTime.Now.AddMinutes(10));
                    //    _postPoint.Enqueue(DateTime.Now.AddMinutes(25));
                    //    //_postPoint.Enqueue(DateTime.Now.AddMinutes(15));                        
                    //    totalPost += 2;
                    //    first = true;
                    //}

                    do
                    {
                        randomInterval = StaticRandom.RandRange(min_interval, max_interval);
                        temp = temp.AddMinutes(randomInterval);

                        if (temp < holdPoint)
                            continue;
                        else
                        {                            
                            if (totalPost <= _scheduleRule.NumberOfPostsPerDay)
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
                    } while (temp < holdPoint.AddDays(15));

                    break;
            };

            //for testing only
            //foreach (var point in _postPoint.Take(10).ToList())
            //{
            //    string message = Utilities.Helper.CreateMessage(DateTimeFormat, "Project [" + _projectName + "]: " + point.ToString());
            //    VirtualNewspapers.Instance.AddMessages(message);
            //}
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
                    HttpWebRequest requestFeatured = (HttpWebRequest)HttpWebRequest.Create(url);                    
                    
                    //get proxy
                    if (ProxyHandler.Instance.VnsDownloadArticles != ProxyType.Disabled)
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
                                p = ProxyHandler.Instance.GetRandomProxy(this, name, ProxyHandler.Instance.VnsDownloadArticles);
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
        /// Get list of PostSiteArticle based on VNSPost & VirtualNewspapersPostRule
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        private List<PostSiteArticle> GetMatchPosts(List<VNSPost> posts, List<VirtualNewspapersPostRule> rules)
        {
            List<PostSiteArticle> temp = new List<PostSiteArticle>();

            foreach (var rule in rules)
            {                
                foreach (var post in posts)
                {
                    if (post.VnsHost == rule.VNSHost && post.VnsCateId == rule.VNSCategory)
                    {
                        PostSiteArticle item = new PostSiteArticle(rule.SiteHost, rule.SiteCategoryID, post.Post);
                        temp.Add(item);
                    }
                }
            }

            return temp;
        }

        /// <summary>
        /// Replace a exact keyword slot by keyword & link
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="link"></param>
        /// <param name="document"></param>
        /// <param name="pos"></param>
        /// <param name="keywordTag"></param>
        private void ReplaceByPosition(List<string> keywords, List<string> links, ref string document, int pos, string keywordTag, bool isInsertLink, ref List<string> chosenKeywords)
        {
            Random rnd = new Random();
            string chosenKeyword = keywords[StaticRandom.RandMax(keywords.Count)];

            if (!chosenKeywords.Contains(chosenKeyword))
                chosenKeywords.Add(chosenKeyword);

            string chosenLink = "";

            if (links.Count > 0)
            {
                if (links.Count >= 2)
                    chosenLink = links[StaticRandom.RandMax(links.Count)];
                else
                    chosenLink = links[0];
            }            
            
            string htmlAnchor = "";

            if (isInsertLink)
            {
                if (chosenLink != string.Empty)
                    htmlAnchor = "<a href=\"" + chosenLink + "\" target=\"_blank\">" + chosenKeyword + "</a>";
                else
                    htmlAnchor = " " + chosenKeyword + " ";
            }            
            else
                htmlAnchor = " " + chosenKeyword + " ";

            document = document.Substring(0, pos) + htmlAnchor + document.Substring(pos + keywordTag.Length);
        }

        /// <summary>
        /// Replace all available keyword slot with limit appearance
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="document"></param>
        /// <param name="appearance"></param>
        /// <param name="keywordTag"></param>
        private void ReplaceAllAvailablePosition(List<string> keywords, ref string document, int appearance, string keywordTag, ref List<string> chosenKeywords)
        {
            Random rnd = new Random();

            while (appearance > 0)
            {
                List<int> appearance_post = new List<int>();

                int pos = document.IndexOf(keywordTag);
                if (pos < 0)
                    break;
                while (pos > 0)
                {
                    appearance_post.Add(pos);
                    pos = document.IndexOf(keywordTag, pos + 1);
                }

                if (appearance_post.Count > 1)
                    pos = appearance_post[StaticRandom.RandMax(appearance_post.Count)];
                else
                    pos = appearance_post[0];

                string chosenKeyword = keywords[StaticRandom.RandMax(keywords.Count)];

                if (!chosenKeywords.Contains(chosenKeyword))
                    chosenKeywords.Add(chosenKeyword);

                document = document.Substring(0, pos) + chosenKeyword + document.Substring(pos + keywordTag.Length);
                appearance--;
            }
        }

        /// <summary>
        /// Replace all available keyword slot
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="document"></param>
        /// <param name="keywordTag"></param>
        private void ReplaceAllAvailablePosition(List<string> keywords, ref string document, string keywordTag)
        {
            Random rnd = new Random();

            while (true)
            {
                int pos = document.IndexOf(keywordTag);
                if (pos < 0)
                    break;

                string chosenKeyword = keywords[StaticRandom.RandMax(keywords.Count)];
                document = document.Substring(0, pos) + chosenKeyword + document.Substring(pos + keywordTag.Length);
            }
        }        

        /// <summary>
        /// Insert keyword & links
        /// </summary>
        /// <param name="document"></param>
        /// <param name="keywords"></param>
        /// <param name="link"></param>
        /// <param name="linkAppearance"></param>
        /// <param name="slots"></param>
        private void InsertKeywordsAndLinks(ref string document, CategorySEORule rule, KeywordType type, int slots, ref List<string> chosenKeywords)
        {
            int inserted = 0;
            Random rnd = new Random();
            List<KeywordPackage> packages = new List<KeywordPackage>();

            List<KeywordList> kList = rule.GetKeywordListByType(type);
            foreach (var i in kList)
                packages.Add(new KeywordPackage(i, rule.GetLinkListByKeywordListName(i.Name), 0));

            while (inserted < slots)
            {
                //pick random a package
                KeywordPackage random = new KeywordPackage();
                if (packages.Count < 2)
                    random = packages[0];
                else
                    random = packages[StaticRandom.RandMax(packages.Count)];

                List<int> appearance_post = new List<int>();

                int pos = document.IndexOf(SysSettings._KeywordTag);

                while (pos > 0)
                {
                    appearance_post.Add(pos);
                    pos = document.IndexOf(SysSettings._KeywordTag, pos + 1);
                }

                if (appearance_post.Count > 1)
                    pos = appearance_post[StaticRandom.RandMax(appearance_post.Count)];
                else
                    pos = appearance_post[0];

                int fate = StaticRandom.RandRange(0, 100);

                if (fate > 50)
                {
                    if (random.LinkAppeared < random.LinkList.ApperanceNumber)
                    {
                        ReplaceByPosition(random.KeywordList.Keywords, random.LinkList.Links, ref document, pos, SysSettings._KeywordTag, true, ref chosenKeywords);
                        random.LinkAppeared++;
                    }
                    else
                    {
                        ReplaceByPosition(random.KeywordList.Keywords, random.LinkList.Links, ref document, pos, SysSettings._KeywordTag, false, ref chosenKeywords);
                    }
                }
                else
                {
                    ReplaceByPosition(random.KeywordList.Keywords, random.LinkList.Links, ref document, pos, SysSettings._KeywordTag, false, ref chosenKeywords);
                }
                inserted++;
            }
        }

        /// <summary>
        /// Apply CategorySEORule to VNS Article
        /// </summary>
        /// <param name="document"></param>
        /// <param name="rule"></param>
        private void ApplySEORule(ref string document, CategorySEORule rule, ref List<string> chosenKeywords)
        {
            //step 1: calculate each type of keyword slot
            int totalKeywordSlots = 0;
            int primaryKeywordSlots = 0;
            int secondaryKeywordSlots = 0;
            int genericKeywordSlots = 0;

            int pos = document.IndexOf(SysSettings._KeywordTag);
           
            while (pos > 0)
            {
                totalKeywordSlots++;
                pos = document.IndexOf(SysSettings._KeywordTag, pos + 1);
            }

            primaryKeywordSlots = (int)(totalKeywordSlots * rule.PrimaryKeywordPercentage) / 100;
            secondaryKeywordSlots = (int)(totalKeywordSlots * rule.SecondaryKeywordPercentage) / 100;
            genericKeywordSlots = (int)(totalKeywordSlots * rule.GenericKeywordPercentage) / 100;

            int notUsedSlots = totalKeywordSlots - primaryKeywordSlots - secondaryKeywordSlots - genericKeywordSlots;

            if (notUsedSlots > 0)
            {
                while (notUsedSlots > 0)
                {
                    genericKeywordSlots++;
                    notUsedSlots--;
                }
            }

            if (primaryKeywordSlots > 0)
                InsertKeywordsAndLinks(ref document, rule, KeywordType.Primary, primaryKeywordSlots, ref chosenKeywords);
            if (secondaryKeywordSlots > 0)
                InsertKeywordsAndLinks(ref document, rule, KeywordType.Secondary, secondaryKeywordSlots, ref chosenKeywords);
            if (genericKeywordSlots > 0)
                InsertKeywordsAndLinks(ref document, rule, KeywordType.Generic, genericKeywordSlots, ref chosenKeywords);            

            List<string> allKeywords = ReunionAllKeywordList(rule);

            ReplaceAllAvailablePosition(allKeywords, ref document, SysSettings._imageAltTag);
        }        

        /// <summary>
        /// Get VNS article featured image
        /// </summary>
        /// <param name="openTag"></param>
        /// <param name="closedTag"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        private string GetFeaturedImageUrl(string openTag, string closedTag, string document)
        {
            string url = string.Empty;
            try
            {
                int startIndex = document.IndexOf(openTag) + openTag.Length;
                int length = document.IndexOf(closedTag) - startIndex;

                url = document.Substring(startIndex, length);
            }
            catch (Exception ex)
            {

            }

            return url;
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

            int a = 0;
            int b = 1;
        }

        /// <summary>
        /// Join all category keywords to one list
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        private List<string> ReunionAllKeywordList(CategorySEORule rule)
        {
            List<string> temp = new List<string>();

            foreach (var i in rule.KeywordList)
            {
                foreach (var word in i.Keywords)
                {
                    if (!temp.Contains(word))
                        temp.Add(word);
                }
            }            

            return temp;
        }

        #endregion
    }
}
