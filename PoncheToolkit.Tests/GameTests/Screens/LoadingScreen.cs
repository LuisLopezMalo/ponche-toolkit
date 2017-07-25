using PoncheToolkit.Core.Management.Screen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Graphics3D.Cameras;
using PoncheToolkit.Graphics3D.Primitives;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Core.Services;
using SharpDX;
using SharpDX.Direct3D11;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;

namespace PoncheToolkit.Tests.GameTests.Screens
{
    public class LoadingScreen : GameScreen
    {
        private ThirdPersonCamera currentCamera;
        private PTModel nano;
        //private CommonEffect effect;
        private Graphics3D.Effects.PTForwardRenderEffect effect;
        private GameTime gameTime;

        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadRenderableComponentsHandler OnFinishLoadRenderableComponents;

        public LoadingScreen(Game11 game)
            : base(game)
        {
            currentCamera = new ThirdPersonCamera(game);
            //effect = new CommonEffect(game);
            
            gameTime = game.Services[typeof(GameTime)] as GameTime;

            OnFinishLoadContent += TestScreen_OnFinishLoadContent;
        }

        public override void Initialize()
        {
            nano = Game.ContentManager.LoadModel("Models/nano_suit/nanosuit.obj");

            currentCamera.Position = new Vector3(0, 0, -3.5f);
            currentCamera.Target = nano;

            Components.AddComponent(currentCamera, "Main_Camera");

            // TODO: Just for tests
            Game.Form.KeyDown += Form_KeyDown;

            base.Initialize();
        }

        public override void LoadContent(IContentManager contentManager)
        {
            nano.Size = new Vector3(0.4f, 0.4f, 0.4f);
            nano.Position = new Vector3(0, -3.5f, 0);

            base.LoadContent(contentManager);
        }

        public override void LoadShadersAndMaterials(IContentManager contentManager)
        {
            effect = contentManager.LoadEffect<PTForwardRenderEffect>("Effects/PTCommonEffect.fx", null);
        }

        public override void AddRenderableScreenComponents()
        {
            //AddRenderableComponentWithEffect(ref nano, ref effect, "nano");

            //for (int i = 0; i < models.Count; i++)
            //{
            //    Model mod = models[i];
            //    AddDrawableComponentWithEffect(ref mod, ref effect, "model" + i);
            //}

            OnFinishLoadRenderableComponents?.Invoke();
        }

        /// <summary>
        /// When finished loading all the content in the screen.
        /// </summary>
        private void TestScreen_OnFinishLoadContent()
        {
        }

        private float cameraMove = 0.2f;
        private float cameraRotation = 0.3f;
        private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Escape)
                this.Game.Shutdown();

            Vector3 position = currentCamera.Position;
            Vector3 rotation = currentCamera.Rotation;

            if (e.KeyCode == System.Windows.Forms.Keys.A)
                position.X -= cameraMove;

            if (e.KeyCode == System.Windows.Forms.Keys.D)
                position.X += cameraMove;

            if (e.KeyCode == System.Windows.Forms.Keys.W)
                position.Y += cameraMove;

            if (e.KeyCode == System.Windows.Forms.Keys.S)
                position.Y -= cameraMove;

            if (e.KeyCode == System.Windows.Forms.Keys.E)
                position.Z += cameraMove;

            if (e.KeyCode == System.Windows.Forms.Keys.Q)
                position.Z -= cameraMove;

            if (e.KeyCode == System.Windows.Forms.Keys.G)
                rotation.Y += cameraRotation;

            if (e.KeyCode == System.Windows.Forms.Keys.F)
                rotation.Y -= cameraRotation;

            currentCamera.Position = position;
            currentCamera.Rotation = rotation;

            // Change FillMode - Solid or Wirefram
            if (e.KeyCode == System.Windows.Forms.Keys.F1)
            {
                Game.Renderer.Rasterizer.FillMode = FillMode.Solid;
                Game.Renderer.Rasterizer.UpdateState();
            }

            if (e.KeyCode == System.Windows.Forms.Keys.F2)
            {
                Game.Renderer.Rasterizer.FillMode = FillMode.Wireframe;
                Game.Renderer.Rasterizer.UpdateState();
            }

            // ==== Send the next screen.
            //if (e.KeyCode == System.Windows.Forms.Keys.F10)
            //{
            //    TestScreen next = new TestScreen(Game);
            //    ToNext(next);
            //}

            // Change Fullscreen
            if (e.KeyCode == System.Windows.Forms.Keys.F5)
                Game.Settings.Fullscreen = true;

            if (e.KeyCode == System.Windows.Forms.Keys.F4)
                Game.Settings.Fullscreen = false;

            //// Change textures.
            //if (e.KeyCode == System.Windows.Forms.Keys.D1)
            //{
            //    triangle2.SetTexturePath(new TexturePath("cara1.jpg"), 0);
            //    duck.SetTexturePath(new TexturePath("cara1.jpg"), 0);
            //    triangle2.UpdateState();
            //}

            //if (e.KeyCode == System.Windows.Forms.Keys.D2)
            //{
            //    triangle2.SetTexturePath(new TexturePath("crate1.jpg"), 0);
            //    duck.SetTexturePath(new TexturePath("crate1.jpg"), 0);
            //    triangle2.UpdateState();
            //}
        }

        private void LoadingScreen_OnFinishLoadingNextScreen(GameScreen next)
        {
            Game.ScreenManager.AddScreen(next);
        }

        private int directionX = 1;
        public override void UpdateLogic(GameTime gameTime)
        {
            base.UpdateLogic(gameTime);

            nano.Rotation += new Vector3(0, .5f, 0) * gameTime.DeltaTime;
            // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
            float yaw = MathUtil.DegreesToRadians(nano.Rotation.Y);
            float pitch = MathUtil.DegreesToRadians(nano.Rotation.X);
            float roll = MathUtil.DegreesToRadians(nano.Rotation.Z);

            // Create the rotation matrix from the yaw, pitch, and roll values.
            Matrix rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);
            nano.Rotation = Vector3.TransformCoordinate(nano.Rotation, rotationMatrix);


        }

        public override void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            base.Render(spriteBatch, context);
        }

        public override void Dispose()
        {
            base.Dispose();
            Game.Form.KeyDown -= Form_KeyDown;
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            Log.Debug("UpdateState - updating state in screen - " + this.Name);
            return true;
        }
    }
}
