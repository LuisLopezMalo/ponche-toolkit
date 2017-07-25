using PoncheToolkit.Core.Management.Screen;
using PoncheToolkit.Graphics3D.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using PoncheToolkit.Core.Management.Input;
using SharpDX.XInput;

using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Graphics2D.Effects;
using PoncheToolkit.Util.Reflection;
using PoncheToolkit.Tests.Storage;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Graphics3D.Cameras;

namespace PoncheToolkit.Tests.GameTests.Screens
{
    using PoncheToolkit.Graphics2D;

    /// <summary>
    /// A screen testing different properties of the Engine.
    /// </summary>
    public class DemoScreen : GameScreen
    {
        #region Fields
        private Square wallLeft;
        private Square wallFront;
        private Square wallRight;
        private Square wallBack;
        private Square wallLeftParallel;

        private Cube cube;
        private Square floor;
        private Sphere sphere;

        //private PTMaterial wallBricksMaterial;
        //private PTMaterial wallRocksMaterial;
        //private PTMaterial reflectiveFloorMaterial;
        //private PTMaterial colorMaterial;
        //private PTMaterial crateMaterial;
        //private PTMaterial spotLightMaterial;

        private PTMaterial wallBricksMaterial;
        private PTMaterial wallRocksMaterial;
        private PTMaterial wallRocksMaterialNoMipMap;
        private PTMaterial reflectiveFloorMaterial;
        private PTMaterial crateMaterial;

        private PTModel duckModel;
        private PTModel nanoSuitModel;
        private PTModel skullModel;
        private PTRenderTarget2D reflectiveRenderTarget;

        private PTForwardRenderEffect basicEffect;
        private PTForwardRenderEffect basicEffect2;
        private PTToonEffect toonEffect;
        private PTLinkingForwardRenderEffect linkingEffect;
        private PTEffect currentEffect;
        private FreeCamera freeCamera;

        private List<Square> squares;
        private int squareCount = 16;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public DemoScreen(Game11 game)
            : base(game)
        {
        }

        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadRenderableComponentsHandler OnFinishLoadRenderableComponents;

        /// <inheritdoc/>
        public override void Initialize()
        {
            freeCamera = new FreeCamera(Game);
            freeCamera.Position = new Vector3(0, 1, -2f);
            freeCamera.FastMovement = true;
            Components.AddComponent(freeCamera, "Free camera");

            Vector3 size = new Vector3(12);
            wallLeft = new Square(Game);
            wallFront = new Square(Game);
            wallLeftParallel = new Square(Game);
            wallRight = new Square(Game);
            wallBack = new Square(Game);
            floor = new Square(Game);
            cube = new Cube(Game);

            squares = new List<Square>();
            for (int i = 0; i < squareCount; i++)
            {
                squares.Add(new Square(Game));
            }
        
            sphere = new Sphere(Game);
            wallLeft.Size = size;
            wallLeft.UVRepeatFactor = 2;
            wallFront.Size = size;
            wallFront.UVRepeatFactor = 2;
            wallLeftParallel.Size = size * 2;
            wallLeftParallel.UVRepeatFactor = 3;
            wallRight.Size = size;
            wallRight.UVRepeatFactor = 2;
            wallBack.Size = size;
            wallBack.UVRepeatFactor = 2;
            floor.Size = new Vector3(38);
            cube.Size = new Vector3(1);

            float separation = 6;
            wallLeft.Position = new Vector3(-separation, 0, 0);
            wallLeft.Rotation = new Vector3(0, MathUtil.DegreesToRadians(-90), 0);
            wallLeftParallel.Position = new Vector3(-separation * 2f, 0, 0);
            wallLeftParallel.Rotation = new Vector3(0, MathUtil.DegreesToRadians(-90), 0);
            wallFront.Position = new Vector3(0, 0, separation);
            wallRight.Position = new Vector3(separation, 0, 0);
            wallRight.Rotation = new Vector3(0, MathUtil.DegreesToRadians(90), 0);
            wallBack.Position = new Vector3(0, 0, -separation);
            wallBack.Rotation = new Vector3(0, MathUtil.DegreesToRadians(180), 0);
            floor.Position = new Vector3(0, -floor.Size.Y / 4 - (size.Y / 2), -2);
            floor.Rotation = new Vector3(MathUtil.DegreesToRadians(90), 0, 0);

            cube.Position = new Vector3(-1, 0.6f, 0);

            for (int i = 0; i < squareCount; i++)
            {
                squares[i].Size = new Vector3(8);
                squares[i].Position = new Vector3(0, -floor.Size.Y / 2 - (size.Y / 2), 0);
                squares[i].Rotation = new Vector3(MathUtil.DegreesToRadians(90), 0, 0);
            }

            wallLeft.UVRepeatFactor = 2;
            wallFront.UVRepeatFactor = 1;
            wallRight.UVRepeatFactor = 4;
            wallBack.UVRepeatFactor = 4;

            reflectiveRenderTarget = new PTRenderTarget2D(Game.Renderer, "ReflectiveRT");

            Game.ToggleBlending(BlendingState.AlphaBlending);

            base.Initialize();

            Game.Renderer.OnEndRender += Renderer_OnEndRender;
        }


        private bool saveFinalImage;
        private void Renderer_OnEndRender()
        {
            if (saveFinalImage)
            {
                saveFinalImage = false;
                Util.WICHelper.SaveTexture(Game.Renderer.Context2D.Device, Game.BackBufferRenderTarget, "backBufferAfterPostProcess.jpg",
                    new Rectangle(0, 0, (int)Game.Renderer.PostProcessRenderTarget.Bitmap.Size.Width, (int)Game.Renderer.PostProcessRenderTarget.Bitmap.Size.Height));
            }
        }

        public override void LoadContent(IContentManager contentManager)
        {
            duckModel = contentManager.LoadModel("Models/duck/duck.dae");
            duckModel.Size = new Vector3(0.008f);
            duckModel.Position = new Vector3(1.5f, 0, 2);

            nanoSuitModel = contentManager.LoadModel("Models/nano_suit/nanosuit.obj");
            nanoSuitModel.Size = new Vector3(0.3f);
            nanoSuitModel.Rotation = new Vector3(0, 180, 0);
            nanoSuitModel.Position = new Vector3(2f, 0, 2);

            //skullModel = contentManager.LoadModel("Models/Cranium/teschioassemblato.fbx");
            //skullModel.Size = new Vector3(1.8f);
            ////skullModel.Rotation = new Vector3(110, 20, 20);
            //skullModel.Rotation = new Vector3(0, 20, 0);
            //skullModel.Position = new Vector3(1f, 1, 1);

            base.LoadContent(contentManager);
        }

        PTLight pointLight;
        PTLight torchLight;
        PTLight directionalLight;
        PTLight spotLight;
        PTLight spotLight2;
        PTLight spotLight3;
        public override void LoadShadersAndMaterials(IContentManager contentManager)
        {
            PTLight.FORWARD_SHADING_MAX_LIGHTS = 16;

            //linkingEffect = new PTLinkingForwardRenderEffect(Game, -1);
            //linkingEffect.LoadContent(contentManager);
            //linkingEffect.GlobalAmbientColor = new Vector4(0.3f, 0.3f, 0.3f, 1);
            //ToDispose(linkingEffect);

            basicEffect = new PTForwardRenderEffect(Game, "Effects/Dynamic/PTVS.fx", "Effects/Dynamic/PTPS.fx", new List<string>() { "Effects", "Effects/Dynamic" });
            basicEffect.LoadContent(contentManager);
            //basicEffect.GlobalAmbientColor = new Vector4(0.1f, 0.1f, 0.1f, 1);
            basicEffect.GlobalAmbientColor = new Vector4(0.3f, 0.3f, 0.3f, 1);
            ToDispose(basicEffect);

            toonEffect = new PTToonEffect(Game, "Effects/Dynamic/PTVS.fx", "Effects/Dynamic/PTPS.fx", new List<string>() { "Effects", "Effects/Dynamic" }, 0);
            toonEffect.LoadContent(contentManager);
            //basicEffect.GlobalAmbientColor = new Vector4(0.1f, 0.1f, 0.1f, 1);
            toonEffect.GlobalAmbientColor = new Vector4(0.4f, 0.4f, 0.8f, 1);
            ToDispose(toonEffect);

            // Set the current effect used.
            currentEffect = basicEffect;


            directionalLight = new PTLight();
            directionalLight.Color = new Vector4(0.5f, 0.5f, 0.9f, 1);
            directionalLight.Direction = new Vector3(0.9f, -0.2f, 0.1f);
            directionalLight.Intensity = 1f;
            directionalLight.Type = LightType.Directional;
            directionalLight.IsEnabled = true;
            //directionalLight.LoadDebugContent(Game);

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

            torchLight = new PTLightTorch();
            torchLight.Position = new Vector3(0f, -2, 0);
            torchLight.Color = new Vector4(1f, 0.5f, 0.2f, 1);
            torchLight.Type = LightType.Point;
            torchLight.ConstantAttenuation = 1f;
            torchLight.LinearAttenuation = 0f;
            //torchLight.LinearAttenuation = 0.8f;
            //torchLight.QuadraticAttenuation = 1.2f;
            torchLight.Range = 40;
            torchLight.IsEnabled = true;
            torchLight.LoadDebugContent(Game);

            spotLight = new PTLight();
            spotLight.Color = new Vector4(0.1f, 0.1f, 1f, 1);
            spotLight.Type = LightType.Spot;
            spotLight.SpotAngle = 100;
            spotLight.ConstantAttenuation = 0.9f;
            spotLight.LinearAttenuation = 0.1f;
            spotLight.QuadraticAttenuation = 0f;
            spotLight.Position = new Vector3(3f, 1, 0);
            spotLight.IsEnabled = true;
            spotLight.Range = 40;
            spotLight.LoadDebugContent(Game);

            spotLight2 = new PTLight();
            spotLight2.Position = new Vector3(-2, 2, 0);
            spotLight2.Color = new Vector4(0.1f, 1f, 0.1f, 1);
            spotLight2.Type = LightType.Spot;
            spotLight2.SpotAngle = 100;
            spotLight2.ConstantAttenuation = 0.6f;
            //spotLight2.LinearAttenuation = 0.1f;
            //spotLight2.QuadraticAttenuation = 0f;
            spotLight2.LinearAttenuation = 0.2f;
            spotLight2.QuadraticAttenuation = 0.08f;
            spotLight2.Range = 20;
            spotLight2.IsEnabled = true;
            spotLight2.LoadDebugContent(Game);

            spotLight3 = new PTLight();
            spotLight3.Position = new Vector3(-2, -2, 0);
            spotLight3.Color = new Vector4(1f, 1f, 1f, 1);
            spotLight3.Type = LightType.Spot;
            spotLight3.SpotAngle = 100;
            spotLight3.ConstantAttenuation = 1f;
            spotLight3.LinearAttenuation = 0.5f;
            spotLight3.QuadraticAttenuation = 0.8f;
            spotLight3.Range = 10;
            spotLight3.IsEnabled = true;
            spotLight3.LoadDebugContent(Game);

            if (basicEffect != null)
            {
                //basicEffect.AddLight(directionalLight);
                //basicEffect.AddLight(pointLight);
                basicEffect.AddLight(torchLight);
                basicEffect.AddLight(spotLight);

                //for (int i = 0; i < 3; i++)
                //{
                //    PTLight li = new PTLight();
                //    li.Position = new Vector3(i * 0.5f, -2, 0);
                //    li.Color = new Vector4(0.2f, 0.5f, 1f, 1);
                //    li.Type = LightType.Point;
                //    li.ConstantAttenuation = 1f;
                //    li.LinearAttenuation = 0.0008f;
                //    li.QuadraticAttenuation = 1.8f;
                //    li.Intensity = 0.4f;
                //    li.IsEnabled = true;
                //    if (i == 0)
                //        li.LoadDebugContent(Game);
                //    basicEffect.AddLight(li);
                //}
                //basicEffect.AddLight(spotLight2);
                //basicEffect.AddLight(spotLight3);
            }

            if (toonEffect != null)
            {
                toonEffect.AddLight(torchLight);
                toonEffect.AddLight(spotLight2);
            }


            //Vector4 diffuse = new Vector4(0.75f, 0.75f, 0.75f, 1f);
            //Vector4 ambient = new Vector4(0.4f, 0.4f, 0.4f, 1f);
            Vector4 diffuse = new Vector4(0.21f, 0.21f, 0.21f, 1f);
            Vector4 ambient = new Vector4(0.41f, 0.41f, 0.41f, 1f);

            #region Materials
            wallBricksMaterial = new PTMaterial(Game, new Graphics3D.PTTexturePath("Textures/bricks.gif", true));
            //wallBricksMaterial = new PTMaterial(Game, new Graphics3D.TexturePath("Textures/bricks.gif"));
            wallBricksMaterial.AddTexturePath(new Graphics3D.PTTexturePath("Textures/bricks_bump.gif", Graphics2D.PTTexture2D.TextureType.BumpMap), true);
            wallBricksMaterial.IsSpecularEnabled = true;
            wallBricksMaterial.IsBumpEnabled = true;
            wallBricksMaterial.EmissiveColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            //wallBricksMaterial.EmissiveColor = new Vector4(0.3f, 0.3f, 0.3f, 1f);
            wallBricksMaterial.DiffuseColor = diffuse;
            wallBricksMaterial.AmbientColor = ambient;
            wallBricksMaterial.SpecularColor = new Vector4(1f, 1f, 1f, 1f);
            wallBricksMaterial.SpecularPower = 10;
            wallBricksMaterial.LoadContent(contentManager);

            //wallRocksMaterial = new PTMaterial(Game, new Graphics3D.TexturePath("Textures/wall_red.jpg", true));
            wallRocksMaterial = new PTMaterial(Game, new Graphics3D.PTTexturePath("Textures/wall_red.jpg", true), new Graphics3D.PTTexturePath("Textures/grass.jpg", true));
            wallRocksMaterial.AddTexturePath(new Graphics3D.PTTexturePath("Textures/wall_red_bump.jpg", Graphics2D.PTTexture2D.TextureType.BumpMap), true);
            //wallRocksMaterial.AddTexturePath(new Graphics3D.TexturePath("Textures/wall_red_specular.jpg", Graphics2D.PTTexture2D.TextureType.SpecularMap), true);
            //wallRocksMaterial = new PTMaterial(Game);
            wallRocksMaterial.SpecularPower = 50f;
            wallRocksMaterial.EmissiveColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            //wallRocksMaterial.EmissiveColor = new Vector4(0.3f, 0.3f, 0.3f, 1f);
            //wallRocksMaterial.AmbientColor = ambient;
            wallRocksMaterial.AmbientColor = new Vector4(0.41f, 0.41f, 0.41f, 1f);
            wallRocksMaterial.DiffuseColor = Vector4.One; //new Vector4(0.51f, 0.51f, 0.51f, 1f);
            wallRocksMaterial.SpecularColor = new Vector4(1f, 0.2f, 0.2f, 1f);
            wallRocksMaterial.IsSpecularEnabled = true;
            wallRocksMaterial.IsBumpEnabled = true;
            wallRocksMaterial.LoadContent(contentManager);

            wallRocksMaterialNoMipMap = new PTMaterial(Game, new Graphics3D.PTTexturePath("Textures/wall_red.jpg", false));
            wallRocksMaterialNoMipMap.AddTexturePath(new Graphics3D.PTTexturePath("Textures/wall_red_bump.jpg", Graphics2D.PTTexture2D.TextureType.BumpMap), true);
            wallRocksMaterialNoMipMap.SpecularPower = 50f;
            wallRocksMaterialNoMipMap.EmissiveColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            wallRocksMaterialNoMipMap.AmbientColor = ambient;
            wallRocksMaterialNoMipMap.DiffuseColor = new Vector4(0.51f, 0.51f, 0.51f, 1f);
            wallRocksMaterialNoMipMap.SpecularColor = new Vector4(0.8f, 0.2f, 0.2f, 1f);
            wallRocksMaterialNoMipMap.IsSpecularEnabled = true;
            wallRocksMaterialNoMipMap.IsBumpEnabled = true;
            wallRocksMaterialNoMipMap.LoadContent(contentManager);

            reflectiveFloorMaterial = new PTMaterial(Game, new Graphics3D.PTTexturePath("Textures/reflective_floor.png", true));
            //reflectiveFloorMaterial = new PTMaterial(Game, new Graphics3D.TexturePath("Textures/reflective_floor.png"));
            reflectiveFloorMaterial.AddTexturePath(new Graphics3D.PTTexturePath("Textures/reflective_floor_bump.jpg", Graphics2D.PTTexture2D.TextureType.BumpMap), true);
            //reflectiveFloorMaterial.PointLight1 = point;
            reflectiveFloorMaterial.IsSpecularEnabled = true;
            reflectiveFloorMaterial.IsBumpEnabled = true;
            reflectiveFloorMaterial.IsReflectivityEnabled = true;
            reflectiveFloorMaterial.EmissiveColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            //reflectiveFloorMaterial.EmissiveColor = new Vector4(0.3f, 0.3f, 0.3f, 0f);
            reflectiveFloorMaterial.AmbientColor = ambient;
            reflectiveFloorMaterial.DiffuseColor = new Vector4(0.51f, 0.51f, 0.51f, 1f);
            reflectiveFloorMaterial.SpecularColor = new Vector4(1f, 1f, 1f, 1f);
            reflectiveFloorMaterial.LoadContent(contentManager);
            reflectiveFloorMaterial.UpdateState();

            reflectiveRenderTarget.Initialize();
            reflectiveRenderTarget.Texture = reflectiveFloorMaterial.TextureByType(PTTexture2D.TextureType.Reflective);


            crateMaterial = new PTMaterial(Game, new Graphics3D.PTTexturePath("crate1.jpg", true), new Graphics3D.PTTexturePath("Textures/grass.jpg", true));
            crateMaterial.AddTexturePath(new Graphics3D.PTTexturePath("Textures/crate1_bump.jpg", Graphics2D.PTTexture2D.TextureType.BumpMap), true);
            crateMaterial.SpecularPower = 5f;
            crateMaterial.IsSpecularEnabled = true;
            crateMaterial.IsBumpEnabled = true;
            crateMaterial.AmbientColor = ambient;
            crateMaterial.EmissiveColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            crateMaterial.DiffuseColor = new Vector4(0.51f, 0.51f, 0.51f, 1f);
            crateMaterial.SpecularColor = new Vector4(1f, 0.2f, 0.2f, 1f);
            crateMaterial.Opacity = 1f;
            crateMaterial.LoadContent(contentManager);

            Game.Renderer.ImmediateContext.GenerateMips(crateMaterial.TextureByType(PTTexture2D.TextureType.Render).ShaderResourceView);

            #endregion

            if (toonEffect != null)
            {
                toonEffect.AddMaterial("wallMaterial", wallBricksMaterial);
                toonEffect.AddMaterial("wallRocksMaterial", wallRocksMaterial);
                toonEffect.AddMaterial("wallRocksMaterialNoMipMap", wallRocksMaterialNoMipMap);
                toonEffect.AddMaterial("reflectiveFloorMaterial", reflectiveFloorMaterial);
                toonEffect.AddMaterial("crateMaterial", crateMaterial);
            }

            if (basicEffect != null)
            {
                basicEffect.AddMaterial("wallMaterial", wallBricksMaterial);
                basicEffect.AddMaterial("wallRocksMaterial", wallRocksMaterial);
                basicEffect.AddMaterial("wallRocksMaterialNoMipMap", wallRocksMaterialNoMipMap);
                basicEffect.AddMaterial("reflectiveFloorMaterial", reflectiveFloorMaterial);
                basicEffect.AddMaterial("crateMaterial", crateMaterial);
            }
        }

        public override void AddRenderableScreenComponents()
        {
            //AddRenderableComponentWithEffect(ref wallLeft, ref basicEffect, "wall1");
            //AddRenderableComponentWithEffect(ref wallLeftParallel, ref basicEffect, "wallLeftParallel");
            //AddRenderableComponentWithEffect(ref wallFront, ref basicEffect, "wall2");
            //AddRenderableComponentWithEffect(ref wallRight, ref basicEffect, "wall3");
            //AddRenderableComponentWithEffect(ref wallBack, ref basicEffect, "wall4");
            //AddRenderableComponentWithEffect(ref floor, ref basicEffect, "floor");
            //AddRenderableComponentWithEffect(ref cube, ref basicEffect, "cube");
            ////AddRenderableComponentWithEffect(ref sphere, ref basicEffect, "sphere");
            //AddRenderableComponentWithEffect(ref duckModel, ref basicEffect, "duck");

            //PTModel model1 = directionalLight.DebugModel;
            //PTModel model2 = pointLight.DebugModel;
            //PTModel modelTorchLight = torchLight.DebugModel;
            //PTModel model3 = spotLight.DebugModel;
            //PTModel model4 = spotLight2.DebugModel;
            //PTModel model5 = spotLight3.DebugModel;
            //AddRenderableComponentWithEffect(ref model1, ref basicEffect, "lightModel1");
            //AddRenderableComponentWithEffect(ref model2, ref basicEffect, "lightModel2");
            //AddRenderableComponentWithEffect(ref modelTorchLight, ref basicEffect, "modelTorchLight");
            //AddRenderableComponentWithEffect(ref model3, ref basicEffect, "lightModel3");
            //AddRenderableComponentWithEffect(ref model4, ref basicEffect, "lightModel4");
            //AddRenderableComponentWithEffect(ref model5, ref basicEffect, "lightModel5");

            //for (int i = 0; i < squareCount; i++)
            //{
            //    Square sq = squares[i];
            //    AddRenderableComponentWithEffect(ref sq, ref basicEffect, "floor" + (i + 1));
            //}


            //List<PTForwardRenderEffect> basicEffectList = new List<PTForwardRenderEffect>() { basicEffect };
            //List<PTEffect> basicEffectList = new List<PTEffect>() { currentEffect };
            //List<PTEffect> basicEffectList = new List<PTEffect>() { linkingEffect };

            //List<PTEffect> basicEffectList = new List<PTEffect>() { basicEffect, toonEffect };
            List<PTEffect> basicEffectList2 = new List<PTEffect>() { toonEffect };
            List<PTEffect> basicEffectList = new List<PTEffect>() { basicEffect };
            AddRenderableComponentWithEffect(ref wallLeft, basicEffectList, "wall1");
            AddRenderableComponentWithEffect(ref wallLeftParallel, basicEffectList, "wallLeftParallel");
            AddRenderableComponentWithEffect(ref wallFront, basicEffectList, "wall2");
            AddRenderableComponentWithEffect(ref wallRight, basicEffectList, "wall3");
            AddRenderableComponentWithEffect(ref wallBack, basicEffectList, "wall4");
            AddRenderableComponentWithEffect(ref floor, basicEffectList, "floor");
            AddRenderableComponentWithEffect(ref cube, basicEffectList, "cube");
            //AddRenderableComponentWithEffect(ref duckModel, basicEffectList2, "duck");
            //AddRenderableComponentWithEffect(ref nanoSuitModel, basicEffectList2, "nanosuit");

            //AddRenderableComponentWithEffect(ref skullModel, basicEffectList2, "skull");
            //AddRenderableComponentWithEffect(ref sphere, ref basicEffect, "sphere");


            //PTModel model1 = directionalLight.DebugModel;
            PTModel model2 = pointLight.DebugModel;
            PTModel modelTorchLight = torchLight.DebugModel;
            PTModel model3 = spotLight.DebugModel;
            PTModel model4 = spotLight2.DebugModel;
            PTModel model5 = spotLight3.DebugModel;

            //AddRenderableComponentWithEffect(ref model1, basicEffectList, "lightModel1");
            AddRenderableComponentWithEffect(ref model2, basicEffectList, "lightModel2");
            AddRenderableComponentWithEffect(ref modelTorchLight, basicEffectList, "modelTorchLight");
            AddRenderableComponentWithEffect(ref model3, basicEffectList, "lightModel3");
            AddRenderableComponentWithEffect(ref model4, basicEffectList, "lightModel4");
            AddRenderableComponentWithEffect(ref model5, basicEffectList, "lightModel5");




            // ========== Instanced
            //AddRenderableInstancedComponentWithEffect(ref wall1, ref basicEffect, "wall1", 0);
            //AddRenderableInstancedComponentWithEffect(ref wall2, ref basicEffect, "wall2", 0);
            //AddRenderableInstancedComponentWithEffect(ref wall3, ref basicEffect, "wall3", 0);
            //AddRenderableInstancedComponentWithEffect(ref wall4, ref basicEffect, "wall4", 0);
            //AddRenderableInstancedComponentWithEffect(ref floor, ref basicEffect, "floor", 1);
            //AddRenderableInstancedComponentWithEffect(ref cube, ref basicEffect, "cube", 2);
            ////AddRenderableComponentWithEffect(ref sphere, ref basicEffect, "sphere");
            ////AddRenderableComponentWithEffect(ref duckModel, ref basicEffect, "duck");


            //List<Task> tasks = new List<Task>();

            //tasks.Add(AddRenderableComponentWithEffectAsync(wall1, basicEffect, "wall1"));
            //tasks.Add(AddRenderableComponentWithEffectAsync(wall2, basicEffect, "wall2"));
            //tasks.Add(AddRenderableComponentWithEffectAsync(wall3, basicEffect, "wall3"));
            //tasks.Add(AddRenderableComponentWithEffectAsync(wall4, basicEffect, "wall4"));
            //tasks.Add(AddRenderableComponentWithEffectAsync(floor, basicEffect, "floor"));
            //tasks.Add(AddRenderableComponentWithEffectAsync(cube, basicEffect, "cube"));
            ////tasks.Add(AddRenderableComponentWithEffectAsync(sphere, basicEffect, "sphere"));
            //tasks.Add(AddRenderableComponentWithEffectAsync(duckModel, basicEffect, "duck"));

            //await Task.WhenAll(tasks);



            wallLeft.SetMaterial(wallRocksMaterial);
            wallLeftParallel.SetMaterial(wallRocksMaterialNoMipMap);
            wallFront.SetMaterial(wallRocksMaterial);
            wallRight.SetMaterial("wallMaterial");
            wallBack.SetMaterial("wallMaterial");
            floor.SetMaterial(reflectiveFloorMaterial);
            //duckModel.SetMaterial(wallRocksMaterial);
            cube.SetMaterial(crateMaterial);
            sphere.SetMaterial(PTMaterial.DEFAULT_COLOR_MATERIAL_KEY);

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

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.F9))
            {
                if (currentEffect == toonEffect)
                    currentEffect = basicEffect;
                else
                    currentEffect = toonEffect;
            }

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D1) ||
                inputManager.IsButtonReleased(UserIndex.Any, GamepadButtonFlags.A))
            {
                foreach (KeyValuePair<string, PTMaterial> mat in currentEffect.Materials.Values)
                    mat.Value.IsBumpEnabled = !mat.Value.IsBumpEnabled;
            }
            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D2))
            {
                foreach (KeyValuePair<string, PTMaterial> mat in currentEffect.Materials.Values)
                    mat.Value.IsSpecularEnabled = !mat.Value.IsSpecularEnabled;
            }

            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.D0) ||
                inputManager.IsButtonReleased(UserIndex.Any, GamepadButtonFlags.RightThumb))
                Game.Renderer.Rasterizer.FillMode = Game.Renderer.Rasterizer.FillMode == SharpDX.Direct3D11.FillMode.Solid ? SharpDX.Direct3D11.FillMode.Wireframe : SharpDX.Direct3D11.FillMode.Solid;

            // Commands - Shift - Ctrl
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.LeftShift))
            {
                wallRocksMaterial.TextureTranslation -= new Vector2(0.005f, 0);
                //foreach (PTMaterial mat in nanoSuitModel.ImportedMaterials)
                //    mat.TextureTranslation -= new Vector2(0.005f, 0);
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.RightShift))
            {
                wallRocksMaterial.TextureTranslation += new Vector2(0.005f, 0);
                //foreach (PTMaterial mat in nanoSuitModel.ImportedMaterials)
                //    mat.TextureTranslation += new Vector2(0.005f, 0);
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.LeftControl))
            {
                foreach (PTLight light in currentEffect.Lights)
                    light.Intensity = Math.Max(0, light.Intensity - 0.05f);
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.RightControl))
            {
                foreach (PTLight light in currentEffect.Lights)
                    light.Intensity += 0.05f;
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Right))
            {
                foreach (PTLight light in currentEffect.Lights)
                    light.Range += 0.05f;
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Left))
            {
                foreach (PTLight light in currentEffect.Lights)
                    light.Range = Math.Max(0, light.Range - 0.05f);
            }

            // === Lighting properties
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.PageDown)
                || inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.LeftShoulder))
            {
                foreach (KeyValuePair<string, PTMaterial> mat in currentEffect.Materials.Values)
                {
                    if (!mat.Value.IsReflectivityEnabled)
                        mat.Value.SpecularPower -= 0.07f;
                    mat.Value.SpecularPower = Math.Max(1f, mat.Value.SpecularPower);

                    if (mat.Value.IsReflectivityEnabled)
                        mat.Value.Reflectivity -= 0.005f;
                }
            }
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.PageUp)
                || inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.RightShoulder))
            {
                foreach (KeyValuePair<string, PTMaterial> mat in currentEffect.Materials.Values)
                {
                    if (!mat.Value.IsReflectivityEnabled)
                        mat.Value.SpecularPower += 0.07f;

                    if (mat.Value.IsReflectivityEnabled)
                        mat.Value.Reflectivity += 0.005f;
                }
            }

            // Render to texture.
            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.R))
                renderReflection = true;


            // Move light
            Vector3 pointLightPosition = currentEffect.Lights.Last().Position;
            //if (inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.DPadLeft))
            //    pointLightPosition.X -= 0.005f;
            //if (inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.DPadRight))
            //    pointLightPosition.X += 0.005f;
            //if (inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.DPadUp))
            //    pointLightPosition.Z += 0.005f;
            //if (inputManager.IsButtonHold(UserIndex.Any, GamepadButtonFlags.DPadDown))
            //    pointLightPosition.Z -= 0.005f;


            Vector3 positionDelta = new Vector3(inputManager.MouseDelta.X * 0.03f, inputManager.MouseDelta.Y * 0.03f, 0);
            //pointLightPosition = new Vector3(pointLightPosition.X + inputManager.MouseDelta.X, pointLightPosition.Y + inputManager.MouseDelta.Y, pointLightPosition.Z);

            if (inputManager.IsMouseHold(InputManager.MouseButton.Left))
            {
                //basicEffect.GlobalAmbientColor = new Vector4(Math.Min(1, basicEffect.GlobalAmbientColor.X + 0.01f), basicEffect.GlobalAmbientColor.Y, basicEffect.GlobalAmbientColor.Z, 1);
                positionDelta.Z += 0.05f;
            }
            if (inputManager.IsMouseHold(InputManager.MouseButton.Right))
            {
                //basicEffect.GlobalAmbientColor = new Vector4(Math.Max(0, basicEffect.GlobalAmbientColor.X - 0.01f), basicEffect.GlobalAmbientColor.Y, basicEffect.GlobalAmbientColor.Z, 1);
                positionDelta.Z -= 0.05f;
            }

            if (inputManager.IsMouseHold(InputManager.MouseButton.Side2))
                pointLight.Range = Math.Min(20, pointLight.Range + 0.05f);
            if (inputManager.IsMouseHold(InputManager.MouseButton.Side1))
                pointLight.Range = Math.Max(0.5f, pointLight.Range - 0.05f);

            positionDelta.Y *= -1; 
            foreach (PTLight light in currentEffect.Lights)
            {
                light.Position += positionDelta;
            }



            // PostProcess
            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Up))
            {
                if (Game.Renderer.PostProcessEffects.Count > 0)
                {
                    if (Game.Renderer.PostProcessEffects.Values[0] is PTEdgeDetectionEffect)
                    {
                        PTEdgeDetectionEffect eff = (Game.Renderer.PostProcessEffects.Values[0] as PTEdgeDetectionEffect);
                        eff.Thickness += 0.01f;
                    }
                }
            }

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Down))
            {
                if (Game.Renderer.PostProcessEffects.Count > 0)
                {
                    if (Game.Renderer.PostProcessEffects.Values[0] is PTEdgeDetectionEffect)
                    {
                        PTEdgeDetectionEffect eff = (Game.Renderer.PostProcessEffects.Values[0] as PTEdgeDetectionEffect);
                        eff.Thickness -= 0.01f;
                    }
                }
            }

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Right))
            {
                if (Game.Renderer.PostProcessEffects.Count > 0)
                {
                    if (Game.Renderer.PostProcessEffects.Values[0] is PTEdgeDetectionEffect)
                    {
                        PTEdgeDetectionEffect eff = (Game.Renderer.PostProcessEffects.Values[0] as PTEdgeDetectionEffect);
                        eff.Threshold += 0.001f;
                    }
                }
            }

            if (inputManager.IsKeyHold(SharpDX.DirectInput.Key.Left))
            {
                if (Game.Renderer.PostProcessEffects.Count > 0)
                {
                    if (Game.Renderer.PostProcessEffects.Values[0] is PTEdgeDetectionEffect)
                    {
                        PTEdgeDetectionEffect eff = (Game.Renderer.PostProcessEffects.Values[0] as PTEdgeDetectionEffect);
                        eff.Threshold -= 0.001f;
                    }
                }
            }

            // Save textures
            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.P))
            {
                saveFinalImage = true;
                Util.WICHelper.SaveTexture(Game.Renderer.Context2D.Device, Game.Renderer.PostProcessRenderTarget, "backBufferBeforPostProcess.jpg",
                    new Rectangle(0, 0, (int)Game.Renderer.PostProcessRenderTarget.Bitmap.Size.Width, (int)Game.Renderer.PostProcessRenderTarget.Bitmap.Size.Height));
            }

            // Save Storage classes
            if (inputManager.IsKeyPressed(SharpDX.DirectInput.Key.Back))
            {
                PTSerializer serializer = new PTSerializer();
                MainCharacterStorage main = new MainCharacterStorage()
                {
                    Name = duckModel.Name,
                    Positon = duckModel.Position,
                    Size = duckModel.Size,
                    Rotation = duckModel.Rotation
                };
                main.ListTest = new List<Vector3>();
                for (int i = 0; i < 3; i++)
                    main.ListTest.Add(new Vector3(i));

                serializer.Serialize(main, true, main.Name + ".ponche");

                MainCharacterStorage algo = serializer.Deserialize<MainCharacterStorage>(main.Name + ".ponche");
            }
        }



        bool renderReflection;
        bool squareCreated = false;
        public override void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            if (Game.Renderer.PostProcessEffects.Count > 0)
            {
                spriteBatch.Begin();
                spriteBatch.DrawTexture(Game.Renderer.PostProcessRenderTarget.Bitmap, new Vector2(Game.Settings.Resolution.Height - 300, Game.Settings.Resolution.Height - 240), new Vector2(0.32f, 0.32f));
                spriteBatch.End();
            }

            if (renderReflection)
            {
                //Game.Renderer.RenderScreenToTexture(this, Game.CurrentCamera, Game.Renderer.SpriteBatch, reflectiveFloorMaterial.reflectionRenderTargetView); //reflectiveFloorMaterial.reflectionDepthStencil);
                Game.Renderer.RenderScreenToTexture(this, Game.CurrentCamera, Game.Renderer.SpriteBatch, ref reflectiveRenderTarget, floor.Meshes); //reflectiveFloorMaterial.reflectionDepthStencil);
                spriteBatch.Begin();
                spriteBatch.DrawTexture(reflectiveRenderTarget.Bitmap, new Vector2(0, Game.Settings.Resolution.Height - 240), new Vector2(0.32f, 0.32f));
                spriteBatch.End();
            }

            //// === To render into a Square.
            //if (renderReflection && !squareCreated)
            //{
            //    Square sq = new Square(this.Game);
            //    sq.Position = new Vector3(2, 0.5f, 3.5f);
            //    sq.Size = new Vector3(2f * 1.6f, 2f * 0.9f, 0); // Aspect ratio - 16:9
            //    AddRenderableComponentWithEffect(ref sq, ref basicEffect, "renderTotexture");

            //    PTMaterial textureMaterial = new PTMaterial(Game);
            //    textureMaterial.Name = "Texture material";
            //    textureMaterial.AddTexture(reflectiveRenderTarget.Texture, PTTexture2D.TextureType.Render);
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
