using Microsoft.Win32;
using PoncheToolkit.Core.Components;
using PoncheToolkit.EffectsCreator.Commands;
using PoncheToolkit.EffectsCreator.Controls;
using PoncheToolkit.EffectsCreator.ViewModels.Controls;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.EffectsCreator.ViewModels
{
    /// <summary>
    /// View model for a single parameter.
    /// </summary>
    public class ParameterSingleViewModel : MainWindowChildViewModelBase
    {
        private string parameterName;
        private object parameterValue;
        private ParameterSingle.ParameterTypes parameterType;
        private object customContent;

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string ParameterName
        {
            get { return parameterName; }
            set { SetProperty(ref parameterName, value); }
        }

        /// <summary>
        /// Value of the parameter.
        /// </summary>
        public object ParameterValue
        {
            get { return parameterValue; }
            set { SetProperty(ref parameterValue, value); }
        }

        /// <summary>
        /// Type of the parameter.
        /// </summary>
        public ParameterSingle.ParameterTypes ParameterType
        {
            get { return parameterType; }
            set
            {
                SetProperty(ref parameterType, value);

                CustomTextBox txtBox = null;
                switch (value)
                {
                    case ParameterSingle.ParameterTypes.Text:
                        txtBox = new CustomTextBox();
                        CustomContent = txtBox;
                        break;
                    case ParameterSingle.ParameterTypes.File:
                        OpenFileDialogButton btn = new OpenFileDialogButton();
                        btn.DataContext = new OpenFileDialogViewModel("Set File", "All Files|*", openDialogCommand);
                        CustomContent = btn;
                        break;
                    case ParameterSingle.ParameterTypes.Vector2:
                        CustomButton btnVector2 = new CustomButton();
                        btnVector2.DataContext = new CustomButtonViewModel("Create Vector2", createVector3Command);
                        CustomContent = btnVector2;
                        break;
                    case ParameterSingle.ParameterTypes.Vector3:
                        CustomButton btnVector3 = new CustomButton();
                        btnVector3.DataContext = new CustomButtonViewModel("Create Vector3", createVector3Command);
                        CustomContent = btnVector3;
                        break;
                    case ParameterSingle.ParameterTypes.Vector4:
                        CustomButton btnVector4 = new CustomButton();
                        btnVector4.DataContext = new CustomButtonViewModel("Create Vector4", createVector3Command);
                        CustomContent = btnVector4;
                        break;
                    case ParameterSingle.ParameterTypes.Combo:
                    case ParameterSingle.ParameterTypes.Uneditable:
                    default:
                        txtBox = new CustomTextBox();
                        txtBox.IsEnabled = false;
                        CustomContent = txtBox;
                        break;
                }
            }
        }

        #region Commands
        private void openDialogCommand(object obj)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = (obj as OpenFileDialogViewModel).Filter;
            if (dialog.ShowDialog() == true)
            {
                MainWindowViewModel context = (this.MainWindow.DataContext as MainWindowViewModel);
                foreach (PTModel comp in context.RenderViewModel.GameWpf.CurrentScreen.Components.Values.Where(c => c is PTModel))
                {
                    //foreach (Mesh mesh in comp.Meshes)
                    //    mesh.AddTexturePath(new TexturePath(dialog.FileName), 0);
                }
            }
        }

        private void createVector3Command(object obj)
        {
            CustomButtonViewModel context = (CustomButtonViewModel)obj;

        }
        #endregion

        /// <summary>
        /// The content to be shown inside the parameter.
        /// </summary>
        public object CustomContent
        {
            get { return customContent; }
            set { SetProperty(ref customContent, value); }
        }

        /// <summary>
        /// Command when finished loading control.
        /// </summary>
        public RelayCommand<ParameterSingleViewModel> LoadedCommand { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ParameterSingleViewModel(MainWindow mainWindow)
           : base(mainWindow)
        {
            LoadedCommand = new RelayCommand<ParameterSingleViewModel>(control_loadedCommandExecution);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        public ParameterSingleViewModel(MainWindow mainWindow, string parameterName, string parameterValue)
            : this(mainWindow)
        {
            this.parameterName = parameterName;
            this.parameterValue = parameterValue;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <param name="type"></param>
        public ParameterSingleViewModel(MainWindow mainWindow, string parameterName, string parameterValue, ParameterSingle.ParameterTypes type)
            : this(mainWindow, parameterName, parameterValue)
        {
            this.parameterType = type;
        }

        #region Commands execution
        private void control_loadedCommandExecution(ParameterSingleViewModel context)
        {
            ParameterType = context.ParameterType;
        }
        #endregion

        /// <inheritdoc/>
        public override void InitializeDesignData()
        {
            this.ParameterName = "Param name";
            this.ParameterValue = "Param value";
            this.CustomContent = new CustomTextBox();
        }
    }
}
