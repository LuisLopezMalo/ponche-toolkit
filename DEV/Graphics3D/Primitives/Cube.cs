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
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Core.Management.Content;

namespace PoncheToolkit.Graphics3D.Primitives
{
    /// <summary>
    /// Draw a simple triangle.
    /// </summary>
    public class Cube : Primitive
    {
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
#if DX11
        public Cube(Game11 game)
#elif DX12
        public Cube(Game12 game)
#endif
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
        public override void LoadContent(IContentManager contentManager)
        {
            var x = 0.5f;
            var y = 0.5f;
            var z = 0.5f;

            PTMesh mesh = new PTMesh(this, "main");

            //// front
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, -z), new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector2(0, 1)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, -z), new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector2(0, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, -z), new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector2(1, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, -z), new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector2(1, 1)));
            //// back
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, +z), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector2(1, 1)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, +z), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector2(0, 1)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, +z), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector2(0, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, +z), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector2(1, 0)));
            //// top
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, -z), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(0, 1)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, +z), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(0, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, +z), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(1, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, -z), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(1, 1)));
            //// bottom
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, -z), new Vector3(-1, 0, 0), new Vector3(0, -1, 0), new Vector2(1, 1)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, -z), new Vector3(-1, 0, 0), new Vector3(0, -1, 0), new Vector2(0, 1)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, +z), new Vector3(-1, 0, 0), new Vector3(0, -1, 0), new Vector2(0, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, +z), new Vector3(-1, 0, 0), new Vector3(0, -1, 0), new Vector2(1, 0)));
            //// left
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, +z), new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector2(0, 1)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, +z), new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector2(0, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, -z), new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector2(1, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, -z), new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector2(1, 1)));
            //// right
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, -z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector2(0, 1)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, -z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector2(0, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, +z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector2(1, 0)));
            //mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, +z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector2(1, 1)));


            // front
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, -z), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector2(0, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, -z), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector2(0, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, -z), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector2(UVRepeatFactor, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, -z), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector2(UVRepeatFactor, 1)));
            // back
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, +z), new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector2(UVRepeatFactor, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, +z), new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector2(0, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, +z), new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector2(0, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, +z), new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector2(UVRepeatFactor, 0)));
            // top
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, -z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector2(0, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, +z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector2(0, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, +z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector2(UVRepeatFactor, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, -z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector2(UVRepeatFactor, UVRepeatFactor)));
            // bottom
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, -z), new Vector3(0, -1, 0), new Vector3(-1, 0, 0), new Vector2(UVRepeatFactor, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, -z), new Vector3(0, -1, 0), new Vector3(-1, 0, 0), new Vector2(0, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, +z), new Vector3(0, -1, 0), new Vector3(-1, 0, 0), new Vector2(0, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, +z), new Vector3(0, -1, 0), new Vector3(-1, 0, 0), new Vector2(UVRepeatFactor, 0)));
            // left
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, +z), new Vector3(-1, 0, 0), new Vector3(0, 0, -1), new Vector2(0, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, +z), new Vector3(-1, 0, 0), new Vector3(0, 0, -1), new Vector2(0, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, -z), new Vector3(-1, 0, 0), new Vector3(0, 0, -1), new Vector2(UVRepeatFactor, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, -z), new Vector3(-1, 0, 0), new Vector3(0, 0, -1), new Vector2(UVRepeatFactor, UVRepeatFactor)));
            // right
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, -z), new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector2(0, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, -z), new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector2(0, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, +z), new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector2(UVRepeatFactor, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, +z), new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector2(UVRepeatFactor, UVRepeatFactor)));


            int[] indices = new[] {0,1,2,0,2,3,
                4,5,6,4,6,7,
                8,9,10,8,10,11,
                12,13,14,12,14,15,
                16,17,18,16,18,19,
                20,21,22,20,22,23
                };

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
