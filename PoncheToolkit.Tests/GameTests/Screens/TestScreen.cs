using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Screen;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Cameras;
using PoncheToolkit.Graphics3D.Primitives;
using SharpDX;
using PoncheToolkit.Core.Components;
using SharpDX.Direct3D11;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Util;
using PoncheToolkit.Core.Management.Input;
using PoncheToolkit.Core.Management.Threading;
using PoncheToolkit.Graphics2D.Animation;
using PoncheToolkit.Graphics2D.Lighting;
using SharpDX.DXGI;
using PoncheToolkit.Graphics2D.Effects;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct2D1;
using SharpDX.XInput;

namespace PoncheToolkit.Tests.GameTests.Screens
{
    using PoncheToolkit.Graphics2D;

    public class TestScreen : GameScreen
    {
        private ThirdPersonCamera currentCamera;
        private FirstPersonCamera freeCamera;
        private Graphics3D.Primitives.Triangle triangle2;
        private PTMaterial materialMultiTexture;
        private PTMaterial materialColor;
        private PTMaterial materialMetal = null;
        private PTMaterial materialWood = null;
        private Square square;
        private Sphere sphere;
        private Cube cube;
        private PTModel duck;
        private PTModel nano;
        private PTModel skull;
        
        private Vector2 imagePosition;
        private Vector2 imageSize;

        private PTForwardRenderEffect basicEffect;
        private GameTime gameTime;

        //private PTTexture2D sprite;
        private Sprite sprite;
        private int duckCounter = 80;
        private int cubesCounter = 40;
        private List<PTModel> cubes;
        private List<PTModel> ducks;
        private AnimatedSprite siul;
        private Animation2DManager animationManager;

        // 2D effects
        private SharpDX.Direct2D1.Effect transformEffect;
        private SharpDX.Direct2D1.Effect rippleEffect;
        private PTRippleEffect ripple;

        private Dynamic2DLightManagerService lightManager2D;
        private Light2D light1;
        private Light2D light2;

        private TestGame game;
        private SharpDX.Direct2D1.DeviceContext context2D;

        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadRenderableComponentsHandler OnFinishLoadRenderableComponents;

        public TestScreen(Game11 game)
            : base(game)
        {
            this.game = game as TestGame;
            currentCamera = new ThirdPersonCamera(game);
            freeCamera = new FirstPersonCamera(game);

            ducks = new List<PTModel>();
            cubes = new List<PTModel>();
            gameTime = game.Services[typeof(GameTime)] as GameTime;

            OnInitialized += TestScreen_OnInitialized;
            OnFinishLoadContent += TestScreen_OnFinishLoadContent;
            OnFinishLoadRenderableComponents += TestScreen_OnFinishLoadRenderableComponents;
        }

        private void TestScreen_OnInitialized()
        {
            
        }

        public override void Initialize()
        {
            triangle2 = new Graphics3D.Primitives.Triangle(Game);
            square = new Square(Game);
            sphere = new Sphere(Game, 1f, 50, 50);
            cube = new Cube(Game);

            currentCamera.Position = new Vector3(0, 2f, -5f);
            currentCamera.Target = triangle2;

            freeCamera.Position = new Vector3(0, 0, -5f);

            triangle2.AcceptInput = false;
            triangle2.Size = new Vector3(5, 5, 5);
            triangle2.Position = new Vector3(0, 0, 3);

            square.AcceptInput = false;
            square.Size = new Vector3(2, 2, 2);
            square.Position = new Vector3(0, 0, 1);

            sphere.AcceptInput = false;
            sphere.Position = new Vector3(0, 2, 1);

            imagePosition = new Vector2(100, 100);
            imageSize = new Vector2(200, 200);

            Components.AddComponent(freeCamera, "Free camera");
            animationManager = Game.Services[typeof(Animation2DManager)] as Animation2DManager;

            Game.ToggleBlending(BlendingState.AlphaBlending);

            base.Initialize();
        }

        public override void LoadContent(IContentManager contentManager)
        {
            //List<Task<PTModel>> tasks = new List<Task<PTModel>>();
            //tasks.Add(Game.ContentManager.LoadModelAsync("Models/duck/duck.dae"));
            //tasks.Add(Game.ContentManager.LoadModelAsync("Models/Cranium/teschioassemblato.fbx"));
            //Task.WaitAll(tasks.ToArray());
            //duck = tasks[0].Result;
            //skull = tasks[1].Result;

            //lightManager2D = Game.Services[typeof(Dynamic2DLightManagerService)] as Dynamic2DLightManagerService;



            duck = Game.ContentManager.LoadModel("Models/duck/duck.dae");
            if (duck.Path.Contains("duck"))
                duck.Size = new Vector3(0.008f, 0.008f, 0.008f);
            duck.Position = new Vector3(2.5f, -1f, 0);

            // Cubes
            float positionY = 1;
            float positionX = -3;
            for (int i = 0; i < cubesCounter; i++)
            {
                PTModel c = new Cube(Game);
                c.Size = new Vector3(0.5f, 0.5f, 0.5f);
                if ((i + 1) % 12 == 0)
                    positionY -= 1f;
                float posX = positionX + ((i % 12) * (c.Size.X * 1.3f));
                c.Position = new Vector3(posX, positionY, -1);
                //cubes.Add(c);
            }

            //// Ducks
            //int currentRow = 1;
            //for (int i = 0; i < duckCounter; i++)
            //{
            //    if ((i + 1) % 10 == 0)
            //        currentRow--;
            //    PTModel mod = Game.ContentManager.LoadModel("Models/duck/duck.dae");
            //    mod.Position = new Vector3(-3.5f + ((i % 10) * 0.5f), currentRow, 0);
            //    mod.Size = new Vector3(0.003f, 0.003f, 0.003f);
            //    ducks.Add(mod);
            //}


            //nano = Game.ContentManager.LoadModel("Models/nano_suit/nanosuit.obj");
            //nano.Size = new Vector3(0.4f, 0.4f, 0.4f);
            //nano.Position = new Vector3(0, -3.5f, 0);

            //skull = Game.ContentManager.LoadModel("Models/Cranium/teschioassemblato.fbx");
            //skull.Size = new Vector3(2f, 2f, 2f);
            ////cranium.Rotation = new Vector3(180, cranium.Rotation.Y, cranium.Rotation.Z);
            //skull.Position = new Vector3(-1, -1f, 0);



            // ==== 2D elements
            context2D = Game.Renderer.SpriteBatch.CreateOfflineDeviceContext();

            //sprite = Game.ContentManager.LoadTexture2D("crate1.jpg");
            sprite = new Sprite(Game, "crate1.jpg");
            sprite.LoadContent(contentManager);

            //siul = animationManager.LoadSpriteSheet("siul", "Sprites/siul.png", new Animation2DFrameCount(4, 4), 5, context2D);
            siul = animationManager.LoadSpriteSheet("siul", "Sprites/siul.png", new Animation2DFrameCount(4, 4), 5);
            siul.Scale *= 0.7f;
            siul.Origin = new Vector2(32, 40);
            siul.Position = new Vector2(300, 300);
            
            animationManager.GenerateAnimation(siul, "walk_down", new Animation2DFrameRange(0, 0, 192, 0, 64, 80), 10);
            animationManager.GenerateAnimation(siul, "walk_left", new Animation2DFrameRange(0, 80, 192, 80, 64, 80), 10);
            animationManager.GenerateAnimation(siul, "walk_right", new Animation2DFrameRange(0, 160, 192, 160, 64, 80), 10);
            animationManager.GenerateAnimation(siul, "walk_up", new Animation2DFrameRange(0, 240, 192, 240, 64, 80), 10);

            siul.CurrentAnimation = siul.Animations["walk_right"];
            siul.Animations["walk_right"].Resume();
            siul.Animations["walk_down"].Resume();
            siul.Animations["walk_left"].Resume();
            siul.Animations["walk_up"].Resume();

            //lightManager2D.SizeShadowMap = Dynamic2DLightManagerService.ShadowMapSize.Size1024;
            //light1 = lightManager2D.CreateLight360(new Vector2(300, 200));
            //light2 = lightManager2D.CreateLight360(new Vector2(10, 10));

            transformEffect = new SharpDX.Direct2D1.Effect(Game.Renderer.Context2D, SharpDX.Direct2D1.Effect.AffineTransform2D);
            //rippleEffect = new SharpDX.Direct2D1.Effect(Game.Renderer.Context2D, PTCustomEffect.GUID_RippleEffect);
            ripple = new PTRippleEffect();
            ripple.Register(Game.Renderer.SpriteBatch.D2dFactory, Game.Renderer.Context2D);

            base.LoadContent(contentManager);
        }

        public override void LoadShadersAndMaterials(IContentManager contentManager)
        {
            basicEffect = new PTForwardRenderEffect(Game, null);
            basicEffect.LoadContent(contentManager);

            materialMultiTexture = new PTMaterial(Game, new PTTexturePath("Textures/bricks.gif", PTTexture2D.TextureType.Render, true), new PTTexturePath("Textures/grass.gif"));
            materialMultiTexture.AddTexturePath(new PTTexturePath("Textures/bricks_bump.gif", PTTexture2D.TextureType.BumpMap), true);
            //materialMultiTexture.AddTexturePath(new TexturePath("Textures/bricks_bump_created.jpg", PTTexture2D.TextureType.BumpMap), true);
            materialMultiTexture.AddTexturePath(new PTTexturePath("Textures/bricks_specular.jpg", PTTexture2D.TextureType.SpecularMap), true);
            materialMultiTexture.Name = "Multi texture material";
            //materialMultiTexture.DirectionalLight1.DiffuseColor = Vector4.One;
            //materialMultiTexture.DirectionalLight1.AmbientColor = new Vector4(0.2f, 0.2f, 0.2f, 1f);
            //materialMultiTexture.DirectionalLight1.LightDirection = new Vector3(-0.8f, -0.2f, 0.8f);
            //materialMultiTexture.DirectionalLight1.SpecularColor = new Vector4(0.85f, 0.95f, 0.85f, 1);
            materialMultiTexture.SpecularPower = 15f;
            materialMultiTexture.IsSpecularEnabled = true;
            materialMultiTexture.IsBumpEnabled = true;
            materialMultiTexture.LoadContent(contentManager);
            //basicEffect.AddMaterial("multitexture", materialMultiTexture);

            materialColor = new PTMaterial(Game);
            materialColor.Name = "Simple color material";
            //materialColor.DirectionalLight1.DiffuseColor = new Vector4(0.6f, 0.65f, 0.695f, 1f);
            //materialColor.DirectionalLight1.AmbientColor = new Vector4(0.2f, 0.2f, 0.2f, 1f);
            //materialColor.DirectionalLight1.LightDirection = new Vector3(-0.8f, 0f, 0.8f);
            //materialColor.DirectionalLight1.SpecularColor = Vector4.One;
            materialColor.SpecularPower = 35f;
            materialColor.IsSpecularEnabled = true;
            materialColor.IsBumpEnabled = false;
            materialColor.LoadContent(contentManager);
            //basicEffect.AddMaterial("simpleColor", materialColor);
        }

        private void TestScreen_OnFinishLoadRenderableComponents()
        {

        }

        public override void AddRenderableScreenComponents()
        {
            //AddRenderableComponentWithEffect(ref duck, ref basicEffect, "duck");
            //AddRenderableComponentWithEffect(ref square, ref basicEffect, "Test_Square");
            //AddRenderableComponentWithEffect(ref cube, ref basicEffect, "test_cube");
            //AddRenderableComponentWithEffect(ref triangle2, ref basicEffect, "Test_Triangle2");
            //AddRenderableComponentWithEffect(ref sphere, ref basicEffect, "sphere");

            //// Cubes
            //for (int i = 0; i < cubes.Count; i++)
            //{
            //    PTModel c = cubes[i];
            //    AddRenderableComponentWithEffect(ref c, ref basicEffect, "cube_model_" + i);
            //    c.SetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);
            //}

            //// Ducks
            //for (int i = 0; i < ducks.Count; i++)
            //{
            //    PTModel duckCopy = ducks[i];
            //    duckCopy.SetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);
            //    AddRenderableComponentWithEffect(ref duckCopy, ref basicEffect, "duck_model_" + i);
            //}

            OnFinishLoadRenderableComponents?.Invoke();
        }

        //public override void AddRenderableScreenComponents()
        //{
        //    List<Task> tasks = new List<Task>();
        //    tasks.Add(AddRenderableComponentWithEffectAsync(duck, basicEffect, "duck"));
        //    tasks.Add(AddRenderableComponentWithEffectAsync(square, basicEffect, "Test_Square"));
        //    tasks.Add(AddRenderableComponentWithEffectAsync(cube, basicEffect, "test_cube"));
        //    tasks.Add(AddRenderableComponentWithEffectAsync(triangle2, basicEffect, "Test_Triangle2"));
        //    tasks.Add(AddRenderableComponentWithEffectAsync(sphere, basicEffect, "sphere"));

        //    // Cubes
        //    for (int i = 0; i < cubes.Count; i++)
        //    {
        //        PTModel c = cubes[i];
        //        c.SetMaterial(PTMaterial.COMMON_METAL_MATERIAL_KEY);
        //        tasks.Add(AddRenderableComponentWithEffectAsync(c, basicEffect, "cube_model_" + i));
        //    }

        //    // Ducks
        //    //for (int i = 0; i < models.Count; i++)
        //    //{
        //    //    PTModel mod = models[i];
        //    //    mod.SetMaterial(PTMaterial.COMMON_METAL_MATERIAL_KEY);
        //    //    tasks.Add(AddRenderableComponentWithEffectAsync(mod, basicEffect, "duck_model_" + i));
        //    //}

        //    Task.WhenAll(tasks);

        //    OnFinishLoadRenderableComponents?.Invoke();
        //}

        /// <summary>
        /// When finished loading all the content in the screen.
        /// </summary>
        private void TestScreen_OnFinishLoadContent()
        {
            //materialWood = basicEffect.GetMaterial(PTMaterial.COMMON_WOOD_MATERIAL_KEY);
            ////foreach (PTMesh mesh in triangle2.Meshes)
            ////    materialWood.DirectionalLight1.DiffuseColor = new Vector4(0.467f, 0.55f, 0.58f, 1f);

            //materialMetal = basicEffect.GetMaterial(PTMaterial.COMMON_METAL_MATERIAL_KEY);

            sphere.SetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);
            duck.SetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);
        }

        private int directionX = 1;
        public override void UpdateLogic(GameTime gameTime)
        {
            base.UpdateLogic(gameTime);

            if (skull != null)
            {
                skull.Rotation += new Vector3(0, .4f, 0) * gameTime.DeltaTime;
                // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
                float yaw = MathUtil.DegreesToRadians(skull.Rotation.Y);
                float pitch = MathUtil.DegreesToRadians(skull.Rotation.X);
                float roll = MathUtil.DegreesToRadians(skull.Rotation.Z);

                // Create the rotation matrix from the yaw, pitch, and roll values.
                Matrix rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);
                skull.Rotation = Vector3.TransformCoordinate(skull.Rotation, rotationMatrix);
            }

            if (duck != null)
            {
                duck.Rotation += new Vector3(0, .7f, 0) * gameTime.DeltaTime;
                // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
                float yaw = MathUtil.DegreesToRadians(duck.Rotation.Y);
                float pitch = MathUtil.DegreesToRadians(duck.Rotation.X);
                float roll = MathUtil.DegreesToRadians(duck.Rotation.Z);

                // Create the rotation matrix from the yaw, pitch, and roll values.
                Matrix rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);
                duck.Rotation = Vector3.TransformCoordinate(duck.Rotation, rotationMatrix);
            }

            //if (materialWood == null)
            //    materialWood = basicEffect.GetMaterial(PTMaterial.COMMON_METAL_MATERIAL_KEY);
            //if (materialWood.DirectionalLight1.LightDirection.X >= 1)
            //    directionX = -1;
            //else if (materialWood.DirectionalLight1.LightDirection.X <= -1)
            //    directionX = 1;

            //float x = (materialWood.DirectionalLight1.LightDirection.X + (0.55f * directionX)) * gameTime.DeltaTime;
            //materialWood.DirectionalLight1.LightDirection = new Vector3(x, materialWood.DirectionalLight1.LightDirection.Y, materialWood.DirectionalLight1.LightDirection.Z);

            //Vector3 metalLight = materialMetal.DirectionalLight1.LightDirection;
            //metalLight.X = (float)Math.Sin(Game.GameTime.DeltaTime) * 2;
            //metalLight.Y = (float)Math.Cos(Game.GameTime.DeltaTime);
            //materialMetal.DirectionalLight1.LightDirection = metalLight;
        }

        private float spriteMovement = 500f;

        public override void UpdateInput(InputManager inputManager)
        {
            base.UpdateInput(inputManager);

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.Escape))
            {
                this.Game.Shutdown();
                return;
            }

            // Change FillMode - Solid or Wireframe
            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.F1))
                Game.Renderer.Rasterizer.FillMode = SharpDX.Direct3D11.FillMode.Solid;

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.F2))
                Game.Renderer.Rasterizer.FillMode = SharpDX.Direct3D11.FillMode.Wireframe;

            // Change Fullscreen
            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.F12))
                Game.Settings.Fullscreen = true;

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.F11))
                Game.Settings.Fullscreen = false;


            // Settings change
            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.PrintScreen))
                Game.Settings.LockFramerate = !Game.Settings.LockFramerate;

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.PageUp))
                ThreadingManager.CURRENT_RENDERING_THREADS_COUNT++;

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.PageDown))
                ThreadingManager.CURRENT_RENDERING_THREADS_COUNT--;


            //// ==== Update materials properties
            //if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.D0))
            //{
            //    materialMetal.SpecularPower += 0.22f;
            //    if (skull != null && skull.IsContentLoaded)
            //    {
            //        PTMaterial nuovo = skull.Effects.GetMaterial("nuovo");
            //        nuovo.SpecularPower = materialMetal.SpecularPower;
            //    }
            //}

            //if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.D9))
            //{
            //    materialMetal.SpecularPower -= 0.22f;
            //    if (skull != null && skull.IsContentLoaded)
            //    {
            //        PTMaterial nuovo = skull.Effects.GetMaterial("nuovo");
            //        nuovo.SpecularPower = materialMetal.SpecularPower;
            //    }
            //}

            //// Change materials.
            //if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D1))
            //{
            //    triangle2.SetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);
            //    duck.SetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);
            //}

            //if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D2))
            //{
            //    triangle2.SetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);
            //    duck.SetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);
            //}

            //if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D3))
            //{
            //    PTMaterial blinn = duck.Effects.GetMaterial("blinn3");
            //    blinn.IsSpecularEnabled = true;
            //    duck.SetMaterial(blinn.Name);

            //    if (skull != null && skull.IsContentLoaded)
            //    {
            //        PTMaterial nuovo = skull.Effects.GetMaterial("nuovo");
            //        nuovo.IsSpecularEnabled = true;
            //        skull.SetMaterial(nuovo.Name);
            //    }
            //}

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D4))
            {
                //triangle2.SetMaterial("simpleColor");
                triangle2.SetMaterial("multitexture");
                cube.SetMaterial("multitexture");
            }

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D5))
            {
                materialMultiTexture.IsBumpEnabled = !materialMultiTexture.IsBumpEnabled;
            }

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D6))
            {
                materialMultiTexture.HasSpecularMap = !materialMultiTexture.HasSpecularMap;
            }

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.RightShift))
            {
                game.Settings.Gamma += 0.001f;
                materialMultiTexture.Gamma = game.Settings.Gamma;
                materialColor.Gamma = game.Settings.Gamma;
                materialWood.Gamma = game.Settings.Gamma;
                materialMetal.Gamma = game.Settings.Gamma;

                materialMultiTexture.TextureTranslation += new Vector2(0.0006f, 0);
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.LeftShift))
            {
                game.Settings.Gamma -= 0.001f;
                materialMultiTexture.Gamma = game.Settings.Gamma;
                materialColor.Gamma = game.Settings.Gamma;
                materialWood.Gamma = game.Settings.Gamma;
                materialMetal.Gamma = game.Settings.Gamma;

                materialMultiTexture.TextureTranslation -= new Vector2(0.0006f, 0);
            }

            Gamepad? state = inputManager.GamepadState(SharpDX.XInput.UserIndex.One);
            if (state != null)
            {
                if (state.Value.Buttons == GamepadButtonFlags.A)
                {
                    cube.CalculateRenderingVectors();
                    triangle2.CalculateRenderingVectors();
                }
            }

            moveModel(duck, inputManager, 4f);

                
            // ====== 2D Updates.
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Right))
            {
                siul.CurrentAnimation = siul.Animations["walk_right"];
                imagePosition.X += spriteMovement * Game.GameTime.DeltaTime;
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Left))
            {
                siul.CurrentAnimation = siul.Animations["walk_left"];
                imagePosition.X -= spriteMovement * Game.GameTime.DeltaTime;
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Up))
            {
                siul.CurrentAnimation = siul.Animations["walk_up"];
                imagePosition.Y -= spriteMovement * Game.GameTime.DeltaTime;
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Down))
            {
                siul.CurrentAnimation = siul.Animations["walk_down"];
                imagePosition.Y += spriteMovement * Game.GameTime.DeltaTime;
            }

            siul.Position = imagePosition;

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.P))
                siul.CurrentAnimation.Pause();


            //light1.Position = inputManager.MousePosition;
        }


        private List<SharpDX.Direct2D1.Effect> effects;
        public override void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            //siul.Rotation += 0.5f;
            //siul.Rotation = siul.Rotation % 360;

            if (effects == null)
            {
                effects = new List<SharpDX.Direct2D1.Effect>();
                effects.Add(transformEffect);
                effects.Add(ripple.Effect);
            }

            RawMatrix3x2 transf;
            SharpDX.Direct2D1.D2D1.MakeRotateMatrix(0, Vector2.Zero, out transf);
            transf = Matrix3x2.Rotation(MathUtil.DegreesToRadians(0));
            transf = Matrix3x2.Multiply(transf, Matrix3x2.Translation(imagePosition));
            transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, transf);

            float delta = MathUtil.Clamp((Game.GameTime.GameTimeElapsed.Milliseconds / 1000), 0 , 10);
            ripple.Effect.SetValue((int)PTRippleEffectProperties.Frequency, 140.0f - delta * 30.0f);
            ripple.Effect.SetValue((int)PTRippleEffectProperties.Phase, -delta * 20.0f);
            ripple.Effect.SetValue((int)PTRippleEffectProperties.Amplitude, 60.0f - delta * 15.0f);
            ripple.Effect.SetValue((int)PTRippleEffectProperties.Spread, 0.01f + delta / 10.0f);
            ripple.Effect.SetValue((int)PTRippleEffectProperties.Center, (RawVector2)Game.InputManager.MousePosition);

            // Common draw
            spriteBatch.Begin();
            //spriteBatch.DrawTexture(sprite, imagePosition, imageSize, 0.4f);
            //spriteBatch.DrawTexture(sprite.Texture, effects, imagePosition, new RectangleF(imagePosition.X, imagePosition.Y, sprite.Size.X, sprite.Size.Y),
            //    Color.White, 0, Vector2.Zero, imageSize, 0.4f);
            animationManager.Draw(siul, spriteBatch, Game.GameTime);
            //spriteBatch.DrawTextLayout("Soy el texto de prueba --- sí que sí", new Vector2(10, 300), SpriteBatch.POZO_BOLD, 30, TextBrushes.Yellow);

            // TODO: tests to render to texture.
            //SharpDX.Direct2D1.Bitmap bit = new SharpDX.Direct2D1.Bitmap(Game.Renderer.Context2D, new Size2(1280, 720), new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)));
            //Game.Renderer.SetRenderTarget2D(bit);
            //Game.Renderer.Context2D.DrawBitmap(bit, 1, InterpolationMode.Anisotropic);
            //Game.Renderer.SetRenderTarget2D(null);
            //Game.Renderer.Context2D.DrawBitmap(bit, 1, InterpolationMode.Anisotropic);
            spriteBatch.End();


            //// Light shadows 2D
            //lightManager2D.Begin(spriteBatch, light1);
            //lightManager2D.DrawCasters(spriteBatch, new List<Sprite>() { siul });
            ////animationManager.Draw(siul, spriteBatch, Game.GameTime);
            ////spriteBatch.DrawTexture(sprite.Texture, imagePosition, new Vector2(0.2f, 0.2f), 0.4f);
            ////spriteBatch.DrawTexture(sprite.Texture, light1.ToRelativePosition(imagePosition), new Vector2(0.5f, 0.5f), 0.4f);
            //lightManager2D.End(spriteBatch, light1);


            //spriteBatch.Begin();
            //Game.Renderer.Context2D.DrawImage(lightManager2D.ShadowMapEffect.Target, new Vector2(0, 530));
            ////Game.Renderer.Context2D.DrawImage(lightManager2D.ShadowRenderEffect.Target, new Vector2(0, 0));
            ////Game.Renderer.Context2D.DrawBitmap(lightManager2D.ShadowMapEffect.Target, new RectangleF(100, 680, lightManager2D.ShadowMapEffect.Target.Size.Width, lightManager2D.ShadowMapEffect.Target.Size.Height),
            ////    1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            ////Game.Renderer.Context2D.DrawBitmap(lightManager2D.ShadowRenderEffect.Target, new RectangleF(600, 220, lightManager2D.ShadowRenderEffect.Target.Size.Width, lightManager2D.ShadowRenderEffect.Target.Size.Height),
            ////    1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            ////Game.Renderer.Context2D.DrawBitmap(lightManager2D.ShadowRenderEffect.Target, new RectangleF(0, 100, lightManager2D.ShadowRenderEffect.Target.Size.Width, lightManager2D.ShadowRenderEffect.Target.Size.Height),
            ////    1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            ////Game.Renderer.Context2D.DrawBitmap(lightManager2D.occludersRenderTarget, 1, SharpDX.Direct2D1.InterpolationMode.Linear);
            //spriteBatch.End();

            //Util.WICHelper.SaveTexture(Game.Renderer.Context2D.Device, lightManager2D.occludersRenderTarget, "occluders.png", new Rectangle(0, 0, (int)lightManager2D.occludersRenderTarget.Size.Width, (int)lightManager2D.occludersRenderTarget.Size.Height));
            //Util.WICHelper.SaveTexture(Game.Renderer.Context2D.Device, lightManager2D.ShadowMapEffect.Effect.Output, "shadowMap.png", new Rectangle(0, 0, (int)lightManager2D.ShadowMapEffect.Target.Size.Width, (int)lightManager2D.ShadowMapEffect.Target.Size.Height));
            //Util.WICHelper.SaveTexture(Game.Renderer.Context2D.Device, lightManager2D.ShadowRenderEffect.Target, "renderShadow.png", new Rectangle(0, 0, (int)lightManager2D.ShadowRenderEffect.Target.Size.Width, (int)lightManager2D.ShadowRenderEffect.Target.Size.Height));

            //spriteBatch.DrawPath();
        }

        private void moveModel(PTModel model, InputManager inputManager, float moveSpeed)
        {
            if (!model.AcceptInput)
                return;

            Vector3 position = model.Position;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Left))
                position.X -= moveSpeed * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Right))
                position.X += moveSpeed * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Down))
                position.Y -= moveSpeed * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Up))
                position.Y += moveSpeed * Game.GameTime.DeltaTime;

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.PageUp))
                model.Size += new Vector3(0.1f, 0.1f, 0.1f);

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.PageDown))
                model.Size -= new Vector3(0.1f, 0.1f, 0.1f);

            model.Position = position;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            Log.Debug("UpdateState - updating state in screen - " + this.Name);
            return true;
        }
    }
}