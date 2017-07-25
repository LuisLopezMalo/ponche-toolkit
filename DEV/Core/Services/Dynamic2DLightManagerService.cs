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
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Core.Services;

using PoncheToolkit.Graphics2D.Lighting;
using PoncheToolkit.Graphics2D.Effects;
using SharpDX.DXGI;
using SharpDX.Direct2D1;

namespace PoncheToolkit.Core.Services
{
    using Graphics2D.Animation;
    using SharpDX.Mathematics.Interop;
    using PoncheToolkit.Graphics2D;

    /// <summary>
    /// Class that manages all lights that can cast shadows in 2D.
    /// Has a Rendertarget to create the shadow map. (Occlusion).
    /// The default value for the shadow map is 512x512.
    /// </summary>
    public class Dynamic2DLightManagerService : GameService, IContentLoadable
    {
        /// <summary>
        /// Enumeration to create the shadow map with this size.
        /// </summary>
        public enum ShadowMapSize
        {
            /// <summary>
            /// Size of 128x128
            /// </summary>
            Size128 = 1,
            /// <summary>
            /// Size of 256x256
            /// </summary>
            Size256 = 2,
            /// <summary>
            /// Size of 512x512
            /// </summary>
            Size512 = 3,
            /// <summary>
            /// Size of 1024x1024
            /// </summary>
            Size1024 = 4
        }

        #region Fields
        private Game11 game;
        private ShadowMapSize shadowMapSize;
        private Vector2 shadowMapSizeVector;
        //private PTShadowMapEffect2D shadowMapEffect;
        //private PTShadowRenderEffect2D shadowRenderEffect;
        private PTShadowMapEffect shadowMapEffect;
        private PTShadowRenderEffect shadowRenderEffect;
        private PTRippleEffect ripple;

        private bool isRendering;

        private List<Light2D> ligths;
        public Bitmap1 occludersRenderTarget;
        public SharpDX.Direct2D1.Effects.BitmapSource occludersBitmapSource;
        private List<Sprite> spriteCasters;
        #endregion

        #region Properties
        /// <summary>
        /// List with the lights used.
        /// </summary>
        public List<Light2D> Lights
        {
            get { return ligths; }
        }

        /// <summary>
        /// The <see cref="PTShadowMapEffect"/> effect used to create the shadow maps.
        /// </summary>
        public PTShadowMapEffect ShadowMapEffect
        {
            get { return shadowMapEffect; }
        }

        /// <summary>
        /// The <see cref="PTShadowRenderEffect"/> effect used to render the final image of the shadows.
        /// </summary>
        public PTShadowRenderEffect ShadowRenderEffect
        {
            get { return shadowRenderEffect; }
        }

        /// <summary>
        /// Get or set the shadow map size.
        /// Bigger the size, the more resolution the shadows will have but it could impact performance.
        /// </summary>
        public ShadowMapSize SizeShadowMap
        {
            get { return shadowMapSize; }
            set
            {
                switch (value)
                {
                    case ShadowMapSize.Size128:
                        this.shadowMapSizeVector = new Vector2(128, 128);
                        break;
                    case ShadowMapSize.Size256:
                        this.shadowMapSizeVector = new Vector2(256, 256);
                        break;
                    case ShadowMapSize.Size512:
                        this.shadowMapSizeVector = new Vector2(512, 512);
                        break;
                    case ShadowMapSize.Size1024:
                        this.shadowMapSizeVector = new Vector2(1024, 1024);
                        break;
                }
                shadowMapSize = value;
            }
        }

        /// <summary>
        /// Get the size of the shadow map in <see cref="Vector2"/>
        /// </summary>
        public Vector2 ShadowMapSizeVector
        {
            get { return shadowMapSizeVector; }
        }

        /// <inheritdoc/>
        public bool IsContentLoaded { get; set; }
        #endregion

        #region Events
        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game isntance.</param>
        /// <param name="shadowMapSize">The size of the texture to create shadows. Bigger the size, the more resolution the shadows will have but
        /// it could impact performance.</param>
        public Dynamic2DLightManagerService(Game11 game, ShadowMapSize shadowMapSize)
            : base(game)
        {
            this.game = game;
            this.SizeShadowMap = shadowMapSize;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            this.ligths = new List<Light2D>();
            isRendering = false;

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public void LoadContent(IContentManager contentManager)
        {
            //// Create the shader resource view.
            //ShaderResourceViewDescription shaderDesc = new ShaderResourceViewDescription()
            //{
            //    Format = textureDesc.Format,
            //    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
            //};
            //shaderDesc.Texture2D.MostDetailedMip = 0;
            //shaderDesc.Texture2D.MipLevels = 1;
            //shaderResourceView = new ShaderResourceView(game.Renderer.Device, texture, shaderDesc);

            occludersRenderTarget = new Bitmap1(game.Renderer.Context2D, new Size2((int)shadowMapSizeVector.X, (int)shadowMapSizeVector.Y),
                    new BitmapProperties1(new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    game.Renderer.BitmapProperties2D.DpiX,
                    game.Renderer.BitmapProperties2D.DpiY,
                    BitmapOptions.Target));

            //occludersBitmapSource = new SharpDX.Direct2D1.Effects.BitmapSource(game.Renderer.Context2D);
            //occludersBitmapSource.Cached = true;

            // Load the shaders
            //shadowMapEffect = new PTShadowMapEffect2D(game, shadowMapSizeVector);
            //shadowMapEffect.Initialize();
            //shadowMapEffect.LoadContent(contentManager);
            //shadowRenderEffect = new PTShadowRenderEffect2D(game);
            //shadowRenderEffect.Initialize();
            //shadowRenderEffect.LoadContent(contentManager);

            shadowMapEffect = new PTShadowMapEffect();
            shadowMapEffect.ShadowMapSize = shadowMapSizeVector;
            shadowMapEffect.Register(game.Renderer.SpriteBatch.D2dFactory, game.Renderer.Context2D);

            shadowRenderEffect = new PTShadowRenderEffect();
            shadowRenderEffect.ShadowMapSize = shadowMapSizeVector;
            shadowRenderEffect.Register(game.Renderer.SpriteBatch.D2dFactory, game.Renderer.Context2D);

            ripple = new PTRippleEffect();
            ripple.Register(game.Renderer.SpriteBatch.D2dFactory, game.Renderer.Context2D);

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Create a new light in the specified position.
        /// The size of the light is the one defined by the <see cref="ShadowMapSizeVector"/> property.
        /// Add it to the <see cref="Lights"/> List.
        /// </summary>
        /// <param name="position"></param>
        public Light2D CreateLight360(Vector2 position)
        {
            Light2D light = new Light2D(game, this, position, shadowMapSizeVector);
            light.LoadContent(Game.ContentManager);
            ligths.Add(light);
            return light;
        }

        /// <summary>
        /// Begin the draw.
        /// Between this method and the <see cref="End"/> method, all the objects that wants to be rendered with shadows
        /// for the given light must be drawn. There must be only one Begin foreach End call. Only one light rendering at a time.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch.</param>
        /// <param name="light">The light used to render the 2D content.</param>
        public void Begin(SpriteBatch spriteBatch, Light2D light)
        {
            if (isRendering)
                throw new Util.Exceptions.RenderingException(@"You need to complete the rendering cycle for each light, calling the End method
                    for the light before calling Begin for a new light");

            //game.Renderer.ImmediateContext.OutputMerger.SetRenderTargets(game.DepthStencilView, light.RenderTargetView);
            //game.Renderer.Device.ImmediateContext.ClearRenderTargetView(light.RenderTargetView, Color.White);
            ////game.Renderer.Device.ImmediateContext.ClearDepthStencilView(game.DepthStencilView, DepthStencilClearFlags.Depth, 1, 0);


            //game.ToggleAlphaBlending(true);
            game.Renderer.SetRenderTarget2D(occludersRenderTarget);
            //game.Renderer.SetRenderTarget2D(occludersBitmapSource.Output);
            //game.Renderer.SpriteBatch.Begin();
            spriteBatch.Begin(Matrix3x2.Translation(light.Position - (light.Size / 2)));
            game.Renderer.Context2D.Clear(Color.Transparent);
            

            isRendering = true;
        }

        /// <summary>
        /// Begin the draw.
        /// Between this method and the <see cref="End"/> method, all the objects that wants to be rendered with shadows
        /// must be drawn.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch.</param>
        /// <param name="lightIndex">The light used to render the 2D content.</param>
        public void Begin(SpriteBatch spriteBatch, int lightIndex)
        {
            Light2D light = Lights[lightIndex];
            this.Begin(spriteBatch, light);
        }

        /// <summary>
        /// End the shadow rendering. Here are applied the effects to blend the result.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch.</param>
        /// <param name="light">The light used to render the 2D content.</param>
        public void End(SpriteBatch spriteBatch, Light2D light)
        {
            if (!isRendering)
                return;

            // Ends the occlusion rendering
            spriteBatch.End();

            //Apply the shadow effects in the GPU
            //spriteBatch.Begin(Matrix3x2.Translation(light.Position - (light.Size / 2)));
            light.ApplyShadowMap(spriteBatch, game.Renderer.Context2D, shadowMapEffect, shadowRenderEffect, occludersRenderTarget, occludersBitmapSource);
            light.RenderShadow(spriteBatch, game.Renderer.Context2D, shadowMapEffect, shadowRenderEffect, occludersRenderTarget);
            // Draw the final elements.
            //game.Renderer.Context2D.DrawBitmap(occludersRenderTarget, 1, SharpDX.Direct2D1.InterpolationMode.Linear);
            //game.Renderer.Context2D.DrawRectangle(new RawRectangleF(light.Position.X, light.Position.Y, light.Position.X + 5, light.Position.Y + 5), light.LightBrush, 3);
            //spriteBatch.End();

            game.Renderer.SetRenderTarget2D(null);

            // Draw the final elements.
            spriteBatch.Begin();
            
            //game.Renderer.Context2D.DrawBitmap(occludersRenderTarget, 1, SharpDX.Direct2D1.InterpolationMode.Linear);
            game.Renderer.Context2D.DrawBitmap(shadowRenderEffect.Target, 1, SharpDX.Direct2D1.InterpolationMode.Linear);
            //game.Renderer.Context2D.DrawImage(shadowRenderEffect.Effect.Output);
            DrawCasters(spriteBatch, spriteCasters);
            game.Renderer.Context2D.DrawRectangle(new RawRectangleF(light.Position.X, light.Position.Y, light.Position.X + 5, light.Position.Y + 5), light.LightBrush, 3);
            spriteBatch.End();

            //// Draw the shadow map to the 1D texture.


            //game.Renderer.SetRenderTarget2D(occludersRenderTarget);
            //spriteBatch.Begin();
            //ripple.Effect.SetInput(0, occludersRenderTarget, true);
            //float delta = MathUtil.Clamp((Game.GameTime.GameTimeElapsed.Milliseconds / 1000), 0, 10);
            //ripple.Effect.SetValue((int)PTRippleEffectProperties.Frequency, 140.0f - delta * 30.0f);
            //ripple.Effect.SetValue((int)PTRippleEffectProperties.Phase, -delta * 20.0f);
            //ripple.Effect.SetValue((int)PTRippleEffectProperties.Amplitude, 60.0f - delta * 15.0f);
            //ripple.Effect.SetValue((int)PTRippleEffectProperties.Spread, 0.01f + delta / 10.0f);
            //ripple.Effect.SetValue((int)PTRippleEffectProperties.Center, (RawVector2)Game.InputManager.MousePosition);
            //game.Renderer.Context2D.DrawImage(ripple.Effect);
            //spriteBatch.End();
            //game.Renderer.SetRenderTarget2D(null);

            isRendering = false;
        }

        public void DrawCasters(SpriteBatch spriteBatch, List<Sprite> sprites)
        {
            spriteCasters = sprites;
            foreach (Sprite sprite in sprites)
            {
                if (sprite is AnimatedSprite)
                    spriteBatch.DrawTexture(sprite.Texture, sprite.Position, (sprite as AnimatedSprite).CurrentAnimation.CurrentFrame.SourceRectangle, sprite.Scale);
                else
                    spriteBatch.DrawTexture(sprite.Texture, sprite.Position, sprite.Scale);
            }
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            shadowMapEffect.Dispose();
            shadowRenderEffect.Dispose();
        }
        #endregion
    }
}
