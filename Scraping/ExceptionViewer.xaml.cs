using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.IO;
using System.Collections;

namespace Scraping
{
    /// <summary>
    /// Interaction logic for ExceptionViewer.xaml
    /// </summary>
    public partial class ExceptionViewer : Window
    {
        /// <summary>
        /// The exception and header message cannot be null.  If owner is specified, this window
        /// uses its Style and will appear centered on the Owner.  You can override this before
        /// calling ShowDialog().
        /// </summary>
        public ExceptionViewer(string headerMessage, Exception e) 
            : this(headerMessage, e, null)
        {            
        }

        /// <summary>
        /// The exception and header message cannot be null.  If owner is specified, this window
        /// uses its Style and will appear centered on the Owner.  You can override this before
        /// calling ShowDialog().
        /// </summary>
        public ExceptionViewer(string headerMessage, Exception e, Window owner)
        {
            InitializeComponent();

            if (owner != null)
            {
                // This hopefully makes our window look like it belongs to the main app.
                //this.Style = owner.Style;

                // This seems to make the window appear on the same monitor as the owner.
                this.Owner = owner;

                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            ExceptionViewerWindow.Title = DefaultTitle;
            BuildTree(e, headerMessage);
        }

        static string _defaultTitle;
        static string _product;

        /// <summary>
        /// The default title to use for the ExceptionViewer window.  Automatically initialized 
        /// to "Error - [ProductName]" where [ProductName] is taken from the application's
        /// AssemblyProduct attribute (set in the AssemblyInfo.cs file).  You can change this
        /// default, or ignore it and set Title yourself before calling ShowDialog().
        /// </summary>
        public static string DefaultTitle
        {
            get
            {
                if (_defaultTitle == null)
                {
                    if (string.IsNullOrEmpty(Product))
                    {
                        _defaultTitle = "Error";
                    }
                    else
                    {
                        _defaultTitle = "Error - " + Product;
                    }
                }

                return _defaultTitle;
            }

            set
            {
                _defaultTitle = value;
            }
        }

        /// <summary>
        /// Gets the value of the AssemblyProduct attribute of the app.  
        /// If unable to lookup the attribute, returns an empty string.
        /// </summary>
        public static string Product
        {
            get
            {
                if (_product == null)
                {
                    _product = GetProductName();
                }

                return _product;
            }
        }
        /// <summary>
        /// Initializes the Product property..
        /// </summary>        
        static string GetProductName()
        {
            string result = "";

            try
            {
                Assembly _appAssembly = GetAppAssembly();

                object[] customAttributes = _appAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    result = ((AssemblyProductAttribute)customAttributes[0]).Product;
                }
            }
            catch
            { }

            return result;
        }
        /// <summary>
        // Tries to get the assembly to extract the product name from.
        /// </summary>
        private static Assembly GetAppAssembly()
        {
            Assembly _appAssembly = null;

            try
            {
                // This is supposedly how Windows.Forms.Application does it.
                _appAssembly = Application.Current.MainWindow.GetType().Assembly;
            }
            catch
            { }

            // If the above didn't work, try less desireable ways to get an assembly.

            if (_appAssembly == null)
            {
                _appAssembly = Assembly.GetEntryAssembly();
            }

            if (_appAssembly == null)
            {
                _appAssembly = Assembly.GetExecutingAssembly();
            }

            return _appAssembly;
        }
        /// <summary>
        /// Builds the tree in the left pane.
        /// Each TreeViewItem.Tag will contain a list of Inlines
        /// to display in the right-hand pane When it is selected.
        /// </summary>
        void BuildTree(Exception e, string summaryMessage)
        {
            // The first node in the tree contains the summary message and all the
            // nested exception messages.

            var inlines = new List<Inline>();
            var firstItem = new TreeViewItem();
            firstItem.Header = "All Messages";
            firstItem.Foreground = new SolidColorBrush(Colors.Gray);
            treeView1.Items.Add(firstItem);

            var inline = new Bold(new Run(summaryMessage));
            inline.FontSize = 18;
            inlines.Add(inline);

            // Now add top-level nodes for each exception while building
            // the contents of the first node.
            while (e != null)
            {
                inlines.Add(new LineBreak());
                inlines.Add(new LineBreak());
                AddLines(inlines, e.Message);

                AddException(e);
                e = e.InnerException;
            }

            firstItem.Tag = inlines;
            firstItem.IsSelected = true;
        }

        void AddProperty(List<Inline> inlines, string propName, object propVal)
        {
            inlines.Add(new LineBreak());
            inlines.Add(new LineBreak());
            var inline = new Bold(new Run(propName + ":"));
            inline.FontSize = 14;
            inlines.Add(inline);
            inlines.Add(new LineBreak());

            if (propVal is string)
            {
                // Might have embedded newlines.

                AddLines(inlines, propVal as string);
            }
            else
            {
                inlines.Add(new Run(propVal.ToString()));
            }
        }
        /// <summary>
        /// Adds the string to the list of Inlines, substituting
        /// LineBreaks for an newline chars found.
        /// </summary>
        void AddLines(List<Inline> inlines, string str)
        {
            string[] lines = str.Split('\n');

            inlines.Add(new Run(lines[0].Trim('\r')));

            foreach (string line in lines.Skip(1))
            {
                inlines.Add(new LineBreak());
                inlines.Add(new Run(line.Trim('\r')));
            }
        }

        // Adds the exception as a new top-level node to the tree with child nodes
        // for all the exception's properties.
        void AddException(Exception e)
        {
            // Create a list of Inlines containing all the properties of the exception object.
            // The three most important properties (message, type, and stack trace) go first.

            var exceptionItem = new TreeViewItem();
            var inlines = new List<Inline>();
            System.Reflection.PropertyInfo[] properties = e.GetType().GetProperties();

            exceptionItem.Header = e.GetType();
            exceptionItem.Tag = inlines;
            exceptionItem.Foreground = new SolidColorBrush(Colors.Gray);
            treeView1.Items.Add(exceptionItem);

            Inline inline = new Bold(new Run(e.GetType().ToString()));
            inline.FontSize = 18;
            inlines.Add(inline);

            AddProperty(inlines, "Message", e.Message);
            AddProperty(inlines, "Stack Trace", e.StackTrace);

            foreach (PropertyInfo info in properties)
            {
                // Skip InnerException because it will get a whole
                // top-level node of its own.

                if (info.Name != "InnerException")
                {
                    var value = info.GetValue(e, null);

                    if (value != null)
                    {
                        if (value is string)
                        {
                            if (string.IsNullOrEmpty(value as string)) continue;
                        }
                        else if (value is IDictionary)
                        {
                            value = RenderDictionary(value as IDictionary);
                            if (string.IsNullOrEmpty(value as string)) continue;
                        }
                        else if (value is IEnumerable && !(value is string))
                        {
                            value = RenderEnumerable(value as IEnumerable);
                            if (string.IsNullOrEmpty(value as string)) continue;
                        }

                        if (info.Name != "Message" &&
                            info.Name != "StackTrace")
                        {
                            // Add the property to list for the exceptionItem.
                            AddProperty(inlines, info.Name, value);
                        }

                        // Create a TreeViewItem for the individual property.
                        var propertyItem = new TreeViewItem();
                        var propertyInlines = new List<Inline>();

                        propertyItem.Header = info.Name;
                        propertyItem.Tag = propertyInlines;
                        propertyItem.Foreground = new SolidColorBrush(Colors.Gray);
                        exceptionItem.Items.Add(propertyItem);
                        AddProperty(propertyInlines, info.Name, value);
                    }
                }
            }
        }

        static string RenderEnumerable(IEnumerable data)
        {
            StringBuilder result = new StringBuilder();

            foreach (object obj in data)
            {
                result.AppendFormat("{0}\n", obj);
            }

            if (result.Length > 0) result.Length = result.Length - 1;
            return result.ToString();
        }

        static string RenderDictionary(IDictionary data)
        {
            StringBuilder result = new StringBuilder();

            foreach (object key in data.Keys)
            {
                if (key != null && data[key] != null)
                {
                    result.AppendLine(key.ToString() + " = " + data[key].ToString());
                }
            }

            if (result.Length > 0) result.Length = result.Length - 1;
            return result.ToString();
        }

        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ShowCurrentItem();
        }

        void ShowCurrentItem()
        {
            if (treeView1.SelectedItem != null)
            {
                var inlines = (treeView1.SelectedItem as TreeViewItem).Tag as List<Inline>;
                var doc = new FlowDocument();

                doc.FontSize = 10;
                doc.FontFamily = treeView1.FontFamily;
                doc.TextAlignment = TextAlignment.Left;
                doc.Background = docViewer.Background;                

                var para = new Paragraph();
                para.Inlines.AddRange(inlines);
                doc.Blocks.Add(para);
                docViewer.Document = doc;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            // Build a FlowDocument with Inlines from all top-level tree items.

            var inlines = new List<Inline>();
            var doc = new FlowDocument();
            var para = new Paragraph();

            doc.FontSize = 10;
            doc.FontFamily = treeView1.FontFamily;
            doc.TextAlignment = TextAlignment.Left;

            foreach (TreeViewItem treeItem in treeView1.Items)
            {
                if (inlines.Any())
                {
                    // Put a line of underscores between each exception.

                    inlines.Add(new LineBreak());
                    inlines.Add(new Run("____________________________________________________"));
                    inlines.Add(new LineBreak());
                }

                inlines.AddRange(treeItem.Tag as List<Inline>);
            }

            para.Inlines.AddRange(inlines);
            doc.Blocks.Add(para);

            // Now place the doc contents on the clipboard in both
            // rich text and plain text format.

            TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
            DataObject data = new DataObject();

            using (Stream stream = new MemoryStream())
            {
                range.Save(stream, DataFormats.Rtf);
                data.SetData(DataFormats.Rtf, Encoding.UTF8.GetString((stream as MemoryStream).ToArray()));
            }

            data.SetData(DataFormats.StringFormat, range.Text);
            Clipboard.SetDataObject(data);

            // The Inlines that were being displayed are now in the temporary document we just built,
            // causing them to disappear from the viewer.  This puts them back.

            ShowCurrentItem();
        }
    }
}
