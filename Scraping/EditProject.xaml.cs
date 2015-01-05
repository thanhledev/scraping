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
using System.Windows.Shapes;
using Utilities;
using DataTypes;
using DataTypes.Interfaces;
using DataTypes.Collections;
using DataTypes.Enums;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.IO;
using System.ComponentModel;
using JoeBlogs;

namespace Scraping
{
    /// <summary>
    /// Interaction logic for EditProject.xaml
    /// </summary>
    public partial class EditProject : Window
    {
        #region variables

        private string _projectName;
        private string _mode = "loading";

        #endregion

        #region Constructors

        public EditProject(Window owner, string projectName)
        {
            InitializeComponent();
            _projectName = projectName;
            this.Loaded += new RoutedEventHandler(EditProjectWindow_Loaded);
            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }            
        }

        private void EditProjectWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitaliziedPostSites();

            //setup Post instance's controls
            AutoPost.Instance.SetupWindow(this);
            AutoPost.Instance.SetupGoogleDriveTestConnectionLabel(lblGDriveConnectStatus);
            AutoPost.Instance.SetupGoogleDriveUsageBar(prbUsage);
            //setup Post instance's actions
            AutoPost.Instance.SetupGoogleDriveTestConnectionAction(displayTestConnectionToGoogleDrive);
            AutoPost.Instance.SetupClearProjectSelectedPostSiteAction(clearProjectSelectedPostSite);
            AutoPost.Instance.SetupDisplayProjectSelectedPostSitesAction(displayProjectSelectedPostSites);
            AutoPost.Instance.SetupUpdateGoogleDriveUsageAction(updateGoogleDriveUsage);
            PostingProject currentProject = SysSettings.Instance.GetProject(_projectName);

            //Load all settings of current project
            tbClientID.Text = currentProject.GoogleClientId;
            tbClientKey.Password = currentProject.GoogleClientSecret;
            tbApplicationName.Text = currentProject.GoogleApplicationName;

            BrushConverter bc = new BrushConverter();
            //reset connection status
            lblGDriveConnectStatus.Content = "Pending!";
            lblGDriveConnectStatus.Background = (Brush)bc.ConvertFrom("Gray");

            List<System.Windows.Controls.DockPanel> dps = Utilities.Helper.FindVisualChildren<DockPanel>(this.spPostingSites).ToList();

            AutoPost.Instance.Site_DockPanel = dps;
            AutoPost.Instance.ChangeProject(currentProject);
            _mode = "editing";
        }        

        /// <summary>
        /// Load Post Sites
        /// </summary>
        private void InitaliziedPostSites()
        {
            ThicknessConverter tc = new ThicknessConverter();
            BrushConverter bc = new BrushConverter();
            int count = 1;
            foreach (var i in SysSettings.Instance.Article_Posting_Sites)
            {
                //create new dockpanel                
                System.Windows.Controls.DockPanel dp = new DockPanel();
                dp.Name = "dp_postsite_" + count;
                dp.Height = 30;
                dp.Margin = (Thickness)tc.ConvertFrom("0,0,3,5");

                //create checkbox
                System.Windows.Controls.CheckBox cb = new System.Windows.Controls.CheckBox { Content = i.Host, IsChecked = i.Chosen };
                cb.Checked += new RoutedEventHandler(CheckboxPostSites_CheckedChange);
                cb.Unchecked += new RoutedEventHandler(CheckboxPostSites_CheckedChange);

                //create label

                System.Windows.Controls.Label lbl = new System.Windows.Controls.Label
                {
                    Content = i.GetConnectStatus(),                    
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Foreground = (Brush)bc.ConvertFrom("Black"),
                    Margin = (Thickness)tc.ConvertFrom("5,0,0,0"),
                    Width = 80
                };

                switch (i.Connect)
                {
                    case 1:
                        lbl.Background = (Brush)bc.ConvertFrom("Green");
                        break;
                    case -1:
                        lbl.Background = (Brush)bc.ConvertFrom("Red");
                        break;
                    default:
                        lbl.Background = (Brush)bc.ConvertFrom("Gray");
                        break;
                };

                dp.Children.Add(cb);
                dp.Children.Add(lbl);

                spPostingSites.Children.Add(dp);
                count++;
            }           
        }
       
        #endregion

        #region EventHandler

        /// <summary>
        /// Handler ScrollViewer PreviewMouseWheel event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        /// <summary>
        /// Handler Close button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }

        /// <summary>
        /// Handler Drag&Drop Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Handler PostSite checkboxes checkedChange event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckboxPostSites_CheckedChange(object sender, RoutedEventArgs e)
        {
            if (_mode == "editing")
            {
                CheckBox cb = sender as CheckBox;
                SysSettings.Instance.Article_Posting_Sites.Where(a => a.Host == cb.Content.ToString()).First().Chosen = Convert.ToBoolean(cb.IsChecked);
                
                if (cb.IsChecked == true)
                {
                    AutoPost.Instance.CurrentProject.LoadSite(SysSettings.Instance.GetSite(cb.Content.ToString()));
                }
                else
                {
                    AutoPost.Instance.CurrentProject.RemoveSite(cb.Content.ToString());
                }
            }
        }

        /// <summary>
        /// Handler TesGDrive button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestGDrive_Click(object sender, RoutedEventArgs e)
        {
            AutoPost.Instance.CurrentProject.UpdateGoogleCredentials(tbClientID.Text, tbClientKey.Password, tbApplicationName.Text);
            AutoPost.Instance.TestConnect(TestType.TestGoogleDrive);
        }

        /// <summary>
        /// Handler TestConnect button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestConnect_Click(object sender, RoutedEventArgs e)
        {
            //List<System.Windows.Controls.DockPanel> dps = Utilities.Helper.FindVisualChildren<DockPanel>(this.spPostingSites).ToList();

            //AutoPost.Instance.Site_DockPanel = dps;
            //AutoPost.Instance.TestConnect(TestType.TestSite);
        }

        /// <summary>
        /// Handler Add button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //List<System.Windows.Controls.TextBox> tbs = new List<System.Windows.Controls.TextBox>();
            //tbs.Add(tbURL);
            //tbs.Add(tbUsername);

            //bool isPass = true;

            //List<Tuple<string, bool, string>> results = AutoPost.Instance.ValidateSiteInformation(tbs, tbPassword, ref isPass);

            //if (isPass)
            //{
            //    PostSites site = new PostSites(tbURL.Text, tbUsername.Text, StringCipher.Encrypt(tbPassword.Password, SysSettings.Instance.EncryptKey));
            //    site.DecryptPassword = tbPassword.Password;
            //    if (AutoPost.Instance.InsertToPostSites(site))
            //    {
            //        //insert new site to interface
            //        InsertPostSites(site);

            //        //check whether or not user selected a project
            //        if (AutoPost.Instance.CheckIfSelectedProject())
            //        {
            //            AutoPost.Instance.CurrentProject.LoadSite(site);
            //        }
            //    }
            //    else
            //    {
            //        //raise error
            //        ErrorMessage viewer = new ErrorMessage(new List<string> { "This site has been already added." }, this);
            //        viewer.ShowDialog();
            //    }
            //}
            //else
            //{
            //    displayValidateAddPostSitesTextBox.Invoke(this, tbs, tbPassword, results);
            //    List<string> errorMessages = new List<string>();
            //    foreach (var i in results)
            //    {
            //        if (!i.Item2)
            //            errorMessages.Add(i.Item3);
            //    }
            //    ErrorMessage viewer = new ErrorMessage(errorMessages, this);
            //    viewer.ShowDialog();
            //}
        }

        /// <summary>
        /// Handler SetupSourceToSites button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetupSourceToSites_Click(object sender, RoutedEventArgs e)
        {
            SetupSourceToSite viewer = new SetupSourceToSite(this, ViewMode.Editable);
            viewer.ShowDialog();
        }

        /// <summary>
        /// Handler SetupLinksToSites button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetupSeoToSites_Click(object sender, RoutedEventArgs e)
        {
            SetupSeoToSite viewer = new SetupSeoToSite(this, ViewMode.Editable);
            viewer.ShowDialog();
        }

        /// <summary>
        /// Handler SetupScheduleToSites button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetupScheduleToSites_Click(object sender, RoutedEventArgs e)
        {
            SetupScheduleToSite viewer = new SetupScheduleToSite(this);
            viewer.Show();
        }

        #endregion

        #region Action
        
        /// <summary>
        /// Display connection result of google drive
        /// </summary>
        Action<Window, System.Windows.Controls.Label, bool> displayTestConnectionToGoogleDrive = (wd, lbl, flag) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                BrushConverter bc = new BrushConverter();

                if (flag)
                {
                    lbl.Content = "Connected!";
                    lbl.Background = (Brush)bc.ConvertFrom("Green");
                }
                else
                {
                    lbl.Content = "Failed!";
                    lbl.Background = (Brush)bc.ConvertFrom("Red");
                }
            }));
        };

        /// <summary>
        /// Clear all of these post sites of current project
        /// </summary>
        Action<Window, List<System.Windows.Controls.DockPanel>> clearProjectSelectedPostSite = (wd, dps) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var dp in dps)
                {
                    System.Windows.Controls.CheckBox cb = Utilities.Helper.FindVisualChildren<System.Windows.Controls.CheckBox>(dp).ToList().First();
                    cb.IsChecked = false;
                }
            }));
        };

        /// <summary>
        /// Display again selected post sites
        /// </summary>
        Action<Window, List<System.Windows.Controls.DockPanel>, string, bool> displayProjectSelectedPostSites = (wd, dps, host, flag) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var dp in dps)
                {
                    System.Windows.Controls.CheckBox cb = Utilities.Helper.FindVisualChildren<System.Windows.Controls.CheckBox>(dp).ToList().First();
                    if (cb.Content.ToString() == host)
                    {
                        cb.IsChecked = flag ? true : false;
                        break;
                    }
                }
            }));
        };

        Action<Window, ProgressBar, double?> updateGoogleDriveUsage = (wd, prg, value) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                prg.Value = (double)value;
            }));
        };

        #endregion

        #region UtilityMethods

        /// <summary>
        /// Insert new post sites
        /// </summary>
        /// <param name="newSite"></param>
        private void InsertPostSites(PostSites newSite)
        {
            ThicknessConverter tc = new ThicknessConverter();
            BrushConverter bc = new BrushConverter();

            //create new dockpanel                
            System.Windows.Controls.DockPanel dp = new DockPanel();
            //dp.Name = new Uri(newSite.Host).Host;
            dp.Height = 30;
            dp.Margin = (Thickness)tc.ConvertFrom("0,0,3,5");

            //create checkbox
            System.Windows.Controls.CheckBox cb = new System.Windows.Controls.CheckBox { Content = newSite.Host, IsChecked = newSite.Chosen };
            cb.Checked += new RoutedEventHandler(CheckboxPostSites_CheckedChange);
            cb.Unchecked += new RoutedEventHandler(CheckboxPostSites_CheckedChange);

            //create label

            System.Windows.Controls.Label lbl = new System.Windows.Controls.Label
            {
                Content = newSite.GetConnectStatus(),
                Background = (Brush)bc.ConvertFrom("Gray"),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Foreground = (Brush)bc.ConvertFrom("Black"),
                Margin = (Thickness)tc.ConvertFrom("5,0,0,0"),
                Width = 80
            };
            dp.Children.Add(cb);
            dp.Children.Add(lbl);

            spPostingSites.Children.Add(dp);
        }

        #endregion

    }
}
