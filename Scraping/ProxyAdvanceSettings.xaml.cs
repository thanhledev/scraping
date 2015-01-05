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
    /// Interaction logic for ProxyAdvanceSettings.xaml
    /// </summary>
    public partial class ProxyAdvanceSettings : Window
    {
        #region variables

        //for temporary variables
        private int _tempAutoSearchProxyInterval = 0;

        #endregion

        #region constructors

        public ProxyAdvanceSettings(Window owner)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            InitializeAutoScrapeProxy();
            InitializeUsingSettings();
        }

        #endregion        

        #region InitializeWindows

        private void InitializeAutoScrapeProxy()
        {
            chbAutoSearchProxy.IsChecked = ProxyHandler.Instance.AutoSearchProxy;

            foreach (ComboBoxItem item in cbSearchProxyInterval.Items)
            {
                if (item.Tag.ToString() == ProxyHandler.Instance.SearchProxyInterval.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            chbTestProxies.IsChecked = ProxyHandler.Instance.TestProxy;
            chbCheckAnonymous.IsChecked = ProxyHandler.Instance.CheckAnonymous;
            tbAnonymousCheckSite.Text = ProxyHandler.Instance.CheckAnonymousLink;
            tbProxyThread.Text = ProxyHandler.Instance.Threads.ToString();

            int seconds = ProxyHandler.Instance.TimeOut / 1000;

            tbProxyTimeout.Text = seconds.ToString();
        }

        private void InitializeUsingSettings()
        {
            _tempAutoSearchProxyInterval = ProxyHandler.Instance.SearchProxyInterval;

            foreach (ComboBoxItem item in cbSearchProxyInterval.Items)
            {
                if (item.Tag.ToString() == _tempAutoSearchProxyInterval.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem item in cbScrapeHarvestLink.Items)
            {
                if (item.Tag.ToString() == ProxyHandler.Instance.ScrapeHarvestLinks.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem item in cbScrapeDownloadArticle.Items)
            {
                if (item.Tag.ToString() == ProxyHandler.Instance.ScrapeDownloadArticles.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem item in cbPostingHarvestLink.Items)
            {
                if (item.Tag.ToString() == ProxyHandler.Instance.PostingHarvestLinks.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem item in cbPostingDownloadArticle.Items)
            {
                if (item.Tag.ToString() == ProxyHandler.Instance.PostingDownloadArticles.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem item in cbVNSDownloadArticle.Items)
            {
                if (item.Tag.ToString() == ProxyHandler.Instance.VnsDownloadArticles.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
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
            this.Close();           
        }

        /// <summary>
        /// Handler scrollViewer mouse wheel event
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
        /// Handler mouse left button click down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Handler AutoSearchProxy checkbox checkedchanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbAutoSearchProxy_Checked(object sender, RoutedEventArgs e)
        {
            ProxyHandler.Instance.AutoSearchProxy = Convert.ToBoolean(chbAutoSearchProxy.IsChecked);
            ProxyHandler.Instance.SwitchManager(ProxyHandler.Instance.AutoSearchProxy);
        }

        /// <summary>
        /// Handler AutoSearchProxy checkbox uncheckedchanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbAutoSearchProxy_Unchecked(object sender, RoutedEventArgs e)
        {
            ProxyHandler.Instance.AutoSearchProxy = Convert.ToBoolean(chbAutoSearchProxy.IsChecked);
            ProxyHandler.Instance.SwitchManager(ProxyHandler.Instance.AutoSearchProxy);
        }

        /// <summary>
        /// Handler SearchProxyInterval comboBox selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSearchProxyInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSearchProxyInterval.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbSearchProxyInterval.SelectedItem;
                ProxyHandler.Instance.SearchProxyInterval = Convert.ToInt32(selected.Tag.ToString());
            }
        }

        /// <summary>
        /// Handler TestProxies checkbox checkedchanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbTestProxies_Checked(object sender, RoutedEventArgs e)
        {
            ProxyHandler.Instance.TestProxy = Convert.ToBoolean(chbTestProxies.IsChecked);
        }

        /// <summary>
        /// Handler TestProxies checkbox uncheckedchanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbTestProxies_Unchecked(object sender, RoutedEventArgs e)
        {
            ProxyHandler.Instance.TestProxy = Convert.ToBoolean(chbTestProxies.IsChecked);
        }

        /// <summary>
        /// Handler CheckAnonymous checkbox checkedchanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbCheckAnonymous_Checked(object sender, RoutedEventArgs e)
        {
            ProxyHandler.Instance.CheckAnonymous = Convert.ToBoolean(chbCheckAnonymous.IsChecked);
        }

        /// <summary>
        /// Handler AnonymousCheckSite textbox lost focus event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbAnonymousCheckSite_LostFocus(object sender, RoutedEventArgs e)
        {
            ProxyHandler.Instance.CheckAnonymousLink = tbAnonymousCheckSite.Text;
        }

        /// <summary>
        /// Handler CheckAnonymous checkbox uncheckedchanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbCheckAnonymous_Unchecked(object sender, RoutedEventArgs e)
        {
            ProxyHandler.Instance.CheckAnonymous = Convert.ToBoolean(chbCheckAnonymous.IsChecked);
        }

        /// <summary>
        /// Handler ProxyThread textbox lost focus event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbProxyThread_LostFocus(object sender, RoutedEventArgs e)
        {
            ProxyHandler.Instance.Threads = Convert.ToInt32(tbProxyThread.Text);
        }

        /// <summary>
        /// Handler ProxyTimeout textbox lost focus event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbProxyTimeout_LostFocus(object sender, RoutedEventArgs e)
        {
            ProxyHandler.Instance.TimeOut = Convert.ToInt32(tbProxyTimeout.Text)*1000;
        }

        /// <summary>
        /// Handler ScrapeHarvestLink comboBox selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbScrapeHarvestLink_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbScrapeHarvestLink.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbScrapeHarvestLink.SelectedItem;
                ProxyHandler.Instance.ScrapeHarvestLinks = Utilities.Helper.Parse<ProxyType>(selected.Tag.ToString()) ?? ProxyType.PublicProxy;
            }
        }

        /// <summary>
        /// Handler ScrapeDownloadArticle comboBox selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbScrapeDownloadArticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbScrapeDownloadArticle.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbScrapeDownloadArticle.SelectedItem;
                ProxyHandler.Instance.ScrapeDownloadArticles = Utilities.Helper.Parse<ProxyType>(selected.Tag.ToString()) ?? ProxyType.PublicProxy;
            }
        }

        /// <summary>
        /// Handler PostingHarvestLink comboBox selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbPostingHarvestLink_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPostingHarvestLink.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbPostingHarvestLink.SelectedItem;
                ProxyHandler.Instance.PostingHarvestLinks = Utilities.Helper.Parse<ProxyType>(selected.Tag.ToString()) ?? ProxyType.PublicProxy;
            }
        }

        /// <summary>
        /// Handler PostingDownloadArticle comboBox selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbPostingDownloadArticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPostingDownloadArticle.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbPostingDownloadArticle.SelectedItem;
                ProxyHandler.Instance.PostingDownloadArticles = Utilities.Helper.Parse<ProxyType>(selected.Tag.ToString()) ?? ProxyType.PublicProxy;
            }
        }

        /// <summary>
        /// Handler VNSDownloadArticle comboBox selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbVNSDownloadArticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbVNSDownloadArticle.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbVNSDownloadArticle.SelectedItem;
                ProxyHandler.Instance.VnsDownloadArticles = Utilities.Helper.Parse<ProxyType>(selected.Tag.ToString()) ?? ProxyType.PublicProxy;
            }
        }

        #endregion   
   
        #region ActionList



        #endregion

        #region UtilityMethods



        #endregion
    }
}
