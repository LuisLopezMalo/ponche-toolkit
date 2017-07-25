using Microsoft.Win32;
using PoncheToolkit.EffectsCreator.Commands;
using PoncheToolkit.EffectsCreator.Resources;
using PoncheToolkit.EffectsCreator.Views;
using PoncheToolkit.EffectsCreator.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PoncheToolkit.EffectsCreator.Controls;

namespace PoncheToolkit.EffectsCreator.ViewModels
{
    /// <summary>
    /// ViewModel representing the menu data.
    /// </summary>
    public class ParametersViewModel : MainWindowChildViewModelBase
    {
        private ObservableCollection<ParameterGroupViewModel> parameterGroups;

        /// <summary>
        /// The collection of group of parameters.
        /// </summary>
        public ObservableCollection<ParameterGroupViewModel> ParameterGroups
        {
            get { return parameterGroups; }
            set { SetProperty(ref parameterGroups, value); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ParametersViewModel(MainWindow mainWindow)
            : base(mainWindow)
        {
            if (IsInDesignMode)
                return;

            ParameterGroups = new ObservableCollection<ParameterGroupViewModel>();
        }

        /// <inheritdoc/>
        public override void InitializeDesignData()
        {
            ParameterGroups = new ObservableCollection<ParameterGroupViewModel>();
            Array arr = Enum.GetValues(typeof(ParameterSingle.ParameterTypes));

            for (int i = 0; i < 5; i++)
            {
                ParameterGroupViewModel group = new ParameterGroupViewModel(MainWindow);
                group.InitializeDesignData();
                group.GroupTitle = "Group " + i;
                ParameterGroups.Add(group);
            }
        }
    }
}
