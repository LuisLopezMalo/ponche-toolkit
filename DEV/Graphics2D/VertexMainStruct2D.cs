using SharpDX;
using System.Runtime.InteropServices;

namespace PoncheToolkit.Graphics2D
{
    /// <summary>
    /// Structure that holds values for position(Vector3), Color(Vector4) and TexCoord(Vector2).
    /// Note: Changing the order of how the properties are declared, change the final result.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexMainStruct2D
    {
        /// <summary>
        /// The Position of the Vertex.
        /// </summary>
        public Vector3 Position;

        ///// <summary>
        ///// The Color of the Vertex.
        ///// </summary>
        //public Vector4 Color;

        /// <summary>
        /// The Texture of the Vertex.
        /// </summary>
        public Vector2 TexCoord;

        ///// <summary>
        ///// Constructor.
        ///// </summary>
        ///// <param name="position">The position of the vertex.</param>
        ///// <param name="color">The position of the vertex.</param>
        ///// <param name="texCoord">The texture coordinates of the vertex in Vector3 format, the z component is discarded.</param>
        //public VertexMainStruct2D(Vector3 position, Vector4 color, Vector2 texCoord)
        //{
        //    this.Position = position;
        //    this.Color = color;
        //    this.TexCoord = texCoord;
        //}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="texCoord">The texture coordinates of the vertex in Vector3 format, the z component is discarded.</param>
        public VertexMainStruct2D(Vector3 position, Vector2 texCoord)
        {
            this.Position = position;
            this.TexCoord = texCoord;
        }
    }
}
