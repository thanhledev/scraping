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
    /// Interaction logic for KeywordListEditor.xaml
    /// </summary>
    public partial class KeywordListEditor : Window
    {
        #region variables

        private string _openMode;
        private List<string> _tempKeywords = new List<string>();

        private KeywordType _chosenType;
        private InsertLinkOptions _tempOption = InsertLinkOptions.Random;
        private CategorySEORule _cateRule;
        private KeywordList _tempKeywordList = new KeywordList();
        private List<TextBox> _textBoxes = new List<TextBox>();
        private ViewMode _viewMode;

        #endregion

        #region constructors

        public KeywordListEditor(Window owner, string mode, CategorySEORule rule)
            : this(owner, mode, ViewMode.Editable, rule, null)
        {

        }

        public KeywordListEditor(Window owner, string mode, ViewMode viewMode, CategorySEORule rule,KeywordList keywordList)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            _textBoxes.Add(tbKeywordListName);
            _textBoxes.Add(tbKeywords);
            _openMode = mode;
            _cateRule = rule;
            _viewMode = viewMode;

            if (_openMode != "Add")
            {
                tbKeywordListName.IsEnabled = false;
                rbPrimay.IsChecked = true;
            }
            if (keywordList != null)
            {
                _tempKeywordList = keywordList;
                InitializeKeywordList(keywordList);
            }
            InitializeViewMode();
        }

        /// <summary>
        /// Set controls disabled based on the chosen ViewMode
        /// </summary>
        private void InitializeViewMode()
        {
            if (_viewMode == ViewMode.ViewOnly)
            {
                tbKeywordListName.IsEnabled = false;
                rbPrimay.IsEnabled = rbGeneric.IsEnabled = rbSecondary.IsEnabled = false;
                cbInsertOptions.IsEnabled = false;
                tbKeywords.IsEnabled = false;
                btnSave.IsEnabled = btnCancel.IsEnabled = false;
            }
        }

        /// <summary>
        /// Initialize of chosen KeywordList
        /// </summary>
        /// <param name="linkList"></param>
        private void InitializeKeywordList(KeywordList keywordList)
        {
            //load name
            tbKeywordListName.Text = keywordList.Name;

            //re-select keywordtype
            switch (keywordList.KeywordType)
            {
                case KeywordType.Primary:
                    rbPrimay.IsChecked = true;
                    break;
                case KeywordType.Secondary:
                    rbSecondary.IsChecked = true;
                    break;
                default:
                    rbGeneric.IsChecked = true;
                    break;
            };

            _tempOption = keywordList.InsertOpt;
            foreach (ComboBoxItem item in cbInsertOptions.Items)
            {
                if (item.Tag.ToString() == keywordList.InsertOpt.ToString())
                {
                    item.IsSelected = true;
                    break;
                }
            }

            _tempKeywords = new List<string>();
            foreach (var word in keywordList.Keywords)
            {
                _tempKeywords.Add(word);
            }
            FillTextBoxByLinks(_tempKeywords, tbKeywords);
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
                    case "rbPrimay":
                        _chosenType = KeywordType.Primary;
                        break;
                    case "rbSecondary":
                        _chosenType = KeywordType.Secondary;
                        break;
                    case "rbGeneric":
                        _chosenType = KeywordType.Generic;
                        break;
                };
            }
        }

        /// <summary>
        /// Handler cbInsertOptions selectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbInsertOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbInsertOptions.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)cbInsertOptions.SelectedItem;
                _tempOption = Utilities.Helper.Parse<InsertLinkOptions>(selected.Tag.ToString()) ?? InsertLinkOptions.Random;
            }
        }

        /// <summary>
        /// Handler Cancel button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!SysSettings.Instance.IsStopNotify)
            {
                ConfirmMessageBox mesg = new ConfirmMessageBox(this, SystemMessage.CloseWarning);
                mesg.ShowDialog();
                if (mesg.Answer)
                    this.Close();
            }
            else
            {
                this.Close();
            }
        }

        /// <summary>
        /// Handler Save button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool isPassed = true;
            displayValidateTextBoxes.Invoke(this, _textBoxes, CheckAddNewKeywordList(ref isPassed));
            if (isPassed)
            {
                if (_openMode == "Add")
                {
                    KeywordList newItem = new KeywordList(tbKeywordListName.Text, _chosenType, _tempOption);
                    newItem.AddKeywords(tbKeywords.Text);

                    _cateRule.AddKeywordList(newItem);
                    this.Close();
                }
                else
                {
                    EditKeywordList();
                    this.Close();
                }
            }
        }

        #endregion

        #region ActionList

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

        #region UtilityMethod

        /// <summary>
        /// Fill chosen textbox with list of keywords
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
        /// Edit current selected KeywordList
        /// </summary>
        private void EditKeywordList()
        {
            _tempKeywordList.KeywordType = _chosenType;
            _tempKeywordList.InsertOpt = _tempOption;
            _tempKeywordList.AddKeywords(tbKeywords.Text);

            _cateRule.DeleteKeywordListByName(_tempKeywordList.Name);
            _cateRule.AddKeywordList(_tempKeywordList);
        }

        /// <summary>
        /// Check if desired name existed or not
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool CheckNameExisted(string name)
        {
            return _cateRule.CheckKeywordListName(name);
        }

        /// <summary>
        /// Check validate of adding new link list
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        private List<Tuple<string, bool>> CheckAddNewKeywordList(ref bool pass)
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
                        if (tb.Name == "tbKeywordListName")
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
