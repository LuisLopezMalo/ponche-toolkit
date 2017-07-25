using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Core.Management.Input;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;
using PoncheToolkit.Graphics3D.Cameras;

namespace PoncheToolkit.Core.Management.Screen
{
    /// <summary>
    /// Class that manages all the screens in the game.
    /// Is in charge to create, pause, destroy and decide which screen gets updated and drawn and which not.
    /// </summary>
    public class GameScreenManager : GameService, IContentLoadable, IInputReceivable
    {
        #region Fields
        private List<GameScreen> screens;
        #endregion

        #region Properties
        /// <summary>
        /// Get the list of screens.
        /// </summary>
        public List<GameScreen> Screens
        {
            get { return screens; }
        }

        /// <summary>
        /// Get the last screen added.
        /// </summary>
        public GameScreen LastScreen
        {
            get { return Screens[Screens.Count - 1]; }
        }

        /// <inheritdoc/>
        public bool IsContentLoaded { get; set; }
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;

        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
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
        public void LoadContent(IContentManager contentManager)
        {
            foreach (GameScreen screen in screens)
                screen.LoadContent(contentManager);

            OnFinishLoadContent?.Invoke();
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            foreach (GameScreen screen in screens)
            {
                if (screen.UpdateMode == GameScreen.ScreenUpdateMode.Always ||
                    (screen.UpdateMode == GameScreen.ScreenUpdateMode.OnlyWhenActive && screen.IsActive))
                    screen.UpdateLogic(gameTime);
            }
        }

        /// <inheritdoc/>
        public void UpdateInput(InputManager inputManager)
        {
            foreach (GameScreen screen in screens)
                screen.UpdateInput(inputManager);
        }

        /// <summary>
        /// Render the screens by their <see cref="GameScreen.ScreenRenderMode"/>
        /// </summary>
        public void Render(GraphicsRenderer11 renderer, Camera camera)
        {
            foreach (GameScreen screen in screens)
            {
                // Render the screen
                if (screen.RenderMode == GameScreen.ScreenRenderMode.Always ||
                    (screen.RenderMode == GameScreen.ScreenRenderMode.OnlyWhenActive && screen.IsActive))
                {
                    //if (screen.RenderTargetType == GameScreen.ScreenRenderTargetType.Texture)
                    //    renderer.RenderScreenToTexture(screen, camera, renderer.SpriteBatch, screen.RenderTargetView);
                    //else

                    // If there are any post process effect, render the scene to the postProcess render target instead of the back buffer.
                    if (renderer.PostProcessEffects.Count > 0)
                        renderer.RenderScreenToTexture(screen, camera, renderer.SpriteBatch, ref renderer.postProcessRenderTarget);
                    else
                        renderer.RenderScreen(screen, camera, renderer.SpriteBatch);
                    screen.Render(renderer.SpriteBatch); // To render the 2D content.
                }
            }

            // Render some basic debugging information.
            if (Game.Settings.DebugMode && renderer.SpriteBatch.IsInitialized)
            {
                renderer.SpriteBatch.Begin(null);
                Game.DebuggerService.Render(renderer.SpriteBatch);
                renderer.SpriteBatch.End();
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
