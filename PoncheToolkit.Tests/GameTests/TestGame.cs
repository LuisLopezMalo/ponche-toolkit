using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using SharpDX.Direct3D;
using System.IO;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Tests.GameTests.Screens;
using System.Diagnostics;
using DebuggerRenderableService = PoncheToolkit.Core.Services.DebuggerRenderableService;
using PoncheToolkit.Graphics3D.Cameras;
using SharpDX.Direct3D11;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Graphics2D.Lighting;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Tests.GameTests
{
    /// <summary>
    /// Class to make every kind of tests to the engine.
    /// </summary>
    public class TestGame : Game11
    {
        #region Fields
        public DebuggerRenderableService debugger;
        private Camera camera;
        private TestScreen screen;
        private DemoScreen demo;
        private Dynamic2DLightManagerService lightManager;
        #endregion

        #region Initialization
        public TestGame()
        {
            this.Settings.Fullscreen = false;
            this.VerticalSyncEnabled = false;
            this.Settings.DebugMode = true;
            this.Settings.LockFramerate = false;
            this.Renderer.ProcessRenderMode = ProcessRenderingMode.Immediate;
            this.Renderer.ShadingRenderMode = ShadingRenderingMode.ForwardShading;
            this.OnInitialized += TestGame_OnInitialized;
        }
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            base.Initialize();

            //lightManager = new Dynamic2DLightManagerService(this, Dynamic2DLightManagerService.ShadowMapSize.Size512);
            //Services.AddService(lightManager);
            //this.Settings.Draw2DShadows = true;

            //this.Renderer.Rasterizer.IsAntialiasedLineEnabled = true;
        }

        public override void LoadContent()
        {
            //lightManager.LoadContent(this.ContentManager);

            base.LoadContent();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Render()
        {
            base.Render();
        }
        #endregion

        #region Events
        /// <summary>
        /// Event raised when finished the base initialization.
        /// </summary>
        private void TestGame_OnInitialized()
        {
            demo = new DemoScreen(this);
            demo.UpdateMode = Core.Management.Screen.GameScreen.ScreenUpdateMode.Always;
            demo.RenderMode = Core.Management.Screen.GameScreen.ScreenRenderMode.Always;
            ScreenManager.AddScreen(demo);

            try
            {
                camera = demo.Components["FirstPerson camera"] as Camera;
                debugger = Services.GetService(typeof(DebuggerRenderableService)) as DebuggerRenderableService;
                debugger.SimulateBurnCPU = false;
            }
            catch (Exception ex)
            {
                Log.Warning("No attached Debugger service", ex);
            }

            //LoadingScreen loading = new LoadingScreen(this);
            //loading.UpdateMode = Core.Management.Screen.GameScreen.ScreenUpdateMode.Always;
            //loading.RenderMode = Core.Management.Screen.GameScreen.ScreenRenderMode.Always;
            //ScreenManager.AddScreen(loading);

            //try
            //{
            //    this.DebugMode = true;
            //    camera = screen.Components["Main_Camera"] as Camera;
            //    debugger = Services.GetService(typeof(DebuggerRenderableService)) as DebuggerRenderableService;
            //    debugger.OnFPSCaptured += Debugger_OnFPSCaptured;
            //}
            //catch (Exception ex)
            //{
            //    Log.Warning("No attached Debugger service", ex);
            //}
        }
        #endregion
    }
}
