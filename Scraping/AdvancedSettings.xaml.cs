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
using DataTypes.Collections;
using DataTypes.Enums;
using DataTypes.Interfaces;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.IO;
using System.ComponentModel;

namespace Scraping
{
    /// <summary>
    /// Interaction logic for AdvancedSettings.xaml
    /// </summary>
    public partial class AdvancedSettings : Window
    {
        #region variables

        /// <summary>
        /// Advanced options interfaces
        /// </summary>
        private static BrushConverter bc = new BrushConverter();
        private static ThicknessConverter tc = new ThicknessConverter();
        private static FontWeightConverter fwc = new FontWeightConverter();

        /// <summary>
        /// Advance settings variables
        /// </summary>        
        private bool _tempAutoIndex = false;

        #endregion

        #region constructors

        public AdvancedSettings(Window owner)
        {
            InitializeComponent();

            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }

            InitializeIndexerServices();
        }

        /// <summary>
        /// Initialize indexer services
        /// </summary>
        private void InitializeIndexerServices()
        {
            //load indexer service chosen
            if (IndexerHandler.Instance._autoindex)
                rbIndexerServicesAccept.IsChecked = true;
            else
                rbIndexerServicesDenied.IsChecked = true;

            //load list of services
            foreach (var i in IndexerHandler.Instance.IndexerServices)
            {
                i._currentWindow = this;
                i.updateStatus = updateIndexerServiceStatus;

                System.Windows.Controls.DockPanel dp = new DockPanel();

                dp.Tag = i.ServiceName;
                dp.MouseLeftButtonDown += IndexerDockPanel_MouseLeftButtonDown;
                dp.Height = 30;
                dp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                dp.Margin = (Thickness)tc.ConvertFrom("0,0,0,1");

                //create indexer service's name label
                System.Windows.Controls.Label lblServiceName = new System.Windows.Controls.Label
                {
                    Content = i.ServiceName,
                    Foreground = (Brush)bc.ConvertFrom("Gray"),
                    Width = 400
                };

                //create indexer service's status label
                System.Windows.Controls.Label lblServiceStatus = new System.Windows.Controls.Label
                {
                    Content = i.GetChosenStatus(),
                    Foreground = (Brush)bc.ConvertFrom("Black"),
                    Width = 150,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center
                };
                i._statusLabel = lblServiceStatus;

                if (i.Chosen)
                    lblServiceStatus.Background = (Brush)bc.ConvertFrom("Green");
                else
                    lblServiceStatus.Background = (Brush)bc.ConvertFrom("#be2f2f");

                dp.Children.Add(lblServiceName);
                dp.Children.Add(lblServiceStatus);
                sp_ListIndexerServices.Children.Add(dp);
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
        /// ScrollViewer PreviewMouseWheel handler event
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
            HandleMessageBox.Instance.GetConfirmation(SystemMessage.CloseWarning);
            if (HandleMessageBox.Instance.Answer)
            {
                IndexerHandler.Instance.UpdateAutoIndex(_tempAutoIndex);
                this.Close();
            }
        }

        /// <summary>
        /// Radio Indexer Yes/No checked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbIndexer_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton ck = sender as RadioButton;
            if (ck.IsChecked.Value)
            {
                switch (ck.Name)
                {
                    case "rbIndexerServicesAccept":
                        dp_IndexerServices_Details.Visibility = System.Windows.Visibility.Visible;
                        _tempAutoIndex = true;
                        break;
                    case "rbIndexerServicesDenied":
                        dp_IndexerServices_Details.Visibility = System.Windows.Visibility.Hidden;
                        _tempAutoIndex = false;
                        break;
                };
            }
        }

        /// <summary>
        /// Dockpanel Indexer mouse left button down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndexerDockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DockPanel dp = (DockPanel)sender;
                ToggleSelectedIndexerServiceDisplay.Invoke(this, sp_ListIndexerServices, dp);
            }
            else
            {
                DockPanel dp = (DockPanel)sender;
                ToggleSelectedIndexerServiceDisplay.Invoke(this, sp_ListIndexerServices, dp);

                IndexerServiceEditor viewer = new IndexerServiceEditor(this, dp.Tag.ToString());
                viewer.ShowDialog();

                if (viewer.Result)
                {
                    sp_ListIndexerServices.Children.Clear();
                    InitializeIndexerServices();
                }
            }
        }

        #endregion

        #region ActionList

        Action<Window, System.Windows.Controls.Label, bool> updateIndexerServiceStatus = (wd, lbl, state) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {                
                if (state)
                {
                    lbl.Content = "Chosen!";
                    lbl.Background = (Brush)bc.ConvertFrom("Green");                    
                }
                else
                {
                    lbl.Content = "None!";
                    lbl.Background = (Brush)bc.ConvertFrom("#be2f2f");
                }
            }));
        };

        Action<Window, StackPanel, DockPanel> ToggleSelectedIndexerServiceDisplay = (wd, sp, dp) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (DockPanel i in Utilities.Helper.FindVisualChildren<DockPanel>(sp))
                {
                    i.Background = i == dp ? (Brush)bc.ConvertFrom("#464545") : (Brush)bc.ConvertFrom("Black");
                }
            }));
        };

        #endregion

        #region UtilityMethods



        #endregion
    }
}
