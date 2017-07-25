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
    public class FirstPersonCamera : Camera
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
                    SetProperty(ref cameraRotation, 3.5f);
                    SetProperty(ref gamepadCameraMove, 0.001f);
                }
                else
                {
                    SetProperty(ref keyboardCameraMove, 0.5f);
                    SetProperty(ref cameraRotation, 1.3f);
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
        public FirstPersonCamera(Game11 game)
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
            //// Calculate the camera's current position.
            //Vector3 cameraPosition = Position;

            //Matrix3x3 rotationMatrix = Matrix3x3.Multiply(Matrix3x3.RotationX(-Rotation.X), Matrix3x3.RotationY(Rotation.Y));

            //// Create a vector pointing the direction the camera is facing.
            //Vector3 transformedReference = Vector3.Transform(new Vector3(0, 0, 10), rotationMatrix);

            //// Calculate the position the camera is looking at.
            //Vector3 cameraLookat = cameraPosition + transformedReference;

            //// Set up the view matrix and projection matrix.
            //View = Matrix.LookAtLH(cameraPosition, cameraLookat, Vector3.Up);

            ////Frustrum.CreateFrustrum(this);
            //Frustrum = new BoundingFrustum(ViewProjection = Matrix.Multiply(View, Projection));



            // Calculate the camera's current position.
            Vector3 cameraPosition = Position;

            //Matrix3x3 rotationMatrix = Matrix3x3.Multiply(Matrix3x3.RotationX(-Rotation.X), Matrix3x3.RotationY(Rotation.Y));
            Matrix rotationMatrix = Matrix.Multiply(Matrix.RotationX(-Rotation.X), Matrix.RotationY(Rotation.Y));
            Quaternion quat = Quaternion.RotationMatrix(rotationMatrix);

            // Create a vector pointing the direction the camera is facing.
            Vector3 transformedReference = Vector3.Transform(new Vector3(0, 0, 1), quat);

            // Calculate the position the camera is looking at.
            Vector3 cameraLookat = cameraPosition + transformedReference;

            //LookAt.Normalize();
            //LookAt = Vector3.TransformCoordinate(LookAt, rotationMatrix);

            // Set up the view matrix and projection matrix.
            View = Matrix.LookAtLH(cameraPosition, cameraLookat, Vector3.Up);

            //Frustrum.CreateFrustrum(this);
            Frustrum = new BoundingFrustum(ViewProjection = Matrix.Multiply(View, Projection));
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
        }
        #endregion
    }
}
