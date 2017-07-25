using PoncheToolkit.EffectsCreator.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PoncheToolkit.EffectsCreator.Controls
{
    /// <summary>
    /// Interaction logic for CustomRichTextBox.xaml
    /// </summary>
    public partial class CustomRichTextBox : RichTextBox
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomRichTextBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document"></param>
        public CustomRichTextBox(FlowDocument document) 
            : base(document)
        {
            InitializeComponent();
        }

        /// <summary>
        /// The selected text property
        /// </summary>
        public static readonly DependencyProperty TextSelectionProperty =
            DependencyProperty.Register("TextSelection", typeof(TextSelection),
            typeof(CustomRichTextBox));

        /// <summary>
        /// The selected text.
        /// </summary>
        [Browsable(true)]
        [Category("CustomProps")]
        [Description("TextSelection")]
        public TextSelection TextSelection
        {
            get { return (TextSelection)GetValue(TextSelectionProperty); }
            set { SetValue(TextSelectionProperty, value); }
        }

        /// <summary>
        /// Text attached property.
        /// </summary>
        private static DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CustomRichTextBox));

        /// <summary>
        /// Get or set the bindable text property for the RichTextBox.
        /// </summary>
        [Category("CustomProps")]
        [Browsable(true)]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// The Document dependency property.
        /// </summary>
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(FlowDocument), typeof(CustomRichTextBox));

        /// <summary>
        /// Get or set the Document property.
        /// </summary>
        [Category("CustomProps")]
        [Browsable(true)]
        public new FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        /// <summary>
        /// The content obtained from the RichTextBox.
        /// </summary>
        public string RTBContent
        {
            get
            {
                TextRange range = new TextRange(Document.ContentStart, Document.ContentEnd);

                using (var ms = new MemoryStream())
                {
                    range.Save(ms, DataFormats.Text);
                    return ASCIIEncoding.Default.GetString(ms.ToArray());
                }
            }
            set
            {
                TextRange range = new TextRange(Document.ContentStart, Document.ContentEnd);
                setRangeContent(range, value);
                Size textSize = FrameworkHelper.MeasureString(range.Text, FontFamily, FontStyle, FontWeight, FontStretch, FontSize);
                //if (textSize.Width > this.Document.PageWidth)
                //    this.Document.PageWidth = textSize.Width;

                if ((string)GetValue(TextBox.TextProperty) != value)
                    SetValue(TextBox.TextProperty, value);
            }
        }


        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"></see> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"></see> is set to true internally.
        /// </summary>
        /// <param title="e">The <see cref="T:System.Windows.RoutedEventArgs"></see> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Hook up to get notified when TextProperty changes.
            var descriptor = DependencyPropertyDescriptor.FromProperty(TextProperty, typeof(TextBox));
            descriptor.AddValueChanged(this, delegate
            {
                if (RTBContent != Text)
                    RTBContent = Text == null ? string.Empty : Text;
            });

            descriptor = DependencyPropertyDescriptor.FromProperty(DocumentProperty, typeof(CustomRichTextBox));
            descriptor.AddValueChanged(this, delegate
            {
                // If the underlying value of the dependency property changes,
                // update the underlying document, also.
                base.Document = (FlowDocument)GetValue(DocumentProperty);
            });

            TextChanged += delegate
            {
                if (Text != RTBContent)
                    Text = RTBContent;
            };

            //this.Document.MinPageWidth = 2000;
            //this.Document.PageWidth = this.Width;

            this.SizeChanged += (obj, args) =>
            {
                this.Document.PageWidth = this.ActualWidth;
            };
        }

        #region Private Methods
        private string setRangeContent(TextRange selection, string content)
        {
            string loadType = DataFormats.Rtf;
            content = content == "" ? " " : content;
            try
            {
                using (MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(content)))
                {
                    if (content.StartsWith("<Section"))
                    {
                        if (selection.CanLoad(DataFormats.Xaml))
                        {
                            selection.Load(ms, DataFormats.Xaml);
                            loadType = DataFormats.Xaml;
                        }
                    }
                    else if (content.StartsWith("PK"))
                    {
                        if (selection.CanLoad(DataFormats.XamlPackage))
                        {
                            selection.Load(ms, DataFormats.XamlPackage);
                            loadType = DataFormats.XamlPackage;
                        }
                    }
                    else if (content.StartsWith("{\\rtf"))
                    {
                        if (selection.CanLoad(DataFormats.Rtf))
                        {
                            selection.Load(ms, DataFormats.Rtf);
                            loadType = DataFormats.Rtf;
                        }
                    }
                    else
                    {
                        if (selection.CanLoad(DataFormats.Text))
                        {
                            selection.Load(ms, DataFormats.Text);
                            loadType = DataFormats.Text;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return loadType;
        }
        #endregion
    }
}
