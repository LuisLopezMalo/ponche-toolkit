using System;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using PoncheToolkit.Util;
using PoncheToolkit.Core.Components;
using System.Collections.Generic;
using PoncheToolkit.Core.Management;
using PoncheToolkit.Core.Management.Screen;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Cameras;
using PoncheToolkit.Core.Management.Content;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Main class that wraps the functionality of SharpDX in four methods, called in the next order:
    /// <para/>- Initialize - LoadContent - Update - Draw
    /// <para/>This class can be directly inherited but the specific implementation must be created.
    /// For specific implementations use the <see cref="Game11"/> or <see cref="Game12"/> for DirectX11 and DirectX12 respectively.
    /// </summary>
    public abstract class Game : IDisposable, ILoggable
    {
        #region Fields
        private string name;
        private string title;
        private bool isPaused;
        private bool hasFocus;
        private bool isInitialized;

        private RenderForm form;
        private GameServicesCollection services;
        private Logger logger;

        private ApplicationStateManager appStateManager;
        private GameScreenManager screenManager;
        private GameTime gameTime;
        private GameSettings settings;
        private List<Camera> cameras;
        #endregion

        #region Public Events
        /// <summary>
        /// Handler to create custom Initialized event.
        /// </summary>
        public delegate void OnInitializedHandler();
        /// <summary>
        /// Event raised when the Initialize method has finished.
        /// </summary>
        public event OnInitializedHandler OnInitialized;

        /// <summary>
        /// Handler to create custom OnCameraAdded event.
        /// </summary>
        public delegate void OnCameraAddedHandler();
        /// <summary>
        /// Event raised when a Camera has been added to the game.
        /// The cameras are added as components.
        /// </summary>
        public event OnCameraAddedHandler OnCameraAdded;
        #endregion

        #region Properties
        /// <summary>
        /// The logger for all events in the game class.
        /// </summary>
        public Logger Log
        {
            get { return logger; }
            set { logger = value; }
        }

        /// <summary>
        /// Get the list of all game services.
        /// </summary>
        public GameServicesCollection Services
        {
            get { return services; }
        }

        /// <summary>
        /// Name of the Game.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Title to be shown in the game's title bar.
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// Used to stop updating the main loop.
        /// </summary>
        public bool IsPaused
        {
            get { return isPaused; }
            set { isPaused = value; }
        }

        /// <summary>
        /// Get or set if the game has finished initialization.
        /// </summary>
        public bool IsInitialized
        {
            get { return isInitialized; }
            set { isInitialized = value; }
        }

        /// <summary>
        /// Gets if the current window of the game has focus.
        /// If it has no focus, the game stops updating.
        /// </summary>
        public bool HasFocus
        {
            get { return hasFocus; }
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

        /// <summary>
        /// The GameTime to measure all the in-game and off-game times.
        /// </summary>
        public GameTime GameTime
        {
            get { return gameTime; }
        }

        /// <summary>
        /// The game settings to initialize this instance of game.
        /// </summary>
        public GameSettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// A list of the cameras used in the game.
        /// </summary>
        public List<Camera> Cameras
        {
            get { return cameras; }
        }

        /// <summary>
        /// The Content Root directory to put in all the content files of the game.
        /// </summary>
        public string ContentDirectory { get; set; } = "Content";
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a game instance with the given settings.
        /// </summary>
        /// <param name="settings">The settings objects to initialize the game. If it is null, it is initialized with default values.</param>
        public Game(GameSettings settings)
        {
            this.settings = settings == null ? new GameSettings() : settings;
            this.logger = new Logger(GetType());
            screenManager = new GameScreenManager(this);
            appStateManager = new ApplicationStateManager(this);
            gameTime = new GameTime(this);
            services = new GameServicesCollection();
            cameras = new List<Camera>();

            // Set the console size.
            try
            {
                Console.SetWindowSize(140, 40);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to set custom console size.", ex);
            }

            services.AddService(gameTime);
            services.AddService(appStateManager);
            services.AddService(screenManager);
        }

        /// <summary>
        /// Main method where the initialization is made.
        /// <para/>Create the swap chain, attach events and start measuring the gameTime.
        /// <para/>Call the <see cref="CreateSwapDescriptionAndDevice"/> method and the <see cref="PostDeviceInitialization"/> that must
        /// be implemented when inherit from this class.
        /// </summary>
        public virtual void Initialize()
        {
            // Initialize each service added.
            logger.Info("Initializing services...");
            foreach (IGameService serv in services.Values)
            {
                if (!serv.IsInitialized)
                    serv.Initialize();
            }

            // Set the current game state.
            appStateManager.SetCurrentState(ApplicationStateManager.ApplicationState.Starting);

            logger.Info("Initializing main application window with size: " + settings.WindowSize.ToString());
            if (form == null)
                form = new RenderForm(this.name);
            form.Size = settings.WindowSize;

            // Create main device.
            CreateSwapDescriptionAndDevice();

            // Extra initialization after creating the device.
            PostDeviceInitialization();

            // Attach Events.
            form.UserResized += WindowResizedEvent;
            form.LostFocus += (sender, e) => { hasFocus = false; };
            form.GotFocus += (sender, e) => { hasFocus = true; };

            // Call the window resized event at the beginning to setup the initial properties.
            WindowResizedEvent(null, null);

            // Start measuring the game time.
            gameTime.Start();

            // Raise the OnInitialized event.
            isInitialized = true;
            OnInitialized?.Invoke();

            logger.Info("Finished Game Initialization");
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
        /// <summary>
        /// Dispose all the managed and unmanaged resources.
        /// </summary>
        public abstract void Dispose();
        #endregion

        #region Public Methods
        /// <summary>
        /// Must be overridden to load the assets and all content.
        /// </summary>
        public virtual void LoadContent()
        {
            screenManager.LoadContent();

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
            form.Show();
            RenderLoop loop = new RenderLoop(form);
            while (loop.NextFrame())
            {
                mainLoop();
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
            if (!hasFocus)
            {
                if (appStateManager.CurrentState == ApplicationStateManager.ApplicationState.Running)
                {
                    appStateManager.SetCurrentState(ApplicationStateManager.ApplicationState.Paused);
                    gameTime.Pause();
                }
                return;
            }

            // Check if the game is running.
            if (appStateManager.CurrentState != ApplicationStateManager.ApplicationState.Running)
            {
                appStateManager.SetCurrentState(ApplicationStateManager.ApplicationState.Running);
                gameTime.Start();
            }

            update();
            render();
        }
        #endregion
    }
}
