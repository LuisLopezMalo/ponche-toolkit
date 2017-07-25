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
using PoncheToolkit.Util.Exceptions;

namespace PoncheToolkit.Graphics2D.Effects
{
    /// <summary>
    /// Class that implements a simple 2D ripple effect.
    /// </summary>
    [CustomEffect("A post process effect for 3D rendering", "PostProcessEffects", "PoncheToolkit", DisplayName = "PostProcessEffect")]
    [CustomEffectInput("Source")]
    public abstract class PTPostProcessEffect : PTCustomEffect
    {
        private Guid guid;
        private bool isUsed;
        private int order;

        #region Properties
        /// <summary>
        /// Get or set the assigned guid for this effect
        /// </summary>
        public override Guid Guid
        {
            get { return guid; }
            set { SetPropertyAsDirty(ref guid, value); }
        }

        /// <summary>
        /// Get or set if the effect is being used as a post processing effect during 3D rendering.
        /// </summary>
        public bool IsUsed
        {
            get { return isUsed; }
            set { SetPropertyAsDirty(ref isUsed, value); }
        }

        /// <summary>
        /// The order that this post process effect will be rendered within the <see cref="IGraphicsRenderer.PostProcessEffects"/> sorted list.
        /// </summary>
        public int Order
        {
            get { return order; }
            set { SetPropertyAsDirty(ref order, value); }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// Creates a new <see cref="System.Guid"/>. This Guid will be used when the Register method is called, so it can be generated again if wanted before calling this method.
        /// </summary>
        /// <param name="pixelShaderPath">The path to the pixel shader relative to the <see cref="Game.ContentDirectoryName"/> directory.</param>
        public PTPostProcessEffect(string pixelShaderPath)
            : base(pixelShaderPath)
        {
            this.guid = Guid.NewGuid();
            this.order = -1;
        }

        /// <inheritdoc/>
        public override void Initialize(EffectContext effectContext, TransformGraph transformGraph)
        {
            PTShader shader = new PTShader();
            shader.LoadContent(Game.Instance.ContentManager);
            Game.Instance.ContentManager.LoadPixelShaderInto(PixelShaderPath, ref shader, -1, new List<string>() { "Effects/PostProcess" });

            if (shader == null)
                throw new ArgumentNullException("Error loading post process pixel shader - " + PixelShaderPath);

            byte[] bytes = SharpDX.IO.NativeFile.ReadAllBytes(
                System.IO.Path.Combine(Game.Instance.ContentDirectoryFullPath,
                    System.IO.Path.GetDirectoryName(PixelShaderPath),
                    System.IO.Path.GetFileNameWithoutExtension(PixelShaderPath) + ContentManager11.PIXEL_SHADER_COMPILED_NAME_EXTENSION));

            effectContext.LoadPixelShader(this.Guid, bytes);
            transformGraph.SetSingleTransformNode(this);

            Utilities.Dispose(ref shader);
            bytes = null;
        }

        /// <summary>
        /// Set the order that this post process effect will be rendered within the <see cref="IGraphicsRenderer.PostProcessEffects"/> sorted list.
        /// </summary>
        /// <param name="order"></param>
        public void SetProcessingOrder(int order)
        {
            this.order = order;
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                // If the effect is added to any mesh to be used, register it.
                if (DirtyProperties.ContainsKey(nameof(IsUsed)))
                {
                    if (order < 0)
                        throw new RenderingException("Before the effect is used, the order property must be set. {0}", Name);
                    Game.Instance.Renderer.AddBackBufferPostProcessEffect(order, this);
                }

                IsStateUpdated = true;
                OnStateUpdated();
            }

            return IsStateUpdated;
        }
    }
}
