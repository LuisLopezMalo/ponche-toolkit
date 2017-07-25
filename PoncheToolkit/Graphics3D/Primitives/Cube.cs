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

namespace PoncheToolkit.Graphics3D.Primitives
{
    /// <summary>
    /// Draw a simple triangle.
    /// </summary>
    public class Cube : Primitive
    {
        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event OnFinishLoadContentHandler OnFinishLoadContent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public Cube(Game11 game)
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
        public override void LoadContent()
        {
            
            // Cube
            AddVertex(new VertexPositionColorStruct(new Vector3(-1.0f, -1.0f, -1.0f), new Vector4(0f, 0.3f, 0f, 1.0f))); // FRONT
            AddVertex(new VertexPositionColorStruct(new Vector3(1.0f, 0.0f, 0.0f), new Vector4(0f, 0.3f, 0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(-1.0f, 1.0f, -1.0f), new Vector4(0f, 0.3f, 0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(1.0f, 0.0f, 0.0f), new Vector4(0f, 0.3f, 0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(1.0f, 1.0f, -1.0f), new Vector4(0f, 0.3f, 0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(1.0f, 0.0f, 0.0f), new Vector4(0f, 0.3f, 0f, 1.0f)));

            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 1.0f, 0.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f))); // BACK
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 1.0f, 0.0f), new Vector4(-1.0f, 1.0f, 1.0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 1.0f, 0.0f), new Vector4(-1.0f, -1.0f, 1.0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 1.0f, 0.0f), new Vector4(1.0f, -1.0f, 1.0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 1.0f, 0.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 1.0f, 0.0f), new Vector4(-1.0f, 1.0f, -1.0f, 1.0f)));

            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 0.0f, 1.0f), new Vector4(-1.0f, 1.0f, 1.0f, 1.0f))); // TOP
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 0.0f, 1.0f), new Vector4(-1.0f, 1.0f, -1.0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, -1.0f, 1.0f)));
            AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 0.0f, 1.0f), new Vector4(-1.0f, -1.0f, -1.0f, 1.0f)));

            //new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Bottom
            //                          new Vector4(1.0f, -1.0f, 1.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4(-1.0f, -1.0f, 1.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4(1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
            //                          new Vector4(1.0f, -1.0f, 1.0f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

            //                          new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Left
            //                          new Vector4(-1.0f, -1.0f, 1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4(-1.0f, 1.0f, 1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4(-1.0f, 1.0f, 1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
            //                          new Vector4(-1.0f, 1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),

            //                          new Vector4(1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Right
            //                          new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            //                          new Vector4(1.0f, -1.0f, 1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            //                          new Vector4(1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            //                          new Vector4(1.0f, 1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            //                          new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),

            base.LoadContent();

            OnFinishLoadContent?.Invoke();
        }

        /// <inheritdoc/>
        public override void Render()
        {
            base.Render();
            Game.Renderer.DeviceContext.Draw(Vertices.Count, 0);
        }

        /// <inheritdoc/>
        public override void Update()
        {
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
