using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX.WIC;
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Core.Management.Content;

namespace PT.Graphics3D.Primitives
{
    /// <summary>
    /// Draw a simple triangle.
    /// </summary>
    public class Triangle : Primitive
    {
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public Triangle(Game11 game)
            : base(game)
        {
        }

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void LoadContent(ContentManager contentManager)
        {
            PTMesh mesh = new PTMesh(this, "main");

            // Add the indices.
            int[] indices = new[] { 0, 1, 2 };

            // Lighting
            mesh.AddVertex(new VertexMainStruct(new Vector3(0.0f, 0.5f, 0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector3(0, 0, -1), new Vector2(0f, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(0.5f, -0.5f, 0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector3(0, 0, -1), new Vector2(UVRepeatFactor, 0f)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(-0.5f, -0.5f, 0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector3(0, 0, -1), new Vector2(0f, 0f)));

            mesh.SetIndices(indices, 3);
            AddMesh(mesh);

            base.LoadContent(contentManager);

            OnFinishLoadContent?.Invoke();
        }

        /// <inheritdoc/>
        public override void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            base.Render(spriteBatch, context);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            UnloadContent();
            base.Dispose();
        }
        #endregion
    }
}
