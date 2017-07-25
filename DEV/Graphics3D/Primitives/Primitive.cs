using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buffer = SharpDX.Direct3D11.Buffer;
using PoncheToolkit.Core.Components;
using SharpDX;
using PoncheToolkit.Core;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.Collections;
using PoncheToolkit.Util;
using System.ComponentModel;

namespace PoncheToolkit.Graphics3D.Primitives
{
    using Core.Management.Content;
    using Graphics2D;

    /// <summary>
    /// Abstract class that represent a generic primitive.
    /// </summary>
    public abstract class Primitive : PTModel
    {
        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
#if DX11
            public Primitive(Game11 game)
#elif DX12
        public Primitive(Game12 game)
#endif
            : base(game)
        {
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void LoadContent(IContentManager contentManager)
        {
            HasCalculatedTangentBinormals = true;
            base.LoadContent(contentManager);
        }

        /// <summary>
        /// Update the Vertex and Pixel stages.
        /// </summary>
        public override void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            base.Render(spriteBatch, context);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (IsStateUpdated)
                return IsStateUpdated;

            object obj = null;
            DirtyProperties.TryGetValue(nameof(UVRepeatFactor), out obj);
            if (obj != null)
            {
                foreach (PTMesh mesh in Meshes)
                {
                    for (int i = 0; i < mesh.Vertices.Count; i++)
                    {
                        var vertex = mesh.Vertices[i];
                        vertex.TexCoord = new Vector2(vertex.TexCoord.X * UVRepeatFactor, vertex.TexCoord.Y * UVRepeatFactor);
                        mesh.Vertices[i] = vertex;
                    }
                }
            }

            if (DirtyProperties.ContainsKey(nameof(Rotation)))
            {

                //TODO: Buscando la manera de recalcular las normales cuando el modelo cambie su rotacion    

                //CalculateRenderingVectors();
            }

            IsStateUpdated = true;
            OnStateUpdated();

            return IsStateUpdated;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }
}
