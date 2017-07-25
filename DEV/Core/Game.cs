using System;
using SharpDX.Windows;
using PoncheToolkit.Util;
using System.Collections.Generic;
using PoncheToolkit.Core.Management;
using PoncheToolkit.Core.Management.Screen;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Graphics3D.Cameras;
using System.Collections.ObjectModel;
using PoncheToolkit.Core.Management.Input;
using PoncheToolkit.Core.Management.Sound;
using PoncheToolkit.Core.Management.Content;
using SharpDX.DXGI;
using System.Threading.Tasks;
using PoncheToolkit.Graphics2D.Animation;
using PoncheToolkit.Graphics2D.Effects;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Core;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Main class that wraps the functionality of SharpDX in four methods, called in the next order:
    /// <para/>- Initialize - LoadContent - Update - Draw
    /// <para/>This class can be directly inherited but the specific implementation must be created.
    /// For specific implementations use the <see cref="Game11"/> or <see cref="Game12"/> for DirectX11 and DirectX12 respectively.
    /// </summary>
    public abstract class Game : UpdatableStateObject, IInitializable
    {
        #region Fields
        private string name;
        private string title;
        private bool isPaused;
        private bool isFocused;
        private bool isInitialized;

        private RenderForm form;
        private GameServicesCollection services;

        private ApplicationStateManager appStateManager;
        private GameScreenManager screenManager;
        private InputManager inputManager;
        private Animation2DManager spriteAnimationsManager;
        private SoundManager soundManager;
        private GameTime gameTimeService;
        private GameSettings settings;
        private ObservableCollection<Camera> cameras;
        private Camera currentCamera;
        private DebuggerRenderableService debugger;
        private AdapterDescription1 systemDescription;
        private bool hasFormRendered;

        private PTRenderTarget2D backBufferRenderTarget;
        internal static List<IUpdatableState> updatableStateObjects;
#if DX11
        internal static Game11 instance;
        internal GraphicsRenderer11 renderer;
        internal IContentManager11 contentManager;
#elif DX12
        internal static Game12 instance;
        internal GraphicsRenderer12 renderer;
        internal IContentManager12 contentManager;
#endif
        #endregion

        #region Public Events
        /// <summary>
        /// Event raised when the Initialize method has finished.
        /// </summary>
        public event EventHandlers.OnInitializedHandler OnInitialized;

        /// <summary>
        /// Handler to create custom OnCameraAdded event.
        /// </summary>
        public delegate void OnCameraAddedHandler(object sender);
        /// <summary>
        /// Event raised when a Camera has been added to the game.
        /// The cameras are added as components.
        /// </summary>
        public virtual event OnCameraAddedHandler OnCameraAdded;
        #endregion

        #region Properties
        /// <summary>
        /// Get the game instance.
        /// </summary>
#if DX11
        public static Game11 Instance
        {
            get { return instance; }
        }
#elif DX12
        public static Game12 Instance
        {
            get { return instance; }
        }
#endif

        /// <summary>
        /// Get or set the render target where the back buffer is set to render its contents. As Dirty.
        /// </summary>
        public PTRenderTarget2D BackBufferRenderTarget
        {
            get { return backBufferRenderTarget; }
            internal set { SetPropertyAsDirty(ref backBufferRenderTarget, value); }
        }

        /// <summary>
        /// Get the list of all game services.
        /// </summary>
        public GameServicesCollection Services
        {
            get { return services; }
        }

        /// <summary>
        /// Get the main renderer. This will be updated depending on DirectX11 or DirectX12 are used.
        /// </summary>
#if DX11
        public GraphicsRenderer11 Renderer
        {
            get { return renderer; }
        }
#elif DX12
        public GraphicsRenderer12 Renderer
        {
            get { return renderer; }
        }
#endif

        /// <summary>
        /// Get if the Form has been rendered so its properties are set.
        /// </summary>
        public bool HasFormRendered
        {
            get { return hasFormRendered; }
        }

        /// <summary>
        /// Name of the Game.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        /// <summary>
        /// Title to be shown in the game's title bar.
        /// </summary>
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        /// <summary>
        /// Used to stop updating the main loop. Set as Dirty.
        /// </summary>
        public bool IsPaused
        {
            get { return isPaused; }
            set { SetPropertyAsDirty(ref isPaused, value); }
        }

        /// <summary>
        /// Get or set if the game has finished initialization.
        /// </summary>
        public bool IsInitialized
        {
            get { return isInitialized; }
            set { SetProperty(ref isInitialized, value); }
        }

        /// <summary>
        /// Gets if the current window of the game has focus.
        /// If it has no focus, the game stops updating.
        /// </summary>
        public bool IsFocused
        {
            get { return isFocused; }
            set { isFocused = value; }
        }

        /// <summary>
        /// Gets or Sets if the game enables the sync with the refresh rate of the monitor. (60 Hz = 60 fps)
        /// If true it sets a 1 in the first parameter from the Present method inside Draw.
        /// Else it sets a 0.
        /// This value is saved in the SyncIntervalParameter variable.
        /// </summary>
        public bool VerticalSyncEnabled
        {
            get { return SyncIntervalParameter == 1 ? true : false; }
            set
            {
                if (value)
                    SyncIntervalParameter = 1;
                else
                    SyncIntervalParameter = 0;
            }
        }

        /// <summary>
        /// The value to be used in the Present method from the swapchain object to determine if 
        /// the vertical sync is enabled or not. (0 or 1).
        /// </summary>
        internal int SyncIntervalParameter { get; set; }

        /// <summary>
        /// Get the instance of the RenderForm where all the content is rendered.
        /// </summary>
        public RenderForm Form
        {
            get { return form; }
        }

        /// <summary>
        /// The complete application state manager.
        /// Contain the status of the application in its different stages.
        /// </summary>
        public ApplicationStateManager AppStateManager
        {
            get { return appStateManager; }
        }

        /// <summary>
        /// Keep the state of the screens of all the game.
        /// </summary>
        public GameScreenManager ScreenManager
        {
            get { return screenManager; }
        }

        ///// <summary>
        ///// Get the content manager to load all kind of content into memory.
        ///// </summary>
        //public ContentManager11 ContentManager
        //{
        //    get { return contentManager; }
        //}

        /// <summary>
        /// Get the content manager to load all kind of content into memory.
        /// </summary>
#if DX11
        public IContentManager11 ContentManager
        {
            get { return contentManager; }
        }
#elif DX12
        public IContentManager12 ContentManager
        {
            get { return contentManager; }
        }
#endif

        /// <summary>
        /// Get the <see cref="Animation2DManager"/> manager to load and render 2D sprites from sprite sheets.
        /// </summary>
        public Animation2DManager SpriteAnimationsManager
        {
            get { return spriteAnimationsManager; }
        }

        /// <summary>
        /// Get the input manager that manage all the inputs of the game.
        /// </summary>
        public InputManager InputManager
        {
            get { return inputManager; }
        }

        /// <summary>
        /// Get the sound manager that manage all sounds in the game.
        /// </summary>
        public SoundManager SoundManager
        {
            get { return soundManager; }
        }

        /// <summary>
        /// The GameTime to measure all the in-game and off-game times.
        /// </summary>
        public GameTime GameTime
        {
            get { return gameTimeService; }
        }

        /// <summary>
        /// The game settings to initialize this instance of game.
        /// </summary>
        public GameSettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// The description of the adapter used.
        /// </summary>
        public AdapterDescription1 SystemDescription
        {
            get { return systemDescription; }
            internal set { systemDescription = value; }
        }

        /// <summary>
        /// A list of the cameras used in the game.
        /// </summary>
        public IReadOnlyList<Camera> Cameras
        {
            get { return cameras; }
        }

        /// <summary>
        /// The current camera used.
        /// </summary>
        public Camera CurrentCamera
        {
            get { return currentCamera; }
            set { SetPropertyAsDirty(ref currentCamera, value); }
        }

        /// <summary>
        /// The Content Root directory name to put in all the content files of the game.
        /// </summary>
        public string ContentDirectoryName { get; set; } = "Content";

        /// <summary>
        /// The Content Root directory to put in all the content files of the game.
        /// </summary>
        public string ContentDirectoryFullPath { get; set; }

        /// <summary>
        /// Property to indicate if the Game is gonna be held in an interop Window. (WPF)
        /// </summary>
        internal bool IsInterop { get; set; }

        /// <summary>
        /// The service for debugging. Internal only.
        /// </summary>
        internal DebuggerRenderableService DebuggerService
        {
            get { return debugger; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a game instance with the given settings.
        /// </summary>
        /// <param name="settings">The settings objects to initialize the game. If it is null, it is initialized with default values.</param>
        public Game(GameSettings settings)
        {
            updatableStateObjects = new List<IUpdatableState>();
            this.settings = settings == null ? new GameSettings() : settings;

            Log.Info("\n\n ======= Initializing Game ======= \n\n");

            screenManager = new GameScreenManager(this);
            appStateManager = new ApplicationStateManager(this);
            gameTimeService = new GameTime(this);
            inputManager = new InputManager(this);
#if DX11
            contentManager = contentManager = new ContentManager11(this);
#elif DX12
            contentManager = new ContentManager12(this);
#endif
            spriteAnimationsManager = new Animation2DManager(this);
            soundManager = new SoundManager(this);

            services = new GameServicesCollection();
            cameras = new ObservableCollection<Camera>();
            cameras.CollectionChanged += (sender, e) =>
            {
                if ((e.OldItems == null && e.NewItems.Count > 0) || (e.NewItems.Count > e.OldItems.Count))
                    OnCameraAdded?.Invoke(this);
            };

            this.OnPropertyChangedEvent += (sender, e) =>
            {
            };

            settings.OnPropertyChangedEvent += (sender, e) =>
            {
                if (settings.DirtyProperties.ContainsKey(nameof(Settings.DebugMode)))
                {
                    if (!Settings.DebugMode)
                    {
                        if (Services.ContainsKey(typeof(DebuggerRenderableService)))
                        {
                            this.Services.RemoveService(typeof(DebuggerRenderableService));
                            debugger.Dispose();
                            debugger = null;
                        }
                    }
                    else
                    {
                        if (!Services.ContainsKey(typeof(DebuggerRenderableService)))
                        {
                            debugger = new DebuggerRenderableService(this);
                            debugger.Initialize();
                            this.Services.AddService(debugger);
                        }
                    }
                }
            };

            ContentDirectoryFullPath = System.IO.Path.Combine(Environment.CurrentDirectory, ContentDirectoryName);

            // Set the console size.
            try
            {
                Console.SetWindowSize(140, 40);
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to set custom console size.", ex);
            }

            services.AddService(gameTimeService);
            services.AddService(appStateManager);
            services.AddService(screenManager);
            Services.AddService(inputManager);
            Services.AddService(contentManager);
            Services.AddService(spriteAnimationsManager);
            Services.AddService(soundManager);
        }

        /// <summary>
        /// Main method where the initialization is made.
        /// <para/>Create the swap chain, attach events and start measuring the gameTime.
        /// <para/>Call the <see cref="CreateSwapDescriptionAndDevice"/> method and the <see cref="PostDeviceInitialization"/> that must
        /// be implemented when inherit from this class.
        /// </summary>
        public virtual void Initialize()
        {
            // Set the current game state.
            appStateManager.SetCurrentState(ApplicationStateManager.ApplicationState.Starting);

            Log.Info("Initializing main application window with size: " + settings.Resolution.ToString());
            if (form == null)
                form = new RenderForm(this.name);
            form.ClientSize = settings.Resolution;

            // Initialize each service added.
            Log.Info("Initializing services...");
            foreach (IGameService serv in services.Values)
            {
                if (!serv.IsInitialized)
                    serv.Initialize();
            }

            // Create main device.
            CreateSwapDescriptionAndDevice();

            // Extra initialization after creating the device.
            PostDeviceInitialization();

            // Attach Events.
            form.UserResized += WindowResizedEvent;
            form.LostFocus += (sender, e) => { isFocused = false; };
            form.GotFocus += (sender, e) => { isFocused = true; };

            // Set the size of the window to the inner bounds.
            settings.Resolution = form.ClientSize;

            // Call the window resized event at the beginning to setup the initial properties.
            WindowResizedEvent(null, null);

            // Start measuring the game time.
            gameTimeService.Start();

            // Raise the OnInitialized event.
            Log.Info("Finished Game Initialization");
            isInitialized = true;
            OnInitialized?.Invoke();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Called inside the <see cref="Initialize"/> method to create the swap description and the device.
        /// </summary>
        public abstract void CreateSwapDescriptionAndDevice();
        /// <summary>
        /// Called inside the <see cref="Initialize"/> method to initialize extra elements. It is called after <see cref="CreateSwapDescriptionAndDevice"/>  method.
        /// </summary>
        public abstract void PostDeviceInitialization();
        /// <summary>
        /// Event raised when the user has resized the Client Window.
        /// </summary>
        /// <param name="sender">The RenderForm object where all the application resides.</param>
        /// <param name="e">Event arguments</param>
        protected abstract void WindowResizedEvent(object sender, EventArgs e);
        #endregion

        #region Public Methods
        /// <summary>
        /// Must be overridden to load the assets and all content.
        /// </summary>
        public virtual void LoadContent()
        {
            screenManager.LoadContent(ContentManager);

            // Set the state to started after loading all the needed components.
            appStateManager.SetCurrentState(ApplicationStateManager.ApplicationState.Started);
        }

        /// <summary>
        /// This method releases the content as Textures, Models, etc.
        /// </summary>
        public virtual void UnloadContent()
        {
        }

        /// <summary>
        /// Main method to update the game logic.
        /// This method is called just after updating the ScreenManager.
        /// </summary>
        public virtual void Update()
        {
            // Update all the game services.
            foreach (IGameService serv in Services.Values)
            {
                serv.UpdateLogic(GameTime);
                //Task.Run(() => serv.UpdateLogic());
            }

            // Update the input.
            ScreenManager.UpdateInput(inputManager);

            // Update the screen manager.
            ScreenManager.UpdateLogic(GameTime);

            UpdateState();
        }

        /// <summary>
        /// Main method to put everything that will be rendered.
        /// This method is called inside the <see cref="mainLoop"/> after the <see cref="Update"/> method.
        /// </summary>
        public virtual void Render()
        {
        }

        /// <summary>
        /// Main method that must be called to start the game's loop.
        /// </summary>
        public virtual void Run()
        {
            Initialize();
            LoadContent();

            // Start the main loop.
            if (!IsInterop)
            {
                form.Show();
                hasFormRendered = true;
                inputManager.MousePosition = new SharpDX.Vector2(form.DesktopLocation.X, form.DesktopLocation.X);

                RenderLoop loop = new RenderLoop(form);
                while (loop.NextFrame())
                {
                    mainLoop();
                }
            }
        }

        /// <summary>
        /// Change the Z Buffer status.
        /// </summary>
        /// <param name="enabled"></param>
        public void ToggleZBuffer(bool enabled)
        {
            Settings.DepthBufferEnabled = enabled;
        }

        /// <summary>
        /// Change the Alpha blending status.
        /// </summary>
        /// <param name="blendState">The <see cref="BlendingState"/> state to set into the GPU.</param>
        public void ToggleBlending(BlendingState blendState)
        {
            Settings.BlendState = blendState;
        }

        /// <summary>
        /// Method to add a camera and optionally set it as the active camera.
        /// </summary>
        /// <param name="camera">The camera to be added.</param>
        /// <param name="setActive">Set it to be the active camera.</param>
        public virtual void AddCamera(Camera camera, bool setActive = true)
        {
            if (!cameras.Contains(camera))
                cameras.Add(camera);
            if (setActive)
                CurrentCamera = camera;
        }

        /// <summary>
        /// Get the first instance for the type of camera searched.
        /// </summary>
        /// <param name="type"></param>
        public Camera GetFirstCamera(CameraType type)
        {
            foreach (Camera cam in cameras)
                if (cam.Type == type)
                    return cam;

            return null;
        }

        /// <summary>
        /// Dispose all the managed and unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            updatableStateObjects.Clear();
            updatableStateObjects = null;
        }

        /// <summary>
        /// Dispose the resources and shutdown the game.
        /// </summary>
        public void Shutdown()
        {
            try
            {
                if (System.Windows.Forms.Application.MessageLoop)
                    // WinForms app
                    System.Windows.Forms.Application.Exit();
                else
                    // Console app
                    System.Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Log.Error("Error shutting down.", ex);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Private method that update the game services and call the ScreenManager Update method. 
        /// It is called inside the MainLoop.
        /// </summary>
        private void update()
        {
            // Call the public and overridable method.
            Update();
        }

        /// <summary>
        /// Private method that just call the ScreenManager Render method.
        /// </summary>
        private void render()
        {
            // Call the public and overridable method.
            Render();
        }

        /// <summary>
        /// The main loop called every frame where Update and Render methods are executed.
        /// </summary>
        private void mainLoop()
        {
            //If window is out of focus, the timer pauses and no more updates nor draws are made.
            if (!isFocused)
            {
                if (appStateManager.CurrentState == ApplicationStateManager.ApplicationState.Running)
                {
                    appStateManager.SetCurrentState(ApplicationStateManager.ApplicationState.Paused);
                    gameTimeService.Pause();
                }
                return;
            }

            // Check if the game is running.
            if (appStateManager.CurrentState != ApplicationStateManager.ApplicationState.Running)
            {
                appStateManager.SetCurrentState(ApplicationStateManager.ApplicationState.Running);
                gameTimeService.Start();
            }

            update();
            render();
        }
        #endregion
    }
}
