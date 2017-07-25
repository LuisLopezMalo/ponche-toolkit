using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Struct that holds the World-View-Projection matrices information to be sent to the shader.
    /// </summary>
    public struct MatricesStruct
    {
        /// <summary>
        /// The world matrix.
        /// </summary>
        public Matrix World;
        /// <summary>
        /// The view matrix.
        /// </summary>
        public Matrix View;
        /// <summary>
        /// The projection matrix
        /// </summary>
        public Matrix Projection;
    }
}
