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
    /// Interaction logic for LinkListEditor.xaml
    /// </summary>
    public partial class LinkListEditor : Window
    {

        #region variables
        
        private string _openMode;
        private List<string> _tempURLs = new List<string>();

        private int _tempApperance = 1;
        private string _tempKeywordListName = "";
        private CategorySEORule _cateRule;
        private LinkList _tempLinkList = new LinkList();
        private List<TextBox> _textBoxes = new List<TextBox>();
        private ViewMode _viewMode;

        #endregion

        #region Initialize

        public LinkListEditor(Window owner, string mode, CategorySEORule rule)
            : this(owner, mode, ViewMode.Editable, rule, null)
        {
        }

        public LinkListEditor(Window owner, string mode, ViewMode viewMode, CategorySEORule rule, LinkList linkList)
        {
            InitializeComponent();

            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }

            _textBoxes.Add(tbLinkListName);
            _textBoxes.Add(tbLinks);
            _openMode = mode;
            _cateRule = rule;
            _viewMode = viewMode;

            InitializeViewMode();
            InitializeKeywordListName();
            if (_openMode != "Add")
            {
                tbLinkListName.IsEnabled = false;                
            }
            if (linkList != null)
            {
                _tempLinkList = linkList;
                InitializeLinkList(linkList);
            }            
        }

        /// <summary>
        /// Set controls disabled based on the chosen ViewMode
        /// </summary>
        private void InitializeViewMode()
        {
            if (_viewMode == ViewMode.ViewOnly)
            {
                tbLinkListName.IsEnabled = false;                
                cbAppearance.IsEnabled = false;
                tbLinks.IsEnabled = false;
                btnSave.IsEnabled = btnCancel.IsEnabled = false;
            }
        }

        /// <summary>
        /// Initialize of all available keywordLists
        /// </summary>
        private void InitializeKeywordListName()
        {
            foreach (KeywordList list in _cateRule.KeywordList)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = list.Name;
                item.Content = list.Name;

                cbKeywordListNames.Items.Add(item);
            }
        }

        /// <summary>
        /// Initialize of chosen LinkList
        /// </summary>
        /// <param name="linkList"></param>
        private void InitializeLinkList(LinkList linkList)
        {
            //load name
            tbLinkListName.Text = linkList.Name;

            //re-select linklist keywordList
            _tempKeywordListName = linkList.KeywordListName;
            foreach (ComboBoxItem item in cbKeywordListNames.Items)
            {
                if (item.Tag.ToString() == _tempKeywordListName)
                {
                    item.IsSelected = true;
                    break;
                }
            }

            //re-select linklist appearance
            _tempApperance = linkList.ApperanceNumber;
            foreach (ComboBoxItem item in cbAppearance.Items)
            {
                if (item.Tag.ToString() == linkList.ApperanceNumber.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            _tempURLs = new List<string>();
            foreach (var link in linkList.Links)
            {
                _tempURLs.Add(link);
            }
            FillTextBoxByLinks(_tempURLs, tbLinks);
        }

        #endregion

        #region EventHandler        

        /// <summary>
        /// Hanlder Moving Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Handler Close click
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
        /// Handler cbKeywordListNames selectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbKeywordListNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbKeywordListNames.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbKeywordListNames.SelectedItem;
                _tempKeywordListName = selected.Tag.ToString();
            }
        }

        /// <summary>
        /// Handler cbAppearance selectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbAppearance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAppearance.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbAppearance.SelectedItem;
                _tempApperance = Int32.Parse(selected.Tag.ToString());
            }
        }

        /// <summary>
        /// Handler Cancel button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
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
        /// Handler Save button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool isPassed = true;
            displayValidateTextBoxes.Invoke(this, _textBoxes, CheckAddNewLinkList(ref isPassed));
            if (isPassed)
            {
                if (_openMode == "Add")
                {
                    LinkList newItem = new LinkList(tbLinkListName.Text, _tempKeywordListName, _tempApperance);
                    newItem.LoadLinks(tbLinks.Text);

                    _cateRule.AddLinkList(newItem);
                    this.Close();
                }
                else
                {
                    EditLinkList();
                    this.Close();
                }
            }
        }

        #endregion

        #region Action

        Action<Window, List<TextBox>, List<Tuple<string, bool>>> displayValidateTextBoxes = (wd, tbs, tups) =>
        {
            wd.Dispatcher.Invoke(new Action(() =>
                {
                    BrushConverter bc = new BrushConverter();
                    ThicknessConverter tc = new ThicknessConverter();

                    foreach (var item in tups)
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
                }));
        };

        #endregion

        #region UtilityMethods        

        /// <summary>
        /// Fill chosen textbox with list of links
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="tb"></param>
        private void FillTextBoxByLinks(List<string> keywords, TextBox tb)
        {
            int count = 1;
            foreach (var keyword in keywords)
            {
                tb.AppendText(keyword);
                if (count < keywords.Count)
                    tb.AppendText(Environment.NewLine);
                count++;
            }
        }

        /// <summary>
        /// Edit current selected LinkList
        /// </summary>
        private void EditLinkList()
        {
            _tempLinkList.KeywordListName = _tempKeywordListName;
            _tempLinkList.ApperanceNumber = _tempApperance;
            _tempLinkList.LoadLinks(tbLinks.Text);

            _cateRule.DeleteLinkListByName(_tempLinkList.Name);
            _cateRule.AddLinkList(_tempLinkList);
        }

        /// <summary>
        /// Check if desired name existed or not
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool CheckNameExisted(string name)
        {
            return _cateRule.CheckLinkListExisted(name);
        }

        /// <summary>
        /// Check validate of adding new link list
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        private List<Tuple<string, bool>> CheckAddNewLinkList(ref bool pass)
        {
            List<Tuple<string, bool>> temp = new List<Tuple<string, bool>>();
            bool isValidate = false;

            foreach (TextBox tb in _textBoxes)
            {
                if (tb.Text == string.Empty)
                {
                    isValidate = false;
                    pass = false;
                }
                else
                {
                    if (_openMode == "Add")
                    {
                        if (tb.Name == "tbLinkListName")
                        {
                            if (CheckNameExisted(tb.Text))
                            {
                                isValidate = false;
                                pass = false;
                            }
                            else
                            {
                                isValidate = true;
                            }
                        }
                        else
                            isValidate = true;
                    }
                }
                temp.Add(new Tuple<string, bool>(tb.Name, isValidate));
            }
            return temp;
        }

        #endregion

    }
}
