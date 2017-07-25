using System;
using PoncheToolkit.Core;
using PoncheToolkit.Util;
using SharpDX.Direct3D11;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.DXGI;
using PoncheToolkit.Core.Management.Content;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PoncheToolkit.Graphics2D.Effects;
using SharpDX.Mathematics.Interop;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// Class that represent the effect that at least all the models will have.
    /// Has basic lights just for a minimum "good looking" rendering.
    /// </summary>
    public class PTToonEffect : PTForwardRenderEffect
    {
        private PTEdgeDetectionEffect edgeEffect;
        private PTRippleEffect rippleEffect;
        private Buffer toonConstantBuffer;

        /// <summary>
        /// The restriction of the 4 toon shadow intensity.
        /// Uses x, y, z, w, going from bright to shadow.
        /// The values must be between 0 - 1.
        /// </summary>
        public Vector4 ToonLimits;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="vertexShaderPath">The path to use for the .fx file.</param>
        /// <param name="pixelShaderPath">The path to use for the .fx file.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        public PTToonEffect(Game11 game, string vertexShaderPath, string pixelShaderPath, List<string> includePaths)
            : this(game, vertexShaderPath, pixelShaderPath, includePaths, -1)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="vertexShaderPath">The path to use for the .fx file.</param>
        /// <param name="pixelShaderPath">The path to use for the .fx file.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="customPixelShaderSlot">The slot to be used if a custom shader want to be used. If no custom shader is going to be used, set it o -1.
        public PTToonEffect(Game11 game, string vertexShaderPath, string pixelShaderPath, List<string> includePaths, int customPixelShaderSlot = -1)
            : base(game, vertexShaderPath, pixelShaderPath, includePaths, customPixelShaderSlot)
        {
            //this.ClipPlainVector = new Vector4(1000, 1000, 1000, 1);
            this.ClipPlainVector = new Vector4(-1, 1, 1, 100);
            this.LightsBuffer = new LightBufferStruct(null);
            this.ToonLimits = new Vector4(0.85f, 0.5f, 0.1f, 0);
        }

        /// <inheritdoc/>
        public override void LoadContent(IContentManager contentManager)
        {
            base.LoadContent(contentManager);

            edgeEffect = new PTEdgeDetectionEffect();
            edgeEffect.Register(Game.Renderer.SpriteBatch.D2dFactory, Game.Renderer.Context2D);
            edgeEffect.Order = 0;

            rippleEffect = new PTRippleEffect();
            rippleEffect.Register(Game.Renderer.SpriteBatch.D2dFactory, Game.Renderer.Context2D);
            rippleEffect.Order = 1;
        }

        /// <inheritdoc/>
        public override void LoadBuffers()
        {
            base.LoadBuffers();
            toonConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<Vector4>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                // If the effect is added to any mesh to be used, register it.
                if (DirtyProperties.ContainsKey(nameof(IsUsed)))
                {
                    edgeEffect.IsUsed = true;
                    rippleEffect.IsUsed = true;
                }

                IsStateUpdated = true;
                OnStateUpdated();
            }
            return IsStateUpdated;
        }

        /// <inheritdoc/>
        public override void Apply(DeviceContext1 context)
        {
            base.Apply(context);

            context.PixelShader.SetConstantBuffer(3, toonConstantBuffer);
            var dataBox = context.MapSubresource(toonConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            Utilities.Write(dataBox.DataPointer, ref ToonLimits);
            context.UnmapSubresource(toonConstantBuffer, 0);

            // Apply the Edge detection post process.
            edgeEffect.Effect.SetValue((int)PTEdgeDetectionEffectProperties.ScreenSize, (RawVector2)new Vector2(Game.Settings.Resolution.Width, Game.Settings.Resolution.Height));
            edgeEffect.Effect.SetValue((int)PTEdgeDetectionEffectProperties.Thickness, edgeEffect.Thickness);
            edgeEffect.Effect.SetValue((int)PTEdgeDetectionEffectProperties.Threshold, edgeEffect.Threshold);

            // Apply ripple parameters
            float delta = MathUtil.Clamp((Game.GameTime.GameTimeElapsed.Milliseconds / 1000), 0, 10);
            rippleEffect.Effect.SetValue((int)PTRippleEffectProperties.Frequency, 140.0f - delta * 40.0f);
            rippleEffect.Effect.SetValue((int)PTRippleEffectProperties.Phase, -delta * 25.0f);
            rippleEffect.Effect.SetValue((int)PTRippleEffectProperties.Amplitude, 60.0f - delta * 25.0f);
            rippleEffect.Effect.SetValue((int)PTRippleEffectProperties.Spread, 0.05f + delta / 10.0f);
            rippleEffect.Effect.SetValue((int)PTRippleEffectProperties.Center, (RawVector2)Game.InputManager.MousePosition);
        }
    }
}
