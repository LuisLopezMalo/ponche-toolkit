using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Cameras;

namespace PoncheToolkit.Core.Management.Screen
{
    /// <summary>
    /// Class from which every different screens in the game must inherit from.
    /// Ex: LoadingScreen, SplashScreen, GameplayScreen, etc.
    /// </summary>
    public abstract class GameScreen : GameDrawableComponent
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

        #region Fields
        private ScreenState state;
        private GameComponentsCollection components;
        #endregion

        #region Properties
        /// <summary>
        /// Get the current state for the game.
        /// </summary>
        public ScreenState CurrentScreenState
        {
            get { return state; }
            set { state = value; }
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
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// By default it set the screen name to its class name.
        /// </summary>
        public GameScreen(Game11 game)
            : base(game)
        {
            this.Name = GetType().Name;
            components = new GameComponentsCollection();
            CurrentScreenState = ScreenState.Created;

            components.OnComponentAdded += Components_OnComponentAdded;
        }

        /// <summary>
        /// Event when a component has been added.
        /// It check specific functionality if necessary.
        /// </summary>
        /// <param name="component"></param>
        private void Components_OnComponentAdded(IGameComponent component)
        {
            if (component is Camera)
                Game.Cameras.Add(component as Camera);
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void LoadShaders()
        {
        }

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

            CurrentScreenState = ScreenState.Initialized;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void LoadContent()
        {
            // First call the LoadShaders method if the component is drawable. (obligatory call)
            foreach (IGameComponent comp in components.Values)
                if (comp is GameDrawableComponent)
                    ((GameDrawableComponent)comp).LoadShaders();

            // Call the LoadContent method of the components added.
            foreach (IGameComponent comp in components.Values)
                comp.LoadContent();

            OnFinishLoadContent?.Invoke();
        }

        /// <inheritdoc/>
        public override void UnloadContent()
        {
            // Call the UnloadContent method of the components added.
            foreach (IGameComponent comp in components.Values)
                comp.UnloadContent();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            // Update the components
            foreach (IGameComponent comp in components.Values)
                comp.Update();
        }

        /// <inheritdoc/>
        public override void Render()
        {
            // If the component inherits from IDrawable, call the Render method.
            foreach (IGameComponent comp in components.Values)
            {
                if (comp is IDrawable)
                    ((IDrawable)comp).Render();
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            // Dispose the screen components.
            foreach (IGameComponent comp in Components.Values)
            {
                comp.UnloadContent();
                comp.Dispose();
            }
        }
        #endregion
    }
}
