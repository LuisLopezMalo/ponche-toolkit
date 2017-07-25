using PoncheToolkit.Graphics2D;
using PoncheToolkit.Graphics2D.Effects;
using PoncheToolkit.Graphics3D;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Main interface to implement to have a graphics renderer.
    /// This interface has properties that DirectX11 and DirectX12 share.
    /// </summary>
    ///// <typeparam name="GameClass"></typeparam>
    //public interface IGraphicsRenderer<GameClass> : IInitializable, IContentLoadable where GameClass : Game
    public interface IGraphicsRenderer : IInitializable, IContentLoadable
    {
        /// <summary>
        /// The game instance.
        /// </summary>
#if DX11
        Game11 Game { get; }
#elif DX12
        Game12 Game { get; }
#endif

        /// <summary>
        /// The <see cref="SharpDX.Direct2D1.DeviceContext"/> context used to render the 2D content.
        /// </summary>
        SharpDX.Direct2D1.DeviceContext Context2D { get; set; }

        /// <summary>
        /// The rasterizer with properties to how the polygons are drawn.
        /// </summary>
        PTRasterizer Rasterizer { get; set; }

        /// <summary>
        /// Get the <see cref="PoncheToolkit.Graphics2D.SpriteBatch"/> object.
        /// </summary>
        SpriteBatch SpriteBatch { get; }

        /// <summary>
        /// The <see cref="SharpDX.Direct2D1.Effect"/> effects to be applied to the back buffer before it is displayed.
        /// </summary>
        SortedList<int, PTCustomEffect> PostProcessEffects { get; set; }

        /// <summary>
        /// The main render target where the post processing is made.
        /// </summary>
        PTRenderTarget2D PostProcessRenderTarget { get; set; }

        /// <summary>
        /// Get or set the rendering type.
        /// See <see cref="ProcessRenderingMode"/> for more information. Set as dirty.
        /// Default: <see cref="ProcessRenderingMode.Immediate"/>
        /// </summary>
        ProcessRenderingMode ProcessRenderMode { get; set; }

        /// <summary>
        /// Get or set the rendering type.
        /// See <see cref="ShadingRenderingMode"/> for more information. Set as dirty.
        /// Default: <see cref="ShadingRenderingMode.ClusteredForwardShading"/>
        /// </summary>
        ShadingRenderingMode ShadingRenderMode { get; set; }

        /// <summary>
        /// The BitmapProperties to initialize the 2D render target.
        /// </summary>
        SharpDX.Direct2D1.BitmapProperties1 BitmapProperties2D { get; set; }

        /// <summary>
        /// The dpi of the monitor.
        /// </summary>
        Size2F Dpi { get; }

        #region Methods
        /// <summary>
        /// Add a new <see cref="SharpDX.Direct2D1.Effect"/> to the post process effects.
        /// This effects will be rendered to the final image obtained from the back buffer.
        /// </summary>
        /// <param name="order">The order in which the post process effects will be applied.</param>
        /// <param name="custom2DEffect">The effect to be added. This typically will be an effect that inherits from
        /// <see cref="PTCustomEffect"/> like the already implemented <see cref="PTEdgeDetectionEffect"/>.</param>
        void AddBackBufferPostProcessEffect(int order, PTCustomEffect custom2DEffect);

        /// <summary>
        /// Set the current 2D render target to be used by the <see cref="SpriteBatch"/> 
        /// </summary>
        void SetRenderTarget2D(SharpDX.Direct2D1.Image target);

        /// <summary>
        /// Set the current render target for 3D elements.
        /// If the target sent is null, the default RenderTarget is used.
        /// </summary>
        void SetRenderTarget3D(PTRenderTarget2D target);
        #endregion
    }
}