using System;
using System.Linq;
using PoncheToolkit.Core;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using PoncheToolkit.Util;

namespace PoncheToolkit.Graphics3D
{
    using Cameras;
    using Core.Management.Content;
    using Core.Management.Screen;
    using Core.Management.Threading;
    using Effects;
    using Graphics2D;
    using Graphics2D.Effects;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Class that wrap the functionality to render content.
    /// </summary>
    public class GraphicsRenderer11 : UpdatableStateObject, IGraphicsRenderer11
    {
        #region Fields
#if DX11
        private Game11 game;
#elif DX12
        private Game12 game;
#endif
        private DeviceContext1 deviceContext;
        private PTRenderTarget2D reflectionParaboloidFrontRT;
        private PTRenderTarget2D reflectionParaboloidBackRT;
        private PTRenderTarget2D currentRenderTarget;
        private FreeCamera reflectionParaboloidCamera;

        private DeviceContext1[] deferredContexts;
        private CommandList[] commandLists;
        private Task[] tasks;
        private Core.Services.DebuggerRenderableService debugger;
        private BlendingState preStateBlendingState;

        // new
        private PTRasterizer rasterizer;
        private SpriteBatch spriteBatch;
        private Size2F dpi;
        private SharpDX.Direct2D1.DeviceContext context2D;
        private SharpDX.Direct2D1.BitmapProperties1 bitmapProperties2D;
        private ProcessRenderingMode processRenderMode;
        private ShadingRenderingMode shadingRenderMode;
        private SortedList<int, PTCustomEffect> postProcessEffects;

        internal SwapChain1 swapChain;
        internal SharpDX.Direct3D11.Device1 device;
        internal PTRenderTarget2D postProcessRenderTarget;
        #endregion

        #region Events
        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        /// <inheritdoc/>
        public event EventHandlers.OnInitializedHandler OnInitialized;

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

        #region Properties
        /// <summary>
        /// The Swap Chain to render.
        /// </summary>
        public SwapChain1 SwapChain
        {
            get { return swapChain; }
            set { SetProperty(ref swapChain, value); }
        }

        /// <summary>
        /// Instance of the device used.
        /// </summary>
        public SharpDX.Direct3D11.Device1 Device
        {
            get { return device; }
            set { SetProperty(ref device, value); }
        }

        /// <summary>
        /// The <see cref="SharpDX.Direct3D11.DeviceContext1"/> immediate context used to render the 3D content.
        /// </summary>
        public SharpDX.Direct3D11.DeviceContext1 ImmediateContext
        {
            get { return deviceContext; }
            set { SetProperty(ref deviceContext, value); }
        }

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
        /// The <see cref="SharpDX.Direct2D1.Effect"/> effects to be applied to the back buffer before it is displayed.
        /// </summary>
        public SortedList<int, PTCustomEffect> PostProcessEffects
        {
            get { return postProcessEffects; }
            set { postProcessEffects = value; }
        }

        /// <inheritdoc/>
        public PTRenderTarget2D PostProcessRenderTarget
        {
            get { return postProcessRenderTarget; }
            set { SetPropertyAsDirty(ref postProcessRenderTarget, value); }
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
            set { SetProperty(ref bitmapProperties2D, value); }
        }

        /// <summary>
        /// The dpi of the monitor.
        /// </summary>
        public Size2F Dpi
        {
            get { return dpi; }
            internal set { SetProperty(ref dpi, value); }
        }

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
        public bool IsContentLoaded { get; set; }

        /// <inheritdoc/>
        public bool IsInitialized { get; set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
#if DX11
        public GraphicsRenderer11(Game11 game)
#elif DX12
        public GraphicsRenderer11(Game12 game)
#endif
        {
            this.game = game;
            this.Rasterizer = new PTRasterizer(this);
            this.processRenderMode = ProcessRenderingMode.Immediate;
            this.shadingRenderMode = ShadingRenderingMode.ForwardShading;
            this.postProcessEffects = new SortedList<int, PTCustomEffect>();
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public void Initialize()
        {
            // Initialize elements for 2D Rendering
            #region 2D rendering initialization
            this.SpriteBatch = new SpriteBatch(game);
            SharpDX.Direct2D1.Device d2dDevice = null;
            SharpDX.Direct2D1.Factory1 d2dFactory = null;

            try
            {
                SharpDX.DXGI.Device1 dxgiDevice = this.Device.QueryInterface<SharpDX.DXGI.Device1>();
                d2dFactory = new SharpDX.Direct2D1.Factory2(SharpDX.Direct2D1.FactoryType.MultiThreaded, SharpDX.Direct2D1.DebugLevel.None);
                d2dDevice = new SharpDX.Direct2D1.Device1((SharpDX.Direct2D1.Factory2)d2dFactory, dxgiDevice);

                Dpi = d2dFactory.DesktopDpi;
                BitmapProperties2D = new SharpDX.Direct2D1.BitmapProperties1(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    Dpi.Width, Dpi.Height, SharpDX.Direct2D1.BitmapOptions.CannotDraw | SharpDX.Direct2D1.BitmapOptions.Target);
                //Utilities.Dispose(ref d2dFactory);
                Utilities.Dispose(ref dxgiDevice);
            }
            catch (Exception ex)
            {
                Log.Warning("Interface Factory2 not supported. Trying with Factory1", ex);
                SharpDX.DXGI.Device1 dxgiDevice = this.Device.QueryInterface<SharpDX.DXGI.Device1>();
                d2dFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.MultiThreaded, SharpDX.Direct2D1.DebugLevel.None);
                d2dDevice = new SharpDX.Direct2D1.Device((SharpDX.Direct2D1.Factory1)d2dFactory, dxgiDevice);

                Dpi = d2dFactory.DesktopDpi;
                BitmapProperties2D = new SharpDX.Direct2D1.BitmapProperties1(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    Dpi.Width, Dpi.Height, SharpDX.Direct2D1.BitmapOptions.CannotDraw | SharpDX.Direct2D1.BitmapOptions.Target);
                //Utilities.Dispose(ref d2dFactory);
                Utilities.Dispose(ref dxgiDevice);
            }

            Context2D = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.EnableMultithreadedOptimizations);
            Context2D.PrimitiveBlend = SharpDX.Direct2D1.PrimitiveBlend.SourceOver;
            Context2D.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
            Context2D.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
            Context2D.UnitMode = SharpDX.Direct2D1.UnitMode.Pixels;
            Utilities.Dispose(ref d2dDevice);

            SpriteBatch.Initialize();
            SpriteBatch.D2dFactory = d2dFactory;

            ToDispose(Context2D);
            ToDispose(SpriteBatch);
            #endregion

            try
            {
                debugger = game.Services[typeof(Core.Services.DebuggerRenderableService)] as Core.Services.DebuggerRenderableService;
            }
            catch (Exception ex)
            {
                Log.Warning("No attached Debugger service", ex);
            }

            reflectionParaboloidFrontRT = new PTRenderTarget2D(this, "ReflectionParaboloidFrontRT");
            reflectionParaboloidBackRT = new PTRenderTarget2D(this, "ReflectionParaboloidBackRT");

            reflectionParaboloidCamera = new FreeCamera(Game);
            reflectionParaboloidCamera.Initialize();

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public void LoadContent(IContentManager contentManager)
        {
            if (SpriteBatch.IsInitialized)
                SpriteBatch.LoadContent(contentManager);

            reflectionParaboloidCamera.LoadContent(contentManager);

            // Create the instance buffers per mesh.
            //InstanceBuffer = Buffer.Create(game.Renderer.Device, BindFlags.ConstantBuffer, Vertices.ToArray(), 0, ResourceUsage.Dynamic, CpuAccessFlags.None, ResourceOptionFlags.None);

            PostProcessRenderTarget = new PTRenderTarget2D(this, "PostProcessRT");
            PostProcessRenderTarget.Initialize();
            PostProcessRenderTarget.UpdateState();

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Begin the render. 
        /// Clear the target view and the depth stencil.
        /// </summary>
        /// <param name="renderTarget"></param>
        /// <param name="depthStencilView"></param>
        /// <param name="clearColor">The color to clear the context. Default: (130, 150, 175)</param>
        //public void BeginRender(RenderTargetView targetView, DepthStencilView depthStencilView, Color? clearColor = null)
        public void BeginRender(PTRenderTarget2D renderTarget, DepthStencilView depthStencilView, Color? clearColor = null)
        {
            if (clearColor == null)
                clearColor = new Color(70, 90, 105);

            preStateBlendingState = Game.Settings.BlendState;

            SetRenderTarget3D(renderTarget);
            ImmediateContext.ClearRenderTargetView(renderTarget.RenderTargetView, clearColor.Value);
            ImmediateContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1, 0);
        }

        /// <summary>
        /// End render. Present the swap chain buffer.
        /// </summary>
        /// <param name="syncIntervalParameter"></param>
        public void EndRender(int syncIntervalParameter)
        {
            //ImmediateContext.Flush();
            SwapChain.Present(syncIntervalParameter, PresentFlags.None);
            OnEndRender?.Invoke();

            Game.ToggleBlending(preStateBlendingState);
        }

        /// <inheritdoc/>
        public virtual void AddBackBufferPostProcessEffect(int order, PTCustomEffect custom2DEffect)
        {
            if (PostProcessEffects.Keys.Contains(order))
                throw new ArgumentException("The order - " + order + " is being used.");

            PostProcessEffects.Add(order, custom2DEffect);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                if (DirtyProperties.ContainsKey(nameof(ProcessRenderMode)))
                {

                    // If changed to immediate, destroy the deferred contexts.
                    if (ProcessRenderMode == ProcessRenderingMode.Immediate)
                    {
                        // Destroy deferred contexts.
                        for (int i = 0; i < ThreadingManager.CURRENT_RENDERING_THREADS_COUNT; i++)
                        {
                            if (tasks != null)
                                Utilities.Dispose(ref tasks[i]);
                            Utilities.Dispose(ref deferredContexts[i]);
                            Utilities.Dispose(ref commandLists[i]);
                        }
                    }
                    else if (ProcessRenderMode == ProcessRenderingMode.MultiThread)
                    {
                        tasks = new Task[ThreadingManager.MAX_RENDERING_THREADS_COUNT];
                        deferredContexts = new SharpDX.Direct3D11.DeviceContext1[ThreadingManager.MAX_RENDERING_THREADS_COUNT];
                        commandLists = new SharpDX.Direct3D11.CommandList[ThreadingManager.MAX_RENDERING_THREADS_COUNT];

                        for (int i = 0; i < ThreadingManager.MAX_RENDERING_THREADS_COUNT; i++)
                            deferredContexts[i] = new SharpDX.Direct3D11.DeviceContext1(Device);
                    }
                }

                IsStateUpdated = true;
                OnStateUpdated();
            }
            return IsStateUpdated;
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
#if DX11
            if (target != null && target.RenderTargetView != null)
                currentRenderTarget = target;
            else
                currentRenderTarget = Game.BackBufferRenderTarget;

            ImmediateContext.OutputMerger.SetRenderTargets(Game.DepthStencilView, currentRenderTarget.RenderTargetView);
#endif
        }

        /// <summary>
        /// Render all the meshes from the <see cref="GameScreen.MeshesPerEffect"/> dictionary.
        /// It groups the rendering by effect so the graphics pipeline does not have to switch
        /// between shaders so often and save some bandwidth power using the <see cref="GameScreen.DrawableComponentsPerEffect"/> dictionary.
        /// </summary>
        public void RenderScreen(GameScreen screen, Camera camera, SpriteBatch spriteBatch, List<PTMesh> excludedMeshes = null)
        {
            #region Render deferred
            if (ProcessRenderMode == ProcessRenderingMode.MultiThread)
            {
                // TODO: Group all rendering by effect AND meshes to pass all the meshes divided in the available deferred contexts.
                //int effectsCount = screen.DrawableComponentsPerEffect.Count;


                // Iterate over the screen effects.
                foreach (PTEffect effect in screen.DrawableComponentsPerEffect.Keys)
                {
                    if (effect.Materials.Count <= 0)
                        throw new Util.Exceptions.RenderingException("The effect -{0}- used has no materials.", System.IO.Path.GetFileNameWithoutExtension(effect.ShaderPath));

                    List<PTMesh> meshes = screen.MeshesPerEffect[effect];

                    int tasksCount = (meshes.Count > ThreadingManager.CURRENT_RENDERING_THREADS_COUNT ? ThreadingManager.CURRENT_RENDERING_THREADS_COUNT : meshes.Count);
                    //int meshesPerContext = meshes.Count / ThreadingManager.CURRENT_RENDERING_THREADS_COUNT;
                    //int remainder = meshes.Count % ThreadingManager.CURRENT_RENDERING_THREADS_COUNT;
                    int meshesPerContext = meshes.Count / tasksCount;
                    int remainder = meshes.Count % tasksCount;
                    tasks = new Task[tasksCount];
                    if (remainder > 0)
                        tasksCount--;
                    //tasks = new Task[meshes.Count > ThreadingManager.CURRENT_RENDERING_THREADS_COUNT ? ThreadingManager.CURRENT_RENDERING_THREADS_COUNT : meshes.Count];

                    // TODO: implement the perMaterial rendering. Right now it just has perEffect.
                    // Apply effect once per context.
                    for (int i = 0; i < ThreadingManager.CURRENT_RENDERING_THREADS_COUNT; i++)
                        effect.Apply(PickContext(i));

                    //for (int threadIndex = 0; threadIndex < ThreadingManager.CURRENT_RENDERING_THREADS_COUNT; threadIndex++)
                    for (int threadIndex = 0; threadIndex < tasksCount; threadIndex++)
                    {
                        int from = (threadIndex == 0) ? 0 : (meshesPerContext * threadIndex);
                        int to = (threadIndex == 0) ? meshesPerContext : (meshesPerContext * threadIndex) + meshesPerContext;
                        renderMultiThreadByEffect(threadIndex, from, to, meshes, effect, spriteBatch);
                    }

                    // Render the remainder.
                    if (remainder > 0)
                    {
                        // Take the last task reserved to render the remainder meshes.
                        renderMultiThreadByEffect(tasksCount, meshesPerContext * tasksCount, meshes.Count, meshes, effect, spriteBatch);
                    }

                    Task.WaitAll(tasks);

                    // Execute the deferred command lists on the immediate context.
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        CommandList commandList = commandLists[i];
                        if (commandList == null || commandList.IsDisposed)
                            continue;

                        ImmediateContext.ExecuteCommandList(commandList, false);
                        commandList.Dispose();
                    }
                }

            }
            #endregion

            #region Render Immediate
            else if (ProcessRenderMode == ProcessRenderingMode.Immediate)
            {
                /* TODO: Implement multiple passes rendering, disabling Blending in the first pass
                 * And adding it for later passes
                 * 
                 * for each mesh being rendered
                       for each light affecting the mesh
                          if (first light)
                              render first light with ambient and no blending
                          else
                              render nth light with no ambient and additive belnding
                */


                // Iterate over the screen effects.
                int passCount = 0;
                foreach (PTEffect effect in screen.DrawableComponentsPerEffect.Keys)
                {
                    if (!screen.MeshesPerEffect.ContainsKey(effect))
                        continue;

                    List<PTMesh> meshes = null;
                    if (excludedMeshes != null)
                        meshes = screen.MeshesPerEffect[effect].Except(excludedMeshes).ToList();
                    else
                        meshes = screen.MeshesPerEffect[effect];

                    // Set additive blending for the second and more passes.
                    //if (passCount > 0)
                    //    Game.ToggleBlending(BlendingState.AdditiveBlending);

                    effect.Apply(PickContext(0));

                    if (debugger != null)
                        debugger.ModelsDrawn = 0;
                    // Draw the meshes that contain that effect.
                    // TODO: implement the perMaterial rendering. Right now it just has perEffect.
                    renderMeshesByEffect(0, 0, meshes.Count, meshes, effect, PickContext(0), spriteBatch);

                    passCount++;
                }
            }
            #endregion

            #region Apply PostProcess
            // Apply PostProces effects into the BackBuffer.
            if (PostProcessEffects.Count > 0)
            {
                SpriteBatch.Begin(Game.BackBufferRenderTarget);
                foreach (PTCustomEffect effect2D in PostProcessEffects.Values)
                {
                    int index = PostProcessEffects.IndexOfValue(effect2D);
                    if (index == 0)
                        effect2D.Effect.SetInput(0, PostProcessRenderTarget, false);
                    else
                        effect2D.Effect.SetInputEffect(0, PostProcessEffects.Values[index - 1], false); // Set the last effect output as input for this.
                }
                // Draw the last effect that contains a chain graph with all the effects.
                // This is using the 2D linking effects for best performance.
                Context2D.DrawImage(PostProcessEffects.Values.Last(), SharpDX.Direct2D1.InterpolationMode.Anisotropic, SharpDX.Direct2D1.CompositeMode.SourceIn);
                SpriteBatch.End();
            }
            #endregion
        }

        #region Render screen to Texture
        /// <summary>
        /// Render all the meshes from the <see cref="GameScreen.MeshesPerEffect"/> dictionary.
        /// It groups the rendering by effect so the graphics pipeline does not have to switch
        /// between shaders so often and save some bandwidth power using the <see cref="GameScreen.DrawableComponentsPerEffect"/> dictionary.
        /// </summary>
        /// <param name="screen">The <see cref="GameScreen"/> to be rendered to a texture.</param>
        /// <param name="camera">The camera from which the render will take place.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="renderTarget">The RenderTarget that contains the texture reference to which the rendering will take place.</param>
        public void RenderScreenToTexture(GameScreen screen, Camera camera, SpriteBatch spriteBatch, ref PTRenderTarget2D renderTarget)
        {
            RenderScreenToTexture(screen, camera, spriteBatch, ref renderTarget, null, null);
        }

        /// <summary>
        /// Render all the meshes from the <see cref="GameScreen.MeshesPerEffect"/> dictionary.
        /// It groups the rendering by effect so the graphics pipeline does not have to switch
        /// between shaders so often and save some bandwidth power using the <see cref="GameScreen.DrawableComponentsPerEffect"/> dictionary.
        /// </summary>
        /// <param name="screen">The <see cref="GameScreen"/> to be rendered to a texture.</param>
        /// <param name="camera">The camera from which the render will take place.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="renderTarget">The RenderTarget that contains the texture reference to which the rendering will take place.</param>
        /// <param name="excludedMeshes">A list with the meshes that want to be excluded from the rendering.</param>
        public void RenderScreenToTexture(GameScreen screen, Camera camera, SpriteBatch spriteBatch, ref PTRenderTarget2D renderTarget, List<PTMesh> excludedMeshes)
        {
            RenderScreenToTexture(screen, camera, spriteBatch, ref renderTarget, excludedMeshes, null);
        }

        /// <summary>
        /// Render all the meshes from the <see cref="GameScreen.MeshesPerEffect"/> dictionary.
        /// It groups the rendering by effect so the graphics pipeline does not have to switch
        /// between shaders so often and save some bandwidth power using the <see cref="GameScreen.DrawableComponentsPerEffect"/> dictionary.
        /// </summary>
        /// <param name="screen">The <see cref="GameScreen"/> to be rendered to a texture.</param>
        /// <param name="camera">The camera from which the render will take place.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="renderTarget">The RenderTarget that contains the texture reference to which the rendering will take place.</param>
        /// <param name="excludedMeshes">A list with the meshes that want to be excluded from the rendering.</param>
        /// <param name="clearColor">The color to clear the context. Default: White</param>
        public void RenderScreenToTexture(GameScreen screen, Camera camera, SpriteBatch spriteBatch, ref PTRenderTarget2D renderTarget, List<PTMesh> excludedMeshes,
            Color? clearColor)
        {
            if (clearColor == null)
                clearColor = Color.White;

            PTRenderTarget2D lastRenderTarget = currentRenderTarget;

            // Render the screen using the specified render target.
            SetRenderTarget3D(renderTarget);
#if DX11
            ImmediateContext.ClearRenderTargetView(renderTarget.RenderTargetView, clearColor.Value);
            ImmediateContext.ClearDepthStencilView(Game.DepthStencilView, DepthStencilClearFlags.Depth, 1, 0);
#elif DX12
            // TODO: Render screen to texture 
#endif
            RenderScreen(screen, camera, spriteBatch, excludedMeshes);

            // Return to the default Back Buffer.
            SetRenderTarget3D(lastRenderTarget);
        }
        #endregion

        #region Render Paraboloid Reflection
        /// <summary>
        /// Render all the meshes from the <see cref="GameScreen.MeshesPerEffect"/> dictionary.
        /// It groups the rendering by effect so the graphics pipeline does not have to switch
        /// between shaders so often and save some bandwidth power using the <see cref="GameScreen.DrawableComponentsPerEffect"/> dictionary.
        /// </summary>
        /// <param name="screen">The <see cref="GameScreen"/> to be rendered to a texture.</param>
        /// <param name="paraboloidCamera">The camera from which the render will take place. This is typically the object of the material</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="renderTarget">The RenderTarget that contains the texture reference to which the rendering will take place.</param>
        /// <param name="excludedMeshes">A list with the meshes that want to be excluded from the rendering.</param>
        /// <param name="depthView">The depth stencil</param>
        /// <param name="clearColor">The color to clear the context. Default: White</param>
        public void RenderScreenParaboloidReflection(GameScreen screen, Camera paraboloidCamera, Vector3 objectPosition, List<PTMesh> excludedMeshes)
        {
            reflectionParaboloidCamera.View.TranslationVector = objectPosition;
            // Render front paraboloid.
            RenderScreenToTexture(screen, reflectionParaboloidCamera, SpriteBatch, ref reflectionParaboloidFrontRT);

            // Render front paraboloid.

            RenderScreenToTexture(screen, reflectionParaboloidCamera, SpriteBatch, ref reflectionParaboloidBackRT);
        }
        #endregion

        /*
        static void drawPreZPass()
        {
	        PROFILE_SCOPE_2("drawPreZPass", TT_OpenGl);
	        glBindFramebuffer(GL_FRAMEBUFFER, g_forwardFbo);
	        glViewport(0,0, g_width, g_height);
	        glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
	        glClear(GL_DEPTH_BUFFER_BIT);
	        if (g_numMsaaSamples != 1)
	        {
		        glEnable(GL_MULTISAMPLE);
	        }
	        glDepthFunc(GL_LEQUAL);
	        glColorMask(GL_FALSE, GL_FALSE, GL_FALSE, GL_FALSE);

	        g_simpleShader->begin(false);
	        g_model->render(0, OBJModel::RF_Opaque);
	        g_simpleShader->end();
	        g_simpleShader->begin(true);
	        g_model->render(0, OBJModel::RF_AlphaTested);
	        g_simpleShader->end();

	        glColorMask(GL_TRUE, GL_TRUE, GL_TRUE, GL_TRUE);
        }
         */

        /// <summary>
        /// Render all the meshes from the <see cref="GameScreen.MeshesPerEffect"/> dictionary.
        /// It groups the rendering by effect so the graphics pipeline does not have to switch
        /// between shaders so often and save some bandwidth power using the <see cref="GameScreen.DrawableComponentsPerEffect"/> dictionary.
        /// </summary>
        /// <param name="screen">The <see cref="GameScreen"/> to be rendered to a texture.</param>
        /// <param name="camera">The camera from which the render will take place.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="renderTarget">The RenderTarget that contains the texture reference to which the rendering will take place.</param>
        /// <param name="clearColor">The color to clear the context. Default: White</param>
        public void RenderZPrePass(GameScreen screen, Camera camera, SpriteBatch spriteBatch, ref PTRenderTarget2D renderTarget, List<PTMesh> excludedMeshes = null,
            Color? clearColor = null)
        {
            if (clearColor == null)
                clearColor = Color.White;

            Game.ToggleBlending(BlendingState.Disabled);

            // Render the screen using the specified render target.
            SetRenderTarget3D(renderTarget);
#if DX11
            ImmediateContext.ClearRenderTargetView(renderTarget.RenderTargetView, clearColor.Value);
            ImmediateContext.ClearDepthStencilView(Game.DepthStencilView, DepthStencilClearFlags.Depth, 1, 0);
#elif DX12
            // TODO: Render screen to texture 
#endif
            RenderScreen(screen, camera, spriteBatch, excludedMeshes);

            // Return to the default Back Buffer.
            SetRenderTarget3D(null);
            Game.ToggleBlending(BlendingState.AlphaBlending);
        }

        #region Private Methods
        /// <summary>
        /// Method to render the specified meshes in different tasks.
        /// It calls the <see cref="renderMeshesByEffect(int, int, int, List{PTMesh}, PTEffect, DeviceContext1, SpriteBatch)"/> method
        /// to apply their materials and create the necessary <see cref="CommandList"/>.
        /// </summary>
        /// <param name="threadIndex">The current used thread, this is used to pick the correct deferred context.</param>
        /// <param name="fromMesh">The index of the starting mesh to render.</param>
        /// <param name="toMesh">The index of the final mesh to render.</param>
        /// <param name="meshesToRender">The list containing all the meshes to render.</param>
        /// <param name="effect">The effect used to render the current meshes.</param>
        /// <param name="spriteBatch">The spriteBatch object.</param>
        private void renderMultiThreadByEffect(int threadIndex, int fromMesh, int toMesh, List<PTMesh> meshesToRender, PTEffect effect, SpriteBatch spriteBatch)
        {
            var index = threadIndex;
            DeviceContext1 context = PickContext(index);

            Task t = new Task(() => renderMeshesByEffect(index, fromMesh, toMesh, meshesToRender, effect, context, spriteBatch));
            tasks[index] = t;
            t.Start();
        }


        /// <summary>
        /// Render the given meshes.
        /// When rendering using <see cref="ProcessRenderingMode.Immediate"/> mode, the
        /// thread index does not matter.
        /// </summary>
        /// <param name="threadIndex"></param>
        /// <param name="fromMesh"></param>
        /// <param name="toMesh"></param>
        /// <param name="meshes"></param>
        /// <param name="effect"></param>
        /// <param name="context"></param>
        /// <param name="spriteBatch"></param>
        private void renderMeshesByEffect(int threadIndex, int fromMesh, int toMesh, List<PTMesh> meshes, PTEffect effect, DeviceContext1 context, SpriteBatch spriteBatch)
        {
            for (int i = fromMesh; i < toMesh; i++)
            {
                PTMesh mesh = meshes[i];

                // Calculate Frustum. TODO: improve.
                ContainmentType contain = Game.CurrentCamera.Frustrum.Contains(mesh.Model.BoundingBox);
                //if (contain == ContainmentType.Disjoint)
                //    continue;

                if (debugger != null)
                    debugger.ModelsDrawn++;

                PTMaterial material = null;
                if (mesh.MaterialIndex < 0 && string.IsNullOrEmpty(mesh.MaterialName))
                    material = effect.GetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY); // Applying default color material
                else if (!string.IsNullOrEmpty(mesh.MaterialName))
                    material = effect.GetMaterial(mesh.MaterialName);
                else
                    material = effect.GetMaterial(mesh.MaterialIndex);

                if (debugger != null && debugger.SimulateBurnCPU)
                {
                    for (int j = 0; j < 50; j++)
                        material.Apply(effect, mesh, context);
                }

                // TODO: apply a material just once, this means sending the code of mesh matrices calculations to some other place.
                material.Apply(effect, mesh, context);
                mesh.Render(spriteBatch, context);
            }

            if (ProcessRenderMode == ProcessRenderingMode.MultiThread)
                commandLists[threadIndex] = context.FinishCommandList(false);
        }

        /// <summary>
        /// Render the given meshes.
        /// When rendering using <see cref="ProcessRenderingMode.Immediate"/> mode, the
        /// thread index does not matter.
        /// </summary>
        /// <param name="threadIndex"></param>
        /// <param name="fromMesh"></param>
        /// <param name="toMesh"></param>
        /// <param name="meshes"></param>
        /// <param name="effect"></param>
        /// <param name="context"></param>
        /// <param name="spriteBatch"></param>
        private void renderMeshesInstancedByEffect(int threadIndex, int fromMesh, int toMesh, List<PTMesh> meshes, Effects.PTForwardRenderEffect effect, DeviceContext1 context, SpriteBatch spriteBatch)
        {
            for (int i = fromMesh; i < toMesh; i++)
            {
                PTMesh mesh = meshes[i];

                // Calculate Frustum. TODO: improve.
                ContainmentType contain = Game.CurrentCamera.Frustrum.Contains(mesh.Model.BoundingBox);
                //if (contain == ContainmentType.Disjoint)
                //    continue;

                if (debugger != null)
                    debugger.ModelsDrawn++;

                PTMaterial material = null;
                if (mesh.MaterialIndex == -1 || string.IsNullOrEmpty(mesh.MaterialName))
                    material = effect.GetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY); // Applying default color material
                else
                    material = effect.GetMaterial(mesh.MaterialName);

                if (debugger != null && debugger.SimulateBurnCPU)
                {
                    for (int j = 0; j < 50; j++)
                        material.Apply(effect, mesh, context);
                }

                // TODO: apply a material just once, this means sendind the code of mesh matrices calculations to some other place.
                material.Apply(effect, mesh, context);
                mesh.Render(spriteBatch, context);
            }

            if (ProcessRenderMode == ProcessRenderingMode.MultiThread)
                commandLists[threadIndex] = context.FinishCommandList(false);
        }
        #endregion

        /// <summary>
        /// Get the next context to render the content.
        /// This method will get the correct context no matter if the <see cref="GraphicsRenderer.ProcessRenderingMode"/> is set to Immediate or Deferred.
        /// </summary>
        /// <returns></returns>
        public SharpDX.Direct3D11.DeviceContext1 PickContext(int index)
        {
            if (ProcessRenderMode == ProcessRenderingMode.Immediate)
                return ImmediateContext;

            return deferredContexts[index];
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Utilities.Dispose(ref swapChain);
            Utilities.Dispose(ref deviceContext);
            Utilities.Dispose(ref device);
            Utilities.Dispose(ref postProcessRenderTarget);
        }
        #endregion
    }
}
