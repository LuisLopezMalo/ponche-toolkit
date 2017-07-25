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
    /// Struct that represent a position (Vector3) and a TexCoord (Vector2) that will
    /// be passed to the shader.
    /// </summary>
    public struct VertexPositionTextureStruct
    {
        /// <summary>
        /// The Position of the Vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The Texture coordinates of the Vertex.
        /// </summary>
        public Vector2 TexCoord;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="texCoord">The texture coordinates of the vertex.</param>
        public VertexPositionTextureStruct(Vector3 position, Vector2 texCoord)
        {
            this.Position = position;
            this.TexCoord = texCoord;
        }

        #region Operator
        static public implicit operator VertexPositionTextureStruct(VertexPositionColorStruct value)
        {
            return new VertexPositionTextureStruct(value.Position, Vector2.Zero);
        }
        #endregion
    }
}
