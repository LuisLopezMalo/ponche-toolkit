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

namespace PT.Graphics3D.Primitives
{
    using Core.Management.Content;
    using Graphics2D;

    /// <summary>
    /// Draw a simple triangle.
    /// </summary>
    public class Square : Primitive
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public Square(Game11 game)
            : base(game)
        {
        }

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            base.Initialize();
        }

        /// <inheritdoc/>
        public override void LoadContent(ContentManager contentManager)
        {
            PTMesh mesh = new PTMesh(this, "main");

            int[] indices = new[] { 0, 1, 2, 0, 2, 3 };

            var x = 0.5f;
            var y = 0.5f;
            var z = 0f;

            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, -y, -z), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector2(0, UVRepeatFactor)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(-x, +y, -z), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector2(0, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, +y, -z), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector2(UVRepeatFactor, 0)));
            mesh.AddVertex(new VertexMainStruct(new Vector3(+x, -y, -z), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector2(UVRepeatFactor, UVRepeatFactor)));

            mesh.SetIndices(indices, 3);
            AddMesh(mesh);
            
            base.LoadContent(contentManager);
        }

        /// <inheritdoc/>
        public override void UpdateLogic()
        {
            
        }

        /// <inheritdoc/>
        public override void UpdateBoundingBox()
        {
            Vector3 minimum = Vector3.TransformCoordinate(new Vector3(Position.X - Size.X / 2, Position.Y - Size.Y / 2, Position.Z + Size.Z / 2),
                Matrix.RotationYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z));

            Vector3 maximum = Vector3.TransformCoordinate(new Vector3(Position.X + Size.X / 2, Position.Y + Size.Y / 2, Position.Z - Size.Z / 2),
                Matrix.RotationYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z));

            BoundingBox = new BoundingBox(minimum, maximum);
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
