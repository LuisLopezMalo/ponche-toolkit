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
using SharpDX.Direct3D11;

namespace PoncheToolkit.Graphics3D.Cameras
{
    /// <summary>
    /// Camera that represent a first person view.
    /// </summary>
    public class ReflectionCamera : Camera
    {
        private float keyboardCameraMove;
        private float gamepadCameraMove;
        private float cameraRotation;
        private PTRenderTarget2D renderTarget;

        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Properties
        /// <summary>
        /// The render target used to render the reflections.
        /// </summary>
        public PTRenderTarget2D RenderTarget
        {
            get { return renderTarget; }
            set { renderTarget = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
#if DX11
        public ReflectionCamera(Game11 game)
#elif DX12
        public ReflectionCamera(Game12 game)
#endif
            : base(game)
        {
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            base.Initialize();
            OnInitialized?.Invoke();
        }

        public void CreateRenderTarget()
        {
            renderTarget = new PTRenderTarget2D(Game.Renderer, "ReflectionCameraRT");
            renderTarget.Initialize();
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {   
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
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
        }
        #endregion
    }
}
