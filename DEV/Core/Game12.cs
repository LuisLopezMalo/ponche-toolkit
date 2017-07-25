#define VERSION_12

using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device12 = SharpDX.Direct3D12.Device;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using PoncheToolkit.Util.Exceptions;
using PoncheToolkit.Graphics3D;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Class that wrap the functionality of a game using DirectX 12.
    /// </summary>
    public class Game12 : Game
    {
        #region Fields
        private Device12 device;
        private CommandQueue commands;
        #endregion

        #region Properties
        /// <summary>
        /// Main Device to keep a reference for drawing stuff.
        /// </summary>
        public Device12 Device
        {
            get { return device; }
            set { device = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// De-constructor. Dispose all resources.
        /// </summary>
        ~Game12()
        {
            this.Dispose();
        }

        /// <summary>
        /// Instantiates the objects needed.
        /// </summary>
        public Game12()
            : this(new GameSettings())
        {
        }

        /// <summary>
        /// Instantiates the objects needed.
        /// </summary>
        public Game12(GameSettings settings)
            : base(settings)
        {
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void CreateSwapDescriptionAndDevice()
        {
            // Create the main device
            try
            {
#if DEBUG
                // Enable the D3D12 debug layer.
                DebugInterface.Get().EnableDebugLayer();
#endif
                using (var fact = new Factory4())
                {
                    Adapter adapter = Settings.DeviceDriverType == DriverType.Hardware ? fact.GetAdapter1(0) : fact.GetWarpAdapter();
                    device = new Device12(adapter, Settings.FeatureLevels[Settings.FeatureLevels.Length - 1]);
                    commands = device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Direct));
                    //Renderer.SwapChain = new SwapChain(fact, commands, swapDescription);
                }
            }
            catch (Exception ex)
            {
                throw new DeviceCreationException(string.Format("Failed to create Device for the Driver {0}", Settings.DeviceDriverType.ToString()), ex);
            }
        }

        /// <inheritdoc/>
        public override void PostDeviceInitialization()
        {
            //// Ignore all windows events
            //Factory2 factory = SwapChain.GetParent<Factory2>();
            //factory.MakeWindowAssociation(Form.Handle, WindowAssociationFlags.IgnoreAll);

            //// New RenderTargetView from the backbuffer
            ////backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            //BackBuffer = SwapChain.GetBackBuffer<Texture2D>(0);
            //RenderView = new RenderTargetView(Device, BackBuffer);

            ////context.InputAssembler.InputLayout = layout;
            //Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            ////context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, 32, 0));
            //Context.Rasterizer.SetViewport(new Viewport11(0, 0, Form.ClientSize.Width, Form.ClientSize.Height, 0.0f, 1.0f));
            //Context.OutputMerger.SetTargets(RenderView);
        }

        /// <inheritdoc/>
        public override void Render()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Events
        /// <inheritdoc/>
        protected override void WindowResizedEvent(object sender, EventArgs e)
        {
            // Dispose all previous allocated resources
            //Utilities.Dispose(ref backBuffer);
            //Utilities.Dispose(ref renderView);
            //Utilities.Dispose(ref depthBuffer);
            //Utilities.Dispose(ref depthView);

            // Resize the backbuffer
            //Renderer.SwapChain.ResizeBuffers(swapDescription.BufferCount, Form.ClientSize.Width, Form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

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
            //viewport = new Viewport11(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f);

            //// Setup targets and viewport for rendering
            //context.Rasterizer.SetViewport(viewport);
            //context.OutputMerger.SetTargets(depthView, renderView);


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
            //UnloadContent();

            //// Dispose the game components.
            //foreach (IGameComponent comp in components)
            //{
            //    comp.UnloadContent();
            //    comp.Dispose();
            //}

            //// Dispose the game services.
            //foreach (IGameService serv in services.Values)
            //    serv.Dispose();

            //// Dispose all previous allocated resources
            //Utilities.Dispose(ref swapChain);
            //Utilities.Dispose(ref context);
            //Utilities.Dispose(ref device);
            //Utilities.Dispose(ref backBuffer);
            //Utilities.Dispose(ref renderView);
            //Utilities.Dispose(ref depthBuffer);
            //Utilities.Dispose(ref depthView);
        }

        public override bool UpdateState()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
