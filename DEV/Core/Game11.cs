#define VERSION_11

using System;
using SharpDX;
using SharpDX.Direct3D11;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using SharpDX.DXGI;
using Device11 = SharpDX.Direct3D11.Device;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Util.Exceptions;
using System.Reflection;
using PoncheToolkit.Graphics3D;
using System.Collections.Generic;
using System.Drawing;
using PoncheToolkit.Graphics2D.Effects;

namespace PoncheToolkit.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Class that wrap the functionality of a game using DirectX 11.
    /// </summary>
    public class Game11 : Game
    {
        #region Fields
        private DepthStencilState enabledDepthStencilState;
        private DepthStencilState disabledDepthStencilState;
        private BlendState additiveBlendState;
        private BlendState alphaBlendState;
        private BlendState disabledBlendState;
        private DepthStencilView depthStencilView;
        //private PTRenderTarget2D backBufferRenderTarget;
        private Texture2D backBuffer;
        private Texture2D depthBuffer;
        private Rational refreshRate;
        //private GraphicsRenderer11 renderer;
        private List<ViewportF> viewports;
        #endregion

        #region Properties
        ///// <summary>
        ///// Get or set the render target where the back buffer is set to render its contents. As Dirty.
        ///// </summary>
        //public PTRenderTarget2D BackBufferRenderTarget
        //{
        //    get { return backBufferRenderTarget; }
        //    internal set { SetPropertyAsDirty(ref backBufferRenderTarget, value); }
        //}

        /// <summary>
        /// Get or set the Depth Stencil View. As Dirty.
        /// </summary>
        public DepthStencilView DepthStencilView
        {
            get { return depthStencilView; }
            set { SetPropertyAsDirty(ref depthStencilView, value); }
        }

        /// <summary>
        /// The Depth Stencil State.
        /// </summary>
        public DepthStencilState DepthStencilState
        {
            get { return enabledDepthStencilState; }
        }

        /// <summary>
        /// The back buffer.
        /// </summary>
        public Texture2D BackBuffer
        {
            get { return backBuffer; }
            set { SetPropertyAsDirty(ref backBuffer, value); }
        }

        /// <summary>
        /// The depth buffer.
        /// </summary>
        public Texture2D DepthBuffer
        {
            get { return depthBuffer; }
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
        public List<ViewportF> Viewports
        {
            get { return viewports; }
        }

        /// <summary>
        /// List of objects that can have a 'dirty' status so when a property is changed
        /// they should be added to this list and their UpdateStatus method will be called.
        /// </summary>
        public static IReadOnlyList<IUpdatableState> UpdatableStateObjects
        {
            get { return updatableStateObjects.AsReadOnly(); }
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
#if DX11
            Log.Info("Using DirectX11\n");
            Log.Info("| Settings:");
            foreach (PropertyInfo prop in settings.GetType().GetProperties())
            {
                Log.Info("| {0}: {1}", prop.Name, prop.GetValue(settings));
            }

            viewports = new List<ViewportF>();
            renderer = new GraphicsRenderer11(this);

            refreshRate = new Rational(60, 1);

            GameTime.OnPhysicsUpdateDeltaTime += GameTimeService_OnPhysicsUpdateDeltaTime;
            GameTime.OnPhysicsUpdate += GameTimeService_OnPhysicsUpdate;

            // Apply the changes to the dirty properties from GameSettings.
            settings.OnPropertyChangedEvent += (sender, e) =>
            {
                // Fullscreen.
                if (HasFormRendered && settings.DirtyProperties.ContainsKey(nameof(settings.Fullscreen)))
                {
                    if (Renderer.SwapChain != null && this.IsFocused)
                        Renderer.SwapChain.SetFullscreenState(settings.Fullscreen, null);
                }

                // Depth buffer
                if (settings.DirtyProperties.ContainsKey(nameof(settings.DepthBufferEnabled)))
                {
                    if (settings.DepthBufferEnabled)
                        Renderer.ImmediateContext.OutputMerger.SetDepthStencilState(enabledDepthStencilState, 1);
                    else
                        Renderer.ImmediateContext.OutputMerger.SetDepthStencilState(disabledDepthStencilState, 1);
                }

                // Alpha blending.
                if (settings.DirtyProperties.ContainsKey(nameof(settings.BlendState)))
                {
                    var blendFactor = new Color4(0.8f, 0.8f, 0.8f, 0.8f);
                    
                    switch (settings.BlendState)
                    {
                        case BlendingState.AdditiveBlending:
                            Renderer.ImmediateContext.OutputMerger.SetBlendState(additiveBlendState, blendFactor, -1);
                            break;
                        case BlendingState.AlphaBlending:
                            Renderer.ImmediateContext.OutputMerger.SetBlendState(alphaBlendState, blendFactor, -1);
                            break;
                        case BlendingState.Disabled:
                            Renderer.ImmediateContext.OutputMerger.SetBlendState(disabledBlendState, blendFactor, -1);
                            break;
                    }
                }

                // Dynamic 2D shadows.
                if (settings.DirtyProperties.ContainsKey(nameof(settings.Draw2DShadows)))
                {
                    if (settings.Draw2DShadows)
                    {
                        if (!this.Services.ContainsKey(typeof(Dynamic2DLightManagerService)))
                        {
                            Dynamic2DLightManagerService serv = new Dynamic2DLightManagerService(this, Dynamic2DLightManagerService.ShadowMapSize.Size512);
                            serv.Initialize();
                            serv.LoadContent(this.ContentManager);
                            Services.AddService(serv);
                        }
                    }
                    else
                    {
                        if (this.Services.ContainsKey(typeof(Dynamic2DLightManagerService)))
                            Services.RemoveService(typeof(Dynamic2DLightManagerService));
                    }
                    
                }
            };

            // Update the self state. This is made here, because if the properties were set as dirty,
            // it would create circular loop calling UpdateState infinitely.
            this.OnPropertyChangedEvent += (sender, e) =>
            {
            };

            instance = this;
#endif
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

#if DX11
            if (Settings.Fullscreen)
                Renderer.SwapChain.SetFullscreenState(true, null);
#endif
        }

        /// <inheritdoc/>
        public override void CreateSwapDescriptionAndDevice()
        {
#if DX11
            // Create the main device
            try
            {
            #region Old way to initialize Device
                //Factory1 fact = new Factory1();
                //Adapter1 adapter = fact.GetAdapter1(0);

                //// Get the refresh rate of the active monitor.
                //ModeDescription[] descriptionModes = adapter.GetOutput(0).GetDisplayModeList(Format.R8G8B8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);
                //foreach (ModeDescription desc in descriptionModes)
                //{
                //    if (desc.Width == Settings.WindowSize.Width && desc.Height == Settings.WindowSize.Height)
                //        refreshRate = desc.RefreshRate;
                //}

                //Utilities.Dispose(ref fact);
                //Utilities.Dispose(ref adapter);
                //descriptionModes = null;

                //// Create the Device and the SwapChain.
                //SwapChainDescription swapDescription = new SwapChainDescription()
                //{
                //    BufferCount = 1,
                //    ModeDescription = new ModeDescription(Form.ClientSize.Width, Form.ClientSize.Height, refreshRate, Format.R8G8B8A8_UNorm),
                //    IsWindowed = true,
                //    OutputHandle = Form.Handle,
                //    SampleDescription = new SampleDescription(1, 0),
                //    SwapEffect = SwapEffect.Discard,
                //    Usage = Usage.RenderTargetOutput,
                //};

                //Device11.CreateWithSwapChain(Settings.DeviceDriverType, Settings.DeviceCreationFlags, swapDescription, out Renderer.Device, out Renderer.SwapChain);
                //Renderer.Context = Renderer.Device.ImmediateContext;

                //var factory = Renderer.SwapChain.GetParent<Factory>();
                //factory.MakeWindowAssociation(Form.Handle, WindowAssociationFlags.IgnoreAll);
                //Utilities.Dispose(ref factory);
            #endregion

                Factory1 fact = new Factory1();
                Adapter1 adapter = fact.GetAdapter1(0);

                // Set custom properties from the adapter.
                SystemDescription = adapter.Description1;

                // Get the refresh rate of the active monitor.
                ModeDescription[] descriptionModes = adapter.GetOutput(0).GetDisplayModeList(Format.R8G8B8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);
                foreach (ModeDescription desc in descriptionModes)
                {
                    if (desc.Width == Settings.Resolution.Width && desc.Height == Settings.Resolution.Height)
                        refreshRate = desc.RefreshRate;
                }

                foreach (ModeDescription desc in descriptionModes)
                {
                    if (desc.RefreshRate == refreshRate)
                        this.Settings.AvailableResolutions.Add(new Size(desc.Width, desc.Height));
                }

                Utilities.Dispose(ref fact);
                Utilities.Dispose(ref adapter);
                descriptionModes = null;

                Device11 device = new Device11(Settings.DeviceDriverType, Settings.DeviceCreationFlags, Settings.FeatureLevels);
                // Query the default device for the supported device and context interfaces.
                Renderer.Device = device.QueryInterface<SharpDX.Direct3D11.Device1>();
                Renderer.ImmediateContext = device.ImmediateContext.QueryInterface<DeviceContext1>();

                // Query for the adapter and more advanced DXGI objects.
                SharpDX.DXGI.Device2 dxgiDevice2 = device.QueryInterface<SharpDX.DXGI.Device2>();
                SharpDX.DXGI.Adapter dxgiAdapter = dxgiDevice2.Adapter;
                SharpDX.DXGI.Factory2 dxgiFactory2 = dxgiAdapter.GetParent<Factory2>();

                Utilities.Dispose(ref device);
                Utilities.Dispose(ref dxgiDevice2);
                Utilities.Dispose(ref dxgiAdapter);

                // Create the Device and the SwapChain.
                SwapChainDescription1 swapDescription = new SwapChainDescription1()
                {
                    // ==== For double buffering
                    //Width = Settings.WindowSize.Width,
                    //Height = Settings.WindowSize.Height,
                    //Format = Format.B8G8R8A8_UNorm,
                    //Stereo = false,
                    //BufferCount = 2,
                    //Scaling = Scaling.None,
                    ////ModeDescription = new ModeDescription(Form.ClientSize.Width, Form.ClientSize.Height, refreshRate, Format.R8G8B8A8_UNorm),
                    ////IsWindowed = true,
                    ////OutputHandle = Form.Handle,
                    //SampleDescription = new SampleDescription(1, 0),
                    //SwapEffect = SwapEffect.FlipSequential,
                    //Usage = Usage.BackBuffer | Usage.RenderTargetOutput,

                    Width = Settings.Resolution.Width,
                    Height = Settings.Resolution.Height,
                    //Format = Format.R8G8B8A8_UNorm,
                    Format = Settings.GlobalRenderTargetsFormat,
                    Stereo = false,
                    SampleDescription = new SampleDescription(1, 0),
                    //SampleDescription = new SampleDescription(4, 1),
                    Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                    BufferCount = 1,
                    Scaling = Scaling.Stretch,
                    SwapEffect = SwapEffect.Discard,
                    Flags = SwapChainFlags.AllowModeSwitch
                };

                SwapChainFullScreenDescription fullScreenDesc = new SwapChainFullScreenDescription()
                {
                    RefreshRate = refreshRate,
                    Scaling = DisplayModeScaling.Unspecified,
                    Windowed = true
                };

                // Generate a swap chain for our window based on the specified description.
                Renderer.SwapChain = new SwapChain1(dxgiFactory2, Renderer.Device, Form.Handle, ref swapDescription, fullScreenDesc, null);
                Utilities.Dispose(ref dxgiFactory2);

#if DEBUG
                // Used for debugging dispose object references
                Configuration.EnableObjectTracking = true;

                // Disable throws on shader compilation errors
                //Configuration.ThrowOnShaderCompileError = false;
#endif
            }
            catch (Exception ex)
            {
                string error = string.Format("Failed to create Device for the Driver {0}", Settings.DeviceDriverType.ToString());
                Log.Error(error, ex);
                throw new DeviceCreationException(error, ex);
            }
#endif
        }

        /// <inheritdoc/>
        public override void PostDeviceInitialization()
        {
#if DX11
            // Ignore all windows events
            Factory2 factory = Renderer.SwapChain.GetParent<Factory2>();
            factory.MakeWindowAssociation(Form.Handle, WindowAssociationFlags.IgnoreAll);
            Utilities.Dispose(ref factory);

            // New RenderTargetView from the backbuffer
            backBuffer = Renderer.SwapChain.GetBackBuffer<Texture2D>(0);
            //renderTargetView = new RenderTargetView(Renderer.Device, backBuffer);
            BackBufferRenderTarget = new PTRenderTarget2D(Renderer, "BackBufferRT");
            BackBufferRenderTarget.Initialize();

            viewports.Add(new ViewportF(0, 0, Form.ClientSize.Width, Form.ClientSize.Height, 0.0f, 1.0f));

            renderer.Rasterizer.FillMode = FillMode.Solid;
            renderer.Rasterizer.CullMode = CullMode.Back;
            renderer.Rasterizer.IsAntialiasedLineEnabled = false;
            renderer.Rasterizer.IsMultisampleEnabled = true;
            renderer.ImmediateContext.Rasterizer.State = renderer.Rasterizer.RasterizerState;

            #region Depth Stencil State
            enabledDepthStencilState = new DepthStencilState(Renderer.Device, new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = true,
                DepthComparison = Comparison.Less,
                DepthWriteMask = DepthWriteMask.All,
                //DepthWriteMask = DepthWriteMask.Zero,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    //DepthFailOperation = StencilOperation.Keep,
                    //PassOperation = StencilOperation.Replace,
                    Comparison = Comparison.Always
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                },
            });

            disabledDepthStencilState = new DepthStencilState(Renderer.Device, new DepthStencilStateDescription()
            {
                DepthComparison = Comparison.Less,
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                },
            });

            ToDispose(enabledDepthStencilState);
            ToDispose(disabledDepthStencilState);
            #endregion

            #region Initialize Blend States
            // Create an additive blend state description.
            BlendStateDescription blendStateDesc = new BlendStateDescription();
            blendStateDesc.RenderTarget[0].IsBlendEnabled = true;
            blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
            blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            additiveBlendState = new BlendState(Renderer.Device, blendStateDesc);

            // Create an alpha enabled blend state description.
            blendStateDesc = new BlendStateDescription();
            blendStateDesc.RenderTarget[0].IsBlendEnabled = true;
            blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            alphaBlendState = new BlendState(Renderer.Device, blendStateDesc);

            // Create an disabled blend state description.
            // Modify the description to create an disabled blend state description.
            blendStateDesc = new BlendStateDescription();
            blendStateDesc.RenderTarget[0].IsBlendEnabled = false;
            blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            disabledBlendState = new BlendState(Renderer.Device, blendStateDesc);

            ToDispose(alphaBlendState);
            ToDispose(disabledBlendState);
            #endregion

            Renderer.ImmediateContext.OutputMerger.SetDepthStencilState(enabledDepthStencilState, 1);

            // Initialize the GraphicsRenderer. Now it only initialize the 2D context and spriteBatch.
            renderer.Initialize();
#endif
        }

        /// <inheritdoc/>
        public override void LoadContent()
        {
            renderer.LoadContent(ContentManager);
            base.LoadContent();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            base.Update();
        }

        /// <inheritdoc/>
        public override void Render()
        {
#if DX11
            //renderer.BeginRender(renderTargetView, depthStencilView);
            renderer.BeginRender(BackBufferRenderTarget, depthStencilView);

            // Render the screens. This manager is the base to render all the content.
            // The Engine is based on screens.
            ScreenManager.Render(renderer, CurrentCamera);

            renderer.EndRender(SyncIntervalParameter);
#endif
        }

        ///// <summary>
        ///// Change the Z Buffer status.
        ///// </summary>
        ///// <param name="enabled"></param>
        //public void ToggleZBuffer(bool enabled)
        //{
        //    Settings.DepthBufferEnabled = enabled;
        //}

        ///// <summary>
        ///// Change the Alpha blending status.
        ///// </summary>
        ///// <param name="blendState">The <see cref="BlendingState"/> state to set into the GPU.</param>
        //public void ToggleBlending(BlendingState blendState)
        //{
        //    Settings.BlendState = blendState;
        //}

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            // Update the states of the dirty objects. From the last added to the first.
            for (int i = updatableStateObjects.Count - 1; i >= 0; i--)
            {
                IUpdatableState obj = updatableStateObjects[i];
                if (obj.UpdateState())
                    updatableStateObjects.Remove(obj);
            }

            IsStateUpdated = true;
            OnStateUpdated();
            return IsStateUpdated;
        }

        /// <summary>
        /// Add an object to the list of dirty objects.
        /// Set the <see cref="IUpdatableState.IsStateUpdated"/> property to false before adding it.
        /// </summary>
        /// <param name="obj">The object to be added.</param>
        public static bool AddDirtyObject(IUpdatableState obj)
        {
            if (updatableStateObjects == null || updatableStateObjects.Contains(obj))
                return false;
            obj.IsStateUpdated = false;
            updatableStateObjects.Add(obj);
            return true;
        }

        /// <summary>
        /// Remove a specific dirty object.
        /// This is called if an UpdatableObject UpdateState method is called separately.
        /// </summary>
        /// <param name="obj">The object to be removed.</param>
        public static bool RemoveDirtyObject(IUpdatableState obj)
        {
            if (!obj.IsStateUpdated || updatableStateObjects == null || !updatableStateObjects.Contains(obj))
                return false;
            updatableStateObjects.Remove(obj);
            return true;
        }
        #endregion

        #region Events
        /// <summary>
        /// Event thrown to update the Delta time from the physics engine.
        /// </summary>
        /// <param name="physicsDeltaTime"></param>
        private void GameTimeService_OnPhysicsUpdateDeltaTime(ref float physicsDeltaTime)
        {
            physicsDeltaTime = GameTime.DeltaTime;
        }

        /// <summary>
        /// Method to set the update of the physics engine.
        /// </summary>
        /// <param name="physicsDeltaTime"></param>
        private void GameTimeService_OnPhysicsUpdate(float physicsDeltaTime)
        {
            // TODO: Add physics updates
        }

        /// <inheritdoc/>
        protected override void WindowResizedEvent(object sender, EventArgs e)
        {
            if (sender != null)
            {
                Log.Debug("Resizing window to: {0}", (sender as SharpDX.Windows.RenderForm)?.ClientSize);
                Settings.Resolution = (Size)(sender as SharpDX.Windows.RenderForm)?.ClientSize;
            }

            try
            {
                if (Settings.Resolution.Width == 0 && Settings.Resolution.Height == 0)
                    return;
                RecreateBuffers();
            }
            catch (Exception ex)
            {
                Log.Error("Error recreating buffers.", ex);
                throw;
            }
        }

        /// <summary>
        /// Recreate the buffers when resizing.
        /// </summary>
        public void RecreateBuffers()
        {
            if (IsInterop)
                return;

#if DX11
            // Dispose all previous allocated resources
            Utilities.Dispose(ref backBuffer);
            Utilities.Dispose(ref Renderer.postProcessRenderTarget); // Post proces render target.
            BackBufferRenderTarget.Dispose();
            Utilities.Dispose(ref depthBuffer);
            Utilities.Dispose(ref depthStencilView);

            if (renderer.SpriteBatch.IsInitialized)
                renderer.SpriteBatch.ReleaseTarget();

            // Resize the backbuffer
            //Renderer.SwapChain.ResizeBuffers(Renderer.SwapChain.Description1.BufferCount, Settings.WindowSize.Width, Settings.WindowSize.Height, Renderer.SwapChain.Description1.Format, Renderer.SwapChain.Description1.Flags);
            Renderer.SwapChain.ResizeBuffers(0, 0, 0, Format.Unknown, 0);

            // Get the backbuffer from the swapchain
            backBuffer = Renderer.SwapChain.GetBackBuffer<Texture2D>(0);

            // Renderview on the backbuffer
            BackBufferRenderTarget = new PTRenderTarget2D(Renderer, "BackBufferRT");
            BackBufferRenderTarget.Initialize();
            BackBufferRenderTarget.Texture = new Graphics2D.PTTexture2D(Renderer.Device, backBuffer);
            BackBufferRenderTarget.UpdateState();

            // Recreate the Post process target.
            Renderer.PostProcessRenderTarget = new PTRenderTarget2D(Renderer, "PostProcesRT");
            Renderer.PostProcessRenderTarget.Initialize();
            Renderer.PostProcessRenderTarget.UpdateState();

            // Create the depth buffer
            depthBuffer = ToDispose(new Texture2D(Renderer.Device, new Texture2DDescription()
            {
                Format = Format.D24_UNorm_S8_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = Settings.Resolution.Width,
                Height = Settings.Resolution.Height,
                SampleDescription = new SampleDescription(1, 0),
                //SampleDescription = new SampleDescription(4, 1),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            }));

            // Create the depth stencil view.
            DepthStencilViewDescription depthDesc = new DepthStencilViewDescription()
            {
                //Dimension = DepthStencilViewDimension.Texture2D,
                Dimension = DepthStencilViewDimension.Texture2DMultisampled,
                Format = Format.D24_UNorm_S8_UInt,
            };
            depthDesc.Texture2D.MipSlice = 0;
            depthStencilView = ToDispose(new DepthStencilView(Renderer.Device, depthBuffer, depthDesc));

            // Initialize the Viewport.
            if (!this.IsInterop)
                viewports[0] = new ViewportF(0, 0, Settings.Resolution.Width, Settings.Resolution.Height, 0.0f, 1f);

            // Setup targets and viewport for rendering
            Renderer.SetRenderTarget3D(BackBufferRenderTarget);
            //// If there are PostProcessing effects set the PostProcess render target instead of the back buffer.
            //if (Renderer.PostProcessEffects.Count > 0)
            //    Renderer.SetRenderTarget3D(backBufferRenderTarget, depthStencilView);
            //else
            //    Renderer.SetRenderTarget3D(Renderer.PostProcessRenderTarget, depthStencilView);

            Renderer.ImmediateContext.Rasterizer.SetViewport(viewports[0]);

            if (renderer.SpriteBatch.IsInitialized)
                renderer.SpriteBatch.Initialize();

#elif DX12
#endif
        }

        /// <summary>
        /// Recreate the buffers when resizing for wpf, using the buffer from the wpf surface.
        /// </summary>
        public void RecreateBuffersWpf(Texture2D wpfSurface, Size size)
        {
#if DX11
            // Dispose all previous allocated resources
            Utilities.Dispose(ref backBuffer);
            BackBufferRenderTarget.Dispose();
            Utilities.Dispose(ref depthBuffer);
            Utilities.Dispose(ref depthStencilView);
            renderer.SpriteBatch.ReleaseTarget();

            // Resize the backbuffer
            Renderer.SwapChain.ResizeBuffers(Renderer.SwapChain.Description.BufferCount, size.Width, size.Height, Format.Unknown, SwapChainFlags.None);

            //backBuffer = wpfSurface;

            // Renderview on the backbuffer
            BackBufferRenderTarget = new PTRenderTarget2D(Renderer, "BackBufferRT");
            BackBufferRenderTarget.Initialize();
            BackBufferRenderTarget.Texture = new Graphics2D.PTTexture2D(Renderer.Device, backBuffer);
            BackBufferRenderTarget.UpdateState();

            // Create the depth buffer
            depthBuffer = new Texture2D(Renderer.Device, new Texture2DDescription()
            {
                //Format = Format.D32_Float_S8X24_UInt,
                Format = Format.D24_UNorm_S8_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = size.Width,
                Height = size.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            #region Depth Stencil State
            enabledDepthStencilState = new DepthStencilState(Renderer.Device, new DepthStencilStateDescription()
            {
                DepthComparison = Comparison.Less,
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                },
            });
            #endregion

            // Create the depth stencil view.
            DepthStencilViewDescription depthDesc = new DepthStencilViewDescription()
            {
                Dimension = DepthStencilViewDimension.Texture2D,
                Format = Format.D24_UNorm_S8_UInt,
            };
            depthDesc.Texture2D.MipSlice = 0;
            depthStencilView = new DepthStencilView(Renderer.Device, depthBuffer, depthDesc);
            //depthStencilView = new DepthStencilView(Renderer.Device, depthBuffer);

            // Initialize the Viewport.
            //viewports[0] = new ViewportF(-200, 0, size.Width, size.Height, 0.0f, 1f);
            viewports[0] = new ViewportF(-200, 0, viewports[0].Width, viewports[0].Height, 0.0f, 1f);

            // Setup targets and viewport for rendering
            Renderer.ImmediateContext.OutputMerger.SetDepthStencilState(enabledDepthStencilState, 1);
            //Renderer.ImmediateContext.OutputMerger.SetTargets(depthStencilView, renderTarget.RenderTarget);
            Renderer.SetRenderTarget3D(BackBufferRenderTarget);
            Renderer.ImmediateContext.Rasterizer.SetViewport(viewports[0]);

            renderer.SpriteBatch.Initialize();
#elif DX12
#endif
        }
        #endregion

        #region IDisposable
        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            UnloadContent();

            // Dispose the game services.
            foreach (IGameService serv in Services.Values)
                serv.Dispose();

            Utilities.Dispose(ref backBuffer);
            //Utilities.Dispose(ref renderTargetView);
            BackBufferRenderTarget.Dispose();
            Utilities.Dispose(ref depthBuffer);
            Utilities.Dispose(ref depthStencilView);
            Utilities.Dispose(ref enabledDepthStencilState);
            Utilities.Dispose(ref disabledDepthStencilState);

            renderer.Dispose();

        }
        #endregion
    }
}
