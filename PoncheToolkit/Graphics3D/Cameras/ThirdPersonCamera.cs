using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using SharpDX;

namespace PoncheToolkit.Graphics3D.Cameras
{
    /// <summary>
    /// Camera that represent a first person view.
    /// </summary>
    public class ThirdPersonCamera : Camera
    {
        /// <summary>
        /// The target the camera will follow.
        /// </summary>
        public Model Target { get; set; }

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
        public ThirdPersonCamera(Game11 game)
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
        public override void Update()
        {
            if (Target == null)
                throw new NullReferenceException("The target has not been set.");
            
            LookAt = Target.Position;
            base.Update();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
        }
        #endregion
    }
}
