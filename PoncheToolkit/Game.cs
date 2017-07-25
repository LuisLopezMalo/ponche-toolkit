using System;
using SharpDX;
using Viewport11 = SharpDX.Viewport;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device11 = SharpDX.Direct3D11.Device;
using Device12 = SharpDX.Direct3D12.Device;
using System.Diagnostics;
using System.Drawing;
using PoncheToolkit.Util;

namespace PoncheToolkit
{
    /// <summary>
    /// Main class that wraps the functionality of SharpDX in four methods, called in the next order:
    /// - Initialize
    /// - LoadContent
    /// - Update
    /// - Draw
    /// </summary>
    public class Game : IDisposable
    {
        #region Fields
#if DEBUG
        private Stopwatch fpsTimer = new Stopwatch();
        public int FPS = 0;
#endif

        private string name;
        private string title;
        private bool isPaused;
        private bool hasFocus;
        private int syncIntervalParameter;
        private RenderForm form;
        private Size windowSize;
        private Device11 device;
        private DeviceContext context;
        private SwapChain swapChain;
        private SwapChainDescription swapDescription;
        private Texture2D backBuffer;
        private Texture2D depthBuffer;
        private RenderTargetView renderView;
        private DepthStencilView depthView;
        private Viewport11 viewport;
        //private Camera camera;
        private GameTime gameTime;
        #endregion

        #region Properties
        /// <summary>
        /// Name of the Game.
        /// </summary>
        public new string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Title to be shown in the game's title bar.
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// Used to stop updating the main loop.
        /// </summary>
        public bool IsPaused
        {
            get { return isPaused; }
            set { isPaused = value; }
        }

        /// <summary>
        /// Gets if the current window of the game has focus.
        /// If it has no focus, the game stops updating.
        /// </summary>
        public bool HasFocus
        {
            get { return hasFocus; }
        }

        /// <summary>
        /// Gets or Sets if the game enables the sync with the refresh rate of the monitor. (60 Htz = 60 fps)
        /// If true it sets a 1 in the first parameter from the Present method inside Draw.
        /// Else it sets a 0.
        /// </summary>
        public bool VerticalSyncEnabled
        {
            get { return syncIntervalParameter == 1 ? true : false; }
            set
            {
                if (value)
                    syncIntervalParameter = 1;
                else
                    syncIntervalParameter = 0;
            }
        }

        /// <summary>
        /// Size of the Form when not in Fullscreen.
        /// </summary>
        public Size WindowSize
        {
            get { return windowSize; }
            set { windowSize = value; }
        }

        /// <summary>
        /// Main Device to keep a reference for drawing stuff.
        /// </summary>
        public Device11 Device
        {
            get { return device; }
            set { device = value; }
        }

        /// <summary>
        /// The Immediate Context to draw on screen.
        /// </summary>
        public DeviceContext Context
        {
            get { return context; }
            set { context = value; }
        }

        /// <summary>
        /// Buffers swap chain.
        /// </summary>
        public SwapChain SwapChain
        {
            get { return swapChain; }
            set { swapChain = value; }
        }

        /// <summary>
        /// The default viewport using all the client size dimensions.
        /// </summary>
        public Viewport11 Viewport
        {
            get { return viewport; }
            set { viewport = value; }
        }

        ///// <summary>
        ///// Measure all the elapsed time from the game.
        ///// </summary>
        //public GameTime GameTime
        //{
        //    get { return gameTime; }
        //    set { gameTime = value; }
        //}
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public Game()
        {

        }

        /// <summary>
        /// Initializes all the basic component to run a windowed game.
        /// If overriden, base.Initialize() must be called first.
        /// Sets default window properties.
        /// Size: 1280x720
        /// </summary>
        public virtual void Initialize()
        {
            if (windowSize == null || (windowSize.Height == 0 && windowSize.Width == 0))
                windowSize = new Size(1280, 720);

            if (form == null)
                form = new RenderForm("PonchoToolkit Default Game");
            form.Size = windowSize;

            // SwapChain description
            swapDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            DeviceCreationFlags creationFlags = DeviceCreationFlags.None;
#if DEBUG
            creationFlags |= DeviceCreationFlags.Debug;
#endif
            // Create the main device
            Device11.CreateWithSwapChain(DriverType.Hardware, creationFlags, swapDescription, out device, out swapChain);
            context = device.ImmediateContext;

            // Ignore all windows events
            Factory2 factory = swapChain.GetParent<Factory2>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            renderView = new RenderTargetView(device, backBuffer);

            //context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            //context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, 32, 0));
            context.Rasterizer.SetViewport(new Viewport11(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f));
            context.OutputMerger.SetTargets(renderView);

            // Call the window resized event at the beginning to setup the initial properties.
            WindowResizedEvent(null, null);

            // Starts measuring the game time.
            gameTime = new GameTime();
            gameTime.Start();

            form.UserResized += WindowResizedEvent;
            form.LostFocus += (sender, e) =>
            {
                hasFocus = false;
            };

            form.GotFocus += (sender, e) =>
            {
                hasFocus = true;
            };
        }

        /// <summary>
        /// Must be overriden to load the assets and all content.
        /// </summary>
        public virtual void LoadContent()
        {

        }

        /// <summary>
        /// This method just dispose and release managed and unmanaged resources.
        /// </summary>
        public virtual void UnloadContent()
        {
            Dispose();
        }

        /// <summary>
        /// Main method to update the game logic.
        /// This method is called inside the MainLoop.
        /// </summary>
        public virtual void Update()
        {
            // Update game time for each frame.
            gameTime.Update();

            //camera.Update(gameTime);
        }

        /// <summary>
        /// Main method tu put everything that will be rendered.
        /// This method is called inside the MainLoop.
        /// The base.Draw() must be called last.
        /// </summary>
        public virtual void Draw()
        {
            //swapChain.Present(syncIntervalParameter, 0);
            //context.ClearRenderTargetView(renderView, Color.Black);
            //for (int i = 0; i < technique.Description.PassCount; ++i)
            //{
            //    pass.Apply(context);
            //    context.Draw(3, 0);
            //}

            swapChain.Present(syncIntervalParameter, PresentFlags.None);
        }

        /// <summary>
        /// Main method that must be called to start the game's loop.
        /// </summary>
        public virtual void Run()
        {
            Initialize();
            LoadContent();

            // For Debugging
#if DEBUG
            fpsTimer.Start();
#endif

            // Start the main loop.
            //RenderLoop.RenderCallback renderCallback = new RenderLoop.RenderCallback(MainLoop);
            //RenderLoop.Run(form, renderCallback);

            form.Show();
            RenderLoop loop = new RenderLoop(form);
            while (loop.NextFrame())
            {
                mainLoop();
            }
        }

        /// <summary>
        /// The main loop where Update and Draw methods are called.
        /// </summary>
        private void mainLoop()
        {
            //If window is out of focus, the timer stops and no more updates nor draws are made.
            if (!hasFocus)
            {
                if (gameTime.IsRunning)
                    gameTime.Stop();
                return;
            }

            if (!gameTime.IsRunning)
                gameTime.Start();

            // Counts the FPS just for debugging purposes.
#if DEBUG
            if (fpsTimer.Elapsed.TotalSeconds >= TimeSpan.FromSeconds(1).TotalSeconds)
            {
                form.Text = "FPS: " + FPS;
                fpsTimer.Restart();
                FPS = 0;
            }

            FPS++;
#endif
            Update();
            Draw();
        }

        #region Events
        /// <summary>
        /// Event raised when the user has resized the Client Window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void WindowResizedEvent(object sender, EventArgs e)
        {
            //// Dispose all previous allocated resources
            //Utilities.Dispose(ref backBuffer);
            //Utilities.Dispose(ref renderView);
            //Utilities.Dispose(ref depthBuffer);
            //Utilities.Dispose(ref depthView);

            //// Resize the backbuffer
            //swapChain.ResizeBuffers(swapDescription.BufferCount, form.ClientSize.Width, form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

            //// Get the backbuffer from the swapchain
            //backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

            //// Renderview on the backbuffer
            //renderView = new RenderTargetView(device, backBuffer);

            //// Create the depth buffer
            //depthBuffer = new Texture2D(device, new Texture2DDescription()
            //{
            //    Format = Format.D32_Float_S8X24_UInt,
            //    ArraySize = 1,
            //    MipLevels = 1,
            //    Width = form.ClientSize.Width,
            //    Height = form.ClientSize.Height,
            //    SampleDescription = new SampleDescription(1, 0),
            //    Usage = ResourceUsage.Default,
            //    BindFlags = BindFlags.DepthStencil,
            //    CpuAccessFlags = CpuAccessFlags.None,
            //    OptionFlags = ResourceOptionFlags.None
            //});

            //// Create the depth buffer view
            //depthView = new DepthStencilView(device, depthBuffer);

            //// Initialize the Viewport.
            //viewport = new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f);

            //// Initialize a StaticCamera by default.
            //camera = new StaticCamera(viewport);

            //// Setup targets and viewport for rendering
            //context.Rasterizer.SetViewport(viewport);
            //context.OutputMerger.SetTargets(depthView, renderView);

            //// Reinitialize the Camera matrices.
            //camera.Initialize();
        }


        #endregion

        #region IDisposable
        public void Dispose()
        {
#if DEBUG
            fpsTimer.Stop();
            fpsTimer = null;
#endif
        }
        #endregion
    }
}
