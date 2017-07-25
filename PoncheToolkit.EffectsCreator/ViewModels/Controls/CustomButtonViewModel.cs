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
    public class CustomButtonViewModel : MainWindowChildViewModelBase
    {
        private string text;

        /// <summary>
        /// Text property.
        /// </summary>
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        /// <summary>
        /// Command executed when clicked.
        /// </summary>
        public RelayCommand<object> ClickCommand { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomButtonViewModel(string text, Action<object> clickCommandAction)
        {
            this.Text = text;
            ClickCommand = new RelayCommand<object>(clickCommandAction);
        }

    }
}
