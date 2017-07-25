using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PoncheToolkit.EffectsCreator.Util
{
    /// <summary>
    /// Class to help retrieve certain elements in the tree.
    /// </summary>
    public class FrameworkHelper
    {
        /// <summary>
        /// Find the parent for the given type.
        /// </summary>
        /// <typeparam name="T">Type of the parent to look for.</typeparam>
        /// <param name="element">Element</param>
        /// <returns>Retrn the parent.</returns>
        public static DependencyObject FindParent<T>(DependencyObject element) where T : DependencyObject
        {
            if (element == null)
                throw new NullReferenceException("The root element to find its parent is null.");

            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(element);
            if (parentObject == null)
                return null;

            // check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        /// <summary>
        /// Measure a string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontFamily"></param>
        /// <param name="fontStyle"></param>
        /// <param name="fontWeight"></param>
        /// <param name="fontStretch"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static Size MeasureString(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                fontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
