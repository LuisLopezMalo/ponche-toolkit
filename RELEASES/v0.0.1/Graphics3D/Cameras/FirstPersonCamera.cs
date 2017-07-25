using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;

namespace PoncheToolkit.Graphics3D.Cameras
{
    /// <summary>
    /// Camera that represent a first person view.
    /// </summary>
    public class FirstPersonCamera : Camera
    {
        #region Events
        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public FirstPersonCamera(Game11 game)
            : base(game)
        {
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
        }
        #endregion
    }
}
