using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Core.Management
{
    /// <summary>
    /// Manages the states of all the game.
    /// </summary>
    public class ApplicationStateManager : GameService
    {
        /// <summary>
        /// Enumeration that represent the state of the application
        /// </summary>
        public enum ApplicationState
        {
            Starting,
            Started,
            Running,
            Paused,
            Stopped
        }

        #region Fields
        private ApplicationState state;
        #endregion

        #region Properties
        /// <summary>
        /// Get the current state for the game.
        /// To set the set use the SetCurrentState() method.
        /// </summary>
        public ApplicationState CurrentState
        {
            get { return state; }
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
        /// <param name="game"></param>
        public ApplicationStateManager(Game game)
            : base(game)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set the current state for the application.
        /// </summary>
        /// <param name="state"></param>
        public void SetCurrentState(ApplicationState state)
        {
            this.state = state;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            
        }
        #endregion
    }
}
