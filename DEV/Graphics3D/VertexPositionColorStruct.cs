using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Struct with only a position and color.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public VertexPositionColorStruct(Vector3 position, Vector4 color)
        {
            this.Position = position;
            this.Color = color;
        }
    }
}
