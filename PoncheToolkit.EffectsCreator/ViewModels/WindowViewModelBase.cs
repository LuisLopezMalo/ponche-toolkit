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
    public abstract class WindowViewModelBase : ObservableObject
    {
        /// <summary>
        /// Property to get if the application is running in design mode.
        /// </summary>
        public bool IsInDesignMode
        {
            get
            {
                return DesignerProperties.GetIsInDesignMode(new DependencyObject());
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        [DesignOnly(true)]
        public WindowViewModelBase()
        {
            if (IsInDesignMode)
                InitializeDesignData();
        }

        /// <summary>
        /// Set the design data.
        /// </summary>
        public virtual void InitializeDesignData()
        {

        }
    }
}
