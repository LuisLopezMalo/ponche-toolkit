using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpDX.D3DCompiler;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Util;
using PoncheToolkit.Util.Exceptions;
using PoncheToolkit.Graphics3D;
using SharpDX.Direct3D12;
using Assimp;
using Assimp.Configs;

namespace PoncheToolkit.Core.Management.Content
{
#if DX12

    using Graphics3D.Effects;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Class that help to manage the creation and disposal of content.
    /// </summary>
    public class ContentManager12 : GameService, IContentManager12
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
        public ContentManager12(Game game)
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
            throw new NotImplementedException("Not implemented in DirectX12 yet.");
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
            throw new NotImplementedException("Not implemented in DirectX12 yet.");
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
            throw new NotImplementedException("Not implemented in DirectX12 yet.");
        }

        /// <summary>
        /// Load in memory a texture2D.
        /// </summary>
        /// <param name="texturePaths"></param>
        /// <returns></returns>
        //public Graphics2D.PTTexture2D LoadTexture2DArray(string[] assetNames)
        public Graphics2D.PTTexture2D LoadTexture2DArray(List<TexturePath> texturePaths)
        {
            throw new NotImplementedException("Not implemented in DirectX12 yet.");
        }

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
            return default(T);
        }

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
            return null;
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
            return null;
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

                    //This is how we add a configuration (each config is its own class)
                    NormalSmoothingAngleConfig config = new NormalSmoothingAngleConfig(66.0f);
                    importer.SetConfig(config);

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
                            //| PostProcessSteps.GenerateUVCoords
                            //| PostProcessSteps.FindInstances 
                            //| PostProcessSteps.LimitBoneWeights
                            //| PostProcessSteps.MakeLeftHanded
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

            //return model;
            return new KeyValuePair<PTModel, Scene>(model, scene);
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
            throw new NotImplementedException("Not implemented in DirectX12 yet.");
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
            throw new NotImplementedException("Not implemented in DirectX12 yet.");
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
            throw new NotImplementedException("Not implemented in DirectX12 yet.");
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
        private void saveShader(string fileRelativePath, CompilationResult compiledShader, string typeWithExtension)
        {
            if (!string.IsNullOrEmpty(fileRelativePath))
            {
                string path = Path.Combine(Game.ContentDirectoryFullPath, System.IO.Path.GetDirectoryName(fileRelativePath));
                string fileName = Path.GetFileNameWithoutExtension(fileRelativePath);
                using (FileStream stream = File.Create(Path.Combine(path, fileName + typeWithExtension)))
                {
                    compiledShader.Bytecode.Save(stream);
                }
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
