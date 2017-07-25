using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace PoncheToolkit.EffectsCreator.Util
{
    /// <summary>
    /// Convert a string to xaml format to be used with the CustomRichTextBox.
    /// </summary>
    public class StringToXamlConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var flowDocument = new FlowDocument();
                var xaml = value.ToString();

                using (var stream = new MemoryStream((new ASCIIEncoding()).GetBytes(xaml)))
                {
                    var text = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                    text.Load(stream, DataFormats.Xaml);
                }

                return flowDocument;
            }
            return value;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var flowDocument = (FlowDocument)value;
                var range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                string xaml;

                using (var stream = new MemoryStream())
                {
                    range.Save(stream, TextDataFormat.Xaml.ToString());
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream))
                    {
                        xaml = reader.ReadToEnd();
                    }
                }

                return xaml;
            }
            return value;
        }
    }
}
