using System;

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

        /// <summary>
        /// The game instance.
        /// </summary>
        public Game Game { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Handler to create custom Initialized event.
        /// </summary>
        public delegate void OnInitializedHandler();
        /// <summary>
        /// Event raised when finished initialization.
        /// </summary>
        public abstract event OnInitializedHandler OnInitialized;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor. Set the game instance.
        /// By default set the name of the service to its class name.
        /// </summary>
        /// <param name="game"></param>
        public GameService(Game game)
        {
            this.Game = game;
            this.Name = GetType().Name;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public virtual void Initialize()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Abstract Methods
        /// <inheritdoc/>
        public abstract void Update();

        /// <inheritdoc/>
        public abstract void Dispose();
        #endregion
    }
}