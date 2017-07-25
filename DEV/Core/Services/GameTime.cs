using System;
using System.Diagnostics;
using PoncheToolkit.Core.Components;
using System.Threading;
using PoncheToolkit.Util;

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
        private float newTime;
        private float lastRealTime;
        private float deltaTime;
        private float deltaAccumulator;
        private float frameTimeElapsed;
        private float physicsDeltaTime;
        private bool isRunning;
        private int fps;

        // Fixed part to calculate the remainder in time for physics
        private static readonly float targetFPS = 1f / 60f;
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
        public override event EventHandlers.OnInitializedHandler OnInitialized;

        /// <summary>
        /// Handler to create custom OnSecondElapsed event.
        /// </summary>
        /// <param name="fps">The frames ran for this second.</param>
        public delegate void OnSecondElapsedEventHandler(int fps);
        /// <summary>
        /// Event raised when a single second has elapsed.
        /// </summary>
        public event OnSecondElapsedEventHandler OnSecondElapsed;

        /// <summary>
        /// Handler to create custom OnUpdatePhysicsDeltaTime event.
        /// </summary>
        public delegate void OnPhysicsUpdateDeltaTimeHandler(ref float physicsDeltaTime);
        /// <summary>
        /// Event raised when before the calculation of Delta Time, so the physics delta time is set correctly
        /// so the game rendering updates and the physics updates are decoupled.
        /// </summary>
        public event OnPhysicsUpdateDeltaTimeHandler OnPhysicsUpdateDeltaTime;

        /// <summary>
        /// Handler to create custom OnPhysicsUpdate event.
        /// </summary>
        public delegate void OnPhysicsUpdateHandler(float physicsDeltaTime);
        /// <summary>
        /// Event raised when inside the loop to update physics.
        /// This must be implemented when incorporating physics, and here the update must me made.
        /// </summary>
        public event OnPhysicsUpdateHandler OnPhysicsUpdate;
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
            lastRealTime = (float)realTime.Elapsed.TotalSeconds;
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            /*
            //  
            // calculate timing metrics  
            //  
            double new_time = SDL_GetTicks();  
            double frame_time = (new_time - current_time) /1000;  
            if ( frame_time > target_frame_time)  
                frame_time = target_frame_time;  
            current_time = new_time;  
            accumulator += frame_time;  
            
            // update physics as many times as possible in remaining frame time  
            
            while ( accumulator >= physics_step )
            {
                getDefaultCamera()->update(physics_step);
                for( unsigned int i = 0; i < physics_list.size(); ++i ) 
                {   
                    physics_list[i]->update( physics_step );  
                }  
                accumulator -= physics_step;  
            }
            // do rendering  
            
            translate( getDefaultCamera()->getViewport().x, getDefaultCamera()->getViewport().y );
            render();
            */


            newTime = (float)realTime.Elapsed.TotalSeconds;
            deltaTime = newTime - lastRealTime;

            if (Game.Settings.LockFramerate)
            {
                if (deltaTime > targetFPS)
                    deltaTime = targetFPS;
            }

            lastRealTime = newTime;
            deltaAccumulator += deltaTime;

            OnPhysicsUpdateDeltaTime?.Invoke(ref physicsDeltaTime);

            // Delays the current frame so it takes at least the time of the fixed target FPS.
            // Update physics as many times as possible in remaining frame time.
            while (deltaAccumulator >= targetFPS)
            {
                OnPhysicsUpdate?.Invoke(physicsDeltaTime);
                deltaAccumulator -= targetFPS;
            }

            fps++;

            //double alpha = deltaAccumulator / targetFPS;

            //State state = currentState * alpha +
            //    previousState * (1.0 - alpha);

            frameTimeElapsed += deltaTime;

            if (frameTimeElapsed >= TimeSpan.FromSeconds(1).TotalSeconds)
            {
                // Fire the event with the FPS for the last second elapsed
                OnSecondElapsed?.Invoke(fps);
                frameTimeElapsed = 0;
                fps = 0;
            }
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
