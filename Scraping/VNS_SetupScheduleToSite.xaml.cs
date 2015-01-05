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
    /// Interaction logic for VNS_SetupScheduleToSite.xaml
    /// </summary>
    public partial class VNS_SetupScheduleToSite : Window
    {
        #region variables

        private static BrushConverter bc = new BrushConverter();
        private static FontWeightConverter fwc = new FontWeightConverter();
        private static FontStyleConverter fsc = new FontStyleConverter();
        private static ThicknessConverter tc = new ThicknessConverter();

        //for temporary variables
        private VirtualNewspapersScheduleRule _tempScheduleRule;

        #endregion

        #region constructors

        public VNS_SetupScheduleToSite(Window owner)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            _tempScheduleRule = VirtualNewspapers.Instance.CurrentProject.ScheduleRule;
            InitializedInterface();
        }

        private void InitializedInterface()
        {
            ChangeTimeUnit_Selection(_tempScheduleRule.TimeUnit);
            ChangeTimeBetweenPost_Selection(_tempScheduleRule.TimeBetweenPost);
            ChangeComebackInterval_Selection(_tempScheduleRule.ComebackInterval);

            tbNumberPostsPerDay.Text = _tempScheduleRule.NumberOfPostsPerDay.ToString();
            tbMinRandomInterval.Text = _tempScheduleRule.MinInterval.ToString();
            tbMaxRandomInterval.Text = _tempScheduleRule.MaxInterval.ToString();
            tbLogoImagePath.Text = VirtualNewspapers.Instance.CurrentProject.LogoPath;
        }

        private void ChangeTimeUnit_Selection(TimeUnit timeUnit)
        {
            foreach (ComboBoxItem item in cbTimeUnit.Items)
            {
                if (item.Tag.ToString() == timeUnit.ToString())
                    cbTimeUnit.SelectedItem = item;
            }
        }

        private void ChangeTimeBetweenPost_Selection(TimeBetweenPost timeBetweenPost)
        {
            foreach (ComboBoxItem item in cbTimeBetweenPost.Items)
            {
                if (item.Tag.ToString() == timeBetweenPost.ToString())
                    cbTimeBetweenPost.SelectedItem = item;
            }
        }

        private void ChangeComebackInterval_Selection(int comeBackInterval)
        {
            foreach (ComboBoxItem item in cbComebackInterval.Items)
            {
                if (item.Tag.ToString() == comeBackInterval.ToString())
                    cbComebackInterval.SelectedItem = item;
            }
        }

        #endregion
        
        #region EventHandler

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
        /// Handler Close button click
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
        /// Handler ComboBox TimeBetweenPost selectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTimeBetweenPost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTimeBetweenPost.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbTimeBetweenPost.SelectedItem;

                _tempScheduleRule.TimeBetweenPost = Utilities.Helper.Parse<TimeBetweenPost>(selected.Tag.ToString()) ?? TimeBetweenPost.Automatically;

                if (_tempScheduleRule.TimeBetweenPost == TimeBetweenPost.Automatically)
                {
                    dpManuallyTime.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    dpManuallyTime.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Handler ComboBox TimeUnit selectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTimeUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTimeUnit.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbTimeUnit.SelectedItem;

                _tempScheduleRule.TimeUnit = Utilities.Helper.Parse<TimeUnit>(selected.Tag.ToString()) ?? TimeUnit.Minute;
            }
        }

        /// <summary>
        /// Handler ChooseLogoImage button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChooseLogoImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "";
            dlg.Filter = "PNG|*.png|BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg";
            dlg.DefaultExt = "PNG|*.png";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                //get logo file name
                string logoFileName = System.IO.Path.GetFileName(dlg.FileName);

                if (System.IO.File.Exists(VirtualNewspapers.Instance.CurrentProject.LogoPath))
                    System.IO.File.Delete(VirtualNewspapers.Instance.CurrentProject.LogoPath);
                System.IO.File.Copy(dlg.FileName, VirtualNewspapers.Instance.CurrentProject.ProjectPath + "\\" + logoFileName);

                VirtualNewspapers.Instance.CurrentProject.LogoPath = VirtualNewspapers.Instance.CurrentProject.ProjectPath + "\\" + logoFileName;
                tbLogoImagePath.Text = VirtualNewspapers.Instance.CurrentProject.LogoPath;
            }
        }

        /// <summary>
        /// Handler NumberPostsPerDay textBox LostFocus event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbNumberPostsPerDay_LostFocus(object sender, RoutedEventArgs e)
        {
            _tempScheduleRule.NumberOfPostsPerDay = Int32.Parse(tbNumberPostsPerDay.Text);
        }

        #endregion

        /// <summary>
        /// Handler Numeric Textbox Keydown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key < Key.D0 || e.Key > Key.D9)
            {
                if (e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
                {
                    if (e.Key != Key.Back)
                        e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handler Save button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            HandleMessageBox.Instance.GetConfirmation(SystemMessage.CloseWarning);
            if (HandleMessageBox.Instance.Answer)
            {
                VirtualNewspapers.Instance.CurrentProject.ScheduleRule = _tempScheduleRule;
            }
        }

        /// <summary>
        /// Handler Reset button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Handler MinRandomInterval textBox LostFocus event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbMinRandomInterval_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbMinRandomInterval.Text != string.Empty)
                _tempScheduleRule.MinInterval = Int32.Parse(tbMinRandomInterval.Text);
        }

        /// <summary>
        /// Handler MaxRandomInterval textBox LostFocus event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbMaxRandomInterval_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbMaxRandomInterval.Text != string.Empty)
                _tempScheduleRule.MaxInterval = Int32.Parse(tbMaxRandomInterval.Text);
        }

        #region ActionList


        #endregion

        #region UtilityMethods



        #endregion
    }
}
