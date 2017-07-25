using PoncheToolkit.Core;
using PoncheToolkit.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core.Management.Content;
using SharpDX.Direct3D11;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Core.Services;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using PoncheToolkit.Graphics2D.Effects;
using SharpDX.Mathematics.Interop;

namespace PoncheToolkit.Graphics2D.Lighting
{
    /// <summary>
    /// Class that represent a light emitter in 2D.
    /// </summary>
    public class Light2D : UpdatableStateObject, IContentLoadable
    {
        private Vector2 position;
        private Vector2 size;

        private Game11 game;
        private PTTexture2D texture;
        private SharpDX.Direct2D1.Effect transformEffect;
        private SharpDX.Direct2D1.DeviceContext deviceContext;
        private Dynamic2DLightManagerService manager;

        /// <inheritdoc/>
        public bool IsContentLoaded { get; set; }

        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;

        PTTexture2D oclussionTexture;
        public PTTexture2D OclussionTexture
        {
            get { return oclussionTexture; }
        }

        ///// <summary>
        ///// Get the <see cref="SharpDX.Direct2D1.RenderTarget"/> where the rendering of light will happen.
        ///// </summary>
        //public SharpDX.Direct2D1.RenderTarget RenderTarget
        //{
        //    get { return renderTarget; }
        //}

        /// <summary>
        /// Get the device context used to render the light.
        /// </summary>
        public SharpDX.Direct2D1.DeviceContext DeviceContext
        {
            get { return deviceContext; }
        }

        /// <summary>
        /// Get or set the position of the light.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Get the size of the light.
        /// </summary>
        public Vector2 Size
        {
            get { return size; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="manager">The manager service that created this light.</param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        public Light2D(Game11 game, Dynamic2DLightManagerService manager, Vector2 position, Vector2 size)
        {
            this.game = game;
            this.manager = manager;
            this.position = position;
            this.size = size;
        }

        /// <inheritdoc/>
        public void LoadContent(IContentManager contentManager)
        {
            // Create the texture for the render target view.
            Texture2DDescription textureDesc = new Texture2DDescription()
            {
                Width = (int)manager.ShadowMapSizeVector.X,
                Height = (int)manager.ShadowMapSizeVector.Y,
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                //Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
            };
            Texture2D t = new Texture2D(game.Renderer.Device, textureDesc);
            texture = new PTTexture2D(game.Renderer.Device, t);


            // Create the Bitmap render target.
            Surface surface = texture.Texture.QueryInterface<Surface>();
            deviceContext = game.Renderer.SpriteBatch.CreateOfflineDeviceContext();
            Bitmap1 target = new Bitmap1(deviceContext, surface, game.Renderer.BitmapProperties2D);
            deviceContext.Target = target;

            Utilities.Dispose(ref surface);
            Utilities.Dispose(ref target);

            oclussionTexture = texture;
            LightBrush = new SolidColorBrush(game.Renderer.Context2D, Color.Yellow);
            transformEffect = new SharpDX.Direct2D1.Effect(game.Renderer.Context2D, SharpDX.Direct2D1.Effect.AffineTransform2D);

            ToDispose(texture);

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Begin the draw.
        /// Between this method and the <see cref="End"/> method, all the objects that wants to be rendered with shadows
        /// must be drawn.
        /// </summary>
        /// <param name="depthView"></param>
        internal void Begin(DepthStencilView depthView)
        {
            //game.Renderer.ImmediateContext.OutputMerger.SetRenderTargets(depthView, renderTarget);
            //game.Renderer.ImmediateContext.ClearRenderTargetView(renderTarget, Color.White);
            //game.Renderer.ImmediateContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1, 0);


        }

        /// <summary>
        /// End the shadow rendering. Here are applied the effects to blend the result.
        /// </summary>
        internal void End(DepthStencilView depthView)
        {
            // Set the render target to the back buffer
            //game.Renderer.Device.ImmediateContext.OutputMerger.SetRenderTargets(depthView, game.RenderTargetView);
            game.Renderer.Device.ImmediateContext.OutputMerger.SetRenderTargets(depthView, game.BackBufferRenderTarget.RenderTargetView);
        }

        /// <summary>
        /// Converts a position in the x,y coordinate to center of a light.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public Vector2 ToRelativePosition(Vector2 worldPosition)
        {
            return worldPosition - (position - (size * 0.5f));
        }

        public Brush LightBrush;
        /// <summary>
        /// Apply its state to the GPU.
        /// </summary>
        public virtual void ApplyShadowMap(SpriteBatch spriteBatch, SharpDX.Direct2D1.DeviceContext context, PTShadowMapEffect shadowMap, PTShadowRenderEffect shadowRender, Bitmap1 occludersTarget, SharpDX.Direct2D1.Effects.BitmapSource source)
        {
            spriteBatch.Begin(Matrix3x2.Translation(Position - (Size / 2)));
            //spriteBatch.Begin();

            game.Renderer.SetRenderTarget2D(shadowMap.Target);
            game.Renderer.Context2D.Clear(Color.Transparent);
            //game.Renderer.Context2D.Clear(new Color(0, 0.3f, 0, 0.5f));

            shadowMap.Effect.SetValue((int)PTShadowMapEffectProperties.Upscale, 1f);
            //shadowMap.Effect.SetValue((int)PTShadowMapEffectProperties.Resolution, (RawVector2)shadowMap.ShadowMapSize);
            shadowMap.Effect.SetValue((int)PTShadowMapEffectProperties.Resolution, (RawVector2)size);
            //shadowMap.Effect.SetValue((int)PTShadowMapEffectProperties.Resolution, new RawVector2(size.X, shadowMap.Target.Size.Height));

            //transformEffect.SetInput(0, occludersTarget, true);
            //transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, (RawMatrix3x2)Matrix3x2.Translation(new Vector2(10, 400)));
            //transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, (RawMatrix3x2)Matrix3x2.Translation(position));
            //shadowMap.Effect.SetInputEffect(0, transformEffect, true);
            shadowMap.Effect.SetInput(0, occludersTarget, false);

            
            //source.WicBitmapSource = occludersTarget;
            //shadowMap.Effect.SetInputEffect(0, source, true);

            game.Renderer.Context2D.DrawImage(shadowMap.Effect);
            //spriteBatch.End();


            // LINKED EFFECTS
            //spriteBatch.Begin();
            ////game.Renderer.SetRenderTarget2D(shadowMap.Target);
            ////game.Renderer.SetRenderTarget2D(shadowRender.Target);
            //game.Renderer.SetRenderTarget2D(null);

            //shadowMap.Effect.SetValue((int)PTShadowMapEffectProperties.Upscale, 1f);
            //shadowMap.Effect.SetValue((int)PTShadowMapEffectProperties.Resolution, (RawVector2)size);
            //shadowRender.Effect.SetValue((int)PTShadowRenderEffectProperties.SoftShadows, 1f);
            //shadowRender.Effect.SetValue((int)PTShadowRenderEffectProperties.Resolution, (RawVector2)size);
            //shadowRender.Effect.SetValue((int)PTShadowRenderEffectProperties.Color, (RawVector4)new Color(0.8f, 0.8f, 0.8f, 1f).ToVector4());

            //transformEffect.SetInput(0, occludersTarget, true);
            ////transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, (RawMatrix3x2)Matrix3x2.Translation(ToRelativePosition(position)));
            ////transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, (RawMatrix3x2)Matrix3x2.Translation(position));
            //shadowMap.Effect.SetInputEffect(0, transformEffect, true);
            //shadowRender.Effect.SetInputEffect(0, shadowMap.Effect, true);

            //game.Renderer.Context2D.DrawImage(shadowRender.Effect);

            //game.Renderer.SetRenderTarget2D(null);
            //game.Renderer.Context2D.DrawRectangle(new RawRectangleF(position.X, position.Y, position.X + 5, position.Y + 5), lightBrush, 3);

            spriteBatch.End();
            game.Renderer.SetRenderTarget2D(null);
        }
        
        /// <summary>
        /// Apply its state to the GPU.
        /// </summary>
        public virtual void RenderShadow(SpriteBatch spriteBatch, SharpDX.Direct2D1.DeviceContext context, PTShadowMapEffect shadowMap, PTShadowRenderEffect shadowRender, Bitmap1 occludersTarget)
        {
            spriteBatch.Begin();

            game.Renderer.SetRenderTarget2D(shadowRender.Target);
            //game.Renderer.SetRenderTarget2D(null);
            //game.Renderer.Context2D.Clear(Color.Transparent);
            //game.Renderer.Context2D.Clear(new Color(0, 0, 0, 50));

            shadowRender.Effect.SetValue((int)PTShadowRenderEffectProperties.Color, (RawVector4)new Color(0.5f, 0.2f, 0.3f, 0.5f).ToVector4());
            //shadowRender.Effect.SetValue((int)PTShadowRenderEffectProperties.Color, (RawVector4)new Color(0.8f, 0.8f, 0.8f, 0.1f).ToVector4());
            shadowRender.Effect.SetValue((int)PTShadowRenderEffectProperties.Resolution, (RawVector2)size);
            shadowRender.Effect.SetValue((int)PTShadowRenderEffectProperties.SoftShadows, 1f);


            //transformEffect.SetInput(0, shadowMap.Target, true);
            //transformEffect.SetInputEffect(0, shadowMap.Effect, true);
            //transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, (RawMatrix3x2)Matrix3x2.Translation(ToRelativePosition(position)));
            //transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, (RawMatrix3x2)Matrix3x2.Translation(position));
            //transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, (RawMatrix3x2)Matrix3x2.Rotation(MathUtil.DegreesToRadians(90)));

            //shadowRender.Effect.SetInput(0, occludersTarget, false);
            //shadowRender.Effect.SetInput(0, shadowMap.Effect.Output, false);
            shadowRender.Effect.SetInputEffect(0, shadowMap.Effect, true);
            //shadowRender.Effect.SetInput(0, shadowMap.Target, true);

            game.Renderer.Context2D.DrawImage(shadowRender.Effect);

            spriteBatch.End();
            game.Renderer.SetRenderTarget2D(null);
        }

        ///// <summary>
        ///// Apply its state to the GPU.
        ///// </summary>
        //public virtual void ApplyShadowMap(SharpDX.Direct3D11.DeviceContext1 context, PTShadowMapEffect2D shadowMap)
        //{
        //    //context.PixelShader.SetSampler(0, Sampler);
        //    //context.PixelShader.SetShaderResource(0, texture);

        //    ShadowMapStruct shadowStruct = new ShadowMapStruct(manager.ShadowMapSizeVector, 1);
        //    DataBox dataBox = context.MapSubresource(shadowMap.ShadowMapConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
        //    Utilities.Write(dataBox.DataPointer, ref shadowStruct);
        //    context.UnmapSubresource(shadowMap.ShadowMapConstantBuffer, 0);
        //}

        ///// <summary>
        ///// Apply its state to the GPU.
        ///// </summary>
        //public virtual void ApplyRenderBlur(SharpDX.Direct3D11.DeviceContext1 context)
        //{
        //    //context.PixelShader.SetSampler(0, Sampler);
        //    //context.PixelShader.SetShaderResource(0, texture);

        //    ShadowMapStruct shadowStruct = new ShadowMapStruct(manager.ShadowMapSizeVector, 1);
        //    DataBox dataBox = context.MapSubresource(manager.ShadowMapEffect.ShadowMapConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
        //    Utilities.Write(dataBox.DataPointer, ref shadowStruct);
        //    context.UnmapSubresource(manager.ShadowMapEffect.ShadowMapConstantBuffer, 0);
        //}

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

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
