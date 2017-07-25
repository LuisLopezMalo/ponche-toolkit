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
        private Vector3 right;
        private Vector3 up;
        private Vector3 direction;
        private BoundingFrustum frustrum;
        private CameraType cameraType;
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
        /// <para>
        /// Default: Vector3.Zero
        /// </para>
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
        /// The calculated right vector.
        /// </summary>
        public Vector3 Right
        {
            get { return right; }
            set { SetProperty(ref right, value); }
        }

        /// <summary>
        /// The calculated up vector.
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { SetProperty(ref up, value); }
        }

        /// <summary>
        /// The direction vector where the camera is pointing.
        /// Default: Vector3.ForwardLH
        /// </summary>
        public Vector3 Direction
        {
            get { return direction; }
            set { SetProperty(ref direction, value); }
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

        /// <summary>
        /// The type of camera.
        /// <para>
        /// Default: <see cref="CameraType.Main"/>.
        /// </para>
        /// </summary>
        public CameraType Type
        {
            get { return cameraType; }
            set { SetProperty(ref cameraType, value); }
        }
        #endregion

        /// <summary>
        /// Constructor. Set default values.
        /// </summary>
        /// <param name="game">The game instance.</param>
#if DX11
        public Camera(Game11 game)
#elif DX12
        public Camera(Game12 game)
#endif
            : base(game)
        {
            NearPlane = 0.1f;
            FarPlane = 1000f;
            AspectRatio = (float)Game.Form.Width / (float)Game.Form.Height;
            FOV = MathUtil.Pi / 4;
            this.cameraType = CameraType.Main;
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
            Up = Vector3.Up;
            Direction = Vector3.ForwardLH;

            base.Initialize();
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            // Setup the initial values for this update.
            Vector3 up = Up;
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
        /// Create the LookAt vector.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        /// <param name="up"></param>
        public virtual void CreateLookAt(Vector3 pos, Vector3 target, Vector3 up)
        {
            Vector3 look = pos - target;
            //Vector3 L = target - pos;
            look = Vector3.Normalize(look);

            Vector3 right;
            right = Vector3.Cross(up, look);
            //R.Normalize();

            Vector3 U;
            U = Vector3.Cross(look, right);
            //U.Normalize();

            Position = pos;
            Right = right;
            Up = U;
            LookAt = look;
        }

        /// <summary>
        /// Create the LookAt vector.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        public virtual void CreateLookAt(Vector3 pos, Vector3 target)
        {
            Vector3 L = pos - target;
            //Vector3 L = target - pos;
            L = Vector3.Normalize(L);

            Vector3 R, U;
            if (Math.Abs(Vector3.Dot(Up, L)) < .5f)
            {
                R = Vector3.Cross(Up, L);
                U = Vector3.Cross(L, R);
            }
            else
            {
                U = Vector3.Cross(L, Right);
                R = Vector3.Cross(U, L);
            }

            Position = pos;
            Right = R;
            Up = U;
            LookAt = L;
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
        /// <param name="offset"></param>
        /// <returns></returns>
        public virtual Matrix GetReflectionMatrix(float offset)
        {
            //Vector3 up = Vector3.Up;
            //Vector3 position = this.position;
            //Vector3 lookAt = LookAt;
            //Matrix rotationMatrix;

            //// Setup the position of the camera in the world.
            //// For planar reflection invert the Y position of the camera.
            //position.Y = -this.position.Y + (height * 2.0f);

            //// Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
            //float pitch = MathUtil.DegreesToRadians(this.Rotation.X);
            //float yaw = MathUtil.DegreesToRadians(this.Rotation.Y);
            //float roll = MathUtil.DegreesToRadians(this.Rotation.Z);

            //// Create the rotation matrix from the yaw, pitch, and roll values.
            //rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            //lookAt.Normalize();

            //// Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            //lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
            //return Matrix.LookAtLH(position, lookAt, up);

            // Setup the position of the camera in the world.
            var position = new Vector3(Position.X, -Position.Y + (offset * 2), Position.Z);

            // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
            var yaw = Rotation.Y * 0.0174532925f;

            // Setup where the camera is looking by default.
            var lookAt = new Vector3((float)Math.Sin(yaw) + position.X, position.Y, (float)Math.Cos(yaw) + position.Z);

            // Finally create the reflection view matrix from the three updated vectors.
            return Matrix.LookAtLH(position, lookAt, Vector3.Up);
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
