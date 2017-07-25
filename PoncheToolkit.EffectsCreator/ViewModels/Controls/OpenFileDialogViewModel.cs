using PoncheToolkit.EffectsCreator.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PoncheToolkit.EffectsCreator.ViewModels.Controls
{
    /// <summary>
    /// View model for the Custom Button.
    /// </summary>
    public class OpenFileDialogViewModel : CustomButtonViewModel
    {
        private string filter;

        /// <summary>
        /// Text property.
        /// </summary>
        public string Filter
        {
            get { return filter; }
            set { SetProperty(ref filter, value); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public OpenFileDialogViewModel(string text, string filter, Action<object> clickCommandAction)
            : base(text, clickCommandAction)
        {
            this.Filter = filter;
        }

    }
}
