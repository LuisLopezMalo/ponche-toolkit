using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Structure that holds values for position(Vector3), Color(Vector4) and TexCoord(Vector2).
    /// </summary>
    public struct VertexMainStruct
    {
        /// <summary>
        /// The Position of the Vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The Color of the Vertex.
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// The Texture of the Vertex.
        /// </summary>
        public Vector2 TexCoord;

        /// <summary>
        /// The Normal for the lighting of the Vertex.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="color">The color for the vertex.</param>
        /// <param name="texCoord">The texture coordinates of the vertex.</param>
        /// <param name="normal">The normal to calculate lighting.</param>
        public VertexMainStruct(Vector3 position, Vector4 color, Vector2 texCoord, Vector3 normal)
        {
            this.Position = position;
            this.Color = color;
            this.TexCoord = texCoord;
            this.Normal = normal;
        }

        #region Operator
        /// <summary>
        /// Convert a VertexPositionColorStruct to VertexPositionColorTextureStruct with a TexCoord value of zero.
        /// </summary>
        /// <param name="value"></param>
        static public implicit operator VertexMainStruct(VertexPositionColorStruct value)
        {
            return new VertexMainStruct(value.Position, value.Color, Vector2.Zero, -Vector3.UnitZ);
        }

        /// <summary>
        /// Convert a VertexPositionTextureStruct to VertexPositionColorTextureStruct with a Color value of one.
        /// </summary>
        /// <param name="value"></param>
        static public implicit operator VertexMainStruct(VertexPositionTextureStruct value)
        {
            return new VertexMainStruct(value.Position, Vector4.One, value.TexCoord, -Vector3.UnitZ);
        }
        #endregion
    }
}
