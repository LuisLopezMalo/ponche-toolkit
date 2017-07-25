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
    public struct MatricesStruct
    {
        /// <summary>
        /// The world matrix.
        /// </summary>
        public Matrix World;
        /// <summary>
        /// The view-projection matrix
        /// </summary>
        public Matrix ViewProjection;
        /// <summary>
        /// The world-view-projection matrix
        /// </summary>
        public Matrix WorldViewProjection;
        ///// <summary>
        ///// The view matrix.
        ///// </summary>
        //public Matrix WorldInverseTranspose;
        ///// <summary>
        ///// The projection matrix
        ///// </summary>
        //public Matrix Projection;
        /// <summary>
        /// The position of the camera. (The eye).
        /// </summary>
        public Vector3 CameraPosition;
        /// <summary>
        /// Padding.
        /// </summary>
        public float Padding;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="world">The world matrix.</param>
        /// <param name="viewProjection">The cpu pre-multiplied view projection matrix.</param>
        /// <param name="worldViewProjection">The cpu pre-multiplied world view projection matrix.</param>
        /// <param name="cameraPosition">The current camera position</param>
        //public MatricesStruct(Matrix world, Matrix worldView, Matrix viewProjection, Matrix worldViewProjection, Matrix worldInverseTranspose, Vector3 cameraPosition)
        public MatricesStruct(Matrix world, Matrix viewProjection, Matrix worldViewProjection, Vector3 cameraPosition)
        {
            this.World = world;
            this.ViewProjection = viewProjection;
            this.WorldViewProjection = worldViewProjection;
            this.CameraPosition = cameraPosition;
            this.Padding = 0;
        }
    }
}
