using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics2D
{
    /// <summary>
    /// Struct that holds the World-View-Projection matrices information to be sent to the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MatricesStruct2D
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
        /// The ortho matrix
        /// </summary>
        public Matrix Ortho;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="view"></param>
        /// <param name="ortho"></param>
        public MatricesStruct2D(Matrix world, Matrix view, Matrix ortho)
        {
            this.World = world;
            this.View = view;
            this.Ortho = ortho;
        }
    }
}
