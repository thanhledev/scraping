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
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Data.SqlClient;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using JoeBlogs;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;

namespace Scraping
{
    public sealed class AutoPost
    {
        /// <summary>
        /// All variable & their properties
        /// </summary>
        #region variables

        //for own variables
        private static AutoPost _instance = null;
        private static readonly object _instancelocker = new object(); //using for instance        
        public Queue<string> messages;
        private TestType _testType;
        private const long _GIGABYTE = 1000000000;
        //for collaboration between main window
        Window _currentWD;        
        Action<Window, Label, bool> actionGoogleDriveTestConnection;        
        Action<Window, List<System.Windows.Controls.DockPanel>> actionClearProjectSelectedPostSite;
        Action<Window, List<System.Windows.Controls.DockPanel>, string, bool> actionDisplayProjectSelectedPostSites;
        Action<Window, ProgressBar, double?> actionUpdateGoogleDriveUsage;
        private List<DockPanel> _site_dockPanel;

        public List<DockPanel> Site_DockPanel
        {
            get { return _site_dockPanel; }
            set { _site_dockPanel = value; }
        }

        private Label _gdrive_ConnectionStatus;
        private ProgressBar _gdrive_UsageBar;
        //for testing connection
        BackgroundWorker _connectWorker = new BackgroundWorker();

        //for working state
        private bool _isGDriveConnectPass;

        //for state
        public PostingState _state;

        //for projects
        private PostingProject _currentProject;

        public PostingProject CurrentProject
        {
            get { return _currentProject; }
            set { _currentProject = value; }
        }        
        
        //for autoPostMessage
        private static readonly object _messageLocker = new object();
        private Thread _messageWorker;
        public ManualResetEvent _signal = new ManualResetEvent(false);

        private TextBox _messageTB;

        public TextBox MessageTB
        {
            get { return _messageTB; }
            set { _messageTB = value; }
        }

        //for using variables
        private Queue<string> _messages = new Queue<string>();
        public string Format { get; set; }
        private int _delay = 20;
        Action<Window, TextBox, string> _updateTextBox;

        #endregion

        /// <summary>
        /// All static method for setup instance
        /// </summary> 
        #region StaticSetup

        AutoPost()
        {
            _isGDriveConnectPass = false;
            _state = PostingState.isInitialize;
            _currentWD = new Window();
            _site_dockPanel = new List<DockPanel>();
            _gdrive_ConnectionStatus = new Label();
            _gdrive_UsageBar = new ProgressBar();
            _connectWorker.DoWork += new DoWorkEventHandler(worker_DoWork);
            _connectWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            messages = new Queue<string>();
        }

        public static AutoPost Instance
        {
            get
            {
                lock (_instancelocker)
                {
                    if (_instance == null)
                    {
                        _instance = new AutoPost();
                    }
                }
                return _instance;
            }
        }

        public void SetupWindow(Window wd)
        {
            _currentWD = wd;
        }

        //setup controls
        public void SetupGoogleDriveTestConnectionLabel(Label lbl)
        {
            _gdrive_ConnectionStatus = lbl;
        }

        public void SetupGoogleDriveUsageBar(ProgressBar prg)
        {
            _gdrive_UsageBar = prg;
        }

        public void SetupGoogleDriveTestConnectionAction(Action<Window, Label, bool> action)
        {
            actionGoogleDriveTestConnection = action;
        }
        
        public void SetupClearProjectSelectedPostSiteAction(Action<Window, List<System.Windows.Controls.DockPanel>> action)
        {
            actionClearProjectSelectedPostSite = action;
        }

        public void SetupDisplayProjectSelectedPostSitesAction(Action<Window, List<System.Windows.Controls.DockPanel>, string, bool> action)
        {
            actionDisplayProjectSelectedPostSites = action;
        }

        public void SetupUpdateGoogleDriveUsageAction(Action<Window, ProgressBar, double?> action)
        {
            actionUpdateGoogleDriveUsage = action;
        }

        #endregion

        #region DynamicSetup

        private void ReleasePostSite()
        {
            _site_dockPanel.Clear();
        }

        public void SetupAutoPostMessage(TextBox tb, Action<Window, TextBox, string> updateTextBox,int delay)
        {
            _messageTB = tb;
            _updateTextBox = updateTextBox;
            _delay = delay;            
            _messageWorker = new Thread(printMessage);
            _messageWorker.IsBackground = true;
            _signal.Set();
        }

        #endregion
                
        /// <summary>
        /// Worker doWork & WorkComplete
        /// </summary>
        #region TestConnectWorker
        
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {                       
            switch(_testType)
            {
                case TestType.TestSite:                    
                    foreach (var site in SysSettings.Instance.Article_Posting_Sites)
                    {
                        //check connect via joeBlogs                
                        try
                        {
                            site.InitializeWordpress();

                            site.LoadCategories();

                            site.SetConnectStatus(1);
                            site.Status = "Connected!";                             
                            site.updateStatus.Invoke(site._currentWindow, site._statusLabel, site.Connect);
                        }
                        catch (Exception ex)
                        {
                            site.SetConnectStatus(-1);
                            site.Status = ex.Message;                            
                            site.updateStatus.Invoke(site._currentWindow, site._statusLabel, site.Connect);
                        }
                        Thread.Sleep(200);
                    }
                    break;
                case TestType.TestGoogleDrive:
                    try
                    {
                        UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = AutoPost.Instance.CurrentProject.GoogleClientId,
                            ClientSecret = AutoPost.Instance.CurrentProject.GoogleClientSecret
                        },
                        new[] { DriveService.Scope.Drive },
                        "user",
                        CancellationToken.None).Result;

                        // Create the service.
                        var service = new DriveService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = AutoPost.Instance.CurrentProject.GoogleApplicationName
                        });

                        FilesResource.ListRequest request = service.Files.List();
                        request.MaxResults = 1;
                        FileList files = request.Execute();
                        
                        actionGoogleDriveTestConnection.Invoke(_currentWD, _gdrive_ConnectionStatus, true);
                        _isGDriveConnectPass = true;

                        actionUpdateGoogleDriveUsage(_currentWD, _gdrive_UsageBar, GetDriveUsagePercentage(service));
                    }
                    catch (Exception ex)
                    {
                        _isGDriveConnectPass = false;
                        actionGoogleDriveTestConnection.Invoke(_currentWD, _gdrive_ConnectionStatus, false);                        
                    }
                    break;
            };
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch(_testType)
            {
                case TestType.TestSite:            
                    ReleasePostSite();                    
                    break;
                case TestType.TestGoogleDrive:
                    break;
            };
        }

        /// <summary>
        /// Run testing function
        /// </summary>        
        public void TestConnect(TestType testType)
        {
            _testType = testType;
            _connectWorker.RunWorkerAsync();
        }

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Get all wp sites connection status 
        /// </summary>
        private bool GetAllPostSiteConnectionStatus()
        {
            return SysSettings.Instance.Article_Posting_Sites.Count == SysSettings.Instance.Article_Posting_Sites.Where(a => a.Connect < 0).Count() ? true : false;
        }

        /// <summary>
        /// Validate information
        /// </summary>
        public List<Tuple<string, bool, string>> ValidateSiteInformation(List<System.Windows.Controls.TextBox> tbs, PasswordBox pb, ref bool isPass)
        {
            List<Tuple<string, bool, string>> list = new List<Tuple<string, bool, string>>();
            bool isValidate = false;
            string error = "";
            foreach (var tb in tbs)
            {
                if (tb.Text != string.Empty)
                {
                    if (tb.Name == "tbURL")
                    {
                        string pattern = @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$";
                        Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match m = r.Match(tb.Text);
                        if (m.Success)
                        {
                            isValidate = true;
                            error = "";
                        }
                        else
                        {
                            isValidate = false;
                            error = "URL is not correct.Please check again";
                        }
                    }
                    else
                    {
                        isValidate = true;
                        error = "";
                    }
                }
                else
                {
                    isValidate = false;
                    error = "This value cannot be nulled";
                }

                Tuple<string, bool, string> item = new Tuple<string, bool, string>(tb.Name, isValidate, error);
                list.Add(item);
            }

            if (pb.Password == string.Empty)
            {
                list.Add(new Tuple<string, bool, string>(pb.Name, false, "This value cannot be nulled"));
            }
            else
            {
                list.Add(new Tuple<string, bool, string>(pb.Name, true, ""));
            }

            foreach (var i in list)
            {
                if (!i.Item2)
                {
                    isPass = false;
                    break;
                }
            }

            return list;
        }

        /// <summary>
        /// Insert new post site to application system
        /// </summary>
        public bool InsertToPostSites(PostSites site)
        {
            if (SysSettings.Instance.Article_Posting_Sites.Where(a => a.Host == site.Host).Count() > 0)
                return false;
            else
            {
                SysSettings.Instance.InsertPostSite(site);
                return true;
            }
        }

        /// <summary>
        /// Check currentProject
        /// </summary>
        public bool CheckIfSelectedProject()
        {
            return _currentProject != null ? true : false;
        }

        /// <summary>
        /// Update autopost system's current project
        /// </summary>
        public void ChangeProject(PostingProject project)
        {
            if (project != null)
            {
                _currentProject = project;
                ReloadProjectSelectedPostSites();
            }
            else
                actionClearProjectSelectedPostSite.Invoke(_currentWD, _site_dockPanel);
        }

        /// <summary>
        /// Reload ComboBox Project after add new project or delete old ones
        /// </summary>
        private void ReloadProjectSelectedPostSites()
        {
            actionClearProjectSelectedPostSite.Invoke(_currentWD,_site_dockPanel);

            foreach (var site in _currentProject.Sites)
            {
                actionDisplayProjectSelectedPostSites.Invoke(_currentWD, _site_dockPanel, site.Host, true);
            }
        }

        /// <summary>
        /// Update current selected project list of chosen post site
        /// </summary>
        /// <param name="dps"></param>
        public void UpdateProjectSelectedPostSites(List<DockPanel> dps)
        {
            foreach (var dp in dps)
            {
                CheckBox chBox = Utilities.Helper.FindVisualChildren<CheckBox>(dp).ToList().First();
                if (!_currentProject.CheckSiteExisted(chBox.Content.ToString()))
                {
                    if (chBox.IsChecked == true)
                        _currentProject.LoadSite(SysSettings.Instance.GetSite(chBox.Content.ToString()));
                }
                else
                {
                    if (chBox.IsChecked == false)
                        _currentProject.RemoveSite(chBox.Content.ToString());
                }
            }
        }

        private double? GetDriveUsagePercentage(DriveService service)
        {
            try
            {
                About about = service.About.Get().Execute();
                return (about.QuotaBytesTotal / _GIGABYTE) / (about.QuotaBytesUsed / _GIGABYTE);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region AutoPostMessage

        /// <summary>
        /// Handle main operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printMessage()
        {            
            while (true)
            {
                _signal.WaitOne();                
                string message = string.Empty;
                lock (_messageLocker)
                {
                    if (_messages.Count > 0)
                        message = _messages.Dequeue();
                }
                if (message != string.Empty)
                {                    
                    _updateTextBox.Invoke(_currentWD, _messageTB, message);
                }
                Thread.Sleep(_delay);
            }
            //printMessage_Completed();
        }

        /// <summary>
        /// After finish handle main operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printMessage_Completed()
        {
            _updateTextBox.Invoke(_currentWD, _messageTB, "Done!");
        }

        /// <summary>
        /// Start handle operation
        /// </summary>
        /// <param name="action"></param>
        public void RunHandle()
        {
            if (_messageWorker.ThreadState != System.Threading.ThreadState.Running)
                _messageWorker.Start();
        }

        /// <summary>
        /// Add new message to Handle
        /// </summary>
        /// <param name="message"></param>
        public void AddMessages(string message)
        {
            lock (_messageLocker)
                _messages.Enqueue(message);
        }

        /// <summary>
        /// Pause handle operation
        /// </summary>
        public void PauseHandle()
        {
            _signal.Reset();
        }

        /// <summary>
        /// Resume handle operation
        /// </summary>
        public void ResumeHandle()
        {
            _signal.Set();
        }

        #endregion
    }
}
