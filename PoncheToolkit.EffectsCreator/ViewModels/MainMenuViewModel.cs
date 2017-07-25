using Microsoft.Win32;
using PoncheToolkit.EffectsCreator.Commands;
using PoncheToolkit.EffectsCreator.Resources;
using PoncheToolkit.EffectsCreator.Util;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PoncheToolkit.EffectsCreator.ViewModels
{
    /// <summary>
    /// ViewModel representing the menu data.
    /// </summary>
    public class MainMenuViewModel : MainWindowChildViewModelBase
    {
        private string fileText = MenuTextResources.File;
        private string newText = MenuTextResources.File_New;
        private string importAddText = MenuTextResources.Import_Add;
        private string importComponentText = MenuTextResources.Import_Add_Component;
        private string importPrimitiveText = MenuTextResources.Import_Add_Primitive;
        private string importPrimitiveTriangleText = MenuTextResources.Import_Add_Primitive_Triangle;
        private string importPrimitiveSquareText = MenuTextResources.Import_Add_Primitive_Square;
        private string importPrimitiveCubeText = MenuTextResources.Import_Add_Primitive_Cube;
        private string importModelText = MenuTextResources.Import_Add_Model;
        private string importText = MenuTextResources.Import;
        private string import_shaderText = MenuTextResources.Import_Shaders;
        private string import_shaderTemplateText = MenuTextResources.Import_ShaderTemplate;
        private string helpText = MenuTextResources.Help;

        #region Texts

        #region FileTexts
        /// <summary>
        /// Text for the file menu.
        /// </summary>
        public string FileText
        {
            get { return fileText; }
            set { SetProperty(ref fileText, value); }
        }
        /// <summary>
        /// Text for the new menu.
        /// </summary>
        public string NewText
        {
            get { return newText; }
            set { SetProperty(ref newText, value); }
        }
        #endregion

        #region ImportTexts
        /// <summary>
        /// Text for the import menu.
        /// </summary>
        public string ImportText
        {
            get { return importText; }
            set { SetProperty(ref importText, value); }
        }

        /// <summary>
        /// Text for the import shader template menu.
        /// </summary>
        public string Import_shaderText
        {
            get { return import_shaderText; }
            set { SetProperty(ref import_shaderText, value); }
        }

        /// <summary>
        /// Text for the import shader template menu.
        /// </summary>
        public string Import_shaderTemplateText
        {
            get { return import_shaderTemplateText; }
            set { SetProperty(ref import_shaderTemplateText, value); }
        }

        /// <summary>
        /// Text for the import add menu.
        /// </summary>
        public string Import_addText
        {
            get { return importAddText; }
            set { SetProperty(ref importAddText, value); }
        }

        /// <summary>
        /// Text for the import component menu.
        /// </summary>
        public string Import_addComponentText
        {
            get { return importComponentText; }
            set { SetProperty(ref importComponentText, value); }
        }

        /// <summary>
        /// Text for the import component menu.
        /// </summary>
        public string Import_addPrimitiveText
        {
            get { return importComponentText; }
            set { SetProperty(ref importComponentText, value); }
        }

        /// <summary>
        /// Text for the import triangle component menu.
        /// </summary>
        public string Import_addPrimitiveTriangleText
        {
            get { return importPrimitiveTriangleText; }
            set { SetProperty(ref importPrimitiveTriangleText, value); }
        }

        /// <summary>
        /// Text for the import square component menu.
        /// </summary>
        public string Import_addPrimitiveSquareText
        {
            get { return importPrimitiveSquareText; }
            set { SetProperty(ref importPrimitiveSquareText, value); }
        }

        /// <summary>
        /// Text for the import cube component menu.
        /// </summary>
        public string Import_addPrimitiveCubeText
        {
            get { return importPrimitiveCubeText; }
            set { SetProperty(ref importPrimitiveCubeText, value); }
        }

        /// <summary>
        /// Text for the import model menu.
        /// </summary>
        public string Import_addModelText
        {
            get { return importModelText; }
            set { SetProperty(ref importModelText, value); }
        }
        #endregion

        #region HelpTexts
        /// <summary>
        /// Text for the help menu.
        /// </summary>
        public string HelpText
        {
            get { return helpText; }
            set { SetProperty(ref helpText, value); }
        }
        #endregion
        #endregion

        #region Commands
        /// <summary>
        /// Command when new menu is clicked.
        /// </summary>
        public RelayCommand<MainWindowViewModel> File_NewCommand { get; set; }

        /// <summary>
        /// Command when importing a shader from a file.
        /// </summary>
        public RelayCommand<MainWindowViewModel> Import_ShaderCommand { get; set; }

        /// <summary>
        /// Command when importing a shader from a template.
        /// </summary>
        public RelayCommand<MainWindowViewModel> Import_ShaderTemplateCommand { get; set; }

        /// <summary>
        /// Command when importing a primitive component.
        /// </summary>
        public RelayCommand<string> Import_ComponentPrimitive { get; set; }

        /// <summary>
        /// Command when importing a model component.
        /// </summary>
        public RelayCommand<MainWindowViewModel> Import_ComponentModel { get; set; }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainMenuViewModel()
        {
            File_NewCommand = new RelayCommand<MainWindowViewModel>(file_newCommandExecution);
            Import_ShaderCommand = new RelayCommand<MainWindowViewModel>(import_ShaderCommandExecution);
            Import_ShaderTemplateCommand = new RelayCommand<MainWindowViewModel>(import_ShaderTemplateCommandExecution);
            Import_ComponentPrimitive = new RelayCommand<string>(import_ComponentPrimitiveCommandExecution);
            Import_ComponentModel = new RelayCommand<MainWindowViewModel>(import_ComponentModelCommandExecution);
        }

        #region File Commands
        private void file_newCommandExecution(MainWindowViewModel model)
        {
            System.Windows.MessageBox.Show("Entrando al file_new command - valor: " + model);
        }
        #endregion

        #region Import Commands
        private void import_ShaderTemplateCommandExecution(MainWindowViewModel model)
        {
            System.Windows.MessageBox.Show("Entrando al import_ShaderTemplateCommandExecution command - valor: " + model);
        }

        private void import_ShaderCommandExecution(MainWindowViewModel context)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.InitialDirectory = context.RenderViewModel.GameWpf.Instance.ContentDirectoryFullPath;
            dialog.Filter = "HLSL file|*.fx";
            dialog.Title = DialogTextResources.OpenShaderTitle;
            if (dialog.ShowDialog() == true)
            {
                string text = File.ReadAllText(dialog.FileName);
                context.ShaderViewModel.ShaderText = text;
                context.DocumentName = dialog.SafeFileName;
            }
        }

        private void import_ComponentPrimitiveCommandExecution(string menu)
        {
            MainWindowViewModel context = (MainWindow.DataContext as MainWindowViewModel);
            if (menu == importPrimitiveTriangleText)
            {
                Triangle prim = new Triangle(context.RenderViewModel.GameWpf.Instance) { Name = "Triangle_" + (context.RenderViewModel.GameWpf.CurrentScreen.Components.Count + 1) };
                prim.Position = new SharpDX.Vector3(0.5f, 0, 0);
                prim.Size = new SharpDX.Vector3(0.5f, 0.5f, 0.5f);
                context.RenderViewModel.GameWpf.CurrentScreen.Components.AddComponent<PTModel>(prim);
            }else if (menu == importPrimitiveSquareText)
            {
                Square prim = new Square(context.RenderViewModel.GameWpf.Instance) { Name = "Square_" + (context.RenderViewModel.GameWpf.CurrentScreen.Components.Count + 1) };
                prim.Position = new SharpDX.Vector3(0.5f, 0, 0);
                prim.Size = new SharpDX.Vector3(0.5f, 0.5f, 0.5f);
                context.RenderViewModel.GameWpf.CurrentScreen.Components.AddComponent<PTModel>(prim);
            }
            else if (menu == importPrimitiveCubeText)
            {
                Cube prim = new Cube(context.RenderViewModel.GameWpf.Instance) { Name = "Cube_" + (context.RenderViewModel.GameWpf.CurrentScreen.Components.Count + 1) };
                prim.Position = new SharpDX.Vector3(0.5f, 0, 0);
                prim.Size = new SharpDX.Vector3(0.5f, 0.5f, 0.5f);
                context.RenderViewModel.GameWpf.CurrentScreen.Components.AddComponent<PTModel>(prim);
            }
        }

        private void import_ComponentModelCommandExecution(MainWindowViewModel context)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.InitialDirectory = context.RenderViewModel.GameWpf.Instance.ContentDirectoryFullPath;
            dialog.Filter = "3D Models|*";
            dialog.Title = DialogTextResources.ImportModelTitle;
            if (dialog.ShowDialog() == true)
            {
                PTModel model = context.RenderViewModel.GameWpf.Instance.ContentManager.LoadModel(dialog.FileName);
                model.Size = new SharpDX.Vector3(0.03f, 0.03f, 0.03f);
                context.RenderViewModel.GameWpf.CurrentScreen.Components.AddComponent(model, "model_duck" + context.RenderViewModel.GameWpf.CurrentScreen.Components.Count);
            }
        }
        #endregion
    }
}
