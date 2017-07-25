using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    public struct VertexPositionColorStruct
    {
        /// <summary>
        /// The Position of the Vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The Color of the Vertex.
        /// </summary>
        public Vector4 Color;

        public VertexPositionColorStruct(Vector3 position, Vector4 color)
        {
            this.Position = position;
            this.Color = color;
        }
    }
}
