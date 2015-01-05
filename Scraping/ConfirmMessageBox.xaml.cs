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

namespace Scraping
{
    /// <summary>
    /// Interaction logic for ConfirmMessageBox.xaml
    /// </summary>
    public partial class ConfirmMessageBox : Window
    {
        #region variables

        private bool _answer;

        public bool Answer
        {
            get { return _answer; }
            set { _answer = value; }
        }
        
        #endregion

        #region Constructors

        public ConfirmMessageBox(Window owner, SystemMessage message)
        {
            InitializeComponent();

            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            lblMessage.Content = message.ToString();   
        }

        #endregion
        
        #region EventHandler

        /// <summary>
        /// Handler OK button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            _answer = true;
            this.Close();
        }

        /// <summary>
        /// Handler Cancel button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _answer = false;
            this.Close();
        }

        /// <summary>
        /// Handler StopNotify Checked changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbStopNotify_CheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsChecked.Value)
                SysSettings.Instance.IsStopNotify = true;
            else
                SysSettings.Instance.IsStopNotify = false;
        }

        #endregion
        
        #region UtilityMethods

        #endregion
    }
}
