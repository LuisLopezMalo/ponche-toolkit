using Microsoft.Win32;
using PoncheToolkit.EffectsCreator.Commands;
using PoncheToolkit.EffectsCreator.ViewModels;
using PoncheToolkit.EffectsCreator.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PoncheToolkit.EffectsCreator.Controls
{
    /// <summary>
    /// Interaction logic for SingleParameter.xaml
    /// </summary>
    public partial class ParameterSingle : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ParameterSingle()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The parameter name property
        /// </summary>
        public static readonly DependencyProperty ParameterNameProperty =
            DependencyProperty.Register("ParameterName",
                typeof(string),
                typeof(ParameterSingle));

        /// <summary>
        /// The parameter name.
        /// </summary>
        [Browsable(true)]
        [Category("CustomProps")]
        [Description("ParameterName")]
        public string ParameterName
        {
            get { return (string)GetValue(ParameterNameProperty); }
            set { SetValue(ParameterNameProperty, value); }
        }

        /// <summary>
        /// The parameter value property.
        /// </summary>
        public static readonly DependencyProperty ParameterValueProperty =
            DependencyProperty.Register("ParameterValue",
                typeof(string),
                typeof(ParameterSingle));

        /// <summary>
        /// The parameter value.
        /// </summary>
        [Browsable(true)]
        [Category("CustomProps")]
        [Description("ParameterValue")]
        public string ParameterValue
        {
            get { return (string)GetValue(ParameterValueProperty); }
            set { SetValue(ParameterValueProperty, value); }
        }


        /// <summary>
        /// The parameter value property.
        /// </summary>
        public static readonly DependencyProperty ParameterTypeProperty =
            DependencyProperty.Register("ParameterType",
                typeof(ParameterTypes),
                typeof(ParameterSingle),
                new FrameworkPropertyMetadata(ParameterTypes.None,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnParameterTypeChanged)
              ));

        /// <summary>
        /// Event raised when the property is created or changed.
        /// Here the ParameterType is set so the type of control and its contets are created.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnParameterTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ParameterSingle obj = d as ParameterSingle;
            if (e.Property.PropertyType == typeof(ParameterTypes))
            {
                obj.ParameterType = (ParameterTypes)e.NewValue;
            }
        }

        /// <summary>
        /// The parameter value.
        /// </summary>
        [Browsable(true)]
        [Category("CustomProps")]
        [Description("ParameterType")]
        public ParameterTypes ParameterType
        {
            get { return (ParameterTypes)GetValue(ParameterTypeProperty); }
            set
            {
                SetValue(ParameterTypeProperty, value);
            }
        }

        /// <summary>
        /// Enumeration with the types of parameters.
        /// </summary>
        public enum ParameterTypes
        {
            /// <summary>
            /// Draws nothing.
            /// </summary>
            None,
            /// <summary>
            /// Draw an uneditable text box.
            /// </summary>
            Uneditable,
            /// <summary>
            /// Draw a text box.
            /// </summary>
            Text,
            /// <summary>
            /// Draw a text box.
            /// </summary>
            Integer,
            /// <summary>
            /// Draw a combo box.
            /// </summary>
            Combo,
            /// <summary>
            /// Put a file dialog.
            /// </summary>
            File,
            /// <summary>
            /// Create a Vector2 object.
            /// </summary>
            Vector2,
            /// <summary>
            /// Create a Vector3 object.
            /// </summary>
            Vector3,
            /// <summary>
            /// Create a Vector4 object.
            /// </summary>
            Vector4
        }


        /// <summary>
        /// The parameter value property.
        /// </summary>
        public static readonly DependencyProperty CustomContentProperty =
            DependencyProperty.Register("CustomContent",
                typeof(Control),
                typeof(ParameterSingle));

        /// <summary>
        /// The custom content to be shown depending on the selected ParameterType.
        /// </summary>
        [Browsable(false)]
        [Category("CustomProps")]
        [Description("CustomContent")]
        public Control CustomContent
        {
            get
            {
                return (Control)GetValue(CustomContentProperty);
            }
            set
            {
                SetValue(CustomContentProperty, value);
            }
        }
    }
}
