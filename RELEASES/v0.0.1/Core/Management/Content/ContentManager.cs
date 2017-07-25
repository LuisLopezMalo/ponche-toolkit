using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Util;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Util.Exceptions;
using PoncheToolkit.Graphics3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace PoncheToolkit.Core.Management.Content
{
    public enum ContentType
    {
        Effect,
        Texture2D,
        Model3D
    }

    /// <summary>
    /// Class that help to manage the creation and disposal of content.
    /// </summary>
    public class ContentManager : GameService
    {
        #region Public Static Fields
        /// <summary>
        /// Array of valid extensions that can be loaded into memory.
        /// </summary>
        public static readonly string[] VALID_EXTENSIONS =
        {
            ".fx",
            ".jpg",
            ".dds",
            ".jpeg",
            ".png",
            ".bmp",
            ".tiff",
            ".fbx"
        };

        /// <summary>
        /// String to pass when compiling .fx files that contain pixel and vertex shaders.
        /// </summary>
        public static readonly string SHADER_EFFECT_VERSION_5 = "fx_5_0";
        /// <summary>
        /// String to pass when compiling .fx files that contain only vertex shader.
        /// </summary>
        public static readonly string SHADER_VERTEX_SHADER_VERSION_5 = "vs_5_0";
        /// <summary>
        /// String to pass when compiling .fx files that contain only pixel shader.
        /// </summary>
        public static readonly string SHADER_PIXEL_SHADER_VERSION_5 = "ps_5_0";
        #endregion

        #region Fields
        private List<ComObject> contents;

        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public new Game11 Game { get; set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public ContentManager(Game11 game)
            : base(game)
        {
            this.Game = game;
            contents = new List<ComObject>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Load into memory an object of any supported type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">When the resource is not found.</exception>
        /// <exception cref="ResourceNotSupportedException">When the resource is not found.</exception>
        public T Load<T>(string assetName) where T : GameContent
        {
            string contentPath = Path.Combine(Game.ContentDirectory, assetName);
            if (!File.Exists(contentPath))
                throw new FileNotFoundException(string.Format("The resource -{0}- was not found.", contentPath));

            string extension = Path.GetExtension(contentPath);
            if (!isValidExtension(extension))
                throw new ResourceNotSupportedException(string.Format("The resource with extension -{0}- is not supported.", extension));

            // Load the asset.
            if (typeof(T) == typeof(Shader))
            {
                Shader shader = new Shader();
                shader.PixelShader = compilePixelShader(contentPath, "PixelShaderEntry");
                shader.VertexShader = compileVertexShader(contentPath, "VertexShaderEntry", out shader.VertexShaderSignature);

                return (T)System.Convert.ChangeType(shader, Type.GetTypeCode(typeof(T)));
            }else if (typeof(T) == typeof(Graphics3D.Texture2D))
            {
                Graphics3D.Texture2D texture = WICHelper.LoadTextureFromFile(Game.Renderer.Device, contentPath);
                return (T)System.Convert.ChangeType(texture, Type.GetTypeCode(typeof(T)));
            }

            return default(T);
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void Update()
        {

        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get if the extension of the resource is valid.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private bool isValidExtension(string extension)
        {
            return VALID_EXTENSIONS.Contains(extension.ToLower());
        }

        /// <summary>
        /// Compile a Vertex Shader.
        /// </summary>
        /// <param name="assetName">The path of the effect.</param>
        /// <param name="entryPoint">String of the name of the shader method to work as the vertex shader entry point.</param>
        /// <param name="signature">Set the ShaderSignature.</param>
        /// <returns>Return the VertexShader object of the compiled shader.</returns>
        /// <exception cref="PoncheToolkit.Util.Exceptions.ResourceCompilationException"/>
        private VertexShader compileVertexShader(string assetName, string entryPoint, out ShaderSignature signature)
        {
            // Compile Vertex Shader
            using (var compiledVertexShader = ShaderBytecode.CompileFromFile(assetName, entryPoint, SHADER_VERTEX_SHADER_VERSION_5))
            {
                if (compiledVertexShader.Bytecode == null || compiledVertexShader.HasErrors)
                    throw new ResourceCompilationException("Error compiling vertex shader. - " + compiledVertexShader.Message);
                VertexShader vertexShader = new VertexShader(Game.Renderer.Device, compiledVertexShader);
                signature = ShaderSignature.GetInputSignature(compiledVertexShader);

                return vertexShader;
            }
        }

        /// <summary>
        /// Compile a pixel shader.
        /// </summary>
        /// <param name="assetName">The path of the effect.</param>
        /// <param name="entryPoint">String of the name of the shader method to work as the pixel shader entry point.</param>
        /// <returns>Return the PixelShader object of the compiled shader.</returns>
        /// <exception cref="PoncheToolkit.Util.Exceptions.ResourceCompilationException"/>
        private PixelShader compilePixelShader(string assetName, string entryPoint)
        {
            // Compile Pixel shader
            using (var compiledPixelShader = ShaderBytecode.CompileFromFile(assetName, entryPoint, SHADER_PIXEL_SHADER_VERSION_5))
            {
                if (compiledPixelShader.Bytecode == null || compiledPixelShader.HasErrors)
                    throw new ResourceCompilationException("Error compiling pixel shader. - " + compiledPixelShader.Message);
                PixelShader pixelShader = new PixelShader(Game.Renderer.Device, compiledPixelShader);

                return pixelShader;
            }
        }
        #endregion
    }
}
