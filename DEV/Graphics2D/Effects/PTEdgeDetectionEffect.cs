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
using PoncheToolkit.Graphics3D;

namespace PoncheToolkit.Graphics2D.Effects
{
    /// <summary>
    /// Class that implements a simple 2D ripple effect.
    /// </summary>
    [CustomEffect("A custom effect to render edges with a determined thickness", "CustomEffects", "PoncheToolkit", DisplayName = "Edge detection effect")]
    [CustomEffectInput("Source")]
    public class PTEdgeDetectionEffect : PTPostProcessEffect
    {
        private EdgeDetectionEffectConstantBuffer buffer;

        #region Properties
        /// <summary>
        /// Gets or sets the Frequency.
        /// </summary>
        [PropertyBinding((int)PTEdgeDetectionEffectProperties.ScreenSize, "(0.0, 0.0)", "(5000.0, 5000.0)", "(1280, 720)")]
        public Vector2 ScreenSize
        {
            get { return buffer.ScreenSize; }
            set
            {
                buffer.ScreenSize = value;
                //UpdateBuffer(ChangeType.None);
            }
        }

        /// <summary>
        /// Gets or sets the phase.
        /// </summary>
        [PropertyBinding((int)PTEdgeDetectionEffectProperties.Thickness, "0", "100.0", "1.5")]
        public float Thickness
        {
            get { return buffer.Thickness; }
            set { buffer.Thickness = MathUtil.Clamp(value, 0.0f, 100.0f); }
        }

        /// <summary>
        /// Gets or sets the amplitude.
        /// </summary>
        [PropertyBinding((int)PTEdgeDetectionEffectProperties.Threshold, "0.0001", "100.0", "0.2")]
        public float Threshold
        {
            get { return buffer.Threshold; }
            set { buffer.Threshold = MathUtil.Clamp(value, 0.0001f, 100.0f); }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTEdgeDetectionEffect()
            : base("Effects/PostProcess/PTEdgeDetectionEffect.fx")
        {
            buffer = new EdgeDetectionEffectConstantBuffer();
        }

        /// <inheritdoc/>
        public override void Initialize(EffectContext effectContext, TransformGraph transformGraph)
        {
            //string shaderPath = "Effects/PostProcess/PTEdgeDetectionEffect.fx";
            //PTShader shader = new PTShader();
            //shader.LoadContent(Game.Instance.ContentManager);
            //Game.Instance.ContentManager.LoadPixelShaderInto(shaderPath, ref shader, -1, new List<string>() { "Effects/PostProcess" });

            //if (shader == null)
            //    throw new ArgumentNullException("Error loading edge detection shader.");

            //byte[] bytes = SharpDX.IO.NativeFile.ReadAllBytes(
            //    System.IO.Path.Combine(Game.Instance.ContentDirectoryFullPath,
            //    System.IO.Path.GetDirectoryName(shaderPath),
            //    System.IO.Path.GetFileNameWithoutExtension(shaderPath) + ContentManager.PIXEL_SHADER_COMPILED_NAME_EXTENSION));

            //effectContext.LoadPixelShader(this.Guid, bytes);
            //transformGraph.SetSingleTransformNode(this);

            //Utilities.Dispose(ref shader);
            //bytes = null;

            base.Initialize(effectContext, transformGraph);

            this.Threshold = 0.2f;
            this.Thickness = 1.5f;
        }

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
                factory.RegisterEffect<PTEdgeDetectionEffect>(this.Guid);

            Effect = new Effect(context, this.Guid);
            IsRegistered = true;
        }

        /// <summary>
        /// Internal structure used for the constant buffer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct EdgeDetectionEffectConstantBuffer
        {
            public Vector2 ScreenSize;
            public float Thickness;
            public float Threshold;
        }
    }
}
