using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX;
using SharpDX.Mathematics.Interop;
using System.Runtime.InteropServices;
using SharpDX.D3DCompiler;
using SharpDX.DXGI;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Graphics2D.Effects
{
    /// <summary>
    /// Class that creates a 2D Custom Effect for shadow maps.
    /// </summary>
    [CustomEffect("A custom effect to render a shadow from a ShadowMap", "CustomEffects", "PoncheToolkit", DisplayName = "Render shadow effect")]
    [CustomEffectInput("Source")]
    public class PTShadowRenderEffect : PTCustomEffect
    {
        private ShadowRenderEffectConstantBuffer buffer;
        private Guid guid;
        private Bitmap1 target;
        private Dynamic2DLightManagerService lightManager;

        #region Properties
        /// <summary>
        /// The size of the shadow map.
        /// </summary>
        public Vector2 ShadowMapSize { get; internal set; }

        /// <summary>
        /// Get or set the render target.
        /// </summary>
        public Bitmap1 Target
        {
            get { return target; }
            set { target = value; }
        }

        /// <summary>
        /// Get or set the assigned guid for this effect
        /// </summary>
        public override Guid Guid
        {
            get { return guid; }
            set { guid = value; }
        }

        /// <summary>
        /// The service that manages the dynamic 2D lighting.
        /// </summary>
        public Dynamic2DLightManagerService LightManager
        {
            get { return lightManager; }
            set { lightManager = value; }
        }

        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        [PropertyBinding((int)PTShadowRenderEffectProperties.Color, "(0.0, 0.0, 0.0, 0.0)", "(1.0, 1.0, 1.0, 1.0)", "(0.0, 0.0, 0.0, 0.0)")]
        public Vector4 Color
        {
            get { return buffer.Color; }
            set { buffer.Color = value; }
        }

        /// <summary>
        /// Gets or sets the spread.
        /// </summary>
        [PropertyBinding((int)PTShadowRenderEffectProperties.SoftShadows, "0.0", "1.0", "1.0")]
        public float SoftShadows
        {
            get { return buffer.SoftShadows; }
            set { buffer.SoftShadows = MathUtil.Clamp(value, 0, 1); }
        }

        /// <summary>
        /// Gets or sets the resolution of the shadow map.
        /// </summary>
        [PropertyBinding((int)PTShadowRenderEffectProperties.Resolution, "(0.0, 0.0)", "(1024.0, 1024.0)", "(0.0, 0.0)")]
        public Vector2 Resolution
        {
            get { return buffer.Resolution; }
            set { buffer.Resolution = value; }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTShadowRenderEffect()
        {
            buffer = new ShadowRenderEffectConstantBuffer();
            guid = Guid.NewGuid();
        }

        /// <inheritdoc/>
        public override void Initialize(EffectContext effectContext, TransformGraph transformGraph)
        {
            string shaderPath = "Effects/PostProcess/PTShadowRenderEffect.fx";
            PTShader shader = new PTShader();
            shader.LoadContent(Game.Instance.ContentManager);
            Game.Instance.ContentManager.LoadPixelShaderInto(shaderPath, ref shader, -1, new List<string>() { "Effects/PostProcess" });

            if (shader == null)
                throw new ArgumentNullException("Error loading shadow render shader.");

            byte[] bytes = SharpDX.IO.NativeFile.ReadAllBytes(
                System.IO.Path.Combine(Game.Instance.ContentDirectoryFullPath,
                System.IO.Path.GetDirectoryName(shaderPath),
                System.IO.Path.GetFileNameWithoutExtension(shaderPath) + ContentManager11.PIXEL_SHADER_COMPILED_NAME_EXTENSION));

            effectContext.LoadPixelShader(this.Guid, bytes);
            transformGraph.SetSingleTransformNode(this);

            shader.Dispose();
            Utilities.Dispose(ref shader);
            bytes = null;
        }

        /// <inheritdoc/>
        public override void UpdateBuffer(ChangeType changeType)
        {
            if (DrawInfo != null)
                DrawInfo.SetPixelConstantBuffer(ref buffer);
        }

        /// <inheritdoc/>
        public override void Register(SharpDX.Direct2D1.Factory1 factory, DeviceContext context)
        {
            if (!factory.RegisteredEffects.Contains(this.Guid))
                factory.RegisterEffect<PTShadowRenderEffect>(this.Guid);

            Effect = new Effect(context, this.Guid);

            target = new Bitmap1(context, new Size2((int)ShadowMapSize.X, (int)ShadowMapSize.Y),
                    new BitmapProperties1(new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    96,
                    96,
                    BitmapOptions.Target));

            //lightManager = game.Services[typeof(Dynamic2DLightManagerService)] as Dynamic2DLightManagerService;
        }

        /// <inheritdoc/>
        public override void MapOutputRectangleToInputRectangles(RawRectangle outputRect, RawRectangle[] inputRects)
        {
            if (inputRects.Length != 1)
                throw new ArgumentException("InputRects must be length of 1", "inputRects");

            inputRects[0].Left = outputRect.Left;
            inputRects[0].Top = outputRect.Top;
            //inputRects[0].Right = outputRect.Right;
            //inputRects[0].Bottom = outputRect.Bottom;
            inputRects[0].Right = inputRects[0].Left + 512;
            inputRects[0].Bottom = inputRects[0].Top + 512;
            //inputRects[0].Right = inputRects[0].Left + (int)ShadowMapSize.X;
            //inputRects[0].Bottom = inputRects[0].Top + (int)ShadowMapSize.Y;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Utilities.Dispose(ref target);
        }

        /// <summary>
        /// Internal structure used for the constant buffer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ShadowRenderEffectConstantBuffer
        {
            public Vector4 Color;
            public Vector2 Resolution;
            public float SoftShadows;
            public float Padding;
        }
    }
}
