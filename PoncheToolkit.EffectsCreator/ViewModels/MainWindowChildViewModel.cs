using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.EffectsCreator.ViewModels
{
    /// <summary>
    /// Represent a view model that is contained in the MainWindow.
    /// </summary>
    public class MainWindowChildViewModel : ViewModelBase
    {
        private MainWindow mainWindow;

        /// <summary>
        /// The MainWindow reference.
        /// </summary>
        public MainWindow MainWindow
        {
            get { return mainWindow; }
            set { SetProperty(ref mainWindow, value); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindowChildViewModel()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="window">The MainWindow reference to be kept within the view model</param>
        public MainWindowChildViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
    }
}
