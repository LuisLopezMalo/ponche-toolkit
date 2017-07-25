using System;
using PoncheToolkit.Util;

namespace PoncheToolkit.Core.Services
{
    /// <summary>
    /// Main abstract class that implements a service that will be updated.
    /// The services are identified by their type.
    /// Only one service by type can be added.
    /// </summary>
    public abstract class GameService : IGameService
    {
        #region Properties
        /// <inheritdoc/>
        public bool IsInitialized { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
#if DX11
        public Game11 Game { get; set; }
#elif DX12
        public Game12 Game { get; set; }
#endif

        /// <inheritdoc/>
        public Logger Log { get; set; }
        #endregion

        #region Events
        ///// <summary>
        ///// Handler to create custom Initialized event.
        ///// </summary>
        //public delegate void OnInitializedHandler();
        /// <summary>
        /// Event raised when finished initialization.
        /// </summary>
        public abstract event EventHandlers.OnInitializedHandler OnInitialized;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor. Set the game instance.
        /// By default set the name of the service to its class name.
        /// </summary>
        /// <param name="game">The game instance used.</param>
        public GameService(Game game)
        {
#if DX11
        this.Game = game as Game11;
#elif DX12
        this.Game = game as Game12;
#endif

            this.Name = GetType().Name;
            this.Log = new Logger(GetType());
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public virtual void Initialize()
        {
            Log.Info("Initializing service -{0}-", Name);
        }
        #endregion

        #region Abstract Methods
        /// <inheritdoc/>
        public abstract void UpdateLogic(GameTime gameTime);

        /// <inheritdoc/>
        public abstract void Dispose();
        #endregion
    }
}