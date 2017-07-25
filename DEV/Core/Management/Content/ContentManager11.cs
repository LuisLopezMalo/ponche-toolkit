using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpDX.D3DCompiler;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Util;
using PoncheToolkit.Util.Exceptions;
using PoncheToolkit.Graphics3D;
using SharpDX.Direct3D11;
using Assimp;

namespace PoncheToolkit.Core.Management.Content
{
#if DX11

    using Graphics3D.Effects;
    using SharpDX.Direct3D;

    /// <summary>
    /// Class that help to manage the creation and disposal of content.
    /// </summary>
    public class ContentManager11 : GameService, IContentManager11
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
            ".gif",
            ".tiff",
            ".fbx",
            ".dae",
            ".obj",
            ".ply"
        };

        ///// <summary>
        ///// String to pass when compiling .fx files that contain pixel and vertex shaders.
        ///// </summary>
        //public static readonly string SHADER_EFFECT_VERSION_5 = "fx_5_0";
        /// <summary>
        /// String to pass when compiling .fx files that contain only vertex shader.
        /// </summary>
        public static readonly string SHADER_VERTEX_SHADER_VERSION_5 = "vs_5_0";
        /// <summary>
        /// String to pass when compiling .fx files that contain only pixel shader.
        /// </summary>
        public static readonly string SHADER_PIXEL_SHADER_VERSION_5 = "ps_5_0";
        /// <summary>
        /// The name of the compiled vertex shader file.
        /// </summary>
        public static readonly string VERTEX_SHADER_COMPILED_NAME_EXTENSION = "_Vertex.cso";
        /// <summary>
        /// The name of the compiled pixel shader file.
        /// </summary>
        public static readonly string PIXEL_SHADER_COMPILED_NAME_EXTENSION = "_Pixel.cso";
        /// <summary>
        /// The name of the compiled tesselation shader file.
        /// </summary>
        public static readonly string TESSELATION_SHADER_COMPILED_NAME_EXTENSION = "_Tesselation.cso";
        /// <summary>
        /// The name of the compiled geometry shader file.
        /// </summary>
        public static readonly string GEOMETRY_SHADER_COMPILED_NAME_EXTENSION = "_Geometry.cso";

        private static object lockRoot = new object();
        #endregion

        #region Fields
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        #endregion

        #region Properties
        ///// <inheritdoc/>
        //public Game11 Game { get; set; }

        /// <summary>
        /// A dictionary with the current loaded shaders in memory.
        /// </summary>
        public Dictionary<string, PTShader> Shaders
        {
            get { return ContentsPool.Shaders; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public ContentManager11(Game game)
            : base(game)
        {
        }
        #endregion

        #region Public Methods
        ///// <summary>
        ///// Load into memory an object of any supported type.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        ///// <exception cref="FileNotFoundException">When the resource is not found.</exception>
        ///// <exception cref="ResourceNotSupportedException">When the resource is not found.</exception>
        //public T Load<T>(string assetName) where T : IGameContent
        //{
        //    // Load the asset.
        //    if (typeof(T) == typeof(Shader))
        //    {
        //    } else if (typeof(T) == typeof(Graphics3D.Texture2D))
        //    {
        //    } else if (typeof(T) == typeof(ModelContent))
        //    {
        //    }
        //    return default(T);
        //}

        /// <summary>
        /// Load in memory a texture2D.
        /// This method is used when creating a custom Texture2D not taken from a physical file.
        /// </summary>
        /// <param name="textureKey">The string to save this texture in the <see cref="ContentsPool.Textures"/> dictionary.</param>
        /// <param name="fromTexture">The texture.</param>
        /// <returns></returns>
        public Graphics2D.PTTexture2D LoadTexture2D(string textureKey, Texture2D fromTexture)
        {
            Graphics2D.PTTexture2D tex;
            ContentsPool.Textures.TryGetValue(textureKey.ToLower(), out tex);
            if (tex != null)
            {
                Log.Debug("Texture2D from path: -{0}- already loaded in memory.", textureKey);
                return tex;
            }

            tex = new Graphics2D.PTTexture2D(Game.Renderer.Device, fromTexture);
            //tex.Path = textureKey;
            tex.SetTexturePath(new PTTexturePath(textureKey), false);
            tex.LoadContent(this);

            Log.Debug("Loaded Texture2D from textureKey: -{0}-", textureKey);
            ContentsPool.Textures.Add(textureKey.ToLower(), tex);

            return tex;
        }

        /// <summary>
        /// Load in memory a texture2D.
        /// This texture will be rendered using the back buffer. (<see cref="GraphicsRenderer.Context2D"/>)
        /// </summary>
        /// <param name="assetName">The local path and name of the texture.</param>
        /// <param name="generateMipMaps">If the texture will generate mip map chain.</param>
        /// <param name="referencePath">A path for reference to combine with the assetName.</param>
        /// <returns></returns>
        public Graphics2D.PTTexture2D LoadTexture2D(string assetName, string referencePath = null, bool generateMipMaps = false)
        {
            string contentPath = fileValidations(assetName, referencePath);

            Graphics2D.PTTexture2D texture;
            ContentsPool.Textures.TryGetValue(contentPath, out texture);
            if (texture != null)
            {
                Log.Debug("Texture2D from path: -{0}- already loaded in memory.", assetName);
                return texture;
            }

            texture = WICHelper.LoadTextureFromFile(Game.Renderer.Device, Game.Renderer.Context2D, contentPath, generateMipMaps);
            texture.LoadContent(this);

            if (generateMipMaps)
                Game.Renderer.Device.ImmediateContext.GenerateMips(texture.ShaderResourceView);

            Log.Debug("Loaded Texture2D from path: -{0}-", assetName);
            ContentsPool.Textures.Add(contentPath, texture);

            return texture;
        }

        /// <summary>
        /// Load in memory a texture2D.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="context">The render target where the texture will be drawn.</param>
        /// <param name="referencePath">A path for reference to combine with the assetName.</param>
        /// <param name="generateMipMaps">If the texture will generate mip maps.</param>
        /// <returns></returns>
        public Graphics2D.PTTexture2D LoadTexture2D(string assetName, SharpDX.Direct2D1.DeviceContext context, string referencePath = null, bool generateMipMaps = false)
        {
            if (context == null)
                return LoadTexture2D(assetName, referencePath);

            string contentPath = fileValidations(assetName, referencePath);

            Graphics2D.PTTexture2D tex;
            ContentsPool.Textures.TryGetValue(contentPath, out tex);
            if (tex != null)
            {
                Log.Debug("Texture2D from path: -{0}- already loaded in memory.", assetName);
                return tex;
            }

            Graphics2D.PTTexture2D texture = WICHelper.LoadTextureFromFile(Game.Renderer.Device, context, contentPath, generateMipMaps);
            texture.LoadContent(this);

            if (generateMipMaps)
                Game.Renderer.Device.ImmediateContext.GenerateMips(texture.ShaderResourceView);

            Log.Debug("Loaded Texture2D from path: -{0}-", assetName);
            ContentsPool.Textures.Add(contentPath, texture);

            return texture;
        }

        /// <summary>
        /// Load in memory a texture2D.
        /// </summary>
        /// <param name="texturePaths"></param>
        /// <returns></returns>
        //public Graphics2D.PTTexture2D LoadTexture2DArray(string[] assetNames)
        public Graphics2D.PTTexture2D LoadTexture2DArray(List<PTTexturePath> texturePaths)
        {
            string key = "";
            string[] contentPaths = fileValidations(texturePaths.Select(t => t.Path).ToArray(), out key);

            Graphics2D.PTTexture2D tex;
            ContentsPool.Textures.TryGetValue(key.ToLower(), out tex);
            if (tex != null)
            {
                Log.Debug("Texture2D from path: -{0}- already loaded in memory.", texturePaths);
                return tex;
            }

            tex = WICHelper.LoadTextureFromFile(Game.Renderer.Device, Game.Renderer.Context2D, false, contentPaths);
            //texture.Type = Graphics2D.PTTexture2D.TextureType.Render | Graphics2D.PTTexture2D.TextureType.Array;
            tex.LoadContent(this);

            Log.Debug("Loaded Texture2D from path: -{0}-", texturePaths);
            ContentsPool.Textures.Add(key.ToLower(), tex);

            return tex;
        }

        #region Async tests
        ///// <summary>
        ///// Load in memory a texture2D.
        ///// </summary>
        ///// <param name="assetName"></param>
        ///// <param name="referencePath">A path for reference to combine with the assetName.</param>
        ///// <returns></returns>
        //public async T LoadTexture2DAsync<T>(string assetName, string referencePath = null) where T : Graphics2D.Texture2D
        //{
        //    await Task.Run(() =>
        //    {
        //        string contentPath = fileValidations(assetName, referencePath);
        //        Graphics2D.Texture2D texture = WICHelper.LoadTextureFromFile(Game.Renderer.Device, contentPath);
        //        texture.Path = contentPath;
        //        return texture;
        //    });
        //}

        //private static TaskFactory<IContentItem> factory;
        //private static object SYNC_ROOT = new object();

        ///// <summary>
        ///// Process a specific task, setting a lock so if any other tasks come, they will wait.
        ///// </summary>
        ///// <param name="action"></param>
        //public static async Task<T> ProcessTask<T>(Func<T> action) where T : IContentItem
        //{
        //    //lock (SYNC_ROOT)
        //    //{
        //    //if (factory == null)
        //    //    factory = new TaskFactory<T>(TaskCreationOptions.AttachedToParent, TaskContinuationOptions.ExecuteSynchronously);
        //    //Task<T> t = factory.StartNew(action, TaskCreationOptions.AttachedToParent);
        //    //t.Wait();
        //    //T result = t.Result;

        //    T result = await Task<T>.Factory.StartNew(action, TaskCreationOptions.AttachedToParent);
        //    return result;

        //    //}
        //}
        #endregion

        /// <summary>
        /// Load in memory a 3D model.
        /// The supported types are defined in the <see cref="VALID_EXTENSIONS"/> property.
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public PTModel LoadModel(string assetName)
        {
            string contentPath = fileValidations(assetName);

            KeyValuePair<PTModel, Scene> pair;
            ContentsPool.ImportedModels.TryGetValue(contentPath.ToLower(), out pair);

            Scene scene = null;
            if (!pair.Equals(default(KeyValuePair<PTModel, Scene>))) // Previously loaded.
            {
                Log.Debug("Model from path: -{0}- already loaded in memory. Importing from loaded Scene...", assetName);
                scene = pair.Value;
                pair = loadModel(contentPath, scene);
            }
            else // Not previously loaded.
            {
                Log.Debug("Loading Model from path: -{0}-", assetName);
                pair = loadModel(contentPath, scene);
                ContentsPool.ImportedModels.Add(contentPath.ToLower(), pair);
            }

            Log.Debug("Finished loaded Model from path: -{0}-", assetName);

            return pair.Key;
        }

        ///// <summary>
        ///// Load in memory a 3D model.
        ///// The supported types are defined in the <see cref="VALID_EXTENSIONS"/> property.
        ///// </summary>
        ///// <param name="assetName"></param>
        ///// <returns></returns>
        //public Task<PTModel> LoadModelAsync(string assetName)
        //{
        //    Log.Debug("Loading Model from path: -{0}-", assetName);
        //    string contentPath = fileValidations(assetName);

        //    Func<PTModel> fun = new Func<PTModel>(() =>
        //    {
        //        PTModel model = loadModel(contentPath);
        //        return model;
        //    });

        //    return Task.Factory.StartNew(fun);
        //}

        /// <summary>
        /// Load in memory a shader (.fx) file.
        /// This method calls the <see cref="PTEffect.LoadContent"/> method, so if you want to inherit from <see cref="PTEffect"/>
        /// to create your own shader, you must instance the new Effect and call the <see cref="PTEffect.LoadContent"/> method manually.
        /// </summary>
        /// <param name="effectPath">The path of the .fx file</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <returns>The <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> object.</returns>
        public T LoadEffect<T>(string effectPath, List<string> includePaths) where T : IPTEffect
        {
            Log.Debug("Loading Effect from path: -{0}-", effectPath);

            string contentPath = fileValidations(effectPath);
            IPTEffect11 effect = new PTEffect11(Game, effectPath, includePaths);
            effect.LoadContent(this);

            return (T)effect;
        }

        ///// <summary>
        ///// Load in memory a shader (.fx) file.
        ///// This method calls the <see cref="PTEffect.LoadContent"/> method, so if you want to inherit from <see cref="PTEffect"/>
        ///// to create your own shader, you must instance the new Effect and call the <see cref="PTEffect.LoadContent"/> method manually.
        ///// </summary>
        ///// <param name="vertexShaderPath">The path of the vertexShader file</param>
        ///// <param name="pixelShaderPath">The path of the pixelShader file</param>
        ///// <returns>The <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> object.</returns>
        //public T LoadEffect<T>(string vertexShaderPath, string pixelShaderPath) where T : PTEffect
        //{
        //    Log.Debug("Loading Vertex Shader from path: -{0}-", vertexShaderPath);
        //    Log.Debug("Loading Pixel Shader from path: -{0}-", pixelShaderPath);

        //    // VertexShader
        //    string contentPath = fileValidations(vertexShaderPath);

        //    PTShader resultShader = new PTShader();
        //    LoadVertexShaderInto(contentPath, ref resultShader);
        //    LoadPixelShaderInto(contentPath, ref resultShader, -1);

        //    PTEffect effect = new PTEffect(Game, resultShader);
        //    effect.LoadContent(this);

        //    return (T)effect;
        //}

        /// <summary>
        /// Load into memory an object of any supported type from the source code.
        /// ** It works only for Shaders for now. **
        /// </summary>
        /// <param name="source">The complete source code to be compiled.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="saveCompiledFileName">Optional file name to save the compiled shader to.</param>
        /// <returns>The object loaded in memory.</returns>
        /// <exception cref = "ResourceNotSupportedException" > When the resource is not found.</exception>
        public PTShader LoadShaderFromSource(string source, List<string> includePaths, string saveCompiledFileName = null)
        {
            // Load the asset.
            PTShader shader = new PTShader();
            shader.VertexShader = compileVertexShader(source, "VertexShaderEntry", ref shader, includePaths, true, saveCompiledFileName);
            shader.PixelShader = compilePixelShader(source, "PixelShaderEntry", ref shader, includePaths, true, saveCompiledFileName);

            return shader;
        }

        #region Load Shader
        /// <summary>
        /// <para>
        /// Load in memory a shader (.fx) file. This method return a <see cref="PTShader"/> compiled file.
        /// It does not return an <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> file, for that use the <see cref="LoadEffect{T}(string, List{string})"/> method.
        /// Check in the <see cref="ContentsPool"/> if the Shader has already been loaded previously.
        /// </para>
        /// It also saves the compiled shaders into a physical file.
        /// </summary>
        /// <param name="assetName">The name of the .fx file</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="vertexShaderEntry">The main entry method for the vertex shader.</param>
        /// <param name="pixelShaderEntry">The main entry method for the pixel shader.</param>
        /// <returns></returns>
        public PTShader LoadShader(string assetName, List<string> includePaths, string vertexShaderEntry = "VertexShaderEntry", string pixelShaderEntry = "PixelShaderEntry")
        {
            vertexShaderEntry = vertexShaderEntry == null ? "VertexShaderEntry" : vertexShaderEntry;
            pixelShaderEntry = pixelShaderEntry == null ? "PixelShaderEntry" : pixelShaderEntry;
            string contentPath = fileValidations(assetName);

            //if (ContentsPool.Shaders.ContainsKey(contentPath.ToLower()))
            //{
            //    Log.Debug("Shader from path: -{0}- already loaded in memory.", assetName);
            //    return ContentsPool.Shaders[contentPath.ToLower()];
            //}

            PTShader referenceShader = null;
            if ((referenceShader = ContentsPool.GetShader(contentPath.ToLower())) != null)
            {
                Log.Debug("Shader from path: -{0}- already loaded in memory.", assetName);
                return referenceShader;
            }

            PTShader shader = new PTShader();
            shader.LoadContent(this);
            shader.VertexShader = compileVertexShader(contentPath, vertexShaderEntry, ref shader, includePaths, saveFileName: assetName);
            shader.PixelShader = compilePixelShader(contentPath, pixelShaderEntry, ref shader, includePaths, saveFileName: assetName);

            Log.Debug("Loaded Shader from path: -{0}-", assetName);
            //ContentsPool.Shaders.Add(contentPath.ToLower(), shader);
            ContentsPool.AddShader(referenceShader, contentPath.ToLower());

            return shader;
        }

        /// <summary>
        /// <para>
        /// Load in memory a shader (.fx) file. This method return a <see cref="PTShader"/> compiled file.
        /// It does not return an <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> file, for that use the <see cref="LoadEffect{T}(string, List{string})"/> method.
        /// Check in the <see cref="ContentsPool"/> if the Shader has already been loaded previously.
        /// </para>
        /// It also saves the compiled shaders into a physical file.
        /// </summary>
        /// <param name="assetName">The name of the .fx file</param>
        /// <param name="referenceShader">The <see cref="PTShader"/> object where the vertexShader will be loaded into.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="vertexShaderEntry">The main entry method for the vertex shader.</param>
        /// <returns></returns>
        public void LoadVertexShaderInto(string assetName, ref PTShader referenceShader, List<string> includePaths, string vertexShaderEntry = "VertexShaderEntry")
        {
            vertexShaderEntry = vertexShaderEntry == null ? "VertexShaderEntry" : vertexShaderEntry;
            string contentPath = fileValidations(assetName);

            // If the shader has been loaded, check if it has a pixel shader already loaded.
            PTShader existingShader = ContentsPool.GetShader(contentPath.ToLower());
            if (existingShader != null)
            {
                if (existingShader.VertexShader == null)
                    existingShader.VertexShader = compileVertexShader(contentPath, vertexShaderEntry, ref existingShader, includePaths, saveFileName: assetName);
                else
                    Log.Debug("Pixel Shader from path: -{0}- already loaded in memory.", assetName);

                // Update the shader.
                ContentsPool.AddShader(existingShader, contentPath.ToLower());
                referenceShader.VertexShader = existingShader.VertexShader;
                referenceShader.VertexShaderSignature = existingShader.VertexShaderSignature;
                return;
            }

            if (referenceShader == null)
            {
                referenceShader = new PTShader();
                referenceShader.LoadContent(this);
            }
            if (!referenceShader.IsContentLoaded)
                referenceShader.LoadContent(this);
            if (!referenceShader.Paths.ContainsKey(ShaderType.Vertex))
                referenceShader.Paths.Add(ShaderType.Vertex, contentPath);
            else
                Log.Warning("Already added a vertex shader path");
            referenceShader.VertexShader = compileVertexShader(contentPath, vertexShaderEntry, ref referenceShader, includePaths, saveFileName: assetName);

            Log.Debug("Loaded Vertex Shader from path: -{0}-", assetName);
            //ContentsPool.Shaders.Add(contentPath.ToLower(), referenceShader);
            ContentsPool.AddShader(referenceShader, contentPath.ToLower());
        }

        /// <summary>
        /// <para>
        /// Load in memory a shader (.fx) file. This method return a <see cref="PTShader"/> compiled file.
        /// It does not return an <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> file, for that use the <see cref="LoadEffect{T}(string, List{string})"/> method.
        /// Check in the <see cref="ContentsPool"/> if the Shader has already been loaded previously.
        /// </para>
        /// It also saves the compiled shaders into a physical file.
        /// </summary>
        /// <param name="assetName">The name of the .fx file</param>
        /// <param name="referenceShader">The <see cref="PTShader"/> object where the pixelShader will be loaded into.</param>
        /// <param name="customPixelShaderSlot">If this is a custom shader (Not the 'Linking' shaders), tell which slot will be used. The Engine support up to 10 custom shaders.
        /// <para>The custom shader must be added inside the 'Content/Effects' folder and must be called: 'Custom_ShaderPS0.fx', the 0 means the slot used.
        /// If the default shaders want to be used, use -1 as value.</para>
        /// </param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="pixelShaderEntry">The main entry method for the pixel shader.</param>
        /// <returns></returns>
        public void LoadPixelShaderInto(string assetName, ref PTShader referenceShader, int customPixelShaderSlot, List<string> includePaths, string pixelShaderEntry = "PixelShaderEntry")
        {
            pixelShaderEntry = pixelShaderEntry == null ? "PixelShaderEntry" : pixelShaderEntry;
            string contentPath = fileValidations(assetName);

            //// If the shader has been loaded, check if it has a pixel shader already loaded.
            //if (ContentsPool.Shaders.ContainsKey(contentPath.ToLower()) && ContentsPool.Shaders[contentPath.ToLower()].CustomPixelShaderSlot == customShaderSlot)
            //{
            //    referenceShader = ContentsPool.Shaders[contentPath.ToLower()];
            //    if (referenceShader.PixelShader == null)
            //        referenceShader.PixelShader = compilePixelShader(contentPath, pixelShaderEntry, ref referenceShader, includePaths, saveFileName: assetName);
            //    else
            //        Log.Debug("Pixel Shader from path: -{0}- already loaded in memory.", assetName);

            //    ContentsPool.Shaders[contentPath.ToLower()] = referenceShader;
            //    return;
            //}

            // If the shader has been loaded, check if it has a pixel shader already loaded.
            PTShader existingShader = ContentsPool.GetShader(contentPath.ToLower(), customPixelShaderSlot);
            if (existingShader != null)
            {
                if (existingShader.PixelShader == null)
                    existingShader.PixelShader = compilePixelShader(contentPath, pixelShaderEntry, ref existingShader, includePaths, saveFileName: assetName);
                else
                    Log.Debug("Pixel Shader from path: -{0}- already loaded in memory.", assetName);

                ContentsPool.AddShader(existingShader, contentPath.ToLower(), customPixelShaderSlot);
                referenceShader = existingShader;
                return;
            }

            if (referenceShader == null)
            {
                referenceShader = new PTShader();
                referenceShader.LoadContent(this);
            }
            if (!referenceShader.IsContentLoaded)
                referenceShader.LoadContent(this);
            if (!referenceShader.Paths.ContainsKey(ShaderType.Pixel))
                referenceShader.Paths.Add(ShaderType.Pixel, contentPath);
            else
                Log.Warning("Already added a pixel shader path");
            referenceShader.CustomPixelShaderSlot = customPixelShaderSlot;
            referenceShader.PixelShader = compilePixelShader(contentPath, pixelShaderEntry, ref referenceShader, includePaths, saveFileName: assetName);

            Log.Debug("Loaded Pixel Shader from path: -{0}-", assetName);
            //ContentsPool.Shaders.Add(contentPath.ToLower(), referenceShader);
            ContentsPool.AddShader(referenceShader, contentPath.ToLower(), customPixelShaderSlot);
        }
        #endregion

        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {

        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            ContentsPool.DisposeShaders();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Private method to load a model.
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private KeyValuePair<PTModel, Scene> loadModel(string assetName, Scene scene = null)
        {
            PTModel model = null;
            model = new PTModel(Game);
            model.Name = Path.GetFileName(assetName);

            // Create a new importer and imports the scene from Assimp
            if (scene == null)
            {
                using (AssimpContext importer = new AssimpContext())
                {
                    if (!importer.IsImportFormatSupported(Path.GetExtension(assetName)))
                        throw new LoadContentException("Model format " + Path.GetExtension(assetName) + " is not supported! - Cannot load {0}", assetName);

                    ////This is how we add a configuration (each config is its own class)
                    //NormalSmoothingAngleConfig config = new NormalSmoothingAngleConfig(66.0f);
                    //importer.SetConfig(config);

                    //Assimp.Configs.ASEReconstructNormalsConfig config = new ASEReconstructNormalsConfig(true);
                    //importer.SetConfig(config);

                    //Assimp.Configs.FBXImportAllGeometryLayersConfig conf = new FBXImportAllGeometryLayersConfig(false);
                    //importer.SetConfig(conf);

                    //Assimp.Configs.NormalizeVertexComponentsConfig conf = new NormalizeVertexComponentsConfig(true);
                    //importer.SetConfig(conf);

                    //This is how we add a logging callback 
                    LogStream logstream = new LogStream(delegate (String msg, String userData)
                    {
                        Log.Debug("{0} -> {1}", userData, msg);
                    });
                    logstream.Attach();

                    //Import the model. All configs are set. The model is imported, loaded into managed memory. 
                    // Then the unmanaged memory is released, and everything is reset.

                    PostProcessSteps steps = PostProcessSteps.None
                        //| PostProcessSteps.GenerateSmoothNormals
                        //| PostProcessSteps.SplitLargeMeshes
                        //| PostProcessSteps.MakeLeftHanded
                        | PostProcessSteps.Triangulate
                        | PostProcessSteps.FlipUVs
                        //| PostProcessSteps.SortByPrimitiveType
                        | PostProcessSteps.JoinIdenticalVertices
                        | PostProcessSteps.TransformUVCoords
                        //| PostProcessSteps.OptimizeMeshes
                        | PostProcessSteps.RemoveRedundantMaterials
                        //| PostProcessSteps.MakeLeftHanded
                        //| PostProcessSteps.GenerateUVCoords
                        //| PostProcessSteps.FindInstances ;
                        ;

                    if (Path.GetExtension(assetName) == ".obj")
                        steps =
                            PostProcessSteps.GenerateSmoothNormals
                            | PostProcessSteps.CalculateTangentSpace
                            | PostProcessSteps.ValidateDataStructure
                            //| PostProcessSteps.SplitLargeMeshes
                            | PostProcessSteps.GenerateUVCoords
                            //| PostProcessSteps.FindInstances 
                            //| PostProcessSteps.LimitBoneWeights
                            //| PostProcessSteps.MakeLeftHanded
                            | PostProcessSteps.JoinIdenticalVertices
                            | PostProcessSteps.TransformUVCoords
                            | PostProcessSteps.RemoveRedundantMaterials
                            ;

                    scene = importer.ImportFile(assetName, steps);
                    importer.RemoveConfigs();
                    importer.RemoveIOSystem();
                    importer.Dispose();
                }
            }

            // Create model from assimp
            PTModel.FromAssimpScene(ref scene, ref model, assetName);

            // Save the loaded model into a physical file.
            saveModelToFile(model);

            //return model;
            return new KeyValuePair<PTModel, Scene>(model, scene);
        }

        private void saveModelToFile(PTModel model)
        {
            //PTSerializer serializer = new PTSerializer();
            //serializer.Serialize(model, true, model.Name + ".ponche");

            //PTModel algo = serializer.Deserialize<PTModel>(model.Name + ".ponche");

            //System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(XmlSerializationTest));
            //using (TextWriter writer = new StreamWriter("modeloXML.ponche"))
            //{
            //    XmlSerializationTest test = new XmlSerializationTest();
            //    test.Path = new TexturePath("Soy una pruebaaaaa");
            //    test.ShipCost = 10;
            //    test.OrderDate = DateTime.Now.ToShortDateString();
            //    serializer.Serialize(writer, test);
            //}
        }

        #region Compile Shaders
        /// <summary>
        /// Compile a Vertex Shader.
        /// </summary>
        /// <param name="assetNameOrSource">The path of the effect.</param>
        /// <param name="entryPoint">String of the name of the shader method to work as the vertex shader entry point.</param>
        /// <param name="shader">The reference to the <see cref="PTShader"/> used.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="fromSource">Value to indicate what the assetNameOrSource represent to search a file or compile the sent code.</param>
        /// <param name="saveFileName">Optional file name to save the compiled shader to.</param>
        /// <returns>Return the VertexShader object of the compiled shader.</returns>
        /// <exception cref="PoncheToolkit.Util.Exceptions.ResourceCompilationException"/>
        private VertexShader compileVertexShader(string assetNameOrSource, string entryPoint, ref PTShader shader, List<string> includePaths, bool fromSource = false, string saveFileName = null)
        {
            ShaderFlags flags = ShaderFlags.None;
#if DEBUG
            flags |= ShaderFlags.Debug;
#endif

            Log.Debug("Compiling Vertex Shader - {0}", assetNameOrSource);
            if (fromSource)
            {
                using (var compiledVertexShader = ShaderBytecode.Compile(assetNameOrSource, entryPoint, SHADER_VERTEX_SHADER_VERSION_5, flags))
                {
                    if (compiledVertexShader.Bytecode == null || compiledVertexShader.HasErrors)
                        throw new ResourceCompilationException(string.Format("Error compiling vertex shader. -- {0} -- Message: {1}", Path.GetFileName(assetNameOrSource), compiledVertexShader.Message));

                    saveShader(saveFileName, shader, compiledVertexShader, VERTEX_SHADER_COMPILED_NAME_EXTENSION);

                    //VertexShader vertexShader = new VertexShader(Game.Renderer.Device, compiledVertexShader, shader.Linkage);
                    VertexShader vertexShader = new VertexShader(Game.Renderer.Device, compiledVertexShader);
                    shader.VertexShaderSignature = ShaderSignature.GetInputSignature(compiledVertexShader);
                    return vertexShader;
                }
            }

            if (includePaths == null)
            {
                Uri currentPath = new Uri(Directory.GetParent(assetNameOrSource).FullName);
                Uri contentPath = new Uri(Game.ContentDirectoryFullPath);
                string includePath = contentPath.MakeRelativeUri(currentPath).OriginalString.Replace(Game.ContentDirectoryName, "").Trim('/', '\\');
                includePaths = new List<string>() { includePath };
            }

            // Compile Vertex Shader
            using (CompilationResult compiledVertexShader = ShaderBytecode.CompileFromFile(assetNameOrSource, entryPoint, SHADER_VERTEX_SHADER_VERSION_5, flags, include: new PTInclude(includePaths)))
            {
                if (compiledVertexShader.Bytecode == null || compiledVertexShader.HasErrors)
                    throw new ResourceCompilationException(string.Format("Error compiling vertex shader. -- {0} -- Message: {1}", Path.GetFileName(assetNameOrSource), compiledVertexShader.Message));

                saveShader(saveFileName, shader, compiledVertexShader, VERTEX_SHADER_COMPILED_NAME_EXTENSION);

                //VertexShader vertexShader = new VertexShader(Game.Renderer.Device, compiledVertexShader, shader.Linkage);
                VertexShader vertexShader = new VertexShader(Game.Renderer.Device, compiledVertexShader);
                shader.VertexShaderSignature = ShaderSignature.GetInputSignature(compiledVertexShader);


                //// TODO: Shader dynamic linking.
                //using (Module shaderLibrary = new Module(compiledVertexShader))
                //{
                //    ModuleInstance libraryInstance = new ModuleInstance("algodon", shaderLibrary);
                //    libraryInstance.BindConstantBuffer(0, 0, 0);
                //    libraryInstance.BindConstantBuffer(1, 1, 0);
                //    libraryInstance.BindConstantBuffer(2, 2, 0);
                //    libraryInstance.BindSampler(0, 0, 1);
                //    // Resources
                //    libraryInstance.BindResource(0, 0, 4); // Blend textures.
                //    libraryInstance.BindResource(5, 1, 1);
                //    libraryInstance.BindResource(6, 2, 1);

                //    FunctionLinkingGraph linkGraph = new FunctionLinkingGraph();
                //    ParameterDescription[] shaderParams = new ParameterDescription[]
                //    {
                //        new ParameterDescription() { Name = "InputPosition", SemanticName = "POSITION", Type = ShaderVariableType.Float, Class = ShaderVariableClass.Vector, Rows = 1, Columns = 3, InterpolationMode = SharpDX.Direct3D.InterpolationMode.Linear, Flags = ParameterFlags.In },
                //        new ParameterDescription() { Name = "InputColor", SemanticName = "COLOR", Type = ShaderVariableType.Float, Class = ShaderVariableClass.Vector, Rows = 1, Columns = 4, InterpolationMode = SharpDX.Direct3D.InterpolationMode.Linear, Flags = ParameterFlags.In },
                //        new ParameterDescription() { Name = "InputTexCoord", SemanticName = "TEXCOORD", Type = ShaderVariableType.Float, Class = ShaderVariableClass.Vector, Rows = 1, Columns = 2, InterpolationMode = SharpDX.Direct3D.InterpolationMode.Linear, Flags = ParameterFlags.In },
                //        new ParameterDescription() { Name = "InputNormal", SemanticName = "NORMAL", Type = ShaderVariableType.Float, Class = ShaderVariableClass.Vector, Rows = 1, Columns = 3, InterpolationMode = SharpDX.Direct3D.InterpolationMode.Linear, Flags = ParameterFlags.In },
                //        new ParameterDescription() { Name = "InputTangent", SemanticName = "TANGENT", Type = ShaderVariableType.Float, Class = ShaderVariableClass.Vector, Rows = 1, Columns = 3, InterpolationMode = SharpDX.Direct3D.InterpolationMode.Linear, Flags = ParameterFlags.In },
                //        new ParameterDescription() { Name = "InputBinormal", SemanticName = "BINORMAL", Type = ShaderVariableType.Float, Class = ShaderVariableClass.Vector, Rows = 1, Columns = 3, InterpolationMode = SharpDX.Direct3D.InterpolationMode.Linear, Flags = ParameterFlags.In },
                //    };

                //    LinkingNode vertexInputNode = linkGraph.SetInputSignature(shaderParams);
                //}

                return vertexShader;
            }
        }

        /// <summary>
        /// Compile a pixel shader.
        /// </summary>
        /// <param name="assetNameOrSource">The path of the effect.</param>
        /// <param name="entryPoint">String of the name of the shader method to work as the pixel shader entry point.</param>
        /// <param name="shader">The reference to the <see cref="PTShader"/> used.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="fromSource">Value to indicate what the assetNameOrSource represent to search a file or compile the sent code.</param>
        /// <param name="saveFileName">Optional file name to save the compiled shader to.</param>
        /// <returns>Return the PixelShader object of the compiled shader.</returns>
        /// <exception cref="PoncheToolkit.Util.Exceptions.ResourceCompilationException"/>
        private PixelShader compilePixelShader(string assetNameOrSource, string entryPoint, ref PTShader shader, List<string> includePaths,
            bool fromSource = false, string saveFileName = null)
        {
            ShaderFlags flags = ShaderFlags.None;
#if DEBUG
            flags |= ShaderFlags.Debug;
#endif

            Log.Debug("Compiling Pixel Shader - {0}", assetNameOrSource);
            // Compile Pixel shader from source
            if (fromSource)
            {
                using (var compiledPixelShader = ShaderBytecode.Compile(assetNameOrSource, entryPoint, SHADER_PIXEL_SHADER_VERSION_5, flags))
                {
                    if (compiledPixelShader.Bytecode == null || compiledPixelShader.HasErrors)
                        throw new ResourceCompilationException(string.Format("Error compiling pixel shader. -- {0} -- Message: {1}", Path.GetFileName(assetNameOrSource), compiledPixelShader.Message));

                    saveShader(saveFileName, shader, compiledPixelShader, PIXEL_SHADER_COMPILED_NAME_EXTENSION);
                    PixelShader pixelShader = new PixelShader(Game.Renderer.Device, compiledPixelShader);
                    return pixelShader;
                }
            }

            if (includePaths == null)
            {
                Uri currentPath = new Uri(Directory.GetParent(assetNameOrSource).FullName);
                Uri contentPath = new Uri(Game.ContentDirectoryFullPath);
                string includePath = contentPath.MakeRelativeUri(currentPath).OriginalString.Replace(Game.ContentDirectoryName, "").Trim('/', '\\');
                includePaths = new List<string>() { includePath };
            }

            // Compile Pixel shader from file
            List<ShaderMacro> macros = new List<ShaderMacro>() { new ShaderMacro("MAX_LIGHTS", PTLight.FORWARD_SHADING_MAX_LIGHTS) };
            //customPixelShaderSlot = 0; // For testing, gets the Custom_ShaderPS0.fx
            if (shader.CustomPixelShaderSlot >= 0)
                macros.Add(new ShaderMacro("CUSTOM_IMPL_PS" + shader.CustomPixelShaderSlot, 1));

            using (var compiledPixelShader = ShaderBytecode.CompileFromFile(assetNameOrSource, entryPoint, SHADER_PIXEL_SHADER_VERSION_5, flags, defines: macros.ToArray(), include: new PTInclude(includePaths)))
            {
                if (compiledPixelShader.Bytecode == null || compiledPixelShader.HasErrors)
                    throw new ResourceCompilationException(string.Format("Error compiling pixel shader. -- {0} -- Message: {1}", Path.GetFileName(assetNameOrSource), compiledPixelShader.Message));

                saveShader(saveFileName, shader, compiledPixelShader, PIXEL_SHADER_COMPILED_NAME_EXTENSION);
                PixelShader pixelShader = new PixelShader(Game.Renderer.Device, compiledPixelShader);

                //PixelShader pixelShader = new PixelShader(Game.Renderer.Device, compiledPixelShader, shader.Linkage);
                //createClassInstances(compiledPixelShader.Bytecode, ref shader);

                return pixelShader;
            }
        }

        /// <summary>
        /// Compile a pixel shader.
        /// </summary>
        /// <param name="compiledPath">The path of the effect.</param>
        /// <param name="shader">The reference to the <see cref="PTShader"/> used.</param>
        /// <returns>Return the PixelShader object of the compiled shader.</returns>
        /// <exception cref="PoncheToolkit.Util.Exceptions.ResourceCompilationException"/>
        private PixelShader fromCompiledPixelShader(string compiledPath, ref PTShader shader)
        {
            ShaderFlags flags = ShaderFlags.None;
#if DEBUG
            flags |= ShaderFlags.Debug;
#endif

            Log.Debug("Retrieving compiled Pixel Shader - {0}", compiledPath);
            // Read the compiled pixel shader.
            using (var compiledPixelShader = ShaderBytecode.FromFile(compiledPath))
            {
                if (compiledPixelShader.Data == null)
                    throw new ResourceCompilationException(string.Format("Error retrieving compiled pixel shader. -- {0}", Path.GetFileName(compiledPath)));

                PixelShader pixelShader = new PixelShader(Game.Renderer.Device, compiledPixelShader);

                //PixelShader pixelShader = new PixelShader(Game.Renderer.Device, compiledPixelShader, shader.Linkage);
                //createClassInstances(compiledPixelShader.Data, ref shader);

                return pixelShader;
            }
        }

        /// <summary>
        /// Assign the <see cref="ClassInstance"/> objects to the shader <see cref="ClassLinkage"/>.
        /// </summary>
        /// <param name="byteCode">The byteCode representing the compiled shader.</param>
        /// <param name="shader">The reference to the <see cref="PTShader"/> used.</param>
        private void createClassInstances(byte[] byteCode, ref PTShader shader)
        {
            using (ShaderReflection reflection = new ShaderReflection(byteCode))
            {
                // Initialize the ClassInstances array with the size of the interfaces declared in the shader.
                shader.ClassInstances = new ClassInstance[reflection.InterfaceSlotCount];

                // ===== Material
                ShaderReflectionVariable varMaterial = reflection.GetVariable("MaterialAbstract");
                int materialSlot = varMaterial.GetInterfaceSlot(0);
                ClassInstance classMaterial = shader.Linkage.GetClassInstance("Material", 0);
                //ClassInstance classMaterial = new ClassInstance(shader.Linkage, "MaterialClass", 0, 0, 0, 0);
                classMaterial.DebugName = "MaterialClassDebug";
                shader.ClassInstances[materialSlot] = classMaterial;

                // ===== Global Data
                ShaderReflectionVariable varGlobalData = reflection.GetVariable("GlobalDataAbstract");
                int globalDataSlot = varGlobalData.GetInterfaceSlot(0);
                ClassInstance classGlobalData = shader.Linkage.GetClassInstance("GlobalData", 0);
                //ClassInstance classGlobalData = new ClassInstance(shader.Linkage, "GlobalDataClass", 1, 0, 0, 0);
                classGlobalData.DebugName = "GlobalDataClassDebug";
                shader.ClassInstances[globalDataSlot] = classGlobalData;

                // ===== Lights
                ShaderReflectionVariable varLights = reflection.GetVariable("LightsAbstract");
                for (int i = 0; i < PTLight.FORWARD_SHADING_MAX_LIGHTS; i++) // TODO: send max lights supported by engine.
                {
                    int lightSlot = varLights.GetInterfaceSlot(i);
                    ClassInstance classLight = shader.Linkage.GetClassInstance("Lights", i);
                    //ClassInstance classLight = new ClassInstance(shader.Linkage, "LightClass", 2, i * Utilities.SizeOf<LightStruct>(), 0, 0);
                    classLight.DebugName = "LightDebug - " + i;
                    shader.ClassInstances[lightSlot] = classLight;
                }
            }
        }

        /// <summary>
        /// Save the shader in a physical path.
        /// </summary>
        /// <param name="fileRelativePath"></param>
        /// <param name="compiledShader"></param>
        /// <param name="typeWithExtension"></param>
        private void saveShader(string fileRelativePath, PTShader shader, CompilationResult compiledShader, string typeWithExtension)
        {
            if (string.IsNullOrEmpty(fileRelativePath))
                fileRelativePath = Path.GetFileNameWithoutExtension(fileRelativePath);

            string path = Path.Combine(Game.ContentDirectoryFullPath, System.IO.Path.GetDirectoryName(fileRelativePath));
            string fileName = Path.GetFileNameWithoutExtension(fileRelativePath);
            fileName = shader.CustomPixelShaderSlot >= 0 ? fileName + shader.CustomPixelShaderSlot : fileName;
            using (FileStream stream = File.Create(Path.Combine(path, fileName + typeWithExtension)))
            {
                compiledShader.Bytecode.Save(stream);
            }
        }
        #endregion

        #region Custom verification methods
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
        /// Make validations of the file.
        /// </summary>
        /// <param name="assetName">Relative path of the file.</param>
        /// <param name="referencePath">A path for reference to combine with the assetName.</param>
        /// <returns>Return the correct path of the file with the ContentManager root directory. Returns the string in lower.</returns>
        /// <exception cref="FileNotFoundException">When the resource is not found.</exception>
        /// <exception cref="ResourceNotSupportedException">When the resource is not found.</exception>
        private string fileValidations(string assetName, string referencePath = null)
        {
            string contentPath = Path.GetFullPath(Path.Combine(Game.ContentDirectoryName, assetName));
            if (!File.Exists(contentPath))
            {
                if (referencePath == null)
                    throw new FileNotFoundException(string.Format("The resource -{0}- was not found.", contentPath));

                contentPath = Path.Combine(referencePath, assetName);
                if (!File.Exists(contentPath))
                    throw new FileNotFoundException(string.Format("The resource -{0}- was not found.", contentPath));
            }

            string extension = Path.GetExtension(contentPath);
            if (!isValidExtension(extension))
                throw new ResourceNotSupportedException(string.Format("The resource with extension -{0}- is not supported.", extension));

            return contentPath;
        }

        /// <summary>
        /// Make validations of the file.
        /// </summary>
        /// <param name="assetNames">Relative path of the files.</param>
        /// <param name="combined">The unique string containing the files added so the texture to be loaded can be identified.</param>
        /// <param name="referencePath">A path for reference to combine with the assetName.</param>
        /// <returns>Return the correct path of the file with the ContentManager root directory.</returns>
        /// <exception cref="FileNotFoundException">When the resource is not found.</exception>
        /// <exception cref="ResourceNotSupportedException">When the resource is not found.</exception>
        private string[] fileValidations(string[] assetNames, out string combined, string referencePath = null)
        {
            string[] result = new string[assetNames.Length];
            combined = "";
            for (int i = 0; i < assetNames.Length; i++)
            {
                string contentPath = Path.GetFullPath(Path.Combine(Game.ContentDirectoryName, assetNames[i]));
                if (!File.Exists(contentPath))
                {
                    if (referencePath == null)
                        throw new FileNotFoundException(string.Format("The resource -{0}- was not found.", contentPath));

                    contentPath = Path.Combine(referencePath, assetNames[i]);
                    if (!File.Exists(contentPath))
                        throw new FileNotFoundException(string.Format("The resource -{0}- was not found.", contentPath));
                }

                string extension = Path.GetExtension(contentPath);
                if (!isValidExtension(extension))
                    throw new ResourceNotSupportedException(string.Format("The resource with extension -{0}- is not supported.", extension));

                combined += Path.GetFileName(contentPath) + "/";
                result[i] = contentPath;
            }

            return result;
        }
        #endregion

        #endregion
    }

#endif
}
