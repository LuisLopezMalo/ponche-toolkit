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
    /// Struct that holds the View-Projection matrix information to be sent to the shader.
    /// This struct will be used when rendering multiple instances of a model in one draw-call.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct PerInstanceStruct
    {
        /// <summary>
        /// The ViewProjectionMatrix matrix per instance.
        /// </summary>
        public Matrix ViewProjectionMatrix;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="viewProjection">The viewProjection matrix.</param>
        public PerInstanceStruct(Matrix viewProjection)
        {
            this.ViewProjectionMatrix = viewProjection;
        }
    }
}
