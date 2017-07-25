using System;
using System.Diagnostics;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using PoncheToolkit.Util;

namespace PoncheToolkit.Core.Services
{
    /// <summary>
    /// Class that has debug stats or functionality.
    /// Implements <see cref="IRenderable"/> interface so it can render debugger info onScreen.
    /// This service will be added when the <see cref="GameSettings.DebugMode"/> is set to true.
    /// If it remains false, the service is not added.
    /// </summary>
    public class DebuggerRenderableService : GameService, IRenderable
    {
        #region Fields
        private GameTime gameTime;
        private Vector2 textPosition;
        private Process process;

        private int fps;
        private long physicalMemoryUsage;
        private TimeSpan userProcessorTime;
        private TimeSpan totalProcessorTime;
        private int lastProcessorUsageMilliseconds;
        private int processorUsageMilliseconds;
        private int modelsDrawn;

        private int textIntervalY;

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

        /// <summary>
        /// Get the memory used by the process.
        /// </summary>
        public long PhysicalMemoryUsage
        {
            get { return physicalMemoryUsage; }
        }

        /// <summary>
        /// Get the user processing time for this process.
        /// </summary>
        public TimeSpan UserProcessorTime
        {
            get { return userProcessorTime; }
        }

        /// <summary>
        /// Get the total processing time for this process.
        /// </summary>
        public TimeSpan TotalProcessorTime
        {
            get { return totalProcessorTime; }
        }

        /// <summary>
        /// Get the processing usage in percentage. 0-1
        /// </summary>
        public float ProcessorUsage
        {
            get { return processorUsageMilliseconds; }
        }

        /// <summary>
        /// Get the current drawn models inside the camera frustrum.
        /// This is updated inside the <see cref="IGraphicsRenderer"/> taking the
        /// <see cref="Game.CurrentCamera"/>. See <see cref="BoundingFrustum"/>
        /// </summary>
        public int ModelsDrawn
        {
            get { return modelsDrawn; }
            internal set { modelsDrawn = value; }
        }

        /// <summary>
        /// Get or set to simulate cpu intense usage when rendering.
        /// It calculates and applies 50x times the Camera matrices and Map/Unmmap materials for each mesh.
        /// Default: false.
        /// </summary>
        public bool SimulateBurnCPU { get; set; }

        /// <inheritdoc/>
        public List<PTEffect> Effects { get; set; }
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The running game instance.</param>
        public DebuggerRenderableService(Game game)
            : base(game)
        {
            gameTime = game.Services.GetService(typeof(GameTime)) as GameTime;
            process = Process.GetCurrentProcess();
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            gameTime.OnSecondElapsed += GameTime_OnSecondElapsed;

            textPosition = new Vector2(5, 40);
            textIntervalY = 15;

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        private void GameTime_OnSecondElapsed(int fps)
        {
            OnFPSCaptured?.Invoke(fps);
            this.fps = fps;

            physicalMemoryUsage = process.WorkingSet64 / 1000 / 1000;
            totalProcessorTime = process.TotalProcessorTime;
            userProcessorTime = process.UserProcessorTime;

            int newTime = totalProcessorTime.Milliseconds;
            processorUsageMilliseconds = (newTime - lastProcessorUsageMilliseconds) / 100;
            lastProcessorUsageMilliseconds = newTime;
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            //fps++;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            
        }

        /// <inheritdoc/>
        public void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            textPosition = new Vector2(5, 40);

            spriteBatch.DrawText("FPS: " + fps, textPosition, SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Milliseconds: " + (Game.GameTime.DeltaTime * 1000), new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Resolution: " + Game.Settings.Resolution, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Current GPU: " + Game.SystemDescription.Description, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Dedicated Memory: " + (Game as Game11).SystemDescription.DedicatedVideoMemory / 1024 / 1024, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Physical Memory: " + physicalMemoryUsage, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Current Screen: " + Game.ScreenManager.LastScreen.Name, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Camera: " + Game.CurrentCamera.Position, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Mouse: " + Game.InputManager.MousePosition, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("User Processing time: " + userProcessorTime, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Total Processing time: " + totalProcessorTime, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Processing percentage: " + ProcessorUsage, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Rendering Mode: " + (Game as Game11).Renderer.ProcessRenderMode, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Framerate Locked: " + Game.Settings.LockFramerate, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Threads for Deferred: " + Management.Threading.ThreadingManager.CURRENT_RENDERING_THREADS_COUNT, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Models Drawn: " + ModelsDrawn, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
            spriteBatch.DrawText("Gamma: " + Game.Settings.Gamma, new Vector2(textPosition.X, textPosition.Y += textIntervalY), SpriteBatch.INFO_TEXT_FORMAT, TextBrushes.White);
        }
        #endregion
    }
}