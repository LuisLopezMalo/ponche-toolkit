using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using SharpDX;
using SharpDX.Direct3D11;
using RasterizerStateDescription = SharpDX.Direct3D11.RasterizerStateDescription;
using PoncheToolkit.Util;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Class that wrap the rasterizing properties.
    /// </summary>
    public class PTRasterizer : UpdatableStateObject
    {
        #region Fields
        private RasterizerStateDescription description;
        private RasterizerState rasterizerState;
        private GraphicsRenderer11 renderer;
        #endregion

        #region Properties
        /// <summary>
        /// Default: false
        /// Only works if <see cref="IsMultisampleEnabled"/> is set to false.
        /// </summary>
        public bool IsAntialiasedLineEnabled { get { return description.IsAntialiasedLineEnabled; } set { SetPropertyAsDirty(ref description.IsAntialiasedLineEnabled, value); } }
        /// <summary>
        /// Default: Back
        /// </summary>
        public CullMode CullMode { get { return description.CullMode; } set { SetPropertyAsDirty(ref description.CullMode, value); } }
        /// <summary>
        /// Default: 0
        /// </summary>
        public int DepthBias
        {
            get { return description.DepthBias; }
            set { SetPropertyAsDirty(ref description.DepthBias, value); }
        }
        /// <summary>
        /// Default: 0.0
        /// </summary>
        public float DepthBiasClamp
        {
            get { return description.DepthBiasClamp; }
            set { SetPropertyAsDirty(ref description.DepthBiasClamp, value); }
        }
        /// <summary>
        /// Default: true
        /// </summary>
        public bool IsDepthClipEnabled
        {
            get { return description.IsDepthClipEnabled; }
            set { SetPropertyAsDirty(ref description.IsDepthClipEnabled, value); }
        }
        /// <summary>
        /// Default: Solid
        /// </summary>
        public FillMode FillMode
        {
            get { return description.FillMode; }
            set { SetPropertyAsDirty(ref description.FillMode, value); }
        }
        /// <summary>
        /// Default: false
        /// </summary>
        public bool IsFrontCounterClockwise
        {
            get { return description.IsFrontCounterClockwise; }
            set { SetPropertyAsDirty(ref description.IsFrontCounterClockwise, value); }
        }
        /// <summary>
        /// <para>Default: false</para>
        /// Specifies whether to use the quadrilateral or alpha line anti-aliasing algorithm
        /// on multisample antialiasing (MSAA) render targets. Set to TRUE to use the quadrilateral
        /// line anti-aliasing algorithm and to SharpDX.Result.False to use the alpha line
        /// anti-aliasing algorithm. For more info about this member, see Remarks.
        /// </summary>
        public bool IsMultisampleEnabled
        {
            get { return description.IsMultisampleEnabled; }
            set { SetPropertyAsDirty(ref description.IsMultisampleEnabled, value); }
        }
        /// <summary>
        /// <para>Default: false</para>
        /// Enable scissor-rectangle culling. All pixels outside an active scissor rectangle are culled.
        /// </summary>
        public bool IsScissorEnabled
        {
            get { return description.IsScissorEnabled; }
            set { SetPropertyAsDirty(ref description.IsScissorEnabled, value); }
        }
        /// <summary>
        /// Default: 0.0
        /// </summary>
        public float SlopeScaledDepthBias
        {
            get { return description.SlopeScaledDepthBias; }
            set { SetPropertyAsDirty(ref description.SlopeScaledDepthBias, value); }
        }
        /// <summary>
        /// Get the rasterizer state.
        /// </summary>
        public RasterizerState RasterizerState
        {
            get
            {
                if (rasterizerState == null)
                {
                    UpdateState();
                }
                return rasterizerState;
            }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTRasterizer(GraphicsRenderer11 renderer)
        {
            this.renderer = renderer;
            description = new RasterizerStateDescription();
            
            this.IsAntialiasedLineEnabled = false;
            this.CullMode = CullMode.Back;
            this.DepthBias = 0;
            this.DepthBiasClamp = 0.0f;
            this.IsDepthClipEnabled = true;
            this.FillMode = FillMode.Solid;
            this.IsFrontCounterClockwise = false;
            this.IsMultisampleEnabled = false;
            this.IsScissorEnabled = false;
            this.SlopeScaledDepthBias = 0.0f;
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                rasterizerState = new RasterizerState(renderer.Device, description);
                renderer.ImmediateContext.Rasterizer.State = rasterizerState;
                IsStateUpdated = true;
                OnStateUpdated();
            }

            return IsStateUpdated;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Utilities.Dispose(ref rasterizerState);
        }
    }
}
