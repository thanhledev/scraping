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
    /// Interaction logic for SetupSourceToSite.xaml
    /// </summary>
    public partial class SetupSourceToSite : Window
    {
        #region variables

        private BrushConverter bc = new BrushConverter();
        private FontStyleConverter fsc = new FontStyleConverter();
        private List<string> _sourceDomains = new List<string>();
        private List<string> _postSiteDomains = new List<string>();
        Dictionary<TreeViewItem, string> selectedSourceCategories = new Dictionary<TreeViewItem, string>();
        Dictionary<TreeViewItem, string> selectedPostSiteCategories = new Dictionary<TreeViewItem, string>();        
        private ViewMode _viewMode;

        //for temporary variables
        private ArticleSourceLanguage _tempArticleSourceLanguage = ArticleSourceLanguage.All;
        private ArticleSourceType _tempArticleSourceType = ArticleSourceType.All;
        private PostRulesCollection _tempPostRuleCollection;
        private PostSites _chosenSite;
        private List<Control> _editableObjects = new List<Control>();
        private Button _toogleHiddenButton;
        private bool _isSaved = false;
        #endregion

        #region Constructors

        public SetupSourceToSite(Window owner, ViewMode mode)
        {
            InitializeComponent();

            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            _viewMode = mode;
            _tempPostRuleCollection = AutoPost.Instance.CurrentProject.RuleCollection;
            //InitializeSourceTree();
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
            _editableObjects.Add(trSourceCategories);
            _editableObjects.Add(btnSelect);
            _editableObjects.Add(btnClear);
            _editableObjects.Add(btnSave);
            _editableObjects.Add(btnReset);

            _toogleHiddenButton = btnSwitchingViewMode;
        }

        private List<ArticleSource> FilterArticleSources()
        {
            List<ArticleSource> satisfiedSources = new List<ArticleSource>();

            if (_tempArticleSourceLanguage != ArticleSourceLanguage.All)
            {
                foreach (var i in SysSettings.Instance.Article_Scraping_ScrapingSources)
                {
                    if (i.Language == _tempArticleSourceLanguage)
                    {
                        if (_tempArticleSourceType != ArticleSourceType.All)
                        {
                            if (i.Types.Contains(_tempArticleSourceType))
                            {
                                satisfiedSources.Add(i);
                            }
                        }
                        else
                        {
                            satisfiedSources.Add(i);
                        }
                    }
                }
            }
            else
            {
                foreach (var i in SysSettings.Instance.Article_Scraping_ScrapingSources)
                {
                    if (_tempArticleSourceType != ArticleSourceType.All)
                    {
                        if (i.Types.Contains(_tempArticleSourceType))
                        {
                            satisfiedSources.Add(i);
                        }
                    }
                    else
                    {
                        satisfiedSources.Add(i);
                    }
                }
            }

            return satisfiedSources;
        }

        /// <summary>
        /// Handler Source tree
        /// </summary>        
        private void InitializeSourceTree(List<ArticleSource> list)
        {
            trSourceCategories.Items.Clear();
            foreach (var src in list)
            {
                _sourceDomains.Add(src.Title);
                TreeViewItem root = new TreeViewItem();
                root.Header = src.Title;
                root.Foreground = (Brush)bc.ConvertFrom("Gray");
                root.IsExpanded = true;
                trSourceCategories.Items.Add(root);
                InitializeSourceTree(trSourceCategories, null, root, src);
            }
        }

        private void InitializeSourceTree(TreeView tr, SourceCategory parent, TreeViewItem parentNode, ArticleSource source)
        {
            foreach (var cate in source.Categories)
            {
                if (cate.ParentCategory == parent)
                {
                    if (parentNode == null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = source.Title + "|" + cate.Slug;
                        item.Foreground = (Brush)bc.ConvertFrom("Gray");
                        tr.Items.Add(item);                        
                        InitializeSourceTree(tr, cate, item, source);
                    }
                    else
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = source.Title + "|" + cate.Slug;
                        item.Foreground = (Brush)bc.ConvertFrom("Gray");
                        parentNode.Items.Add(item);
                        InitializeSourceTree(tr, cate, item, source);
                    }
                }
            }
        }

        void DeselectSourceCategory(TreeViewItem treeViewItem)
        {
            //treeViewItem.Background = Brushes.Black ;// change background and foreground colors            
            treeViewItem.Foreground = Brushes.Gray;
            treeViewItem.FontStyle = (FontStyle)fsc.ConvertFrom("Normal");
            selectedSourceCategories.Remove(treeViewItem);
        }

        /// <summary>
        /// changes the state of the tree item:
        /// selects it if it has not been selected and
        /// deselects it otherwise
        /// </summary>
        void ChangeSelectedStateSourceCategory(TreeViewItem treeViewItem)
        {
            if (!selectedSourceCategories.ContainsKey(treeViewItem))
            {
                //treeViewItem.Background = (Brush)bc.ConvertFrom("#be2f2f"); // change background and foreground colors                
                treeViewItem.Foreground = (Brush)bc.ConvertFrom("#be2f2f");
                treeViewItem.FontStyle = (FontStyle)fsc.ConvertFrom("Oblique");
                selectedSourceCategories.Add(treeViewItem, treeViewItem.Header.ToString());
            }
            else
            {
                DeselectSourceCategory(treeViewItem);
            }
        }

        /// <summary>
        /// Handler trSourceCategories TreeView SelectedItemChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trSourceCategories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem treeViewItem = trSourceCategories.SelectedItem as TreeViewItem;

            if (treeViewItem == null)
                return;

            //prevent the WPF tree item selection
            treeViewItem.IsSelected = false;

            treeViewItem.Focus();

            if (!_sourceDomains.Contains(treeViewItem.Header.ToString()))
            {
                if (!CtrlPressed)
                {
                    List<TreeViewItem> selectedTreeViewList = new List<TreeViewItem>();
                    foreach (TreeViewItem treeViewItem1 in selectedSourceCategories.Keys)
                    {
                        selectedTreeViewList.Add(treeViewItem1);
                    }

                    foreach (TreeViewItem treeViewItem1 in selectedTreeViewList)
                    {
                        DeselectSourceCategory(treeViewItem1);
                    }
                }
                ChangeSelectedStateSourceCategory(treeViewItem);
            }
        }

        /// <summary>
        /// Handler Source Categories ScrollView MouseWheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourceViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        /// <summary>
        /// Handler PostSite Categories tree 
        /// </summary>        
        private void LoadSites()
        {
            foreach (var site in AutoPost.Instance.CurrentProject.Sites)
            {
                _postSiteDomains.Add(site.Host);
                ComboBoxItem item = new ComboBoxItem();
                item.Content = site.Host;

                cbSelectPostSite.Items.Add(item);
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
                ApplySelectedPostSiteCategoryToSource(treeViewItem);
            }
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

        #region EventHandler

        /// <summary>
        /// Handler comboBox SourceLanguage selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSourceLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSourceLanguage.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbSourceLanguage.SelectedItem;
                _tempArticleSourceLanguage = Utilities.Helper.Parse<ArticleSourceLanguage>(selected.Tag.ToString()) ?? ArticleSourceLanguage.English;
                InitializeSourceTree(FilterArticleSources());
            }
        }

        /// <summary>
        /// Handler comboBox SourceType selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSourceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSourceType.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbSourceType.SelectedItem;
                _tempArticleSourceType = Utilities.Helper.Parse<ArticleSourceType>(selected.Tag.ToString()) ?? ArticleSourceType.News;
                InitializeSourceTree(FilterArticleSources());
            }
        }

        bool CtrlPressed
        {
            get { return System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl); }
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
                ComboBoxItem selected = (ComboBoxItem)cbSelectPostSite.SelectedItem;

                _chosenSite = AutoPost.Instance.CurrentProject.GetSiteByHost(selected.Content.ToString());

                ReloadPostSiteTreeItem();

                //finally applied any existed rules for this combination of sources and selected site
                ReloadSourceTreeItem();
            }
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
        /// Handler mouse left button click down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
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

                foreach (var sourceCate in selectedSourceCategories)
                {
                    string[] content = sourceCate.Value.Split('|');
                    PostRule rule = new PostRule(content[0], content[1], chosen.CategoryID, host);                    
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
            ReloadSourceTreeItem();
            ReloadPostSiteTreeItem();
            _tempPostRuleCollection.RemoveRules(_chosenSite.Host);
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
                AutoPost.Instance.CurrentProject.RuleCollection = _tempPostRuleCollection;
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
                _tempPostRuleCollection = AutoPost.Instance.CurrentProject.RuleCollection;
                _isSaved = false;

                ReloadPostSiteTreeItem();

                //finally applied any existed rules for this combination of sources and selected site
                ReloadSourceTreeItem();
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

        private void RefreshSourceTreeItem()
        {
            trSourceCategories.Items.Clear();
            InitializeSourceTree(FilterArticleSources());            
        }

        private void RefreshPostSiteTreeItem()
        {
            trSelectedPostSiteCategories.Items.Clear();
            InitializePostSiteTree(_chosenSite);            
        }

        private void ReloadSourceTreeItem()
        {
            trSourceCategories.Items.Clear();
            InitializeSourceTree(FilterArticleSources());
            selectedSourceCategories.Clear();
        }

        private void ReloadPostSiteTreeItem()
        {
            trSelectedPostSiteCategories.Items.Clear();
            InitializePostSiteTree(_chosenSite);
            selectedPostSiteCategories.Clear();
        }

        private void ApplySelectedPostSiteCategoryToSource(TreeViewItem treeViewItem)
        {            
            ReloadSourceTreeItem();
            Category chosen = FindPostSiteCategory(treeViewItem.Header.ToString());
            string host = _chosenSite.Host;

            List<PostRule> rules = _tempPostRuleCollection.GetRulesBySiteHostAndCategory(host, chosen.CategoryID);

            if (rules.Count > 0)
            {
                foreach (TreeViewItem item in trSourceCategories.Items)
                {
                    ApplyRuleToItem(rules, item);
                }
            }            
        }

        private void ApplyRuleToItem(List<PostRule> rules,TreeViewItem item)
        {            
            foreach (TreeViewItem subItem in item.Items)
                ApplyRuleToItem(rules, subItem);                      
            if (rules.Any(a => item.Header.ToString().Contains(a.SourceCategory) && item.Header.ToString().Contains(a.SourceHost)))
            {
                item.Foreground = (Brush)bc.ConvertFrom("#be2f2f");
                item.FontStyle = (FontStyle)fsc.ConvertFrom("Oblique");
                selectedSourceCategories.Add(item, item.Header.ToString());
            }           
        }

        #endregion

        

        
        
    }
}
