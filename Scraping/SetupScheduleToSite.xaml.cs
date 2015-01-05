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
    /// Interaction logic for SetupScheduleToSite.xaml
    /// </summary>
    public partial class SetupScheduleToSite : Window
    {
        #region variables

        private static BrushConverter bc = new BrushConverter();
        private static FontWeightConverter fwc = new FontWeightConverter();
        private static FontStyleConverter fsc = new FontStyleConverter();
        private static ThicknessConverter tc = new ThicknessConverter();

        //for temporary variables
        private ScheduleRule _tempScheduleRule;
        //private ScheduleMode _scheduleMode;
        //private TimeRange _postRange;
        //private TimeBetweenPost _timeBetweenPost;
        //private int _numberOfDays = 1;
        //private TimeUnit _timeUnit = TimeUnit.Minute;
        //private int _comeBackInterval = 30;
        //private int _searchDepth = 1;
        //private DateTime _startDate;
        //private DateTime _endDate;

        #endregion

        #region Constructors

        public SetupScheduleToSite(Window owner)
        {
            InitializeComponent();

            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            _tempScheduleRule = AutoPost.Instance.CurrentProject.ScheduleRule;
            InitializedInterface();
        }

        private void InitializedInterface()
        {
            //_scheduleMode = _tempScheduleRule.ScheduleMode;
            if (_tempScheduleRule.ScheduleMode == ScheduleMode.LiveFeed)
            {
                ToogleButtonStyle.Invoke(this, spScheduleButton, "btnLiveFeed");
                ToogleSettingInterfaces.Invoke(this, dp_btnLiveFeed, dp_btnRecords, "btnLiveFeed");
                ChangeTimeRange_Selection(_tempScheduleRule.TimeRange);
                ChangeNumberOfDaysLiveFeed_Selection(_tempScheduleRule.NumberOfDays);
            }
            else
            {
                ToogleButtonStyle.Invoke(this, spScheduleButton, "btnRecords");
                ToogleSettingInterfaces.Invoke(this, dp_btnLiveFeed, dp_btnRecords, "btnRecords");
                ChangeTimeRange_Selection(TimeRange.NumberOfDays);
                ChangeNumberOfDaysDatabaseRecords_Selection(_tempScheduleRule.NumberOfDays);                
            }

            if (_tempScheduleRule.StartDate != DateTime.MinValue)
                tbStartDate.Text = _tempScheduleRule.StartDate.ToShortDateString();
            if(_tempScheduleRule.EndDate != DateTime.MinValue)
                tbEndDate.Text = _tempScheduleRule.EndDate.ToShortDateString();

            ChangeTimeUnit_Selection(_tempScheduleRule.TimeUnit);
            ChangeTimeBetweenPost_Selection(_tempScheduleRule.TimeBetweenPost);
            ChangeComebackInterval_Selection(_tempScheduleRule.ComebackInterval);
            ChangeSearchDepth_Selection(_tempScheduleRule.SearchDepth);
            ChangeLimitation_Selection(_tempScheduleRule.Limitation);

            //change textbox value            
            tbTotalPost.Text = tbTotalPost1.Text = _tempScheduleRule.NumberOfPosts.ToString();
            tbMinRandomInterval.Text = _tempScheduleRule.MinInterval.ToString();
            tbMaxRandomInterval.Text = _tempScheduleRule.MaxInterval.ToString();
            tbLogoImagePath.Text = AutoPost.Instance.CurrentProject.LogoPath;
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

        private void ChangeSearchDepth_Selection(int searchDepth)
        {
            foreach (ComboBoxItem item in cbSearchDepth.Items)
            {
                if (item.Tag.ToString() == searchDepth.ToString())
                    cbSearchDepth.SelectedItem = item;
            }
        }

        private void ChangeLimitation_Selection(Limitation limit)
        {
            foreach (ComboBoxItem item in cbLimitation.Items)
            {
                if (item.Tag.ToString() == limit.ToString())
                    cbLimitation.SelectedItem = item;
            }
        }
                

        private void ChangeNumberOfDaysLiveFeed_Selection(int numberOfDays)
        {
            foreach (ComboBoxItem item in cbNumberOfDayChoices.Items)
            {
                if (item.Tag.ToString() == numberOfDays.ToString())
                    cbNumberOfDayChoices.SelectedItem = item;
            }
        }

        private void ChangeNumberOfDaysDatabaseRecords_Selection(int numberOfDays)
        {
            foreach (ComboBoxItem item in cbNumberOfDayChoices1.Items)
            {
                if (item.Tag.ToString() == numberOfDays.ToString())
                    cbNumberOfDayChoices1.SelectedItem = item;
            }
        }

        #endregion
        
        #region EventHandler

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
        /// Handler Drag&Drop Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Handler Schedule mode button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScheduleMode_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Name == "btnLiveFeed")
                _tempScheduleRule.ScheduleMode = ScheduleMode.LiveFeed;
            else
            {
                _tempScheduleRule.ScheduleMode = ScheduleMode.DatabaseRecords;
                ChangeTimeRange_Selection(TimeRange.NumberOfDays);
            }

            ToogleButtonStyle.Invoke(this, spScheduleButton, btn.Name);
            ToogleSettingInterfaces.Invoke(this, dp_btnLiveFeed, dp_btnRecords, btn.Name);
        }

        private void ChangeTimeRange_Selection(TimeRange range)
        {
            foreach (ComboBoxItem item in cbTimeRangeOptions.Items)
            {
                if (item.Tag.ToString() == range.ToString())
                    cbTimeRangeOptions.SelectedItem = item;
            }
        }

        /// <summary>
        /// Handler ComboBox TimeRange selectionchanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTimeRangeOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTimeRangeOptions.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbTimeRangeOptions.SelectedItem;

                _tempScheduleRule.TimeRange = Utilities.Helper.Parse<TimeRange>(selected.Tag.ToString()) ?? TimeRange.Daily;
                if (selected.Tag.ToString() == TimeRange.Daily.ToString())
                {
                    lblLasting.Visibility = System.Windows.Visibility.Hidden;
                    cbNumberOfDayChoices.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    lblLasting.Visibility = System.Windows.Visibility.Visible;
                    cbNumberOfDayChoices.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Handler ComboBox NumberOfDayChoices selectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberOfDayChoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)comboBox.SelectedItem;

                _tempScheduleRule.NumberOfDays = Int32.Parse(selected.Content.ToString());
            }
        }

        /// <summary>
        /// Handler textbox TotalPost Keydown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TotalPost_PreviewKeyDown(object sender, KeyEventArgs e)
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
        /// Handler TextBox MinRandomInterval KeyDown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbInterval_PreviewKeyDown(object sender, KeyEventArgs e)
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
        /// Handler Textbox MinRandomInterval lost focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbMinRandomInterval_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbMinRandomInterval.Text != string.Empty)
                _tempScheduleRule.MinInterval = Int32.Parse(tbMinRandomInterval.Text);
        }

        /// <summary>
        /// Handler Textbox MaxRandomInterval lost focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbMaxRandomInterval_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbMaxRandomInterval.Text != string.Empty)
                _tempScheduleRule.MaxInterval = Int32.Parse(tbMaxRandomInterval.Text);
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
        /// Handler CombBox ComebackInterval selectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbComebackInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbComebackInterval.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbComebackInterval.SelectedItem;

                _tempScheduleRule.ComebackInterval = Int32.Parse(selected.Tag.ToString());
            }
        }

        /// <summary>
        /// Handler ComboBox SearchDepth selectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSearchDepth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSearchDepth.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbSearchDepth.SelectedItem;

                _tempScheduleRule.SearchDepth = Int32.Parse(selected.Tag.ToString());
            }
        }

        /// <summary>
        /// Handler ComboBox Limitation selectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbLimitation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLimitation.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbLimitation.SelectedItem;

                _tempScheduleRule.Limitation = Utilities.Helper.Parse<Limitation>(selected.Tag.ToString()) ?? Limitation.Unlimited;
            }
        }

        /// <summary>
        /// Handler ViewSourceRules button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewSourceRules_Click(object sender, RoutedEventArgs e)
        {
            SetupSourceToSite viewer = new SetupSourceToSite(this, ViewMode.ViewOnly);
            viewer.ShowDialog();
        }

        /// <summary>
        /// Handler ViewSEORules button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewSEORuls_Click(object sender, RoutedEventArgs e)
        {
            SetupSeoToSite viewer = new SetupSeoToSite(this, ViewMode.ViewOnly);
            viewer.ShowDialog();
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
                //copy logo image to folder
                if (System.IO.File.Exists(AutoPost.Instance.CurrentProject.LogoPath))
                    System.IO.File.Delete(AutoPost.Instance.CurrentProject.LogoPath);
                System.IO.File.Copy(dlg.FileName, AutoPost.Instance.CurrentProject.ProjectPath + "\\" + logoFileName);                
                
                AutoPost.Instance.CurrentProject.LogoPath = AutoPost.Instance.CurrentProject.ProjectPath + "\\" + logoFileName;
                tbLogoImagePath.Text = AutoPost.Instance.CurrentProject.LogoPath;
            }
        }

        /// <summary>
        /// Handler StartDate textBox LostFocus event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbStartDate_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime temp = new DateTime();
            if (!DateTime.TryParse(tbStartDate.Text, out temp))
            {
                e.Handled = true;
                displayValidateTextBox.Invoke(this, (TextBox)sender, false);
            }
            else
            {
                _tempScheduleRule.StartDate = temp;
                displayValidateTextBox.Invoke(this, (TextBox)sender, true);
            }
        }

        /// <summary>
        /// Handler EndDate textBox LostFocus event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbEndDate_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime temp = new DateTime();
            if (!DateTime.TryParse(tbEndDate.Text, out temp))
            {
                e.Handled = true;
                displayValidateTextBox.Invoke(this, (TextBox)sender, false);
            }
            else
            {
                _tempScheduleRule.EndDate = temp;
                displayValidateTextBox.Invoke(this, (TextBox)sender, true);
            }
        }

        private void tbTotalPost_LostFocus(object sender, RoutedEventArgs e)
        {
            _tempScheduleRule.NumberOfPosts = Int32.Parse(tbTotalPost.Text);            
        }

        private void tbTotalPost1_LostFocus(object sender, RoutedEventArgs e)
        {
            _tempScheduleRule.NumberOfPosts = Int32.Parse(tbTotalPost1.Text);
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
                AutoPost.Instance.CurrentProject.ScheduleRule = _tempScheduleRule;

                //AutoPost.Instance.CurrentProject.searchPages = new List<string>();
                //List<SourceCategory> chosenCategories = new List<SourceCategory>();
                //foreach (var rule in AutoPost.Instance.CurrentProject.RuleCollection.Rules)
                //{
                //    foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                //    {
                //        SourceCategory check = source.FindCategory(source.Title, rule.SourceCategory);
                //        if (check != null)
                //            source.GetCategoryChildren(chosenCategories, check);
                //    }
                //}

                //foreach (var cate in chosenCategories)
                //{
                //    foreach (var source in SysSettings.Instance.Article_Scraping_ScrapingSources)
                //    {
                //        if (source.Title == cate.SourceHost)
                //        {
                //            ISpawning spawner = SpawnerFactory.CreateSpawner(source);
                //            spawner.CreateSearchPagesLiveFeed(source, AutoPost.Instance.CurrentProject.ScheduleRule.SearchDepth, cate, ref AutoPost.Instance.CurrentProject.searchPages);
                //        }
                //    }
                //}
            }
        }

        #endregion
                
        #region Action

        Action<Window, StackPanel, string> ToogleButtonStyle = (wd, sp, name) =>
            {
                wd.Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (Button btn in Utilities.Helper.FindVisualChildren<Button>(sp))
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

        Action<Window, DockPanel, DockPanel, string> ToogleSettingInterfaces = (wd, mode1, mode2, name) =>
            {
                wd.Dispatcher.Invoke(new Action(() =>
                    {
                        if (name == "btnLiveFeed")
                        {
                            mode1.Visibility = Visibility.Visible;
                            mode2.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            mode1.Visibility = Visibility.Hidden;
                            mode2.Visibility = Visibility.Visible;
                        }
                    }));
            };

        Action<Window, TextBox, bool> displayValidateTextBox = (wd, tb, flag) =>
            {
                wd.Dispatcher.Invoke(new Action(() =>
                    {
                        if (flag)
                        {
                            tb.ClearValue(Border.BorderBrushProperty);
                            tb.ClearValue(Border.BorderThicknessProperty);
                        }
                        else
                        {
                            tb.BorderBrush = (Brush)bc.ConvertFrom("Red");
                            tb.BorderThickness = (Thickness)tc.ConvertFrom("1");
                        }
                    }));
            };

        #endregion        

        #region UtilityMethods


        #endregion
    }
}
