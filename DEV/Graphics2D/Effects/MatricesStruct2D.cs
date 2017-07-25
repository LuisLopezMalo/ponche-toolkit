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
    public struct MatricesStruct2D
    {
        /// <summary>
        /// The projection matrix.
        /// </summary>
        public Matrix Projection;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="projection">The projectionmatrix.</param>
        public MatricesStruct2D(Matrix projection)
        {
            this.Projection = projection;
        }
    }
}
