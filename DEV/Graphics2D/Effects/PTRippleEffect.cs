using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SharpDX;
using System.Runtime.InteropServices;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using SharpDX.Mathematics.Interop;

namespace PoncheToolkit.Graphics2D.Effects
{
    /// <summary>
    /// Class that implements a simple 2D ripple effect.
    /// </summary>
    //[CustomEffect("A custom effect to render ripple", "CustomEffects", "PoncheToolkit", DisplayName = "Ripple effect")]
    //[CustomEffectInput("Source")]
    public class PTRippleEffect : PTPostProcessEffect
    {
        private RippleEffectConstantBuffer buffer;

        #region Properties
        /// <summary>
        /// Gets or sets the Frequency.
        /// </summary>
        [PropertyBinding((int)PTRippleEffectProperties.Frequency, "0.0", "1000.0", "0.0")]
        public float Frequency
        {
            get { return buffer.Frequency; }
            set
            {
                buffer.Frequency = MathUtil.Clamp(value, 0.0f, 1000.0f);
                UpdateBuffer(ChangeType.None);
            }
        }

        /// <summary>
        /// Gets or sets the phase.
        /// </summary>
        [PropertyBinding((int)PTRippleEffectProperties.Phase, "-100.0", "100.0", "0.0")]
        public float Phase
        {
            get { return buffer.Phase; }
            set { buffer.Phase = MathUtil.Clamp(value, -100.0f, 100.0f); }
        }

        /// <summary>
        /// Gets or sets the amplitude.
        /// </summary>
        [PropertyBinding((int)PTRippleEffectProperties.Amplitude, "0.0001", "1000.0", "0.0")]
        public float Amplitude
        {
            get { return buffer.Amplitude; }
            set { buffer.Amplitude = MathUtil.Clamp(value, 0.0001f, 1000.0f); }
        }

        /// <summary>
        /// Gets or sets the spread.
        /// </summary>
        [PropertyBinding((int)PTRippleEffectProperties.Spread, "0.0001", "1000.0", "0.0")]
        public float Spread
        {
            get { return buffer.Spread; }
            set { buffer.Spread = MathUtil.Clamp(value, 0.0001f, 1000.0f); }
        }

        /// <summary>
        /// Gets or sets the center of the ripple effect.
        /// </summary>
        [PropertyBinding((int)PTRippleEffectProperties.Center, "(-2000.0, -2000.0)", "(2000.0, 2000.0)", "(0.0, 0.0)")]
        public Vector2 Center
        {
            get { return buffer.Center; }
            set { buffer.Center = value; }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTRippleEffect()
            : base("Effects/PostProcess/PTRippleEffect.fx")
        {
            buffer = new RippleEffectConstantBuffer();
        }

        ///// <inheritdoc/>
        //public override void Initialize(EffectContext effectContext, TransformGraph transformGraph)
        //{
        //    string shaderPath = "Effects/PostProcess/PTRippleEffect.fx";
        //    PTShader shader = new PTShader();
        //    shader.LoadContent(Game11.Instance.ContentManager);
        //    Game.Instance.ContentManager.LoadPixelShaderInto(shaderPath, ref shader, -1, new List<string>() { "Effects/PostProcess" });

        //    if (shader == null)
        //        throw new ArgumentNullException("Error loading ripple map shader.");

        //    byte[] bytes = SharpDX.IO.NativeFile.ReadAllBytes(
        //        System.IO.Path.Combine(Game.Instance.ContentDirectoryFullPath,
        //        System.IO.Path.GetDirectoryName(shaderPath),
        //        System.IO.Path.GetFileNameWithoutExtension(shaderPath) + ContentManager.PIXEL_SHADER_COMPILED_NAME_EXTENSION));

        //    effectContext.LoadPixelShader(this.Guid, bytes);
        //    transformGraph.SetSingleTransformNode(this);

        //    Utilities.Dispose(ref shader);
        //    bytes = null;
        //}

        /// <inheritdoc/>
        public override void UpdateBuffer(ChangeType changeType)
        {
            if (DrawInfo != null)
                DrawInfo.SetPixelConstantBuffer(ref buffer);
        }

        /// <inheritdoc/>
        public override void Register(Factory1 factory, DeviceContext context)
        {
            if (!factory.RegisteredEffects.Contains(this.Guid))
                factory.RegisterEffect<PTRippleEffect>(this.Guid);

            Effect = new Effect(context, this.Guid);

            IsRegistered = true;
        }

        /// <summary>
        /// Internal structure used for the constant buffer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RippleEffectConstantBuffer
        {
            public float Frequency;
            public float Phase;
            public float Amplitude;
            public float Spread;
            public Vector2 Center;
        }
    }
}
