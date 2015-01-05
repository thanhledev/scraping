using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Enums;
using Utilities;
using System.IO;
using DataTypes.Collections;
using System.Text.RegularExpressions;
using JoeBlogs;
using System.Xml;
using System.Xml.Linq;

namespace Scraping
{
    public sealed class SysSettings
    {
        #region variables

        private static SysSettings _instance = null;
        private static readonly object _locker = new object();
        private static readonly object _locker2 = new object();
        private static readonly object _loadingLocker = new object();
        public List<string> _messages = new List<string>();
        public static string _applicationPath = Environment.CurrentDirectory;
        public static string _systemIniFile = Environment.CurrentDirectory + "\\system.ini";
        private static string _sourceIniFile = Environment.CurrentDirectory + "\\sources.ini";
        private static string _siteIniFile = Environment.CurrentDirectory + "\\sites.ini";        
        private static string _languageIniFile = Environment.CurrentDirectory + "\\languages.ini";
        public static string _proxycFile = Environment.CurrentDirectory + "\\proxyc.txt";
        public static string _proxypFile = Environment.CurrentDirectory + "\\proxyp.txt";
        private static string _gDriveIniFile = Environment.CurrentDirectory + "\\gdrive.ini";
        private static string _projectsPath = Environment.CurrentDirectory + "\\Cache\\Projects\\";
        private static string _vnsPath = Environment.CurrentDirectory + "\\Cache\\VNS\\";
        public string _tempPath = Environment.CurrentDirectory + "\\temp\\";
        private static string _defaultValue = "???";
        public static string _KeywordTag = "{k}";
        public static string _imageAltTag = "[alt]";
        public static string openFeaturedPictureTag = "[(";
        public static string closedFeaturedPictureTag = ")]";
        /// <summary>
        /// Software information
        /// </summary>
        private string _applicationName;

        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        private string _applicationVersion;

        public string ApplicationVersion
        {
            get { return _applicationVersion; }
            set { _applicationVersion = value; }
        }

        //private SystemState _systemState;     

        private ScrapeState _scrapeState;
        private PostingState _postState;

        private static string _encryptKey;

        public string EncryptKey
        {
            get { return _encryptKey; }
        }

        /// <summary>
        /// Article scraping settings
        /// </summary>

        private ArticleSearchOptions? _article_scraping_searchOps;

        public ArticleSearchOptions? Article_Scraping_SearchOps
        {
            get { return _article_scraping_searchOps; }
            set { _article_scraping_searchOps = value; }
        }

        private bool _article_scraping_MinWordOps;

        public bool Article_Scraping_MinWordOps
        {
            get { return _article_scraping_MinWordOps; }
            set { _article_scraping_MinWordOps = value; }
        }

        private int _article_scraping_MinWordValue;

        public int Article_Scraping_MinWordValue
        {
            get { return _article_scraping_MinWordValue; }
            set { _article_scraping_MinWordValue = value; }
        }

        private bool _article_scraping_MaxWordOps;

        public bool Article_Scraping_MaxWordOps
        {
            get { return _article_scraping_MaxWordOps; }
            set { _article_scraping_MaxWordOps = value; }
        }

        private int _article_scraping_MaxWordValue;

        public int Article_Scraping_MaxWordValue
        {
            get { return _article_scraping_MaxWordValue; }
            set { _article_scraping_MaxWordValue = value; }
        }

        private bool _article_scraping_ThreadsOpts;

        public bool Article_scraping_ThreadsOpts
        {
            get { return _article_scraping_ThreadsOpts; }
            set { _article_scraping_ThreadsOpts = value; }
        }

        private int _article_scraping_Threads;

        public int Article_Scraping_Threads
        {
            get { return _article_scraping_Threads; }
            set { _article_scraping_Threads = value; }
        }

        private bool _article_scraping_MaxDepthOpts;

        public bool Article_scraping_MaxDepthOpts
        {
            get { return _article_scraping_MaxDepthOpts; }
            set { _article_scraping_MaxDepthOpts = value; }
        }

        private int _article_scraping_MaxDepth;

        public int Article_Scraping_MaxDepth
        {
            get { return _article_scraping_MaxDepth; }
            internal set { _article_scraping_MaxDepth = value; }
        }

        private bool _article_scraping_UseProxies;

        public bool Article_scraping_UseProxies
        {
            get { return _article_scraping_UseProxies; }
            set { _article_scraping_UseProxies = value; }
        }

        private Queue<SystemProxy> _proxies = new Queue<SystemProxy>();

        public Queue<SystemProxy> Proxies
        {
            get { return _proxies; }            
        }

        private Queue<SystemProxy> _privateProxies = new Queue<SystemProxy>();

        public Queue<SystemProxy> PrivateProxies
        {
            get { return _privateProxies; }
        }

        /// <summary>
        /// Article storing settings
        /// </summary>

        private ArticleCopyOptions? _article_scraping_copyOps;

        public ArticleCopyOptions? Article_Scraping_CopyOps
        {
            get { return _article_scraping_copyOps; }
            set { _article_scraping_copyOps = value; }
        }

        private ArticleCopyContentSavingOptions? _article_scraping_savingOps;

        public ArticleCopyContentSavingOptions? Article_Scraping_SavingOps
        {
            get { return _article_scraping_savingOps; }
            set { _article_scraping_savingOps = value; }
        }

        private string _article_scraping_SavingFolder;

        public string Article_Scraping_SavingFolder
        {
            get { return _article_scraping_SavingFolder; }
            set { _article_scraping_SavingFolder = value; }
        }

        private bool _article_scraping_RemoveBlankLines;

        public bool Article_Scraping_RemoveBlankLines
        {
            get { return _article_scraping_RemoveBlankLines; }
            set { _article_scraping_RemoveBlankLines = value; }
        }

        private bool _article_scraping_UseTitleAsFileName;

        public bool Article_Scraping_UseTitleAsFileName
        {
            get { return _article_scraping_UseTitleAsFileName; }
            set { _article_scraping_UseTitleAsFileName = value; }
        }
                
        /// <summary>
        /// Article image scraping settings
        /// </summary>

        /// <summary>
        /// Article video scraping settings
        /// </summary>

        /// <summary>
        /// Article advanced scraping settings
        /// </summary>        
        private List<ArticleSource> _article_scraping_ScrapingSources = new List<ArticleSource>();

        public List<ArticleSource> Article_Scraping_ScrapingSources
        {
            get { return _article_scraping_ScrapingSources; }            
        }

        private List<Language> _article_scraping_Languages = new List<Language>();

        public List<Language> Article_Scraping_Languages
        {
            get { return _article_scraping_Languages; }
        }

        private TokenList _article_scraping_FilterWords = new TokenList();

        public TokenList Article_Scraping_FilterWords
        {
            get { return _article_scraping_FilterWords; }
        }

        /// <summary>
        /// Wordpress posting sites settings
        /// </summary>
        private List<PostSites> _article_posting_Sites = new List<PostSites>();

        public List<PostSites> Article_Posting_Sites
        {
            get { return _article_posting_Sites; }            
        }

        /// <summary>
        /// VNS sites settings
        /// </summary>
        private List<VirtualNewspapersSite> _article_VNS_Sites = new List<VirtualNewspapersSite>();

        public List<VirtualNewspapersSite> Article_VNS_Sites
        {
            get { return _article_VNS_Sites; }
        }

        /// <summary>
        /// Google drive settings
        /// </summary>

        private string _google_apis_clientID;

        public string Google_apis_clientID
        {
            get { return _google_apis_clientID; }            
        }

        private string _google_apis_clientSecret;

        public string Google_apis_clientSecret
        {
            get { return _google_apis_clientSecret; }            
        }

        private string _google_apis_applicationName;

        public string Google_apis_applicationName
        {
            get { return _google_apis_applicationName; }           
        }

        private List<PostingProject> _projects = new List<PostingProject>();

        public List<PostingProject> Projects
        {
            get { return _projects; }            
        }

        private List<VNSPostingProject> _vnsProjects = new List<VNSPostingProject>();

        public List<VNSPostingProject> VnsProjects
        {
            get { return _vnsProjects; }
        }

        private List<SEOPlugin> _plugins = new List<SEOPlugin>();

        public List<SEOPlugin> Plugins
        {
            get { return _plugins; }            
        }

        private bool _isStopNotify = false;

        public bool IsStopNotify
        {
            get { return _isStopNotify; }
            set { _isStopNotify = value; }
        }

        private ScrapeMode _article_scrapingMode;

        public ScrapeMode Article_ScrapingMode
        {
            get { return _article_scrapingMode; }
            set { _article_scrapingMode = value; }
        }

        public List<string> _agents =
            new List<string>
            {
                "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.67 Safari/537.36",
                "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.67 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1944.0 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.47 Safari/537.36",
                "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.116 Safari/537.36 Mozilla/5.0 (iPad; U; CPU OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B334b Safari/531.21.10",
                "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1664.3 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1664.3 Safari/537.36",
                "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.16 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1623.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.17 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36",
                "Mozilla/5.0 (X11; CrOS i686 4319.74.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.57 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.2 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1468.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1467.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1464.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 5.1; rv:31.0) Gecko/20100101 Firefox/31.0",
                "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:29.0) Gecko/20120101 Firefox/29.0",
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:25.0) Gecko/20100101 Firefox/29.0",
                "Mozilla/5.0 (X11; Linux x86_64; rv:28.0) Gecko/20100101  Firefox/28.0",
                "Mozilla/5.0 (Windows NT 6.1; rv:27.3) Gecko/20130101 Firefox/27.3",
                "Mozilla/5.0 (Windows NT 6.2; Win64; x64; rv:27.0) Gecko/20121011 Firefox/27.0",
                "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:24.0) Gecko/20100101 Firefox/24.0",
                "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.8; rv:24.0) Gecko/20100101 Firefox/24.0",
                "Mozilla/5.0 (Windows NT 6.2; rv:22.0) Gecko/20130405 Firefox/23.0",
                "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:23.0) Gecko/20130406 Firefox/23.0",
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:23.0) Gecko/20131011 Firefox/23.0",
                "Mozilla/5.0 (Windows NT 6.2; rv:22.0) Gecko/20130405 Firefox/22.0",
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:22.0) Gecko/20130328 Firefox/22.0",
                "Mozilla/5.0 (Windows NT 6.1; rv:22.0) Gecko/20130405 Firefox/22.0",
                "Opera/9.80 (Windows NT 6.0) Presto/2.12.388 Version/12.14",
                "Mozilla/5.0 (Windows NT 6.0; rv:2.0) Gecko/20100101 Firefox/4.0 Opera 12.14",
                "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0) Opera 12.14",
                "Opera/12.80 (Windows NT 5.1; U; en) Presto/2.10.289 Version/12.02",
                "Opera/9.80 (Windows NT 6.1; U; es-ES) Presto/2.9.181 Version/12.00",
                "Opera/9.80 (Windows NT 5.1; U; zh-sg) Presto/2.9.181 Version/12.00",
                "Opera/12.0(Windows NT 5.2;U;en)Presto/22.9.168 Version/12.00",
                "Mozilla/5.0 (Windows NT 5.1) Gecko/20100101 Firefox/14.0 Opera/12.0",
                "Opera/9.80 (Windows NT 6.1; WOW64; U; pt) Presto/2.10.229 Version/11.62",
                "Opera/9.80 (Windows NT 6.0; U; pl) Presto/2.10.229 Version/11.62",
                "Opera/9.80 (Macintosh; Intel Mac OS X 10.6.8; U; fr) Presto/2.9.168 Version/11.52",
                "Opera/9.80 (Macintosh; Intel Mac OS X 10.6.8; U; de) Presto/2.9.168 Version/11.52",
                "Opera/9.80 (Windows NT 5.1; U; en) Presto/2.9.168 Version/11.51",
                "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; de) Opera 11.51",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_6_8) AppleWebKit/537.13+ (KHTML, like Gecko) Version/5.1.7 Safari/534.57.2",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_3) AppleWebKit/534.55.3 (KHTML, like Gecko) Version/5.1.3 Safari/534.53.10",
                "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_8; de-at) AppleWebKit/533.21.1 (KHTML, like Gecko) Version/5.0.5 Safari/533.21.1",
                "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_7; da-dk) AppleWebKit/533.21.1 (KHTML, like Gecko) Version/5.0.5 Safari/533.21.1",
                "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27",
                "Mozilla/5.0 (Windows; U; Windows NT 6.1; tr-TR) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27",
                "Mozilla/5.0 (Windows; U; Windows NT 6.1; ko-KR) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27",
                "Mozilla/5.0 (Windows; U; Windows NT 6.1; fr-FR) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27",
                "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27",
                "Mozilla/5.0 (Windows; U; Windows NT 6.1; cs-CZ) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27",
                "Mozilla/5.0 (Windows; U; Windows NT 6.0; ja-JP) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27",
                "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/533.20.25 (KHTML, like Gecko) Version/5.0.4 Safari/533.20.27"
            };

        public bool doShowSplashScreen = true;
        public bool isLoadingCompleted = false;

        #endregion

        #region LoadSettings

        public static SysSettings Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new SysSettings();
                    }
                }
                return _instance;
            }
        }

        SysSettings()
        {
            /// <summary>
            /// Try to read settings, sources & languages from previous saving ini files.
            /// </summary>
            try
            {
                /// <summary>
                /// Initialize Article scraping settings
                /// check setting.ini exists
                /// </summary>                
                if (System.IO.File.Exists(_systemIniFile))
                {
                    LoadSystemSettings();
                    _messages.Add("Load system.ini successfully!");
                }
                else
                {
                    _messages.Add("Failed to load system.ini!");
                    //load defautl settings
                    DefaulSettings();
                }

                /// <summary>
                /// Initialize Article sources
                /// check source.ini exists 
                /// </summary>                    
                if (System.IO.File.Exists(_sourceIniFile))
                {
                    LoadSources();
                    _messages.Add("Load sources.ini successfully!");
                }
                else
                {
                    _messages.Add("Failed to load sources.ini!");
                }

                /// <summary>
                /// Initialize WP sites & VNS sites
                /// check sites.ini exists 
                /// </summary>                    
                if (System.IO.File.Exists(_siteIniFile))
                {
                    LoadPostSites();
                    LoadVnsSites();
                    _messages.Add("Load sites.ini successfully!");
                }
                else
                {
                    _messages.Add("Failed to load sites.ini!");
                }                

                /// <summary>
                /// Initialize GoogleDrive Information
                /// check gdrive.ini exists 
                /// </summary>                    
                //if (System.IO.File.Exists(_gDriveIniFile))
                //{
                //    LoadGDrive();
                //    _messages.Add("Load gdrive.ini successfully!");
                //}
                //else
                //{
                //    _messages.Add("Failed to load gdrive.ini!");
                //}

                /// <summary>
                /// Initialize language sources
                /// check language.ini exists 
                /// </summary>                                    
                if (System.IO.File.Exists(_languageIniFile))
                {
                    LoadLanguages();
                    _messages.Add("Load languages.ini successfully!");
                }
                else
                {
                    _messages.Add("Failed to load languages.ini!");
                }

                /// <summary>
                /// Initialize BlackWords                
                /// </summary> 
                LoadBlackWords();

                if (System.IO.File.Exists(_proxycFile))
                {
                    LoadProxies();
                }
                if (System.IO.File.Exists(_proxypFile))
                {
                    LoadPrivateProxies();
                }

                /// <summary>
                /// Initialize all saved posting projects & VNS projects
                /// </summary> 
                LoadAllProjects();
                LoadAllVnsProject();
                lock(_loadingLocker)
                    isLoadingCompleted = true;
            }
            catch (Exception ex)
            {
                var appEx = new ApplicationException("Cannot initialize some parts of scraping system!!!", ex);

                throw appEx;
            }
                        
            _scrapeState = ScrapeState.isInitialize;
            _postState = PostingState.isInitialize;
        }

        private void DefaulSettings()
        {
            /// <summary>
            /// Initialize Article scraping settings
            /// </summary>
            _article_scraping_searchOps = ArticleSearchOptions.InTitle;
            _article_scraping_MinWordOps = false;
            _article_scraping_MinWordValue = 5;
            _article_scraping_MaxWordOps = false;
            _article_scraping_MaxWordValue = 100;
            _article_scraping_Threads = 5;
            _article_scraping_MaxDepth = 10;

            /// <summary>
            /// Initialize Article storing settings
            /// </summary>
            _article_scraping_copyOps = ArticleCopyOptions.IndividualFiles;
            _article_scraping_savingOps = ArticleCopyContentSavingOptions.ArticlesAndTitles;
            _article_scraping_RemoveBlankLines = true;
            _article_scraping_UseTitleAsFileName = true;

            //create default saving folder in case of missing system ini file
            if (!Directory.Exists(_applicationPath + "\\Cache"))
            {
                Directory.CreateDirectory(_applicationPath + "\\Cache");
            }
            _article_scraping_SavingFolder = _applicationPath + "\\Cache\\Scrape";

            //make new software system ini file
            RemakeSystemIniFile();
        }
        
        private void RemakeSystemIniFile()
        {

        }

        private void LoadSystemSettings()
        {            
            //load settings
            _applicationName = IniHelper.GetIniFileString(_systemIniFile, "settings", "name", _defaultValue);
            _applicationVersion = IniHelper.GetIniFileString(_systemIniFile, "settings", "version", _defaultValue);

            string[] minWordValues = IniHelper.GetIniFileString(_systemIniFile, "settings", "minwords", _defaultValue).Split('|');
            _article_scraping_MinWordOps = Convert.ToBoolean(minWordValues[0]);
            _article_scraping_MinWordValue = Convert.ToInt32(minWordValues[1]);

            string[] maxWordValues = IniHelper.GetIniFileString(_systemIniFile, "settings", "maxwords", _defaultValue).Split('|');
            _article_scraping_MaxWordOps = Convert.ToBoolean(maxWordValues[0]);
            _article_scraping_MaxWordValue = Convert.ToInt32(maxWordValues[1]);

            string[] maxThreadsValue = IniHelper.GetIniFileString(_systemIniFile, "settings", "maxthreads", _defaultValue).Split('|');
            _article_scraping_ThreadsOpts = Convert.ToBoolean(maxThreadsValue[0]);
            _article_scraping_Threads = Convert.ToInt32(maxThreadsValue[1]);

            string[] maxDepthValues = IniHelper.GetIniFileString(_systemIniFile, "settings", "maxdepth", _defaultValue).Split('|');
            _article_scraping_MaxDepthOpts = Convert.ToBoolean(maxDepthValues[0]);
            _article_scraping_MaxDepth = Convert.ToInt32(maxDepthValues[1]);

            _article_scraping_RemoveBlankLines = Convert.ToBoolean(IniHelper.GetIniFileString(_systemIniFile, "settings", "removeblanklines", _defaultValue));
            _article_scraping_UseTitleAsFileName = Convert.ToBoolean(IniHelper.GetIniFileString(_systemIniFile, "settings", "usefilenameastitle", _defaultValue));
            _article_scraping_UseProxies = Convert.ToBoolean(IniHelper.GetIniFileString(_systemIniFile, "settings", "useproxies", _defaultValue));
            _encryptKey = IniHelper.GetIniFileString(_systemIniFile, "settings", "key", _defaultValue);

            //load options
            _article_scraping_searchOps = Utilities.Helper.Parse<ArticleSearchOptions>(IniHelper.GetIniFileString(_systemIniFile, "options", "searchopts", _defaultValue));
            _article_scraping_copyOps = Utilities.Helper.Parse<ArticleCopyOptions>(IniHelper.GetIniFileString(_systemIniFile, "options", "copyingopts", _defaultValue));
            _article_scraping_savingOps = Utilities.Helper.Parse<ArticleCopyContentSavingOptions>(IniHelper.GetIniFileString(_systemIniFile, "options", "scrapingopts", _defaultValue));
            _article_scrapingMode = Utilities.Helper.Parse<ScrapeMode>(IniHelper.GetIniFileString(_systemIniFile, "options", "scrapemode", _defaultValue)) ?? ScrapeMode.ByKeywords;

            //load path
            _article_scraping_SavingFolder = IniHelper.GetIniFileString(_systemIniFile, "path", "savepath", _defaultValue);

            //load plugins
            List<string> keys = IniHelper.GetKeys(_systemIniFile, "seoplugins");
            foreach (string key in keys)
            {
                string[] content = IniHelper.GetIniFileString(_systemIniFile, "seoplugins", key, _defaultValue).Split('|');
                SEOPlugin plugin = new SEOPlugin(Utilities.Helper.Parse<SEOPluginType>(content[0]), content[1]);

                _plugins.Add(plugin);
            }
        }

        private void LoadSources()
        {           
            //load all sources
            List<string> keys = IniHelper.GetKeys(_sourceIniFile, "articlesources");
            foreach (string key in keys)
            {
                string[] sourceContent = IniHelper.GetIniFileString(_sourceIniFile, "articlesources", key, _defaultValue).Split('|');
                ArticleSource item = new ArticleSource(sourceContent[0], sourceContent[1], sourceContent[2]);
                item.Language = Utilities.Helper.Parse<ArticleSourceLanguage>(sourceContent[3]) ?? ArticleSourceLanguage.English;                                
                item.Choosen = Convert.ToBoolean(sourceContent[5]);
                string[] sourceTypes = sourceContent[4].Split(';');
                string[] galleryCates = sourceContent[6].Split(';');
                string[] forbidCates = sourceContent[7].Split(';');

                foreach (var i in sourceTypes)
                {
                    item.Types.Add(Utilities.Helper.Parse<ArticleSourceType>(i) ?? ArticleSourceType.News);
                }

                foreach (var i in galleryCates)
                {
                    item.GalleryCategories.Add(i);
                }

                foreach (var i in forbidCates)
                {
                    item.ForbiddenCategories.Add(i);
                }

                item.ScrapeLinkStructure = sourceContent[8];

                _article_scraping_ScrapingSources.Add(item);
            }

            //load source categories
            List<string> categories = IniHelper.GetKeys(_sourceIniFile, "categories");

            foreach (var cate in categories)
            {
                string[] sourceContent = IniHelper.GetIniFileString(_sourceIniFile, "categories", cate, _defaultValue).Split('|');

                ArticleSource source = getSourceOfCategory(_article_scraping_ScrapingSources, sourceContent[0]);

                if (sourceContent.Count() == 2) // root category
                {
                    SourceCategory newCate = new SourceCategory(source.Title, sourceContent[1]);
                    source.AddCategory(newCate);
                }
                else //leaf category
                {
                    SourceCategory newCate = new SourceCategory(source.Title, sourceContent[1], source.FindCategory(source.Title, sourceContent[2]));
                    source.AddCategory(newCate);
                }
            }

            foreach (var src in _article_scraping_ScrapingSources)
            {
                src.LoadListCategories(null);
            }
        }

        private ArticleSource getSourceOfCategory(List<ArticleSource> list, string host)
        {
            foreach (var i in list)
            {
                if (i.Title == host)
                    return i;
            }
            return null;
        }

        private void LoadPostSites()
        {
            //load all sources
            List<string> sites = IniHelper.GetKeys(_siteIniFile, "sites");
            foreach (string site in sites)
            {
                string[] siteInfos = IniHelper.GetIniFileString(_siteIniFile, "sites", site, _defaultValue).Split('|');
                PostSiteType type = Helper.Parse<PostSiteType>(siteInfos[4]) ?? PostSiteType.Wordpress;
                TimeZoneInfo timeZone = Helper.GetTimeZoneInfo(siteInfos[5]);
                PostSites item = new PostSites(siteInfos[0], siteInfos[1], siteInfos[2], type, timeZone);
                item.DecryptPassword = StringCipher.Decrypt(siteInfos[2], _encryptKey);
                item.InitializeWordpress();               
                _article_posting_Sites.Add(item);
            }
        }

        private void LoadVnsSites()
        {
            //load all sources
            List<string> sites = IniHelper.GetKeys(_siteIniFile, "vns");
            foreach (string site in sites)
            {
                string[] siteInfos = IniHelper.GetIniFileString(_siteIniFile, "vns", site, _defaultValue).Split('|');
                VirtualNewspapersSite item = new VirtualNewspapersSite(siteInfos[0], siteInfos[1], siteInfos[2]);
                item.DecryptPassword = StringCipher.Decrypt(siteInfos[2], _encryptKey);
                item.InitializeWordpress();                
                _article_VNS_Sites.Add(item);
            }
        }

        public void InsertPostSite(PostSites site)
        {
            _article_posting_Sites.Add(site);
        }

        public void InsertVnsSite(VirtualNewspapersSite site)
        {
            _article_VNS_Sites.Add(site);
        }

        private void LoadLanguages()
        {
            //load all languages
            List<string> keys = IniHelper.GetKeys(_languageIniFile, "lang");
            foreach (string key in keys)
            {
                string[] sourceContent = IniHelper.GetIniFileString(_languageIniFile, "lang", key, _defaultValue).Split('|');
                Language item = new Language(sourceContent[0], Convert.ToBoolean(sourceContent[1]));
                _article_scraping_Languages.Add(item);
            }
        }

        private void LoadBlackWords()
        {
            List<string> removeWords = Utilities.Helper.ReadAdvancedValues(AppDomain.CurrentDomain.BaseDirectory + "advancedfilters.xml", "word");
            foreach (var word in removeWords)
            {
                _article_scraping_FilterWords.Add(new Token(word, ""));
            }
        }

        private void LoadProxies()
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(_proxycFile))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    _proxies.Enqueue(Utilities.Helper.GetProxy(line));
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
                    _proxies.Enqueue(Utilities.Helper.GetProxy(line));
                }
            }
        }

        private void LoadPrivateProxies()
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(_proxypFile))
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

        private void LoadGDrive()
        {
            _google_apis_clientID = IniHelper.GetIniFileString(_gDriveIniFile, "credentials", "ClientID", _defaultValue);
            _google_apis_clientSecret = StringCipher.Decrypt(IniHelper.GetIniFileString(_gDriveIniFile, "credentials", "ClientSecret", _defaultValue), _encryptKey);
            _google_apis_applicationName = IniHelper.GetIniFileString(_gDriveIniFile, "credentials", "ApplicationName", _defaultValue);                        
        }

        /// <summary>
        /// Posting Project loading
        /// </summary>
        private void LoadAllProjects()
        {
            if (Directory.Exists(_projectsPath))
            {
                string[] projectDirs = Directory.GetDirectories(_projectsPath);

                foreach (var dir in projectDirs)
                {
                    string projectPath = dir;
                    string[] savedProjects = Directory.GetFiles(dir, "*.ini");
                    foreach (string project in savedProjects)
                    {                        
                        _projects.Add(CreateProject(project, projectPath));
                    }                    
                }
                LoadProjectRules();
            }            
        }

        private PostingProject CreateProject(string projectINI, string projectPath)
        {
            PostingProject temp = new PostingProject("Temp");

            temp.ProjectPath = projectPath;
            temp.ProjectName = IniHelper.GetIniFileString(projectINI, "project", "name", _defaultValue);
            temp.LogoPath = IniHelper.GetIniFileString(projectINI, "project", "logopath", _defaultValue);
            temp.GoogleClientId = IniHelper.GetIniFileString(projectINI, "google", "ClientID", _defaultValue);
            temp.GoogleClientSecret = StringCipher.Decrypt(IniHelper.GetIniFileString(projectINI, "google", "ClientSecret", _defaultValue), _encryptKey);
            temp.GoogleApplicationName = IniHelper.GetIniFileString(projectINI, "google", "ApplicationName", _defaultValue);            

            List<string> sites = IniHelper.GetKeys(projectINI, "sites");
            foreach (string site in sites)
            {
                string host = IniHelper.GetIniFileString(projectINI, "sites", site, _defaultValue);
                foreach (var sysSite in _article_posting_Sites)
                {
                    if (sysSite.Host == host)
                    {
                        temp.LoadSite(sysSite);
                        break;
                    }
                }
            }

            return temp;
        }

        public void LoadProjectRules()
        {
            if (Directory.Exists(_projectsPath))
            {
                string[] projectDirs = Directory.GetDirectories(_projectsPath);

                foreach (var dir in projectDirs)
                {
                    //load ini file
                    string[] savedINIProjects = Directory.GetFiles(dir, "*.ini");
                    string[] savedXMLProjects = Directory.GetFiles(dir, "*.xml");

                    foreach (string path in savedINIProjects)
                    {
                        string projectName = IniHelper.GetIniFileString(path, "project", "name", _defaultValue);
                        foreach (var loadedProject in _projects)
                        {
                            if (loadedProject.ProjectName == projectName)
                            {
                                LoadProjectPostRules(path, loadedProject);
                                LoadProjectScheduleRules(path, loadedProject);
                            }
                        }
                    }

                    //load xml file
                    foreach (string path in savedXMLProjects)
                    {
                        XDocument doc = XDocument.Load(path);
                        var xmlProject = from n in doc.Descendants("project")
                                         select n;
                        foreach (var loadProject in _projects)
                        {
                            if (loadProject.ProjectName == xmlProject.First().Value)
                            {
                                LoadProjectSEORules(path, loadProject);
                            }
                        }
                    }
                }
            }
        }

        private void LoadProjectPostRules(string INIPath, PostingProject project)
        {
            List<string> rules = IniHelper.GetKeys(INIPath, "postrules");
            foreach (var rule in rules)
            {
                string[] content = IniHelper.GetIniFileString(INIPath, "postrules", rule, _defaultValue).Split('|');
                PostRule newRule = new PostRule(content[2], content[3], long.Parse(content[1]), content[0]);

                project.RuleCollection.AddRules(newRule);
            }
        }

        private void LoadProjectScheduleRules(string INIPath, PostingProject project)
        {
            project.ScheduleRule.ScheduleMode = Helper.Parse<ScheduleMode>(IniHelper.GetIniFileString(INIPath, "schedulerules", "mode", _defaultValue)) ?? ScheduleMode.LiveFeed;
            project.ScheduleRule.NumberOfPosts = Int32.Parse(IniHelper.GetIniFileString(INIPath, "schedulerules", "totalpost", _defaultValue));
            project.ScheduleRule.TimeRange = Helper.Parse<TimeRange>(IniHelper.GetIniFileString(INIPath, "schedulerules", "timerange", _defaultValue)) ?? TimeRange.Daily;
            project.ScheduleRule.NumberOfDays = Int32.Parse(IniHelper.GetIniFileString(INIPath, "schedulerules", "numberofday", _defaultValue));
            project.ScheduleRule.TimeBetweenPost = Helper.Parse<TimeBetweenPost>(IniHelper.GetIniFileString(INIPath, "schedulerules", "timebetweenpost", _defaultValue)) ?? TimeBetweenPost.Automatically;
            project.ScheduleRule.MinInterval = Int32.Parse(IniHelper.GetIniFileString(INIPath, "schedulerules", "mininterval", _defaultValue));
            project.ScheduleRule.MaxInterval = Int32.Parse(IniHelper.GetIniFileString(INIPath, "schedulerules", "maxinterval", _defaultValue));
            project.ScheduleRule.TimeUnit = Helper.Parse<TimeUnit>(IniHelper.GetIniFileString(INIPath, "schedulerules", "timeunit", _defaultValue)) ?? TimeUnit.Minute;
            project.ScheduleRule.ComebackInterval = Int32.Parse(IniHelper.GetIniFileString(INIPath, "schedulerules", "comebackinterval", _defaultValue));
            project.ScheduleRule.SearchDepth = Int32.Parse(IniHelper.GetIniFileString(INIPath, "schedulerules", "searchdepth", _defaultValue));
            project.ScheduleRule.Limitation = Helper.Parse<Limitation>(IniHelper.GetIniFileString(INIPath, "schedulerules", "limitation", _defaultValue)) ?? Limitation.Unlimited;
            project.ScheduleRule.StartDate = Convert.ToDateTime(IniHelper.GetIniFileString(INIPath, "schedulerules", "startdate", _defaultValue));
            project.ScheduleRule.EndDate = Convert.ToDateTime(IniHelper.GetIniFileString(INIPath, "schedulerules", "enddate", _defaultValue));
        }

        private void LoadProjectSEORules(string XMLpath, PostingProject project)
        {
            XDocument doc = XDocument.Load(XMLpath);
            var sites = from n in doc.Descendants("site")
                        select n;

            foreach (var site in sites)
            {
                SEORule rule = new SEORule(site.Element("name").Value);
                SEOPluginType? type = Utilities.Helper.Parse<SEOPluginType>(site.Element("plugin").Value);
                rule.CreateSEOPlugin(type);
                var categories = from cat in site.Descendants("category")
                                 select cat;
                foreach (var cate in categories)
                {
                    CategorySEORule categoryRule = new CategorySEORule(rule.Host, long.Parse(cate.Element("ID").Value));

                    var totalKWs = from tKWs in cate.Descendants("totalkeyword")
                                   select tKWs;

                    foreach (var value in totalKWs)
                    {
                        foreach (var element in value.Elements("tkvalue"))
                        {
                            categoryRule.ChangeTotalKeyword(Convert.ToInt32(element.Value));
                        }
                    }

                    var primaryKWsPer = from pKWsPer in cate.Descendants("primarypercent")
                                        select pKWsPer;

                    foreach (var pKWsPer in primaryKWsPer)
                    {
                        foreach (var element in pKWsPer.Elements("ppvalue"))
                        {
                            categoryRule.ChangePrimaryKeywordPercentage(Convert.ToInt32(element.Value));
                        }
                    }
                    
                    var secondaryKWsPer = from sKWsPer in cate.Descendants("secondarypercent")
                                          select sKWsPer;

                    foreach (var sKWsPer in secondaryKWsPer)
                    {
                        foreach (var element in sKWsPer.Elements("spvalue"))
                        {
                            categoryRule.ChangeSecondaryKeywordPercentage(Convert.ToInt32(element.Value));
                        }
                    }                    

                    var genericKWsPer = from gKWsPer in cate.Descendants("genericpercent")
                                        select gKWsPer;

                    foreach (var gKWsPer in genericKWsPer)
                    {
                        foreach (var element in gKWsPer.Elements("gpvalue"))
                        {
                            categoryRule.ChangeGenericKeywordPercentage(Convert.ToInt32(element.Value));
                        }
                    }

                    var keywordList = from kwList in cate.Descendants("keywordlist")
                                      select kwList;

                    foreach (var list in keywordList)
                    {
                        KeywordType kType = Helper.Parse<KeywordType>(list.Element("type").Value) ?? KeywordType.Generic;
                        InsertLinkOptions kOpt = Helper.Parse<InsertLinkOptions>(list.Element("insertoptions").Value) ?? InsertLinkOptions.Random;
                        KeywordList newItem = new KeywordList(list.Element("name").Value, kType, kOpt);

                        var keywords = from k in list.Descendants("keywords")
                                       select k;

                        foreach (var key in keywords)
                        {
                            foreach (var element in key.Elements("kvalue"))
                                newItem.AddPerKeyword(element.Value);
                        }

                        categoryRule.KeywordList.Add(newItem);
                    }  

                    var lists = from llst in cate.Descendants("linklist")
                                select llst;

                    foreach (var list in lists)
                    {
                        LinkList newItem = new LinkList();
                        newItem.Name = list.Element("name").Value;
                        newItem.KeywordListName = list.Element("keywordlistname").Value;
                        newItem.ApperanceNumber = Int32.Parse(list.Element("apperance").Value);                        

                        var links = from l in list.Descendants("links")
                                    select l;
                        foreach (var link in links)
                        {
                            foreach (var element in link.Elements("lvalue"))
                            {
                                newItem.Links.Add(element.Value);
                            }
                        }

                        categoryRule.AddLinkList(newItem);
                    }

                    var authority = from author in cate.Descendants("authority")
                                    select author;

                    categoryRule.InsertAuthorityLinks = false;
                    categoryRule.AuthorityKeywords = AuthorityKeywords.ArticleTags;
                    categoryRule.AuthoritySearch = AuthoritySearchOptions.SearchEngine;
                    categoryRule.AuthorityApperance = AuthorityApperance.UpTo1;

                    foreach (var value in authority)
                    {
                        categoryRule.InsertAuthorityLinks = Convert.ToBoolean(value.Element("avalue").Value);
                        categoryRule.AuthorityKeywords = Utilities.Helper.Parse<AuthorityKeywords>(value.Element("akvalue").Value) ?? AuthorityKeywords.ArticleTags;
                        categoryRule.AuthoritySearch = Utilities.Helper.Parse<AuthoritySearchOptions>(value.Element("asvalue").Value) ?? AuthoritySearchOptions.SearchEngine;
                        categoryRule.AuthorityApperance = Utilities.Helper.Parse<AuthorityApperance>(value.Element("aavalue").Value) ?? AuthorityApperance.UpTo1;
                    }

                    var video = from vd in cate.Descendants("video")
                                select vd;

                    categoryRule.InsertVideo = false;
                    categoryRule.VideoKeywords = AuthorityKeywords.ArticleTags;
                    categoryRule.VideoSearch = AuthoritySearchOptions.HighAuthoritySite;

                    foreach (var value in video)
                    {
                        categoryRule.InsertVideo = Convert.ToBoolean(value.Element("vvalue").Value);
                        categoryRule.VideoKeywords = Utilities.Helper.Parse<AuthorityKeywords>(value.Element("vkvalue").Value) ?? AuthorityKeywords.ArticleTags;
                        categoryRule.VideoSearch = Utilities.Helper.Parse<AuthoritySearchOptions>(value.Element("vsvalue").Value) ?? AuthoritySearchOptions.HighAuthoritySite;
                    }

                    var internalLink = from intL in cate.Descendants("internal")
                                       select intL;

                    categoryRule.InsertInternalLink = false;
                    categoryRule.InternalKeywords = AuthorityKeywords.ArticleTags;

                    foreach (var value in internalLink)
                    {
                        categoryRule.InsertInternalLink = Convert.ToBoolean(value.Element("ivalue").Value);
                        categoryRule.InternalKeywords = Utilities.Helper.Parse<AuthorityKeywords>(value.Element("ikvalue").Value) ?? AuthorityKeywords.ArticleTags;
                    }

                    rule.AddCategorySEORule(categoryRule);
                }
                project.SeoRuleCollection.AddRule(rule);
            }
        }

        /// <summary>
        /// VNS project loading
        /// </summary>
        private void LoadAllVnsProject()
        {
            if (Directory.Exists(_vnsPath))
            {
                string[] projectDirs = Directory.GetDirectories(_vnsPath);

                foreach (var dir in projectDirs)
                {
                    string projectPath = dir;
                    string[] savedProjects = Directory.GetFiles(dir, "*.ini");

                    foreach (string project in savedProjects)
                    {
                        _vnsProjects.Add(CreateVnsProject(project, projectPath));
                    }
                }
                LoadVnsProjectRules();
            }
        }

        private VNSPostingProject CreateVnsProject(string projectINI, string projectPath)
        {
            VNSPostingProject temp = new VNSPostingProject("temp");

            temp.ProjectPath = projectPath;
            temp.ProjectName = IniHelper.GetIniFileString(projectINI, "project", "name", _defaultValue);
            temp.LogoPath = IniHelper.GetIniFileString(projectINI, "project", "logopath", _defaultValue);
            temp.GoogleClientId = IniHelper.GetIniFileString(projectINI, "google", "ClientID", _defaultValue);
            temp.GoogleClientSecret = StringCipher.Decrypt(IniHelper.GetIniFileString(projectINI, "google", "ClientSecret", _defaultValue), _encryptKey);
            temp.GoogleApplicationName = IniHelper.GetIniFileString(projectINI, "google", "ApplicationName", _defaultValue);

            List<string> sites = IniHelper.GetKeys(projectINI, "sites");
            foreach (string site in sites)
            {
                string host = IniHelper.GetIniFileString(projectINI, "sites", site, _defaultValue);
                foreach (var sysSite in _article_posting_Sites)
                {
                    if (sysSite.Host == host)
                    {
                        temp.LoadPostSite(sysSite);
                        break;
                    }
                }
            }

            return temp;
        }

        public void LoadVnsProjectRules()
        {
            if (Directory.Exists(_vnsPath))
            {
                string[] projectDirs = Directory.GetDirectories(_vnsPath);

                foreach (var dir in projectDirs)
                {
                    //load ini file
                    string[] savedINIProjects = Directory.GetFiles(dir, "*.ini");
                    string[] savedXMLProjects = Directory.GetFiles(dir, "*.xml");

                    foreach (string path in savedINIProjects)
                    {
                        string projectName = IniHelper.GetIniFileString(path, "project", "name", _defaultValue);
                        foreach (var loadedProject in _vnsProjects)
                        {
                            if (loadedProject.ProjectName == projectName)
                            {
                                LoadVnsProjectPostRules(path, loadedProject);
                                LoadVnsProjectScheduleRules(path, loadedProject);
                            }
                        }
                    }

                    //load xml file
                    foreach (string path in savedXMLProjects)
                    {
                        XDocument doc = XDocument.Load(path);
                        var xmlProject = from n in doc.Descendants("project")
                                         select n;
                        foreach (var loadProject in _vnsProjects)
                        {
                            if (loadProject.ProjectName == xmlProject.First().Value)
                            {
                                LoadVnsProjectSEORules(path, loadProject);
                            }
                        }
                    }
                }
            }
        }

        private void LoadVnsProjectPostRules(string INIPath, VNSPostingProject project)
        {
            List<string> rules = IniHelper.GetKeys(INIPath, "vnspostrules");
            foreach (var rule in rules)
            {
                string[] content = IniHelper.GetIniFileString(INIPath, "vnspostrules", rule, _defaultValue).Split('|');
                VirtualNewspapersPostRule newRule = new VirtualNewspapersPostRule(content[0], long.Parse(content[1]), content[2], long.Parse(content[3]));

                project.RuleCollection.AddRules(newRule);
            }
        }

        private void LoadVnsProjectScheduleRules(string INIPath, VNSPostingProject project)
        {
            project.ScheduleRule.NumberOfPostsPerDay = Int32.Parse(IniHelper.GetIniFileString(INIPath, "vnsschedulerules", "numberofpostperday", _defaultValue));
            project.ScheduleRule.TimeBetweenPost = Helper.Parse<TimeBetweenPost>(IniHelper.GetIniFileString(INIPath, "vnsschedulerules", "timebetweenpost", _defaultValue)) ?? TimeBetweenPost.Automatically;
            project.ScheduleRule.MinInterval = Int32.Parse(IniHelper.GetIniFileString(INIPath, "vnsschedulerules", "mininterval", _defaultValue));
            project.ScheduleRule.MaxInterval = Int32.Parse(IniHelper.GetIniFileString(INIPath, "vnsschedulerules", "maxinterval", _defaultValue));
            project.ScheduleRule.TimeUnit = Helper.Parse<TimeUnit>(IniHelper.GetIniFileString(INIPath, "vnsschedulerules", "timeunit", _defaultValue)) ?? TimeUnit.Minute;
            project.ScheduleRule.ComebackInterval = Int32.Parse(IniHelper.GetIniFileString(INIPath, "vnsschedulerules", "comebackinterval", _defaultValue));
        }

        private void LoadVnsProjectSEORules(string XMLPath, VNSPostingProject project)
        {
            XDocument doc = XDocument.Load(XMLPath);
            var sites = from n in doc.Descendants("site")
                        select n;

            foreach (var site in sites)
            {
                SEORule rule = new SEORule(site.Element("name").Value);
                SEOPluginType? type = Utilities.Helper.Parse<SEOPluginType>(site.Element("plugin").Value);
                rule.CreateSEOPlugin(type);
                var categories = from cat in site.Descendants("category")
                                 select cat;
                foreach (var cate in categories)
                {
                    CategorySEORule categoryRule = new CategorySEORule(rule.Host, long.Parse(cate.Element("ID").Value));

                    var totalKWs = from tKWs in cate.Descendants("totalkeyword")
                                   select tKWs;

                    foreach (var value in totalKWs)
                    {
                        foreach (var element in value.Elements("tkvalue"))
                        {
                            categoryRule.ChangeTotalKeyword(Convert.ToInt32(element.Value));
                        }
                    }

                    var primaryKWsPer = from pKWsPer in cate.Descendants("primarypercent")
                                        select pKWsPer;

                    foreach (var pKWsPer in primaryKWsPer)
                    {
                        foreach (var element in pKWsPer.Elements("ppvalue"))
                        {
                            categoryRule.ChangePrimaryKeywordPercentage(Convert.ToInt32(element.Value));
                        }
                    }                    

                    var secondaryKWsPer = from sKWsPer in cate.Descendants("secondarypercent")
                                          select sKWsPer;

                    foreach (var sKWsPer in secondaryKWsPer)
                    {
                        foreach (var element in sKWsPer.Elements("spvalue"))
                        {
                            categoryRule.ChangeSecondaryKeywordPercentage(Convert.ToInt32(element.Value));
                        }
                    }
                    
                    var genericKWsPer = from gKWsPer in cate.Descendants("genericpercent")
                                        select gKWsPer;

                    foreach (var gKWsPer in genericKWsPer)
                    {
                        foreach (var element in gKWsPer.Elements("gpvalue"))
                        {
                            categoryRule.ChangeGenericKeywordPercentage(Convert.ToInt32(element.Value));
                        }
                    }

                    var keywordList = from kwList in cate.Descendants("keywordlist")
                                      select kwList;

                    foreach (var list in keywordList)
                    {
                        KeywordType kType = Helper.Parse<KeywordType>(list.Element("type").Value) ?? KeywordType.Generic;
                        InsertLinkOptions kOpt = Helper.Parse<InsertLinkOptions>(list.Element("insertoptions").Value) ?? InsertLinkOptions.Random;
                        KeywordList newItem = new KeywordList(list.Element("name").Value, kType, kOpt);

                        var keywords = from k in list.Descendants("keywords")
                                       select k;

                        foreach (var key in keywords)
                        {
                            foreach (var element in key.Elements("kvalue"))
                                newItem.AddPerKeyword(element.Value);
                        }

                        categoryRule.KeywordList.Add(newItem);
                    }

                    var lists = from llst in cate.Descendants("linklist")
                                select llst;

                    foreach (var list in lists)
                    {
                        LinkList newItem = new LinkList();
                        newItem.Name = list.Element("name").Value;
                        newItem.KeywordListName = list.Element("keywordlistname").Value;
                        newItem.ApperanceNumber = Int32.Parse(list.Element("apperance").Value);                        

                        var links = from l in list.Descendants("links")
                                    select l;
                        foreach (var link in links)
                        {
                            foreach (var element in link.Elements("lvalue"))
                            {
                                newItem.Links.Add(element.Value);
                            }
                        }

                        categoryRule.AddLinkList(newItem);
                    }

                    var authority = from author in cate.Descendants("authority")
                                    select author;

                    categoryRule.InsertAuthorityLinks = false;
                    categoryRule.AuthorityKeywords = AuthorityKeywords.ArticleTags;
                    categoryRule.AuthoritySearch = AuthoritySearchOptions.SearchEngine;
                    categoryRule.AuthorityApperance = AuthorityApperance.UpTo1;

                    foreach (var value in authority)
                    {
                        categoryRule.InsertAuthorityLinks = Convert.ToBoolean(value.Element("avalue").Value);
                        categoryRule.AuthorityKeywords = Utilities.Helper.Parse<AuthorityKeywords>(value.Element("akvalue").Value) ?? AuthorityKeywords.ArticleTags;
                        categoryRule.AuthoritySearch = Utilities.Helper.Parse<AuthoritySearchOptions>(value.Element("asvalue").Value) ?? AuthoritySearchOptions.SearchEngine;
                        categoryRule.AuthorityApperance = Utilities.Helper.Parse<AuthorityApperance>(value.Element("aavalue").Value) ?? AuthorityApperance.UpTo1;
                    }


                    rule.AddCategorySEORule(categoryRule);
                }
                project.SeoRuleCollection.AddRule(rule);
            }
        }

        #endregion

        #region SaveSettings

        public void SaveSystem()
        {
            SaveSettings();
            SaveSources();
            SavePostSites();
            SaveVnsSites();
            SaveLanguages();
            //SaveProxies();
            SaveAllProjects();
            SaveAllVnsProjects();
        }

        private void SaveSettings()
        {
            //write System settings
            IniHelper.WriteKey(_systemIniFile, "settings", "name", _applicationName);
            IniHelper.WriteKey(_systemIniFile, "settings", "version", _applicationVersion);
            IniHelper.WriteKey(_systemIniFile, "settings", "minwords", _article_scraping_MinWordOps.ToString() + "|" + _article_scraping_MinWordValue.ToString());
            IniHelper.WriteKey(_systemIniFile, "settings", "maxwords", _article_scraping_MaxWordOps.ToString() + "|" + _article_scraping_MaxWordValue.ToString());
            IniHelper.WriteKey(_systemIniFile, "settings", "maxthreads", _article_scraping_ThreadsOpts.ToString() + "|" + _article_scraping_Threads.ToString());
            IniHelper.WriteKey(_systemIniFile, "settings", "maxdepth", _article_scraping_MaxDepthOpts.ToString() + "|" + _article_scraping_MaxDepth.ToString());
            IniHelper.WriteKey(_systemIniFile, "settings", "removeblanklines", _article_scraping_RemoveBlankLines.ToString());
            IniHelper.WriteKey(_systemIniFile, "settings", "usefilenameastitle", _article_scraping_UseTitleAsFileName.ToString());
            IniHelper.WriteKey(_systemIniFile, "settings", "useproxies", _article_scraping_UseProxies.ToString());
            IniHelper.WriteKey(_systemIniFile, "settings", "key", _encryptKey);

            IniHelper.WriteKey(_systemIniFile, "options", "searchopts", _article_scraping_searchOps.ToString());
            IniHelper.WriteKey(_systemIniFile, "options", "scrapingopts", _article_scraping_savingOps.ToString());
            IniHelper.WriteKey(_systemIniFile, "options", "copyingopts", _article_scraping_copyOps.ToString());
            IniHelper.WriteKey(_systemIniFile, "options", "scrapemode", _article_scrapingMode.ToString());

            IniHelper.WriteKey(_systemIniFile, "path", "savepath", _article_scraping_SavingFolder.ToString());
        }

        private void SaveSources()
        {
            int count = 1;
            foreach (var i in _article_scraping_ScrapingSources)
            {
                string value = i.Name + "|" + i.Title + "|" + i.Url + "|" + i.Language.ToString() + "|";

                foreach (var j in i.Types)
                {
                    if (j != i.Types.Last())
                        value += j.ToString() + ";";
                    else
                        value += j.ToString() + "|";
                }

                value += i.Choosen.ToString() + "|";

                foreach (var j in i.GalleryCategories)
                {
                    if (j != i.GalleryCategories.Last())
                        value += j + ";";
                    else
                        value += j + "|";
                }

                foreach (var j in i.ForbiddenCategories)
                {
                    if (j != i.ForbiddenCategories.Last())
                        value += j + ";";
                    else
                        value += j + "|";
                }

                value += i.ScrapeLinkStructure;

                IniHelper.WriteKey(_sourceIniFile, "articlesources", "source" + count, value);
                count++;
            }
            count = 1;
            foreach (var i in _article_scraping_ScrapingSources)
            {
                foreach (var str in i._cateList)
                {
                    IniHelper.WriteKey(_sourceIniFile, "categories", "cate" + count, str);
                    count++;
                }
            }
        }

        private void SavePostSites()
        {
            int count = 1;
            foreach (var i in _article_posting_Sites)
            {
                string value = i.Host + "|" + i.Username + "|" + i.EncryptPassword + "|" + i.Chosen.ToString() + "|" + i.Type.ToString() + "|" + i.TimeZone.Id;
                IniHelper.WriteKey(_siteIniFile, "sites", "site" + count, value);
                count++;
            }
        }

        private void SaveVnsSites()
        {
            int count = 1;
            foreach (var i in _article_VNS_Sites)
            {
                string value = i.Host + "|" + i.Username + "|" + i.EncryptPassword + "|" + i.Chosen.ToString();
                IniHelper.WriteKey(_siteIniFile, "vns", "vns" + count, value);
                count++;
            }
        }

        private void SaveLanguages()
        {
            int count = 1;
            foreach (var i in _article_scraping_Languages)
            {
                IniHelper.WriteKey(_languageIniFile, "lang", "lang" + count, i.Name + "|" + i.Choosen.ToString());
                count++;
            }
        }

        private void SaveProxies()
        {            
            using (StreamWriter writer = new StreamWriter(_proxycFile, false))
            {
                foreach (var i in _proxies)
                {
                    writer.WriteLine(i.ToString());       
                }
            }

            using (StreamWriter writer = new StreamWriter(_proxypFile, false))
            {
                foreach (var i in _privateProxies)
                {
                    writer.WriteLine(i.ToString());
                }
            }
        }

        private void SaveGDrive()
        {
            IniHelper.WriteKey(_gDriveIniFile, "credentials", "ClientID", _google_apis_clientID);
            IniHelper.WriteKey(_gDriveIniFile, "credentials", "ClientSecret", _google_apis_clientSecret);
            IniHelper.WriteKey(_gDriveIniFile, "credentials", "ApplicationName", _google_apis_applicationName);
        }

        private void SaveAllProjects()
        {
            int count = 1;
            foreach (var proj in _projects)
            {
                string iniFileName = _projectsPath + Regex.Replace(proj.ProjectName, @"\s+", "_").ToLower() + "\\" + Regex.Replace(proj.ProjectName, @"\s+", "").ToLower() + ".ini";
                string xmlFileName = _projectsPath + Regex.Replace(proj.ProjectName, @"\s+", "_").ToLower() + "\\" + Regex.Replace(proj.ProjectName, @"\s+", "").ToLower() + ".xml";

                //save project settings
                IniHelper.WriteKey(iniFileName, "project", "name", proj.ProjectName);
                IniHelper.WriteKey(iniFileName, "project", "logopath", proj.LogoPath);
                //save google settings
                IniHelper.WriteKey(iniFileName, "google", "ClientID", proj.GoogleClientId);
                IniHelper.WriteKey(iniFileName, "google", "ClientSecret", StringCipher.Encrypt(proj.GoogleClientSecret, _encryptKey));
                IniHelper.WriteKey(iniFileName, "google", "ApplicationName", proj.GoogleApplicationName);

                //save chosen sites settings
                count = 1;
                foreach (var site in proj.Sites)
                {
                    IniHelper.WriteKey(iniFileName, "sites", "site" + count, site.Host);
                    count++;
                }

                //save PostRule settings
                count = 1;
                foreach (PostRule rule in proj.RuleCollection.Rules)
                {
                    IniHelper.WriteKey(iniFileName, "postrules", "rule" + count, rule.SiteHost + "|" + rule.SiteCategoryID.ToString() + "|"  + rule.SourceHost + "|" + rule.SourceCategory);
                    count++;
                }

                //save ScheduleRule settings                
                IniHelper.WriteKey(iniFileName, "schedulerules", "mode", proj.ScheduleRule.ScheduleMode.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "totalpost", proj.ScheduleRule.NumberOfPosts.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "timerange", proj.ScheduleRule.TimeRange.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "numberofday", proj.ScheduleRule.NumberOfDays.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "timebetweenpost", proj.ScheduleRule.TimeBetweenPost.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "mininterval", proj.ScheduleRule.MinInterval.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "maxinterval", proj.ScheduleRule.MaxInterval.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "timeunit", proj.ScheduleRule.TimeUnit.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "comebackinterval", proj.ScheduleRule.ComebackInterval.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "searchdepth", proj.ScheduleRule.SearchDepth.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "limitation", proj.ScheduleRule.Limitation.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "startdate", proj.ScheduleRule.StartDate.ToString());
                IniHelper.WriteKey(iniFileName, "schedulerules", "enddate", proj.ScheduleRule.EndDate.ToString());
                //save SEORule settings
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "\t";
                using (XmlWriter writer = XmlWriter.Create(xmlFileName, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("sites");
                    writer.WriteElementString("project", proj.ProjectName);   
                    
                    foreach (var rule in proj.SeoRuleCollection.Rules)
                    {                        
                        writer.WriteStartElement("site");                        
                        writer.WriteElementString("name", rule.Host);
                        writer.WriteElementString("plugin", rule.Type.ToString());

                        foreach (var category in rule.CategoryRules)
                        {
                            writer.WriteStartElement("category");
                            writer.WriteElementString("ID", category.CategoryID.ToString());
                            
                            //write totalkeyword
                            writer.WriteStartElement("totalkeyword");
                            writer.WriteElementString("tkvalue", category.TotalKeywords.ToString());
                            writer.WriteEndElement();

                            //write primary keyword percent
                            writer.WriteStartElement("primarypercent");
                            writer.WriteElementString("ppvalue", category.PrimaryKeywordPercentage.ToString());
                            writer.WriteEndElement();
                           
                            //write secondary keyword percent
                            writer.WriteStartElement("secondarypercent");
                            writer.WriteElementString("spvalue", category.SecondaryKeywordPercentage.ToString());
                            writer.WriteEndElement();
                            
                            //write generic keyword percent
                            writer.WriteStartElement("genericpercent");
                            writer.WriteElementString("gpvalue", category.GenericKeywordPercentage.ToString());
                            writer.WriteEndElement();

                            //write keywordList
                            foreach (var list in category.KeywordList)
                            {
                                writer.WriteStartElement("keywordlist");
                                writer.WriteElementString("name", list.Name);
                                writer.WriteElementString("type", list.KeywordType.ToString());
                                writer.WriteElementString("insertoptions", list.InsertOpt.ToString());

                                writer.WriteStartElement("keywords");

                                foreach (var key in list.Keywords)
                                    writer.WriteElementString("kvalue", key);

                                writer.WriteEndElement();
                                writer.WriteEndElement();
                            }

                            //write linklist
                            foreach (var linkList in category.CategoryLinkList)
                            {
                                writer.WriteStartElement("linklist");
                                writer.WriteElementString("name", linkList.Name);
                                writer.WriteElementString("keywordlistname", linkList.KeywordListName);
                                writer.WriteElementString("apperance", linkList.ApperanceNumber.ToString());                                

                                writer.WriteStartElement("links");
                                foreach (var link in linkList.Links)
                                {
                                    writer.WriteElementString("lvalue", link);
                                }
                                writer.WriteEndElement();
                                writer.WriteEndElement();
                            }

                            //write authority settings
                            writer.WriteStartElement("authority");
                            writer.WriteElementString("avalue", category.InsertAuthorityLinks.ToString());
                            writer.WriteElementString("akvalue", category.AuthorityKeywords.ToString());
                            writer.WriteElementString("asvalue", category.AuthoritySearch.ToString());
                            writer.WriteElementString("aavalue", category.AuthorityApperance.ToString());
                            writer.WriteEndElement();

                            //write video settings
                            writer.WriteStartElement("video");
                            writer.WriteElementString("vvalue", category.InsertVideo.ToString());
                            writer.WriteElementString("vkvalue", category.VideoKeywords.ToString());
                            writer.WriteElementString("vsvalue", category.VideoSearch.ToString());
                            writer.WriteEndElement();

                            writer.WriteStartElement("internal");
                            writer.WriteElementString("ivalue", category.InsertInternalLink.ToString());
                            writer.WriteElementString("ikvalue", category.InternalKeywords.ToString());
                            writer.WriteEndElement();

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
        }

        private void SaveAllVnsProjects()
        {
            int count = 1;
            foreach (var proj in _vnsProjects)
            {
                string iniFileName = _vnsPath + Regex.Replace(proj.ProjectName, @"\s+", "_").ToLower() + "\\" + Regex.Replace(proj.ProjectName, @"\s+", "").ToLower() + ".ini";
                string xmlFileName = _vnsPath + Regex.Replace(proj.ProjectName, @"\s+", "_").ToLower() + "\\" + Regex.Replace(proj.ProjectName, @"\s+", "").ToLower() + ".xml";

                //save project settings
                IniHelper.WriteKey(iniFileName, "project", "name", proj.ProjectName);
                IniHelper.WriteKey(iniFileName, "project", "logopath", proj.LogoPath);
                //save google settings
                IniHelper.WriteKey(iniFileName, "google", "ClientID", proj.GoogleClientId);
                IniHelper.WriteKey(iniFileName, "google", "ClientSecret", StringCipher.Encrypt(proj.GoogleClientSecret, _encryptKey));
                IniHelper.WriteKey(iniFileName, "google", "ApplicationName", proj.GoogleApplicationName);

                //save chosen sites settings
                count = 1;
                foreach (var site in proj.PostSites)
                {
                    IniHelper.WriteKey(iniFileName, "sites", "site" + count, site.Host);
                    count++;
                }

                //save PostRule settings
                count = 1;
                foreach (VirtualNewspapersPostRule rule in proj.RuleCollection.Rules)
                {
                    IniHelper.WriteKey(iniFileName, "vnspostrules", "rule" + count, rule.VNSHost + "|" + rule.VNSCategory.ToString() + "|" + rule.SiteHost + "|" + rule.SiteCategoryID);
                    count++;
                }

                IniHelper.WriteKey(iniFileName, "vnsschedulerules", "numberofpostperday", proj.ScheduleRule.NumberOfPostsPerDay.ToString());
                IniHelper.WriteKey(iniFileName, "vnsschedulerules", "timebetweenpost", proj.ScheduleRule.TimeBetweenPost.ToString());
                IniHelper.WriteKey(iniFileName, "vnsschedulerules", "mininterval", proj.ScheduleRule.MinInterval.ToString());
                IniHelper.WriteKey(iniFileName, "vnsschedulerules", "maxinterval", proj.ScheduleRule.MaxInterval.ToString());
                IniHelper.WriteKey(iniFileName, "vnsschedulerules", "timeunit", proj.ScheduleRule.TimeUnit.ToString());
                IniHelper.WriteKey(iniFileName, "vnsschedulerules", "comebackinterval", proj.ScheduleRule.ComebackInterval.ToString());

                //save SEORule settings
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "\t";
                using (XmlWriter writer = XmlWriter.Create(xmlFileName, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("sites");
                    writer.WriteElementString("project", proj.ProjectName);
                    foreach (var rule in proj.SeoRuleCollection.Rules)
                    {
                        writer.WriteStartElement("site");
                        writer.WriteElementString("name", rule.Host);
                        writer.WriteElementString("plugin", rule.Type.ToString());

                        foreach (var category in rule.CategoryRules)
                        {
                            writer.WriteStartElement("category");
                            writer.WriteElementString("ID", category.CategoryID.ToString());

                            //write totalkeyword
                            writer.WriteStartElement("totalkeyword");
                            writer.WriteElementString("tkvalue", category.TotalKeywords.ToString());
                            writer.WriteEndElement();

                            //write primary keyword percent
                            writer.WriteStartElement("primarypercent");
                            writer.WriteElementString("ppvalue", category.PrimaryKeywordPercentage.ToString());
                            writer.WriteEndElement();
                            
                            //write secondary keyword percent
                            writer.WriteStartElement("secondarypercent");
                            writer.WriteElementString("spvalue", category.SecondaryKeywordPercentage.ToString());
                            writer.WriteEndElement();
                            
                            //write generic keyword percent
                            writer.WriteStartElement("genericpercent");
                            writer.WriteElementString("gpvalue", category.GenericKeywordPercentage.ToString());
                            writer.WriteEndElement();

                            //write keywordList
                            foreach (var list in category.KeywordList)
                            {
                                writer.WriteStartElement("keywordlist");
                                writer.WriteElementString("name", list.Name);
                                writer.WriteElementString("type", list.KeywordType.ToString());
                                writer.WriteElementString("insertoptions", list.InsertOpt.ToString());

                                writer.WriteStartElement("keywords");

                                foreach (var key in list.Keywords)
                                    writer.WriteElementString("kvalue", key);

                                writer.WriteEndElement();
                                writer.WriteEndElement();
                            }

                            //write linklist
                            foreach (var linkList in category.CategoryLinkList)
                            {
                                writer.WriteStartElement("linklist");
                                writer.WriteElementString("name", linkList.Name);
                                writer.WriteElementString("keywordlistname", linkList.KeywordListName);
                                writer.WriteElementString("apperance", linkList.ApperanceNumber.ToString());                                

                                writer.WriteStartElement("links");
                                foreach (var link in linkList.Links)
                                {
                                    writer.WriteElementString("lvalue", link);
                                }
                                writer.WriteEndElement();
                                writer.WriteEndElement();
                            }

                            //write authority settings
                            writer.WriteStartElement("authority");
                            writer.WriteElementString("avalue", category.InsertAuthorityLinks.ToString());
                            writer.WriteElementString("akvalue", category.AuthorityKeywords.ToString());
                            writer.WriteElementString("asvalue", category.AuthoritySearch.ToString());
                            writer.WriteElementString("aavalue", category.AuthorityApperance.ToString());
                            writer.WriteEndElement();

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
        }

        #endregion

        #region UtilityMethods

        public void UpdateScrapeState(ScrapeState value)
        {
            lock(_locker2)
                _scrapeState = value;
        }

        public void UpdatePostState(PostingState value)
        {
            lock (_locker2)
                _postState = value;
        }

        public SystemState getSystemState()
        {
            lock (_locker2)
            {
                if (_postState == PostingState.isInitialize && _scrapeState == ScrapeState.isInitialize)
                    return SystemState.isIdle;
                return SystemState.isWorking;
            }
        }

        public void ChangeGoogleDrive(string id, string secret, string name)
        {
            _google_apis_clientID = id;
            _google_apis_clientSecret = secret;
            _google_apis_applicationName = name;
        }
        
        public void AddProject(PostingProject project)
        {
            //create project folder & files
            string projectFolder = _projectsPath + Regex.Replace(project.ProjectName, @"\s+", "_").ToLower();

            if (!Directory.Exists(projectFolder))
                Directory.CreateDirectory(projectFolder);

            string iniFileName = _projectsPath + Regex.Replace(project.ProjectName, @"\s+", "_").ToLower() + "\\" + Regex.Replace(project.ProjectName, @"\s+", "").ToLower() + ".ini";

            if (!System.IO.File.Exists(iniFileName))
            {
                using (Stream file = System.IO.File.Create(iniFileName))
                {
                }
            }

            string xmlFileName = _projectsPath + Regex.Replace(project.ProjectName, @"\s+", "_").ToLower() + "\\" + Regex.Replace(project.ProjectName, @"\s+", "").ToLower() + ".xml";

            if (!System.IO.File.Exists(xmlFileName))
            {
                using (Stream file = System.IO.File.Create(xmlFileName))
                {
                }
            }
            project.ProjectPath = projectFolder;
            _projects.Add(project);
        }

        public void AddVnsProject(VNSPostingProject project)
        {
            string projectFolder = _vnsPath + Regex.Replace(project.ProjectName, @"\s+", "_").ToLower();

            if (!Directory.Exists(projectFolder))
                Directory.CreateDirectory(projectFolder);

            string iniFileName = _vnsPath + Regex.Replace(project.ProjectName, @"\s+", "_").ToLower() + "\\" + Regex.Replace(project.ProjectName, @"\s+", "").ToLower() + ".ini";

            if (!System.IO.File.Exists(iniFileName))
            {
                using (Stream file = System.IO.File.Create(iniFileName))
                {
                }
            }

            string xmlFileName = _vnsPath + Regex.Replace(project.ProjectName, @"\s+", "_").ToLower() + "\\" + Regex.Replace(project.ProjectName, @"\s+", "").ToLower() + ".xml";

            if (!System.IO.File.Exists(xmlFileName))
            {
                using (Stream file = System.IO.File.Create(xmlFileName))
                {
                }
            }

            _vnsProjects.Add(project);
        }

        public PostingProject GetProject(string name)
        {
            foreach (var prj in _projects)
                if (string.Compare(prj.ProjectName,name,true) == 0)
                    return prj;
            return null;
        }
        
        public VNSPostingProject GetVnsProject(string name)
        {
            foreach (var prj in _vnsProjects)
                if (string.Compare(prj.ProjectName, name, true) == 0)
                    return prj;
            return null;
        }

        public List<PostingProject> GetAllProjects()
        {
            return _projects;
        }

        public List<VNSPostingProject> GetAllVNSProjects()
        {
            return _vnsProjects;
        }

        public void RemoveProject(string name)
        {
            PostingProject remove = _projects.Where(a => a.ProjectName == name).Single();
            string iniFileName = _projectsPath + Regex.Replace(remove.ProjectName, @"\s+", "").ToLower() + ".ini";

            if (System.IO.File.Exists(iniFileName))
                System.IO.File.Delete(iniFileName);

            string xmlFileName = _projectsPath + Regex.Replace(remove.ProjectName, @"\s+", "").ToLower() + ".xml";

            if (System.IO.File.Exists(xmlFileName))
                System.IO.File.Delete(xmlFileName);

            _projects.Remove(remove);
        }

        public void RemoveVnsProject(string name)
        {
            VNSPostingProject remove = _vnsProjects.Where(a => a.ProjectName == name).Single();

            string iniFileName = _vnsPath + Regex.Replace(remove.ProjectName, @"\s+", "").ToLower() + ".ini";

            if (System.IO.File.Exists(iniFileName))
                System.IO.File.Delete(iniFileName);

            string xmlFileName = _vnsPath + Regex.Replace(remove.ProjectName, @"\s+", "").ToLower() + ".xml";

            if (System.IO.File.Exists(xmlFileName))
                System.IO.File.Delete(xmlFileName);

            _vnsProjects.Remove(remove);
        }

        public PostSites GetSite(string host)
        {
            foreach (var site in _article_posting_Sites)
                if (string.Compare(site.Host, host, true) == 0)
                    return site;
            return null;
        }

        public VirtualNewspapersSite GetVnsSite(string host)
        {
            foreach (var site in _article_VNS_Sites)
                if (string.Compare(site.Host, host, true) == 0)
                    return site;
            return null;
        }

        public void UpdateSite(string oldSiteHost, PostSites newSite)
        {
            foreach (var site in _article_posting_Sites)
                if (string.Compare(site.Host, oldSiteHost, true) == 0)
                {
                    site.ClonePostSites(newSite);
                    break;
                }            
        }

        public void UpdateVnsSite(string oldSiteHost, VirtualNewspapersSite newSite)
        {
            foreach (var site in _article_VNS_Sites)
                if (string.Compare(site.Host, oldSiteHost, true) == 0)
                {
                    site.CloneVirtualNewspapersSites(newSite);
                    break;
                }
        }

        public void DoSomething()
        {

        }

        public bool IsLoadingCompleled()
        {
            lock (_loadingLocker)
                return isLoadingCompleted;
        }

        #endregion
    }
}
