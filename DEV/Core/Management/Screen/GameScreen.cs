using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Cameras;
using PoncheToolkit.Util;
using PoncheToolkit.Graphics3D.Effects;
using System.ComponentModel;
using System.Threading;
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Core.Management.Input;
using PoncheToolkit.Core.Management.Content;
using SharpDX.Direct3D11;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Core.Management.Screen
{
    /// <summary>
    /// Class from which every different screens in the game must inherit from.
    /// Ex: LoadingScreen, SplashScreen, GameplayScreen, etc.
    /// </summary>
    public abstract class GameScreen : GameRenderableComponent, IShaderLoadable, IInputReceivable
    {
        /// <summary>
        /// Represent the state of the screen.
        /// </summary>
        public enum ScreenState
        {
            /// <summary>
            /// Created and instance.
            /// </summary>
            Created = 0,
            /// <summary>
            /// The Initialize method was called.
            /// </summary>
            Initialized,
            /// <summary>
            /// When the screen is transitioning in.
            /// </summary>
            TransitioningIn,
            /// <summary>
            /// When the screen is transitioning in.
            /// </summary>
            TransitioningOut,
            /// <summary>
            /// When the screen is active (updating and rendering).
            /// </summary>
            Active,
            /// <summary>
            /// When the screen is paused (NOT updating nor rendering).
            /// </summary>
            Paused
        }

        /// <summary>
        /// Represent the way the screen is updated.
        /// </summary>
        public enum ScreenUpdateMode
        {
            /// <summary>
            /// The screen is updated always, even if some other screen is in top of it.
            /// </summary>
            Always,
            /// <summary>
            /// The screen is updated only when it is active.
            /// The active state will be managed in the specific screen as you see fit.
            /// </summary>
            OnlyWhenActive,
            /// <summary>
            /// The screen is never updated.
            /// </summary>
            Never,
        }

        /// <summary>
        /// Represent the way the screen is rendered.
        /// </summary>
        public enum ScreenRenderMode
        {
            /// <summary>
            /// The screen is rendered always, even if some other screen is in top of it.
            /// </summary>
            Always,
            /// <summary>
            /// The screen is rendered only when it is active.
            /// The active state will be managed in the specific screen as you see fit.
            /// </summary>
            OnlyWhenActive,
            /// <summary>
            /// The screen is never rendered.
            /// </summary>
            Never,
        }

        /// <summary>
        /// Represent the way the screen is rendered.
        /// </summary>
        public enum ScreenRenderTargetType
        {
            /// <summary>
            /// Render the screen to the backbuffer.
            /// </summary>
            Backbuffer,
            /// <summary>
            /// Render the screen to a texture.
            /// </summary>
            Texture
        }

        #region Fields
        private ScreenState state;
        private GameComponentsCollection components;
        private Dictionary<PTEffect, List<PTMesh>> meshesPerEffect;
        private Dictionary<PTEffect, Dictionary<int, List<PTMesh>>> instancedMeshesPerEffect;
        private Dictionary<PTMaterial, List<PTMesh>> meshesPerMaterial;
        private Dictionary<PTEffect, GameComponentsCollection> drawableComponentsPerEffect;
        private Dictionary<PTEffect, Dictionary<int, IGameComponent>> drawableInstancedComponentsPerEffect;
        private Dictionary<int, SharpDX.Direct3D11.Buffer> instancedBuffers;

        private TimeSpan transitionIn;
        private TimeSpan transitionOut;

        private static object lockObj = new object();
        #endregion

        #region Properties
        /// <summary>
        /// Dictionary containing the meshes per effect.
        /// </summary>
        public Dictionary<PTEffect, List<PTMesh>> MeshesPerEffect
        {
            get { return meshesPerEffect; }
            internal set { SetProperty(ref meshesPerEffect, value); }
        }

        /// <summary>
        /// Dictionary containing the meshes per effect by instance.
        /// </summary>
        public Dictionary<PTEffect, Dictionary<int, List<PTMesh>>> InstancedMeshesPerEffect
        {
            get { return instancedMeshesPerEffect; }
            internal set { SetProperty(ref instancedMeshesPerEffect, value); }
        }

        /// <summary>
        /// Dictionary containing the meshes per material in this screen.
        /// </summary>
        public Dictionary<PTMaterial, List<PTMesh>> MeshesPerMaterial
        {
            get { return meshesPerMaterial; }
            internal set { SetProperty(ref meshesPerMaterial, value); }
        }

        /// <summary>
        /// Dictionary containing the components per effect in this screen.
        /// </summary>
        public Dictionary<PTEffect, GameComponentsCollection> DrawableComponentsPerEffect
        {
            get { return drawableComponentsPerEffect; }
            internal set { SetProperty(ref drawableComponentsPerEffect, value); }
        }

        /// <summary>
        /// Dictionary containing the components per effect in this screen.
        /// </summary>
        public Dictionary<PTEffect, Dictionary<int, IGameComponent>> DrawableInstanceComponentsPerEffect
        {
            get { return drawableInstancedComponentsPerEffect; }
            internal set { SetProperty(ref drawableInstancedComponentsPerEffect, value); }
        }

        /// <summary>
        /// Get or set the current state for the game screen.
        /// </summary>
        public ScreenState CurrentScreenState
        {
            get { return state; }
            set { state = value; }
        }

        /// <summary>
        /// Get or set the time for the screen to transition in.
        /// </summary>
        public TimeSpan TransitionIn
        {
            get { return transitionIn; }
            set { transitionIn = value; }
        }

        /// <summary>
        /// Get or set the time for the screen to transition in.
        /// </summary>
        public TimeSpan TransitionOut
        {
            get { return transitionOut; }
            set { transitionOut = value; }
        }

        /// <inheritdoc/>
        public new bool IsInitialized
        {
            get { return this.state >= ScreenState.Initialized; }
        }

        /// <summary>
        /// Get if the current state of the screen is active.
        /// This value is arbitrary for every implemented screen.
        /// </summary>
        public bool IsActive
        {
            get { return this.state == ScreenState.Active; }
        }

        /// <summary>
        /// Get Collection (Dictionary) of the components in the screen.
        /// </summary>
        public GameComponentsCollection Components
        {
            get { return components; }
        }

        /// <summary>
        /// Set when the screen will be updated.
        /// <para/>Default: ScreenUpdateMode.Always
        /// </summary>
        public ScreenUpdateMode UpdateMode { get; set; } = ScreenUpdateMode.Always;

        /// <summary>
        /// Set when the screen will be rendered.
        /// <para/> Default: ScreenRenderMode.Always
        /// </summary>
        public ScreenRenderMode RenderMode { get; set; } = ScreenRenderMode.Always;

        /// <summary>
        /// Get or set the render target of the screen.
        /// <para/> Default: ScreenRenderTarget.Backbuffer
        /// </summary>
        public ScreenRenderTargetType RenderTargetType { get; set; } = ScreenRenderTargetType.Backbuffer;

        /// <summary>
        /// Get or set the render target of the screen.
        /// <para/> Default: ScreenRenderTarget.Backbuffer
        /// </summary>
        public RenderTargetView RenderTargetView { get; set; }
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;

        /// <summary>
        /// Event raised when a new effect is added when using the <see cref="GameScreen.AddRenderableComponentWithEffect{T, E}(ref T, E, string)"/> method.
        /// </summary>
        public event EventHandlers.OnNewEffectAddedHandler OnNewEffectAdded;

        /// <summary>
        /// Event that must be raised last when implementing the <see cref="GameScreen.AddRenderableComponentWithEffect{T, E}(ref T, E, string)"/>
        /// method.
        /// </summary>
        public abstract event EventHandlers.OnFinishLoadRenderableComponentsHandler OnFinishLoadRenderableComponents;

        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// By default it set the screen name to its class name.
        /// </summary>
#if DX11
        public GameScreen(Game11 game)
#elif DX12
        public GameScreen(Game12 game)
#endif
            : base(game)
        {
            this.Name = GetType().Name;
            components = new GameComponentsCollection();
            MeshesPerMaterial = new Dictionary<PTMaterial, List<PTMesh>>();
            meshesPerEffect = new Dictionary<PTEffect, List<PTMesh>>();
            drawableComponentsPerEffect = new Dictionary<PTEffect, GameComponentsCollection>();
            instancedMeshesPerEffect = new Dictionary<PTEffect, Dictionary<int, List<PTMesh>>>();
            drawableInstancedComponentsPerEffect = new Dictionary<PTEffect, Dictionary<int, IGameComponent>>();

            CurrentScreenState = ScreenState.Created;
            components.OnComponentAdded += Components_OnComponentAdded;
        }

        /// <summary>
        /// Event when a component has been added.
        /// If the component has not been initialized, it is initialized here.
        /// It check specific functionality if necessary.
        /// <para>If the component is a Camera it is added to the <see cref="Game.Cameras"/> list.
        /// The last added camera is set as the current camera. </para>
        /// </summary>
        /// <param name="component"></param>
        private void Components_OnComponentAdded(IGameComponent component)
        {
            if (!component.IsInitialized)
            {
                component.Initialize();
                //if (component is GameDrawableComponent)
                //    (component as GameDrawableComponent).LoadShaders();
                //component.LoadContent();
            }

            if (component is Camera)
                Game.AddCamera(component as Camera, true);
                
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Here add all the components, this is called after the <see cref="GameScreen.LoadShadersAndMaterials"/> methods.
        /// </summary>
        public abstract void AddRenderableScreenComponents();

        /// <inheritdoc/>
        public abstract void LoadShadersAndMaterials(IContentManager contentManager);

        /// <inheritdoc/>
        public override void Initialize()
        {
            // Initialize each component added.
            Log.Info("Initializing components for Screen -{0}- ", this.Name);
            foreach (IGameComponent comp in components.Values)
            {
                if (!comp.IsInitialized)
                    comp.Initialize();
            }

            this.OnFinishLoadContent += GameScreen_OnFinishLoadContent;
            this.OnNewEffectAdded += GameScreen_OnNewEffectAdded;

            CurrentScreenState = ScreenState.Initialized;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void LoadContent(IContentManager contentManager)
        {
            // First call the LoadShaders method if the component is renderable. (obligatory call)
            LoadShadersAndMaterials(contentManager);

            // Call the LoadContent method of the components added.
            foreach (IGameComponent comp in components.Values)
                comp.LoadContent(contentManager);

            // Add the necessary renderable components.
            AddRenderableScreenComponents();

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        #region Events implementation
        private void GameScreen_OnFinishLoadContent()
        {
            instancedBuffers = new Dictionary<int, SharpDX.Direct3D11.Buffer>();

            // Create the instance buffers per component.
            foreach (Dictionary<int, IGameComponent> dict in drawableInstancedComponentsPerEffect.Values)
            {

            }
        }

        private void GameScreen_OnNewEffectAdded(PTEffect effect)
        {
            effect.IsUsed = true;
        }
        #endregion

        /// <inheritdoc/>
        public override void UnloadContent()
        {
            // Call the UnloadContent method of the components added.
            foreach (IGameComponent comp in components.Values)
                comp.UnloadContent();

            //foreach (GameComponentsCollection collection in drawableComponentsPerEffect.Values)
            //{
            //    for (int i = 0; i < collection.Count; i++)
            //        collection[i].UnloadContent();
            //}

            foreach (Dictionary<int, IGameComponent> dict in drawableInstancedComponentsPerEffect.Values)
            {
                foreach (GameComponentsCollection collection in dict.Values)
                {
                    for (int i = 0; i < collection.Count; i++)
                        collection[i].UnloadContent();
                }
            }
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {

            // Update the components
            foreach (IGameComponent comp in components.Values)
            {
                comp.UpdateLogic(gameTime);
                //Task.Factory.StartNew(() => comp.UpdateLogic());
            }

            //foreach (GameComponentsCollection collection in drawableComponentsPerEffect.Values)
            //{
            //    for (int i = 0; i < collection.Count; i++)
            //    {
            //        IGameComponent comp = collection[i];
            //        comp.UpdateLogic();
            //    }
            //}

            foreach (Dictionary<int, IGameComponent> dict in drawableInstancedComponentsPerEffect.Values)
            {
                foreach (GameComponentsCollection collection in dict.Values)
                {
                    for (int i = 0; i < collection.Count; i++)
                    {
                        IGameComponent comp = collection[i];
                        comp.UpdateLogic(gameTime);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void UpdateInput(InputManager inputManager)
        {
            // Call the UpdateInput method of the components added.
            foreach (IGameComponent comp in components.Values)
                comp.UpdateInput(inputManager);

            //foreach (GameComponentsCollection collection in drawableComponentsPerEffect.Values)
            //{
            //    for (int i = 0; i < collection.Count; i++)
            //    {
            //        IGameComponent comp = collection[i];
            //        comp.UpdateInput(inputManager);
            //    }
            //}

            foreach (Dictionary<int, IGameComponent> dict in drawableInstancedComponentsPerEffect.Values)
            {
                foreach (GameComponentsCollection collection in dict.Values)
                {
                    for (int i = 0; i < collection.Count; i++)
                    {
                        IGameComponent comp = collection[i];
                        comp.UpdateInput(inputManager);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            //// Update the components
            //foreach (IGameComponent comp in components.Values)
            //{
            //    if (comp is IRenderable)
            //        ((IRenderable)comp).Render();
            //}

            //// If the component inherits from IRenderable, call the Render method.
            //foreach (GameComponentsCollection coll in drawableComponentsPerEffect.Values)
            //{
            //    for (int i = 0; i < coll.Count; i++)
            //    {
            //        IGameComponent comp = coll[i];
            //        if (comp is IRenderable)
            //            ((IRenderable)comp).Render();
            //    }
            //}
        }

        /// <summary>
        /// Add a component ordered by its effect. This is to create a more efficient rendering.
        /// If the Effect has not been loaded, it cannot be added.
        /// First load the Effect into memory.
        /// It also check if the component is a <see cref="PTMesh"/> so it is added to the meshes sorted dictionary.
        /// <para>If the component has not been initialized, here the <see cref="GameComponent.Initialize"/> and 
        /// <see cref="GameComponent.LoadContent(ContentManager11)"/> methods are called.
        /// </para>
        /// </summary>
        /// <param name="component">The <see cref="GameRenderableComponent"/> component to be rendered.</param>
        /// <param name="effects">The effect to be assigned to the component when rendering.</param>
        /// <param name="componentName">The name of the component to be added.</param>
        //public void AddRenderableComponentWithEffect<T, Eff>(ref T component, ref Eff effect, string componentName)
        public void AddRenderableComponentWithEffect<T, E>(ref T component, E effects, string componentName)
            where T : GameRenderableComponent
            where E : List<PTEffect>
        {
            //AddRenderableInstancedComponentWithEffect(ref component, ref effect, componentName, -1);

            //    if (effect == null || !effect.IsContentLoaded)
            //        throw new Util.Exceptions.ComponentNotInitializedException("The effect has not been loaded. The Screen effect loading must be done within the LoadShadersAndMaterials() overridable method.");

            //    if (effect.Materials.Count <= 0)
            //        throw new Util.Exceptions.LoadContentException("The effect -{0}- for the component -{1}- has no materials assigned.", System.IO.Path.GetFileNameWithoutExtension(effect.ShaderPath), component.Name);

            //component.Effects = effects;
            //GameComponentsCollection componentsCollection = null;
            //// Check if this effect has components added.
            //if (!drawableComponentsPerEffect.ContainsKey(component.Effects as PTForwardRenderEffect))
            //{
            //    componentsCollection = new GameComponentsCollection();
            //    componentsCollection.OnComponentAdded += Collection_OnDrawableComponentAdded;
            //    drawableComponentsPerEffect.Add(effects, componentsCollection);
            //}
            //else
            //    componentsCollection = drawableComponentsPerEffect[effects];

            //componentsCollection.AddComponent(component, componentName);

            //// Initialize and LoadContent the component.
            //if (!component.IsInitialized)
            //    component.Initialize();
            //if (!component.IsContentLoaded)
            //    component.LoadContent(Game.ContentManager);

            //// If the component is a model, add its meshes to the dictionary.
            //if (component is PTModel)
            //{
            //    PTModel model = component as PTModel;

            //    // Add the meshes.
            //    for (int i = 0; i < model.Meshes.Count; i++)
            //    {
            //        PTMesh mesh = model.Meshes[i];
            //        if (!mesh.IsInitialized)
            //            mesh.Initialize();
            //        if (!mesh.IsContentLoaded)
            //            mesh.LoadContent(Game.ContentManager);

            //        if (mesh.Effects == null)
            //            mesh.Effects = model.Effects;
            //        if (string.IsNullOrEmpty(mesh.Name))
            //            mesh.Name = model.Name + "_comp" + i;

            //        // Add the imported materials (from assimp) to the effect materials.
            //        List<PTMesh> meshes = null;
            //        for (int j = 0; j < model.ImportedMaterials.Count; j++)
            //        {
            //            PTMaterial mat = effects.GetMaterial(model.ImportedMaterials[j].Name);
            //            if (mat == null)
            //            {
            //                Log.Debug("Adding material -{0}- to effect: -{1}-", model.ImportedMaterials[j].Name, effects.Name);
            //                effects.AddMaterial(model.ImportedMaterials[j].Name, model.ImportedMaterials[j]);
            //            }
            //        }

            //        //model.ImportedMaterials.Clear();

            //        // Check if this effect has components added.
            //        if (!meshesPerEffect.ContainsKey(mesh.Effects as PTForwardRenderEffect))
            //        {
            //            meshes = new List<PTMesh>();
            //            meshesPerEffect.Add(mesh.Effects as PTForwardRenderEffect, meshes);
            //        }
            //        else
            //            meshes = meshesPerEffect[mesh.Effects as PTForwardRenderEffect];

            //        meshes.Add(mesh);
            //    }
            //}

            component.Effects = effects;
            GameComponentsCollection componentsCollection = null;

            foreach (PTEffect eff in effects)
            {
                if (eff == null || !eff.IsContentLoaded)
                    throw new Util.Exceptions.ComponentNotInitializedException("The effect has not been loaded. The Screen effect loading must be done within the LoadShadersAndMaterials() overridable method.");

                if (eff.Materials.Count <= 0)
                    throw new Util.Exceptions.LoadContentException("The effect -{0}- for the component -{1}- has no materials assigned.", System.IO.Path.GetFileNameWithoutExtension(eff.ShaderPath), component.Name);

                // Check if this effect has components added.
                if (!drawableComponentsPerEffect.ContainsKey(eff))
                {
                    componentsCollection = new GameComponentsCollection();
                    componentsCollection.OnComponentAdded += Collection_OnDrawableComponentAdded;
                    drawableComponentsPerEffect.Add(eff, componentsCollection);
                }
                else
                    componentsCollection = drawableComponentsPerEffect[eff];
            }
            
            componentsCollection.AddComponent(component, componentName);

            // Initialize and LoadContent the component.
            if (!component.IsInitialized)
                component.Initialize();
            if (!component.IsContentLoaded)
                component.LoadContent(Game.ContentManager);

            // If the component is a model, add its meshes to the dictionary.
            if (component is PTModel)
            {
                PTModel model = component as PTModel;

                // Add the meshes.
                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    PTMesh mesh = model.Meshes[i];
                    if (!mesh.IsInitialized)
                        mesh.Initialize();
                    if (!mesh.IsContentLoaded)
                        mesh.LoadContent(Game.ContentManager);

                    if (mesh.Effects == null || mesh.Effects.Count == 0)
                        mesh.Effects = model.Effects;
                    if (string.IsNullOrEmpty(mesh.Name))
                        mesh.Name = model.Name + "_comp" + i;

                    // Add the imported materials (from assimp) to the effect materials.
                    foreach (PTEffect eff in mesh.Effects)
                    {
                        List<PTMesh> meshes = null;
                        for (int j = 0; j < model.ImportedMaterials.Count; j++)
                        {
                            PTMaterial mat = eff.GetMaterial(model.ImportedMaterials[j].Name);
                            if (mat == null)
                            {
                                Log.Debug("Adding material -{0}- to effect: -{1}-", model.ImportedMaterials[j].Name, eff.Name);
                                eff.AddMaterial(model.ImportedMaterials[j].Name, model.ImportedMaterials[j]);
                            }
                        }

                        // Check if this effect has components added.
                        if (!meshesPerEffect.ContainsKey(eff))
                        {
                            meshes = new List<PTMesh>();
                            meshesPerEffect.Add(eff, meshes);
                            OnNewEffectAdded?.Invoke(eff);
                        }
                        else
                            meshes = meshesPerEffect[eff];

                        meshes.Add(mesh);
                    }

                    //model.ImportedMaterials.Clear();
                    //meshes.Add(mesh);
                }
            }
        }

        /*
        /// <summary>
        /// Add a component ordered by its effect. This is to create a more efficient rendering.
        /// If the Effect has not been loaded, it cannot be added.
        /// First load the Effect into memory.
        /// It also check if the component is a <see cref="PTMesh"/> so it is added to the meshes sorted dictionary.
        /// <para>If the component has not been initialized, here the <see cref="GameComponent.Initialize"/> and 
        /// <see cref="GameComponent.LoadContent(ContentManager)"/> methods are called.
        /// </para>
        /// </summary>
        /// <param name="component">The <see cref="GameRenderableComponent"/> component to be rendered.</param>
        /// <param name="effect">The effect to be assigned to the component when rendering.</param>
        /// <param name="componentName">The name of the component to be added.</param>
        /// <param name="instanceIndex">The index to group the instance into. This index will work to distinguish between </param>
        public void AddRenderableInstancedComponentWithEffect<T, Eff>(ref T component, ref Eff effect, string componentName, int instanceIndex)
            where T : GameRenderableComponent
            where Eff : PTForwardRenderEffect
        {
            if (effect == null || !effect.IsContentLoaded)
                throw new Util.Exceptions.ComponentNotInitializedException("The effect has not been loaded. The Screen effect loading must be done within the LoadShadersAndMaterials() overridable method.");

            if (effect.Materials.Count <= 0)
                throw new Util.Exceptions.LoadContentException("The effect -{0}- for the component -{1}- has no materials assigned.", System.IO.Path.GetFileNameWithoutExtension(effect.ShaderPath), component.Name);

            bool instanceIndexUsed = false;
            component.Effects = effect;
            Dictionary<int, IGameComponent> instances = null;
            //GameComponentsCollection componentsCollection = null;

            // Check if this effect has components added.
            if (!drawableInstancedComponentsPerEffect.ContainsKey(component.Effects))
            {
                instances = new Dictionary<int, IGameComponent>();
                drawableInstancedComponentsPerEffect.Add(effect, instances);
            }
            else
                instances = drawableInstancedComponentsPerEffect[effect];

            //// Check if the Components collection exists.
            //if (instances.ContainsKey(instanceIndex))
            //{
            //    componentsCollection = instances[instanceIndex];
            //    instanceIndexUsed = true;
            //}
            //else
            //{
            //    componentsCollection = new GameComponentsCollection();
            //    instances.Add(instanceIndex, componentsCollection);
            //    componentsCollection.OnComponentAdded += Collection_OnDrawableComponentAdded;
            //}

            if (!instances.ContainsKey(instanceIndex))
                instances.Add(instanceIndex, component);
            else
                instanceIndexUsed = true;

            // If it is a new index, add the component.
            // There will only be one component per index. But there will be as many meshes as added, so they preserve
            // their material properties.
            //if (!instanceIndexUsed)
            //    componentsCollection.AddComponent(component, componentName);

            // Initialize and LoadContent the component.
            if (!component.IsInitialized)
                component.Initialize();
            if (!component.IsContentLoaded)
                component.LoadContent(Game.ContentManager);


            // If the component is a model, add its meshes to the dictionary if its vertices have not been added.
            if (component is PTModel)
            {
                PTModel model = component as PTModel;

                // Add the meshes.
                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    PTMesh mesh = model.Meshes[i];
                    if (!mesh.IsInitialized)
                        mesh.Initialize();
                    if (!mesh.IsContentLoaded)
                        mesh.LoadContent(Game.ContentManager);

                    if (mesh.Effects == null)
                        mesh.Effects = model.Effects;
                    if (string.IsNullOrEmpty(mesh.Name))
                        mesh.Name = model.Name + "_comp" + i;

                    // Add the imported materials (from assimp) to the effect materials.
                    for (int j = 0; j < model.ImportedMaterials.Count; j++)
                    {
                        PTMaterial mat = effect.GetMaterial(model.ImportedMaterials[j].Name);
                        if (mat == null)
                        {
                            Log.Debug("Adding material -{0}- to effect: -{1}-", model.ImportedMaterials[j].Name, effect.Name);
                            effect.AddMaterial(model.ImportedMaterials[j].Name, model.ImportedMaterials[j]);
                        }
                    }

                    //model.ImportedMaterials.Clear();

                    List<PTMesh> meshes = null;
                    Dictionary<int, List<PTMesh>> dict = null;
                    // Check if this effect has components added.
                    if (!instancedMeshesPerEffect.ContainsKey(mesh.Effects))
                    {
                        meshes = new List<PTMesh>();
                        dict = new Dictionary<int, List<PTMesh>>();
                        dict.Add(instanceIndex, meshes);
                        instancedMeshesPerEffect.Add(mesh.Effects, dict);
                    }
                    else
                    {
                        dict = instancedMeshesPerEffect[mesh.Effects];
                        if (!dict.ContainsKey(instanceIndex))
                        {
                            meshes = new List<PTMesh>();
                            instancedMeshesPerEffect.Add(mesh.Effects, dict);
                        }
                        else
                            meshes = dict[instanceIndex];
                    }

                    meshes.Add(mesh);
                }
            }
        }
        */

        ///// <summary>
        ///// Add a component ordered by its effect. This is to create a more efficient rendering.
        ///// If the Effect has not been loaded, it cannot be added.
        ///// First load the Effect into memory.
        ///// It also check if the component is a <see cref="PTMesh"/> so it is added to the meshes sorted dictionary.
        ///// <para>If the component has not been initialized, here the <see cref="GameComponent.Initialize"/> and 
        ///// <see cref="GameComponent.LoadContent(ContentManager)"/> methods are called.
        ///// </para>
        ///// <para>Thread safe on collections.</para>
        ///// </summary>
        ///// <param name="component">The <see cref="GameRenderableComponent"/> component to be rendered.</param>
        ///// <param name="effect">The effect to be assigned to the component when rendering.</param>
        ///// <param name="componentName">The name of the component to be added.</param>
        //public async Task AddRenderableComponentWithEffectAsync<T, Eff>(T component, Eff effect, string componentName)
        //    where T : GameRenderableComponent 
        //    where Eff : PTEffect
        //{
        //    Action action = new Action(() =>
        //    {
        //        if (effect == null || !effect.IsContentLoaded)
        //            throw new Util.Exceptions.ComponentNotInitializedException("The effect has not been loaded. The Screen effect loading must be done within the LoadShadersAndMaterials() overridable method.");

        //        if (effect.MaterialCount <= 0)
        //            throw new Util.Exceptions.LoadContentException("The effect -{0}- for the component -{1}- has no materials assigned.", System.IO.Path.GetFileNameWithoutExtension(effect.ShaderPath), component.Name);

        //        component.Effect = effect;
        //        GameComponentsCollection componentsCollection = null;

        //        lock (lockObj)
        //        {
        //            // Check if this effect has components added.
        //            if (!drawableComponentsPerEffect.ContainsKey(component.Effect))
        //            {
        //                componentsCollection = new GameComponentsCollection();
        //                componentsCollection.OnComponentAdded += Collection_OnDrawableComponentAdded;
        //                drawableComponentsPerEffect.Add(effect, componentsCollection);
        //            }
        //            else
        //                componentsCollection = drawableComponentsPerEffect[effect];

        //            if (!componentsCollection.ContainsKey(componentName))
        //                componentsCollection.AddComponent(component, componentName);
        //        }

        //        // Initialize and LoadContent the component.
        //        if (!component.IsInitialized)
        //        {
        //            component.Initialize();
        //            component.LoadContent(Game.ContentManager);
        //        }

        //        // If the component is a model, add its meshes to the dictionary.
        //        if (component is PTModel)
        //        {
        //            PTModel model = component as PTModel;

        //            // Add the meshes.
        //            for (int i = 0; i < model.Meshes.Count; i++)
        //            {
        //                PTMesh mesh = model.Meshes[i];
        //                if (mesh.Effect == null)
        //                    mesh.Effect = model.Effect;
        //                if (string.IsNullOrEmpty(mesh.Name))
        //                    mesh.Name = model.Name + "_comp" + i;

        //                // Add the imported materials (from assimp) to the effect materials.
        //                List<PTMesh> meshes = null;
        //                for (int j = 0; j < model.ImportedMaterials.Count; j++)
        //                {
        //                    lock (lockObj)
        //                    {
        //                        PTMaterial mat = effect.GetMaterial(model.ImportedMaterials[j].Name);
        //                        if (mat == null)
        //                        {
        //                            Log.Debug("Adding material -{0}- to effect: -{1}-", model.ImportedMaterials[j].Name, effect.Name);
        //                            effect.AddMaterial(model.ImportedMaterials[j].Name, model.ImportedMaterials[j]);
        //                        }
        //                    }
        //                }

        //                model.ImportedMaterials.Clear();

        //                lock (lockObj)
        //                {
        //                    // Check if this effect has components added.
        //                    if (!meshesPerEffect.ContainsKey(mesh.Effect))
        //                    {
        //                        meshes = new List<PTMesh>();
        //                        meshesPerEffect.Add(mesh.Effect, meshes);
        //                    }
        //                    else
        //                        meshes = meshesPerEffect[mesh.Effect];

        //                    meshes.Add(mesh);
        //                }
        //            }
        //        }

        //        // TODO: tests!
        //        //Thread.Sleep(1000);
        //    });

        //    await Task.Factory.StartNew(action);
        //}

        /// <inheritdoc/>
        public override void Dispose()
        {
            // Dispose the screen components.
            foreach (IGameComponent comp in Components.Values)
            {
                comp.UnloadContent();
                comp.Dispose();
            }

            foreach (GameComponentsCollection collection in drawableComponentsPerEffect.Values)
                collection.Dispose();

            foreach (PTEffect eff in drawableComponentsPerEffect.Keys)
                eff.Dispose();
            

            // Dispose instanced elements.
            foreach (Dictionary<int, IGameComponent> dict in drawableInstancedComponentsPerEffect.Values)
            {
                foreach (GameComponentsCollection collection in dict.Values)
                {
                    collection.Dispose();
                }
            }

            drawableComponentsPerEffect.Clear();
            drawableComponentsPerEffect = null;
            drawableInstancedComponentsPerEffect.Clear();
            drawableInstancedComponentsPerEffect = null;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initialize the component when added if it has not been initialized.
        /// </summary>
        /// <param name="component"></param>
        private void Collection_OnDrawableComponentAdded(IGameComponent component)
        {
            //if (!component.IsInitialized)
            //{
            //    component.Initialize();
            //    component.LoadContent(Game.ContentManager);
            //}
        }
        #endregion
    }
}
