using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Core.Management.Screen
{
    /// <summary>
    /// Class that manages all the screens in the game.
    /// Is in charge to create, pause, destroy and decide which screen gets updated and drawn and which not.
    /// </summary>
    public class GameScreenManager : GameService
    {
        #region Fields
        private List<GameScreen> screens;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public bool IsInitialized { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <summary>
        /// Get the list of screens.
        /// </summary>
        public List<GameScreen> Screens { get { return screens; } }
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public GameScreenManager(Game game)
            : base(game)
        {
            screens = new List<GameScreen>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a new screen to the list.
        /// If the screen has not been initialized, it is initialized here.
        /// </summary>
        /// <param name="screen"></param>
        public void AddScreen(GameScreen screen)
        {
            if (!screen.IsInitialized)
                screen.Initialize();

            screens.Add(screen);
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            // Initialize all the screens and set them to initialized state.
            foreach (GameScreen screen in screens)
            {
                screen.Initialize();
                screen.CurrentScreenState = GameScreen.ScreenState.Initialized;
            }
            IsInitialized = true;

            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Load the contents from the added screens.
        /// </summary>
        public void LoadContent()
        {
            foreach (GameScreen screen in screens)
                screen.LoadContent();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            foreach (GameScreen screen in screens)
            {
                if (screen.UpdateMode == GameScreen.ScreenUpdateMode.Always ||
                    (screen.UpdateMode == GameScreen.ScreenUpdateMode.OnlyWhenActive && screen.IsActive))
                    screen.Update();
            }
        }

        /// <summary>
        /// Render the screens by their <see cref="GameScreen.ScreenRenderMode"/>
        /// </summary>
        public void Render()
        {
            foreach (GameScreen screen in screens)
            {
                // Render the screen
                if (screen.RenderMode == GameScreen.ScreenRenderMode.Always ||
                    (screen.RenderMode == GameScreen.ScreenRenderMode.OnlyWhenActive && screen.IsActive))
                    screen.Render();
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            foreach (GameScreen screen in screens)
                screen.Dispose();
        }
        
        #endregion
    }
}
