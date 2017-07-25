using System;
using PoncheToolkit.Util;

namespace PoncheToolkit.Core.Components
{
    /// <summary>
    /// Main abstract class that implements a component that will only be updated and not drawn.
    /// </summary>
    public abstract class GameComponent : IGameComponent
    {
        #region Properties
        /// <inheritdoc/>
        public bool IsInitialized { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public Logger Log { get; set; }

        /// <inheritdoc/>
        public Game11 Game { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Handler to create custom Initialized event.
        /// </summary>
        public delegate void OnInitializedHandler();
        /// <summary>
        /// Event raised when finished initialization.
        /// It is recommended to add any other functionality for initialization using this event,
        /// to ensure that the initialization has completed.
        /// </summary>
        public abstract event OnInitializedHandler OnInitialized;

        /// <summary>
        /// Handler to create custom LoadContent event.
        /// </summary>
        public delegate void OnFinishLoadContentHandler();
        /// <summary>
        /// Event raised when finished loading content.
        /// It is recommended to add any other functionality for loading content using this event,
        /// to ensure that the any previous loading has completed.
        /// </summary>
        public abstract event OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public GameComponent(Game11 game)
        {
            this.Game = game;
            Log = new Logger(GetType());
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public virtual void Initialize()
        {
            Log.Info("Initialized component of type -{0}- with name: -{1}-", GetType().Name, this.Name);
        }

        /// <inheritdoc/>
        public virtual void LoadContent()
        {
            Log.Info("Loading content from component of type -{0}- with name: -{1}-", GetType().Name, this.Name);
        }

        /// <inheritdoc/>
        public virtual void UnloadContent()
        {
            
        }

        /// <inheritdoc/>
        public virtual void Update()
        {
            
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Dispose the managed and unmanaged resources.
        /// </summary>
        public abstract void Dispose();
        #endregion

    }
}
