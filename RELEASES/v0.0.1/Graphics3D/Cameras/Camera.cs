using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using SharpDX;

namespace PoncheToolkit.Graphics3D.Cameras
{
    /// <summary>
    /// Main class that represent a world-view-projection camera.
    /// </summary>
    public abstract class Camera : GameComponent
    {
        #region Properties
        /// <summary>
        /// The view matrix.
        /// </summary>
        internal Matrix View;

        /// <summary>
        /// The projection matrix.
        /// </summary>
        internal Matrix Projection;

        /// <summary>
        /// The field of view of the camera. Generally its PI / 4.
        /// </summary>
        public float FOV;

        /// <summary>
        /// The aspect ratio.
        /// </summary>
        public float AspectRatio;

        /// <summary>
        /// The near plane to clip the projection.
        /// </summary>
        public float NearPlane;

        /// <summary>
        /// The far plane to clip the projection.
        /// </summary>
        public float FarPlane;

        /// <summary>
        /// Position of the camera.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The point where the camera is looking at.
        /// </summary>
        public Vector3 LookAt;

        /// <summary>
        /// Rotation of the camera.
        /// </summary>
        public Vector3 Rotation;
        #endregion


        /// <summary>
        /// Constructor. Set default values.
        /// </summary>
        /// <param name="game">The game instance.</param>
        public Camera(Game11 game)
            : base(game)
        {
        }

        #region Public Methods
        /// <inheritdoc/>
        public abstract override void Dispose();

        /// <inheritdoc/>
        public override void Initialize()
        {
            NearPlane = 0.1f;
            FarPlane = 1000f;
            AspectRatio = Game.Form.Width / Game.Form.Height;
            FOV = MathUtil.Pi / 4;

            // Create the matrices.
            Projection = Matrix.PerspectiveFovLH(FOV, AspectRatio, NearPlane, FarPlane);
            LookAt = Vector3.Zero;

            base.Initialize();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            // Setup the initial values for this update.
            Vector3 up = Vector3.Up;
            Vector3 position = Position;
            //Vector3 lookAt = Vector3.UnitZ;
            Vector3 lookAt = LookAt;
            Matrix rotationMatrix;
            float yaw, pitch, roll;

            // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
            pitch = MathUtil.DegreesToRadians(this.Rotation.X);
            yaw = MathUtil.DegreesToRadians(this.Rotation.Y);
            roll = MathUtil.DegreesToRadians(this.Rotation.Z);

            // Create the rotation matrix from the yaw, pitch, and roll values.
            rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
            up = Vector3.TransformCoordinate(up, rotationMatrix);

            // Translate the rotated camera position to the location of the viewer.
            //lookAt = position + lookAt;

            // Finally create the view matrix from the three updated vectors.
            View = Matrix.LookAtLH(position, lookAt, up);
        }
        #endregion
    }
}
