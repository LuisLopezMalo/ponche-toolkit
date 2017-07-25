using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PT.Graphics3D.Effects
{
    /// <summary>
    /// Struct that holds the World-View-Projection matrices information to be sent to the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct MatricesStructPerFrame
    {
        /// <summary>
        /// The wolrd matrix.
        /// </summary>
        public Matrix WorldMatrix;

        /// <summary>
        /// The world inverse transposed matrix of the Vertex.
        /// </summary>
        public Matrix WorldInverseTransposeMatrix;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="world">The world for the instance.</param>
        /// <param name="worldInverseTranspose">The worldInverseTranspose matrix for the instance.</param>
        public MatricesStructPerFrame(Matrix world, Matrix worldInverseTranspose)
        {
            this.WorldMatrix = world;
            this.WorldInverseTransposeMatrix = worldInverseTranspose;
        }
    }
}
