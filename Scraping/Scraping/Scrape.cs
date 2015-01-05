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
using Utilities;
using DataTypes;
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Data.SqlClient;
using HtmlAgilityPack;

namespace Scraping
{
    public sealed class Scrape
    {
        #region variables

        private static Scrape _instance = null;
        //for display UI                        
        public static List<TextBox> tbList = new List<TextBox>();
        
        //for variables
        public Window _currentWindow; // the window in which contains this action
        private int _finishWorker; // number of workers have finished its job.
        private int _runningWorker; // number of workers are still running        
        private static readonly object _synclocker = new object(); //using for worker
        private static readonly object _synclocker2 = new object(); //using for worker
        private static readonly object _synclocker3 = new object(); //using for worker
        private static readonly object _synclocker4 = new object(); //using for stopping all threads
        private static readonly object _statisticslocker = new object(); //using for statistics
        private static readonly object _randomlocker = new object(); //using for random
        private static readonly object _finishlocker = new object(); //using for statistics
        public ManualResetEvent _signal = new ManualResetEvent(false);
        public ManualResetEvent _saveSignal = new ManualResetEvent(false);
        private static readonly object _instancelocker = new object(); //using for instance
        //private static SemaphoreSlim _slim = new SemaphoreSlim(10);
        private static string DateTimeFormat = "dd-MM HH:mm:ss";
        private static string _connString = "Data Source=MPV17035;Initial Catalog=MockData;Integrated Security=True";
        //private static string _connString = "Data Source=KUPO-PC\\SQLEXPRESS;Initial Catalog=MockData;Integrated Security=True";
        private string _linkThresholdCommand = "WITH OrderedLinks AS ( SELECT *, ROW_NUMBER() OVER (ORDER BY Publish) AS 'RowNumber' FROM MockData.dbo.Records) SELECT * FROM OrderedLinks WHERE RowNumber > @floor AND RowNumber <= @celling";        //for counter variables
        //private static string _insertCommand = "INSERT INTO MockData.dbo.Records(Title,URL,Image,Publish,Body,Shampoo) OUTPUT INSERTED.Id VALUES(@Title,@URL,@Image,@Publish,@Body,@Shampoo)";
        private static string _insertCommand = "INSERT INTO MockData.dbo.Records(Title,URL,Image,Publish,Body,Shampoo,HtmlBody,SEOTitle,SEODescription) VALUES(@Title,@URL,@Image,@Publish,@Body,@Shampoo,@HtmlBody,@SEOTitle,@SEODescription)";
        private int _queueLinks; //number of links has been queued
        private int _successArticle; // number of success download & parsing 
        private int _failedArticle; // number of failed download & parsing
        private int _savedArticle;
        private const int _MINDELAYINTERVAL = 25000; //minimum delay interval (miliseconds)
        private const int _MAXDELAYINTERVAL = 45000; //maximum delay interval (miliseconds)
        private const int _MINDELAYINTERVAL1 = 35000; //minimum delay interval (miliseconds)
        private const int _MAXDELAYINTERVAL1 = 70000; //maximum delay interval (miliseconds)
        private const int _RANDOMDELAYINTERVAL = 15; //maximum delay interval (miliseconds)
        private int _threshold = 20000;
        private static List<Record>[] _linksThreshold = new List<Record>[50];
        private string _folderPath;
        private Queue<HarvestLink> _harvestURLs = new Queue<HarvestLink>();
        private Queue<string> _sourceLinks = new Queue<string>();
        private Queue<Article> _needSaved = new Queue<Article>();
        public List<string> _keywords = new List<string>();
        public string _rawKeywords;
        public BackgroundWorker _prepareWorker = new BackgroundWorker();
        private Thread[] _harvestWorker = new Thread[SysSettings.Instance.Article_Scraping_Threads];
        private Thread[] _downloadWorker = new Thread[SysSettings.Instance.Article_Scraping_Threads];
        private Thread _updateWorker;

        private static DateTime _begin;
        private static DateTime _end;
        private static List<DateTime> _range = new List<DateTime>();
        //public List<BackgroundWorker> _worker = new List<BackgroundWorker>();
        //public BackgroundWorker _watcher = new BackgroundWorker();
        private bool _readyToSave = false;
        private bool _stop = false;
        //for action
        //public Action<Window, TextBox, int> updateStatistics;        
        public Action<string, Window, TextBox> updateLiveStatus;

        //for state
        public ScrapeState _state;

        #endregion

        #region Constructors

        Scrape()
        {
            _currentWindow = new Window();
            _finishWorker = 0;
            _runningWorker = 0;            
            _queueLinks = 0;
            _successArticle = 0;
            _failedArticle = 0;
            _savedArticle = 0;
            _signal.Set();
            _saveSignal.Reset();
            _folderPath = "";            

            for (int i = 0; i < 50; i++)
            {
                _linksThreshold[i] = new List<Record>();
            }

            _state = ScrapeState.isInitialize;
        }

        public static Scrape Instance
        {
            get
            {
                lock (_instancelocker)
                {
                    if (_instance == null)
                    {
                        _instance = new Scrape();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Phase 0: preparation

        /// <summary>
        /// Prepare for scraping: Load scraped urls from database
        /// </summary>
        private void LoadExistedURLs()
        {
            do
            {
                bool result = false;
                RecordHandler.Instance.ConsumeHandler(this, ref result);
                if (!result)
                    Thread.Sleep(500);
                else
                {                    
                    HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Begin loading Existed URLs!"));
                    try
                    {
                        int NumOfRecord = RecordHandler.Instance._records.Count;
                        if (NumOfRecord >= _threshold * 5)
                        {
                            int floor = 0;
                            int celling = _threshold;
                            for (int i = 0; i < SysSettings.Instance.Article_Scraping_Threads; i++)
                            {
                                floor = i * _threshold;
                                celling = (i + 1) * _threshold;

                                for (int j = floor; j < celling && j < RecordHandler.Instance._records.Count; j++)
                                {
                                    _linksThreshold[i].Add(RecordHandler.Instance._records[j]);
                                }
                            }
                        }
                        else if (NumOfRecord > 0 && NumOfRecord < _threshold * 5)
                        {
                            foreach (var i in RecordHandler.Instance._records)
                                _linksThreshold[0].Add(i);
                        }
                        else
                        {
                            //do nothing
                        }
                        HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Loading Existed URLs successfully! Number of records is " + NumOfRecord));
                    }
                    catch (Exception ex)
                    {
                        HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Error while loading existed URLs! Message: " + ex.Message));
                    }
                    finally
                    {
                        RecordHandler.Instance.ReleaseHandler(this, ref result);
                    }
                    break;                   
                }
            } while (true);

            //do
            //{
            //    bool result = false;
            //    SQLHandler.Instance.ConsumeHandler(this, ref result);
            //    if (!result)
            //        Thread.Sleep(500);
            //    else
            //    {
            //        Tuple<bool, SqlConnection> sqlHandle = SQLHandler.Instance.GetDBConnection(this);
            //        if (sqlHandle.Item1)
            //        {
            //            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Begin loading Existed URLs!"));
            //            try
            //            {
            //                SqlCommand cmd = sqlHandle.Item2.CreateCommand();
            //                cmd.CommandText = "SELECT COUNT(*) FROM Records";
            //                int DataRows = (int)cmd.ExecuteScalar();
            //                SqlDataReader reader;

            //                if (DataRows >= _threshold * 5)
            //                {
            //                    cmd.CommandText = _linkThresholdCommand;
            //                    cmd.Parameters.AddWithValue("@floor", 0);
            //                    cmd.Parameters.AddWithValue("@celling", 0);

            //                    for (int i = 0; i < SysSettings.Instance.Article_Scraping_Threads; i++)
            //                    {
            //                        cmd.Parameters[0].Value = i * _threshold;
            //                        cmd.Parameters[1].Value = (i + 1) * _threshold;

            //                        reader = cmd.ExecuteReader();
            //                        while (reader.Read())
            //                        {
            //                            _linksThreshold[i].Add(reader["URL"].ToString());
            //                        }
            //                        reader.Close();
            //                    }
            //                }
            //                else if (DataRows > 0 && DataRows < _threshold * 5)
            //                {
            //                    cmd.CommandText = "SELECT * FROM Records";
            //                    reader = cmd.ExecuteReader();

            //                    while (reader.Read())
            //                    {
            //                        _linksThreshold[0].Add(reader["URL"].ToString());
            //                    }
            //                    reader.Close();
            //                }
            //                else
            //                {
            //                    //do nothing
            //                }
            //                HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Loading Existed URLs successfully! Records " + DataRows));                            
            //            }
            //            catch (Exception ex)
            //            {
            //                HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Error while loading existed URLs! Message: " + ex.Message));
            //            }
            //            finally
            //            {                            
            //                SQLHandler.Instance.ReleaseHandler(this, ref result);                            
            //            }
            //            break;
            //        }
            //        else
            //            Thread.Sleep(500);
            //    }
            //} while (true);

            //try
            //{
            //    myConnection.Open();
            //    SqlCommand cmd = myConnection.CreateCommand();
            //    cmd.CommandText = "SELECT COUNT(*) FROM Records";
            //    int DataRows = (int)cmd.ExecuteScalar();
            //    SqlDataReader reader;
            //    if (DataRows >= _threshold*5)
            //    {
            //        cmd.CommandText = _linkThresholdCommand;
            //        cmd.Parameters.AddWithValue("@floor", 0);
            //        cmd.Parameters.AddWithValue("@celling", 0);

            //        for (int i = 0; i < SysSettings.Instance.Article_Scraping_Threads; i++)
            //        {
            //            cmd.Parameters[0].Value = i * _threshold;
            //            cmd.Parameters[1].Value = (i + 1) * _threshold;

            //            reader = cmd.ExecuteReader();                        
            //            while (reader.Read())
            //            {
            //                _linksThreshold[i].Add(reader["URL"].ToString());
            //            }
            //            reader.Close();
            //        }
            //    }
            //    else if (DataRows > 0 && DataRows < _threshold*5)
            //    {
            //        cmd.CommandText = "SELECT * FROM Records";
            //        reader = cmd.ExecuteReader();
                    
            //        while (reader.Read())
            //        {
            //            _linksThreshold[0].Add(reader["URL"].ToString());
            //        }
            //        reader.Close();
            //    }
            //    else
            //    {
            //        //do nothing
            //    }
            //    HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Loading Existed URLs successfully! Records " + DataRows));
            //}
            //catch (Exception ex)
            //{
            //    //show message
            //    HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Error while loading existed URLs! Message: " + ex.Message));
            //}
            //finally
            //{                
            //    myConnection.Close();
            //}
        }

        /// <summary>
        /// Prepare for scraping: Using spawner factory to create search pages for new urls
        /// </summary>
        public void PrepareLinks()
        {
            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Prepare source URLs!"));
            
            switch(SysSettings.Instance.Article_ScrapingMode)
            {
                case ScrapeMode.ByKeywords:
                    string[] spliter = _rawKeywords.ToLower().Split(',');

                    foreach (var str in spliter)
                    {
                        string temp = str.Trim();
                        if (!_keywords.Contains(temp))
                            _keywords.Add(temp);
                    }

                    foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                    {
                        foreach (var word in _keywords)
                        {
                            for (int i = 1; i <= SysSettings.Instance.Article_Scraping_MaxDepth; i++)
                            {
                                _sourceLinks.Enqueue(Utilities.Helper.CreateSearchPageByKeywords(source, word, i));
                            }
                        }
                    }
                    HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Prepare source URLs successfully!"));

                    foreach (var link in _sourceLinks)
                    {
                        HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, link));
                    }
                    break;
                case ScrapeMode.ByCategories:
                    foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                    {
                        if (source.Choosen)
                        {
                            ISpawning spawner = SpawnerFactory.CreateSpawner(source);
                            List<string> temp = spawner.CreateSearchPages(source, SysSettings.Instance.Article_Scraping_MaxDepth);
                            foreach (string str in temp)
                                _sourceLinks.Enqueue(str);
                        }
                    }

                    HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Prepare source URLs successfully!"));

                    foreach (var link in _sourceLinks)
                    {
                        HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, link));
                    }
                    break;
            };
        }

        /// <summary>
        /// Phase 0 jobs: prepare all materials
        /// </summary>              
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _begin = DateTime.Now;
            LoadExistedURLs();
            PrepareLinks();
        }

        /// <summary>
        /// Handler worker RunCompleted event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Prepare completed! Move to phase 1 - Collect article urls!"));
            TransitionPhase0ToPhase1();
        }

        /// <summary>
        /// Transition from phase 1 to phase 2: Prepare harvestWorker & run it
        /// </summary>
        private void TransitionPhase0ToPhase1()
        {
            _finishWorker = 0;
            _runningWorker = 0;

            for (int i = 0; i < SysSettings.Instance.Article_Scraping_Threads; i++)
            {
                int index = i;
                _harvestWorker[i] = new Thread(CollectLinks);
                _harvestWorker[i].IsBackground = true;
                _harvestWorker[i].Start(index);
            }
        }

        #endregion

        #region Phase 1: collect links

        /// <summary>
        /// Phase 1 jobs: collect all possible articles links and filter these ones.
        /// </summary>        
        private void CollectLinks(object index)
        {
            int Index = (int)index;
            lock (_statisticslocker)
            {
                _runningWorker++;
                HandleScrapeStatistics.Instance.UpdateTextBox("tbThreads", _runningWorker);
            }
            do
            {
                lock (_synclocker4)
                {
                    if (_stop)
                        goto StopThread;
                }
                _signal.WaitOne();
                string link = "";
                int retry = 0;
                lock (_synclocker)
                {
                    if (_sourceLinks.Count > 0)
                        link = _sourceLinks.Dequeue();
                    else
                        break;
                }
                if (link != string.Empty)
                {
                    string domain = "http://" + new Uri(link).Host;
                    while (retry < 2)
                    {
                        _signal.WaitOne();
                        try
                        {
                            HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;
                            int pos = 0;
                            if (ProxyHandler.Instance.ScrapeHarvestLinks != ProxyType.Disabled)
                            {
                                WebProxy p;
                                do
                                {
                                    bool result = false;
                                    ProxyHandler.Instance.ConsumeHandler(this, "Harvest|Thread[" + Index + "]", ref result);
                                    if (!result)
                                        Thread.Sleep(150);
                                    else
                                    {
                                        p = ProxyHandler.Instance.GetRandomProxy(this, "Harvest|Thread[" + Index + "]", ProxyHandler.Instance.ScrapeHarvestLinks);
                                        ProxyHandler.Instance.ReleaseHandler(this, "Harvest|Thread[" + Index + "]", ref result);
                                        break;
                                    }

                                } while (true);

                                //prepare the request
                                request.Proxy = p;
                            }
                            request.Timeout = 60000;
                            request.KeepAlive = false;
                            request.ProtocolVersion = HttpVersion.Version10;
                            request.Method = "GET";
                            request.Accept = "text/html,application/xhtml+xml,application/xml";
                            request.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                            request.ServicePoint.ConnectionLimit = 1;
                            //request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

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
                                        try
                                        {
                                            document.Load(reader.BaseStream, encoding);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    request.Abort();
                                }
                            }

                            IHarvest harvester = HarvesterFactory.CreateHarvester(link);
                            int page = harvester.PageNumber(link);
                            List<HarvestLink> harvestedLinks = harvester.HarvestLinks(document, link, page);

                            if (harvestedLinks.Count > 0)
                            {
                                foreach (var lnk in harvestedLinks)
                                {
                                    string needChecked = "";
                                    if (!lnk.articleUrl.Contains("http://"))
                                        needChecked = domain + lnk.articleUrl;
                                    else
                                        needChecked = lnk.articleUrl;

                                    if (!IsExisted(needChecked) && !IsViolate(needChecked))
                                    {
                                        lock (_synclocker2)
                                        {
                                            _queueLinks++;
                                            _harvestURLs.Enqueue(lnk);
                                            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Thread[" + index + "]: Add " + needChecked + " to queue."));
                                            HandleScrapeStatistics.Instance.UpdateTextBox("tbQueue", _queueLinks);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                        catch (WebException ex)
                        {
                            retry++;
                            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Thread[" + index + "]: Attempt failed. Try again. Message: " + ex.Message));                            
                            Thread.Sleep(_RANDOMDELAYINTERVAL);
                            Random retryRand = new Random();
                            Thread.Sleep(retryRand.Next(_MINDELAYINTERVAL, _MAXDELAYINTERVAL));
                        }
                    }
                }
                Thread.Sleep(_RANDOMDELAYINTERVAL);
                Random nextTurn = new Random();
                Thread.Sleep(nextTurn.Next(_MINDELAYINTERVAL, _MAXDELAYINTERVAL));
            } while (true);
            StopThread: if (_stop) Stop();
            if (!_stop) Completed_CollectLinks();
        }

        /// <summary>
        /// Handler worker completed working
        /// </summary>
        private void Completed_CollectLinks()
        {
            lock (_finishlocker)
            {
                _finishWorker++;
                _runningWorker--;
                HandleScrapeStatistics.Instance.UpdateTextBox("tbThreads", _runningWorker);
            }
            if (_finishWorker == SysSettings.Instance.Article_Scraping_Threads)
            {
                HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Harvested completed! Move to phase 2 - Download article urls!"));
                TransitionPhase1ToPhase2();
            }
        }

        /// <summary>
        /// Check if current is existed whether or not 
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private static bool IsExisted(string link)
        {
            for (int i = 0; i < 50; i++)
            {
                if (_linksThreshold[i].Count > 0)
                {
                    foreach (var recd in _linksThreshold[i])
                    {
                        if (recd.url.Contains(link))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if current link is violated with rules whether or not
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static bool IsViolate(string url)
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

        private bool IsExisted2(List<HarvestLink> list, string link)
        {
            return list.Any(a => a.articleUrl == link);
        }

        /// <summary>
        /// Clean all of harvested URLs
        /// </summary>
        private void CleanHarvestedURLs()
        {
            List<HarvestLink> temp = new List<HarvestLink>();
            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Begin cleaning harvested URLs!"));
            while (_harvestURLs.Count > 0)
            {
                var url = _harvestURLs.Dequeue();

                if (!IsExisted2(temp, url.articleUrl))
                    temp.Add(url);
            }

            foreach (var i in temp)
            {
                _harvestURLs.Enqueue(i);
            }
            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Clean harvested URLs completed!"));
            HandleScrapeStatistics.Instance.UpdateTextBox("tbQueue", _harvestURLs.Count);
        }

        /// <summary>
        /// Transition from phase 1 to phase 2: download article and stored it in database & files
        /// </summary>
        private void TransitionPhase1ToPhase2()
        {
            CleanHarvestedURLs();

            _updateWorker = new Thread(UpdateRecord);
            _updateWorker.IsBackground = true;
            _updateWorker.Start();

            for (int i = 0; i < SysSettings.Instance.Article_Scraping_Threads; i++)
            {
                int index = i;
                _downloadWorker[i] = new Thread(DownloadArticle);
                _downloadWorker[i].IsBackground = true;
                _downloadWorker[i].Start(index);
            }
        }

        #endregion

        #region Phase 2.1: Download article

        /// <summary>
        /// Phase 2 jobs: download all article links.
        /// </summary>
        private void DownloadArticle(object Index)
        {
            int index = (int)Index;
            lock (_statisticslocker)
            {
                _runningWorker++;
                HandleScrapeStatistics.Instance.UpdateTextBox("tbThreads", _runningWorker);
            }

            int count = 1;

            do
            {                
                lock (_synclocker4)
                {
                    if (_stop)
                        goto StopThread;
                }
                _signal.WaitOne();
                HarvestLink link = new HarvestLink(string.Empty, string.Empty);
                int retry = 0;
                lock (_synclocker)
                {
                    if (_harvestURLs.Count > 0)
                        link = _harvestURLs.Dequeue();
                    else
                        break;
                }
                if (link.articleUrl != string.Empty && link.harvestUrl != string.Empty)
                {
                    bool IsDownload = false;
                    while (retry < 2)
                    {
                        _signal.WaitOne();
                        try
                        {
                            HttpWebRequest request = WebRequest.Create(link.articleUrl) as HttpWebRequest;
                            int pos = 0;

                            if (ProxyHandler.Instance.ScrapeDownloadArticles != ProxyType.Disabled)
                            {
                                WebProxy p;
                                do
                                {
                                    bool result = false;
                                    ProxyHandler.Instance.ConsumeHandler(this, "Download|Thread[" + Index + "]", ref result);
                                    if (!result)
                                        Thread.Sleep(150);
                                    else
                                    {
                                        p = ProxyHandler.Instance.GetRandomProxy(this, "Download|Thread[" + Index + "]", ProxyHandler.Instance.ScrapeDownloadArticles);
                                        ProxyHandler.Instance.ReleaseHandler(this, "Download|Thread[" + Index + "]", ref result);
                                        break;
                                    }

                                } while (true);
                                request.Proxy = p;
                            }

                            request.Timeout = 60000;
                            request.KeepAlive = false;
                            request.ProtocolVersion = HttpVersion.Version10;
                            request.Method = "GET";
                            request.Accept = "text/html,application/xhtml+xml,application/xml";
                            request.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                            //request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

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
                            Article needDownload = new Article(link.articleUrl, link.harvestUrl, document, GetArticleType(link.articleUrl));
                            IScraper scraper = ScraperFactory.CreateScraper(needDownload);

                            needDownload.title = scraper.GetTitleFor(needDownload.doc, needDownload.type, needDownload.url);
                            needDownload.publish = scraper.GetPublish(needDownload.doc, needDownload.type);
                            needDownload.shampoo = scraper.GetShampoo(needDownload.doc, needDownload.type);
                            needDownload.image = scraper.GetImage(needDownload.doc, needDownload.type, needDownload.url);
                            needDownload.body = scraper.GetBody(needDownload.doc, needDownload.type, needDownload.url);
                            needDownload.htmlBody = scraper.GetHtmlBody(needDownload.doc, needDownload.type, needDownload.url);
                            needDownload.SEOTitle = scraper.GetSEOTitle(needDownload.doc, GetArticleType(link.articleUrl));
                            needDownload.SEODescription = scraper.GetSEODescription(needDownload.doc, needDownload.type);
                            needDownload.Tags = scraper.GetTags(needDownload.doc, needDownload.type);
                            needDownload.downloaded = DateTime.Now;

                            //remove all black words
                            //needDownload.body = SysSettings.Instance.Article_Scraping_FilterWords.Replace(needDownload.body);
                            //needDownload.htmlBody = SysSettings.Instance.Article_Scraping_FilterWords.Replace(needDownload.htmlBody);
                            //needDownload.shampoo = SysSettings.Instance.Article_Scraping_FilterWords.Replace(needDownload.shampoo);
                            //needDownload.SEOTitle = SysSettings.Instance.Article_Scraping_FilterWords.Replace(needDownload.SEOTitle);
                            //needDownload.SEODescription = SysSettings.Instance.Article_Scraping_FilterWords.Replace(needDownload.SEODescription);

                            lock (_synclocker2)
                            {
                                if (!_readyToSave)
                                {
                                    _readyToSave = true;
                                    _saveSignal.Set();
                                }
                                _needSaved.Enqueue(needDownload);
                                _successArticle++;
                                HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Thread[" + index + "]: Enqueued " + needDownload.url + " successfully!"));
                                HandleScrapeStatistics.Instance.UpdateTextBox("tbSuccess", _successArticle);
                            }
                            IsDownload = true;
                            break;
                        }
                        catch (WebException ex)
                        {
                            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Thread[" + index + "]: Attempt failed. Try again. Message: " + ex.Message));
                            retry++;
                            Thread.Sleep(_RANDOMDELAYINTERVAL);
                            Random retryRand = new Random();
                            Thread.Sleep(retryRand.Next(_MINDELAYINTERVAL1, _MAXDELAYINTERVAL1));
                        }
                    }
                    if (!IsDownload)
                    {
                        count++;
                        lock (_synclocker3)
                        {
                            _failedArticle++;
                            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Thread[" + index + "]: Download " + link + " failed!"));
                            HandleScrapeStatistics.Instance.UpdateTextBox("tbFailed", _failedArticle);
                        }
                    }
                }
                count++;
                Thread.Sleep(_RANDOMDELAYINTERVAL);
                Random nextTurn = new Random();
                Thread.Sleep(nextTurn.Next(_MINDELAYINTERVAL1, _MAXDELAYINTERVAL1));
            }
            while (count == 1);
            StopThread: if (_stop) Stop();
            if (!_stop) Completed_DownloadArticle();
        }

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

        private void Completed_DownloadArticle()
        {
            lock (_finishlocker)
            {
                _finishWorker++;
                _runningWorker--;
                HandleScrapeStatistics.Instance.UpdateTextBox("tbThreads", _runningWorker);
            }
            if (_runningWorker == 0)
            {
                _end = DateTime.Now;
                HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Download completed!!!"));

                //Test problem
                //foreach (var i in _needSaved)
                //{
                //    HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Article " + i.url + " need to be saved!"));    
                //}

                lock (_synclocker2)
                {
                    Article stop = new Article("stop-save", "stop-harvest", new HtmlDocument(), ArticleType.NewsArticle);
                    _needSaved.Enqueue(stop);
                }
                //TransitionPhase2ToPhase3();
            }
        }

        private void TransitionPhase2ToPhase3()
        {
            CreateRootFolder();
            _updateWorker = new Thread(UpdateRecord);
            _updateWorker.IsBackground = true;
            _updateWorker.Start();
        }

        #endregion

        #region Phase 2.2: Update Record

        /// <summary>
        /// Update record
        /// </summary>
        private void UpdateRecord()
        {
            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Beginning update record!"));

            while (true)
            {
                lock (_synclocker4)
                {
                    if (_stop)
                        goto StopThread;
                }
                _saveSignal.WaitOne();
                Article needSave = new Article();
                lock (_synclocker2)
                {
                    if (_needSaved.Count > 0)
                        needSave = _needSaved.Dequeue();
                }
                if (needSave.url == "stop-save")
                    break;
                else
                {
                    if (needSave.url != string.Empty)
                    {
                        do
                        {
                            bool result = false;
                            RecordHandler.Instance.ConsumeHandler(this, ref result);
                            if (!result)
                                Thread.Sleep(50);
                            else
                            {
                                try
                                {
                                    //write file
                                    //string parentFolder = _folderPath + "\\" + new Uri(needSave.url).Host;
                                    //if (!Utilities.Helper.CheckExistFolder(parentFolder))
                                    //    Directory.CreateDirectory(parentFolder);

                                    //string filePath = parentFolder + "\\" + Utilities.Helper.convertToUnSign(needSave.title) + ".txt";
                                    string filePath = "";
                                    string fileName = "";
                                    //CreateFilePath(needSave.harvestUrl, needSave.url, ref filePath, ref fileName);

                                    IPathCreater pathCreater = PathCreaterFactory.GetPathCreater(needSave.harvestUrl);
                                    pathCreater.CreateFilePath(needSave.harvestUrl, needSave.url, SysSettings.Instance.Article_Scraping_SavingFolder, SysSettings.Instance.Article_Scraping_ScrapingSources, ref filePath, ref fileName);

                                    //test
                                    //HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Create path " + filePath + " successfully!"));

                                    using (StreamWriter writer = new StreamWriter(filePath + "\\" + fileName))
                                    {
                                        needSave.doc.Save(writer);
                                    }
                                    //insert to record
                                    RecordHandler.Instance.AddNewRecords(this, Helper.ConvertFromArticle(needSave, filePath, fileName));
                                    //_successArticle--;
                                    //HandleScrapeStatistics.Instance.UpdateTextBox("tbSuccess", _successArticle);
                                    _savedArticle++;
                                    HandleScrapeStatistics.Instance.UpdateTextBox("tbSaved", _savedArticle);
                                    HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Save " + needSave.url + " successfully!"));
                                }
                                catch (Exception ex)
                                {
                                    //_successArticle--;
                                    //HandleScrapeStatistics.Instance.UpdateTextBox("tbSuccess", _successArticle);
                                    HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Save " + needSave.url + " failed! Message: " + ex.Message));
                                }
                                finally
                                {
                                    RecordHandler.Instance.ReleaseHandler(this, ref result);
                                }
                                Thread.Sleep(150);
                                break;
                            }
                        } while (true);
                    }
                    else
                        Thread.Sleep(150);
                }                
            }
            StopThread: if (_stop) Stop();
            if (!_stop) Completed_UpdateRecord();            
        }

        private void Completed_UpdateRecord()
        {
            HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "Update record completed! All jobs have been finished!"));
            _updateWorker.Abort();

            _state = ScrapeState.isDone;
            SysSettings.Instance.UpdateScrapeState(_state);
        }

        #endregion
  
        #region Setup & control functions

        public void SetupScrape(Window wd, Action<Window, TextBox, int> UpdateStatistics, Action<string, Window, TextBox> UpdateLiveStatus, string rawKeywords)
        {
            _rawKeywords = rawKeywords;
            if (_state == ScrapeState.isInitialize)
            {
                //load handle
                HandleMessages.Instance.Format = DateTimeFormat;
                HandleMessages.Instance.SetupHandle(_currentWindow, FindTextBox("tbLiveStatus"), 20);

                HandleScrapeStatistics.Instance.SetupHandle(_currentWindow, UpdateStatistics, FindTextBox("tbQueue"), FindTextBox("tbSuccess"), FindTextBox("tbFailed"), FindTextBox("tbThreads"), FindTextBox("tbSaved"));

                //load action
                //updateStatistics = UpdateStatistics;
                updateLiveStatus = UpdateLiveStatus;

                //load window
                _currentWindow = wd;

                //load keywords                
                _state = ScrapeState.isReady;
                SysSettings.Instance.UpdateScrapeState(_state);
            }            
        }

        public void RunScrape()
        {
            if (_state == ScrapeState.isReady)
            {
                //start message handle
                HandleMessages.Instance.RunHandle(updateLiveStatus);

                // prepare first working phase                
                _prepareWorker.DoWork += new DoWorkEventHandler(worker_DoWork);
                _prepareWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                _prepareWorker.RunWorkerAsync();
                //change state
                _state = ScrapeState.isRunning;
                SysSettings.Instance.UpdateScrapeState(_state);
            }
        }

        public void PauseScrape()
        {
            if (_state == ScrapeState.isRunning)
            {
                _signal.Reset();
                _state = ScrapeState.isPaused;
                SysSettings.Instance.UpdateScrapeState(_state);
            }
        }

        public void ResumeScrape()
        {
            if (_state == ScrapeState.isPaused)
            {
                _signal.Set();
                _state = ScrapeState.isRunning;
                SysSettings.Instance.UpdateScrapeState(_state);
            }
        }

        public void StopScrape()
        {
            lock(_synclocker4)
                _stop = true;
        }

        private void Stop()
        {
            lock (_finishlocker)
            {
                _finishWorker++;
                _runningWorker--;
                HandleScrapeStatistics.Instance.UpdateTextBox("tbThreads", _runningWorker);
            }
            if (_finishWorker == SysSettings.Instance.Article_Scraping_Threads)
            {
                HandleMessages.Instance.AddMessages(Utilities.Helper.CreateMessage(DateTimeFormat, "All threads has been stopped!!!"));
                _state = ScrapeState.isDone;
                SysSettings.Instance.UpdateScrapeState(_state);
            }
        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Utility functions
        /// </summary>
        private TextBox FindTextBox(string name)
        {
            foreach (var tb in tbList)
            {
                if (string.Compare(tb.Name, name, true) == 0)
                {
                    return tb;
                }
            }
            return null;
        }                

        /// <summary>
        /// Add textbox to list
        /// </summary>
        /// <param name="tb"></param>
        public void AddTextBox(TextBox tb)
        {
            if (!tbList.Contains(tb))
                tbList.Add(tb);
        }

        /// <summary>
        /// Create Root Folder
        /// </summary>
        private void CreateRootFolder()
        {
            CalculateDateRange();

            foreach (var src in SysSettings.Instance.Article_Scraping_ScrapingSources)
            {
                string dir = SysSettings.Instance.Article_Scraping_SavingFolder + "\\" + src.Title;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                bool isLeaf = true;
                ReverseCategories(ref isLeaf, dir, null, src);
            }
        }

        private void ReverseCategories(ref bool isLeaf, string pattern, SourceCategory parent, ArticleSource source)
        {
            string value = pattern;
            foreach (var cate in source.Categories)
            {
                if (cate.ParentCategory == parent)
                {
                    isLeaf = false;
                    bool myLeaf = true;
                    value = pattern + "\\" + cate.Slug;
                    if (!Directory.Exists(value))
                        Directory.CreateDirectory(value);
                    ReverseCategories(ref myLeaf, value, cate, source);
                }
            }
            if (isLeaf)
            {
                foreach (var date in _range)
                {
                    string dateFolder = string.Format("{0}-{1}-{2}", date.Month, date.Day, date.Year);
                    if (!Directory.Exists(value + "\\" + dateFolder))
                        Directory.CreateDirectory(value + "\\" + dateFolder);
                }
            }
        }

        private void CalculateDateRange()
        {
            TimeSpan span = _end - _begin;
            for (int day = 0; day <= span.Days; day++)
            {
                _range.Add(_begin.AddDays(day));
            }
        }

        private void CreateFilePath(string hUrl, string aUrl, ref string filePath, ref string fileName)
        {
            string hLink = hUrl;

            Uri tmp = new Uri(hLink);
            string host = tmp.Scheme + Uri.SchemeDelimiter + tmp.Host + "/";
            hLink = hLink.Replace(host, "").Trim();
            string[] content = hLink.Split('/');

            if (content.Last() == string.Empty)
                content = content.Take(content.Length - 1).ToArray();

            string[] dirs = Directory.GetDirectories(SysSettings.Instance.Article_Scraping_SavingFolder + "\\");
            foreach (var site in dirs)
            {
                if (host.Contains(new DirectoryInfo(site).Name))
                {
                    filePath += site;
                    break;
                }
            }

            for (int i = 0; i < content.Count() - 1; i++)
            {
                if (CheckSlugIsExisted(tmp.Host, content[i]) != null)
                    filePath += "\\" + CheckSlugIsExisted(tmp.Host, content[i]);
            }

            filePath = filePath + "\\" + string.Format("{0}-{1}-{2}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year);

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            fileName = aUrl.Split('/').Last();

            //if (!fileName.Contains(".html") && !fileName.Contains(".aspx") && !fileName.Contains(".htm"))
            //{
            //    fileName += ".html";
            //}

            if (!fileName.Contains('.'))
            {
                fileName += ".html";
            }
            else
            {
                if (!fileName.Contains(".html") && !fileName.Contains(".aspx") && !fileName.Contains(".htm"))
                {
                    string[] content1 = fileName.Split('.');
                    fileName = content1[0] + ".html";
                }
            }
        }

        private static string CheckSlugIsExisted(string host, string path)
        {
            foreach (var src in SysSettings.Instance.Article_Scraping_ScrapingSources)
            {
                if (host.Contains(src.Title))
                {
                    foreach (var cate in src.Categories)
                    {
                        if (cate.Slug.ToLower().Contains(path.ToLower()))
                            return cate.Slug;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
