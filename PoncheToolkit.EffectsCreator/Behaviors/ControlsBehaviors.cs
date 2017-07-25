using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PoncheToolkit.EffectsCreator.Behaviors
{
    /// <summary>
    /// Class that contains common windows or controls behaviors and commands.
    /// </summary>
    public class ControlsBehaviors
    {
        #region Mouse wheel
        /// <summary>
        /// Property for mouse wheel change.
        /// </summary>
        public static readonly DependencyProperty SizeChangedCommandProperty =
            DependencyProperty.RegisterAttached("SizeChangedCommand",
                typeof(ICommand),
                typeof(ControlsBehaviors),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(SizeChangedCommandChanged)));

        private static void SizeChangedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;
            element.SizeChanged += Element_SizeChanged;
        }

        private static void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            ICommand command = GetSizeChangedCommand(element);
            command.Execute(e);
        }

        /// <summary>
        /// Set the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetSizeChangedCommand(UIElement element, ICommand value)
        {
            element.SetValue(SizeChangedCommandProperty, value);
        }

        /// <summary>
        /// Get the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        public static ICommand GetSizeChangedCommand(UIElement element)
        {
            ICommand com = (ICommand)element.GetValue(SizeChangedCommandProperty);
            return com;
        }
        #endregion
    }
}
