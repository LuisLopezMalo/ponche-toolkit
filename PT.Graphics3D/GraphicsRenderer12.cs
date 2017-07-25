using System;
using PoncheToolkit.Core;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using PoncheToolkit.Util;

namespace PT.Graphics3D
{
    using Core.Management.Content;
    using Core.Management.Screen;
    using Effects;
    using Graphics2D;
    using SharpDX.Direct2D1;

    /// <summary>
    /// Class that wrap the functionality to render content.
    /// </summary>
    public class GraphicsRenderer12 : UpdatableStateObject, IInitializable, IContentLoadable
    {
        #region Fields
        private Game game;
        private SharpDX.Direct2D1.DeviceContext d2dContext;
        private SpriteBatch spriteBatch;
        private Size2F dpi;
        private BitmapProperties1 bitmapProperties;
        #endregion

        #region Properties
        /// <summary>
        /// Instance of the device used.
        /// </summary>
        public SharpDX.Direct3D11.Device1 Device;

        /// <summary>
        /// The Swap Chain to render.
        /// </summary>
        public SwapChain1 SwapChain;

        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        /// <inheritdoc/>
        public event EventHandlers.OnInitializedHandler OnInitialized;

        /// <summary>
        /// The <see cref="SharpDX.Direct2D1.DeviceContext"/> context used to render the 2D content.
        /// </summary>
        public SharpDX.Direct2D1.DeviceContext Context2D
        {
            get { return d2dContext; }
            set { SetProperty(ref d2dContext, value); }
        }

        /// <summary>
        /// Get the <see cref="PoncheToolkit.Graphics2D.SpriteBatch"/> object.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        /// <summary>
        /// The BitmapProperties to initialize the 2D render target.
        /// </summary>
        public BitmapProperties1 BitmapProperties
        {
            get { return bitmapProperties; }
        }

        /// <summary>
        /// The dpi of the monitor.
        /// </summary>
        public Size2F Dpi
        {
            get { return dpi; }
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
        public GraphicsRenderer12(Game game)
        {
            this.game = game;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public void Initialize()
        {
            SharpDX.Direct2D1.Device d2dDevice = null;

            try
            {
                SharpDX.DXGI.Device1 dxgiDevice = this.Device.QueryInterface<SharpDX.DXGI.Device1>();
                SharpDX.Direct2D1.Factory2 d2dFactory2 = new SharpDX.Direct2D1.Factory2(FactoryType.MultiThreaded, DebugLevel.None);
                d2dDevice = new SharpDX.Direct2D1.Device1(d2dFactory2, dxgiDevice);
                
                dpi = d2dFactory2.DesktopDpi;
                bitmapProperties = new BitmapProperties1(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied),
                    dpi.Width, dpi.Height, BitmapOptions.CannotDraw | BitmapOptions.Target);
                Utilities.Dispose(ref d2dFactory2);
                Utilities.Dispose(ref dxgiDevice);
            }
            catch (Exception ex)
            {
                Log.Warning("Interface Factory2 not supported. Trying with Factory1", ex);
                SharpDX.DXGI.Device1 dxgiDevice = this.Device.QueryInterface<SharpDX.DXGI.Device1>();
                SharpDX.Direct2D1.Factory1 d2dFactory1 = new SharpDX.Direct2D1.Factory1(FactoryType.MultiThreaded, DebugLevel.Error);
                d2dDevice = new SharpDX.Direct2D1.Device(d2dFactory1, dxgiDevice);

                dpi = d2dFactory1.DesktopDpi;
                bitmapProperties = new BitmapProperties1(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied),
                    dpi.Width, dpi.Height, BitmapOptions.CannotDraw | BitmapOptions.Target);
                Utilities.Dispose(ref d2dFactory1);
                Utilities.Dispose(ref dxgiDevice);
            }

            d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, DeviceContextOptions.EnableMultithreadedOptimizations);
            d2dContext.PrimitiveBlend = PrimitiveBlend.SourceOver;
            d2dContext.AntialiasMode = AntialiasMode.Aliased;
            Utilities.Dispose(ref d2dDevice);

            spriteBatch.Initialize();

            ToDispose(Context2D);
            ToDispose(spriteBatch);

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public void LoadContent(ContentManager contentManager)
        {
            spriteBatch.LoadContent(contentManager);
            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Begin the render. 
        /// Clear the target view and the depth stencil.
        /// </summary>
        /// <param name="targetView"></param>
        /// <param name="depthStencilView"></param>
        /// <param name="clearColor"></param>
        public void BeginRender(RenderTargetView targetView, DepthStencilView depthStencilView, Color? clearColor = null)
        {
            if (clearColor == null)
                clearColor = new Color(70, 90, 105);
        }

        /// <summary>
        /// End render. Present the swap chain buffer.
        /// </summary>
        /// <param name="syncIntervalParameter"></param>
        public void EndRender(int syncIntervalParameter)
        {
            SwapChain.Present(syncIntervalParameter, PresentFlags.None);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                IsStateUpdated = true;
                OnStateUpdated();
            }
            return IsStateUpdated;
        }

        /// <summary>
        /// Add a model to the list of renderable models.
        /// The models here will be added to a single vertex buffer with just one
        /// Draw call.
        /// </summary>
        /// <param name="model"></param>
        public void AddRenderable(PTModel model)
        {
        }

        /// <summary>
        /// Render all the meshes from the <see cref="GameScreen.MeshesPerEffect"/> dictionary.
        /// It groups the rendering by effect so the graphics pipeline does not have to switch
        /// between shaders so often and save some bandwidth power using the <see cref="GameScreen.DrawableComponentsPerEffect"/> dictionary.
        /// </summary>
        public void RenderScreen(GameScreen screen, SpriteBatch spriteBatch)
        {
            // Iterate over the screen effects.
            foreach (Effects.PTForwardRenderEffect effect in screen.DrawableComponentsPerEffect.Keys)
            {
                effect.Apply(null);

                // Draw the meshes that contain that effect.
                // TODO: implement the perMaterial rendering. Right now it just has perEffect.
                foreach (PTMesh mesh in screen.MeshesPerEffect[effect])
                {
                    if (effect.Materials.Count <= 0)
                        throw new Util.Exceptions.RenderingException("The effect -{0}- used for the mesh -{1}- has no materials.", System.IO.Path.GetFileNameWithoutExtension(effect.ShaderPath), mesh.Name);

                    PTMaterial material = null;
                    if (mesh.MaterialIndex == -1 || string.IsNullOrEmpty(mesh.MaterialName))
                        material = effect.GetMaterial(PTMaterial.COMMON_WOOD_MATERIAL_KEY); // Applying default common wood material
                    else
                        material = effect.GetMaterial(mesh.MaterialName);

                    //material.Apply(effect, mesh);
                    mesh.Render(spriteBatch);
                }
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Utilities.Dispose(ref SwapChain);
            Utilities.Dispose(ref Device);
        }
        #endregion
    }
}
