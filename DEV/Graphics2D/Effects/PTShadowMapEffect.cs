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

namespace PoncheToolkit.Graphics2D.Effects
{
    /// <summary>
    /// Class that creates a 2D Custom Effect for shadow maps.
    /// </summary>
    [CustomEffect("A custom effect to generate a shadow map", "CustomEffects", "PoncheToolkit", DisplayName = "Shadow map effect")]
    [CustomEffectInput("Source")]
    public class PTShadowMapEffect : PTCustomEffect
    {
        private ShadowMapEffectConstantBuffer buffer;
        private Guid guid;
        private Bitmap1 target;

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
        /// Gets or sets the spread.
        /// </summary>
        [PropertyBinding((int)PTShadowMapEffectProperties.Upscale, "1.0", "5.0", "1.0")]
        public float Upscale
        {
            get { return buffer.UpScale; }
            set { buffer.UpScale = MathUtil.Clamp(value, 1, 5); }
        }

        /// <summary>
        /// Gets or sets the center of the ripple effect.
        /// </summary>
        [PropertyBinding((int)PTShadowMapEffectProperties.Resolution, "(0.0, 0.0)", "(1024.0, 1024.0)", "(0.0, 0.0)")]
        public Vector2 Resolution
        {
            get { return buffer.Resolution; }
            set { buffer.Resolution = value; }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTShadowMapEffect()
        {
            buffer = new ShadowMapEffectConstantBuffer();
            guid = Guid.NewGuid();
        }

        /// <inheritdoc/>
        public override void Initialize(EffectContext effectContext, TransformGraph transformGraph)
        {
            string shaderPath = "Effects/PostProcess/PTShadowMapEffect.fx";
            PTShader shader = new PTShader();
            shader.LoadContent(Game.Instance.ContentManager);
            Game.Instance.ContentManager.LoadPixelShaderInto(shaderPath, ref shader, -1, new List<string>() { "Effects/PostProcess" });

            if (shader == null)
                throw new ArgumentNullException("Error loading shadow map shader.");

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
                factory.RegisterEffect<PTShadowMapEffect>(this.Guid);

            Effect = new Effect(context, this.Guid);

            target = new Bitmap1(context, new Size2((int)ShadowMapSize.X, 1),
                    new BitmapProperties1(new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    96,
                    96,
                    BitmapOptions.Target));
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
        private struct ShadowMapEffectConstantBuffer
        {
            public Vector2 Resolution;
            public float UpScale;
            public float Padding;
        }
    }
}
