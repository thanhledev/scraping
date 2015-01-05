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
namespace Scraping
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class Message : Window
    {
        private List<string> _messages = new List<string>();

        public Message(List<string> message)
            : this(message, null)
        {

        }

        public Message(List<string> message, Window owner)
        {
            InitializeComponent();

            if (owner != null)
            {
                this.Owner = owner;

                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            _messages = message;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Message_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var i in _messages)
            {
                tbMessage.AppendText(i);
                tbMessage.AppendText(Environment.NewLine);
            }
        }
    }
}
