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
    /// Interaction logic for VnsSiteEditor.xaml
    /// </summary>
    public partial class VnsSiteEditor : Window
    {
        #region variables

        private OpenMode _mode;

        public OpenMode Mode
        {
            get { return _mode; }
        }

        private VirtualNewspapersSite _tempSite;
        private string _tempHost;

        private string _tempUsername;
        private string _tempPassword;

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

        private string _newVnsSite;

        public string NewVnsSite
        {
            get { return _newVnsSite; }
        }

        #endregion

        #region constructors

        public VnsSiteEditor(Window owner, OpenMode mode, string vnsSite)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            _mode = mode;

            if (_mode == OpenMode.Edit)
                InitializeVnsSite(vnsSite);
        }

        /// <summary>
        /// Load current post site settings
        /// </summary>
        private void InitializeVnsSite(string vnsSite)
        {
            foreach (var site in SysSettings.Instance.Article_VNS_Sites)
            {
                if (site.Host == vnsSite)
                    _tempSite = site;
            }

            //load settings
            tbHost.Text = _tempHost = _originalHost = _tempSite.Host;
            tbUsername.Text = _tempUsername = _tempSite.Username;
            tbStatus.Text = _tempSite.Status;
            pbPassword.Password = _tempPassword = _tempSite.DecryptPassword;
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

            List<Tuple<string, bool, string>> results = VirtualNewspapers.Instance.ValidateSiteInformation(tbs, pbPassword, ref isPass);

            if (_mode == OpenMode.Add)
            {
                if (isPass)
                {
                    VirtualNewspapersSite site = new VirtualNewspapersSite(tbHost.Text, tbUsername.Text, StringCipher.Encrypt(pbPassword.Password, SysSettings.Instance.EncryptKey));
                    site.DecryptPassword = pbPassword.Password;
                    if (VirtualNewspapers.Instance.InsertToVnsSites(site))
                    {
                        _result = true;
                        _newVnsSite = tbHost.Text;
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
                    displayValidateAddVnsSitesTextBox.Invoke(this, tbs, pbPassword, results);
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
                    VirtualNewspapersSite site = new VirtualNewspapersSite(tbHost.Text, tbUsername.Text, StringCipher.Encrypt(pbPassword.Password, SysSettings.Instance.EncryptKey));
                    site.DecryptPassword = pbPassword.Password;

                    SysSettings.Instance.UpdateVnsSite(_originalHost, site);
                    _newVnsSite = tbHost.Text;
                    _result = true;
                    this.Close();
                }
                else
                {
                    displayValidateAddVnsSitesTextBox.Invoke(this, tbs, pbPassword, results);
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

        #endregion        
   
        #region Action

        /// <summary>
        /// Validate Add PostSite textbox
        /// </summary>
        Action<Window, List<System.Windows.Controls.TextBox>, PasswordBox, List<Tuple<string, bool, string>>> displayValidateAddVnsSitesTextBox = (wd, tbs, pb, tups) =>
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
