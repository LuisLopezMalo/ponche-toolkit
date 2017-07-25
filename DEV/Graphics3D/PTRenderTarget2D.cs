using PoncheToolkit.Core;
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Simplified Render target to be used to render any scene to a <see cref="Texture2D"/>
    /// </summary>
    public class PTRenderTarget2D : UpdatableStateObject, IInitializable
    {
        private SharpDX.Direct2D1.Bitmap1 bitmap;
        private PTTexture2D texture;
        private RenderTargetView renderTarget;
#if DX11
        private IGraphicsRenderer11 renderer;
#elif DX12
        private IGraphicsRenderer12 renderer;
#endif
        private string name;

        /// <summary>
        /// Simple name to identif the render target.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        /// <summary>
        /// The bitmap reference of the rendered content.
        /// This can be used to be rendered as 2D using the <see cref="SpriteBatch"/> object.
        /// </summary>
        public SharpDX.Direct2D1.Bitmap1 Bitmap
        {
            get { return bitmap; }
            set { SetProperty(ref bitmap, value); }
        }

        /// <summary>
        /// The <see cref="PTTexture2D"/> texture to be used as target.
        /// </summary>
        public PTTexture2D Texture
        {
            get { return texture; }
            set { SetPropertyAsDirty(ref texture, value); }
        }

        /// <summary>
        /// The <see cref="SharpDX.Direct3D11.RenderTargetView"/> where the rendering takes place.
        /// </summary>
        public RenderTargetView RenderTargetView
        {
            get { return renderTarget; }
            internal set { SetPropertyAsDirty(ref renderTarget, value); }
        }

        /// <inheritdoc/>
        public bool IsInitialized { get; set; }

        /// <inheritdoc/>
        public event EventHandlers.OnInitializedHandler OnInitialized;


        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="renderer">The <see cref="GraphicsRenderer"/> renderer used. </param>
        /// <param name="name">The <see cref="GraphicsRenderer"/> Name to identify the render target.</param>
        //public PTRenderTarget2D(GraphicsRenderer renderer, string name)   TODO: modified
        public PTRenderTarget2D(IGraphicsRenderer renderer, string name)
        {
#if DX11
            this.renderer = renderer as IGraphicsRenderer11;
#elif DX12
            this.renderer = renderer as IGraphicsRenderer12;
#endif
            this.name = name;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
#if DX11
            IGraphicsRenderer11 renderer11 = this.renderer as IGraphicsRenderer11;
            // Create the reflection Render Target view.
            Texture2DDescription desc = new Texture2DDescription();
            desc.Width = renderer.Game.Settings.Resolution.Width;
            desc.Height = renderer.Game.Settings.Resolution.Height;
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            //desc.Usage = ResourceUsage.Default;
            desc.Format = renderer.Game.Settings.GlobalRenderTargetsFormat;
            //desc.Format = SharpDX.DXGI.Format.R32G32B32A32_Float;
            //desc.Format = Game.BackBuffer.Description.Format;
            desc.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            //desc.OptionFlags = ResourceOptionFlags.Shared;

            // Load the texture through the ContentManager so it is disposed correctly.
            Texture2D localTexture = new Texture2D(renderer11.Device, desc);
            this.Texture = renderer.Game.ContentManager.LoadTexture2D("RTTexture" + Guid.NewGuid(), localTexture);

            RenderTargetViewDescription viewDesc = new RenderTargetViewDescription();
            viewDesc.Format = desc.Format;
            viewDesc.Dimension = RenderTargetViewDimension.Texture2D;
            viewDesc.Texture2D.MipSlice = 0;
            renderTarget = new RenderTargetView(renderer11.Device, localTexture, viewDesc);
            //reflectionRenderTargetView = new RenderTargetView(Game.Renderer.Device, reflectionTexture.Texture, viewDesc);

            ShaderResourceViewDescription shaderDesc = new ShaderResourceViewDescription();
            shaderDesc.Format = desc.Format;
            shaderDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
            shaderDesc.Texture2D.MostDetailedMip = 0;
            shaderDesc.Texture2D.MipLevels = 1;
            this.Texture.ShaderResourceView = new ShaderResourceView(localTexture.Device, localTexture, shaderDesc);

            //// Create the Reflection Depth stencil view.
            //var depthTexDesc = new Texture2DDescription()
            //{
            //    Width = game.Settings.Resolution.Width,
            //    Height = game.Settings.Resolution.Height,
            //    MipLevels = 1,
            //    ArraySize = 1,
            //    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            //    Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
            //    Usage = ResourceUsage.Default,
            //    BindFlags = BindFlags.DepthStencil,
            //    CpuAccessFlags = CpuAccessFlags.None,
            //    OptionFlags = ResourceOptionFlags.None
            //};
            //var depthTex = new Texture2D(game.Renderer.Device, depthTexDesc);
            //var depthDesc = new DepthStencilViewDescription()
            //{
            //    Format = depthTexDesc.Format,
            //    //Flags = DepthStencilViewFlags.None,
            //    Dimension = DepthStencilViewDimension.Texture2D
            //};
            //DepthStencilView reflectionDepthStencil = new DepthStencilView(game.Renderer.Device, depthTex, depthDesc);
#elif DX12
            throw new NotImplementedException("DX12 not yet implemented");
#endif
            IsInitialized = true;
            OnInitialized?.Invoke();
        }
        #endregion

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (IsStateUpdated)
                return IsStateUpdated;

            if (DirtyProperties.ContainsKey(nameof(Texture)))
            {
#if DX11
                Surface surface = texture.Texture.QueryInterface<Surface>();
                Bitmap = new SharpDX.Direct2D1.Bitmap1(renderer.Context2D, surface);
                Utilities.Dispose(ref surface);

                RenderTargetViewDescription viewDesc = new RenderTargetViewDescription();
                viewDesc.Format = texture.Texture.Description.Format;
                viewDesc.Dimension = RenderTargetViewDimension.Texture2D;
                viewDesc.Texture2D.MipSlice = 0;
                renderTarget = new RenderTargetView(renderer.Device, texture.Texture, viewDesc);

                Texture.Bitmap = Bitmap;
#elif DX12
                throw new NotImplementedException("DX12 not yet implemented");
#endif
            }

            IsStateUpdated = true;
            OnStateUpdated();
            return IsStateUpdated;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            texture.Dispose();
            Utilities.Dispose(ref renderTarget);
            Utilities.Dispose(ref bitmap);
            base.Dispose();
        }

        /// <summary>
        /// Implicit operator that returns the <see cref="SharpDX.Direct2D1.Bitmap1"/> property from the render target.
        /// </summary>
        /// <param name="renderTarget"></param>
        public static implicit operator SharpDX.Direct2D1.Bitmap1(PTRenderTarget2D renderTarget)
        {
            return renderTarget.Bitmap;
        }
    }
}
