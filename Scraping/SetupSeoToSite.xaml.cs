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
    /// Interaction logic for SetupSeoToSite.xaml
    /// </summary>
    public partial class SetupSeoToSite : Window
    {
        #region variables

        private static BrushConverter bc = new BrushConverter();
        private static FontWeightConverter fwc = new FontWeightConverter();
        private static FontStyleConverter fsc = new FontStyleConverter();
        private List<string> _postSiteDomains = new List<string>();
        Dictionary<TreeViewItem, string> selectedPostSiteCategories = new Dictionary<TreeViewItem, string>();
        private List<Control> _editableObjects = new List<Control>();
        private Button _toogleHiddenButton;
        private bool _isSaved = false;

        //for temporary variables
        private SEORulesCollection _tempSEORulesCollection;
        private SEORule _tempSiteRule;
        private PostSites _chosenSite;
        private SEOPluginType? _chosenPluginType;
        private CategorySEORule _tempCategoryRule;
        private int _tempPrimaryKeywordPercentage = 0;
        private int _tempSecondaryKeywordPercentage = 0;
        private int _tempGenericKeywordPercentage = 0;
        private int _tempTotalKeywords = 0;
        private AuthorityKeywords _tempAuthorityKeywords = AuthorityKeywords.ArticleTags;
        private AuthoritySearchOptions _tempAuthoritySearch = AuthoritySearchOptions.SearchEngine;
        private AuthorityApperance _tempAuthorityApperance = AuthorityApperance.UpTo1;
        private AuthorityKeywords _tempVideoKeywords = AuthorityKeywords.ArticleTags;
        private AuthoritySearchOptions _tempVideoSearch = AuthoritySearchOptions.HighAuthoritySite;
        private AuthorityKeywords _tempInternalKeywords = AuthorityKeywords.ArticleTags;
        private string _chosenLinkList;
        private string _chosenKeywordList;
        private ViewMode _viewMode;

        #endregion

        #region constructors

        public SetupSeoToSite(Window owner, ViewMode mode)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            _tempSEORulesCollection = AutoPost.Instance.CurrentProject.SeoRuleCollection;
            _viewMode = mode;
            LoadSites();
            InitializeSEOPlugin();
            InitializePercentComboBox();
            InitializeEditableObjects();
            switchingViewMode.Invoke(this, _editableObjects, _toogleHiddenButton, _viewMode);
        }

        /// <summary>
        /// Set controls disabled based on the chosen ViewMode
        /// </summary>
        private void InitializeEditableObjects()
        {
            _editableObjects.Add(cbSEOPlugin);
            _editableObjects.Add(cbTotalKeywords);
            _editableObjects.Add(cbPrimaryKeywordPercentage);
            _editableObjects.Add(cbSecondaryKeywordPercentage);
            _editableObjects.Add(cbGenericKeywordPercentage);
            _editableObjects.Add(cbAuthorityApperanceNumber);
            _editableObjects.Add(cbAuthorityKeywords);
            _editableObjects.Add(cbAuthoritySearchOptions);
            _editableObjects.Add(cbVideoKeywords);
            _editableObjects.Add(cbVideoSearchOptions);
            _editableObjects.Add(btnAddKeywordList);
            _editableObjects.Add(btnDeleteKeywordList);            
            _editableObjects.Add(btnAddUrlList);
            _editableObjects.Add(btnDeleteUrlList);
            _editableObjects.Add(btnSave);
            _editableObjects.Add(btnReset);
            _toogleHiddenButton = btnSwitchingViewMode;            
        }

        /// <summary>
        /// Initialize SEOPlugins ComboBox
        /// </summary>
        private void InitializeSEOPlugin()
        {
            foreach (var plugin in SysSettings.Instance.Plugins)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = plugin.Name;
                item.Tag = plugin.Type.ToString();

                cbSEOPlugin.Items.Add(item);
            }
        }

        /// <summary>
        /// Initialize all percent combobox
        /// </summary>
        private void InitializePercentComboBox()
        {
            for (int i = 0; i <= 100; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                ComboBoxItem item1 = new ComboBoxItem();
                ComboBoxItem item2 = new ComboBoxItem();

                item.Tag = i;
                item.Content = i.ToString() + " %";

                item1.Tag = i;
                item1.Content = i.ToString() + " %";

                item2.Tag = i;
                item2.Content = i.ToString() + " %";

                cbPrimaryKeywordPercentage.Items.Add(item);
                cbSecondaryKeywordPercentage.Items.Add(item1);
                cbGenericKeywordPercentage.Items.Add(item2);
            }
        }

        /// <summary>
        /// Load all selected postsite of chosen project to ComboBox
        /// </summary>
        void LoadSites()
        {
            foreach (var site in AutoPost.Instance.CurrentProject.Sites)
            {
                _postSiteDomains.Add(site.Host);
                ComboBoxItem item = new ComboBoxItem();
                item.Content = site.Host;

                cbSelectPostSite.Items.Add(item);
            }
        }

        /// <summary>
        /// Initialize PostSite TreeView
        /// </summary>
        /// <param name="site"></param>
        private void InitializePostSiteTree(PostSites site)
        {
            TreeViewItem root = new TreeViewItem();
            root.Header = site.Host;
            root.Foreground = (Brush)bc.ConvertFrom("Gray");
            root.IsExpanded = true;
            trSelectedPostSiteCategories.Items.Add(root);
            InitializePostSiteTree(trSelectedPostSiteCategories, 0, root, site.Categories);
        }

        /// <summary>
        /// Insert category to PostSite TreeView
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="parent"></param>
        /// <param name="parentNode"></param>
        /// <param name="categories"></param>
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
        /// Handler PostSites ScrollView MouseWheel
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
        /// Handler cbSelectPostSite ComboBox Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSelectPostSite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSelectPostSite.SelectedItem != null)
            {
                if (_viewMode == ViewMode.Editable)
                    SaveSEORuleBeforeChange(); //save old rule

                //load new rule
                ComboBoxItem selected = (ComboBoxItem)cbSelectPostSite.SelectedItem;

                _chosenSite = AutoPost.Instance.CurrentProject.GetSiteByHost(selected.Content.ToString());
                _tempSiteRule = _tempSEORulesCollection.GetRule(_chosenSite.Host);
                if (_tempSiteRule == null)
                    _tempSiteRule = new SEORule(_chosenSite.Host);
                else
                {
                    _chosenPluginType = _tempSiteRule.Type;
                    foreach (ComboBoxItem item in cbSEOPlugin.Items)
                    {
                        if (item.Tag.ToString() == _chosenPluginType.ToString())
                        {
                            item.IsSelected = true;
                            break;
                        }
                    }
                }
                ReloadPostSiteTreeItem();
                _tempCategoryRule = null;
                RefreshView();
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

            if (_viewMode == ViewMode.Editable)
                SaveCategoryRuleBeforeChange(); //saved old node to temporary variables

            //load new node and replace temporary variables
            if (!_postSiteDomains.Any(s => treeViewItem.Header.ToString().Contains(s)))
            {
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
                ApplySelectedPostSiteCategorySEORule(treeViewItem);
            }
        }

        /// <summary>
        /// Handler SEOPlugin ComboBox SelectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSEOPlugin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSEOPlugin.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbSEOPlugin.SelectedItem;
                _chosenPluginType = Utilities.Helper.Parse<SEOPluginType>(selected.Tag.ToString());
                _tempSiteRule.CreateSEOPlugin(_chosenPluginType);
            }
        }

        /// <summary>
        /// Handler TotalKeywords ComboBox SelectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTotalKeywords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTotalKeywords.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbTotalKeywords.SelectedItem;
                _tempTotalKeywords = Int32.Parse(selected.Tag.ToString());
            }
        }

        /// <summary>
        /// Handler AddKeywordList button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddKeywordList_Click(object sender, RoutedEventArgs e)
        {
            KeywordListEditor viewer = new KeywordListEditor(this, "Add", _tempCategoryRule);
            viewer.ShowDialog();
            RefreshKeywordList();
        }

        /// <summary>
        /// Handler DeleteKeywordList button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteKeywordList_Click(object sender, RoutedEventArgs e)
        {
            if (_chosenKeywordList != "")
            {
                _tempCategoryRule.DeleteLinkListByKeywordList(_chosenKeywordList);
                RefreshLinkList();

                _tempCategoryRule.DeleteKeywordListByName(_chosenKeywordList);
                RefreshKeywordList();
                _chosenKeywordList = "";
            }
        }

        /// <summary>
        /// Handler KeywordPercentage combobox selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeywordPercentage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.Name == "cbPrimaryKeywordPercentage")
            {
                ComboBoxItem selected = (ComboBoxItem)cbPrimaryKeywordPercentage.SelectedItem;
                _tempPrimaryKeywordPercentage = Convert.ToInt32(selected.Tag.ToString());
            }
            else if (cb.Name == "cbSecondaryKeywordPercentage")
            {
                ComboBoxItem selected = (ComboBoxItem)cbSecondaryKeywordPercentage.SelectedItem;
                _tempSecondaryKeywordPercentage = Convert.ToInt32(selected.Tag.ToString());
            }
            else
            {
                ComboBoxItem selected = (ComboBoxItem)cbGenericKeywordPercentage.SelectedItem;
                _tempGenericKeywordPercentage = Convert.ToInt32(selected.Tag.ToString());
            }
        }

        /// <summary>
        /// Handler MouseLeftButtonDown event on KeywordList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeywordListDockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DockPanel dp = (DockPanel)sender;
                ToogleListDisplay.Invoke(this, sp_KeywordList, dp);

                foreach (Label lbl in Utilities.Helper.FindVisualChildren<Label>(dp))
                {
                    if (lbl.Content.ToString() != "Primary" && lbl.Content.ToString() != "Secondary" && lbl.Content.ToString() != "Generic")
                    {
                        _chosenKeywordList = lbl.Content.ToString();
                    }
                }
            }
            else
            {
                DockPanel dp = (DockPanel)sender;
                ToogleListDisplay.Invoke(this, sp_KeywordList, dp);

                foreach (Label lbl in Utilities.Helper.FindVisualChildren<Label>(dp))
                {
                    if (lbl.Content.ToString() != "Primary" && lbl.Content.ToString() != "Secondary" && lbl.Content.ToString() != "Generic")
                    {
                        _chosenKeywordList = lbl.Content.ToString();
                    }
                }
                KeywordListEditor viewer = new KeywordListEditor(this, "Edit", _viewMode, _tempCategoryRule, _tempCategoryRule.GetKeywordListByName(_chosenKeywordList));
                viewer.ShowDialog();
                RefreshKeywordList();
            }
        }

        /// <summary>
        /// Handler Delete ListURL settings event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteUrlList_Click(object sender, RoutedEventArgs e)
        {
            if (_chosenLinkList != "")
            {
                _tempCategoryRule.DeleteLinkListByName(_chosenLinkList);
                RefreshLinkList();
                _chosenLinkList = "";
            }
        }

        /// <summary>
        /// Handler Add ListURL settings event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddUrlList_Click(object sender, RoutedEventArgs e)
        {
            LinkListEditor viewer = new LinkListEditor(this, "Add", _tempCategoryRule);
            viewer.ShowDialog();
            RefreshLinkList();
        }

        /// <summary>
        /// Handler MouseLeftButtonDown event on LinkList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkListDockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DockPanel dp = (DockPanel)sender;
                ToogleListDisplay.Invoke(this, sp_ListUrlSettings, dp);

                foreach (Label lbl in Utilities.Helper.FindVisualChildren<Label>(dp))
                {
                    if (lbl.Width == 250)
                    {
                        _chosenLinkList = lbl.Content.ToString();
                    }
                }
            }
            else
            {
                DockPanel dp = (DockPanel)sender;
                ToogleListDisplay.Invoke(this, sp_ListUrlSettings, dp);

                foreach (Label lbl in Utilities.Helper.FindVisualChildren<Label>(dp))
                {
                    if (lbl.Width == 250)
                    {
                        _chosenLinkList = lbl.Content.ToString();
                    }
                }
                LinkListEditor viewer = new LinkListEditor(this, "Edit", _viewMode, _tempCategoryRule, _tempCategoryRule.GetLinkListByName(_chosenLinkList));
                viewer.ShowDialog();
                RefreshLinkList();
            }
        }

        /// <summary>
        /// Handler radioButton checked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton ck = sender as RadioButton;
            if (ck.IsChecked.Value)
            {
                switch (ck.Name)
                {
                    case "rbAuthorityAccept":
                        dp_AuthoritySettings.Visibility = System.Windows.Visibility.Visible;
                        _tempCategoryRule.InsertAuthorityLinks = true;
                        break;
                    case "rbAuthorityDeny":
                        dp_AuthoritySettings.Visibility = System.Windows.Visibility.Hidden;
                        _tempCategoryRule.InsertAuthorityLinks = false;
                        break;
                    case "rbVideoAccept":
                        dp_VideoSettings.Visibility = System.Windows.Visibility.Visible;
                        _tempCategoryRule.InsertVideo = true;
                        break;
                    case "rbVideoDeny":
                        dp_VideoSettings.Visibility = System.Windows.Visibility.Hidden;
                        _tempCategoryRule.InsertVideo = false;
                        break;
                    case "rbInternalLinksAccept":
                        dp_InternalSettings.Visibility = System.Windows.Visibility.Visible;
                        _tempCategoryRule.InsertInternalLink = true;
                        break;
                    case "rbInternalLinksDeny":
                        dp_InternalSettings.Visibility = System.Windows.Visibility.Hidden;
                        _tempCategoryRule.InsertInternalLink = false;
                        break;
                };
            }
        }

        /// <summary>
        /// Handler ComboBox cbAuthorityKeywords selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbAuthorityKeywords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAuthorityKeywords.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbAuthorityKeywords.SelectedItem;
                _tempAuthorityKeywords = Utilities.Helper.Parse<AuthorityKeywords>(selected.Tag.ToString()) ?? AuthorityKeywords.ArticleTags;
            }
        }

        /// <summary>
        /// Handler ComboBox cbAuthoritySearchOptions selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbAuthoritySearchOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAuthoritySearchOptions.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbAuthoritySearchOptions.SelectedItem;
                _tempAuthoritySearch = Utilities.Helper.Parse<AuthoritySearchOptions>(selected.Tag.ToString()) ?? AuthoritySearchOptions.SearchEngine;
            }
        }

        /// <summary>
        /// Handler ComboBox cbAuthorityApperanceNumber selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbAuthorityApperanceNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAuthorityApperanceNumber.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbAuthorityApperanceNumber.SelectedItem;
                _tempAuthorityApperance = Utilities.Helper.Parse<AuthorityApperance>(selected.Tag.ToString()) ?? AuthorityApperance.UpTo1;
            }
        }

        /// <summary>
        /// Handler ComboBox cbVideoKeywords selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbVideoKeywords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbVideoKeywords.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbVideoKeywords.SelectedItem;
                _tempVideoKeywords = Utilities.Helper.Parse<AuthorityKeywords>(selected.Tag.ToString()) ?? AuthorityKeywords.ArticleTags;
            }
        }

        /// <summary>
        /// Handler ComboBox cbVideoSearchOptions selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbVideoSearchOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbVideoSearchOptions.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbVideoSearchOptions.SelectedItem;
                _tempVideoSearch = Utilities.Helper.Parse<AuthoritySearchOptions>(selected.Tag.ToString()) ?? AuthoritySearchOptions.HighAuthoritySite;
            }
        }

        /// <summary>
        /// Handler ComboBox cbInternalLinksKeywords selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbInternalLinksKeywords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbInternalLinksKeywords.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbInternalLinksKeywords.SelectedItem;
                _tempInternalKeywords = Utilities.Helper.Parse<AuthorityKeywords>(selected.Tag.ToString()) ?? AuthorityKeywords.ArticleTags;
            }
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
                SaveSEORuleBeforeChange();
                AutoPost.Instance.CurrentProject.SeoRuleCollection = _tempSEORulesCollection;
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
                _tempSEORulesCollection = AutoPost.Instance.CurrentProject.SeoRuleCollection;
                _isSaved = false;

                //load new rule
                ComboBoxItem selected = (ComboBoxItem)cbSelectPostSite.SelectedItem;

                _chosenSite = AutoPost.Instance.CurrentProject.GetSiteByHost(selected.Content.ToString());
                _tempSiteRule = _tempSEORulesCollection.GetRule(_chosenSite.Host);
                if (_tempSiteRule == null)
                    _tempSiteRule = new SEORule(_chosenSite.Host);
                else
                {
                    _chosenPluginType = _tempSiteRule.Type;
                    foreach (ComboBoxItem item in cbSEOPlugin.Items)
                    {
                        if (item.Tag.ToString() == _chosenPluginType.ToString())
                        {
                            item.IsSelected = true;
                            break;
                        }
                    }
                }
                ReloadPostSiteTreeItem();
                _tempCategoryRule = null;
                RefreshView();
            }
        }

        #endregion

        #region Action

        Action<Window, StackPanel, DockPanel> ToogleListDisplay = (wd, sp, dp) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
            {
                foreach (DockPanel i in Utilities.Helper.FindVisualChildren<DockPanel>(sp))
                {
                    i.Background = i == dp ? (Brush)bc.ConvertFrom("#454545") : (Brush)bc.ConvertFrom("#2a2a2a");
                }
            }));
        };

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

        #region UtilityMethod

        /// <summary>
        /// Refresh PostTree visual items
        /// </summary>
        private void RefreshPostSiteTreeItem()
        {
            trSelectedPostSiteCategories.Items.Clear();
            InitializePostSiteTree(_chosenSite);
        }

        /// <summary>
        /// Clear selected & reload PostTree items
        /// </summary>
        private void ReloadPostSiteTreeItem()
        {
            trSelectedPostSiteCategories.Items.Clear();
            InitializePostSiteTree(_chosenSite);
            selectedPostSiteCategories.Clear();
        }

        /// <summary>
        /// Get Category instance of chosen host via its name
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private Category FindPostSiteCategory(string value)
        {
            return _chosenSite.FindPostSiteCategoryByName(value);
        }

        /// <summary>
        /// Apply the SEO Rules of chosen category of chosen website
        /// </summary>
        /// <param name="item"></param>
        private void ApplySelectedPostSiteCategorySEORule(TreeViewItem item)
        {
            Category chosen = FindPostSiteCategory(item.Header.ToString());
            string host = _chosenSite.Host;

            if (_tempSiteRule != null)
            {
                if (_chosenPluginType != null)
                {
                    if (_chosenPluginType != _tempSiteRule.Type)
                    {
                        _chosenPluginType = _tempSiteRule.Type;
                        foreach (ComboBoxItem cbItem in cbSEOPlugin.Items)
                        {
                            if (cbItem.Tag.ToString() == _chosenPluginType.ToString())
                            {
                                cbItem.IsSelected = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    _chosenPluginType = _tempSiteRule.Type;
                    foreach (ComboBoxItem cbItem in cbSEOPlugin.Items)
                    {
                        if (cbItem.Tag.ToString() == _chosenPluginType.ToString())
                        {
                            cbItem.IsSelected = true;
                            break;
                        }
                    }
                }
                _tempCategoryRule = _tempSiteRule.GetCategorySEORule(chosen.CategoryID);

                //check if category rule isExisted
                if (_tempCategoryRule != null)
                {
                    //load primary keywords
                    RefreshView();
                    ReloadTotalKeyword();
                    InitializeKeywordList();
                    ReloadKeywordPercentage();
                    InitializeLinkList();
                    InitializeAuthority();
                    InitializeVideo();
                    InitializeInternal();
                }
                else
                {
                    _tempCategoryRule = new CategorySEORule(host, chosen.CategoryID);
                    RefreshView();
                    rbAuthorityDeny.IsChecked = true;
                    rbInternalLinksDeny.IsChecked = true;
                    rbVideoDeny.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// Refresh setup view when change category on a selected post site
        /// </summary>
        private void RefreshView()
        {
            //clear visual control            
            sp_ListUrlSettings.Children.Clear();
            sp_KeywordList.Children.Clear();

            //clear temp variables                                    
            _chosenLinkList = "";
            _chosenKeywordList = "";
        }

        /// <summary>
        /// Load keyword percentage based on category rule
        /// </summary>
        private void ReloadKeywordPercentage()
        {
            foreach (ComboBoxItem cbItem in cbPrimaryKeywordPercentage.Items)
            {
                if (cbItem.Tag.ToString() == _tempCategoryRule.PrimaryKeywordPercentage.ToString())
                {
                    cbItem.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem cbItem in cbSecondaryKeywordPercentage.Items)
            {
                if (cbItem.Tag.ToString() == _tempCategoryRule.SecondaryKeywordPercentage.ToString())
                {
                    cbItem.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem cbItem in cbGenericKeywordPercentage.Items)
            {
                if (cbItem.Tag.ToString() == _tempCategoryRule.GenericKeywordPercentage.ToString())
                {
                    cbItem.IsSelected = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Load totalKeyword based on category rule
        /// </summary>
        private void ReloadTotalKeyword()
        {
            foreach (ComboBoxItem cbItem in cbTotalKeywords.Items)
            {
                if (cbItem.Tag.ToString() == _tempCategoryRule.TotalKeywords.ToString())
                {
                    cbItem.IsSelected = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Load KeywordList settings
        /// </summary>
        private void InitializeKeywordList()
        {
            foreach (KeywordList list in _tempCategoryRule.KeywordList)
            {
                DockPanel dp = new DockPanel();
                dp.Background = (Brush)bc.ConvertFrom("#2a2a2a");
                dp.MouseLeftButtonDown += KeywordListDockPanel_MouseLeftButtonDown;

                Label lblName = new Label();
                lblName.Content = list.Name;
                lblName.Width = 290;

                Label lblType = new Label();
                lblType.Content = list.KeywordType.ToString();

                dp.Children.Add(lblName);
                dp.Children.Add(lblType);

                sp_KeywordList.Children.Add(dp);
            }
        }
        /// <summary>
        /// Load LinkList settings
        /// </summary>
        private void InitializeLinkList()
        {
            foreach (LinkList list in _tempCategoryRule.CategoryLinkList)
            {
                DockPanel dp = new DockPanel();
                dp.Background = (Brush)bc.ConvertFrom("#2a2a2a");
                dp.MouseLeftButtonDown += LinkListDockPanel_MouseLeftButtonDown;
                Label lblName = new Label();
                lblName.Content = list.Name;
                lblName.Width = 250;

                Label lblType = new Label();
                lblType.Content = list.KeywordListName;

                dp.Children.Add(lblName);
                dp.Children.Add(lblType);

                sp_ListUrlSettings.Children.Add(dp);
            }
        }

        /// <summary>
        /// Refresh again KeywordList
        /// </summary>
        private void RefreshKeywordList()
        {
            sp_KeywordList.Children.Clear();
            InitializeKeywordList();
        }

        /// <summary>
        /// Refresh again ListURL settings
        /// </summary>
        private void RefreshLinkList()
        {
            sp_ListUrlSettings.Children.Clear();
            InitializeLinkList();
        }

        /// <summary>
        /// Load Authority settings
        /// </summary>
        private void InitializeAuthority()
        {
            switch (_tempCategoryRule.InsertAuthorityLinks)
            {
                case true:
                    rbAuthorityAccept.IsChecked = true;
                    break;
                case false:
                    rbAuthorityDeny.IsChecked = true;
                    break;
            };

            foreach (ComboBoxItem item in cbAuthorityKeywords.Items)
            {
                if (item.Tag.ToString() == _tempCategoryRule.AuthorityKeywords.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem item in cbAuthoritySearchOptions.Items)
            {
                if (item.Tag.ToString() == _tempCategoryRule.AuthoritySearch.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem item in cbAuthorityApperanceNumber.Items)
            {
                if (item.Tag.ToString() == _tempCategoryRule.AuthorityApperance.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Load Video settings
        /// </summary>
        private void InitializeVideo()
        {
            switch (_tempCategoryRule.InsertVideo)
            {
                case true:
                    rbVideoAccept.IsChecked = true;
                    break;
                case false:
                    rbVideoDeny.IsChecked = true;
                    break;
            };

            foreach (ComboBoxItem item in cbVideoKeywords.Items)
            {
                if (item.Tag.ToString() == _tempCategoryRule.VideoKeywords.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            foreach (ComboBoxItem item in cbVideoSearchOptions.Items)
            {
                if (item.Tag.ToString() == _tempCategoryRule.VideoSearch.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Load internal settings
        /// </summary>
        private void InitializeInternal()
        {
            switch (_tempCategoryRule.InsertInternalLink)
            {
                case true:
                    rbInternalLinksAccept.IsChecked = true;
                    break;
                case false:
                    rbInternalLinksDeny.IsChecked = true;
                    break;
            }

            foreach (ComboBoxItem item in cbInternalLinksKeywords.Items)
            {
                if (item.Tag.ToString() == _tempCategoryRule.InternalKeywords.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Save category SEORule before change to other
        /// </summary>
        private void SaveCategoryRuleBeforeChange()
        {
            if (_tempCategoryRule != null)
            {
                _tempCategoryRule.ChangeTotalKeyword(_tempTotalKeywords);
                _tempCategoryRule.ChangePrimaryKeywordPercentage(_tempPrimaryKeywordPercentage);
                _tempCategoryRule.ChangeSecondaryKeywordPercentage(_tempSecondaryKeywordPercentage);
                _tempCategoryRule.ChangeGenericKeywordPercentage(_tempGenericKeywordPercentage);

                _tempCategoryRule.AuthorityApperance = _tempAuthorityApperance;
                _tempCategoryRule.AuthorityKeywords = _tempAuthorityKeywords;
                _tempCategoryRule.AuthoritySearch = _tempAuthoritySearch;

                _tempCategoryRule.VideoKeywords = _tempVideoKeywords;
                _tempCategoryRule.VideoSearch = _tempVideoSearch;

                _tempSiteRule.DeleteCategorySEORule(_tempCategoryRule.Host, _tempCategoryRule.CategoryID);
                _tempSiteRule.AddCategorySEORule(_tempCategoryRule);
            }
        }

        /// <summary>
        /// Save all category's SEO Rules of chosen site before change to other
        /// </summary>
        private void SaveSEORuleBeforeChange()
        {
            if (_tempSiteRule != null)
            {
                SaveCategoryRuleBeforeChange();
                _tempSEORulesCollection.DeleteRule(_tempSiteRule.Host);
                _tempSEORulesCollection.AddRule(_tempSiteRule);
            }
        }

        #endregion
        
    }
}
