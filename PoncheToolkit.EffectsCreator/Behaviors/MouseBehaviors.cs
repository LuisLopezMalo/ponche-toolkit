using PoncheToolkit.EffectsCreator.Commands;
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
    /// Class that contains common mouse behaviors and commands.
    /// </summary>
    public class MouseBehaviors
    {
        #region Left button up
        /// <summary>
        /// Property for left mouse button up.
        /// </summary>
        public static readonly DependencyProperty MouseLeftButtonUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseLeftButtonUpCommand", 
                typeof(ICommand),
                typeof(MouseBehaviors), 
                new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseLeftButtonUpCommandChanged)));

        private static void MouseLeftButtonUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;
            element.MouseLeftButtonUp += new MouseButtonEventHandler(element_MouseUp);
        }

        private static void element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            ICommand command = GetMouseLeftButtonUpCommand(element);
            command.Execute(e);
        }

        /// <summary>
        /// Set the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetMouseLeftButtonUpCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseLeftButtonUpCommandProperty, value);
        }

        /// <summary>
        /// Get the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        public static ICommand GetMouseLeftButtonUpCommand(UIElement element)
        {
            ICommand com = (ICommand)element.GetValue(MouseLeftButtonUpCommandProperty);
            return com;
        }
        #endregion

        #region Left button down
        /// <summary>
        /// Property for left mouse button up.
        /// </summary>
        public static readonly DependencyProperty MouseLeftButtonDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseLeftButtonDownCommand",
                typeof(ICommand),
                typeof(MouseBehaviors),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseLeftButtonDownCommandChanged)));

        private static void MouseLeftButtonDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;
            element.MouseLeftButtonDown += new MouseButtonEventHandler(element_MouseDown);
        }

        private static void element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            ICommand command = GetMouseLeftButtonDownCommand(element);
            command.Execute(e);
        }

        /// <summary>
        /// Set the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetMouseLeftButtonDownCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseLeftButtonDownCommandProperty, value);
        }

        /// <summary>
        /// Get the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        public static ICommand GetMouseLeftButtonDownCommand(UIElement element)
        {
            ICommand com = (ICommand)element.GetValue(MouseLeftButtonDownCommandProperty);
            return com;
        }
        #endregion

        #region Mouse move
        /// <summary>
        /// Property for left mouse button up.
        /// </summary>
        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.RegisterAttached("MouseMoveCommand",
                typeof(ICommand),
                typeof(MouseBehaviors),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseMoveCommandChanged)));

        private static void MouseMoveCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;
            element.MouseMove += Element_MouseMove;
        }

        private static void Element_MouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            ICommand command = GetMouseMoveCommand(element);
            command.Execute(e);
        }

        /// <summary>
        /// Set the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetMouseMoveCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseMoveCommandProperty, value);
        }

        /// <summary>
        /// Get the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        public static ICommand GetMouseMoveCommand(UIElement element)
        {
            ICommand com = (ICommand)element.GetValue(MouseMoveCommandProperty);
            return com;
        }
        #endregion

        #region Mouse wheel
        /// <summary>
        /// Property for mouse wheel change.
        /// </summary>
        public static readonly DependencyProperty MouseWheelCommandProperty =
            DependencyProperty.RegisterAttached("MouseWheelCommand",
                typeof(ICommand),
                typeof(MouseBehaviors),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseWheelCommandChanged)));

        private static void MouseWheelCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;
            element.MouseWheel += Element_MouseWheel;
        }

        private static void Element_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            ICommand command = GetMouseWheelCommand(element);
            command.Execute(e);
        }

        /// <summary>
        /// Set the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetMouseWheelCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseWheelCommandProperty, value);
        }

        /// <summary>
        /// Get the left mouse button up property.
        /// </summary>
        /// <param name="element"></param>
        public static ICommand GetMouseWheelCommand(UIElement element)
        {
            ICommand com = (ICommand)element.GetValue(MouseWheelCommandProperty);
            return com;
        }
        #endregion
    }
}
