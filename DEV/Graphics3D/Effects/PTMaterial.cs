using PoncheToolkit.Core;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D.Effects
{
    using Cameras;
    using Core.Management.Content;
    using PoncheToolkit.Graphics2D;
    using SharpDX.Mathematics.Interop;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represent a single material that an <see cref="PTEffect"/> object has.
    /// </summary>
    public class PTMaterial : UpdatableStateObject, IInitializable, IContentLoadable
    {
        #region Fields
#if DX11
        private Game11 game;
#elif DX12
        private Game12 game;
#endif
        private bool isContentLoaded;
        private string name;
        private List<ShaderResourceView> renderShaderResourceViews;

        private Vector4 emissiveColor;
        private Vector4 ambientColor;
        private Vector4 diffuseColor;
        private Vector4 specularColor;
        private Vector4 reflectiveColor;
        private float specularPower;
        private float reflectivity;
        private float opacity;
        private float gamma;
        private bool isSpecularEnabled;
        private bool isReflectivityEnabled;
        private bool isBumpEnabled;
        private bool hasSpecularMap;
        private bool hasBumpMap;
        private int blendTexturesCount;
        private Vector2 textureTranslation;
        
        private ReflectionStruct reflectionBuffer;
        private List<PTTexture2D> textures;
        private List<PTTexturePath> texturePaths;
        private PTTexturePath[] defaultTexturePaths;

        //private PTTexture2D reflectionTexture;
        //private RenderTargetView reflectionRenderTargetView;
        //private DepthStencilView reflectionDepthStencil;

        private MaterialStruct MaterialBuffer;
        private ReflectionCamera reflectionCamera;

        #region Events
        /// <inheritdoc/>
        public event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        /// <summary>
        /// The key to retrieve the common material for wood rendering from the <see cref="PTEffect.Materials"/> dictionary.
        /// </summary>
        public static readonly string DEFAULT_COLOR_MATERIAL_KEY = "default_color_material";
        #endregion

        #region Properties
        /// <inheritdoc/>
        public bool IsContentLoaded { get { return isContentLoaded; } set { SetPropertyAsDirty(ref isContentLoaded, value); } }

        /// <summary>
        /// The name of the material.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        /// <summary>
        /// The specular power of the material.
        /// Default: 10.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public float SpecularPower
        {
            get { return specularPower; }
            set
            {
                SetProperty(ref specularPower, value);
                MaterialBuffer.SpecularPower = specularPower;
            }
        }
        /// <summary>
        /// The reflectivity of the material.
        /// Default: 1.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public float Reflectivity
        {
            get { return reflectivity; }
            set
            {
                SetProperty(ref reflectivity, value);
                MaterialBuffer.Reflectivity = reflectivity;
            }
        }
        /// <summary>
        /// The opacity of the material.
        /// Default: 1
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public float Opacity
        {
            get { return opacity; }
            set
            {
                SetProperty(ref opacity, value);
                MaterialBuffer.Opacity = opacity;
            }
        }

        /// <summary>
        /// The emmisive color of the material even if there is no light.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public Vector4 EmissiveColor
        {
            get { return emissiveColor; }
            set
            {
                SetPropertyAsDirty(ref emissiveColor, value);
                MaterialBuffer.EmissiveColor = emissiveColor;
            }
        }
        /// <summary>
        /// The ambient color of the material.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public Vector4 AmbientColor
        {
            get { return ambientColor; }
            set
            {
                SetPropertyAsDirty(ref ambientColor, value);
                MaterialBuffer.AmbientColor = ambientColor;
            }
        }
        /// <summary>
        /// The diffuse color of the material.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public Vector4 DiffuseColor
        {
            get { return diffuseColor; }
            set
            {
                SetPropertyAsDirty(ref diffuseColor, value);
                MaterialBuffer.DiffuseColor = diffuseColor;
            }
        }
        /// <summary>
        /// The specular color of the material.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public Vector4 SpecularColor
        {
            get { return specularColor; }
            set
            {
                SetPropertyAsDirty(ref specularColor, value);
                MaterialBuffer.SpecularColor = specularColor;
            }
        }
        /// <summary>
        /// The emmisive color of the material.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public Vector4 ReflectiveColor
        {
            get { return reflectiveColor; }
            set
            {
                SetPropertyAsDirty(ref reflectiveColor, value);
                MaterialBuffer.ReflectiveColor = reflectiveColor;
            }
        }
        /// <summary>
        /// Set or get if the material render the specular lighting.
        /// Default: false
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public bool IsSpecularEnabled
        {
            get { return isSpecularEnabled; }
            set
            {
                SetPropertyAsDirty(ref isSpecularEnabled, value);
                MaterialBuffer.IsSpecular = isSpecularEnabled == true ? 1 : 0;
            }
        }

        /// <summary>
        /// Set or get if the material render the specular lighting.
        /// Default: false
        /// <para>Set as dirty.</para>
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public bool IsReflectivityEnabled
        {
            get { return isReflectivityEnabled; }
            set
            {
                SetPropertyAsDirty(ref isReflectivityEnabled, value);
                MaterialBuffer.IsReflective = isReflectivityEnabled == true ? 1 : 0;
            }
        }

        /// <summary>
        /// Set or get if the material render the specular lighting.
        /// Default: false
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public bool IsBumpEnabled
        {
            get { return isBumpEnabled; }
            set
            {
                SetPropertyAsDirty(ref isBumpEnabled, value);
                MaterialBuffer.IsBump = isBumpEnabled == true ? 1 : 0;
            }
        }

        /// <summary>
        /// Set or get if the material has a specular map specified to be sento to the gpu.
        /// If not, the specular lighting is set overall the model/mesh.
        /// Default: false
        /// <para>Set as dirty.</para>
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public bool HasSpecularMap
        {
            get { return hasSpecularMap; }
            set
            {
                SetPropertyAsDirty(ref hasSpecularMap, value);
                MaterialBuffer.HasSpecularMap = hasSpecularMap == true ? 1 : 0;
            }
        }

        /// <summary>
        /// Set or get if the material has a bump map specified to be sento to the gpu.
        /// Default: false
        /// <para>Set as dirty.</para>
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public bool HasBumpMap
        {
            get { return hasBumpMap; }
            set
            {
                SetProperty(ref hasBumpMap, value);
                //MaterialBuffer.HasBumpMap = hasBumpMap == true ? 1 : 0;
            }
        }

        /// <summary>
        /// The number of textures sent to the gpu.
        /// Default: 0
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public int TexturesCount
        {
            get { return blendTexturesCount; }
            set
            {
                SetPropertyAsDirty(ref blendTexturesCount, value);
                MaterialBuffer.BlendTexturesCount = blendTexturesCount;
            }
        }

        /// <summary>
        /// The number of textures sent to the gpu.
        /// Default: 1
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public float Gamma
        {
            get { return gamma; }
            set
            {
                SetPropertyAsDirty(ref gamma, value);
                MaterialBuffer.Gamma = gamma;
            }
        }

        /// <summary>
        /// The translation of the UV coordinates for the blended textures.
        /// Default: Vector3(0, 0).
        /// <para>Set as dirty.</para>
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public Vector2 TextureTranslation
        {
            get { return textureTranslation; }
            set
            {
                SetProperty(ref textureTranslation, value);
                MaterialBuffer.TextureTranslation = textureTranslation;
            }
        }

        /// <summary>
        /// Get or set the <see cref="ReflectionStruct"/> resource to be updated to the gpu.
        /// This value is automatically recreated inside the <see cref="UpdateState"/> method when any of the properties has changed.
        /// </summary>
        internal ReflectionStruct ReflectionBuffer
        {
            get { return reflectionBuffer; }
            set { SetPropertyAsDirty(ref reflectionBuffer, value); }
        }

        /// <summary>
        /// The textures used in this model.
        /// </summary>
        public List<PTTexture2D> Textures
        {
            get { return textures; }
            set { SetProperty(ref textures, value); }
        }

        /// <summary>
        /// The paths of the textures for this model.
        /// </summary>
        [Util.Reflection.PTSerializableProperty()]
        public List<PTTexturePath> TexturePaths
        {
            get { return texturePaths; }
        }

        /// <summary>
        /// Value to indicate if the model has a texture, so it can be sent to the shader correctly.
        /// </summary>
        public bool HasTexture
        {
            get { return textures.Count > 0; }
        }

        /// <summary>
        /// The game instance.
        /// </summary>
#if DX11
        public Game11 Game
        {
            get { return game; }
        }
#elif DX12
        public Game12 Game
        {
            get { return game; }
        }
#endif

        /// <inheritdoc/>
        public bool IsInitialized { get; set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
#if DX11
        public PTMaterial(Game11 game)
#elif DX12
        public PTMaterial(Game12 game)
#endif
            : this(game, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance</param>
        /// <param name="defaultTexturePaths">The path to load a default texture when the <see cref="LoadContent"/> method is called.</param>
#if DX11
        public PTMaterial(Game11 game, params PTTexturePath[] defaultTexturePaths)
#elif DX12
        public PTMaterial(Game12 game, params TexturePath[] defaultTexturePaths)
#endif
        {
            this.game = game;
            this.defaultTexturePaths = defaultTexturePaths;
            this.reflectionCamera = new ReflectionCamera(game);

            MaterialBuffer = new MaterialStruct();

            this.Opacity = 1;
            this.Reflectivity = 1;
            this.SpecularPower = 10;
            this.IsSpecularEnabled = false;
            this.IsBumpEnabled = false;
            this.IsReflectivityEnabled = false;
            this.TexturesCount = 0;
            this.Gamma = game.Settings.Gamma;
            this.HasSpecularMap = false;
            this.TextureTranslation = Vector2.Zero;
            this.AmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            this.DiffuseColor = new Vector4(0.7f, 0.7f, 0.7f, 1);
            this.SpecularColor = new Vector4(0.9f, 0.9f, 0.9f, 1);
            this.ReflectiveColor = new Vector4(0.9f, 0.9f, 0.9f, 1);

            MaterialBuffer = new MaterialStruct(emissiveColor, ambientColor, diffuseColor, specularColor, reflectiveColor,
                specularPower, reflectivity, opacity, gamma, isSpecularEnabled, isBumpEnabled, isReflectivityEnabled,
                hasSpecularMap, blendTexturesCount, textureTranslation);

            textures = new List<PTTexture2D>();
            renderShaderResourceViews = new List<ShaderResourceView>();
            texturePaths = new List<PTTexturePath>();
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public void Initialize()
        {
            if (IsInitialized)
                return;

            reflectionCamera.Initialize();

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// If there is no TexturePath or Texture added, here it adds a default texture.
        /// Create a default Sampler.
        /// </summary>
        /// <param name="contentManager"></param>
        public void LoadContent(IContentManager contentManager)
        {
            // Load the default textures.
            //if (defaultTexturePaths != null)
            //    AddTexture(0, defaultTexturePaths);

            if (defaultTexturePaths != null)
            {
                for (int i = 0; i < defaultTexturePaths.Length; i++)
                    AddTexturePath(defaultTexturePaths[i], true);
            }
            
            UpdateState();

            reflectionCamera.LoadContent(contentManager);

            isContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Update the state of the Material.
        /// If any property of lighting is dirty, the values are recreated.
        /// Also check for newly added texture paths to create and load the textures.
        /// </summary>
        /// <returns></returns>
        public override bool UpdateState()
        {
            if (IsStateUpdated)
                return IsStateUpdated;

            // === TEXTURE PATHS === Update textures. It only loads the ones that has not been previously loaded.
            object obj;
            DirtyProperties.TryGetValue(nameof(TexturePaths), out obj);
            if (obj != null)
            {
                // Load textures.
                #region Single (Not array)
                int textureCount = 0;
                for (int i = 0; i < texturePaths.Count; i++)
                {
                    if (textures.Count > i && textures[i].IsContentLoaded)
                    {
                        renderShaderResourceViews[i] = textures[i].ShaderResourceView;
                        continue;
                    }

                    // If the textures list has an element for this texturePath, assign the new texture.
                    // If not, add a new texture.
                    if (textures.Count > i)
                    {
                        RemoveTexture(textures[i]);
                        textures[i].Dispose();
                        textures[i] = game.ContentManager.LoadTexture2D(texturePaths[i].Path, generateMipMaps: texturePaths[i].GenerateMipMaps);
                        textures[i].LoadContent(Game.ContentManager);
                    }
                    else
                    {
                        PTTexture2D texture = game.ContentManager.LoadTexture2D(texturePaths[i].Path, generateMipMaps: texturePaths[i].GenerateMipMaps);
                        texture.LoadContent(Game.ContentManager);
                        textures.Add(texture);
                    }

                    textures[i].Type = texturePaths[i].Type;
                    switch (texturePaths[i].Type)
                    {
                        // Add the shaderResourceViews that will be rendered as is.
                        case PTTexture2D.TextureType.Render:
                            renderShaderResourceViews.Add(textures[i].ShaderResourceView);
                            textureCount++;
                            break;
                        case PTTexture2D.TextureType.BumpMap:
                            HasBumpMap = true;
                            break;
                        case PTTexture2D.TextureType.SpecularMap:
                            HasSpecularMap = true;
                            break;
                        case PTTexture2D.TextureType.Reflective:
                            break;
                    }

                    // Check if one of the textures is a bump map to rest it from the rendering textures count.
                    MaterialBuffer.BlendTexturesCount = renderShaderResourceViews.Count;
                    textures[i].IsContentLoaded = true;
                }
                // Check if one of the textures is a bump map to rest it from the rendering textures count.
                MaterialBuffer.BlendTexturesCount = textureCount;
                #endregion

                #region Texture2D array
                //int textureCount = 0;
                //foreach (int slot in pathsPerTexture.Keys)
                //{
                //    List<TexturePath> paths = pathsPerTexture[slot];
                //    textureCount += paths.Count;

                //    PTTexture2D texture = null;

                //    // TODO: implement creation of Texture2DArray.
                //    // If there are multiple paths, they are loaded as a Texture2D array.
                //    //if (paths.Count > 1)
                //    //    texture = game.ContentManager.LoadTexture2DArray(paths);
                //    //else
                //    //    texture = game.ContentManager.LoadTexture2D(paths[0].Path);

                //    // Right now it is one texture per slot.
                //    texture = game.ContentManager.LoadTexture2D(paths[0].Path);

                //    texture.Type = paths[0].Type;
                //    textures.Add(slot, texture);

                //    switch (texture.Type)
                //    {
                //        // Add the shaderResourceViews that will be rendered as is.
                //        case PTTexture2D.TextureType.Render:
                //            renderShaderResourceViews.Add(slot, texture.ShaderResourceView);
                //            break;
                //        case PTTexture2D.TextureType.Bump:
                //            //bumpShaderResourceView = texture.ShaderResourceView;
                //            textureCount--; // Remove the bump texture from the blend textures count.
                //            break;
                //    }
                //}
                //// Check if one of the textures is a bump map to rest it from the rendering textures count.
                //materialBuffer.RenderTexturesCount = textureCount;
                #endregion
            }

            // === TEXTURES === If textures had been added through the AddTexture method.
            #region Textures
            obj = null;
            DirtyProperties.TryGetValue(nameof(Textures), out obj);
            if (obj != null)
            {
                for (int i = 0; i < textures.Count; i++)
                {
                    if (!textures[i].IsContentLoaded)
                        textures[i].LoadContent(Game.ContentManager);

                    switch (textures[i].Type)
                    {
                        // This is if the texture wants to be rendered to a model or primitive.
                        case PTTexture2D.TextureType.Render:
                            renderShaderResourceViews.Add(textures[i].ShaderResourceView);
                            MaterialBuffer.BlendTexturesCount = renderShaderResourceViews.Count;
                            break;
                        case PTTexture2D.TextureType.Reflective:
                            //reflectionTexture = textures[i];
                            break;
                    }
                }
            }
            #endregion
            // === TEXTURES ===

            // === REFLECTIVITY === Create the objects for the reflectivity render target.
            #region Reflectivity
            obj = null;
            DirtyProperties.TryGetValue(nameof(IsReflectivityEnabled), out obj);
            if (obj != null)
            {
                reflectionCamera.CreateRenderTarget();

                //// Create the reflection Render Target view.
                //Texture2DDescription desc = new Texture2DDescription();
                //desc.Width = Game.Settings.Resolution.Width;
                //desc.Height = Game.Settings.Resolution.Height;
                //desc.MipLevels = 1;
                //desc.ArraySize = 1;
                ////desc.Usage = ResourceUsage.Default;
                //desc.Format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                ////desc.Format = Game.BackBuffer.Description.Format;
                //desc.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
                //desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
                //desc.CpuAccessFlags = CpuAccessFlags.None;
                ////desc.OptionFlags = ResourceOptionFlags.Shared;

                //// Load the texture through the ContentManager so it is disposed correctly.
                //Texture2D texture = new Texture2D(Game.Renderer.Device, desc);
                //reflectionTexture = Game.ContentManager.LoadTexture2D("Reflection Texture", texture);

                //RenderTargetViewDescription viewDesc = new RenderTargetViewDescription();
                //viewDesc.Format = desc.Format;
                //viewDesc.Dimension = RenderTargetViewDimension.Texture2D;
                //viewDesc.Texture2D.MipSlice = 0;
                //reflectionRenderTargetView = new RenderTargetView(Game.Renderer.Device, texture, viewDesc);
                ////reflectionRenderTargetView = new RenderTargetView(Game.Renderer.Device, reflectionTexture.Texture, viewDesc);

                //ShaderResourceViewDescription shaderDesc = new ShaderResourceViewDescription();
                //shaderDesc.Format = desc.Format;
                //shaderDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
                //shaderDesc.Texture2D.MostDetailedMip = 0;
                //shaderDesc.Texture2D.MipLevels = 1;
                //reflectionTexture.ShaderResourceView = new ShaderResourceView(texture.Device, texture, shaderDesc);

                //// Create the Reflection Depth stencil view.
                //var depthTexDesc = new Texture2DDescription()
                //{
                //    Width = Game.Settings.Resolution.Width,
                //    Height = Game.Settings.Resolution.Height,
                //    MipLevels = 1,
                //    ArraySize = 1,
                //    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                //    Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
                //    Usage = ResourceUsage.Default,
                //    BindFlags = BindFlags.DepthStencil,
                //    CpuAccessFlags = CpuAccessFlags.None,
                //    OptionFlags = ResourceOptionFlags.None
                //};
                //var depthTex = new Texture2D(Game.Renderer.Device, depthTexDesc);
                //var depthDesc = new DepthStencilViewDescription()
                //{
                //    Format = depthTexDesc.Format,
                //    //Flags = DepthStencilViewFlags.None,
                //    Dimension = DepthStencilViewDimension.Texture2D
                //};
                //reflectionDepthStencil = new DepthStencilView(Game.Renderer.Device, depthTex, depthDesc);
            }
            #endregion
            // === REFLECTIVITY === 
            
            IsStateUpdated = true;
            OnStateUpdated();

            return IsStateUpdated;
        }

        /// <summary>
        /// Get the first texture that finds by its type.
        /// </summary>
        /// <param name="type">The <see cref="PTTexture2D.TextureType"/> type to search for.</param>
        /// <returns></returns>
        public PTTexture2D TextureByType(PTTexture2D.TextureType type)
        {
            if (type == PTTexture2D.TextureType.Reflective)
                return reflectionCamera.RenderTarget.Texture;

            foreach (PTTexture2D texture in textures)
                if (texture.Type == type)
                    return texture;

            return null;
        }

        /// <summary>
        /// Add a texture path.
        /// This method fire the OnPropertyChanged event.
        /// If the texture paths are added before calling the <see cref="LoadContent(ContentManager11)"/> method,
        /// those textures are loaded there.
        /// If they are set as dirty and added after calling <see cref="LoadContent(ContentManager11)"/>, they will be
        /// loaded in the next <see cref="UpdateState"/> iteration. (Immediatly when they are added).
        /// </summary>
        /// <param name="texture">The texture to be added.</param>
        /// <param name="type">The type of texture to be added.</param>
        public void AddTexture(PTTexture2D texture, PTTexture2D.TextureType type)
        {
            if (!texture.IsContentLoaded)
                texture.LoadContent(Game.ContentManager);

            texture.Type = type;
            //texture.SetTexturePath(null, 0, false);
            //texturePaths.Add(null); // Add a dummy texture path so both lists have the same number of elements
            textures.Add(texture);

            SetPropertyAsDirty(ref textures, new List<PTTexture2D>(textures), nameof(Textures));
        }

        /// <summary>
        /// Removes a texture from the material and optionally dispose it from memory.
        /// </summary>
        /// <param name="path">The <see cref="PTTexturePath"/> of the texture to be removed.</param>
        /// <param name="disposeFromMemory">If the texture will also be dispose from the <see cref="ContentsPool"/>. </param>
        public void RemoveTexture(PTTexturePath path, bool disposeFromMemory = true)
        {
            string pathLower = path.Path.ToLower();
            PTTexture2D texture = textures.FirstOrDefault(t => t.Path.Path.ToLower() == pathLower);
            if (texture != null)
            {
                textures.Remove(texture);
                texturePaths.Remove(texturePaths.FirstOrDefault(p => p.Path.ToLower() == pathLower));
                if (disposeFromMemory)
                    ContentsPool.ClearTexture(path.Path);
            }
        }

        /// <summary>
        /// Removes a texture from the material and optionally dispose it from memory.
        /// </summary>
        /// <param name="texture">The <see cref="PTTexture2D"/> to be removed.</param>
        /// <param name="disposeFromMemory">If the texture will also be dispose from the <see cref="ContentsPool"/>. </param>
        public void RemoveTexture(PTTexture2D texture, bool disposeFromMemory = true)
        {
            if (texture != null)
            {
                textures.Remove(texture);
                texturePaths.Remove(texturePaths.FirstOrDefault(p => p.Path.ToLower() == texture.Path.Path.ToLower()));
                if (disposeFromMemory)
                    ContentsPool.ClearTexture(texture.Path.Path);
            }
        }

        #region Add Texture paths
        /// <summary>
        /// Add a texture path. This path will be loaded the next time the <see cref="UpdateState"/> method is called.
        /// The type of texture matters when loading and on how this texture will be used.
        /// This method fire the OnPropertyChanged event.
        /// </summary>
        /// <param name="texturePath">The texture path to be added.</param>
        /// <param name="asDirty">The value to indicate if the property must be set dirty, so when the <see cref="UpdateState"/>
        /// method is called, the textures are created. If it set to false, the textures must be manually loaded into memory.</param>
        public void AddTexturePath(PTTexturePath texturePath, bool asDirty)
        {
            //// There can only be one bump map or specular map texture.
            //if (texturePath.Type == PTTexture2D.TextureType.BumpMap 
            //    || texturePath.Type == PTTexture2D.TextureType.SpecularMap)
            //{
            //    Log.Warning("There can only be one texture 'map' of each type (Bump, Specular) for a given material. The last texture will be replaced.");
            //}

            // Check if the texture is a bump, specular, or any other type of 'map'
            // For now the engine can only support one texture for each map.
            bool addPath = true;
            int pathIndex = -1;
            if (texturePath.Type != PTTexture2D.TextureType.Render)
            {
                foreach (PTTexturePath path in texturePaths)
                {
                    if (path.Type == texturePath.Type)
                    {
                        Log.Warning("There can only be one texture 'map' of each type (Bump, Specular) for a given material. The last texture will be replaced.");
                        path.Path = texturePath.Path;
                        pathIndex = texturePaths.IndexOf(path);
                        addPath = false;
                        break;
                    }
                }
            }
            if (addPath)
                texturePaths.Add(texturePath);
            else if (pathIndex > -1)
            {
                texturePaths[pathIndex] = texturePath;
                textures[pathIndex].IsContentLoaded = false;
            }
            if (asDirty)
                SetPropertyAsDirty(ref texturePaths, new List<PTTexturePath>(texturePaths), nameof(TexturePaths));
        }

        /// <summary>
        /// Add a texture path. This path will be loaded the next time the <see cref="UpdateState"/> method is called.
        /// The type of texture matters when loading and on how this texture will be used.
        /// This method fire the OnPropertyChanged event.
        /// </summary>
        /// <param name="texturePath">The texture path to be added.</param>
        /// <param name="type">The type of texture.</param>
        /// <param name="asDirty">The value to indicate if the property must be set dirty, so when the <see cref="UpdateState"/>
        /// method is called, the textures are created. If it set to false, the textures must be manually loaded into memory.</param>
        public void AddTexturePath(PTTexturePath texturePath, PTTexture2D.TextureType type, bool asDirty)
        {
            texturePath.Type = type;
            AddTexturePath(texturePath, asDirty);
        }

        /// <summary>
        /// Add a texture path. This path will be loaded the next time the <see cref="UpdateState"/> method is called.
        /// The type of texture matters when loading and on how this texture will be used.
        /// This method fire the OnPropertyChanged event.
        /// </summary>
        /// <param name="texturePath">The texture path to be added.</param>
        /// <param name="index">The index of the textures list where the texture must be set.</param>
        /// <param name="asDirty">The value to indicate if the property must be set dirty, so when the <see cref="UpdateState"/>
        /// method is called, the textures are created. If it set to false, the textures must be manually loaded into memory.</param>
        public void AddTexturePath(PTTexturePath texturePath, bool asDirty, int index = -1)
        {
            if (texturePaths.Count < index)
            {
                Log.Error("The index of the texture path is out of bounds. -{0}-", this.Name);
                return;
            }

            if (index == -1)
            {
                AddTexturePath(texturePath, true);
                return;
            }

            if (texturePaths.Count == index)
            {
                AddTexturePath(texturePath, asDirty);
                return;
            }

            if (texturePaths.Count > index)
            {
                SetTexturePath(texturePath, index);
                return;
            }
        }

        /// <summary>
        /// Add a texture path.
        /// This method fire the OnPropertyChanged event.
        /// </summary>
        /// <param name="texturePath">The texture path to be added.</param>
        /// <param name="index">The index where the texture path will be updated.</param>
        public void SetTexturePath(PTTexturePath texturePath, int index)
        {
            if (texturePaths.Count <= index)
            {
                Log.Error("The index of the texture path is out of bounds. -{0}-", this.Name);
                return;
            }
            texturePaths[index] = texturePath;
            SetPropertyAsDirty(ref texturePaths, new List<PTTexturePath>(texturePaths), nameof(TexturePaths));
        }
        #endregion
        
        /// <summary>
        /// Apply its state to the GPU for the given mesh with a certain effect.
        /// </summary>
        /// <param name="effect">The effect used for this material.</param>
        /// <param name="mesh">The mesh to which the material will be applied.</param>
        /// <param name="context">Context to apply the rendering.</param>
        public void Apply(PTEffect effect, PTMesh mesh, DeviceContext1 context)
        {
            if (HasTexture)
            {
                PTTexture2D texture = TextureByType(PTTexture2D.TextureType.Render);
                context.PixelShader.SetSamplers(0, 1, texture.Sampler);

                // Multi texturing
                context.PixelShader.SetShaderResources(0, MaterialBuffer.BlendTexturesCount, renderShaderResourceViews.ToArray());

                // Send the bump map texture if it has been added.
                if (isBumpEnabled && hasBumpMap)
                {
                    PTTexture2D bumpTexture = TextureByType(PTTexture2D.TextureType.BumpMap);
                    context.PixelShader.SetShaderResource(4, bumpTexture.ShaderResourceView);
                }

                // Send the specular map texture if it has been added.
                if (isSpecularEnabled && hasSpecularMap)
                {
                    PTTexture2D specularTexture = TextureByType(PTTexture2D.TextureType.SpecularMap);
                    context.PixelShader.SetShaderResource(5, specularTexture.ShaderResourceView);
                }
            }

            // Set the matrices constant buffer.
            mesh.Apply(effect, context);

            // Apply reflection buffer.
            if (isReflectivityEnabled)
            {
                ReflectionStruct reflection = ReflectionBuffer;
                Matrix reflectiveMatrix = Game.CurrentCamera.GetReflectionMatrix(-1.5f);
                // Update matrices.
                Matrix reflectionProjection = (reflectiveMatrix * Game.CurrentCamera.Projection);
                reflection.ReflectionMatrixProjectionWorld = mesh.Model.World * reflectionProjection;
                reflection.ReflectionMatrixProjectionWorld.Transpose();
                ReflectionBuffer = reflection;
                var dataBoxReflection = context.MapSubresource(effect.ReflectionConstantBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
                Utilities.Write(dataBoxReflection.DataPointer, ref reflection);
                context.UnmapSubresource(effect.ReflectionConstantBuffer, 0);

                // Send the reflective texture.
                context.PixelShader.SetShaderResource(6, reflectionCamera.RenderTarget.Texture.ShaderResourceView);
            }

            // Send all the material data to the GPU.
            MaterialStruct material = MaterialBuffer;
            //context.UpdateSubresource(ref light, effect.LightningConstantBuffer);
            var dataBoxMaterial = context.MapSubresource(effect.MaterialConstantBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            Utilities.Write(dataBoxMaterial.DataPointer, ref material);
            context.UnmapSubresource(effect.MaterialConstantBuffer, 0);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();

            //Utilities.Dispose(ref Sampler);
            foreach (PTTexture2D tex in textures)
            {
                ContentsPool.ClearTexture(tex.Path.Path.ToLower());
            }

            textures.Clear();
        }
        #endregion

        #region Operators
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (int)(this.Opacity + ambientColor.X + ambientColor.Y + specularPower + emissiveColor.X + emissiveColor.Y + diffuseColor.X + diffuseColor.Y);
        }

        /// <summary>
        /// Operator to compare equality of materials.
        /// </summary>
        /// <param name="a">First material.</param>
        /// <param name="b">Second material.</param>
        /// <returns></returns>
        public static bool operator ==(PTMaterial a, PTMaterial b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null))
                return true;

            if (object.ReferenceEquals(b, null))
                return false;

            bool result;
            result = (a.name == b.name
                && a.texturePaths.Count == b.texturePaths.Count
                && a.ambientColor == b.ambientColor
                && a.diffuseColor == b.diffuseColor
                && a.opacity == b.opacity
                && a.gamma == b.gamma
                && a.specularPower == b.specularPower
                && a.reflectiveColor == b.reflectiveColor
                && a.reflectivity == b.reflectivity
                && a.specularColor == b.specularColor
                && a.textures.Count == b.textures.Count);

            return result;
        }

        /// <summary>
        /// Operator to compare unequality of materials.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(PTMaterial a, PTMaterial b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null))
                return true;

            bool result;
            result = (a.name != b.name
                || a.texturePaths.Count != b.texturePaths.Count
                || a.ambientColor != b.ambientColor
                || a.diffuseColor != b.diffuseColor
                || a.opacity != b.opacity
                || a.gamma != b.gamma
                || a.specularPower != b.specularPower
                || a.reflectiveColor != b.reflectiveColor
                || a.reflectivity != b.reflectivity
                || a.specularColor != b.specularColor
                || a.textures.Count != b.textures.Count);

            return result;
        }
        #endregion
    }
}