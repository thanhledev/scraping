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

namespace Scraping
{
    /// <summary>
    /// Interaction logic for ErrorMessage.xaml
    /// </summary>
    public partial class ErrorMessage : Window
    {
        /// <summary>
        /// The exception and header message cannot be null.  If owner is specified, this window
        /// uses its Style and will appear centered on the Owner.  You can override this before
        /// calling ShowDialog().
        /// </summary>
        public ErrorMessage(List<string> errorMessages)
            : this(errorMessages, null)
        {

        }
        /// <summary>
        /// The header message cannot be null.  If owner is specified, this window
        /// uses its Style and will appear centered on the Owner.  You can override this before
        /// calling ShowDialog().
        /// </summary>
        public ErrorMessage(List<string> errorMessages, Window owner)
        {
            InitializeComponent();

            if (owner != null)
            {
                // This hopefully makes our window look like it belongs to the main app.
                //this.Style = owner.Style;

                // This seems to make the window appear on the same monitor as the owner.
                this.Owner = owner;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }

            //Create document
            LoadErrorDocument(errorMessages);
        }

        private void LoadErrorDocument(List<string> errorMessages)
        {
            BrushConverter bc = new BrushConverter();
            var doc = new FlowDocument();
            doc.FontSize = 10;
            doc.TextAlignment = TextAlignment.Left;
            doc.Background = mainGrid.Background;
            
            foreach(var i in errorMessages)
            {
                Paragraph para = new Paragraph();
                para.FontSize = 12;
                para.Foreground = (Brush)bc.ConvertFrom("Gray");
                para.Inlines.Add(new Run(i));

                doc.Blocks.Add(para);
            }            
            ErrorDoc.Document = doc;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
