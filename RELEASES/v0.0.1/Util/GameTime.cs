using System;
using System.Diagnostics;
using PoncheToolkit.Core.Components;

namespace PoncheToolkit.Util
{
    /// <summary>
    /// Class that help measure the elapsed time of the game.
    /// This must be used to calculate physics, collisions, etc.
    /// </summary>
    public class GameTime : IGameComponent
    {
        #region Fields
        private Stopwatch watch;
        #endregion

        #region Properties
        public Stopwatch Watch
        {
            get { return watch; }
            set { watch = value; }
        }

        public bool IsRunning { get; set; }

        public bool IsInitialized { get; set; }
        #endregion

        #region Initialization
        public GameTime()
        {
            watch = new Stopwatch();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Pause the timer with the current time.
        /// </summary>
        public void Pause()
        {
            watch.Stop();
            IsRunning = false;
        }

        /// <summary>
        /// Stop the timer and resets its value to zero.
        /// </summary>
        public void Stop()
        {
            watch.Stop();
            watch.Reset();
            IsRunning = false;
        }

        /// <summary>
        /// Start to measure time.
        /// </summary>
        public void Start()
        {
            watch.Start();
            IsRunning = true;
        }

        public void Initialize()
        {

        }

        public void Update()
        {

        }

        public void Dispose()
        {
            watch.Stop();
            watch = null;
        }
        #endregion
    }
}
