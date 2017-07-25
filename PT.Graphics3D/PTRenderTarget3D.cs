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

namespace PT.Graphics3D
{
    /// <summary>
    /// Simplified Render target to be used to render any scene to a <see cref="Texture2D"/>
    /// </summary>
    public class PTRenderTarget3D : UpdatableStateObject, IInitializable
    {
        private SharpDX.Direct2D1.Bitmap1 bitmap;
        private PTTexture2D texture;
        private Game11 game;
        private RenderTargetView renderTarget;

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
        /// The <see cref="RenderTargetView"/> where the rendering takes place.
        /// </summary>
        public RenderTargetView RenderTarget
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
        /// <param name="game"></param>
        public PTRenderTarget3D(Game11 game)
        {
            this.game = game;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            // Create the reflection Render Target view.
            Texture2DDescription desc = new Texture2DDescription();
            desc.Width = game.Settings.Resolution.Width;
            desc.Height = game.Settings.Resolution.Height;
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            //desc.Usage = ResourceUsage.Default;
            //desc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            desc.Format = SharpDX.DXGI.Format.R32G32B32A32_Float;
            //desc.Format = Game.BackBuffer.Description.Format;
            desc.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            //desc.OptionFlags = ResourceOptionFlags.Shared;

            // Load the texture through the ContentManager so it is disposed correctly.
            Texture2D localTexture = new Texture2D(game.Renderer.Device, desc);
            this.Texture = game.ContentManager.LoadTexture2D("Render Target Texture", localTexture);

            RenderTargetViewDescription viewDesc = new RenderTargetViewDescription();
            viewDesc.Format = desc.Format;
            viewDesc.Dimension = RenderTargetViewDimension.Texture2D;
            viewDesc.Texture2D.MipSlice = 0;
            renderTarget = new RenderTargetView(game.Renderer.Device, localTexture, viewDesc);
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
                Surface surface = texture.Texture.QueryInterface<Surface>();
                Bitmap = new SharpDX.Direct2D1.Bitmap1(game.Renderer.Context2D, surface);
                Utilities.Dispose(ref surface);

                RenderTargetViewDescription viewDesc = new RenderTargetViewDescription();
                viewDesc.Format = texture.Texture.Description.Format;
                viewDesc.Dimension = RenderTargetViewDimension.Texture2D;
                viewDesc.Texture2D.MipSlice = 0;
                renderTarget = new RenderTargetView(game.Renderer.Device, texture.Texture, viewDesc);
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
    }
}
