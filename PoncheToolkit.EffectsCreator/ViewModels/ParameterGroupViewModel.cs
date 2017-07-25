using PoncheToolkit.EffectsCreator.Commands;
using PoncheToolkit.EffectsCreator.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.EffectsCreator.ViewModels
{
    /// <summary>
    /// Class that define a group of parameters.
    /// </summary>
    public class ParameterGroupViewModel : MainWindowChildViewModelBase
    {
        private ObservableCollection<ParameterSingleViewModel> parameters;
        private string groupTitle;
        private bool isExpanded;

        /// <summary>
        /// The pairs of the parameters (name, value).
        /// </summary>
        public ObservableCollection<ParameterSingleViewModel> Parameters
        {
            get { return parameters; }
            set { SetProperty(ref parameters, value); }
        }

        /// <summary>
        /// The title that will be shown in the expander of the group.
        /// </summary>
        public string GroupTitle
        {
            get { return groupTitle; }
            set { SetProperty(ref groupTitle, value); }
        }

        /// <summary>
        /// The expanded property.
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public RelayCommand<ParameterSingle> ParameterValueChangedCommand { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ParameterGroupViewModel(MainWindow mainWindow)
            : base(mainWindow)
        {
            if (IsInDesignMode)
                return;

            Parameters = new ObservableCollection<ParameterSingleViewModel>();
            ParameterValueChangedCommand = new RelayCommand<ParameterSingle>(parameterValueChangedCommandExecution);
        }

        #region Commands execution
        private void parameterValueChangedCommandExecution(ParameterSingle param)
        {

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Method to check if a certain name has already been added.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsParameterName(string name)
        {
            foreach (ParameterSingleViewModel param in parameters)
            {
                if (param.ParameterName == name)
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override void InitializeDesignData()
        {
            Parameters = new ObservableCollection<ParameterSingleViewModel>();
            Array arr = Enum.GetValues(typeof(ParameterSingle.ParameterTypes));
            for (int i = 0; i < arr.Length; i++)
            {
                ParameterSingle.ParameterTypes type = (ParameterSingle.ParameterTypes)arr.GetValue((i + 1) % arr.Length);
                this.Parameters.Add(new ParameterSingleViewModel(MainWindow, "Param " + i + " type " + type, "Value " + i, type));
            }
            this.IsExpanded = true;
            this.GroupTitle = "Group 1";
        }
        #endregion
    }
}
