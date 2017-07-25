using PoncheToolkit.Core.Management.Screen;
using PoncheToolkit.Graphics3D.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Graphics3D.Cameras;
using SharpDX;
using PoncheToolkit.Core.Management.Input;
using SharpDX.XInput;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Tests.GameTests.Screens
{
    using PoncheToolkit.Graphics2D;

    public class DemoScreen2 : GameScreen
    {
        private List<Square> walls;
        private Square floor;

        private PTMaterial wallBricksMaterial;
        private PTMaterial wallRocksMaterial;
        private PTMaterial colorMaterial;
        private PTMaterial crateMaterial;

        private PTModel duckModel;

        private PTForwardRenderEffect basicEffect;
        private ThirdPersonCamera chaseCamera;

        public DemoScreen2(Game11 game)
            : base(game)
        {
            walls = new List<Square>();
        }

        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadRenderableComponentsHandler OnFinishLoadRenderableComponents;

        public override void Initialize()
        {
            //freeCamera = new FirstPersonCamera(Game);
            //freeCamera.Position = new Vector3(0, 1, -2f);
            //freeCamera.FastMovement = false;
            //Components.AddComponent(freeCamera, "FirstPerson camera");

            chaseCamera = new ThirdPersonCamera(Game);
            chaseCamera.Position = new Vector3(0, 1, -2f);
            Components.AddComponent(chaseCamera, "Chase camera");

            Vector3 wallSize = new Vector3(12);
            float separation = 12;
            int side = 1;
            for (int i = 0; i<20; i++)
            {
                Square wall = new Square(Game);
                wall.Size = wallSize;
                wall.UVRepeatFactor = 2;
                wall.Rotation = new Vector3(0, MathUtil.DegreesToRadians(90), 0);
                if (i >= 10)
                {
                    side = -1;
                    wall.Rotation = new Vector3(0, MathUtil.DegreesToRadians(-90), 0);
                }
                wall.Position = new Vector3(side * separation, 6, wallSize.Z * (i % 10));
                walls.Add(wall);
            }

            floor = new Square(Game);
            floor.Size = new Vector3(12 * 20, 24, 1);
            floor.Rotation = new Vector3(MathUtil.DegreesToRadians(90), MathUtil.DegreesToRadians(-90), 0);

            Game.ToggleBlending(BlendingState.AlphaBlending);

            base.Initialize();
        }

        public override void LoadContent(IContentManager contentManager)
        {
            duckModel = contentManager.LoadModel("Models/duck/duck.dae");
            duckModel.Size = new Vector3(0.008f);
            duckModel.Position = new Vector3(0f, 1, 1);
            duckModel.Rotation = new Vector3(0, MathUtil.DegreesToRadians(-90), 0);

            chaseCamera.Target = duckModel;

            base.LoadContent(contentManager);
        }

        PTLight pointLight;
        PTLight directionalLight;
        PTLight spotLight;
        PTLight spotLight2;
        PTLight spotLight3;
        public override void LoadShadersAndMaterials(IContentManager contentManager)
        {
            basicEffect = new PTForwardRenderEffect(Game, null);
            basicEffect.LoadContent(contentManager);
            basicEffect.GlobalAmbientColor = new Vector4(0.1f, 0.1f, 0.1f, 1);
            basicEffect.GlobalAmbientColor = Vector4.Zero;

            directionalLight = new PTLight();
            directionalLight.Color = new Vector4(0.6f, 0.5f, 0.5f, 1);
            directionalLight.Direction = new Vector3(0.3f, 0.1f, 0.7f);
            //directionalLight.Direction = new Vector3(0.3f, 0f, 0.9f);
            directionalLight.Type = LightType.Directional;
            directionalLight.IsEnabled = true;
            directionalLight.LoadDebugContent(Game);

            pointLight = new PTLight();
            pointLight.Position = new Vector3(0f, -2, 0);
            pointLight.Color = new Vector4(0.2f, 0.5f, 1f, 1);
            pointLight.Type = LightType.Point;
            pointLight.ConstantAttenuation = 1f;
            //pointLight.LinearAttenuation = 0.0008f;
            //pointLight.LinearAttenuation = 0.008f;
            //pointLight.QuadraticAttenuation = 0.0001f;
            pointLight.IsEnabled = true;
            pointLight.LoadDebugContent(Game);

            spotLight = new PTLight();
            spotLight.Color = new Vector4(0.1f, 0.1f, 1f, 1);
            spotLight.Type = LightType.Spot;
            spotLight.SpotAngle = 100;
            spotLight.ConstantAttenuation = 0.9f;
            spotLight.LinearAttenuation = 0.1f;
            spotLight.QuadraticAttenuation = 0f;
            spotLight.Position = new Vector3(2.5f, 1, 0);
            spotLight.IsEnabled = true;
            spotLight.LoadDebugContent(Game);

            spotLight2 = new PTLight();
            spotLight2.Position = new Vector3(-2, 2, 0);
            spotLight2.Color = new Vector4(0.1f, 1f, 0.1f, 1);
            spotLight2.Type = LightType.Spot;
            spotLight2.SpotAngle = 100;
            spotLight2.ConstantAttenuation = 0.9f;
            spotLight2.LinearAttenuation = 0.1f;
            spotLight2.QuadraticAttenuation = 0f;
            spotLight2.IsEnabled = true;
            spotLight2.LoadDebugContent(Game);

            spotLight3 = new PTLight();
            spotLight3.Position = new Vector3(-2, -2, 0);
            spotLight3.Color = new Vector4(1f, 1f, 1f, 1);
            spotLight3.Type = LightType.Spot;
            spotLight3.SpotAngle = 100;
            spotLight3.ConstantAttenuation = 0.9f;
            spotLight3.LinearAttenuation = 0.1f;
            spotLight3.QuadraticAttenuation = 0f;
            spotLight3.IsEnabled = true;
            spotLight3.LoadDebugContent(Game);

            //basicEffect.AddLight(directionalLight);
            basicEffect.AddLight(pointLight);
            //basicEffect.AddLight(spotLight);
            //basicEffect.AddLight(spotLight2);
            //basicEffect.AddLight(spotLight3);


            //Vector4 diffuse = new Vector4(0.75f, 0.75f, 0.75f, 1f);
            //Vector4 ambient = new Vector4(0.4f, 0.4f, 0.4f, 1f);
            Vector4 diffuse = new Vector4(0.31f, 0.31f, 0.31f, 1f);
            Vector4 ambient = new Vector4(0.8f, 0.8f, 0.8f, 1f);

            #region Materials
            wallBricksMaterial = new PTMaterial(Game, new Graphics3D.PTTexturePath("Textures/bricks.gif", PTTexture2D.TextureType.Render, true));
            wallBricksMaterial.AddTexturePath(new Graphics3D.PTTexturePath("Textures/bricks_bump.gif", Graphics2D.PTTexture2D.TextureType.BumpMap), true);
            wallBricksMaterial.IsSpecularEnabled = true;
            wallBricksMaterial.IsBumpEnabled = true;
            //wallBricksMaterial.EmissiveColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            wallBricksMaterial.EmissiveColor = new Vector4(0.3f, 0.3f, 0.3f, 1f);
            wallBricksMaterial.DiffuseColor = diffuse;
            wallBricksMaterial.AmbientColor = ambient;
            wallBricksMaterial.SpecularColor = new Vector4(1f, 1f, 1f, 1f);
            wallBricksMaterial.SpecularPower = 5;
            wallBricksMaterial.LoadContent(contentManager);

            wallRocksMaterial = new PTMaterial(Game, new Graphics3D.PTTexturePath("Textures/wall_red.jpg", PTTexture2D.TextureType.Render, true));
            wallRocksMaterial.AddTexturePath(new Graphics3D.PTTexturePath("Textures/wall_red_bump.jpg", Graphics2D.PTTexture2D.TextureType.BumpMap), true);
            //wallRocksMaterial.AddTexturePath(new Graphics3D.TexturePath("Textures/wall_red_specular.jpg", Graphics2D.PTTexture2D.TextureType.SpecularMap), true);
            //wallRocksMaterial = new PTMaterial(Game);
            wallRocksMaterial.SpecularPower = 20f;
            wallRocksMaterial.EmissiveColor = new Vector4(0.3f, 0.3f, 0.3f, 1f);
            wallRocksMaterial.AmbientColor = ambient;
            wallRocksMaterial.DiffuseColor = new Vector4(0.51f, 0.51f, 0.51f, 1f);
            wallRocksMaterial.SpecularColor = new Vector4(1f, 0.2f, 0.2f, 1f);
            wallRocksMaterial.IsSpecularEnabled = true;
            wallRocksMaterial.IsBumpEnabled = true;
            wallRocksMaterial.LoadContent(contentManager);

            //reflectiveFloorMaterial = new PTMaterial(Game, new Graphics3D.TexturePath("Textures/reflective_floor.png"));
            //reflectiveFloorMaterial.AddTexturePath(new Graphics3D.TexturePath("Textures/reflective_floor_bump.jpg", Graphics2D.PTTexture2D.TextureType.BumpMap), true);
            ////reflectiveFloorMaterial.PointLight1 = point;
            //reflectiveFloorMaterial.IsSpecularEnabled = true;
            //reflectiveFloorMaterial.IsBumpEnabled = true;
            //reflectiveFloorMaterial.IsReflectivityEnabled = true;
            //reflectiveFloorMaterial.EmissiveColor = new Vector4(0.3f, 0.3f, 0.3f, 0f);
            //reflectiveFloorMaterial.AmbientColor = ambient;
            //reflectiveFloorMaterial.DiffuseColor = new Vector4(0.51f, 0.51f, 0.51f, 1f);
            //reflectiveFloorMaterial.SpecularColor = new Vector4(1f, 1f, 1f, 1f);
            //reflectiveFloorMaterial.LoadContent(contentManager);
            //reflectiveFloorMaterial.UpdateState();

            //reflectiveRenderTarget.Initialize();
            //reflectiveRenderTarget.Texture = reflectiveFloorMaterial.TextureByType(PTTexture2D.TextureType.Reflective);


            crateMaterial = new PTMaterial(Game, new Graphics3D.PTTexturePath("crate1.jpg"));
            crateMaterial.AddTexturePath(new Graphics3D.PTTexturePath("Textures/crate1_bump.jpg", Graphics2D.PTTexture2D.TextureType.BumpMap), true);
            crateMaterial.SpecularPower = 55f;
            crateMaterial.IsSpecularEnabled = true;
            crateMaterial.AmbientColor = ambient;
            crateMaterial.EmissiveColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            crateMaterial.DiffuseColor = new Vector4(0.51f, 0.51f, 0.51f, 1f);
            crateMaterial.SpecularColor = new Vector4(1f, 0.2f, 0.2f, 1f);
            crateMaterial.IsBumpEnabled = true;
            crateMaterial.LoadContent(contentManager);

            colorMaterial = new PTMaterial(Game);
            colorMaterial.EmissiveColor = new Vector4(0.6f, 0.6f, 0.6f, 1f);
            colorMaterial.DiffuseColor = new Vector4(0.51f, 0.51f, 0.51f, 1f);
            colorMaterial.SpecularColor = new Vector4(1f, 1f, 1f, 1f);
            colorMaterial.SpecularPower = 50f;
            colorMaterial.AmbientColor = ambient;
            colorMaterial.IsSpecularEnabled = true;
            colorMaterial.IsBumpEnabled = false;
            colorMaterial.LoadContent(contentManager);
            #endregion

            basicEffect.AddMaterial("wallMaterial", wallBricksMaterial);
            basicEffect.AddMaterial("wallRocksMaterial", wallRocksMaterial);
            basicEffect.AddMaterial("colorMaterial", colorMaterial);
            //basicEffect.AddMaterial("reflectiveFloorMaterial", reflectiveFloorMaterial);
            //basicEffect.AddMaterial("crateMaterial", crateMaterial);
        }

        public override void AddRenderableScreenComponents()
        {
            //for (int i = 0; i < walls.Count; i++)
            //{
            //    Square w = walls[i];
            //    AddRenderableComponentWithEffect(ref w, ref basicEffect, "wall" + i);
            //    w.SetMaterial(wallBricksMaterial);
            //}

            //AddRenderableComponentWithEffect(ref duckModel, ref basicEffect, "duck");

            ////AddRenderableComponentWithEffect(ref floor, ref basicEffect, "floor");
            ////floor.SetMaterial(wallRocksMaterial);

            ////PTModel model1 = directionalLight.DebugModel;
            ////PTModel model3 = spotLight.DebugModel;
            ////PTModel model4 = spotLight2.DebugModel;
            ////PTModel model5 = spotLight3.DebugModel;
            ////AddRenderableComponentWithEffect(ref model1, ref basicEffect, "lightModel1");
            ////AddRenderableComponentWithEffect(ref model3, ref basicEffect, "lightModel3");
            ////AddRenderableComponentWithEffect(ref model4, ref basicEffect, "lightModel4");
            ////AddRenderableComponentWithEffect(ref model5, ref basicEffect, "lightModel5");

            //PTModel model2 = pointLight.DebugModel;
            //AddRenderableComponentWithEffect(ref model2, ref basicEffect, "lightModel2");

            OnFinishLoadRenderableComponents?.Invoke();
        }

        public override void UpdateLogic(GameTime gameTime)
        {
            base.UpdateLogic(gameTime);
        }

        public override void UpdateInput(InputManager inputManager)
        {
            base.UpdateInput(inputManager);

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.Escape))
                Game.Shutdown();

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.F12))
                Game.Settings.Fullscreen = !Game.Settings.Fullscreen;

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.F8) ||
                inputManager.IsButtonReleased(UserIndex.Any, GamepadButtonFlags.B))
                Game.VerticalSyncEnabled = !Game.VerticalSyncEnabled;

            //if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D1) ||
            //    inputManager.IsButtonReleased(UserIndex.Any, GamepadButtonFlags.A))
            //{
            //    foreach (PTMaterial mat in basicEffect.Materials.Values)
            //        mat.IsBumpEnabled = !mat.IsBumpEnabled;
            //}
            //if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D2))
            //{
            //    foreach (PTMaterial mat in basicEffect.Materials.Values)
            //        mat.IsSpecularEnabled = !mat.IsSpecularEnabled;
            //}

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D0) ||
                inputManager.IsButtonReleased(UserIndex.Any, GamepadButtonFlags.RightThumb))
                Game.Renderer.Rasterizer.FillMode = Game.Renderer.Rasterizer.FillMode == SharpDX.Direct3D11.FillMode.Solid ? SharpDX.Direct3D11.FillMode.Wireframe : SharpDX.Direct3D11.FillMode.Solid;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.LeftShift))
                wallRocksMaterial.TextureTranslation -= new Vector2(0.005f, 0);
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.RightShift))
                wallRocksMaterial.TextureTranslation += new Vector2(0.005f, 0);


            //// === Lighting properties
            //if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.PageDown)
            //    || inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.LeftShoulder))
            //{
            //    foreach (PTMaterial mat in basicEffect.Materials.Values)
            //    {
            //        if (!mat.IsReflectivityEnabled)
            //            mat.SpecularPower -= 0.07f;
            //        mat.SpecularPower = Math.Max(1f, mat.SpecularPower);

            //        if (mat.IsReflectivityEnabled)
            //            mat.Reflectivity -= 0.005f;
            //    }
            //}
            //if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.PageUp)
            //    || inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.RightShoulder))
            //{
            //    foreach (PTMaterial mat in basicEffect.Materials.Values)
            //    {
            //        if (!mat.IsReflectivityEnabled)
            //            mat.SpecularPower += 0.07f;

            //        if (mat.IsReflectivityEnabled)
            //            mat.Reflectivity += 0.005f;
            //    }
            //}


            // Move light
            Vector3 pointLightPosition = basicEffect.Lights.Last().Position;
            //if (inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.DPadLeft))
            //    pointLightPosition.X -= 0.005f;
            //if (inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.DPadRight))
            //    pointLightPosition.X += 0.005f;
            //if (inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.DPadUp))
            //    pointLightPosition.Z += 0.005f;
            //if (inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.DPadDown))
            //    pointLightPosition.Z -= 0.005f;


            Gamepad? state = inputManager.GamepadState(SharpDX.XInput.UserIndex.One);
            if (state != null)
            {
                if (state.Value.LeftThumbY > InputManager.GAMEPAD_MIN_THUMB_THRESHOLD || state.Value.LeftThumbY < -InputManager.GAMEPAD_MIN_THUMB_THRESHOLD)
                    duckModel.Position = new Vector3(duckModel.Position.X, duckModel.Position.Y, duckModel.Position.Z + (state.Value.LeftThumbY * 0.0001f * Game.GameTime.DeltaTime));

                if (state.Value.RightThumbY > InputManager.GAMEPAD_MIN_THUMB_THRESHOLD || state.Value.RightThumbY < -InputManager.GAMEPAD_MIN_THUMB_THRESHOLD)
                    duckModel.Position = new Vector3(duckModel.Position.X, duckModel.Position.Y + (state.Value.RightThumbY * 0.0001f * Game.GameTime.DeltaTime), duckModel.Position.Z);
            }

            //// Move light with mouse
            //Vector3 positionDelta = new Vector3(inputManager.MouseDelta.X * 0.03f, inputManager.MouseDelta.Y * 0.03f, 0);
            //pointLightPosition = new Vector3(pointLightPosition.X + inputManager.MouseDelta.X * 0.03f, pointLightPosition.Y - inputManager.MouseDelta.Y * 0.03f, pointLightPosition.Z);

            //if (inputManager.IsMouseHold(InputManager.MouseButton.Left))
            //{
            //    //basicEffect.GlobalAmbientColor = new Vector4(Math.Min(1, basicEffect.GlobalAmbientColor.X + 0.01f), basicEffect.GlobalAmbientColor.Y, basicEffect.GlobalAmbientColor.Z, 1);
            //    pointLightPosition.Z += 0.05f;
            //    positionDelta.Z += 0.05f;
            //}
            //if (inputManager.IsMouseHold(InputManager.MouseButton.Right))
            //{
            //    //basicEffect.GlobalAmbientColor = new Vector4(Math.Max(0, basicEffect.GlobalAmbientColor.X - 0.01f), basicEffect.GlobalAmbientColor.Y, basicEffect.GlobalAmbientColor.Z, 1);
            //    pointLightPosition.Z -= 0.05f;
            //    positionDelta.Z -= 0.05f;
            //}

            //positionDelta.Y *= -1; 
            //foreach (PTLight light in basicEffect.Lights)
            //{
            //    light.Position += positionDelta;
            //}


            pointLight.Position = new Vector3(duckModel.Position.X, duckModel.Position.Y + 2, duckModel.Position.Z + 0.5f);
        }
        
        public override void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            //if (renderReflection)
            //{
            //    //Game.Renderer.RenderScreenToTexture(this, Game.CurrentCamera, Game.Renderer.SpriteBatch, reflectiveFloorMaterial.reflectionRenderTargetView); //reflectiveFloorMaterial.reflectionDepthStencil);
            //    Game.Renderer.RenderScreenToTexture(this, Game.CurrentCamera, Game.Renderer.SpriteBatch, ref reflectiveRenderTarget, floor.Meshes); //reflectiveFloorMaterial.reflectionDepthStencil);
            //    spriteBatch.Begin();
            //    spriteBatch.DrawTexture(reflectiveRenderTarget.Bitmap, new Vector2(0, Game.Settings.Resolution.Height - 250), new Vector2(0.32f, 0.32f));
            //    spriteBatch.End();
            //}

            //// === To render into a Square.
            //if (created && !squareCreated)
            //{
            //    Square sq = new Square(this.Game);
            //    sq.Position = new Vector3(2, 0.5f, 3.5f);
            //    sq.Size = new Vector3(2f * 1.6f, 2f * 0.9f, 0); // Aspect ratio - 16:9
            //    AddRenderableComponentWithEffect(ref sq, ref basicEffect, "renderTotexture");

            //    PTMaterial textureMaterial = new PTMaterial(Game);
            //    textureMaterial.Name = "Texture material";
            //    textureMaterial.AddTexture(reflectionTexture, PTTexture2D.TextureType.Render);
            //    textureMaterial.IsReflectivityEnabled = true;
            //    basicEffect.AddMaterial(textureMaterial);
            //    sq.SetMaterial(textureMaterial);

            //    squareCreated = true;
            //}
        }

        public override bool UpdateState()
        {
            if (IsStateUpdated)
                return IsStateUpdated;

            OnStateUpdated();
            return IsStateUpdated;
        }
    }
}
