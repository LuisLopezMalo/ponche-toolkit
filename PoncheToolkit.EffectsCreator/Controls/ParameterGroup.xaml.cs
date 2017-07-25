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
    /// Interaction logic for GroupParameter.xaml
    /// </summary>
    public partial class ParameterGroup : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ParameterGroup()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The parameter name property
        /// </summary>
        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register("Parameters",
                typeof(List<ParameterSingle>),
                typeof(ParameterGroup));

        /// <summary>
        /// The parameter name.
        /// </summary>
        [Browsable(true)]
        [Category("CustomProps")]
        [Description("Parameters")]
        public List<ParameterSingle> Parameters
        {
            get { return (List<ParameterSingle>)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        /// <summary>
        /// The he group title header property
        /// </summary>
        public static readonly DependencyProperty GroupTitleProperty =
            DependencyProperty.Register("GroupTitle",
                typeof(string),
                typeof(ParameterGroup));

        /// <summary>
        /// The group title header to display in the expander.
        /// </summary>
        [Browsable(true)]
        [Category("CustomProps")]
        [Description("GroupTitle")]
        public string GroupTitle
        {
            get { return (string)GetValue(GroupTitleProperty); }
            set { SetValue(GroupTitleProperty, value); }
        }
    }
}
