using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using SharpDX;
using SharpDX.Direct3D11;
using RasterizerStateDescription = SharpDX.Direct3D11.RasterizerStateDescription;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Class that wrap the rasterizing properties.
    /// </summary>
    public class Rasterizer : UpdatableState, IDisposable
    {
        #region Fields
        private RasterizerStateDescription description;
        private RasterizerState rasterizerState;
        private GraphicsRenderer renderer;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public event EventHandler OnValueChanged;
        /// <inheritdoc/>
        public event EventHandler OnUpdated;

        /// <summary>
        /// Default: false
        /// </summary>
        public bool IsAntialiasedLineEnabled { get { return description.IsAntialiasedLineEnabled; } set { description.IsAntialiasedLineEnabled = value; OnValueChanged?.Invoke(this, null); } }
        /// <summary>
        /// Default: Back
        /// </summary>
        public CullMode CullMode { get { return description.CullMode; } set { description.CullMode = value; OnValueChanged?.Invoke(this, null); } }
        /// <summary>
        /// Default: 0
        /// </summary>
        public int DepthBias { get { return description.DepthBias; } set { description.DepthBias = value; OnValueChanged?.Invoke(this, null); } }
        /// <summary>
        /// Default: 0.0
        /// </summary>
        public float DepthBiasClamp { get { return description.DepthBiasClamp; } set { description.DepthBiasClamp = value; OnValueChanged?.Invoke(this, null); } }
        /// <summary>
        /// Default: true
        /// </summary>
        public bool IsDepthClipEnabled { get { return description.IsDepthClipEnabled; } set { description.IsDepthClipEnabled = value; OnValueChanged?.Invoke(this, null); } }
        /// <summary>
        /// Default: Solid
        /// </summary>
        public FillMode FillMode
        {
            get { return description.FillMode; }
            set { description.FillMode = value; OnValueChanged?.Invoke(this, null); }
        }
        /// <summary>
        /// Default: false
        /// </summary>
        public bool IsFrontCounterClockwise { get { return description.IsFrontCounterClockwise; } set { description.IsFrontCounterClockwise = value; OnValueChanged?.Invoke(this, null); } }
        /// <summary>
        /// Default: false
        /// </summary>
        public bool IsMultisampleEnabled { get { return description.IsMultisampleEnabled; } set { description.IsMultisampleEnabled = value; OnValueChanged?.Invoke(this, null); } }
        /// <summary>
        /// Default: false
        /// </summary>
        public bool IsScissorEnabled { get { return description.IsScissorEnabled; } set { description.IsScissorEnabled = value; OnValueChanged?.Invoke(this, null); } }
        /// <summary>
        /// Default: 0.0
        /// </summary>
        public float SlopeScaledDepthBias { get { return description.SlopeScaledDepthBias; } set { description.SlopeScaledDepthBias = value; OnValueChanged?.Invoke(this, null); } }
        /// <summary>
        /// Get the rasterizer state.
        /// </summary>
        public RasterizerState RasterizerState
        {
            get
            {
                if (rasterizerState == null)
                {
                    Rasterizer_OnChanged(this, null);
                    UpdateState();
                }
                return rasterizerState;
            }
        }

        /// <inheritdoc/>
        public bool IsUpdated { get; set; }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public Rasterizer(GraphicsRenderer renderer)
        {
            this.renderer = renderer;
            this.OnValueChanged += Rasterizer_OnChanged;
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

        private void Rasterizer_OnChanged(object sender, EventArgs e)
        {
            IsUpdated = false;
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsUpdated)
            {
                rasterizerState = new RasterizerState(renderer.Device, description);
                IsUpdated = true;
                OnUpdated?.Invoke(this, null);
            }

            return IsUpdated;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Utilities.Dispose(ref rasterizerState);
        }
    }
}
