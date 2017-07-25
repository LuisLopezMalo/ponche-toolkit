using System;
using PoncheToolkit.Util;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Collections.Generic;
using PoncheToolkit.Core.Management.Input;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Core.Services;
using System.Runtime.Serialization;

namespace PoncheToolkit.Core.Components
{
    /// <summary>
    /// Main abstract class that implements a component that will only be updated and not drawn.
    /// </summary>
    public abstract class GameComponent : UpdatableStateObject, IGameComponent
    {
        private static string initializeStr;
        private static string loadContentStr;

        private bool isInitialized;
        private bool isContentLoaded;
        private string name;
#if DX11
        private Game11 game;
#elif DX12
        private Game12 game;
#endif

        #region Properties
        /// <inheritdoc/>
        public bool IsInitialized { get { return isInitialized; } set { SetPropertyAsDirty(ref isInitialized, value); } }

        /// <inheritdoc/>
        public bool IsContentLoaded { get { return isContentLoaded; } set { SetPropertyAsDirty(ref isContentLoaded, value); } }

        /// <inheritdoc/>
        public string Name { get { return name; } set { SetPropertyAsDirty(ref name, value); } }

        /// <inheritdoc/>
#if DX11
        public Game11 Game
        {
            get { return game; }
            set { SetProperty(ref game, value); }
        }
#elif DX12
        public Game12 Game
        {
            get { return game; }
            set { SetProperty(ref game, value); }
        }
#endif

        #endregion

        #region Events
        /// <inheritdoc/>
        public abstract event EventHandlers.OnInitializedHandler OnInitialized;

        /// <summary>
        /// Event raised when finished loading content.
        /// It is recommended to add any other functionality for loading content using this event,
        /// to ensure that the any previous loading has completed.
        /// </summary>
        public abstract event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
#if DX11
        public GameComponent(Game11 game)
#elif DX12
        public GameComponent(Game12 game)
#endif
            : base()
        {
            this.Game = game;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public virtual void Initialize()
        {
            initializeStr = null;
            initializeStr = string.Format("Initialized component of type -{0}- with name: -{1}-", GetType().Name, this.Name);
            Log.Info(initializeStr);
        }

        /// <inheritdoc/>
        public virtual void LoadContent(IContentManager contentManager)
        {
            loadContentStr = null;
            loadContentStr = string.Format("Loading content from component of type -{0}- with name: -{1}-", GetType().Name, this.Name);
            Log.Info(loadContentStr);
        }

        /// <inheritdoc/>
        public virtual void UnloadContent()
        {
        }

        /// <inheritdoc/>
        public virtual void UpdateLogic(GameTime gameTime)
        {
        }

        /// <inheritdoc/>
        public virtual void UpdateInput(InputManager inputManager)
        {
        }
        #endregion

    }
}
