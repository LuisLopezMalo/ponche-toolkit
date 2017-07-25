using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Class that wrap the functionality to render content.
    /// </summary>
    public class GraphicsRenderer : UpdatableState, IDisposable
    {
        private Game11 game;

        /// <summary>
        /// Instance of the device used.
        /// </summary>
        public SharpDX.Direct3D11.Device Device;

        /// <summary>
        /// The graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice;

        /// <summary>
        /// The Device Context
        /// </summary>
        public DeviceContext DeviceContext;

        /// <summary>
        /// The Swap Chain to render.
        /// </summary>
        public SwapChain SwapChain;

        /// <summary>
        /// The rasterizer with properties to how the polygons are drawn.
        /// </summary>
        public Rasterizer Rasterizer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public GraphicsRenderer(Game11 game)
        {
            this.game = game;
            this.Rasterizer = new Rasterizer(this);
            Rasterizer.OnUpdated += Rasterizer_OnUpdated;
        }

        private void Rasterizer_OnUpdated(object sender, EventArgs e)
        {
            DeviceContext.Rasterizer.State = Rasterizer.RasterizerState;
        }

        /// <summary>
        /// Begin the render. Clear the target view.
        /// </summary>
        /// <param name="targetView"></param>
        public void BeginRender(RenderTargetView targetView)
        {
            DeviceContext.ClearRenderTargetView(targetView, Color.Black);
        }

        /// <summary>
        /// End render. Present the swap chain buffer.
        /// </summary>
        /// <param name="syncIntervalParameter"></param>
        public void EndRender(int syncIntervalParameter)
        {
            SwapChain.Present(syncIntervalParameter, PresentFlags.None);
        }

        /// <summary>
        /// Update the values of the renderer if something changed.
        /// </summary>
        public override bool UpdateState()
        {
            bool rastUpdated = Rasterizer.UpdateState();
            return rastUpdated;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Utilities.Dispose(ref SwapChain);
            Utilities.Dispose(ref DeviceContext);
            Utilities.Dispose(ref Device);
            Utilities.Dispose(ref GraphicsDevice);
            Utilities.Dispose(ref Rasterizer);
        }
    }
}
