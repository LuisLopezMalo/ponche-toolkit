using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D.Primitives
{
    /// <summary>
    /// Create a simple sphere given the initial values of Radius.
    /// </summary>
    public class Sphere : Primitive
    {
        #region Fields
        private float radius;
        private int horizontalSlices;
        private int verticalSlices;
        #endregion

        #region Properties
        /// <summary>
        /// The radius of the sphere.
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set { SetProperty(ref radius, value); }
        }

        /// <summary>
        /// The horizontal quality.
        /// </summary>
        public int HorizontalSlices
        {
            get { return horizontalSlices; }
            set { SetProperty(ref horizontalSlices, value); }
        }

        /// <summary>
        /// The vertical quality.
        /// </summary>
        public int VerticalSlices
        {
            get { return verticalSlices; }
            set { SetProperty(ref verticalSlices, value); }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
#if DX11
        public Sphere(Game11 game)
#elif DX12
        public Sphere(Game12 game)
#endif
            : this(game, 0.5f, 10, 10)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="radius">The radius of the sphere. Default: 0.5</param>
        /// <param name="horizontalSlices">The number of horizontal slices. Default: 10</param>
        /// <param name="verticalSlices">The number of vertical slices. Default: 10</param>
#if DX11
        public Sphere(Game11 game, float radius, int horizontalSlices, int verticalSlices)
#elif DX12
        public Sphere(Game12 game, float radius, int horizontalSlices, int verticalSlices)
#endif
            : base(game)
        {
            this.radius = radius;
            this.horizontalSlices = horizontalSlices;
            this.verticalSlices = verticalSlices;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <inheritdoc/>
        public override void LoadContent(IContentManager contentManager)
        {
            PTMesh mesh = new PTMesh(this, "main");
            mesh.AddVertex(new VertexMainStruct(new Vector3(0, radius, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(0, 0)));
            var phiStep = Math.PI / horizontalSlices;
            var thetaStep = 2.0f * Math.PI / verticalSlices;

            for (var i = 1; i <= horizontalSlices - 1; i++)
            {
                var phi = i * phiStep;
                for (var j = 0; j <= verticalSlices; j++)
                {
                    var theta = j * thetaStep;
                    var p = new Vector3(
                        (float)(radius * Math.Sin(phi) * Math.Cos(theta)),
                        (float)(radius * Math.Cos(phi)),
                        (float)(radius * Math.Sin(phi) * Math.Sin(theta)));

                    var t = new Vector3((float)(-radius * Math.Sin(phi) * Math.Sin(theta)), 0, (float)(radius * Math.Sin(phi) * Math.Cos(theta)));
                    t.Normalize();
                    var n = Vector3.Normalize(p);

                    var uv = new Vector2((float)(theta / (Math.PI * 2)), (float)(phi / Math.PI));
                    mesh.AddVertex(new VertexMainStruct(p, t, n, uv));
                }
            }

            //ret.Vertices.Add(new Vertex(0, -radius, 0, 0, -1, 0, 1, 0, 0, 0, 1));
            mesh.AddVertex(new VertexMainStruct(new Vector3(0, -radius, 0), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector2(0, 1)));

            List<int> indices = new List<int>();
            for (int i = 1; i <= verticalSlices; i++)
            {
                indices.Add(0);
                indices.Add(i + 1);
                indices.Add(i);
            }

            var baseIndex = 1;
            var ringVertexCount = verticalSlices + 1;
            for (var i = 0; i < horizontalSlices - 2; i++)
            {
                for (var j = 0; j < verticalSlices; j++)
                {
                    indices.Add(baseIndex + i * ringVertexCount + j);
                    indices.Add(baseIndex + i * ringVertexCount + j + 1);
                    indices.Add(baseIndex + (i + 1) * ringVertexCount + j);

                    indices.Add(baseIndex + (i + 1) * ringVertexCount + j);
                    indices.Add(baseIndex + i * ringVertexCount + j + 1);
                    indices.Add(baseIndex + (i + 1) * ringVertexCount + j + 1);
                }
            }
            var southPoleIndex = mesh.Vertices.Count - 1;
            baseIndex = southPoleIndex - ringVertexCount;
            for (var i = 0; i < verticalSlices; i++)
            {
                indices.Add(southPoleIndex);
                indices.Add(baseIndex + i);
                indices.Add(baseIndex + i + 1);
            }

            mesh.SetIndices(indices, 3);
            AddMesh(mesh);

            base.LoadContent(contentManager);
        }
    }
}
