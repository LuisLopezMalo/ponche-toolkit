using System;
using System.Diagnostics;
using PoncheToolkit.Core.Components;

namespace PoncheToolkit.Core.Services
{
    /// <summary>
    /// Class that help measure the elapsed time of the game.
    /// This must be used to calculate physics, collisions, etc.
    /// </summary>
    public class GameTime : GameService
    {
        #region Fields
        private Stopwatch gameTime;
        private Stopwatch realTime;
        private float lastDeltaTime;
        private float deltaTime;
        private float frameTimeElapsed;
        private bool isRunning;

        /// <summary>
        /// Handler to create custom OnSecondElapsed event.
        /// </summary>
        public delegate void OnSecondElapsedEventHandler();
        /// <summary>
        /// Event raised when a single second has elapsed.
        /// </summary>
        public event OnSecondElapsedEventHandler OnSecondElapsed;
        #endregion

        #region Properties
        /// <summary>
        /// The real time elapsed. Measure the time from the beginning of the game.
        /// This time is never stopped or paused.
        /// </summary>
        public TimeSpan RealTimeElapsed
        {
            get { return realTime.Elapsed; }
        }

        /// <summary>
        /// Measure the time between frames.
        /// This times is paused when the IsPaused variable is active in the Game class.
        /// </summary>
        public TimeSpan GameTimeElapsed
        {
            get { return gameTime.Elapsed; }
        }

        /// <summary>
        /// The time difference from the last frame.
        /// </summary>
        public float DeltaTime
        {
            get { return deltaTime; }
        }

        /// <summary>
        /// Get if the gameTime status is running or paused.
        /// </summary>
        public bool IsRunning { get { return isRunning; } }
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public GameTime(Game game)
            : base(game)
        {
            gameTime = new Stopwatch();
            realTime = new Stopwatch();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Pause the timer with the current time.
        /// </summary>
        public void Pause()
        {
            gameTime.Stop();
            isRunning = false;
        }

        /// <summary>
        /// Stop the timer and resets its value to zero.
        /// </summary>
        public void Stop()
        {
            gameTime.Stop();
            gameTime.Reset();
            isRunning = false;
        }

        /// <summary>
        /// Start to measure time.
        /// </summary>
        public void Start()
        {
            gameTime.Start();
            realTime.Start();
            isRunning = true;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            // Set the delta time
            deltaTime = (float)gameTime.Elapsed.TotalSeconds;

            frameTimeElapsed += deltaTime;

            // Counts the FPS.
            if (frameTimeElapsed >= TimeSpan.FromSeconds(1).TotalSeconds)
            {
                // Fire the event with the FPS for the last second elapsed
                OnSecondElapsed?.Invoke();
                frameTimeElapsed = 0;
            }
            //if (gameTime.Elapsed.TotalSeconds >= TimeSpan.FromSeconds(1).TotalSeconds)
            //{
            //    // Fire the event with the FPS for the last second elapsed
            //    OnSecondElapsed?.Invoke();
            //    gameTime.Restart();
            //}

            gameTime.Restart();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            gameTime.Stop();
            gameTime = null;
        }
        #endregion
    }
}
