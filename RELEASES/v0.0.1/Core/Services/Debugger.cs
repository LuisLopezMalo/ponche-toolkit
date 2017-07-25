using System;
using System.Diagnostics;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Core.Services
{
    /// <summary>
    /// Class that has debug stats or functionality.
    /// </summary>
    public class Debugger : GameService
    {
        #region Fields
        private GameTime gameTime;
        private int fps;

        /// <summary>
        /// Handler to create custom OnFPSCaptured event.
        /// </summary>
        public delegate void OnFPSCapturedEventHandler(int fps);
        /// <summary>
        /// Event raised when a second has elapsed.
        /// </summary>
        public event OnFPSCapturedEventHandler OnFPSCaptured;
        #endregion

        #region Properties
        /// <summary>
        /// The Frames per second count.
        /// </summary>
        public int FPS
        {
            get { return fps; }
            set { fps = value; }
        }
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The running game instance.</param>
        public Debugger(Game game)
            : base(game)
        {
            gameTime = game.Services.GetService(typeof(GameTime)) as GameTime;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            gameTime.OnSecondElapsed += GameTime_OnSecondElapsed;
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        private void GameTime_OnSecondElapsed()
        {
            OnFPSCaptured?.Invoke(fps);
            fps = 0;
        }

        /// <inheritdoc/>
        public override void Update()
        {
            fps++;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            
        }
        #endregion
    }
}