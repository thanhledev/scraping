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
    /// Interaction logic for IndexerServiceEditor.xaml
    /// </summary>
    public partial class IndexerServiceEditor : Window
    {
        #region variables
        
        private IndexerService _tempService;        
        private bool _tempServiceChosen;

        private bool _result = false;

        public bool Result
        {
            get { return _result; }
        }

        #endregion

        #region constructors

        public IndexerServiceEditor(Window owner, string IndexerServiceName)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            InitializeService(IndexerServiceName);
        }

        private void InitializeService(string name)
        {
            foreach (var service in IndexerHandler.Instance.IndexerServices)
            {
                if (service.ServiceName == name)
                    _tempService = service;
            }

            //load to interface
            tbServiceName.Text = _tempService.ServiceName;
            tbServiceAPI.Text = _tempService.ServiceAPI;
            tbServiceKey.Text = _tempService.DecryptPassword;
            LoadServiceChosenComboBox(_tempService.Chosen);
        }

        private void LoadServiceChosenComboBox(bool chosen)
        {
            string value = chosen ? "1" : "0";

            foreach (ComboBoxItem item in cbServiceChosen.Items)
            {
                if (item.Tag.ToString() == value.ToString())
                    cbServiceChosen.SelectedItem = item;
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
        /// Handler ServiceChosen combox selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbServiceChosen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)cbServiceChosen.SelectedItem;

            if (item.Tag.ToString() == "1")
                _tempServiceChosen = true;
            else
                _tempServiceChosen = false;
        }

        /// <summary>
        /// Handler Save button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _tempService.ServiceAPI = tbServiceAPI.Text.Trim();
            _tempService.DecryptPassword = tbServiceKey.Text.Trim();
            _tempService.Chosen = _tempServiceChosen;

            IndexerHandler.Instance.UpdateIndexerService(_tempService);
            _result = true;
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

        #endregion        

        #region ActionList



        #endregion

        #region UtilityMethods



        #endregion
    }
}
