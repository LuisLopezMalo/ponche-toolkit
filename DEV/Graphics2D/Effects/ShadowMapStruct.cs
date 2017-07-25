using SharpDX;
using System.Runtime.InteropServices;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Structure that holds values for position(Vector3), Color(Vector4) and TexCoord(Vector2).
    /// Note: Changing the order of how the properties are declared, change the final result.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct ShadowMapStruct
    {
        /// <summary>
        /// The resolution of the shadow map texture.
        /// </summary>
        public Vector2 Resolution;

        /// <summary>
        /// The scale to use for the given resolution.
        /// </summary>
        public float Scale;

        /// <summary>
        /// Padding
        /// </summary>
        public float Padding;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resolution">The position of the vertex.</param>
        /// <param name="scale">The color for the vertex.</param>
        public ShadowMapStruct(Vector2 resolution, float scale)
        {
            this.Resolution = resolution;
            this.Scale = scale;
            this.Padding = 0;
        }
    }
}
