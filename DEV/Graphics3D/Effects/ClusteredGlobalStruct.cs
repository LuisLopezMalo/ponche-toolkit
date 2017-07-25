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
    /// Struct that holds the basic lighting information to be sent to the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct ClusteredGlobalStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public Vector2 BufferSize;
        /// <summary>
        /// 
        /// </summary>
        public Vector2 InverseBufferSize;
        /// <summary>
        /// The near plane from the camera.
        /// </summary>
        public float Near;
        /// <summary>
        /// The near plane using the logarithmic function to compute cluster index.  -- 1 / log(sD + 1).
        /// </summary>
        public float NearLog;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ClusteredGlobalStruct(Vector2 bufferSize, Vector2 inverseBufferSize, float near, float nearLog)
        {
            this.BufferSize = bufferSize;
            this.InverseBufferSize = inverseBufferSize;
            this.Near = near;
            this.NearLog = nearLog;
        }

        public static int SizeOf()
        {
            return Marshal.SizeOf<ClusteredGlobalStruct>();
        }
    }
}
