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
using DataTypes.Collections;
using DataTypes.Enums;
using Utilities;

namespace Scraping
{
    /// <summary>
    /// Interaction logic for PostSiteEditor.xaml
    /// </summary>
    public partial class PostSiteEditor : Window
    {
        #region variable

        private OpenMode _mode;

        public OpenMode Mode
        {
            get { return _mode; }
        }
        private PostSites _tempSite;

        private PostSiteType _tempPostSiteType = PostSiteType.Wordpress;
        private string _tempHost;
        
        private string _tempUsername;
        private string _tempPassword;
        private TimeZoneInfo _tempTimeZone = TimeZoneInfo.Utc;

        private string _originalHost;

        public string OriginalHost
        {
            get { return _originalHost; }            
        }

        private bool _result;

        public bool Result
        {
            get { return _result; }
        }

        private string _newPostSite;

        public string NewPostSite
        {
            get { return _newPostSite; }            
        }

        #endregion

        #region constructors

        public PostSiteEditor(Window owner, OpenMode mode, string postSite)
        {
            InitializeComponent();
            InitializeTimeZone();
            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            _mode = mode;

            if (_mode == OpenMode.Edit)
                InitializePostSite(postSite);
        }

        /// <summary>
        /// Load all system timeZone
        /// </summary>
        private void InitializeTimeZone()
        {
            foreach (TimeZoneInfo info in TimeZoneInfo.GetSystemTimeZones())
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = info.Id;
                item.Content = info.DisplayName;

                cbTimeZoneOfPostSite.Items.Add(item);
            }
        }

        /// <summary>
        /// Load current post site settings
        /// </summary>
        private void InitializePostSite(string postSite)
        {
            foreach (var site in SysSettings.Instance.Article_Posting_Sites)
            {
                if (site.Host == postSite)
                    _tempSite = site;
            }

            //Load settings
            tbHost.Text = _tempHost = _originalHost = _tempSite.Host;
            tbUsername.Text = _tempUsername = _tempSite.Username;
            tbStatus.Text = _tempSite.Status;
            pbPassword.Password = _tempPassword = _tempSite.DecryptPassword;
            LoadPostSiteTypeComboBox(_tempSite.Type);
            LoadTimeZoneInfoComboBox(_tempSite.TimeZone);
        }

        /// <summary>
        /// Load PostSiteType ComboBox based on current type
        /// </summary>
        /// <param name="type"></param>
        private void LoadPostSiteTypeComboBox(PostSiteType type)
        {
            foreach (ComboBoxItem item in cbTypeOfPostSite.Items)
            {
                if (item.Tag.ToString() == type.ToString())
                    cbTypeOfPostSite.SelectedItem = item;
            }
        }

        /// <summary>
        /// Load TimeZoneInfo ComboBox based on current timeZoneInfo
        /// </summary>
        /// <param name="info"></param>
        private void LoadTimeZoneInfoComboBox(TimeZoneInfo info)
        {
            foreach (ComboBoxItem item in cbTimeZoneOfPostSite.Items)
            {
                if (item.Tag.ToString() == info.Id)
                    cbTimeZoneOfPostSite.SelectedItem = item;
            }
        }

        #endregion

        #region EventHandler

        /// <summary>
        /// Handler Header drag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Handler Close button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            HandleMessageBox.Instance.GetConfirmation(SystemMessage.CloseWarning);
            if (HandleMessageBox.Instance.Answer)
                this.Close();
        }

        /// <summary>
        /// Handler TypeOfPostSite combobox selectionChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTypeOfPostSite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)cbTypeOfPostSite.SelectedItem;

            _tempPostSiteType = Helper.Parse<PostSiteType>(item.Tag.ToString()) ?? PostSiteType.Wordpress;
        }

        /// <summary>
        /// Handler TimeOfZonePostSite comboBox selectionChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTimeZoneOfPostSite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)cbTimeZoneOfPostSite.SelectedItem;

            _tempTimeZone = Helper.GetTimeZoneInfo(item.Tag.ToString());
        }

        /// <summary>
        /// Handler Save button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            List<System.Windows.Controls.TextBox> tbs = new List<System.Windows.Controls.TextBox>();
            tbs.Add(tbHost);
            tbs.Add(tbUsername);

            bool isPass = true;

            List<Tuple<string, bool, string>> results = AutoPost.Instance.ValidateSiteInformation(tbs, pbPassword, ref isPass);

            if (_mode == OpenMode.Add)
            {
                if (isPass)
                {
                    PostSites site = new PostSites(tbHost.Text, tbUsername.Text, StringCipher.Encrypt(pbPassword.Password, SysSettings.Instance.EncryptKey), _tempPostSiteType, _tempTimeZone);
                    site.DecryptPassword = pbPassword.Password;
                    if (AutoPost.Instance.InsertToPostSites(site))
                    {
                        //insert new site to interface
                        _result = true;
                        _newPostSite = tbHost.Text;
                        this.Close();
                    }
                    else
                    {
                        //raise error
                        ErrorMessage viewer = new ErrorMessage(new List<string> { "This site has been already added." }, this);
                        viewer.ShowDialog();
                    }
                }
                else
                {
                    displayValidateAddPostSitesTextBox.Invoke(this, tbs, pbPassword, results);
                    List<string> errorMessages = new List<string>();
                    foreach (var i in results)
                    {
                        if (!i.Item2)
                            errorMessages.Add(i.Item3);
                    }
                    ErrorMessage viewer = new ErrorMessage(errorMessages, this);
                    viewer.ShowDialog();
                }
            }
            else
            {
                if (isPass)
                {
                    PostSites site = new PostSites(tbHost.Text, tbUsername.Text, StringCipher.Encrypt(pbPassword.Password, SysSettings.Instance.EncryptKey), _tempPostSiteType, _tempTimeZone);
                    site.DecryptPassword = pbPassword.Password;

                    SysSettings.Instance.UpdateSite(_originalHost, site);
                    _newPostSite = tbHost.Text;
                    _result = true;
                    this.Close();
                }
                else
                {
                    displayValidateAddPostSitesTextBox.Invoke(this, tbs, pbPassword, results);
                    List<string> errorMessages = new List<string>();
                    foreach (var i in results)
                    {
                        if (!i.Item2)
                            errorMessages.Add(i.Item3);
                    }
                    ErrorMessage viewer = new ErrorMessage(errorMessages, this);
                    viewer.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Handler Cancel button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            HandleMessageBox.Instance.GetConfirmation(SystemMessage.CloseWarning);
            if (HandleMessageBox.Instance.Answer)
                this.Close();
        }

        #endregion

        #region Action

        /// <summary>
        /// Validate Add PostSite textbox
        /// </summary>
        Action<Window, List<System.Windows.Controls.TextBox>, PasswordBox, List<Tuple<string, bool, string>>> displayValidateAddPostSitesTextBox = (wd, tbs, pb, tups) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                BrushConverter bc = new BrushConverter();
                ThicknessConverter tc = new ThicknessConverter();
                foreach (var item in tups)
                {
                    if (item.Item1 != "pbPassword")
                    {
                        if (item.Item2)
                        {
                            tbs.Where(a => a.Name == item.Item1).First().ClearValue(Border.BorderBrushProperty);
                            tbs.Where(a => a.Name == item.Item1).First().ClearValue(Border.BorderThicknessProperty);
                        }
                        else
                        {
                            tbs.Where(a => a.Name == item.Item1).First().BorderBrush = (Brush)bc.ConvertFrom("Red");
                            tbs.Where(a => a.Name == item.Item1).First().BorderThickness = (Thickness)tc.ConvertFrom("1");
                        }
                    }
                    else
                    {
                        if (item.Item2)
                        {
                            pb.ClearValue(Border.BorderBrushProperty);
                            pb.ClearValue(Border.BorderThicknessProperty);
                        }
                        else
                        {
                            pb.BorderBrush = (Brush)bc.ConvertFrom("Red");
                            pb.BorderThickness = (Thickness)tc.ConvertFrom("1");
                        }
                    }
                }
            }));
        };

        #endregion
        
        #region UtilityMethods

        #endregion
    }
}
