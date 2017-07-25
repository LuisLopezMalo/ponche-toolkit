using System;
using PoncheToolkit.Core;
using SharpDX;
using SharpDX.Direct3D12;
using PoncheToolkit.Util;
using SharpDX.Direct2D1;

namespace PoncheToolkit.Graphics3D
{
    using System.Collections.Generic;
    using Core.Management.Content;
    using Core.Management.Screen;
    using Effects;
    using Graphics2D;
    using Graphics2D.Effects;
    using SharpDX.DXGI;
    

    /// <summary>
    /// Class that wrap the functionality to render content.
    /// </summary>
    public class GraphicsRenderer12 : UpdatableStateObject, IGraphicsRenderer12
    {
        #region Fields

#if DX11
        private Game11 game;
#elif DX12
        private Game12 game;
#endif
        private SharpDX.Direct2D1.DeviceContext d2dContext;
        private SpriteBatch spriteBatch;
        private Size2F dpi;
        private BitmapProperties1 bitmapProperties;

        internal PTRenderTarget2D postProcessRenderTarget;
        #endregion

        #region Properties
        /// <summary>
        /// Instance of the device used.
        /// </summary>
        public SharpDX.Direct3D12.Device Device;

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

        /// <inheritdoc/>
#if DX11
        public Game11 Game
#elif DX12
        public Game12 Game
#endif
        {
            get { return game; }
        }
        
        /// <inheritdoc/>
        public PTRasterizer Rasterizer
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public SortedList<int, PTCustomEffect> PostProcessEffects
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public PTRenderTarget2D PostProcessRenderTarget
        {
            get { return postProcessRenderTarget; }
            set { SetPropertyAsDirty(ref postProcessRenderTarget, value); }
        }

        public ProcessRenderingMode ProcessRenderMode
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ShadingRenderingMode ShadingRenderMode
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public BitmapProperties1 BitmapProperties2D
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Handler used to call the OnEndRender event.
        /// </summary>
        public delegate void OnEndRenderHandler();
        /// <summary>
        /// Event fired inside the <see cref="EndRender(int)"/> method.
        /// This event can be used to retrieve the final image from the back buffer of from the
        /// post process render target.
        /// </summary>
        public event OnEndRenderHandler OnEndRender;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public GraphicsRenderer12(Game game)
        {
#if DX11
            this.game = game as Game11;
#elif DX12
            this.game = game as Game12;
#endif
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
                bitmapProperties = new BitmapProperties1(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied),
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
                bitmapProperties = new BitmapProperties1(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied),
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
        public void LoadContent(IContentManager contentManager)
        {
            spriteBatch.LoadContent(contentManager);
            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Begin the render. 
        /// Clear the target view and the depth stencil.
        /// </summary>
        /// <param name="clearColor"></param>
        public void BeginRender(Color? clearColor = null)
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
                        material = effect.GetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY); // Applying default common wood material
                    else
                        material = effect.GetMaterial(mesh.MaterialName);

                    //material.Apply(effect, mesh);
                    mesh.Render(spriteBatch);
                }
            }
        }

        /// <inheritdoc/>
        public void SetRenderTarget2D(SharpDX.Direct2D1.Image target)
        {
            if (target == null)
                Context2D.Target = SpriteBatch.Target;
            else
                Context2D.Target = target;
        }

        /// <inheritdoc/>
        public void SetRenderTarget3D(PTRenderTarget2D target)
        {
        }

        /// <inheritdoc/>
        public void AddBackBufferPostProcessEffect(int order, PTCustomEffect custom2DEffect)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Utilities.Dispose(ref Device);
        }
        #endregion
    }
}
