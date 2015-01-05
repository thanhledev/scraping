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
using DataTypes.Enums;
using Utilities;
using System.IO;
using DataTypes.Collections;
using System.Text.RegularExpressions;

namespace Scraping
{
    /// <summary>
    /// Interaction logic for CreateProject.xaml
    /// </summary>
    public partial class CreateProject : Window
    {
        private bool _result;

        public bool Result
        {
            get { return _result; }            
        }

        private string _newProjectName;

        public string NewProjectName
        {
            get { return _newProjectName; }            
        }

        public CreateProject(Window owner)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;                          
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (SysSettings.Instance.GetProject(tbName.Text) == null)
            {
                PostingProject temp = new PostingProject(tbName.Text);
                SysSettings.Instance.AddProject(temp);
                _newProjectName = tbName.Text;
                _result = true;
            }
            
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _result = false;
            this.Close();
        }        
    }
}
