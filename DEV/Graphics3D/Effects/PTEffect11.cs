using PoncheToolkit.Core;
using PoncheToolkit.Util;
using System;
using System.Collections.Generic;

namespace PoncheToolkit.Graphics3D.Effects
{
#if DX11
    using Buffer = SharpDX.Direct3D11.Buffer;
    using Core.Management.Content;
    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;

    /// <summary>
    /// The basic class that represent a shader effect.
    /// </summary>
    public class PTEffect11 : UpdatableStateObject, IPTEffect11
    {
        /// <summary>
        /// The default path for the forward rendering effect. Does not include the 'Content' directory.
        /// </summary>
        public const string FORWARD_RENDER_EFFECT_PATH = "Effects/PTForwardRenderEffect.fx";

        /// <summary>
        /// The default path for the toon rendering effect. Does not include the 'Content' directory.
        /// </summary>
        public const string TOON_RENDER_EFFECT_PATH = "Effects/PTToonEffect.fx";

        /// <summary>
        /// The path for the reflection rendering effect using dual paraboloids.
        /// </summary>
        public const string DUAL_PARABOLOID_REFLECTION_EFFECT_PATH = "Effects/PTDualParaboloid.fx";

        /// <summary>
        /// The default path for the clustered forward rendering effect. Does not include the 'Content' directory.
        /// </summary>
        public const string CLUSTERED_FORWARD_RENDER_EFFECT_PATH = "Effects/PTClusteredForwardRenderEffect.fx";

        /// <summary>
        /// The path for the effect to render 2D content.
        /// </summary>
        public const string SPRITE_EFFECT_PATH = "Effects/PTSpriteEffect.fx";

        #region Fields
#if DX11
        private Game11 game;
#elif DX12
        private Game12 game;
#endif
        private bool isInitialized;
        private bool isContentLoaded;
        private bool isUsed;
        private string shaderPath;
        private string vertexShaderPath;
        private string pixelShaderPath;
        private string name;
        private string vertexShaderEntry;
        private string pixelShaderEntry;
        private int customPixelShaderSlot;

        private PTShader shader;
        //private Dictionary<string, PTMaterial> materials;
        private SortedList<int, KeyValuePair<string, PTMaterial>> materials;
        private List<PTLight> lights;
        private List<string> includePaths;
        #endregion

        #region Properties
        internal LightBufferStruct LightsBuffer;
        internal GlobalDataStruct GlobalDataBuffer;

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
        /// Set to true if this effects creates some post process effects and add them using the <see cref="IGraphicsRenderer.AddBackBufferPostProcessEffect(int, Graphics2D.Effects.PTCustomEffect)"/> 
        /// method.
        /// </summary>
        public bool IsUsed
        {
            get { return isUsed; }
            set { SetPropertyAsDirty(ref isUsed, value); }
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
        /// The slot if the a custom pixel shader want to be used.
        /// For more information see the <see cref="ContentManager11.LoadPixelShaderInto(string, ref PTShader, int, List{string}, string)"/> description.
        /// Default: -1 (use the default implementation)
        /// </summary>
        public int CustomPixelShaderSlot
        {
            get { return customPixelShaderSlot; }
            set { SetProperty(ref customPixelShaderSlot, value); }
        }

        /// <summary>
        /// The global ambient color of the scene.
        /// <para>
        /// Default: Vector4(0.5f, 0.5f, 0.5f, 1)
        /// </para>
        /// </summary>
        public Vector4 GlobalAmbientColor
        {
            get { return GlobalDataBuffer.GlobalAmbient; }
            set { SetProperty(ref GlobalDataBuffer.GlobalAmbient, value); }
        }

        /// <summary>
        /// The number of lights that will be sent to the GPU in the current frame.
        /// <para>
        /// Default: 0
        /// </para>
        /// </summary>
        internal int CurrentLights
        {
            get { return GlobalDataBuffer.CurrentLights; }
            set { SetProperty(ref GlobalDataBuffer.CurrentLights, value); }
        }

        /// <summary>
        /// The materials dictionary that this effect have.
        /// The key is the name of the material, there cannot be two materials with the same name.
        /// </summary>
        //public IReadOnlyDictionary<string, PTMaterial> Materials
        public SortedList<int, KeyValuePair<string, PTMaterial>> Materials
        {
            get { return materials; }
        }

        /// <summary>
        /// The physical path of the loaded shader.
        /// </summary>
        public string ShaderPath
        {
            get { return shaderPath; }
        }

        /// <summary>
        /// The physical path of the loaded shader.
        /// </summary>
        public string VertexShaderPath
        {
            get { return shaderPath; }
        }

        /// <summary>
        /// The physical path of the loaded shader.
        /// </summary>
        public string PixelShaderPath
        {
            get { return shaderPath; }
        }

        /// <summary>
        /// All the lights used by this effect.
        /// The lights can be added or removed using the <see cref="AddLight(PTLight)"/> or <see cref="RemoveLight(PTLight)"/> methods.
        /// <para>
        /// </para>
        /// </summary>
        public IReadOnlyList<PTLight> Lights
        {
            get { return lights; }
        }

        /// <summary>
        /// The maximun number of lights this effect can render.
        /// </summary>
        public virtual int MaxLightsCount { get; }

        //#if DX11
        /// <summary>
        /// The game instance.
        /// </summary>
        public Game11 Game
        {
            get { return game; }
        }

        /// <summary>
        /// The buffer used to draw instanced meshes.
        /// </summary>
        internal Buffer instanceBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        internal Buffer matricesConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        internal Buffer clipConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        internal Buffer reflectionConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        internal Buffer materialConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        internal Buffer globalDataConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        internal Buffer lightsConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        public Buffer MatricesConstantBuffer
        {
            get { return matricesConstantBuffer; }
            set { SetProperty(ref matricesConstantBuffer, value); }
        }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        public Buffer ClipConstantBuffer
        {
            get { return clipConstantBuffer; }
            set { SetProperty(ref clipConstantBuffer, value); }
        }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        public Buffer ReflectionConstantBuffer
        {
            get { return reflectionConstantBuffer; }
            set { SetProperty(ref reflectionConstantBuffer, value); }
        }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        public Buffer MaterialConstantBuffer
        {
            get { return materialConstantBuffer; }
            set { SetProperty(ref materialConstantBuffer, value); }
        }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        public Buffer GlobalDataConstantBuffer
        {
            get { return globalDataConstantBuffer; }
            set { SetProperty(ref globalDataConstantBuffer, value); }
        }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        public Buffer LightsConstantBuffer
        {
            get { return lightsConstantBuffer; }
            set { SetProperty(ref lightsConstantBuffer, value); }
        }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        public Buffer InstanceBuffer
        {
            get { return instanceBuffer; }
            set { SetProperty(ref instanceBuffer, value); }
        }
        //#endif
        #endregion

        #region Events
        /// <inheritdoc/>
        public virtual event EventHandlers.OnInitializedHandler OnInitialized;

        /// <inheritdoc/>
        public virtual event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Initialization
        //#if DX11
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="shaderPath">The physical path to load the .fx file</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        public PTEffect11(Game11 game, string shaderPath, List<string> includePaths)
            : this(game, shaderPath, null, null, includePaths)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="vertexShaderPath">The physical path to load the vertex shader file</param>
        /// <param name="pixelShaderPath">The physical path to load the pixel shader file</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        public PTEffect11(Game11 game, string vertexShaderPath, string pixelShaderPath, List<string> includePaths)
            : this(game, null, null, null, includePaths)
        {
            this.vertexShaderPath = vertexShaderPath;
            this.pixelShaderPath = pixelShaderPath;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="vertexShaderPath">The physical path to load the vertex shader file</param>
        /// <param name="pixelShaderPath">The physical path to load the pixel shader file</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="customPixelShaderSlot">The slot to be used if a custom shader want to be used. If no custom shader is going to be used, set it o -1.
        /// For more information see <see cref="ContentManager11.LoadPixelShaderInto(string, ref PTShader, int, List{string}, string)"/> </param>
        public PTEffect11(Game11 game, string vertexShaderPath, string pixelShaderPath, List<string> includePaths, int customPixelShaderSlot = -1)
            : this(game, null, null, null, includePaths)
        {
            this.vertexShaderPath = vertexShaderPath;
            this.pixelShaderPath = pixelShaderPath;
            this.customPixelShaderSlot = customPixelShaderSlot;
        }

        /// <summary>
        /// Constructor.
        /// Initialize Effect from an already compiled <see cref="PTShader"/>.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="shader">The compiled <see cref="PTShader"/> shader.</param>
        public PTEffect11(Game11 game, PTShader shader)
            : this(game, null, null, null, null)
        {
            this.shader = shader;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="vertexShaderPath">The physical path to load the vertex shader file</param>
        /// <param name="pixelShaderPath">The physical path to load the pixel shader file</param>
        /// <param name="vertexShaderEntry">The vertex shader main entry for the .fx file</param>
        /// <param name="pixelShaderEntry">The pixel shader main entry for the .fx file</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        public PTEffect11(Game11 game, string vertexShaderPath, string pixelShaderPath, string vertexShaderEntry, string pixelShaderEntry, List<string> includePaths)
            : this(game, null, vertexShaderEntry, pixelShaderEntry, includePaths)
        {
            this.vertexShaderPath = vertexShaderPath;
            this.pixelShaderPath = pixelShaderPath;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="shaderPath">The physical path to load the .fx file</param>
        /// <param name="vertexShaderEntry">The vertex shader main entry for the .fx file</param>
        /// <param name="pixelShaderEntry">The pixel shader main entry for the .fx file</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        public PTEffect11(Game11 game, string shaderPath, string vertexShaderEntry, string pixelShaderEntry, List<string> includePaths)
        {
            this.game = game;
            this.shaderPath = shaderPath;
            this.vertexShaderEntry = vertexShaderEntry;
            this.pixelShaderEntry = pixelShaderEntry;
            this.name = GetType().Name + "_" + System.IO.Path.GetFileName(shaderPath);
            //this.materials = new Dictionary<string, PTMaterial>();
            this.materials = new SortedList<int, KeyValuePair<string, PTMaterial>>();
            this.lights = new List<PTLight>();
            this.customPixelShaderSlot = -1;
            this.GlobalAmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            this.includePaths = includePaths;

            this.GlobalDataBuffer = new GlobalDataStruct(this.GlobalAmbientColor, 0);
            this.LightsBuffer = new LightBufferStruct(null);
        }
        //#endif
        #endregion

        /// <inheritdoc/>
        public virtual void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        /// <remarks>If this class is inherited it is recommended to call the base.LoadContent() method first inside the
        /// overriden LoadContent, this base class Loads the shader into memory using the <see cref="ContentManager11.LoadShader(string, List{string}, string, string)"/>
        /// method.</remarks>
        public virtual void LoadContent(IContentManager contentManager)
        {
            //#if DX11
            // If not shader is loaded.
            if (shader == null)
            {
                // Compile Vertex and Pixel shaders
                if (!string.IsNullOrEmpty(ShaderPath)) // If the shaders are divided in different files (pixelShader, vertexShader, etc).
                {
                    shader = contentManager.LoadShader(shaderPath, includePaths, vertexShaderEntry, pixelShaderEntry);
                }
                else
                {
                    shader = new PTShader();
                    shader.LoadContent(contentManager);
                    contentManager.LoadVertexShaderInto(vertexShaderPath, ref shader, includePaths, vertexShaderEntry);
                    contentManager.LoadPixelShaderInto(pixelShaderPath, ref shader, customPixelShaderSlot, includePaths, pixelShaderEntry: pixelShaderEntry);
                }
            }

            // Add the default materials.
            PTMaterial colorMaterial = new PTMaterial(Game);
            colorMaterial.Name = "Default color material";
            colorMaterial.SpecularPower = 25f;
            colorMaterial.IsSpecularEnabled = true;
            colorMaterial.EmissiveColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            colorMaterial.AmbientColor = new Vector4(0.31f, 0.31f, 0.31f, 1f);
            colorMaterial.DiffuseColor = new Vector4(0.51f, 0.51f, 0.51f, 1f);
            colorMaterial.SpecularColor = new Vector4(1f, 1f, 1f, 1f);
            colorMaterial.LoadContent(contentManager);
            AddMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY, colorMaterial);

            LoadBuffers();
            //#endif
            OnFinishLoadContent?.Invoke();
        }

        //#if DX11
        /// <inheritdoc/>
        public virtual void LoadBuffers()
        {
            // Create the constant buffers.
            MatricesConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<MatricesStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            ClipConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<ClipPlainStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            ReflectionConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<ReflectionStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            MaterialConstantBuffer = new Buffer(Game.Renderer.Device, MaterialStruct.SizeOf(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            LightsConstantBuffer = new Buffer(Game.Renderer.Device, LightBufferStruct.SizeOf(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            GlobalDataConstantBuffer = new Buffer(Game.Renderer.Device, GlobalDataStruct.SizeOf(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            ToDispose(MatricesConstantBuffer);
            ToDispose(ClipConstantBuffer);
            ToDispose(ReflectionConstantBuffer);
            ToDispose(MaterialConstantBuffer);
            ToDispose(LightsConstantBuffer);
            ToDispose(GlobalDataConstantBuffer);

        }
        //#endif

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

            // Update lights logic.
            foreach (PTLight l in lights)
                l.UpdateLogic(Game.GameTime);

            // Set the shaders
            context.VertexShader.Set(Shader.VertexShader);
            if (Shader.ClassInstances != null && Shader.ClassInstances.Length > 0)
                context.PixelShader.Set(Shader.PixelShader, Shader.ClassInstances);
            else
                context.PixelShader.Set(Shader.PixelShader);

            // Set the input layout
            context.InputAssembler.InputLayout = Shader.InputLayout;
        }

        //#region Materials
        ///// <summary>
        ///// Add a new material to the <see cref="Materials"/> dictionary.
        ///// The material must have a name assigned.
        ///// </summary>
        ///// <param name="material">The material.</param>
        //public void AddMaterial(PTMaterial material)
        //{
        //    if (string.IsNullOrEmpty(material.Name))
        //        throw new ArgumentNullException("The name property is null or empty.");

        //    PTMaterial mat;
        //    Materials.TryGetValue(material.Name.ToLower(), out mat);
        //    if (mat == null)
        //        materials.Add(material.Name.ToLower(), material);
        //    else
        //        Log.Warning("The material trying to be added with name -{0}- already exists.", material.Name);
        //}

        ///// <summary>
        ///// Add a new material to the <see cref="Materials"/> dictionary.
        ///// </summary>
        ///// <param name="name">Name of the material</param>
        ///// <param name="material">The material.</param>
        //public void AddMaterial(string name, PTMaterial material)
        //{
        //    if (string.IsNullOrEmpty(name))
        //        throw new ArgumentNullException("The name property is null or empty.");

        //    material.Name = name;
        //    PTMaterial mat;
        //    materials.TryGetValue(name.ToLower(), out mat);
        //    if (mat == null)
        //        materials.Add(name.ToLower(), material);
        //    else
        //        Log.Warning("The material trying to be added with name -{0}- already exists.", name);
        //}

        ///// <summary>
        ///// Retrieve a material by its name.
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public PTMaterial GetMaterial(string name)
        //{
        //    if (string.IsNullOrEmpty(name))
        //        throw new ArgumentNullException("The name of the material to retrieve is null or empty for effect -" + System.IO.Path.GetFileName(ShaderPath) + "-");

        //    PTMaterial mat;
        //    materials.TryGetValue(name.ToLower(), out mat);

        //    if (mat == null)
        //        Log.Warning("Material -{0}- does not exist in effect: -{1}-", name, System.IO.Path.GetFileName(ShaderPath));

        //    return mat;
        //}
        //#endregion

        #region Materials
        /// <summary>
        /// Add a new material to the <see cref="Materials"/> dictionary.
        /// The material must have a name assigned.
        /// </summary>
        /// <param name="material">The material.</param>
        public void AddMaterial(PTMaterial material)
        {
            this.AddMaterial(materials.Count, material);
        }

        /// <summary>
        /// Add a new material to the <see cref="Materials"/> dictionary.
        /// The material must have a name assigned.
        /// </summary>
        /// <param name="material">The material.</param>
        public void AddMaterial(int index, PTMaterial material)
        {
            if (string.IsNullOrEmpty(material.Name))
                throw new ArgumentNullException("The name property is null or empty.");

            //PTMaterial mat;
            //Materials.TryGetValue(material.Name.ToLower(), out mat);
            //if (mat == null)
            //    materials.Add(material.Name.ToLower(), material);
            //else
            //    Log.Warning("The material trying to be added with name -{0}- already exists.", material.Name);

            bool exists = false;
            for (int i = 0; i < materials.Count; i++)
            {
                KeyValuePair<string, PTMaterial> pair = materials[i];
                if (pair.Key == material.Name.ToLower())
                {
                    exists = true;
                    Log.Warning("The material trying to be added with name -{0}- already exists.", material.Name);
                    break;
                }
            }

            if (!exists)
                materials.Add(index, new KeyValuePair<string, PTMaterial>(material.Name.ToLower(), material));
        }

        /// <summary>
        /// Add a new material to the <see cref="Materials"/> dictionary.
        /// </summary>
        /// <param name="name">Name of the material</param>
        /// <param name="material">The material.</param>
        public void AddMaterial(string name, PTMaterial material)
        {
            this.AddMaterial(materials.Count, name, material);
        }

        /// <summary>
        /// Add a new material to the <see cref="Materials"/> dictionary.
        /// </summary>
        /// <param name="name">Name of the material</param>
        /// <param name="material">The material.</param>
        public void AddMaterial(int index, string name, PTMaterial material)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("The name property is null or empty.");

            //material.Name = name;
            //PTMaterial mat;
            //materials.TryGetValue(name.ToLower(), out mat);
            //if (mat == null)
            //    materials.Add(name.ToLower(), material);
            //else
            //    Log.Warning("The material trying to be added with name -{0}- already exists.", name);

            material.Name = name.ToLower();
            bool exists = false;
            for (int i = 0; i < materials.Count; i++)
            {
                KeyValuePair<string, PTMaterial> pair = materials[i];
                if (pair.Key == material.Name)
                {
                    exists = true;
                    Log.Warning("The material trying to be added with name -{0}- already exists.", material.Name);
                    break;
                }
            }

            if (!exists)
                materials.Add(index, new KeyValuePair<string, PTMaterial>(material.Name, material));
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

            //PTMaterial mat;
            //materials.TryGetValue(name.ToLower(), out mat);

            //if (mat == null)
            //    Log.Warning("Material -{0}- does not exist in effect: -{1}-", name, System.IO.Path.GetFileName(ShaderPath));

            //return mat;

            name = name.ToLower();
            for (int i = 0; i < materials.Count; i++)
            {
                KeyValuePair<string, PTMaterial> pair = materials[i];
                if (pair.Key == name)
                    return pair.Value;
            }

            return null;
        }

        /// <summary>
        /// Retrieve a material by its index.
        /// If the index is out of range, return the default textureless material.
        /// </summary>
        /// <param name="index">The index of the material to retrieve.</param>
        /// <returns></returns>
        public PTMaterial GetMaterial(int index)
        {
            // If the index is out of range, return the default textureless material.
            if (!materials.ContainsKey(index))
                return GetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);

            return materials[index].Value;
        }
        #endregion

        #region Lights
        /// <summary>
        /// Add a new light to the <see cref="Lights"/> list.
        /// </summary>
        /// <param name="light">The Light.</param>
        public void AddLight(PTLight light)
        {
            if (lights.Count >= MaxLightsCount)
            {
                Log.Warning("The maximum number of lights reached. Light not added - Max Lights: " + MaxLightsCount);
                return;
            }

            light.Index = lights.Count;
            //LightsBuffer.AddLight(light);
            light.OnStateUpdatedEvent += Light_OnStateUpdatedEvent;
            lights.Add(light);

            CurrentLights = Math.Min(Lights.Count, PTLight.FORWARD_SHADING_MAX_LIGHTS); // Set the number of current lights.
            LightsBuffer = new LightBufferStruct(lights);
        }

        /// <summary>
        /// Remove a light from the <see cref="Lights"/> list.
        /// </summary>
        /// <param name="light">The Light to be removed.</param>
        public void RemoveLight(PTLight light)
        {
            if (lights.Contains(light))
            {
                LightsBuffer.Lights[light.Index].IsEnabled = 0;
                light.OnStateUpdatedEvent -= Light_OnStateUpdatedEvent;
                lights.Remove(light);
            }
        }

        private void Light_OnStateUpdatedEvent(object sender, EventArgs e)
        {
            PTLight light = (sender as PTLight);
            LightsBuffer.Lights[light.Index] = light.LightBuffer;
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();

            ContentsPool.ClearShader(Shader);

            //foreach (PTMaterial material in materials.Values)
            //    material.Dispose();
            foreach (KeyValuePair<string, PTMaterial> material in materials.Values)
                material.Value.Dispose();
            materials.Clear();
        }
        #endregion

    }

#endif
}
