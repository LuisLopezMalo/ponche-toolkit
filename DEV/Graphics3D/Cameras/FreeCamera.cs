using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Util;
using SharpDX;
using PoncheToolkit.Core.Management.Input;
using SharpDX.XInput;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Graphics3D.Cameras
{
    /// <summary>
    /// Camera that represent a first person view.
    /// </summary>
    public class FreeCamera : Camera
    {
        private float keyboardCameraMove;
        private float gamepadCameraMove;
        private float cameraRotation;
        private bool fastMovement;

        /// <summary>
        /// To set the movment of the camera to a faster pace.
        /// </summary>
        public bool FastMovement
        {
            get { return fastMovement; }
            set
            {
                SetProperty(ref fastMovement, value);
                if (fastMovement)
                {
                    SetProperty(ref keyboardCameraMove, 5);
                    SetProperty(ref cameraRotation, 30f);
                    SetProperty(ref gamepadCameraMove, 0.001f);
                }
                else
                {
                    SetProperty(ref keyboardCameraMove, 0.5f);
                    SetProperty(ref cameraRotation, 16f);
                    SetProperty(ref gamepadCameraMove, 0.0001f);
                }
            }
        }

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
#if DX11
        public FreeCamera(Game11 game)
#elif DX12
        public FreeCamera(Game12 game)
#endif
            : base(game)
        {
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            if (!fastMovement)
                FastMovement = false;

            IsInitialized = true;
            base.Initialize();
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void UpdateInput(InputManager inputManager)
        {
            Vector3 position = Position;
            Vector3 rotation = Rotation;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.A))
                position.X -= keyboardCameraMove * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.D))
                position.X += keyboardCameraMove * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.W))
                position.Y += keyboardCameraMove * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.S))
                position.Y -= keyboardCameraMove * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.E))
                position.Z += keyboardCameraMove * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Q))
                position.Z -= keyboardCameraMove * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.C))
                rotation.Y += cameraRotation * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Z))
                rotation.Y -= cameraRotation * Game.GameTime.DeltaTime;

            Position = position;
            Rotation = rotation;

            manageGamepad(inputManager);
        }

        private void manageGamepad(InputManager inputManager)
        {
            Gamepad? state = inputManager.GamepadState(SharpDX.XInput.UserIndex.One);
            if (state != null)
            {
                Vector3 position = Position;
                Vector3 rotation = Rotation;

                // Translation
                if (state.Value.LeftThumbX > InputManager.GAMEPAD_MIN_THUMB_THRESHOLD || state.Value.LeftThumbX < -InputManager.GAMEPAD_MIN_THUMB_THRESHOLD)
                    position.X += (state.Value.LeftThumbX * gamepadCameraMove) * Game.GameTime.DeltaTime;

                if (state.Value.LeftThumbY > InputManager.GAMEPAD_MIN_THUMB_THRESHOLD || state.Value.LeftThumbY < -InputManager.GAMEPAD_MIN_THUMB_THRESHOLD)
                    position.Z += (state.Value.LeftThumbY * gamepadCameraMove) * Game.GameTime.DeltaTime;

                if (state.Value.RightTrigger > InputManager.GAMEPAD_MIN_TRIGGER_THRESHOLD || state.Value.RightTrigger < -InputManager.GAMEPAD_MIN_TRIGGER_THRESHOLD)
                    position.Y += (state.Value.RightTrigger * 0.06f) * Game.GameTime.DeltaTime;

                if (state.Value.LeftTrigger > InputManager.GAMEPAD_MIN_TRIGGER_THRESHOLD || state.Value.LeftTrigger < -InputManager.GAMEPAD_MIN_TRIGGER_THRESHOLD)
                    position.Y -= (state.Value.LeftTrigger * 0.06f) * Game.GameTime.DeltaTime;

                // Rotation
                if (state.Value.RightThumbY > InputManager.GAMEPAD_MIN_THUMB_THRESHOLD || state.Value.RightThumbY < -InputManager.GAMEPAD_MIN_THUMB_THRESHOLD)
                    rotation.X += (state.Value.RightThumbY * 0.00006f) * Game.GameTime.DeltaTime;

                if (state.Value.RightThumbX > InputManager.GAMEPAD_MIN_THUMB_THRESHOLD || state.Value.RightThumbX < -InputManager.GAMEPAD_MIN_THUMB_THRESHOLD)
                    rotation.Y += (state.Value.RightThumbX * 0.00006f) * Game.GameTime.DeltaTime;

                Position = position;
                Rotation = rotation;
            }
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            //Vector3 cameraPosition = Vector3.Normalize(target - pos);

            //Matrix3x3 rotationMatrix = Matrix3x3.Multiply(Matrix3x3.RotationX(-Rotation.X), Matrix3x3.RotationY(Rotation.Y));
            //Matrix rotationMatrix = Matrix.Multiply(Matrix.RotationX(-Rotation.X), Matrix.RotationY(Rotation.Y));
            //Quaternion quat = Quaternion.RotationMatrix(rotationMatrix);

            //LookAt = Vector3.TransformNormal(Vector3.Zero, transform);

            //// Create a vector pointing the direction the camera is facing.
            //Vector3 facing = new Vector3(0, 0, 1);
            //Vector3 transformedReference = Vector3.Transform(facing, quat);

            //// Calculate the position the camera is looking at.
            //LookAt = Position + transformedReference;

            ////LookAt.Normalize();
            ////LookAt = Vector3.TransformCoordinate(LookAt, rotationMatrix);

            //// Set up the view matrix and projection matrix.
            //View = Matrix.LookAtLH(Position, LookAt, Vector3.Up);

            //Frustrum = new BoundingFrustum(ViewProjection = Matrix.Multiply(View, Projection));



            
            // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
            float pitch = MathUtil.DegreesToRadians(this.Rotation.X);
            float yaw = MathUtil.DegreesToRadians(this.Rotation.Y);
            float roll = MathUtil.DegreesToRadians(this.Rotation.Z);

            // Create the rotation matrix from the yaw, pitch, and roll values.
            Matrix rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);
            Quaternion quat = Quaternion.RotationMatrix(rotationMatrix);

            Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            Direction.Normalize();
            Right = Vector3.Cross(Direction, Up);

            // Create a vector pointing the direction the camera is facing.
            Vector3 facing = new Vector3(0, 0, 1);
            Vector3 transformedReference = Vector3.Transform(facing, quat);

            // Calculate the position the camera is looking at.
            LookAt = Position + transformedReference;

            //LookAt.Normalize();
            //LookAt = Vector3.TransformCoordinate(LookAt, rotationMatrix);

            // Set up the view matrix and projection matrix.
            View = Matrix.LookAtLH(Position, LookAt, Vector3.Up);

            Frustrum = new BoundingFrustum(ViewProjection = Matrix.Multiply(View, Projection));




            //Matrix rotationMatrix = Matrix.RotationAxis(Right, Rotation.Y) * Matrix.RotationY(-Rotation.X);
            //Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            //Up = Vector3.TransformNormal(Up, rotationMatrix);
            //Direction.Normalize();
            //Right = Vector3.Cross(Direction, Up);
            //Up = Vector3.Cross(Right, Direction);

            ////Matrix transform = Matrix.Identity;
            ////transform.Forward = Direction;
            ////transform.Up = Up;
            ////transform.Right = Vector3.Cross(Up, Direction);
            ////Up = Vector3.Normalize(Vector3.Cross(Position, Right));
            ////Right = Vector3.Normalize(Vector3.Cross(Up, Position));

            ////LookAt = Vector3.TransformNormal(Position, transform);
            ////LookAt = Vector3.Transform(Direction, (Matrix3x3)transform);

            //// Set up the view matrix and projection matrix.
            //View = Matrix.LookAtLH(Position, LookAt, Up);

            ////Frustrum.CreateFrustrum(this);
            //Frustrum = new BoundingFrustum(ViewProjection = Matrix.Multiply(View, Projection));



        }

        /*
         * private void UpdateWorldPositions()
        {
            // Construct a matrix to transform from object space to worldspace
            Matrix transform = Matrix.Identity;
            transform.Forward = ChaseDirection;
            transform.Up = Up;
            transform.Right = Vector3.Cross(Up, ChaseDirection);

            // Calculate desired camera properties in world space
            desiredPosition = ChasePosition +
                Vector3.TransformNormal(DesiredPositionOffset, transform);
            lookAt = ChasePosition +
                Vector3.TransformNormal(LookAtOffset, transform);
        }

        /// <summary>
        /// Animates the camera from its current position towards the desired offset
        /// behind the chased object. The camera's animation is controlled by a simple
        /// physical spring attached to the camera and anchored to the desired position.
        /// </summary>
        public void Update(GameTime gameTime, Vector3 targetPosition, Vector3 targetDirection, Vector3 targetUp)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            UpdateWorldPositions();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate spring force
            Vector3 stretch = position - desiredPosition;
            Vector3 force = -stiffness * stretch - damping * velocity;

            // Apply acceleration
            Vector3 acceleration = force / mass;
            velocity += acceleration * elapsed;

            // Apply velocity
            position += velocity * elapsed;

            UpdateMatrices();
            UpdateCameraChaseTarget(targetPosition, targetDirection, targetUp);
        }
        */


        /// <inheritdoc/>
        public override void Dispose()
        {
        }
        #endregion
    }
}
