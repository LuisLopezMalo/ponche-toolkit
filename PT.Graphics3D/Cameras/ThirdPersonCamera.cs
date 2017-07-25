using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using SharpDX;
using PoncheToolkit.Util;
using PoncheToolkit.Core.Management.Input;

namespace PT.Graphics3D.Cameras
{
    /// <summary>
    /// Camera that represent a first person view.
    /// </summary>
    public class ThirdPersonCamera : Camera
    {
        #region Fields
        private PTModel target;
        private float cameraMove = 0.2f;
        private float cameraRotation = 0.3f;
        #endregion

        #region Properties
        /// <summary>
        /// The target the camera will follow.
        /// </summary>
        public PTModel Target { get { return target; } set { SetProperty(ref target, value); } }
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public ThirdPersonCamera(Game11 game)
            : base(game)
        {
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            cameraMove = 0.2f;
            cameraRotation = 0.3f;

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void UpdateLogic()
        {
            if (Target == null)
                throw new NullReferenceException("The target has not been set.");
            
            LookAt = target.Position;
            Position = new Vector3(target.Position.X, target.Position.Y + 3, target.Position.Z - 4);

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

            Frustrum = new BoundingFrustum(ViewProjection = Matrix.Multiply(View, Projection));


            //base.UpdateLogic();
        }

        /// <inheritdoc/>
        public override void UpdateInput(InputManager inputManager)
        {
            Vector3 position = Position;
            Vector3 rotation = Rotation;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.A))
                position.X -= cameraMove;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.D))
                position.X += cameraMove;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.W))
                position.Y += cameraMove;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.S))
                position.Y -= cameraMove;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.E))
                position.Z += cameraMove;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Q))
                position.Z -= cameraMove;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.C))
                rotation.Y += cameraRotation;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Z))
                rotation.Y -= cameraRotation;

            Position = position;
            Rotation = rotation;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
        }
        #endregion
    }
}