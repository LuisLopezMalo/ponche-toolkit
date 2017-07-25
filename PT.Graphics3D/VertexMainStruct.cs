using SharpDX;
using System.Runtime.InteropServices;

namespace PT.Graphics3D
{
    /// <summary>
    /// Structure that holds values for position(Vector3), Color(Vector4) and TexCoord(Vector2).
    /// Note: Changing the order of how the properties are declared, change the final result.
    /// </summary>
    //[StructLayout(LayoutKind.Sequential, Pack = 16)]
    [StructLayout(LayoutKind.Sequential)]
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
        /// The Tangent of the Vertex.
        /// </summary>
        public Vector3 Tangent;

        /// <summary>
        /// The Binormal of the Vertex.
        /// </summary>
        public Vector3 BiNormal;

        ///// <summary>
        ///// The wolrd matrix.
        ///// </summary>
        //public Matrix WorldMatrix;

        ///// <summary>
        ///// The world inverse transposed matrix of the Vertex.
        ///// </summary>
        //public Matrix WorldInverseTransposeMatrix;

        ///// <summary>
        ///// Constructor.
        ///// </summary>
        ///// <param name="position">The position of the vertex.</param>
        ///// <param name="color">The color for the vertex.</param>
        ///// <param name="normal">The normal to calculate lighting.</param>
        ///// <param name="tangent">The tangent for the vertex.</param>
        ///// <param name="binormal">The binormal for the vertex.</param>
        ///// <param name="texCoord">The texture coordinates of the vertex.</param>
        //public VertexMainStruct(Vector3 position, Vector4 color, Vector3 normal, Vector3 tangent, Vector3 binormal, Vector2 texCoord,
        //    Matrix world, Matrix worldInverseTranspose)
        //{
        //    this.Position = position;
        //    this.Color = color;
        //    this.Normal = normal;
        //    this.Tangent = tangent;
        //    this.BiNormal = binormal;
        //    this.TexCoord = texCoord;
        //    this.WorldMatrix = world;
        //    this.WorldInverseTransposeMatrix = worldInverseTranspose;
        //}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="color">The color for the vertex.</param>
        /// <param name="normal">The normal to calculate lighting.</param>
        /// <param name="tangent">The tangent for the vertex.</param>
        /// <param name="binormal">The binormal for the vertex.</param>
        /// <param name="texCoord">The texture coordinates of the vertex.</param>
        public VertexMainStruct(Vector3 position, Vector4 color, Vector3 normal, Vector3 tangent, Vector3 binormal, Vector2 texCoord)
            //: this(position, color, normal, tangent, binormal, texCoord, Matrix.Identity, Matrix.Identity)
        {
            this.Position = position;
            this.Color = color;
            this.Normal = normal;
            this.Tangent = tangent;
            this.BiNormal = binormal;
            this.TexCoord = texCoord;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="color">The color for the vertex.</param>
        /// <param name="normal">The normal to calculate lighting.</param>
        /// <param name="tangent">The tangent for the vertex.</param>
        /// <param name="texCoord">The texture coordinates of the vertex.</param>
        public VertexMainStruct(Vector3 position, Vector4 color, Vector3 normal, Vector3 tangent, Vector2 texCoord)
            : this(position, color, normal, Vector3.UnitX, Vector3.One, texCoord)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="color">The color for the vertex.</param>
        /// <param name="texCoord">The texture coordinates of the vertex.</param>
        /// <param name="normal">The normal to calculate lighting.</param>
        public VertexMainStruct(Vector3 position, Vector4 color, Vector3 normal, Vector2 texCoord)
            : this(position, color, normal, Vector3.UnitX, texCoord)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The normal to calculate lighting.</param>
        /// <param name="texCoord">The texture coordinates of the vertex.</param>
        public VertexMainStruct(Vector3 position, Vector3 normal, Vector2 texCoord)
            : this(position, Vector4.One, normal, texCoord)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The normal to calculate lighting.</param>
        /// <param name="tangent">The tangent for the vertex.</param>
        /// <param name="texCoord">The texture coordinates of the vertex.</param>
        public VertexMainStruct(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texCoord)
            : this(position, Vector4.One, normal, tangent, texCoord)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="texCoord">The texture coordinates of the vertex in Vector3 format, the z component is discarded.</param>
        /// <param name="normal">The normal to calculate lighting.</param>
        public VertexMainStruct(Vector3 position, Vector3 normal, Vector3 texCoord)
            : this (position, Vector4.One, normal, new Vector2(texCoord.X, texCoord.Y))
        {
        }

        #region Operator
        /// <summary>
        /// Convert a VertexPositionColorStruct to VertexPositionColorTextureStruct with a TexCoord value of zero.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator VertexMainStruct(VertexPositionColorStruct value)
        {
            return new VertexMainStruct(value.Position, value.Color, -Vector3.UnitZ, new Vector2(1, 0));
        }

        /// <summary>
        /// Convert a VertexPositionTextureStruct to VertexPositionColorTextureStruct with a Color value of one.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator VertexMainStruct(VertexPositionTextureStruct value)
        {
            return new VertexMainStruct(value.Position, -Vector3.UnitZ, value.TexCoord);
        }
        #endregion
    }
}
