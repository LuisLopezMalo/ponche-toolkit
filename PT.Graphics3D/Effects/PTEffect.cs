using PoncheToolkit.Core;
using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace PT.Graphics3D.Effects
{
    using Core.Management.Content;
    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;

    /// <summary>
    /// The basic class that represent a shader effect.
    /// </summary>
    public class PTEffect : UpdatableStateObject, IInitializable, IContentLoadable, IContentItem
    {
        /// <summary>
        /// The default path for the forward rendering effect. Does not include the 'Content' directory.
        /// </summary>
        public static readonly string FORWARD_RENDER_EFFECT_PATH = "Effects/PTForwardRenderEffect.fx";

        /// <summary>
        /// The default path for the clustered forward rendering effect. Does not include the 'Content' directory.
        /// </summary>
        public static readonly string CLUSTERED_FORWARD_RENDER_EFFECT_PATH = "Effects/PTClusteredForwardRenderEffect.fx";

        /// <summary>
        /// The path for the effect to render 2D content.
        /// </summary>
        public static readonly string SPRITE_EFFECT_PATH = "Effects/PTSpriteEffect.fx";

        #region Fields
        private Game11 game;
        private bool isInitialized;
        private bool isContentLoaded;
        private string shaderPath;
        private string name;
        private string vertexShaderEntry;
        private string pixelShaderEntry;

        private PTShader shader;
        private Dictionary<string, PTMaterial> materials;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public bool IsInitialized
        {
            get { return isInitialized; }
            set { SetPropertyAsDirty(ref isInitialized, value); }
        }

        /// <inheritdoc/>
        public bool IsContentLoaded
        {
            get { return isContentLoaded; }
            set { SetPropertyAsDirty(ref isContentLoaded, value); }
        }

        /// <summary>
        /// A simple name to identify the Effect.
        /// If it is not assigned the name will be the type of the effect + the physical name.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyAsDirty(ref name, value); }
        }

        /// <summary>
        /// The main entry point for the vertex shader.
        /// </summary>
        internal string VertexShaderEntry
        {
            get { return vertexShaderEntry; }
            set { SetPropertyAsDirty(ref vertexShaderEntry, value); }
        }

        /// <summary>
        /// The main entry point for the pixel shader.
        /// </summary>
        internal string PixelShaderEntry
        {
            get { return pixelShaderEntry; }
            set { SetPropertyAsDirty(ref pixelShaderEntry, value); }
        }

        /// <summary>
        /// The Shader object.
        /// </summary>
        public PTShader Shader
        {
            get { return shader; }
            set { SetProperty(ref shader, value); }
        }

        /// <summary>
        /// The materials that this effect has.
        /// </summary>
        public IReadOnlyDictionary<string, PTMaterial> Materials
        {
            get { return materials; }
        }

        /// <summary>
        /// The game instance.
        /// </summary>
        public Game11 Game
        {
            get { return game; }
        }

        /// <summary>
        /// The physical path of the loaded shader.
        /// </summary>
        public string ShaderPath
        {
            get { return shaderPath; }
        }

        ///// <summary>
        ///// Get or Set the buffer to be sent to the shader. VertexShader
        ///// </summary>
        //public Buffer MatricesConstantBuffer;
        ////public Buffer MatricesConstantBufferPerObject;

        ///// <summary>
        ///// Get or Set the buffer to be sent to the shader. VertexShader
        ///// </summary>
        //public Buffer MatricesConstantBufferPerFrame;

        ///// <summary>
        ///// Get or Set the buffer to be sent to the shader. VertexShader
        ///// </summary>
        //public Buffer ClipConstantBuffer;

        ///// <summary>
        ///// Get or Set the buffer to be sent to the shader. VertexShader
        ///// </summary>
        //public Buffer ReflectionConstantBuffer;

        ///// <summary>
        ///// Get or Set the buffer to be sent to the shader. PixelShader
        ///// </summary>
        //public Buffer MaterialConstantBuffer;

        ///// <summary>
        ///// Get or Set the buffer to be sent to the shader. PixelShader
        ///// </summary>
        //public Buffer LightConstantBuffer;

        /// <summary>
        /// The buffer used to draw instanced meshes.
        /// </summary>
        public Buffer InstanceBuffer;
        #endregion

        #region Events
        /// <inheritdoc/>
        public virtual event EventHandlers.OnInitializedHandler OnInitialized;

        /// <inheritdoc/>
        public virtual event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="shaderPath">The physical path to load the .fx file</param>
        public PTEffect(Game11 game, string shaderPath)
            : this(game, shaderPath, null, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="shaderPath">The physical path to load the .fx file</param>
        /// <param name="vertexShaderEntry">The vertex shader main entry for the .fx file</param>
        /// <param name="pixelShaderEntry">The pixel shader main entry for the .fx file</param>
        public PTEffect(Game11 game, string shaderPath, string vertexShaderEntry, string pixelShaderEntry)
        {
            this.game = game;
            this.shaderPath = shaderPath;
            this.vertexShaderEntry = vertexShaderEntry;
            this.pixelShaderEntry = pixelShaderEntry;
            this.name = GetType().Name + "_" + System.IO.Path.GetFileName(shaderPath);
            this.materials = new Dictionary<string, PTMaterial>();
        }
        #endregion

        /// <inheritdoc/>
        public virtual void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        /// <remarks>If this class is inherited it is recommended to call the base.LoadContent() method first inside the
        /// overriden LoadContent, this base class Loads the shader into memory using the <see cref="ContentManager.LoadShader(string, string, string)"/>
        /// method.</remarks>
        public virtual void LoadContent(ContentManager contentManager)
        {
            // Compile Vertex and Pixel shaders
            if (string.IsNullOrEmpty(vertexShaderEntry) && string.IsNullOrEmpty(pixelShaderEntry))
                shader = contentManager.LoadShader(shaderPath);
            else
                shader = contentManager.LoadShader(shaderPath, vertexShaderEntry, pixelShaderEntry);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                IsStateUpdated = true;
                OnStateUpdated();
            }

            return IsStateUpdated;
        }

        /// <summary>
        /// Apply its state to the GPU.
        /// Set the vertex and pixel shaders.
        /// </summary>
        public virtual void Apply(DeviceContext1 context)
        {
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            // Set the shaders
            context.VertexShader.Set(Shader.VertexShader);
            context.PixelShader.Set(Shader.PixelShader);

            // Set the input layout
            context.InputAssembler.InputLayout = Shader.InputLayout;

            //// Set the matrices and lights constant buffers.
            //context.VertexShader.SetConstantBuffer(0, MatricesConstantBuffer);
            //context.VertexShader.SetConstantBuffer(1, ClipConstantBuffer);
            //context.VertexShader.SetConstantBuffer(2, ReflectionConstantBuffer);
            //context.PixelShader.SetConstantBuffer(0, MaterialConstantBuffer);

            //// ===== Update common Effect properties
            ////context.UpdateSubresource(ref light, effect.LightningConstantBuffer);
            //var dataBoxClip = context.MapSubresource(ClipConstantBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            //Utilities.Write(dataBoxClip.DataPointer, ref clipPlain);
            //context.UnmapSubresource(ClipConstantBuffer, 0);

            //if (Game.Renderer.RenderMode == GraphicsRenderer.RenderingMode.Deferred)
            //{
            //    context.Rasterizer.SetViewport(Game.Viewports[0]);
            //    context.OutputMerger.SetTargets(Game.DepthStencilView, Game.RenderTarget.RenderTarget);
            //}
        }

        /// <summary>
        /// Add a new material to the <see cref="Materials"/> dictionary.
        /// The material must have a name assigned.
        /// </summary>
        /// <param name="material">The material.</param>
        public void AddMaterial(PTMaterial material)
        {
            if (string.IsNullOrEmpty(material.Name))
                throw new ArgumentNullException("The name property is null or empty.");

            PTMaterial mat;
            Materials.TryGetValue(material.Name.ToLower(), out mat);
            if (mat == null)
                materials.Add(material.Name.ToLower(), material);
            else
                Log.Warning("The material trying to be added with name -{0}- already exists.", material.Name);
        }

        /// <summary>
        /// Add a new material to the <see cref="Materials"/> dictionary.
        /// </summary>
        /// <param name="name">Name of the material</param>
        /// <param name="material">The material.</param>
        public void AddMaterial(string name, PTMaterial material)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("The name property is null or empty.");

            material.Name = name;
            PTMaterial mat;
            materials.TryGetValue(name.ToLower(), out mat);
            if (mat == null)
                materials.Add(name.ToLower(), material);
            else
                Log.Warning("The material trying to be added with name -{0}- already exists.", name);
        }

        /// <summary>
        /// Retrieve a material by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PTMaterial GetMaterial(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("The name of the material to retrieve is null or empty for effect -" + System.IO.Path.GetFileName(ShaderPath) + "-");

            PTMaterial mat;
            materials.TryGetValue(name.ToLower(), out mat);

            if (mat == null)
                Log.Warning("Material -{0}- does not exist in effect: -{1}-", name, System.IO.Path.GetFileName(ShaderPath));

            return mat;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            
            ContentsPool.ClearShader(Shader.Path);

            foreach (PTMaterial material in materials.Values)
                material.Dispose();
            materials.Clear();
        }
    }
}
