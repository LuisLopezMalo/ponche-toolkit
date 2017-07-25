using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Struct that holds the lighting information to be sent to the shader.
    /// </summary>
    public struct LightningStruct
    {
        /// <summary>
        /// Color for diffuse lightning.
        /// </summary>
        public Vector4 DiffuseColor;
        /// <summary>
        /// The lightning direction.
        /// </summary>
        public Vector3 LightDirection;
        /// <summary>
        /// Padding so the struct is a multiple of 16. (Vector4 = 16, Vector3 = 12)
        /// </summary>
        float Padding;
    }
}
