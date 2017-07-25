using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// Struct that holds the World-View-Projection matrices information to be sent to the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct MatricesStructPerObject
    {
        /// <summary>
        /// The wolrd matrix.
        /// </summary>
        public Matrix WorldMatrix;

        /// <summary>
        /// The wolrd matrix.
        /// </summary>
        public Matrix WorldViewProjection;

        /// <summary>
        /// The world inverse transposed matrix of the Vertex.
        /// </summary>
        public Matrix InverseTransposeWorld;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="world">The world for the instance.</param>
        /// <param name="worldViewProjection">The worldViewProjection for the instance.</param>
        /// <param name="inverseTransposeWorld">The worldInverseTranspose matrix for the instance.</param>
        public MatricesStructPerObject(Matrix world, Matrix worldViewProjection, Matrix inverseTransposeWorld)
        {
            this.WorldMatrix = world;
            this.WorldViewProjection = worldViewProjection;
            this.InverseTransposeWorld = inverseTransposeWorld;
        }
    }
}
