using System;
using PoncheToolkit.Core;
using PoncheToolkit.Util;
using SharpDX.Direct3D11;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.DXGI;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX.Direct3D;
using PoncheToolkit.Graphics3D;
using SharpDX.Direct2D1;

namespace PoncheToolkit.Graphics2D.Effects
{
    /// <summary>
    /// Class that represent the effect that at least all the models will have.
    /// Has basic lights just for a minimum "good looking" rendering.
    /// </summary>
    public class PTShadowMapEffect2D : PTEffect
    {
        public static string NAME_PATH = "Effects/PT2DShadowMapEffect.fx";

        private SharpDX.Direct2D1.Bitmap1 target;

        /// <summary>
        /// The buffer to hold the shadow map parameters.
        /// </summary>
        public Buffer ShadowMapConstantBuffer;

        /// <summary>
        /// The size of the shadow map.
        /// </summary>
        public Vector2 ShadowMapSize;

        /// <summary>
        /// Get the Bitmap used as render target.
        /// </summary>
        public SharpDX.Direct2D1.Bitmap1 Target
        {
            get { return target; }
        }


        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;

        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="shadowMapSize"></param>
        public PTShadowMapEffect2D(Game11 game, Vector2 shadowMapSize)
            : base(game, NAME_PATH)
        {
            this.ShadowMapSize = shadowMapSize;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void LoadContent(ContentManager contentManager)
        {
            // Compile Vertex and Pixel shaders
            try
            {
                base.LoadContent(contentManager);

                Shader.InputLayout = new InputLayout(Game.Renderer.Device, Shader.VertexShaderSignature, new[]
                {
                    new SharpDX.Direct3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    new SharpDX.Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, SharpDX.Direct3D11.InputElement.AppendAligned, 0),
                    new SharpDX.Direct3D11.InputElement("TEXCOORD", 0, Format.R32G32_Float, SharpDX.Direct3D11.InputElement.AppendAligned, 0),
                });

                MatricesConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<MatricesStruct2D>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
                ShadowMapConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<ShadowMapStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
                ToDispose(MatricesConstantBuffer);
                ToDispose(ShadowMapConstantBuffer);

                //// Create the texture for the shadows render target view.
                //Texture2DDescription textureDesc = new Texture2DDescription()
                //{
                //    Width = (int)ShadowMapSize.X,
                //    Height = 1,
                //    ArraySize = 1,
                //    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                //    Usage = ResourceUsage.Default,
                //    CpuAccessFlags = CpuAccessFlags.None,
                //    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                //    MipLevels = 1,
                //    OptionFlags = ResourceOptionFlags.None,
                //    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
                //};
                //Texture2D t = new Texture2D(Game.Renderer.Device, textureDesc);
                //texture = contentManager.LoadTexture2D(this.GetType().Name, t);
                
                
                //Bitmap1 target = new Bitmap1(deviceContext, surface, Game.Renderer.BitmapProperties2D);
                target = new Bitmap1(Game.Renderer.Context2D, new Size2((int)ShadowMapSize.X, 1), 
                    new BitmapProperties1(new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    Game.Renderer.BitmapProperties2D.DpiX,
                    Game.Renderer.BitmapProperties2D.DpiY, 
                    BitmapOptions.Target 
                    //| BitmapOptions.CannotDraw
                    ));

                //Utilities.Dispose(ref surface);

                ToDispose(target);
            }
            catch (Exception ex)
            {
                Log.Error("Error when loading effect.", ex);
                throw;
            }

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Apply its state to the GPU.
        /// </summary>
        public override void Apply(SharpDX.Direct3D11.DeviceContext1 context)
        {
            //Game.Renderer.ImmediateContext.OutputMerger.SetRenderTargets(Game.DepthStencilView, renderTarget);
            //Game.Renderer.Device.ImmediateContext.ClearRenderTargetView(renderTarget, Color.Black);

            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            // Set the shaders
            context.VertexShader.Set(Shader.VertexShader);
            context.PixelShader.Set(Shader.PixelShader);

            // Set the input layout
            context.InputAssembler.InputLayout = Shader.InputLayout;

            // Set the matrices and lights constant buffers.
            context.VertexShader.SetConstantBuffer(0, MatricesConstantBuffer);

            if (Game.Renderer.RenderType == GraphicsRenderer.RenderingType.Deferred)
            {
                context.Rasterizer.SetViewport(Game.Viewports[0]);
                context.OutputMerger.SetTargets(Game.DepthStencilView, Game.RenderTargetView);
            }

            // Set the matrices constant buffer.
            Matrix proj = Game.CurrentCamera.Projection;

            DataBox dataBox = context.MapSubresource(MatricesConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            Utilities.Write(dataBox.DataPointer, ref proj);
            context.UnmapSubresource(MatricesConstantBuffer, 0);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
