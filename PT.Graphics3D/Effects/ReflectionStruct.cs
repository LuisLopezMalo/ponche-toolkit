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
    internal struct ReflectionStruct
    {
        /// <summary>
        /// The ReflectionMatrixProjectionWorld matrix.
        /// </summary>
        public Matrix ReflectionMatrixProjectionWorld;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reflectionMatrixProjectionWorld">The reflectionMatrixProjectionWorld matrix.</param>
        public ReflectionStruct(Matrix reflectionMatrixProjectionWorld)
        {
            this.ReflectionMatrixProjectionWorld = reflectionMatrixProjectionWorld;
        }
    }
}
