#define VERSION_11

using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using SharpDX.DXGI;
using Device11 = SharpDX.Direct3D11.Device;
using System.Diagnostics;
using PoncheToolkit.Util;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Util.Exceptions;
using System.Reflection;
using PoncheToolkit.Graphics3D;
using System.Collections.Generic;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Core.Management.Input;

namespace PoncheToolkit.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Class that wrap the functionality of a game using DirectX 11.
    /// </summary>
    public class Game11 : Game
    {
        #region Fields
        private ContentManager contentManager;
        private InputManager inputManager;
        private DepthStencilState depthStencilState;
        private DepthStencilView depthStencilView;
        private RenderTargetView renderTargetView;
        private Texture2D backBuffer;
        private Texture2D depthBuffer;
        private Adapter1 adapter;
        private Rational refreshRate;
        private GraphicsRenderer renderer;
        private List<Viewport> viewports;
        #endregion

        #region Properties
        /// <summary>
        /// Get the content manager to load all kind of content into memory.
        /// </summary>
        public ContentManager ContentManager
        {
            get { return contentManager; }
        }

        /// <summary>
        /// Get the input manager that manage all the inputs of the game.
        /// </summary>
        public InputManager InputManager
        {
            get { return InputManager; }
        }

        /// <summary>
        /// The render view where the back buffer is set to render its contents.
        /// </summary>
        public RenderTargetView RenderTargetView
        {
            get { return renderTargetView; }
        }

        /// <summary>
        /// The Depth Stencil View.
        /// </summary>
        public DepthStencilView DepthStencilView
        {
            get { return depthStencilView; }
        }

        /// <summary>
        /// The Depth Stencil State.
        /// </summary>
        public DepthStencilState DepthStencilState
        {
            get { return depthStencilState; }
        }

        /// <summary>
        /// The back buffer.
        /// </summary>
        public Texture2D BackBuffer
        {
            get { return backBuffer; }
        }

        /// <summary>
        /// The depth buffer.
        /// </summary>
        public Texture2D DepthBuffer
        {
            get { return depthBuffer; }
        }

        /// <summary>
        /// Get the Main renderer. Contain the Device and context to render.
        /// </summary>
        public GraphicsRenderer Renderer
        {
            get { return renderer; }
        }

        /// <summary>
        /// The description of the adapter used.
        /// </summary>
        public AdapterDescription SystemDescription
        {
            get { return renderer.SwapChain.GetParent<Factory2>().GetAdapter1(0).Description; }
        }

        /// <summary>
        /// The refresh rate of the first monitor used.
        /// </summary>
        public Rational RefreshRate
        {
            get { return refreshRate; }
        }

        /// <summary>
        /// A list of viewports, if it a single player game, there is always going to be just 1.
        /// If a split-screen game wants to be implemented, here add more viewports.
        /// </summary>
        public List<Viewport> Viewports
        {
            get { return viewports; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// De-constructor. Dispose all resources.
        /// </summary>
        ~Game11()
        {
            this.Dispose();
        }

        /// <summary>
        /// Instantiates the objects needed.
        /// </summary>
        public Game11()
            : this(new GameSettings())
        {
        }

        /// <summary>
        /// Instantiates the objects needed.
        /// </summary>
        public Game11(GameSettings settings)
            : base(settings)
        {
            Log.Info("Using DirectX11\n");
            Log.Info("| Settings:");
            foreach (PropertyInfo prop in settings.GetType().GetProperties())
            {
                Log.Info("| {0}: {1}", prop.Name, prop.GetValue(settings));
            }

            viewports = new List<Viewport>();
            renderer = new GraphicsRenderer(this);
            contentManager = new ContentManager(this);
            inputManager = new InputManager(this);

            Services.AddService(contentManager);
            Services.AddService(inputManager);
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <inheritdoc/>
        public override void CreateSwapDescriptionAndDevice()
        {
            // Create the main device
            try
            {
                Factory1 fact = new Factory1();
                Adapter1 adapter = fact.GetAdapter1(0);
                Utilities.Dispose(ref fact);

                // Get the refresh rate of the active monitor.
                ModeDescription[] descriptionModes = adapter.GetOutput(0).GetDisplayModeList(Format.R8G8B8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);
                foreach (ModeDescription desc in descriptionModes)
                {
                    if (desc.Width == Form.Width && desc.Height == Form.Height)
                        refreshRate = desc.RefreshRate;
                }
                Utilities.Dispose(ref adapter);

                // Create the Device and the SwapChain.
                SwapChainDescription swapDescription = new SwapChainDescription()
                {
                    BufferCount = 1,
                    ModeDescription = new ModeDescription(Form.ClientSize.Width, Form.ClientSize.Height, refreshRate, Format.R8G8B8A8_UNorm),
                    IsWindowed = !Settings.Fullscreen,
                    OutputHandle = Form.Handle,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,
                    Usage = Usage.RenderTargetOutput
                };
                Device11.CreateWithSwapChain(Settings.DeviceDriverType, Settings.DeviceCreationFlags, swapDescription, out Renderer.Device, out Renderer.SwapChain);
                Renderer.DeviceContext = Renderer.Device.ImmediateContext;
            }
            catch (Exception ex)
            {
                string error = string.Format("Failed to create Device for the Driver {0}", Settings.DeviceDriverType.ToString());
                Log.Error(error, ex);
                throw new DeviceCreationException(error, ex);
            }
        }

        /// <inheritdoc/>
        public override void PostDeviceInitialization()
        {
            // Ignore all windows events
            Factory2 factory = Renderer.SwapChain.GetParent<Factory2>();
            factory.MakeWindowAssociation(Form.Handle, WindowAssociationFlags.IgnoreAll);
            Utilities.Dispose(ref factory);

            // New RenderTargetView from the backbuffer
            backBuffer = Renderer.SwapChain.GetBackBuffer<Texture2D>(0);
            renderTargetView = new RenderTargetView(Renderer.Device, backBuffer);
            
            viewports.Add(new Viewport(0, 0, Form.ClientSize.Width, Form.ClientSize.Height, 0.0f, 1.0f));

            renderer.Rasterizer.FillMode = FillMode.Solid;
            renderer.Rasterizer.CullMode = CullMode.Back;
            renderer.DeviceContext.Rasterizer.State = renderer.Rasterizer.RasterizerState;
        }

        /// <inheritdoc/>
        public override void Update()
        {
            // Update all the game services.
            foreach (IGameService serv in Services.Values)
                serv.Update();

            // Update the screen manager.
            ScreenManager.Update();
        }

        /// <inheritdoc/>
        public override void Render()
        {
            renderer.UpdateState();
            renderer.BeginRender(renderTargetView);
            
            // Render the screens. This manager is the base to render all the content.
            // The Engine is based on screens.
            ScreenManager.Render();

            renderer.EndRender(SyncIntervalParameter);
        }
        #endregion

        #region Events
        /// <inheritdoc/>
        protected override void WindowResizedEvent(object sender, EventArgs e)
        {
            if (sender != null)
                Log.Debug("Resizing window to: {0}", (sender as SharpDX.Windows.RenderForm)?.ClientSize);

            // Dispose all previous allocated resources
            Utilities.Dispose(ref backBuffer);
            Utilities.Dispose(ref renderTargetView);
            Utilities.Dispose(ref depthBuffer);
            Utilities.Dispose(ref depthStencilView);

            // Resize the backbuffer
            Renderer.SwapChain.ResizeBuffers(Renderer.SwapChain.Description.BufferCount, Form.ClientSize.Width, Form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            backBuffer = Renderer.SwapChain.GetBackBuffer<Texture2D>(0);

            // Renderview on the backbuffer
            renderTargetView = new RenderTargetView(Renderer.Device, backBuffer);

            // Create the depth buffer
            depthBuffer = new Texture2D(Renderer.Device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = Form.ClientSize.Width,
                Height = Form.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            // Create the depth buffer view
            depthStencilView = new DepthStencilView(Renderer.Device, depthBuffer);

            // Initialize the Viewport.
            viewports[0] = new Viewport(0, 0, Form.ClientSize.Width, Form.ClientSize.Height, 0.0f, 1.0f);

            // Setup targets and viewport for rendering
            Renderer.DeviceContext.Rasterizer.SetViewport(viewports[0]);
            Renderer.DeviceContext.OutputMerger.SetTargets(depthStencilView, renderTargetView);
            //Renderer.DeviceContext.OutputMerger.SetTargets(renderTargetView);

            //// Initialize a StaticCamera by default.
            //camera = new StaticCamera(viewport);
            //// Reinitialize the Camera matrices.
            //camera.Initialize();
        }
        #endregion

        #region IDisposable
        /// <inheritdoc/>
        public override void Dispose()
        {
            UnloadContent();

            // Dispose the game services.
            foreach (IGameService serv in Services.Values)
                serv.Dispose();

            Utilities.Dispose(ref renderer);
            Utilities.Dispose(ref backBuffer);
            Utilities.Dispose(ref renderTargetView);
            Utilities.Dispose(ref depthBuffer);
            Utilities.Dispose(ref depthStencilView);
            Utilities.Dispose(ref adapter);
        }
        #endregion
    }
}
