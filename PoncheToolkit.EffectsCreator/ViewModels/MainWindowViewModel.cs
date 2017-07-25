using PoncheToolkit.EffectsCreator.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PoncheToolkit.EffectsCreator.ViewModels
{
    /// <summary>
    /// ViewModel for the MainWindow.
    /// </summary>
    public class MainWindowViewModel : WindowViewModelBase
    {
        private MainWindow mainWindow;
        private MainMenuViewModel mainMenuViewModel;
        private ShaderViewModel shaderViewModel;
        private RenderViewModel renderViewModel;
        private ParametersViewModel parametersViewModel;

        private string bottomInformation;
        private List<string> shaderNames;
        private string documentName;

        #region Properties
        /// <summary>
        /// The instance of the MainWindow.
        /// </summary>
        public MainWindow MainWindow
        {
            get { return mainWindow; }
            set { mainWindow = value; }
        }

        /// <summary>
        /// View Model for the top menu interaction.
        /// </summary>
        public MainMenuViewModel MainMenuViewModel
        {
            get { return mainMenuViewModel; }
            set { SetProperty(ref mainMenuViewModel, value); }
        }

        /// <summary>
        /// Text that is inside the shader editor.
        /// This is the text that will be compiled.
        /// </summary>
        public ShaderViewModel ShaderViewModel
        {
            get { return shaderViewModel; }
            set { SetProperty(ref shaderViewModel, value); }
        }

        /// <summary>
        /// View Model for the render window.
        /// </summary>
        public RenderViewModel RenderViewModel
        {
            get { return renderViewModel; }
            set { SetProperty(ref renderViewModel, value); }
        }

        /// <summary>
        /// View Model for the parameters window.
        /// </summary>
        public ParametersViewModel ParametersViewModel
        {
            get { return parametersViewModel; }
            set { SetProperty(ref parametersViewModel, value); }
        }

        /// <summary>
        /// Information shown at the bottom of the application.
        /// </summary>
        public string BottomInformation
        {
            get { return bottomInformation; }
            set { SetProperty(ref bottomInformation, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> ShaderNames
        {
            get { return shaderNames; }
            set { SetProperty(ref shaderNames, value); }
        }

        /// <summary>
        /// The title of the current document
        /// </summary>
        public string DocumentName
        {
            get { return documentName; }
            set { SetProperty(ref documentName, value); }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command when the window has loaded.
        /// </summary>
        public RelayCommand<MainWindow> Window_LoadedCommand { get; set; }

        /// <summary>
        /// Command when the window is focused.
        /// </summary>
        public RelayCommand<MainWindow> Window_GotFocusCommand { get; set; }

        /// <summary>
        /// Command when the window has lost focus.
        /// </summary>
        public RelayCommand<MainWindow> Window_LostFocusCommand { get; set; }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindowViewModel()
        {
            if (IsInDesignMode)
                return;

            mainMenuViewModel = new MainMenuViewModel();
            shaderViewModel = new ShaderViewModel(mainWindow);
            renderViewModel = new RenderViewModel();
            parametersViewModel = new ParametersViewModel(mainWindow);
            shaderNames = new List<string>();
            Window_LoadedCommand = new RelayCommand<MainWindow>(window_loadedCommandExecution);
            Window_GotFocusCommand = new RelayCommand<MainWindow>(window_gotFocusCommandExecution);
            Window_LostFocusCommand = new RelayCommand<MainWindow>(window_lostFocusCommandExecution);

            DocumentName = "Document";
        }

        #region Commands Execution
        private void window_loadedCommandExecution(MainWindow window)
        {
            this.mainWindow = window;

            ShaderViewModel.MainWindow = this.MainWindow;
            ParametersViewModel.MainWindow = this.MainWindow;
            MainMenuViewModel.MainWindow = this.MainWindow;
            RenderViewModel.MainWindow = this.MainWindow;
        }

        private void window_gotFocusCommandExecution(MainWindow window)
        {
            if (this.renderViewModel.GameWpf != null)
            {
                this.renderViewModel.GameWpf.Instance.IsFocused = true;
            }
        }

        private void window_lostFocusCommandExecution(MainWindow window)
        {
            if (this.renderViewModel.GameWpf != null)
            {
                this.renderViewModel.GameWpf.Instance.IsFocused = false;
            }
        }
        #endregion

        /// <inheritdoc/>
        public override void InitializeDesignData()
        {
            mainMenuViewModel = new MainMenuViewModel();
            shaderViewModel = new ShaderViewModel(mainWindow);
            renderViewModel = new RenderViewModel();
            parametersViewModel = new ParametersViewModel(mainWindow);
            shaderNames = new List<string>();

            ParametersViewModel.InitializeDesignData();
            ShaderViewModel.InitializeDesignData();
        }
    }
}
