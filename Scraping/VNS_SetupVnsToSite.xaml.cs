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
    /// Interaction logic for SetupVnsToSite.xaml
    /// </summary>
    public partial class VNS_SetupVnsToSite : Window
    {
        #region variables

        private BrushConverter bc = new BrushConverter();
        private FontStyleConverter fsc = new FontStyleConverter();
        private List<string> _vnsDomains = new List<string>();
        private List<string> _postSiteDomains = new List<string>();
        Dictionary<TreeViewItem, string> selectedVnsCategories = new Dictionary<TreeViewItem, string>();
        Dictionary<TreeViewItem, string> selectedPostSiteCategories = new Dictionary<TreeViewItem, string>(); 
        private ViewMode _viewMode;

        //for temporary variables
        private VirtualNewspapersPostRulesCollection _tempPostRuleCollection;
        private PostSites _chosenSite;
        private List<Control> _editableObjects = new List<Control>();
        private Button _toogleHiddenButton;
        private bool _isSaved = false; 

        #endregion

        #region constructors

        public VNS_SetupVnsToSite(Window owner, ViewMode mode)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            _viewMode = mode;
            _tempPostRuleCollection = VirtualNewspapers.Instance.CurrentProject.RuleCollection;
            InitializeVNSTree();
            LoadSites();
            InitializeEditableObjects();
            switchingViewMode.Invoke(this, _editableObjects, _toogleHiddenButton, _viewMode);
        }

        #endregion
        
        #region InitializeWindows

        /// <summary>
        /// Set controls disabled based on the chosen ViewMode
        /// </summary>
        private void InitializeEditableObjects()
        {            
            _editableObjects.Add(trSelectedVnsSiteCategories);
            _editableObjects.Add(btnSelect);
            _editableObjects.Add(btnClear);
            _editableObjects.Add(btnSave);
            _editableObjects.Add(btnReset);

            _toogleHiddenButton = btnSwitchingViewMode;
        }

        /// <summary>
        /// Handler VNS tree
        /// </summary>        
        private void InitializeVNSTree()
        {
            foreach (var vns in SysSettings.Instance.Article_VNS_Sites)
            {
                _vnsDomains.Add(vns.Host);
                TreeViewItem root = new TreeViewItem();
                root.Header = vns.Host;
                root.Foreground = (Brush)bc.ConvertFrom("Gray");
                root.IsExpanded = true;
                trSelectedVnsSiteCategories.Items.Add(root);
                InitializeVNSTree(trSelectedVnsSiteCategories, 0, root, vns);
            }
        }

        private void InitializeVNSTree(TreeView tr, long parentId, TreeViewItem parentNode, VirtualNewspapersSite vns)
        {
            foreach (var cate in vns.Categories)
            {
                if (cate.Value == parentId)
                {
                    if (parentNode == null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = vns.Host + "|" + cate.Key.Name + "|" + cate.Key.CategoryID;
                        item.Foreground = (Brush)bc.ConvertFrom("Gray");
                        tr.Items.Add(item);
                        InitializeVNSTree(tr, cate.Key.CategoryID, item, vns);
                    }
                    else
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = vns.Host + "|" + cate.Key.Name + "|" + cate.Key.CategoryID;
                        item.Foreground = (Brush)bc.ConvertFrom("Gray");
                        parentNode.Items.Add(item);
                        InitializeVNSTree(tr, cate.Key.CategoryID, item, vns);
                    }
                }
            }
        }

        private void DeselectVnsCategory(TreeViewItem treeViewItem)
        {
            //treeViewItem.Background = Brushes.Black ;// change background and foreground colors            
            treeViewItem.Foreground = Brushes.Gray;
            treeViewItem.FontStyle = (FontStyle)fsc.ConvertFrom("Normal");
            selectedVnsCategories.Remove(treeViewItem);
        }

        /// <summary>
        /// changes the state of the tree item:
        /// selects it if it has not been selected and
        /// deselects it otherwise
        /// </summary>
        void ChangeSelectedStateVnsCategory(TreeViewItem treeViewItem)
        {
            if (!selectedVnsCategories.ContainsKey(treeViewItem))
            {
                //treeViewItem.Background = (Brush)bc.ConvertFrom("#be2f2f"); // change background and foreground colors                
                treeViewItem.Foreground = (Brush)bc.ConvertFrom("#be2f2f");
                treeViewItem.FontStyle = (FontStyle)fsc.ConvertFrom("Oblique");
                selectedVnsCategories.Add(treeViewItem, treeViewItem.Header.ToString());
            }
            else
            {
                DeselectVnsCategory(treeViewItem);
            }
        }

        /// <summary>
        /// Handler trSelectedVnsSiteCategories TreeView SelectedItemChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trSelectedVnsSiteCategories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem treeViewItem = trSelectedVnsSiteCategories.SelectedItem as TreeViewItem;

            if (treeViewItem == null)
                return;

            //prevent the WPF tree item selection
            treeViewItem.IsSelected = false;

            treeViewItem.Focus();

            if (!_vnsDomains.Contains(treeViewItem.Header.ToString()))
            {
                if (!CtrlPressed)
                {
                    List<TreeViewItem> selectedTreeViewList = new List<TreeViewItem>();
                    foreach (TreeViewItem treeViewItem1 in selectedVnsCategories.Keys)
                    {
                        selectedTreeViewList.Add(treeViewItem1);
                    }

                    foreach (TreeViewItem treeViewItem1 in selectedTreeViewList)
                    {
                        DeselectVnsCategory(treeViewItem1);
                    }
                }
                ChangeSelectedStateVnsCategory(treeViewItem);
            }
        }

        /// <summary>
        /// Handler PostSite Categories tree 
        /// </summary>        
        private void LoadSites()
        {
            foreach (var site in VirtualNewspapers.Instance.CurrentProject.PostSites)
            {
                _postSiteDomains.Add(site.Host);
                ComboBoxItem item = new ComboBoxItem();
                item.Content = site.Host;

                cbSelectPostSites.Items.Add(item);
            }
        }

        private void InitializePostSiteTree(PostSites site)
        {
            TreeViewItem root = new TreeViewItem();
            root.Header = site.Host;
            root.Foreground = (Brush)bc.ConvertFrom("Gray");
            root.IsExpanded = true;
            trSelectedPostSiteCategories.Items.Add(root);
            InitializePostSiteTree(trSelectedPostSiteCategories, 0, root, site.Categories);
        }

        private void InitializePostSiteTree(TreeView tr, long parent, TreeViewItem parentNode, Dictionary<Category, long> categories)
        {
            foreach (var cate in categories)
            {
                if (cate.Value == parent)
                {
                    if (parentNode == null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = cate.Key.Name;
                        item.Foreground = (Brush)bc.ConvertFrom("Gray");
                        tr.Items.Add(item);
                        InitializePostSiteTree(tr, cate.Key.CategoryID, item, categories);
                    }
                    else
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = cate.Key.Name;
                        item.Foreground = (Brush)bc.ConvertFrom("Gray");
                        parentNode.Items.Add(item);
                        InitializePostSiteTree(tr, cate.Key.CategoryID, item, categories);
                    }
                }
            }
        }

        /// <summary>
        /// Handler PostSite tree selected items
        /// </summary>
        void DeselectPostSiteCategory(TreeViewItem treeViewItem)
        {
            treeViewItem.Foreground = Brushes.Gray;
            treeViewItem.FontStyle = (FontStyle)fsc.ConvertFrom("Normal");
            selectedPostSiteCategories.Remove(treeViewItem);
        }

        /// <summary>
        /// changes the state of the tree item:
        /// selects it if it has not been selected and
        /// deselects it otherwise
        /// </summary>
        void ChangeSelectedStatePostSiteCategory(TreeViewItem treeViewItem)
        {
            if (!selectedPostSiteCategories.ContainsKey(treeViewItem))
            {
                treeViewItem.Foreground = (Brush)bc.ConvertFrom("#be2f2f");
                treeViewItem.FontStyle = (FontStyle)fsc.ConvertFrom("Oblique");
                selectedPostSiteCategories.Add(treeViewItem, treeViewItem.Header.ToString());
            }
            else
            {
                DeselectPostSiteCategory(treeViewItem);
            }
        }

        /// <summary>
        /// Handler trSelectedPostSiteCategories TreeView SelectedItemChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trSelectedPostSiteCategories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem treeViewItem = trSelectedPostSiteCategories.SelectedItem as TreeViewItem;

            if (treeViewItem == null)
                return;

            //prevent the WPF tree item selection
            treeViewItem.IsSelected = false;

            treeViewItem.Focus();
            if (!_postSiteDomains.Any(s => treeViewItem.Header.ToString().Contains(s)))
            {
                //if (!CtrlPressed)
                //{
                //    List<TreeViewItem> selectedTreeViewList = new List<TreeViewItem>();
                //    foreach (TreeViewItem treeViewItem1 in selectedPostSiteCategories.Keys)
                //    {
                //        selectedTreeViewList.Add(treeViewItem1);
                //    }

                //    foreach (TreeViewItem treeViewItem1 in selectedTreeViewList)
                //    {
                //        DeselectPostSiteCategory(treeViewItem1);
                //    }
                //}

                List<TreeViewItem> selectedTreeViewList = new List<TreeViewItem>();
                foreach (TreeViewItem treeViewItem1 in selectedPostSiteCategories.Keys)
                {
                    selectedTreeViewList.Add(treeViewItem1);
                }

                foreach (TreeViewItem treeViewItem1 in selectedTreeViewList)
                {
                    DeselectPostSiteCategory(treeViewItem1);
                }
                ChangeSelectedStatePostSiteCategory(treeViewItem);
                ApplySelectedPostSiteCategoryToVns(treeViewItem);
            }
        }

        #endregion

        #region EventHandler

        bool CtrlPressed
        {
            get { return System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl); }
        }

        /// <summary>
        /// Handler cbSelectPostSite ComboBox Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSelectPostSites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSelectPostSites.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbSelectPostSites.SelectedItem;

                _chosenSite = VirtualNewspapers.Instance.CurrentProject.GetPostSiteByHost(selected.Content.ToString());

                ReloadPostSiteTreeItem();

                //finally applied any existed rules for this combination of sources and selected site
                ReloadVnsTreeItem();
            }
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
        /// Handler Close button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_viewMode == ViewMode.Editable)
            {
                HandleMessageBox.Instance.GetConfirmation(SystemMessage.CloseWarning);
                if (HandleMessageBox.Instance.Answer)
                    this.Close();
            }
            else
                this.Close();
        }

        /// <summary>
        /// Handler SwitchingViewMode button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSwitchingViewMode_Click(object sender, RoutedEventArgs e)
        {
            if (_viewMode == ViewMode.ViewOnly)
            {
                _viewMode = ViewMode.Editable;
                btnSwitchingViewMode.Content = "Switch to ViewOnly";
            }
            else
            {
                _viewMode = ViewMode.ViewOnly;
                btnSwitchingViewMode.Content = "Switch to Editable";
            }

            switchingViewMode.Invoke(this, _editableObjects, _toogleHiddenButton, _viewMode);
        }

        /// <summary>
        /// Handler Save button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            HandleMessageBox.Instance.GetConfirmation(SystemMessage.Warning);
            if (HandleMessageBox.Instance.Answer)
            {
                VirtualNewspapers.Instance.CurrentProject.RuleCollection = _tempPostRuleCollection;
                _isSaved = true;
            }
        }

        /// <summary>
        /// Handler Reset button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            HandleMessageBox.Instance.GetConfirmation(SystemMessage.Warning);
            if (HandleMessageBox.Instance.Answer)
            {
                _tempPostRuleCollection = VirtualNewspapers.Instance.CurrentProject.RuleCollection;
                _isSaved = false;

                ReloadPostSiteTreeItem();

                //finally applied any existed rules for this combination of sources and selected site
                ReloadVnsTreeItem();
            }
        }

        /// <summary>
        /// Handler Select button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var postCate in selectedPostSiteCategories)
            {
                Category chosen = FindPostSiteCategory(postCate.Value);
                string host = _chosenSite.Host;

                //clear all old rules
                _tempPostRuleCollection.RemoveRules(host, chosen.CategoryID);

                foreach (var vnsCate in selectedVnsCategories)
                {
                    string[] content = vnsCate.Value.Split('|');
                    VirtualNewspapersPostRule rule = new VirtualNewspapersPostRule(content[0], long.Parse(content[2]), host, chosen.CategoryID);
                    _tempPostRuleCollection.AddRules(rule);
                }
            }
        }

        /// <summary>
        /// Handler Clear button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ReloadVnsTreeItem();
            ReloadPostSiteTreeItem();
            _tempPostRuleCollection.RemoveRules(_chosenSite.Host);
        }

        /// <summary>
        /// Handler PostSites ScrollView MouseWheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SiteViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        #endregion

        #region Action

        Action<Window, List<Control>, Button, ViewMode> switchingViewMode = (wd, objs, toogleButton, mode) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                if (mode == ViewMode.ViewOnly)
                {
                    foreach (Control obj in objs)
                    {
                        obj.IsEnabled = false;
                    }
                    toogleButton.Visibility = Visibility.Visible;
                }
                else
                {
                    foreach (Control obj in objs)
                    {
                        obj.IsEnabled = true;
                    }
                    toogleButton.Visibility = Visibility.Hidden;
                }
            }));
        };

        #endregion
       
        #region UtilityMethods

        private Category FindPostSiteCategory(string value)
        {
            //string[] content = value.Split('|');
            return _chosenSite.FindPostSiteCategoryByName(value);
        }

        private void RefreshVnsTreeItem()
        {
            trSelectedVnsSiteCategories.Items.Clear();
            InitializeVNSTree();
        }

        private void RefreshPostSiteTreeItem()
        {
            trSelectedPostSiteCategories.Items.Clear();
            InitializePostSiteTree(_chosenSite);
        }

        private void ReloadVnsTreeItem()
        {
            trSelectedVnsSiteCategories.Items.Clear();
            InitializeVNSTree();
            selectedVnsCategories.Clear();
        }

        private void ReloadPostSiteTreeItem()
        {
            trSelectedPostSiteCategories.Items.Clear();
            InitializePostSiteTree(_chosenSite);
            selectedPostSiteCategories.Clear();
        }

        private void ApplySelectedPostSiteCategoryToVns(TreeViewItem treeViewItem)
        {
            ReloadVnsTreeItem();
            Category chosen = FindPostSiteCategory(treeViewItem.Header.ToString());
            string host = _chosenSite.Host;

            List<VirtualNewspapersPostRule> rules = _tempPostRuleCollection.GetRulesBySiteHostAndCategory(host, chosen.CategoryID);

            if (rules.Count > 0)
            {
                foreach (TreeViewItem item in trSelectedVnsSiteCategories.Items)
                {
                    ApplyRuleToItem(rules, item);
                }
            }
        }

        private void ApplyRuleToItem(List<VirtualNewspapersPostRule> rules, TreeViewItem item)
        {
            foreach (TreeViewItem subItem in item.Items)
                ApplyRuleToItem(rules, subItem);
            if (rules.Any(a => item.Header.ToString().Contains(a.VNSHost) && item.Header.ToString().Contains(a.VNSCategory.ToString())))
            {
                item.Foreground = (Brush)bc.ConvertFrom("#be2f2f");
                item.FontStyle = (FontStyle)fsc.ConvertFrom("Oblique");
                selectedVnsCategories.Add(item, item.Header.ToString());
            }
        }

        #endregion

    }
}
