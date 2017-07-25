using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using SharpDX;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Graphics3D.Cameras
{
    /// <summary>
    /// Main class that represent a world-view-projection camera.
    /// </summary>
    public interface ICamera
    {
        #region Properties
        /// <summary>
        /// The view matrix.
        /// </summary>
        Matrix View { get; }

        /// <summary>
        /// The projection matrix.
        /// </summary>
        Matrix Projection { get; }

        /// <summary>
        /// The multiplied viewProjection matrix.
        /// </summary>
        Matrix ViewProjection { get; }

        /// <summary>
        /// The field of view of the camera. Generally its PI / 4.
        /// </summary>
        float FOV { get; set; }

        /// <summary>
        /// The aspect ratio.
        /// </summary>
        float AspectRatio { get; set; }

        /// <summary>
        /// The near plane to clip the projection.
        /// </summary>
        float NearPlane { get; set; }

        /// <summary>
        /// The far plane to clip the projection.
        /// </summary>
        float FarPlane { get; set; }

        /// <summary>
        /// Position of the camera.
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// The point where the camera is looking at.
        /// <para>
        /// Default: Vector3.Zero
        /// </para>
        /// </summary>
        Vector3 LookAt { get; set; }

        /// <summary>
        /// Rotation of the camera.
        /// </summary>
        Vector3 Rotation { get; set; }

        /// <summary>
        /// The calculated right vector.
        /// </summary>
        Vector3 Right { get; set; }

        /// <summary>
        /// The calculated up vector.
        /// </summary>
        Vector3 Up { get; set; }

        /// <summary>
        /// The direction vector where the camera is pointing.
        /// Default: Vector3.ForwardLH
        /// </summary>
        Vector3 Direction { get; set; }

        /// <summary>
        /// The frustrum object to calculate the view frustrum.
        /// This frustrum must be updated typically at the end of the <see cref="UpdateLogic"/> method
        /// when the View Projection matrices has already been calculated.
        /// </summary>
        BoundingFrustum Frustrum { get; set; }

        /// <summary>
        /// The type of camera.
        /// <para>
        /// Default: <see cref="CameraType.Main"/>.
        /// </para>
        /// </summary>
        CameraType Type { get; set; }
        #endregion
    }
}
