using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Util;

namespace PT.Graphics3D.Cameras
{
    /// <summary>
    /// Camera that represent a first person view.
    /// </summary>
    public class FreeCamera : Camera
    {
        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public FreeCamera(Game11 game)
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

        /// <inheritdoc/>
        public override void UpdateLogic()
        {
            base.UpdateLogic();
        }
        #endregion
    }
}
