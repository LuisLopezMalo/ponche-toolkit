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
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Graphics3D.Cameras
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
        private Vector3 targetOffset;
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
            targetOffset = new Vector3(0, -2, 5);

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            if (Target == null)
                throw new NullReferenceException("The target has not been set.");
            
            //Position = Position - targetOffset;
            LookAt = target.Position;
            //Position = new Vector3(target.Position.X, target.Position.Y + 3, target.Position.Z - 4);

            Matrix rotationMatrix = Matrix.RotationAxis(Right, Rotation.Y) * Matrix.RotationY(-Rotation.X);
            Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            Up = Vector3.TransformNormal(Up, rotationMatrix);
            Direction.Normalize();
            Right = Vector3.Cross(Direction, Up);
            Up = Vector3.Cross(Right, Direction);

            //Matrix transform = Matrix.Identity;
            //transform.Forward = Direction;
            //transform.Up = Up;
            //transform.Right = Vector3.Cross(Up, Direction);
            //Up = Vector3.Normalize(Vector3.Cross(Position, Right));
            //Right = Vector3.Normalize(Vector3.Cross(Up, Position));

            //LookAt = Vector3.TransformNormal(Position, transform);
            //LookAt = Vector3.Transform(Direction, (Matrix3x3)transform);

            // Set up the view matrix and projection matrix.
            View = Matrix.LookAtLH(Position, LookAt, Up);

            //Frustrum.CreateFrustrum(this);
            Frustrum = new BoundingFrustum(ViewProjection = Matrix.Multiply(View, Projection));
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