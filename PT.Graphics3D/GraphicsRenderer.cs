using System;
using PoncheToolkit.Core;
using SharpDX;
using SharpDX.DXGI;
using PoncheToolkit.Util;

namespace PT.Graphics3D
{
    using Core.Management.Content;
    using Graphics2D;

    /// <summary>
    /// Class that wrap the functionality to render content.
    /// </summary>
    public abstract class GraphicsRenderer : UpdatableStateObject, IInitializable, IContentLoadable
    {
        /// <summary>
        /// Set the type of rendering used.
        /// By default the <see cref="ProcessRenderingMode.Immediate"/> rendering will be used.
        /// The <see cref="ProcessRenderingMode.MultiThread"/> rendering can be used - if the gpu driver supports it - so
        /// the Engine group the meshes to be rendered and use different threads and 
        /// <see cref="SharpDX.Direct3D11.CommandList"/> or <see cref="SharpDX.Direct3D12.CommandList"/> to send info to the gpu.
        /// </summary>
        public enum ProcessRenderingMode
        {
            /// <summary>
            /// Use only one thread for rendering.
            /// </summary>
            Immediate,
            /// <summary>
            /// Use multiple threads for rendering.
            /// </summary>
            MultiThread
        }

        /// <summary>
        /// The rendering technique for the entire Engine.
        /// Define the algorithm used to render dynamic lights, shadowing, etc.
        /// </summary>
        public enum ShadingRenderingMode
        {
            /// <summary>
            /// Basic implementation for rendering dynamic lights.
            /// It just loop through all the lights to be applied to all materials.
            /// <para>
            /// This is the slowest method. In modern hardware, more less 12 dynamic lights can be rendered at the same time.
            /// </para>
            /// </summary>
            ForwardShading,
            /// <summary>
            /// This method is an evolution of the TiledShading, so this converts the tiles (2D) to clusters (3D) so the depth is
            /// inherently incorporated and the depth discontinuities are solved in a better way. Also when using many lights (thousands)
            /// it performs better.
            /// <para>
            /// The only con it has, is the creation of the clusters, but even this calculation is faster thatn the "Z-prepass" needed for tiledShading.
            /// It supports like 10,000 dynamic lights and it can be optimized more in the future.
            /// </para>
            /// </summary>
            ClusteredForwardShading,
            /// <summary>
            /// This will be used if a custom propietary algorithm wants to be used.
            /// </summary>
            Custom1,
            /// <summary>
            /// This will be used if a custom propietary algorithm wants to be used.
            /// </summary>
            Custom2,
            /// <summary>
            /// This will be used if a custom propietary algorithm wants to be used.
            /// </summary>
            Custom3
        }

        #region Fields
        private Game game;
        private PTRasterizer rasterizer;
        private SpriteBatch spriteBatch;
        private Size2F dpi;
        private SharpDX.Direct2D1.DeviceContext context2D;
        private SharpDX.Direct2D1.BitmapProperties1 bitmapProperties2D;
        private ProcessRenderingMode processRenderMode;
        private ShadingRenderingMode shadingRenderMode;
        #endregion

        #region Properties
        /// <summary>
        /// The Swap Chain to render.
        /// </summary>
        public SwapChain1 SwapChain;

        /// <inheritdoc/>
        public virtual event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        /// <inheritdoc/>
        public virtual event EventHandlers.OnInitializedHandler OnInitialized;

        ///// <summary>
        ///// The current <see cref="SharpDX.Direct2D1.DeviceContext"/> used as render target.
        ///// By default it is the main <see cref="Context2D"/> that uses the back buffer for rendering.
        ///// </summary>
        //public SharpDX.Direct2D1.DeviceContext CurrentRenderTarget2D
        //{
        //    get { return currentRenderTarget2D; }
        //    set { SetProperty(ref currentRenderTarget2D, value); }
        //}

        /// <summary>
        /// The <see cref="SharpDX.Direct2D1.DeviceContext"/> context used to render the 2D content.
        /// </summary>
        public SharpDX.Direct2D1.DeviceContext Context2D
        {
            get { return context2D; }
            set { SetProperty(ref context2D, value); }
        }

        /// <summary>
        /// The rasterizer with properties to how the polygons are drawn.
        /// </summary>
        public PTRasterizer Rasterizer
        {
            get { return rasterizer; }
            set { SetPropertyAsDirty(ref rasterizer, value); }
        }

        /// <summary>
        /// Get the <see cref="PoncheToolkit.Graphics2D.SpriteBatch"/> object.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
            internal set { SetProperty(ref spriteBatch, value); }
        }

        /// <summary>
        /// Get or set the rendering type.
        /// See <see cref="ProcessRenderingMode"/> for more information. Set as dirty.
        /// Default: <see cref="ProcessRenderingMode.Immediate"/>
        /// </summary>
        public ProcessRenderingMode ProcessRenderMode
        {
            get { return processRenderMode; }
            set { SetPropertyAsDirty(ref processRenderMode, value); }
        }

        /// <summary>
        /// Get or set the rendering type.
        /// See <see cref="ShadingRenderingMode"/> for more information. Set as dirty.
        /// Default: <see cref="ShadingRenderingMode.ClusteredForwardShading"/>
        /// </summary>
        public ShadingRenderingMode ShadingRenderMode
        {
            get { return shadingRenderMode; }
            set { SetPropertyAsDirty(ref shadingRenderMode, value); }
        }

        /// <summary>
        /// The BitmapProperties to initialize the 2D render target.
        /// </summary>
        public SharpDX.Direct2D1.BitmapProperties1 BitmapProperties2D
        {
            get { return bitmapProperties2D; }
            internal set { SetProperty(ref bitmapProperties2D, value); }
        }

        /// <summary>
        /// The dpi of the monitor.
        /// </summary>
        public Size2F Dpi
        {
            get { return dpi; }
            internal set { SetProperty(ref dpi, value); }
        }

        /// <summary>
        /// Get the game instance.
        /// </summary>
        public Game Game
        {
            get { return game; }
        }

        /// <inheritdoc/>
        public bool IsContentLoaded { get; set; }

        /// <inheritdoc/>
        public bool IsInitialized { get; set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public GraphicsRenderer(Game game)
        {
            this.game = game;
            this.processRenderMode = ProcessRenderingMode.Immediate;
            this.shadingRenderMode = ShadingRenderingMode.ForwardShading;
        }

        /// <inheritdoc/>
        public abstract void Initialize();

        /// <inheritdoc/>
        public abstract void LoadContent(ContentManager contentManager);

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Utilities.Dispose(ref rasterizer);
        }
        #endregion

    }
}
