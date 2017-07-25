using Microsoft.Wpf.Interop.DirectX;
using PoncheToolkit.Core.Components;
using PoncheToolkit.EffectsCreator.Commands;
using PoncheToolkit.EffectsCreator.Resources;
using PoncheToolkit.EffectsCreator.Util;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Graphics3D.Primitives;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SlimShader;
using SlimShader.Chunks.Shex.Tokens;
using SlimShader.Chunks.Xsgn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using PoncheToolkit.EffectsCreator.Controls;
using PoncheToolkit.Core.Management.Content;
using SlimShader.Chunks.Rdef;
using PoncheToolkit.Graphics3D;
using SlimShader.Chunks.Common;

namespace PoncheToolkit.EffectsCreator.ViewModels
{
    /// <summary>
    /// Data Context for the RichTextBox where the shader code is.
    /// </summary>
    public class ShaderViewModel : MainWindowChildViewModelBase
    {
        private string shaderText;
        private string shaderTextSelection;
        private PTShader shader;
        private BackgroundWorker compilerWorker;
        private BackgroundWorker parserWorker;

        private const string SHADER_COMPILED_TEMP_BASE_NAME = "Toolkit_Shader";

        /// <summary>
        /// Type of process to be made in another Thread.
        /// </summary>
        protected enum ThreadProcessType
        {
            /// <summary>
            /// When a shader is compiled.
            /// </summary>
            ShaderCompilation,
            /// <summary>
            /// When a shader is parsed.
            /// </summary>
            ShaderParsing
        }

        /// <summary>
        /// Types of shader.
        /// </summary>
        protected enum ShaderType
        {
            /// <summary>
            /// Vertex shader.
            /// </summary>
            VertexShader,
            /// <summary>
            /// Pixel shader.
            /// </summary>
            PixelShader,
            /// <summary>
            /// Geometry shader.
            /// </summary>
            GeometryShader
        }

        #region Properties
        /// <summary>
        /// Dictionary of results for each process made in a different Thread. (internal, not used in UI).
        /// </summary>
        protected Dictionary<ThreadProcessType, bool> ProcessResults { get; set; }

        /// <summary>
        /// Get or set if the last try to compile the code was succesful. (internal, not used in UI).
        /// </summary>
        protected bool ProcessResult { get; set; }

        /// <summary>
        /// Message of any process made in another Thread. (internal, not used in UI).
        /// </summary>
        protected bool ProcessResultText { get; set; }

        /// <summary>
        /// Text that is inside the shader editor.
        /// This is the text that will be compiled.
        /// </summary>
        public string ShaderText
        {
            get { return shaderText; }
            set { SetProperty(ref shaderText, value); }
        }

        /// <summary>
        /// Selected Text that inside the shader editor.
        /// </summary>
        public string ShaderTextSelection
        {
            get { return shaderTextSelection; }
            set { SetProperty(ref shaderTextSelection, value); }
        }

        /// <summary>
        /// The shader that is applied to the models.
        /// </summary>
        public PTShader Shader
        {
            get { return shader; }
            set { SetProperty(ref shader, value); }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command when any text is selected.
        /// </summary>
        public RelayCommand<RichTextBox> Editor_SelectTextCommand { get; set; }

        /// <summary>
        /// Command when the text has changed so it can be recompiled.
        /// </summary>
        public RelayCommand<MainWindowViewModel> Editor_TextChangedCommand { get; set; }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public ShaderViewModel(MainWindow mainWindow)
            : base(mainWindow)
        {
            if (IsInDesignMode)
                return;

            Editor_SelectTextCommand = new RelayCommand<RichTextBox>(editor_SelectTextCommandExecution);
            Editor_TextChangedCommand = new RelayCommand<MainWindowViewModel>(editor_textChangedCommandExecution);
            ProcessResults = new Dictionary<ThreadProcessType, bool>();
            compilerWorker = new BackgroundWorker();
            compilerWorker.RunWorkerCompleted += CompilerWorker_RunWorkerCompleted;
            compilerWorker.DoWork += CompilerWorker_DoWork;

            parserWorker = new BackgroundWorker();
            parserWorker.RunWorkerCompleted += ParserWorker_RunWorkerCompleted;
            parserWorker.DoWork += ParserWorker_DoWork;
        }

        #region Editor Commands
        private void editor_SelectTextCommandExecution(RichTextBox param)
        {
            if (MainWindow == null)
                MainWindow = FrameworkHelper.FindParent<MainWindow>(param) as MainWindow;
            MainWindowViewModel context = MainWindow.DataContext as MainWindowViewModel;
            TextRange tempRange = new TextRange(param.Document.ContentStart, param.Selection.Start);
            //context.BottomInformation = "Selection starts at character #" + tempRange.Text.Length;
            //context.BottomInformation += " -- Selection is " + param.Selection.Text.Length + " character(s) long";
            //context.BottomInformation += " -- Selected text: '" + param.Selection.Text + "'";

            ShaderTextSelection = param.Selection.Text;
        }

        private void editor_textChangedCommandExecution(MainWindowViewModel context)
        {
            context.BottomInformation = "Compiling...";

            try
            {
                compilerWorker.RunWorkerAsync(context);
            }
            catch (Exception ex)
            {
                context.BottomInformation = "Not compiled...";
                Console.WriteLine("Error - " + ex.Message);
            }
        }
        #endregion

        #region Thread Workers

        #region Compiler Worker
        /// <summary>
        /// Event that make the compilation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompilerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MainWindowViewModel context = (e.Argument as MainWindowViewModel);
            if (!context.ShaderViewModel.ProcessResults.ContainsKey(ThreadProcessType.ShaderCompilation))
                context.ShaderViewModel.ProcessResults.Add(ThreadProcessType.ShaderCompilation, true);

            e.Result = context;
            try
            {
                // Do the compilation using the Content Manager.
                context.ShaderViewModel.Shader = context.RenderViewModel.GameWpf.Instance.ContentManager.LoadShaderFromSource(context.ShaderViewModel.ShaderText, null, SHADER_COMPILED_TEMP_BASE_NAME);
                context.ShaderViewModel.ProcessResults[ThreadProcessType.ShaderCompilation] = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error - " + ex.Message);
                context.ShaderViewModel.ProcessResults[ThreadProcessType.ShaderCompilation] = false;
            }
        }

        /// <summary>
        /// Event raised when finished compiling successfuly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompilerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MainWindowViewModel context = (e.Result as MainWindowViewModel);

            if (!context.ShaderViewModel.ProcessResults[ThreadProcessType.ShaderCompilation])
            {
                context.BottomInformation = InformationTextResources.Compilation_NotCompiled;
                return;
            }

            // === Compiled success.
            context.BottomInformation = InformationTextResources.Compilation_Success;
            try
            {
                parserWorker.RunWorkerAsync(context);
            }
            catch (Exception ex)
            {
                context.BottomInformation = "Parse error...";
                Console.WriteLine("Error - " + ex.Message);
            }
        }
        #endregion

        #region Parser Worker
        /// <summary>
        /// Parse the shader to get the parameters and color the text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParserWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MainWindowViewModel context = (e.Argument as MainWindowViewModel);
            if (!context.ShaderViewModel.ProcessResults.ContainsKey(ThreadProcessType.ShaderParsing))
                context.ShaderViewModel.ProcessResults.Add(ThreadProcessType.ShaderParsing, true);

            try
            {
                string vertexPath = Path.Combine(Directory.GetCurrentDirectory(), SHADER_COMPILED_TEMP_BASE_NAME + ContentManager11.VERTEX_SHADER_COMPILED_NAME_EXTENSION);
                string pixelPath = Path.Combine(Directory.GetCurrentDirectory(), SHADER_COMPILED_TEMP_BASE_NAME + ContentManager11.PIXEL_SHADER_COMPILED_NAME_EXTENSION);

                ObservableCollection<ParameterGroupViewModel> groups = new ObservableCollection<ParameterGroupViewModel>();
                fillSingleShader(context, vertexPath, ShaderType.VertexShader, ref groups);
                fillSingleShader(context, pixelPath, ShaderType.PixelShader, ref groups);

                context.ShaderViewModel.ProcessResults[ThreadProcessType.ShaderCompilation] = true;
                e.Result = new object[] { context, groups };
            }
            catch (Exception ex)
            {
                Log.Error("Error parsing file", ex);
                Console.WriteLine("Error parsing file", ex.Message);
                context.ShaderViewModel.ProcessResults[ThreadProcessType.ShaderCompilation] = false;
            }
        }

        private void fillSingleShader(MainWindowViewModel context, string compiledPath, ShaderType type, ref ObservableCollection<ParameterGroupViewModel> groups)
        {
            // Parse the compiled shader.
            BytecodeContainer container = BytecodeContainer.Parse(File.ReadAllBytes(compiledPath));

            // === Constant Buffers
            ParameterGroupViewModel group = new ParameterGroupViewModel(MainWindow);
            foreach (ConstantBuffer buffer in container.ResourceDefinition.ConstantBuffers)
            {
                group.Parameters.Add(new ParameterSingleViewModel(MainWindow, buffer.Name, buffer.BufferType.ToString(), ParameterSingle.ParameterTypes.Uneditable));
                foreach (ShaderVariable variable in buffer.Variables)
                {
                    if (variable.Flags == ShaderVariableFlags.None)
                        continue;

                    switch (variable.ShaderType.BaseTypeName)
                    {
                        case "float2":
                            group.Parameters.Add(new ParameterSingleViewModel(MainWindow, variable.Name, variable.DefaultValue == null ? "" : variable.DefaultValue.ToString(), ParameterSingle.ParameterTypes.Vector2));
                            break;
                        case "float3":
                            group.Parameters.Add(new ParameterSingleViewModel(MainWindow, variable.Name, variable.DefaultValue == null ? "" : variable.DefaultValue.ToString(), ParameterSingle.ParameterTypes.Vector3));
                            break;
                        case "float4":
                            group.Parameters.Add(new ParameterSingleViewModel(MainWindow, variable.Name, variable.DefaultValue == null ? "" : variable.DefaultValue.ToString(), ParameterSingle.ParameterTypes.Vector4));
                            break;
                    }
                }
            }
            group.GroupTitle = "Constant Buffers";
            groups.Add(group);
            group.IsExpanded = context.ParametersViewModel.ParameterGroups.Count > 0 ? context.ParametersViewModel.ParameterGroups[0].IsExpanded : false;

            // === Resource Bindings (Textures)
            group = new ParameterGroupViewModel(MainWindow);
            if (container.ResourceDefinition.ResourceBindings != null && container.ResourceDefinition.ResourceBindings.Count > 0)
            {
                foreach (ResourceBinding bind in container.ResourceDefinition.ResourceBindings)
                {
                    if (bind.Type == ShaderInputType.Texture)
                        group.Parameters.Add(new ParameterSingleViewModel(MainWindow, bind.Name, "", ParameterSingle.ParameterTypes.File));
                }
                group.GroupTitle = "Resource  Bindings";
                if (group.Parameters.Count > 0)
                {
                    groups.Add(group);
                    group.IsExpanded = context.ParametersViewModel.ParameterGroups.Count > 0 ? context.ParametersViewModel.ParameterGroups[1].IsExpanded : false;
                }
            }

            // ====== Input Signature. Create the InputLayout from this property.
            // Create the input elements for the Vertex Shader.
            List<InputElement> inputElements = new List<InputElement>();
            group = new ParameterGroupViewModel(MainWindow);
            foreach (SignatureParameterDescription param in container.InputSignature.Parameters)
            {
                group.Parameters.Add(new ParameterSingleViewModel(MainWindow, param.SemanticName, "Index: " + param.SemanticIndex.ToString(), ParameterSingle.ParameterTypes.Uneditable));
                Format format;
                switch (param.Mask)
                {
                    case ComponentMask.Xy:
                        format = Format.R32G32_Float;
                        break;
                    case ComponentMask.Xyz:
                        format = Format.R32G32B32_Float;
                        break;
                    case ComponentMask.All:
                        format = Format.R32G32B32A32_Float;
                        break;
                    default:
                        format = Format.R32G32B32_Float;
                        break;
                }
                // Specific case for Position
                format = param.SemanticName.ToLower() == "position" ? Format.R32G32B32_Float : format;
                inputElements.Add(new InputElement(param.SemanticName, (int)param.SemanticIndex, format, InputElement.AppendAligned, 0));
            }
            group.GroupTitle = "Input Signature";
            groups.Add(group);
            group.IsExpanded = context.ParametersViewModel.ParameterGroups.Count > 0 ? context.ParametersViewModel.ParameterGroups[2].IsExpanded : false;

            // Set the input layout of the vertex shader.
            if (type == ShaderType.VertexShader)
            {
                context.ShaderViewModel.Shader.InputLayout = new InputLayout(context.RenderViewModel.GameWpf.Device, context.ShaderViewModel.Shader.VertexShaderSignature, inputElements.ToArray());

                // Set the shader for each component of the current screen.
                foreach (IGameComponent comp in context.RenderViewModel.GameWpf.CurrentScreen.Components.Values)
                {
                    if (comp is PTModel)
                    {
                        PTModel model = (comp as PTModel);
                        foreach (PTMesh mesh in model.Meshes)
                            mesh.Effects[0].Shader = context.ShaderViewModel.Shader;
                    }
                }
            }


            //// === Output Signature
            //group = new ParameterGroupViewModel();
            //foreach (SignatureParameterDescription param in container.OutputSignature.Parameters)
            //{
            //    group.Parameters.Add(new ParameterSingleViewModel(param.SemanticName, param.SemanticIndex.ToString(), ParameterSingle.ParameterTypes.Text));
            //}
            //group.IsExpanded = context.ParametersViewModel.ParameterGroups.Count > 0 ? context.ParametersViewModel.ParameterGroups[2].IsExpanded : false;
            //group.GroupTitle = "Output Signature";
            //groups.Add(group);

            //// === Declaration Tokens
            //group = new ParameterGroupViewModel();
            //foreach (DeclarationToken token in container.Shader.DeclarationTokens)
            //{
            //    group.Parameters.Add(new ParameterSingleViewModel(token.Header.OpcodeType.ToString(), token.Header.IsExtended.ToString(), ParameterSingle.ParameterTypes.Uneditable));
            //}
            //group.IsExpanded = context.ParametersViewModel.ParameterGroups.Count > 0 ? context.ParametersViewModel.ParameterGroups[3].IsExpanded : false;
            //group.GroupTitle = "Declaration Tokens";
            //groups.Add(group);
        }

        /// <summary>
        /// Parsing finished.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParserWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                return;
            }
            
            object[] result = e.Result as object[];
            if (result == null || result.Length <= 0)
                return;
            MainWindowViewModel context = result[0] as MainWindowViewModel;
            if (context.ShaderViewModel.ProcessResults[ThreadProcessType.ShaderCompilation])
            {
                context.ParametersViewModel.ParameterGroups.Clear();
                context.ParametersViewModel.ParameterGroups = (result[1] as ObservableCollection<ParameterGroupViewModel>);
            }else
            {
                Log.Error("Error parsing file");
                MessageBox.Show("Error parsing file.");
            }
        }
        #endregion

        #endregion

        /// <inheritdoc/>
        public override void InitializeDesignData()
        {
            ShaderText = @"cbuffer MatrixBuffer
{
    float4x4 world;
    float4x4 view;
    float4x4 projection;
};

//cbuffer MatrixBuffer
//{
//    float4x4 wvp;
//};

struct VertexShader_IN
{
    float4 pos : POSITION0;
	float4 col : COLOR0;
};

struct PixelShader_IN
{
	float4 pos : SV_POSITION0;
	float4 col : COLOR0;
};

PixelShader_IN VertexShaderEntry(VertexShader_IN input)
{
    PixelShader_IN output = (PixelShader_IN) 0;

    input.pos.w = 1.0f;
	// Calculate the wvp multiplied matrices.
    // The world-view-projection matrix is calculated in code. (using the CPU)
    //output.pos = mul(input.pos, wvp);

    // Calculate te wvp matrices here. (using the GPU).
    output.pos = mul(input.pos, world);
    output.pos = mul(output.pos, view);
    output.pos = mul(output.pos, projection);

	output.col = input.col;

	return output;
}

float4 PixelShaderEntry(PixelShader_IN input) : SV_TARGET
{
    return input.col;
}

technique11 Render
{
	pass P0
	{
		SetGeometryShader(0);
        SetVertexShader(CompileShader(vs_5_0, VertexShaderEntry()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderEntry()));
    }
}";
        }
    }
}
