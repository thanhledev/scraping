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
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;    
using System.ComponentModel;

namespace Scraping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables

        /// <summary>
        /// Variable for checking proxies 
        /// </summary>              
        private HandleUIs _mainHandleUIs = new HandleUIs();
        private SystemView _mainView;

        //global variable 
        /// <summary>
        /// Main navigation labels 
        /// </summary>
        List<System.Windows.Controls.Label> mainLbls = new List<System.Windows.Controls.Label>();        
                
        /// <summary>
        /// Advanced options interfaces
        /// </summary>
        private static BrushConverter bc = new BrushConverter();
        private static ThicknessConverter tc = new ThicknessConverter();
        private static FontWeightConverter fwc = new FontWeightConverter();

        private ArticleSourceLanguage _tempArticleSourceLanguage = ArticleSourceLanguage.All;
        private ArticleSourceType _tempArticleSourceType = ArticleSourceType.All;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            //this.Loaded += new RoutedEventHandler(MainWindow_Loaded);                     
        }

        #region EventHandler

        /// <summary>
        /// Handled load event
        /// </summary>
        //void MainWindow_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //load all main navigation labels
        //    foreach (System.Windows.Controls.Label lbl in Utilities.Helper.FindVisualChildren<System.Windows.Controls.Label>(this.dpMainNavigation))
        //    {
        //        mainLbls.Add(lbl);
        //    }
        //    //initialize all dynamic controls from ini & xml files.
        //    InitializeMissingComponents();                        
        //    HandleSystemStatistics.Instance.SetupHandle(this, tbCPUConsumption, tbRamConsumption, updateTextBoxStatistic);
        //    HandleSystemStatistics.Instance.RunHandle();
            
        //    //setup handleUIs
        //    _mainHandleUIs.SetupHandle(this, hiddenChange, showChange);
        //    _mainView = SystemView.ScrapingInterface;            
        //    Change_WindowView(_mainView);           

        //    //Setup HandleMessageBox
        //    HandleMessageBox.Instance.SetupHandle(this);            

        //    //Setup AutoPost
        //    AutoPost.Instance.SetupAutoPostMessage(tbPostingLiveStatus, updateTextBox1, 20);
        //    AutoPost.Instance.RunHandle();
        //}

        public void ApplySettings()
        {
            //load all main navigation labels
            foreach (System.Windows.Controls.Label lbl in Utilities.Helper.FindVisualChildren<System.Windows.Controls.Label>(this.dpMainNavigation))
            {
                mainLbls.Add(lbl);
            }
            //initialize all dynamic controls from ini & xml files.
            InitializeMissingComponents();
            HandleSystemStatistics.Instance.SetupHandle(this, tbCPUConsumption, tbRamConsumption, updateTextBoxStatistic);
            HandleSystemStatistics.Instance.RunHandle();

            //setup handleUIs
            _mainHandleUIs.SetupHandle(this, hiddenChange, showChange);
            _mainView = SystemView.ScrapingInterface;
            Change_WindowView(_mainView);

            //Setup HandleMessageBox
            HandleMessageBox.Instance.SetupHandle(this);
            RecordHandler.Instance.Initialize();
            //Setup AutoPost
            AutoPost.Instance.SetupAutoPostMessage(tbPostingLiveStatus, updateTextBox1, 20);
            AutoPost.Instance.RunHandle();
            //Setup VirtualNewspapers
            VirtualNewspapers.Instance.SetupVirtualNewspapersMessage(tbVNSProjectStatus, updateTextBox1, 20);
            VirtualNewspapers.Instance.RunHandle();
            //Setup Indexer Handler
            IndexerHandler.Instance.Initialized();
            ProxyHandler.Instance.RegisterActions(updateAliveProxy, updateProxyStatistics, updateTextBox1, updateProxiesTable);
            ProxyHandler.Instance.RegisterControls(this, tbPublicProxies, tbTotalPublicProxies, tbAlivePublicProxies, tbPrivateProxies, tbTotalPrivateProxies, tbAlivePrivateProxies, tbScrapeStatistic);

            //Setup VNSControlHandler
            VNSHarvestControlHandler.Instance.Initialize();
        }

        /// <summary>
        /// Handled closing event
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SysSettings.Instance.SaveSystem();
            ProxyHandler.Instance.SaveSystem();
            IndexerHandler.Instance.SaveSystem();
            HandleSystemStatistics.Instance.StopHandle();
        }

        /// <summary>
        /// Handled closed event
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Finding & creating all dynamic controls are missing
        /// </summary>
        private void InitializeMissingComponents()
        {
            //Scraping interface
            InitializeScrapeMode();
            //InitializeArticleSources();

            //Posting interface
            InitializeProjects();
            InitializeVnsProjects();
            //Configure interface
            InitializePostSites();
            InitializeVnsSites();
            InitializeBlackwords();
            InitializeLanguagues();
            InitializeProxies();
            ImplementSettings();
        }

        /// <summary>
        /// Load ScrapeMode
        /// </summary>
        private void InitializeScrapeMode()
        {
            switch (SysSettings.Instance.Article_ScrapingMode)
            {
                case ScrapeMode.ByKeywords:
                    rbModeKeywords.IsChecked = true;
                    break;
                case ScrapeMode.ByCategories:
                    rbModeCategory.IsChecked = true;
                    break;
            };
        }
        
        /// <summary>
        /// Load ArticleSource
        /// </summary>
        private void InitializeArticleSources()
        {
            spArticleSources.Children.Clear();

            if (_tempArticleSourceLanguage != ArticleSourceLanguage.All)
            {
                foreach (var i in SysSettings.Instance.Article_Scraping_ScrapingSources)
                {
                    if (i.Language == _tempArticleSourceLanguage)
                    {
                        if (_tempArticleSourceType != ArticleSourceType.All)
                        {
                            if (i.Types.Contains(_tempArticleSourceType))
                            {
                                System.Windows.Controls.CheckBox cb = new System.Windows.Controls.CheckBox { Name = i.Name, Content = i.Title, IsChecked = i.Choosen };
                                cb.Checked += new RoutedEventHandler(CheckboxSources_CheckedChange);
                                cb.Unchecked += new RoutedEventHandler(CheckboxSources_CheckedChange);
                                spArticleSources.Children.Add(cb);
                            }
                        }
                        else
                        {
                            System.Windows.Controls.CheckBox cb = new System.Windows.Controls.CheckBox { Name = i.Name, Content = i.Title, IsChecked = i.Choosen };
                            cb.Checked += new RoutedEventHandler(CheckboxSources_CheckedChange);
                            cb.Unchecked += new RoutedEventHandler(CheckboxSources_CheckedChange);
                            spArticleSources.Children.Add(cb);
                        }
                    }
                }
            }
            else
            {
                foreach (var i in SysSettings.Instance.Article_Scraping_ScrapingSources)
                {
                    if (_tempArticleSourceType != ArticleSourceType.All)
                    {
                        if (i.Types.Contains(_tempArticleSourceType))
                        {
                            System.Windows.Controls.CheckBox cb = new System.Windows.Controls.CheckBox { Name = i.Name, Content = i.Title, IsChecked = i.Choosen };
                            cb.Checked += new RoutedEventHandler(CheckboxSources_CheckedChange);
                            cb.Unchecked += new RoutedEventHandler(CheckboxSources_CheckedChange);
                            spArticleSources.Children.Add(cb);
                        }
                    }
                    else
                    {
                        System.Windows.Controls.CheckBox cb = new System.Windows.Controls.CheckBox { Name = i.Name, Content = i.Title, IsChecked = i.Choosen };
                        cb.Checked += new RoutedEventHandler(CheckboxSources_CheckedChange);
                        cb.Unchecked += new RoutedEventHandler(CheckboxSources_CheckedChange);
                        spArticleSources.Children.Add(cb);
                    }
                }
            }

            //foreach (var i in SysSettings.Instance.Article_Scraping_ScrapingSources)
            //{
            //    System.Windows.Controls.CheckBox cb = new System.Windows.Controls.CheckBox { Name = i.Name, Content = i.Title, IsChecked = i.Choosen };
            //    cb.Checked += new RoutedEventHandler(CheckboxSources_CheckedChange);
            //    cb.Unchecked += new RoutedEventHandler(CheckboxSources_CheckedChange);
            //    spArticleSources.Children.Add(cb);
            //}
        }

        /// <summary>
        /// Load Blackwords list
        /// </summary>
        private void InitializeBlackwords()
        {
            foreach (var i in SysSettings.Instance.Article_Scraping_FilterWords)
            {
                tbFilterWords.AppendText(i.Text);
                tbFilterWords.AppendText(Environment.NewLine);
            }
        }

        /// <summary>
        /// Load Languages list
        /// </summary>
        private void InitializeLanguagues()
        {
            System.Windows.ThicknessConverter converter = new ThicknessConverter();
            Thickness dpMargin = (Thickness) converter.ConvertFromString("0,0,5,5");

            foreach (var i in SysSettings.Instance.Article_Scraping_Languages)
            {
                DockPanel dp = new DockPanel { Margin = dpMargin, VerticalAlignment = System.Windows.VerticalAlignment.Top };
                System.Windows.Controls.CheckBox cb = new System.Windows.Controls.CheckBox { Name = i.Name, Content = i.Name, IsChecked = i.Choosen };
                cb.Checked += new RoutedEventHandler(CheckboxLanguages_CheckedChange);
                cb.Unchecked += new RoutedEventHandler(CheckboxLanguages_CheckedChange);
                dp.Children.Add(cb);

                spLanguageList.Children.Add(dp);
            }
        }

        /// <summary>
        /// Load proxies if exists
        /// </summary>
        private void InitializeProxies()
        {                                    
            updateProxiesTable.Invoke(this, tbPublicProxies, ProxyHandler.Instance.PublicProxies);
            updateProxiesTable.Invoke(this, tbPrivateProxies, ProxyHandler.Instance.PrivateProxies);

            updateProxyStatistics.Invoke(this, tbTotalPublicProxies, tbAlivePublicProxies, ProxyHandler.Instance.PublicProxies.Count.ToString(), "-");
            updateProxyStatistics.Invoke(this, tbTotalPrivateProxies, tbAlivePrivateProxies, ProxyHandler.Instance.PrivateProxies.Count.ToString(), "-");
        }        

        /// <summary>
        /// Apply current system settings
        /// </summary>
        private void ImplementSettings()
        {            
            //loop through Search Options
            foreach (object obj in cbSearchOps.Items)
            {
                ComboBoxItem item = (ComboBoxItem)obj;
                if (Utilities.Helper.Parse<ArticleSearchOptions>(item.Tag.ToString()) == SysSettings.Instance.Article_Scraping_SearchOps)
                {
                    cbSearchOps.SelectedIndex = cbSearchOps.Items.IndexOf(item);
                    break;
                }
            }

            //loop through checkboxes
            cbMinWords.IsChecked = SysSettings.Instance.Article_Scraping_MinWordOps;
            tbMinWords.AppendText(SysSettings.Instance.Article_Scraping_MinWordValue.ToString());

            cbMaxWords.IsChecked = SysSettings.Instance.Article_Scraping_MaxWordOps;
            tbMaxWords.AppendText(SysSettings.Instance.Article_Scraping_MaxWordValue.ToString());

            cbMaxThread.IsChecked = SysSettings.Instance.Article_scraping_ThreadsOpts;
            tbMaxThread.AppendText(SysSettings.Instance.Article_Scraping_Threads.ToString());

            cbMaxDepth.IsChecked = SysSettings.Instance.Article_scraping_MaxDepthOpts;
            tbMaxDepth.AppendText(SysSettings.Instance.Article_Scraping_MaxDepth.ToString());

            //loop through Copy Options
            foreach (object obj in cbCopyingOpts.Items)
            {
                ComboBoxItem item = (ComboBoxItem)obj;
                if (Utilities.Helper.Parse<ArticleCopyOptions>(item.Tag.ToString()) == SysSettings.Instance.Article_Scraping_CopyOps)
                {
                    cbCopyingOpts.SelectedIndex = cbCopyingOpts.Items.IndexOf(item);
                    break;
                }
            }

            //loop through Saving Options
            foreach (object obj in cbSavingOpts.Items)
            {
                ComboBoxItem item = (ComboBoxItem)obj;
                if (Utilities.Helper.Parse<ArticleCopyContentSavingOptions>(item.Tag.ToString()) == SysSettings.Instance.Article_Scraping_SavingOps)
                {
                    cbSavingOpts.SelectedIndex = cbSavingOpts.Items.IndexOf(item);
                    break;
                }
            }

            tbFolderPath.AppendText(SysSettings.Instance.Article_Scraping_SavingFolder);

            cbRemoveBlankLines.IsChecked = SysSettings.Instance.Article_Scraping_RemoveBlankLines;
            cbUsingTitleAsFileName.IsChecked = SysSettings.Instance.Article_Scraping_UseTitleAsFileName;
        }

        /// <summary>
        /// Load all projects
        /// </summary>
        private void InitializeProjects()
        {
            foreach (var proj in SysSettings.Instance.Projects)
            {
                //setup project's window & actions
                proj._currentWindow = this;
                proj.updateStatus = updateRunningStatus;
                proj.updateStatistics = updateStatistics;                               
                DockPanel dp = new DockPanel();
                dp.Tag = proj.ProjectName;
                dp.MouseLeftButtonDown += ProjectsDockPanel_MouseLeftButtonDown;
                //label for project name
                System.Windows.Controls.Label lblName = new System.Windows.Controls.Label();
                lblName.Width = 250;
                lblName.Content = proj.ProjectName;
                lblName.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblName.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                dp.Children.Add(lblName);

                //label for project type
                System.Windows.Controls.Label lblType = new System.Windows.Controls.Label();
                lblType.Width = 150;
                switch (proj.ScheduleRule.ScheduleMode)
                {
                    case ScheduleMode.LiveFeed:
                        lblType.Content = "Live feed";
                        break;
                    case ScheduleMode.DatabaseRecords:
                        lblType.Content = "Database records";
                        break;
                }
                lblType.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblType.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                dp.Children.Add(lblType);

                //label for connection status
                System.Windows.Controls.Label lblStatus = new System.Windows.Controls.Label();
                lblStatus.Width = 100;
                lblStatus.Content = "Ready";
                lblStatus.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblStatus.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                lblStatus.Foreground = (Brush)bc.ConvertFrom("Black");
                lblStatus.Background = (Brush)bc.ConvertFrom("#eaff00");
                lblStatus.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                proj._statusLabel = lblStatus;
                dp.Children.Add(lblStatus);

                //label for project statistics
                System.Windows.Controls.Label lblStatistics = new System.Windows.Controls.Label();
                lblStatistics.Width = 295;
                lblStatistics.Content = "// awaiting";
                lblStatistics.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblStatistics.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                proj._statisticsLabel = lblStatistics;
                dp.Children.Add(lblStatistics);

                sp_ListProjects.Children.Add(dp);
            }
        }

        /// <summary>
        /// Load all vns projects
        /// </summary>
        private void InitializeVnsProjects()
        {
            foreach (var proj in SysSettings.Instance.VnsProjects)
            {
                proj._currentWindow = this;
                proj.updateStatus = updateRunningStatus;
                proj.updateStatistics = updateStatistics;

                DockPanel dp = new DockPanel();
                dp.Tag = proj.ProjectName;
                dp.MouseLeftButtonDown += VnsProjectsDockPanel_MouseLeftButtonDown;
                System.Windows.Controls.Label lblName = new System.Windows.Controls.Label();
                lblName.Width = 400;
                lblName.Content = proj.ProjectName;
                lblName.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblName.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                dp.Children.Add(lblName);

                //label for connection status
                System.Windows.Controls.Label lblStatus = new System.Windows.Controls.Label();
                lblStatus.Width = 100;
                lblStatus.Content = "Ready";
                lblStatus.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblStatus.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                lblStatus.Foreground = (Brush)bc.ConvertFrom("Black");
                lblStatus.Background = (Brush)bc.ConvertFrom("#eaff00");
                lblStatus.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                proj._statusLabel = lblStatus;
                dp.Children.Add(lblStatus);

                //label for project statistics
                System.Windows.Controls.Label lblStatistics = new System.Windows.Controls.Label();
                lblStatistics.Width = 295;
                lblStatistics.Content = "// awaiting";
                lblStatistics.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblStatistics.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                proj._statisticsLabel = lblStatistics;
                dp.Children.Add(lblStatistics);

                sp_ListVNSProjects.Children.Add(dp);
            }
        }

        /// <summary>
        /// Load All Post sites
        /// </summary>
        private void InitializePostSites()
        {
            foreach (var i in SysSettings.Instance.Article_Posting_Sites)
            {
                i._currentWindow = this;
                i.updateStatus = updateConnectionStatus;

                System.Windows.Controls.DockPanel dp = new DockPanel();

                dp.Tag = i.Host;
                dp.MouseLeftButtonDown += PostSitesDockPanel_MouseLeftButtonDown;
                dp.Height = 30;
                dp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                dp.Margin = (Thickness)tc.ConvertFrom("0,0,0,1");
                
                //create post site url label
                System.Windows.Controls.Label lblName = new System.Windows.Controls.Label
                {
                    Content = i.Host,
                    Foreground = (Brush)bc.ConvertFrom("Gray"),
                    Width = 400
                };

                //create post site connection label
                System.Windows.Controls.Label lblConnection = new System.Windows.Controls.Label
                {
                    Content = i.GetConnectStatus(),
                    Foreground = (Brush)bc.ConvertFrom("Black"),
                    Width = 147,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center
                };
                i._statusLabel = lblConnection;

                switch (i.Connect)
                {
                    case 1:
                        lblConnection.Background = (Brush)bc.ConvertFrom("Green");
                        break;
                    case -1:
                        lblConnection.Background = (Brush)bc.ConvertFrom("Red");
                        break;
                    default:
                        lblConnection.Background = (Brush)bc.ConvertFrom("Gray");
                        break;
                };

                dp.Children.Add(lblName);
                dp.Children.Add(lblConnection);
                sp_ListPostSites.Children.Add(dp);
            }

            //setup window & controls
            //AutoPost.Instance.SetupWindow(this);

            //setup actions
        }

        /// <summary>
        /// Load All Virtual newspapers sites
        /// </summary>
        private void InitializeVnsSites()
        {
            foreach (var i in SysSettings.Instance.Article_VNS_Sites)
            {
                i._currentWindow = this;
                i.updateStatus = updateConnectionStatus;

                System.Windows.Controls.DockPanel dp = new DockPanel();

                dp.Tag = i.Host;
                dp.MouseLeftButtonDown += VnsSitesDockPanel_MouseLeftButtonDown;
                dp.Height = 30;
                dp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                dp.Margin = (Thickness)tc.ConvertFrom("0,0,0,1");

                //create post site url label
                System.Windows.Controls.Label lblName = new System.Windows.Controls.Label
                {
                    Content = i.Host,
                    Foreground = (Brush)bc.ConvertFrom("Gray"),
                    Width = 400
                };

                //create post site connection label
                System.Windows.Controls.Label lblConnection = new System.Windows.Controls.Label
                {
                    Content = i.GetConnectStatus(),
                    Foreground = (Brush)bc.ConvertFrom("Black"),
                    Width = 147,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center
                };
                i._statusLabel = lblConnection;

                switch (i.Connect)
                {
                    case 1:
                        lblConnection.Background = (Brush)bc.ConvertFrom("Green");
                        break;
                    case -1:
                        lblConnection.Background = (Brush)bc.ConvertFrom("Red");
                        break;
                    default:
                        lblConnection.Background = (Brush)bc.ConvertFrom("Gray");
                        break;
                };

                dp.Children.Add(lblName);
                dp.Children.Add(lblConnection);
                sp_ListVnsSites.Children.Add(dp);
            }
        }

        /// <summary>
        /// Main label click single handler event
        /// </summary>
        private void MainLabel_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Label lbl = e.Source as System.Windows.Controls.Label;
            BrushConverter bc = new BrushConverter();
            foreach (System.Windows.Controls.Label item in mainLbls)
            {                
                item.Foreground = (item.Content == lbl.Content) ? (Brush)bc.ConvertFrom("#be2f2f") : (Brush)bc.ConvertFrom("#767676");
            }

            switch (lbl.Content.ToString().ToLower())
            {
                case "// scrape":
                    Change_WindowView(SystemView.ScrapingInterface);
                    break;
                case "// post":
                    Change_WindowView(SystemView.PostingInterface);
                    break;
                case "// vns":
                    Change_WindowView(SystemView.VNSInterface);
                    break;
                case "// configure":
                    Change_WindowView(SystemView.ConfigureInterface);
                    break;
            };
        }

        /// <summary>
        /// Advanced option buttons click handler event
        /// </summary>
        private void AdvancedOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = e.Source as System.Windows.Controls.Button;
            switch (btn.Content.ToString().ToLower())
            {
                case "blacklist":
                    dpBlackList.Visibility = System.Windows.Visibility.Visible;
                    dpLanguage.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case "language":
                    dpBlackList.Visibility = System.Windows.Visibility.Hidden;
                    dpLanguage.Visibility = System.Windows.Visibility.Visible;
                    break;
            };
        }
       
        private void btnChoose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;           

            // Show the FolderBrowserDialog. 
            DialogResult result = folderDlg.ShowDialog(); 
            if (result == System.Windows.Forms.DialogResult.OK) 
            { 
                tbFolderPath.Text = folderDlg.SelectedPath;
                SysSettings.Instance.Article_Scraping_SavingFolder = folderDlg.SelectedPath;
            }
        }

        private void MinMaxTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)sender;

            switch (tb.Name)
            {
                case "tbMinWords":
                    SysSettings.Instance.Article_Scraping_MinWordValue = Int32.Parse(tb.Text);
                    break;
                case "tbMaxWords":
                    SysSettings.Instance.Article_Scraping_MaxWordValue = Int32.Parse(tb.Text);
                    break;
                case "tbMaxThread":
                    SysSettings.Instance.Article_Scraping_Threads = Int32.Parse(tb.Text);
                    break;
                case "tbMaxDepth":
                    SysSettings.Instance.Article_Scraping_MaxDepth = Int32.Parse(tb.Text);
                    break;
            };
        }

        private void CheckboxSystem_CheckedChange(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox cb = (System.Windows.Controls.CheckBox)sender;

            switch (cb.Name)
            {
                case "cbMinWords":
                    SysSettings.Instance.Article_Scraping_MinWordOps = Convert.ToBoolean(cb.IsChecked);
                    break;
                case "cbMaxWords":
                    SysSettings.Instance.Article_Scraping_MaxWordOps = Convert.ToBoolean(cb.IsChecked);
                    break;
                case "cbMaxThread":
                    SysSettings.Instance.Article_scraping_ThreadsOpts = Convert.ToBoolean(cb.IsChecked);
                    break;
                case "cbMaxDepth":
                    SysSettings.Instance.Article_scraping_MaxDepthOpts = Convert.ToBoolean(cb.IsChecked);
                    break;
                case "cbRemoveBlankLines":
                    SysSettings.Instance.Article_Scraping_RemoveBlankLines = Convert.ToBoolean(cb.IsChecked);
                    break;
                case "cbUsingTitleAsFileName":
                    SysSettings.Instance.Article_Scraping_UseTitleAsFileName = Convert.ToBoolean(cb.IsChecked);
                    break;                
            };
        }

        private void CheckboxSources_CheckedChange(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox cb = (System.Windows.Controls.CheckBox)sender;

            SysSettings.Instance.Article_Scraping_ScrapingSources.Where(a => a.Name == cb.Name).First().Choosen = Convert.ToBoolean(cb.IsChecked);
        }        

        private void CheckboxLanguages_CheckedChange(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox cb = (System.Windows.Controls.CheckBox)sender;

            SysSettings.Instance.Article_Scraping_Languages.Where(a => a.Name == cb.Name).First().Choosen = Convert.ToBoolean(cb.IsChecked);
        }

        private void ComboSystem_SelectedChange(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox cb = (System.Windows.Controls.ComboBox)sender;

            switch (cb.Name)
            {
                case "cbSearchOps":
                    SysSettings.Instance.Article_Scraping_SearchOps = Utilities.Helper.Parse<ArticleSearchOptions>(((System.Windows.Controls.ComboBoxItem)cb.SelectedItem).Tag.ToString());
                    break;
                case "cbCopyingOpts":
                    SysSettings.Instance.Article_Scraping_CopyOps = Utilities.Helper.Parse<ArticleCopyOptions>(((System.Windows.Controls.ComboBoxItem)cb.SelectedItem).Tag.ToString());
                    break;
                case "cbSavingOpts":
                    SysSettings.Instance.Article_Scraping_SavingOps = Utilities.Helper.Parse<ArticleCopyContentSavingOptions>(((System.Windows.Controls.ComboBoxItem)cb.SelectedItem).Tag.ToString());
                    break;
            };
        }
        
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        /// <summary>
        /// Handler radioButton checked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrapeMode_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton ck = sender as System.Windows.Controls.RadioButton;
            if (ck.IsChecked.Value)
            {
                switch (ck.Name)
                {
                    case "rbModeCategory":
                        SysSettings.Instance.Article_ScrapingMode = ScrapeMode.ByCategories;
                        break;
                    case "rbModeKeywords":
                        SysSettings.Instance.Article_ScrapingMode = ScrapeMode.ByKeywords;
                        break;                    
                };
            }
        }

        /// <summary>
        /// Handler MouseLeftButtonDown event on List Projects settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectsDockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DockPanel dp = (DockPanel)sender;
                ToogleSelectedProjectDisplay.Invoke(this, sp_ListProjects, dp);

                AutoPost.Instance.CurrentProject = SysSettings.Instance.GetProject(dp.Tag.ToString());
                toogleControlButtonText.Invoke(this, btnControl, AutoPost.Instance.CurrentProject.ProjectState);
            }
            else
            {                
                DockPanel dp = (DockPanel)sender;

                if (SysSettings.Instance.GetProject(dp.Tag.ToString()).ProjectState != ProjectState.IsRunning)
                {
                    ToogleSelectedProjectDisplay.Invoke(this, sp_ListProjects, dp);

                    EditProject viewer = new EditProject(this, dp.Tag.ToString());
                    viewer.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Handler MouseLeftButtonDown event on List Vns Projects settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VnsProjectsDockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DockPanel dp = (DockPanel)sender;
                ToogleSelectedProjectDisplay.Invoke(this, sp_ListVNSProjects, dp);

                VirtualNewspapers.Instance.CurrentProject = SysSettings.Instance.GetVnsProject(dp.Tag.ToString());
                toogleControlButtonText.Invoke(this, btnControlVNSProject, VirtualNewspapers.Instance.CurrentProject.ProjectState);
            }
            else
            {
                DockPanel dp = (DockPanel)sender;

                if (SysSettings.Instance.GetVnsProject(dp.Tag.ToString()).ProjectState != ProjectState.IsRunning)
                {
                    ToogleSelectedProjectDisplay.Invoke(this, sp_ListVNSProjects, dp);

                    EditVnsProject viewer = new EditVnsProject(this, dp.Tag.ToString());
                    viewer.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Handler MouseLeftButtonDown event on List Post sites settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PostSitesDockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DockPanel dp = (DockPanel)sender;
                ToggleSelectedPostSiteDisplay.Invoke(this, sp_ListPostSites, dp);                
            }
            else
            {
                DockPanel dp = (DockPanel)sender;
                ToggleSelectedPostSiteDisplay.Invoke(this, sp_ListPostSites, dp);

                PostSiteEditor viewer = new PostSiteEditor(this, OpenMode.Edit, dp.Tag.ToString());
                viewer.ShowDialog();

                if (viewer.Result)
                {
                    UpdatePostSite(viewer.OriginalHost, viewer.NewPostSite);
                }
            }
        }

        /// <summary>
        /// Handler MouseLeftButtonDown event on List Post sites settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VnsSitesDockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DockPanel dp = (DockPanel)sender;
                ToogleSelectedVnsSiteDisplay.Invoke(this, sp_ListVnsSites, dp);
            }
            else
            {
                DockPanel dp = (DockPanel)sender;
                ToogleSelectedVnsSiteDisplay.Invoke(this, sp_ListVnsSites, dp);

                VnsSiteEditor viewer = new VnsSiteEditor(this, OpenMode.Edit, dp.Tag.ToString());
                viewer.ShowDialog();

                if (viewer.Result)
                {
                    UpdateVnsSite(viewer.OriginalHost, viewer.NewVnsSite);
                }
            }
        }

        /// <summary>
        /// Handler Control button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            if (AutoPost.Instance.CurrentProject != null)
            {
                if (AutoPost.Instance.CurrentProject.ProjectState == ProjectState.IsReady)
                {
                    if (AutoPost.Instance.CurrentProject.ScheduleRule.ScheduleMode == ScheduleMode.LiveFeed)
                    {
                        //Create again searchPages
                        AutoPost.Instance.CurrentProject.searchPages = new List<string>();
                        //List<SourceCategory> chosenCategories = new List<SourceCategory>();
                        AutoPost.Instance.CurrentProject.chosenCategories = new List<SourceCategory>();
                        foreach (var rule in AutoPost.Instance.CurrentProject.RuleCollection.Rules)
                        {
                            foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                            {
                                SourceCategory check = source.FindCategory(rule.SourceHost, rule.SourceCategory);
                                if (check != null)
                                    source.GetCategoryChildren(AutoPost.Instance.CurrentProject.chosenCategories, check);
                            }
                        }

                        foreach (var cate in AutoPost.Instance.CurrentProject.chosenCategories)
                        {
                            foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                            {
                                if (source.Title == cate.SourceHost)
                                {
                                    ISpawning spawner = SpawnerFactory.CreateSpawner(source);
                                    spawner.CreateSearchPagesLiveFeed(source, AutoPost.Instance.CurrentProject.ScheduleRule.SearchDepth, cate, ref AutoPost.Instance.CurrentProject.searchPages);
                                }
                            }
                        }
                    }
                    else
                    {
                        AutoPost.Instance.CurrentProject.chosenCategories = new List<SourceCategory>();
                        foreach (var rule in AutoPost.Instance.CurrentProject.RuleCollection.Rules)
                        {
                            foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                            {
                                SourceCategory check = source.FindCategory(source.Title, rule.SourceCategory);
                                if (check != null)
                                    source.GetCategoryChildren(AutoPost.Instance.CurrentProject.chosenCategories, check);
                            }
                        }
                    }
                    AutoPost.Instance.CurrentProject.Start();
                    toogleControlButtonText.Invoke(this, btnControl, AutoPost.Instance.CurrentProject.ProjectState);
                }
                else if (AutoPost.Instance.CurrentProject.ProjectState == ProjectState.IsStopped)
                {
                    //AutoPost.Instance.CurrentProject.Start();
                }
                else
                    AutoPost.Instance.CurrentProject.Stop();                
            }
        }

        /// <summary>
        /// Start all posting projects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartAll_Click(object sender, RoutedEventArgs e)
        {            
            Thread startAll = new Thread(() =>
                {
                    lockControls.Invoke(this, btnControl, sp_ListProjects, true);

                    foreach (var proj in SysSettings.Instance.GetAllProjects())
                    {
                        if (proj.ProjectState == ProjectState.IsReady)
                        {
                            if (proj.ScheduleRule.ScheduleMode == ScheduleMode.LiveFeed)
                            {
                                proj.searchPages = new List<string>();

                                proj.chosenCategories = new List<SourceCategory>();

                                foreach (var rule in proj.RuleCollection.Rules)
                                {
                                    foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                                    {
                                        SourceCategory check = source.FindCategory(rule.SourceHost, rule.SourceCategory);
                                        if (check != null)
                                            source.GetCategoryChildren(proj.chosenCategories, check);
                                    }
                                }

                                foreach (var cate in proj.chosenCategories)
                                {
                                    foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                                    {
                                        if (source.Title == cate.SourceHost)
                                        {
                                            ISpawning spawner = SpawnerFactory.CreateSpawner(source);
                                            spawner.CreateSearchPagesLiveFeed(source, proj.ScheduleRule.SearchDepth, cate, ref proj.searchPages);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                proj.chosenCategories = new List<SourceCategory>();

                                foreach (var rule in proj.RuleCollection.Rules)
                                {
                                    foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                                    {
                                        SourceCategory check = source.FindCategory(source.Title, rule.SourceCategory);

                                        if (check != null)
                                            source.GetCategoryChildren(proj.chosenCategories, check);
                                    }
                                }
                            }
                            proj.Start();
                        }
                        else
                        {
                            if (proj.ProjectState == ProjectState.IsStopped)
                            {
                                //proj.Start();
                            }
                        }
                        Thread.Sleep(1000);
                    }
                    lockControls.Invoke(this, btnControl, sp_ListProjects, false);
                });
            startAll.IsBackground = true;
            startAll.Start();
        }

        /// <summary>
        /// Handler ControlVnsProject button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnControlVNSProject_Click(object sender, RoutedEventArgs e)
        {
            if (VirtualNewspapers.Instance.CurrentProject != null)
            {
                if (VirtualNewspapers.Instance.CurrentProject.ProjectState == ProjectState.IsReady)
                {
                    VirtualNewspapers.Instance.CurrentProject.Start();
                    toogleControlButtonText.Invoke(this, btnControlVNSProject, VirtualNewspapers.Instance.CurrentProject.ProjectState);
                }
                else if (VirtualNewspapers.Instance.CurrentProject.ProjectState == ProjectState.IsStopped)
                {

                }
                else
                {

                }                
            }
        }

        /// <summary>
        /// Handler StartAllVNS button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartAllVNS_Click(object sender, RoutedEventArgs e)
        {
            Thread startAll = new Thread(() =>
            {
                lockControls.Invoke(this, btnControlVNSProject, sp_ListVNSProjects, true);

                foreach (var proj in SysSettings.Instance.GetAllVNSProjects())
                {
                    proj.Start();
                    Thread.Sleep(1000);
                }

                lockControls.Invoke(this, btnControlVNSProject, sp_ListVNSProjects, false);
            });
            startAll.IsBackground = true;
            startAll.Start();
        }

        /// <summary>
        /// Handler NewProject button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNewProject_Click(object sender, RoutedEventArgs e)
        {
            CreateProject viewer = new CreateProject(this);
            viewer.ShowDialog();

            if (viewer.Result)
            {
                PostingProject proj = SysSettings.Instance.GetProject(viewer.NewProjectName);

                //setup project's window & actions
                proj._currentWindow = this;
                proj.updateStatus = updateRunningStatus;
                proj.updateStatistics = updateStatistics;                
                DockPanel dp = new DockPanel();
                dp.Tag = proj.ProjectName;
                dp.MouseLeftButtonDown += ProjectsDockPanel_MouseLeftButtonDown;
                //label for project name
                System.Windows.Controls.Label lblName = new System.Windows.Controls.Label();
                lblName.Width = 250;
                lblName.Content = proj.ProjectName;
                lblName.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblName.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                dp.Children.Add(lblName);

                //label for project type
                System.Windows.Controls.Label lblType = new System.Windows.Controls.Label();
                lblType.Width = 150;
                switch (proj.ScheduleRule.ScheduleMode)
                {
                    case ScheduleMode.LiveFeed:
                        lblType.Content = "Live feed";
                        break;
                    case ScheduleMode.DatabaseRecords:
                        lblType.Content = "Database records";
                        break;
                }
                lblType.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblType.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                dp.Children.Add(lblType);

                //label for connection status
                System.Windows.Controls.Label lblStatus = new System.Windows.Controls.Label();
                lblStatus.Width = 100;
                lblStatus.Content = "Ready";
                lblStatus.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblStatus.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                lblStatus.Foreground = (Brush)bc.ConvertFrom("Black");
                lblStatus.Background = (Brush)bc.ConvertFrom("#eaff00");
                lblStatus.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                proj._statusLabel = lblStatus;
                dp.Children.Add(lblStatus);

                //label for project statistics
                System.Windows.Controls.Label lblStatistics = new System.Windows.Controls.Label();
                lblStatistics.Width = 295;
                lblStatistics.Content = "// awaiting";
                lblStatistics.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblStatistics.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                proj._statisticsLabel = lblStatistics;
                dp.Children.Add(lblStatistics);

                sp_ListProjects.Children.Add(dp);
            }
        }

        /// <summary>
        /// Hanlder NewVnsProject button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNewVNSProject_Click(object sender, RoutedEventArgs e)
        {
            CreateVnsProject viewer = new CreateVnsProject(this);
            viewer.ShowDialog();

            if (viewer.Result)
            {
                VNSPostingProject proj = SysSettings.Instance.GetVnsProject(viewer.NewProjectName);

                proj._currentWindow = this;
                proj.updateStatus = updateRunningStatus;
                proj.updateStatistics = updateStatistics;

                DockPanel dp = new DockPanel();
                dp.Tag = proj.ProjectName;
                dp.MouseLeftButtonDown += VnsProjectsDockPanel_MouseLeftButtonDown;
                System.Windows.Controls.Label lblName = new System.Windows.Controls.Label();
                lblName.Width = 400;
                lblName.Content = proj.ProjectName;
                lblName.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblName.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                dp.Children.Add(lblName);

                //label for connection status
                System.Windows.Controls.Label lblStatus = new System.Windows.Controls.Label();
                lblStatus.Width = 100;
                lblStatus.Content = "Ready";
                lblStatus.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblStatus.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                lblStatus.Foreground = (Brush)bc.ConvertFrom("Black");
                lblStatus.Background = (Brush)bc.ConvertFrom("#eaff00");
                lblStatus.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                proj._statusLabel = lblStatus;
                dp.Children.Add(lblStatus);

                //label for project statistics
                System.Windows.Controls.Label lblStatistics = new System.Windows.Controls.Label();
                lblStatistics.Width = 295;
                lblStatistics.Content = "// awaiting";
                lblStatistics.BorderBrush = (Brush)bc.ConvertFrom("#2a2a2a");
                lblStatistics.BorderThickness = (Thickness)tc.ConvertFrom("0,0,0,1");
                proj._statisticsLabel = lblStatistics;
                dp.Children.Add(lblStatistics);

                sp_ListVNSProjects.Children.Add(dp);
            }
        }

        /// <summary>
        /// Handler DeleteProject button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (AutoPost.Instance.CurrentProject != null)
            {
                if (AutoPost.Instance.CurrentProject.ProjectState != ProjectState.IsRunning)
                {
                    HandleMessageBox.Instance.GetConfirmation(SystemMessage.Warning);
                    if (HandleMessageBox.Instance.Answer)
                    {
                        PostingProject currentProject = AutoPost.Instance.CurrentProject;

                        if (SysSettings.Instance.Projects.Where(a => a.ProjectName != currentProject.ProjectName).Count() > 0)
                        {
                            //remove selected item from interface
                            foreach (DockPanel item in Helper.FindVisualChildren<DockPanel>(sp_ListProjects))
                            {
                                if (item.Tag.ToString() == currentProject.ProjectName)
                                    item.Visibility = System.Windows.Visibility.Collapsed;
                            }

                            SysSettings.Instance.RemoveProject(currentProject.ProjectName);
                            AutoPost.Instance.CurrentProject = null;
                        }
                        else
                        {
                            SysSettings.Instance.RemoveProject(currentProject.ProjectName);
                            AutoPost.Instance.CurrentProject = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handler DeleteVnsProject button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteVNSProject_Click(object sender, RoutedEventArgs e)
        {
            if (VirtualNewspapers.Instance.CurrentProject != null)
            {
                if (VirtualNewspapers.Instance.CurrentProject.ProjectState != ProjectState.IsRunning)
                {
                    HandleMessageBox.Instance.GetConfirmation(SystemMessage.Warning);
                    if (HandleMessageBox.Instance.Answer)
                    {
                        VNSPostingProject currentProject = VirtualNewspapers.Instance.CurrentProject;

                        if (SysSettings.Instance.VnsProjects.Where(a => a.ProjectName != currentProject.ProjectName).Count() > 0)
                        {
                            //remove selected item from interface
                            foreach (DockPanel item in Helper.FindVisualChildren<DockPanel>(sp_ListVNSProjects))
                            {
                                if (item.Tag.ToString() == currentProject.ProjectName)
                                    item.Visibility = System.Windows.Visibility.Collapsed;
                            }

                            SysSettings.Instance.RemoveVnsProject(currentProject.ProjectName);
                            VirtualNewspapers.Instance.CurrentProject = null;
                        }
                        else
                        {
                            SysSettings.Instance.RemoveVnsProject(currentProject.ProjectName);
                            VirtualNewspapers.Instance.CurrentProject = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handler Configure buttons click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfigureSettings_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            ToogleButtonStyle.Invoke(this, sp_ConfigureButtons, btn.Name);
            ToogleSettingInterfaces.Invoke(this, dp_SystemSettings, dp_PostSites, dp_BlogSites, dp_Proxies, dp_VnsSites, btn.Name);
        }

        /// <summary>
        /// Handler TestConnection button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            AutoPost.Instance.TestConnect(TestType.TestSite);
        }

        /// <summary>
        /// Handler AddPostSite button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddPostSite_Click(object sender, RoutedEventArgs e)
        {
            PostSiteEditor viewer = new PostSiteEditor(this, OpenMode.Add, "");
            viewer.ShowDialog();
            
            if (viewer.Result)
            {
                InsertNewPostSite(viewer.NewPostSite);
            }            
        }

        /// <summary>
        /// Handler AddVNSSite button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddVNSSite_Click(object sender, RoutedEventArgs e)
        {
            VnsSiteEditor viewer = new VnsSiteEditor(this, OpenMode.Add, "");
            viewer.ShowDialog();

            if (viewer.Result)
                InsertNewVnsSite(viewer.NewVnsSite);
        }

        private void btnDeleteVNSSite_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Handler TestVNSConnection button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestVNSConnection_Click(object sender, RoutedEventArgs e)
        {
            VirtualNewspapers.Instance.TestConnect(TestType.TestSite);
        }

        /// <summary>
        /// Handler AddPublicProxis button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddPublicProxies_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "";
            dlg.Filter = "txt files (*.txt)|*.txt";
            dlg.DefaultExt = "txt files (*.txt)|*.txt";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string fileName = dlg.FileName;

                ProxyHandler.Instance.LoadProxies(fileName);
                updateProxiesTable.Invoke(this, tbPublicProxies, ProxyHandler.Instance.PublicProxies);
                updateProxyStatistics.Invoke(this, tbTotalPublicProxies, tbAlivePublicProxies, ProxyHandler.Instance.PublicProxies.Count.ToString(), "-");
            }
        }

        /// <summary>
        /// Handler AddPrivateProxies button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddPrivateProxies_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "";
            dlg.Filter = "txt files (*.txt)|*.txt";
            dlg.DefaultExt = "txt files (*.txt)|*.txt";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string fileName = dlg.FileName;

                ProxyHandler.Instance.LoadPrivateProxies(fileName);
                updateProxiesTable.Invoke(this, tbPrivateProxies, ProxyHandler.Instance.PrivateProxies);
                updateProxyStatistics.Invoke(this, tbTotalPrivateProxies, tbAlivePrivateProxies, ProxyHandler.Instance.PrivateProxies.Count.ToString(), "-");
            }
        }        

        /// <summary>
        /// Handler ProxyGrabber button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProxyGrabber_Click(object sender, RoutedEventArgs e)
        {
            if (!ProxyHandler.Instance.IsTesting && !ProxyHandler.Instance.IsScraping)
                ProxyHandler.Instance.ScrapeProxy(ProxyTestType.Public);
        }

        /// <summary>
        /// Handler AdvanceSettings button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProxyAdvanceSettings_Click(object sender, RoutedEventArgs e)
        {
            ProxyAdvanceSettings viewer = new ProxyAdvanceSettings(this);
            viewer.ShowDialog();
        }

        /// <summary>
        /// Handler TestPublic button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestPublic_Click(object sender, RoutedEventArgs e)
        {
            if (!ProxyHandler.Instance.IsTesting)
                ProxyHandler.Instance.RunHandle(ProxyTestType.Public);
        }

        /// <summary>
        /// Handler TestPrivate button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestPrivate_Click(object sender, RoutedEventArgs e)
        {
            if (!ProxyHandler.Instance.IsTesting)
                ProxyHandler.Instance.RunHandle(ProxyTestType.Private);
        }

        /// <summary>
        /// Handler AdvancedSystemSettings button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdvancedSystemSettings_Click(object sender, RoutedEventArgs e)
        {
            AdvancedSettings viewer = new AdvancedSettings(this);
            viewer.ShowDialog();
        }

        #endregion
       
        #region ActionList

        Action<string, Window, System.Windows.Controls.TextBox> updateTextBox = (message, wd, tb) => { wd.Dispatcher.Invoke(new Action(() => { tb.AppendText(message); tb.AppendText(Environment.NewLine); tb.ScrollToEnd(); })); };

        Action<Window, System.Windows.Controls.TextBox, string> updateTextBox1 = (wd, tb, message) => { wd.Dispatcher.Invoke(new Action(() => { tb.AppendText(message); tb.AppendText(Environment.NewLine); tb.ScrollToEnd(); })); };

        Action<Window, System.Windows.Controls.TextBox, string> updateTextBoxStatistic = (wd, tb, value) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                tb.Text = value;
            }));
        };

        Action<Window, System.Windows.Controls.Label, int> updateStatus = (wd, lbl, value) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
                {
                    lbl.Content = "Process " + value + " proxies";
                }));
        };
        
        Action<Window, System.Windows.Controls.TextBox, Queue<SystemProxy>> updateProxiesTable = (wd, tb, proxies) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                tb.Text = "";
                foreach (var proxy in proxies)
                {
                    tb.AppendText(proxy.ToString());
                    tb.AppendText(Environment.NewLine);
                }
            }));
        };

        Action<Window, System.Windows.Controls.TextBox, System.Windows.Controls.TextBox, string, string> updateProxyStatistics = (wd, tbTotal, tbAlive, total, alive) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                tbTotal.Text = total;
                tbAlive.Text = alive;
            }));
        };

        Action<Window, System.Windows.Controls.TextBox, string> updateAliveProxy = (wd, tbAlive, alive) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {                
                tbAlive.Text = alive;
            }));
        };

        Action<Window, List<System.Windows.Controls.DockPanel>, bool> enableChange = (wd, ctr, flag) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var i in ctr)
                {
                    i.IsEnabled = (flag) ? true : false;
                }
            }));
        };

        Action<Window, List<System.Windows.Controls.DockPanel>, bool> hiddenChange = (wd, ctr, flag) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var i in ctr)
                {
                    i.Visibility = (flag) ? Visibility.Hidden : Visibility.Visible;
                }
            }));
        };

        Action<Window, List<System.Windows.Controls.DockPanel>, bool> showChange = (wd, ctr, flag) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var i in ctr)
                {
                    i.Visibility = (flag) ? Visibility.Visible : Visibility.Hidden;
                }
            }));
        };

        Action<Window, System.Windows.Controls.TextBox, int> updateScrapeTextBox = (wd, tb, value) =>
            {
                wd.Dispatcher.Invoke(new Action(() =>
                    {
                        tb.Text = value.ToString();
                    }));
            };

        Action<Window, StackPanel, DockPanel> ToogleSelectedProjectDisplay = (wd, sp, dp) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (DockPanel i in Utilities.Helper.FindVisualChildren<DockPanel>(sp))
                {
                    i.Background = i == dp ? (Brush)bc.ConvertFrom("#464545") : (Brush)bc.ConvertFrom("Black");
                }
            }));
        };

        Action<Window, StackPanel, DockPanel> ToggleSelectedPostSiteDisplay = (wd, sp, dp) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (DockPanel i in Utilities.Helper.FindVisualChildren<DockPanel>(sp))
                {
                    i.Background = i == dp ? (Brush)bc.ConvertFrom("#464545") : (Brush)bc.ConvertFrom("Black");
                }
            }));
        };

        Action<Window, StackPanel, DockPanel> ToogleSelectedVnsSiteDisplay = (wd, sp, dp) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (DockPanel i in Utilities.Helper.FindVisualChildren<DockPanel>(sp))
                {
                    i.Background = i == dp ? (Brush)bc.ConvertFrom("#464545") : (Brush)bc.ConvertFrom("Black");
                }
            }));
        };

        Action<Window, System.Windows.Controls.Label, ProjectState> updateRunningStatus = (wd, lbl, state) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                switch (state)
                {
                    case ProjectState.IsReady:
                        lbl.Content = "Ready";
                        lbl.Background = (Brush)bc.ConvertFrom("#eaff00");
                        break;
                    case ProjectState.IsRunning:
                        lbl.Content = "Running";
                        lbl.Background = (Brush)bc.ConvertFrom("#48ff4c");
                        break;
                    case ProjectState.IsStopped:
                        lbl.Content = "Inactive";
                        lbl.Background = (Brush)bc.ConvertFrom("#fe4545");
                        break;
                };
            }));
        };

        Action<Window, System.Windows.Controls.Label, string> updateStatistics = (wd, lbl, value) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                lbl.Content = value;
            }));
        };

        Action<Window, System.Windows.Controls.Button, ProjectState> toogleControlButtonText = (wd, btn, state) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                if (state == ProjectState.IsReady || state == ProjectState.IsStopped)
                    btn.Content = "Start";                
                else
                    btn.Content = "Stop";
            }));
        };

        Action<Window, StackPanel, string> ToogleButtonStyle = (wd, sp, name) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (System.Windows.Controls.Button btn in Utilities.Helper.FindVisualChildren<System.Windows.Controls.Button>(sp))
                {
                    if (btn.Name == name)
                    {
                        btn.Foreground = (Brush)bc.ConvertFrom("Green");
                        btn.FontWeight = (FontWeight)fwc.ConvertFrom("Bold");
                    }
                    else
                    {
                        btn.Foreground = (Brush)bc.ConvertFrom("Gray");
                        btn.FontWeight = (FontWeight)fwc.ConvertFrom("Normal");
                    }
                }
            }));
        };

        Action<Window, DockPanel, DockPanel, DockPanel, DockPanel, DockPanel, string> ToogleSettingInterfaces = (wd, mode1, mode2, mode3, mode4, mode5, name) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                if (name == "btnSystemSettings")
                {
                    mode1.Visibility = Visibility.Visible;
                    mode2.Visibility = Visibility.Hidden;
                    mode3.Visibility = Visibility.Hidden;
                    mode4.Visibility = Visibility.Hidden;
                    mode5.Visibility = Visibility.Hidden;
                }
                else if (name == "btnPostSite")
                {
                    mode1.Visibility = Visibility.Hidden;
                    mode2.Visibility = Visibility.Visible;
                    mode3.Visibility = Visibility.Hidden;
                    mode4.Visibility = Visibility.Hidden;
                    mode5.Visibility = Visibility.Hidden;
                }
                else if (name == "btnBlogSite")
                {
                    mode1.Visibility = Visibility.Hidden;
                    mode2.Visibility = Visibility.Hidden;
                    mode3.Visibility = Visibility.Visible;
                    mode4.Visibility = Visibility.Hidden;
                    mode5.Visibility = Visibility.Hidden;
                }
                else if (name == "btnProxy")
                {
                    mode1.Visibility = Visibility.Hidden;
                    mode2.Visibility = Visibility.Hidden;
                    mode3.Visibility = Visibility.Hidden;
                    mode4.Visibility = Visibility.Visible;
                    mode5.Visibility = Visibility.Hidden;
                }
                else
                {
                    mode1.Visibility = Visibility.Hidden;
                    mode2.Visibility = Visibility.Hidden;
                    mode3.Visibility = Visibility.Hidden;
                    mode4.Visibility = Visibility.Hidden;
                    mode5.Visibility = Visibility.Visible;
                }
            }));
        };

        Action<Window, System.Windows.Controls.Label, int> updateConnectionStatus = (wd, lbl, state) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                switch (state)
                {
                    case 1:
                        lbl.Content = "Connected!";
                        lbl.Background = (Brush)bc.ConvertFrom("Green");
                        break;
                    case -1:
                        lbl.Content = "Failed!";
                        lbl.Background = (Brush)bc.ConvertFrom("#be2f2f");
                        break;  
                    default:
                        lbl.Content = "Pending!";
                        lbl.Background = (Brush)bc.ConvertFrom("Gray");
                        break;  
                };
            }));
        };

        Action<Window, System.Windows.Controls.Button, System.Windows.Controls.StackPanel, bool> lockControls = (wd, btn, sp, flag) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                if (flag)
                {
                    btn.IsEnabled = false;
                    sp.IsEnabled = false;
                }
                else
                {
                    btn.IsEnabled = true;
                    sp.IsEnabled = true;
                }
            }));
        };

        #endregion

        #region ChangeView

        public void Change_WindowView(SystemView view)
        {
            _mainHandleUIs.ResetHandle();
            switch (view)
            {
                case SystemView.ScrapingInterface:
                    DisplayScrapingInterface();
                    break;                
                case SystemView.ConfigureInterface:
                    DisplayConfigureInterface();
                    break;
                case SystemView.PostingInterface:
                    DisplayPostingInterface();
                    break;
                case SystemView.VNSInterface:
                    DisplayVirtualNewspapersInterface();
                    break;
            };
        }

        public void DisplayScrapingInterface()
        {
            if (Scrape.Instance._state == ScrapeState.isInitialize)
            {
                //add enable dockpanel
                //_mainHandleUIs.AddEnable(dpMainNavigation);

                //add hidden dockpanel
                _mainHandleUIs.AddHidden(dp_Scraping_WorkingInterface);
                _mainHandleUIs.AddHidden(dp_ConfigureInterface);
                _mainHandleUIs.AddHidden(dp_Posting_SettingsInterface);
                _mainHandleUIs.AddHidden(dp_VNS_SettingsInterface);

                //add show dockpanel
                _mainHandleUIs.AddShow(dp_KeywordInput);
                _mainHandleUIs.AddShow(dp_Scraping_SourceSettingsInterface);
                _mainHandleUIs.AddShow(dp_ScrapeMode);

                _mainHandleUIs.RunHandle(true);
            }
            else
            {
                //add enable dockpanel
                //_mainHandleUIs.AddEnable(dpMainNavigation);

                //add hidden dockpanel
                _mainHandleUIs.AddHidden(dp_ScrapeMode);
                _mainHandleUIs.AddHidden(dp_KeywordInput);
                _mainHandleUIs.AddHidden(dp_Scraping_SourceSettingsInterface);
                _mainHandleUIs.AddHidden(dp_ConfigureInterface);
                _mainHandleUIs.AddHidden(dp_Posting_SettingsInterface);
                _mainHandleUIs.AddHidden(dp_VNS_SettingsInterface);
                //add show dockpanel
                _mainHandleUIs.AddShow(dp_Scraping_WorkingInterface);

                _mainHandleUIs.RunHandle(false);
            }
        }

        public void DisplayPostingInterface()
        {
            //add hidden dockpanel
            _mainHandleUIs.AddHidden(dp_ScrapeMode);
            _mainHandleUIs.AddHidden(dp_KeywordInput);
            _mainHandleUIs.AddHidden(dp_Scraping_SourceSettingsInterface);
            _mainHandleUIs.AddHidden(dp_Scraping_WorkingInterface);
            _mainHandleUIs.AddHidden(dp_ConfigureInterface);
            _mainHandleUIs.AddHidden(dp_VNS_SettingsInterface);
            //add show dockpanel
            _mainHandleUIs.AddShow(dp_Posting_SettingsInterface);

            _mainHandleUIs.RunHandle(true);
        }

        public void DisplayVirtualNewspapersInterface()
        {
            //add hidden dockpanel
            _mainHandleUIs.AddHidden(dp_ScrapeMode);
            _mainHandleUIs.AddHidden(dp_KeywordInput);
            _mainHandleUIs.AddHidden(dp_Scraping_SourceSettingsInterface);
            _mainHandleUIs.AddHidden(dp_Scraping_WorkingInterface);
            _mainHandleUIs.AddHidden(dp_Posting_SettingsInterface);
            _mainHandleUIs.AddHidden(dp_ConfigureInterface);

            //add show dockpanel
            _mainHandleUIs.AddShow(dp_VNS_SettingsInterface);

            _mainHandleUIs.RunHandle(true);
        }

        public void DisplayConfigureInterface()
        {
            if (SysSettings.Instance.getSystemState() == SystemState.isIdle)
            {
                //add enable dockpanel
                //_mainHandleUIs.AddEnable(dpMainNavigation);

                //add hidden dockpanel
                _mainHandleUIs.AddHidden(dp_ScrapeMode);
                _mainHandleUIs.AddHidden(dp_KeywordInput);
                _mainHandleUIs.AddHidden(dp_Scraping_SourceSettingsInterface);
                _mainHandleUIs.AddHidden(dp_Scraping_WorkingInterface);
                _mainHandleUIs.AddHidden(dp_Posting_SettingsInterface);
                _mainHandleUIs.AddHidden(dp_VNS_SettingsInterface);
                //add show dockpanel
                _mainHandleUIs.AddShow(dp_ConfigureInterface);

                _mainHandleUIs.RunHandle(true);
            }
        }        
       
        #endregion

        #region ScrapingZone

        /// <summary>
        /// Handler button Action click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAction_Click(object sender, RoutedEventArgs e)
        {            
            Scrape.Instance.AddTextBox(tbLiveStatus);
            Scrape.Instance.AddTextBox(tbQueue);
            Scrape.Instance.AddTextBox(tbSuccess);
            Scrape.Instance.AddTextBox(tbFailed);
            Scrape.Instance.AddTextBox(tbThreads);
            Scrape.Instance.AddTextBox(tbSaved);
            Scrape.Instance.SetupScrape(this, updateScrapeTextBox, updateTextBox, tbKeywords.Text);
            Change_WindowView(SystemView.ScrapingInterface);                               
        }

        /// <summary>
        /// Handler comboBox SourceLanguage selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSourceLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSourceLanguage.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbSourceLanguage.SelectedItem;
                _tempArticleSourceLanguage = Utilities.Helper.Parse<ArticleSourceLanguage>(selected.Tag.ToString()) ?? ArticleSourceLanguage.English;
                InitializeArticleSources();
            }
        }

        /// <summary>
        /// Handler comboBox SourceType selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSourceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSourceType.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbSourceType.SelectedItem;
                _tempArticleSourceType = Utilities.Helper.Parse<ArticleSourceType>(selected.Tag.ToString()) ?? ArticleSourceType.News;
                InitializeArticleSources();
            }
        }

        /// <summary>
        /// Handler articleSource contextMenu click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void articleSource_ContextMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (Scrape.Instance._state == ScrapeState.isReady)
            {
                Scrape.Instance.RunScrape();
                btnPause.Content = "Pause";
            }
            else if (Scrape.Instance._state == ScrapeState.isRunning)
            {
                Scrape.Instance.PauseScrape();
                btnPause.Content = "Play";
            }
            else
            {
                Scrape.Instance.ResumeScrape();
                btnPause.Content = "Pause";
            }
        }

        //private void btnExit_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Scrape.Instance._state == ScrapeState.isRunning)
        //    {

        //    }
        //    else if (Scrape.Instance._state == ScrapeState.isPaused)
        //    {

        //    }
        //    else
        //    {

        //    }
        //}

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            switch (Scrape.Instance._state)
            {
                case ScrapeState.isReady:

                    break;
                case ScrapeState.isDone:
                    //do nothing because is done
                    break;
                case ScrapeState.isRunning:
                    Scrape.Instance.StopScrape();
                    btnPause.Content = "Play";
                    break;
                case ScrapeState.isPaused:
                    Scrape.Instance.ResumeScrape();
                    Scrape.Instance.StopScrape();
                    btnPause.Content = "Play";
                    break;

            };
        }

        #endregion

        #region PostingZone
               


        #endregion                  

        #region VNSZone



        #endregion

        #region ConfigureZone

        private void InsertNewPostSite(string name)
        {
            PostSites newSite = SysSettings.Instance.GetSite(name);

            newSite._currentWindow = this;
            newSite.updateStatus = updateConnectionStatus;

            System.Windows.Controls.DockPanel dp = new DockPanel();

            dp.Tag = newSite.Host;
            dp.MouseLeftButtonDown += PostSitesDockPanel_MouseLeftButtonDown;
            dp.Height = 30;
            dp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            dp.Margin = (Thickness)tc.ConvertFrom("0,0,0,1");

            //create post site url label
            System.Windows.Controls.Label lblName = new System.Windows.Controls.Label
            {
                Content = newSite.Host,
                Foreground = (Brush)bc.ConvertFrom("Gray"),
                Width = 400
            };

            //create post site connection label
            System.Windows.Controls.Label lblConnection = new System.Windows.Controls.Label
            {
                Content = newSite.GetConnectStatus(),
                Foreground = (Brush)bc.ConvertFrom("Black"),
                Width = 147,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center
            };
            newSite._statusLabel = lblConnection;

            switch (newSite.Connect)
            {
                case 1:
                    lblConnection.Background = (Brush)bc.ConvertFrom("Green");
                    break;
                case -1:
                    lblConnection.Background = (Brush)bc.ConvertFrom("Red");
                    break;
                default:
                    lblConnection.Background = (Brush)bc.ConvertFrom("Gray");
                    break;
            };

            dp.Children.Add(lblName);
            dp.Children.Add(lblConnection);
            sp_ListPostSites.Children.Add(dp);
        }

        private void InsertNewVnsSite(string name)
        {
            VirtualNewspapersSite newSite = SysSettings.Instance.GetVnsSite(name);

            newSite._currentWindow = this;
            newSite.updateStatus = updateConnectionStatus;

            System.Windows.Controls.DockPanel dp = new DockPanel();

            dp.Tag = newSite.Host;
            dp.MouseLeftButtonDown += VnsSitesDockPanel_MouseLeftButtonDown;
            dp.Height = 30;
            dp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            dp.Margin = (Thickness)tc.ConvertFrom("0,0,0,1");

            //create post site url label
            System.Windows.Controls.Label lblName = new System.Windows.Controls.Label
            {
                Content = newSite.Host,
                Foreground = (Brush)bc.ConvertFrom("Gray"),
                Width = 400
            };

            //create post site connection label
            System.Windows.Controls.Label lblConnection = new System.Windows.Controls.Label
            {
                Content = newSite.GetConnectStatus(),
                Foreground = (Brush)bc.ConvertFrom("Black"),
                Width = 147,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center
            };
            newSite._statusLabel = lblConnection;

            switch (newSite.Connect)
            {
                case 1:
                    lblConnection.Background = (Brush)bc.ConvertFrom("Green");
                    break;
                case -1:
                    lblConnection.Background = (Brush)bc.ConvertFrom("Red");
                    break;
                default:
                    lblConnection.Background = (Brush)bc.ConvertFrom("Gray");
                    break;
            };

            dp.Children.Add(lblName);
            dp.Children.Add(lblConnection);
            sp_ListVnsSites.Children.Add(dp);
        }

        private void UpdatePostSite(string oldName, string newName)
        {
            foreach (DockPanel dp in Helper.FindVisualChildren<DockPanel>(sp_ListPostSites))
            {
                if (dp.Tag.ToString() == oldName)
                {
                    System.Windows.Controls.Label lblName = Helper.FindVisualChildren<System.Windows.Controls.Label>(dp).ToList().Where(a => a.Content.ToString() == oldName).First();
                    System.Windows.Controls.Label lblConnection = Helper.FindVisualChildren<System.Windows.Controls.Label>(dp).ToList().Where(a => a.Content.ToString() == "Pending!" || a.Content.ToString() == "Connected!" || a.Content.ToString() == "Failed!").First();

                    lblName.Content = newName;

                    lblConnection.Background = (Brush)bc.ConvertFrom("Gray");
                    lblConnection.Content = "Pending!";
                }
            }
        }

        private void UpdateVnsSite(string oldName, string newName)
        {
            foreach (DockPanel dp in Helper.FindVisualChildren<DockPanel>(sp_ListVnsSites))
            {
                if (dp.Tag.ToString() == oldName)
                {
                    System.Windows.Controls.Label lblName = Helper.FindVisualChildren<System.Windows.Controls.Label>(dp).ToList().Where(a => a.Content.ToString() == oldName).First();
                    System.Windows.Controls.Label lblConnection = Helper.FindVisualChildren<System.Windows.Controls.Label>(dp).ToList().Where(a => a.Content.ToString() == "Pending!" || a.Content.ToString() == "Connected!" || a.Content.ToString() == "Failed!").First();

                    lblName.Content = newName;

                    lblConnection.Background = (Brush)bc.ConvertFrom("Gray");
                    lblConnection.Content = "Pending!";
                }
            }
        }

        #endregion      

    }
}
