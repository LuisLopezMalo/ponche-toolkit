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
    /// Struct to send the clip properties to the gpu.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct ClipPlainStruct
    {
        /// <summary>
        /// The clip distance.
        /// </summary>
        public Vector4 Clip;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="clip"></param>
        public ClipPlainStruct(Vector4 clip)
        {
            this.Clip = clip;
        }
    }
}
