using PoncheToolkit.EffectsCreator.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PoncheToolkit.EffectsCreator.ViewModels
{
    /// <summary>
    /// Represent a view model that is contained in the MainWindow.
    /// </summary>
    public abstract class MainWindowChildViewModelBase : WindowViewModelBase
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
        public MainWindowChildViewModelBase()
        {
            if (IsInDesignMode)
                InitializeDesignData();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mainWindow">The MainWindow reference to be kept within the view model</param>
        public MainWindowChildViewModelBase(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
    }
}
