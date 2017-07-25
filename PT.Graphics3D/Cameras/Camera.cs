using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using SharpDX;
using PoncheToolkit.Graphics3D.Effects;

namespace PT.Graphics3D.Cameras
{
    /// <summary>
    /// Main class that represent a world-view-projection camera.
    /// </summary>
    public abstract class Camera : GameComponent
    {
        #region Fields
        private float fov;
        private float aspectRatio;
        private float nearPlane;
        private float farPlane;
        private Vector3 position;
        private Vector3 lookAt;
        private Vector3 rotation;
        //private Frustrum frustrum;
        private BoundingFrustum frustrum;
        #endregion

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
        /// The multiplied viewProjection matrix.
        /// </summary>
        internal Matrix ViewProjection;

        /// <summary>
        /// The field of view of the camera. Generally its PI / 4.
        /// </summary>
        public float FOV
        {
            get { return fov; }
            set { SetPropertyAsDirty(ref fov, value); }
        }

        /// <summary>
        /// The aspect ratio.
        /// </summary>
        public float AspectRatio
        {
            get { return aspectRatio; }
            set { SetPropertyAsDirty(ref aspectRatio, value); }
        }

        /// <summary>
        /// The near plane to clip the projection.
        /// </summary>
        public float NearPlane
        {
            get { return nearPlane; }
            set { SetPropertyAsDirty(ref nearPlane, value); }
        }

        /// <summary>
        /// The far plane to clip the projection.
        /// </summary>
        public float FarPlane
        {
            get { return farPlane; }
            set { SetPropertyAsDirty(ref farPlane, value); }
        }

        /// <summary>
        /// Position of the camera.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { SetProperty(ref position, value); }
        }

        /// <summary>
        /// The point where the camera is looking at.
        /// </summary>
        public Vector3 LookAt
        {
            get { return lookAt; }
            set { SetProperty(ref lookAt, value); }
        }

        /// <summary>
        /// Rotation of the camera.
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { SetProperty(ref rotation, value); }
        }

        /// <summary>
        /// The frustrum object to calculate the view frustrum.
        /// This frustrum must be updated typically at the end of the <see cref="UpdateLogic"/> method
        /// when the View Projection matrices has already been calculated.
        /// </summary>
        public BoundingFrustum Frustrum
        {
            get { return frustrum; }
            set { SetProperty(ref frustrum, value); }
        }
        #endregion

        /// <summary>
        /// Constructor. Set default values.
        /// </summary>
        /// <param name="game">The game instance.</param>
        public Camera(Game11 game)
            : base(game)
        {
            NearPlane = 0.1f;
            FarPlane = 1000f;
            AspectRatio = (float)Game.Form.Width / (float)Game.Form.Height;
            FOV = MathUtil.Pi / 4;
        }

        #region Public Methods
        /// <inheritdoc/>
        public abstract override void Dispose();

        /// <inheritdoc/>
        public override void Initialize()
        {
            // Create the matrices.
            Projection = Matrix.PerspectiveFovLH(FOV, AspectRatio, NearPlane, FarPlane);
            LookAt = Vector3.Zero;

            base.Initialize();
        }

        /// <inheritdoc/>
        public override void UpdateLogic()
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

            lookAt.Normalize();

            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
            up = Vector3.TransformCoordinate(up, rotationMatrix);

            // Translate the rotated camera position to the location of the viewer.
            //lookAt = position + lookAt;            

            // Finally create the view matrix from the three updated vectors.
            View = Matrix.LookAtLH(position, lookAt, up);

            frustrum = new BoundingFrustum(ViewProjection = Matrix.Multiply(View, Projection));
        }

        /// <summary>
        /// Update the wvp matrices and return a <see cref="MatricesStruct"/> to be sent to the GPU.
        /// This method transposes the matrices.
        /// </summary>
        /// <param name="world">The world matrix of the model.</param>
        /// <returns></returns>
        public virtual MatricesStruct GetMatrices(Matrix world)
        {
            //Matrix worldInverseTranspose = Matrix.Invert(world);
            //worldInverseTranspose = Matrix.Transpose(worldInverseTranspose);

            //Matrix worldView = Matrix.Multiply(world, View);
            Matrix worldViewProj = Matrix.Multiply(world, ViewProjection);
            worldViewProj.Transpose();
            world.Transpose();
            //worldView.Transpose();

            MatricesStruct matrices = new MatricesStruct(world, ViewProjection, worldViewProj, this.Position);
            
            return matrices;
        }

        /// <summary>
        /// Retrieve a reflection matrix from the current view.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public virtual Matrix GetReflectionMatrix(float height)
        {
            Vector3 up = Vector3.Up;
            Vector3 position = this.position;
            Vector3 lookAt = LookAt;
            Matrix rotationMatrix;

            // Setup the position of the camera in the world.
            // For planar reflection invert the Y position of the camera.
            position.Y = -this.position.Y + (height * 2.0f);

            // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
            float pitch = MathUtil.DegreesToRadians(this.Rotation.X);
            float yaw = MathUtil.DegreesToRadians(this.Rotation.Y);
            float roll = MathUtil.DegreesToRadians(this.Rotation.Z);

            // Create the rotation matrix from the yaw, pitch, and roll values.
            rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            lookAt.Normalize();

            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
            return Matrix.LookAtLH(position, lookAt, up);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                if (DirtyProperties.ContainsKey(nameof(AspectRatio)))
                    Projection = Matrix.PerspectiveFovLH(FOV, AspectRatio, NearPlane, FarPlane);

                IsStateUpdated = true;
                OnStateUpdated();
            }

            return IsStateUpdated;
        }
        #endregion
    }
}
