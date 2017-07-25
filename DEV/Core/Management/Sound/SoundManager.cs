using PoncheToolkit.Core.Services;
using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Sound
{
    /// <summary>
    /// Class that manages the sounds.
    /// Made as a service.
    /// </summary>
    public class SoundManager : GameService
    {
        #region Fields
        private SharpDX.XAudio2.XAudio2 xAudio;
        #endregion

        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        public SoundManager(Game game)
            : base(game)
        {
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            xAudio = new SharpDX.XAudio2.XAudio2();

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            
        }

        /// <inheritdoc/>
        public override void Dispose()
        {

        }
        #endregion

    }
}
