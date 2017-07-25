using Microsoft.Wpf.Interop.DirectX;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Core.WPF;
using PoncheToolkit.EffectsCreator.Commands;
using PoncheToolkit.EffectsCreator.Entities.SharpDX;
using PoncheToolkit.EffectsCreator.Util;
using PoncheToolkit.EffectsCreator.Views;
using PoncheToolkit.Graphics3D.Cameras;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace PoncheToolkit.EffectsCreator.ViewModels
{
    /// <summary>
    /// Data Context for the RichTextBox where the shader code is held.
    /// </summary>
    public class RenderViewModel : MainWindowChildViewModelBase
    {
        private MainWindowViewModel context;
        private D3D11Image interopImage;
        private GameWpf game;
        private TimeSpan lastRender;
        private int width;
        private int height;
        private bool lastVisible;
        private bool captureMouse;
        private Point initialMousePosition;
        private Camera camera;
        private Size size;

        #region Properties
        /// <summary>
        /// The DirectX Image that is rendered.
        /// </summary>
        public D3D11Image InteropImage
        {
            get { return interopImage; }
            set { SetProperty(ref interopImage, value); }
        }

        /// <summary>
        /// The Width of the component.
        /// </summary>
        public int Width
        {
            get { return width; }
            set { SetProperty(ref width, value); }
        }

        /// <summary>
        /// The Height of the component.
        /// </summary>
        public int Height
        {
            get { return height; }
            set { SetProperty(ref height, value); }
        }

        /// <summary>
        /// The Size of the component.
        /// </summary>
        public Size Size
        {
            get { return size; }
            set { SetProperty(ref size, value); }
        }

        /// <summary>
        /// The instance of the Sharp-Wpf game ready.
        /// </summary>
        public GameWpf GameWpf
        {
            get { return game; }
            set { game = value; }
        }

        /// <summary>
        /// The current selected component.
        /// </summary>
        public GameComponent SelectedComponent { get; set; }
        #endregion

        #region Events
        ///// <summary>
        ///// Handler to create custom OnRequestRender event.
        ///// </summary>
        //public delegate void OnRequestRenderHandler();
        ///// <summary>
        ///// Event raised when a request to refresh the rendering from other viewModel is made.
        ///// </summary>
        //public event OnRequestRenderHandler OnRequestRender;
        #endregion
        
        #region Commands
        /// <summary>
        /// Command when the window has loaded.
        /// </summary>
        public RelayCommand<RenderView> Render_LoadedCommand { get; set; }

        /// <summary>
        /// Command when the window has changed size.
        /// </summary>
        public RelayCommand<SizeChangedEventArgs> Render_SizeChangedCommand { get; set; }

        /// <summary>
        /// Set the captureMouse flag to true.
        /// </summary>
        public RelayCommand<MouseButtonEventArgs> Render_MouseLeftButtonDownCommand { get; set; }

        /// <summary>
        /// Set the captureMouse flag to false.
        /// </summary>
        public RelayCommand<MouseButtonEventArgs> Render_MouseLeftButtonUpCommand { get; set; }

        /// <summary>
        /// Map the position to set the <see cref="PoncheToolkit.Graphics3D.Cameras.Camera"/> position.
        /// </summary>
        public RelayCommand<MouseEventArgs> Render_MouseMoveCommand { get; set; }

        /// <summary>
        /// Detect changes on the mouse wheel.
        /// </summary>
        public RelayCommand<MouseWheelEventArgs> Render_MouseWheelCommand { get; set; }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public RenderViewModel()
        {
            if (IsInDesignMode)
                return;

            Render_LoadedCommand = new RelayCommand<RenderView>(render_loadedCommandExecution);
            Render_SizeChangedCommand = new RelayCommand<SizeChangedEventArgs>(render_sizeChangedCommandExecution);
            Render_MouseLeftButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(render_mouseLeftButtonDownCommandExecution);
            Render_MouseLeftButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(render_mouseLeftButtonUpCommandExecution);
            Render_MouseMoveCommand = new RelayCommand<MouseEventArgs>(render_mouseMoveCommandExecution);
            Render_MouseWheelCommand = new RelayCommand<MouseWheelEventArgs>(render_mouseWheelCommandExecution);
            InteropImage = new D3D11Image();
        }

        #region Editor Commands
        private void render_loadedCommandExecution(RenderView renderView)
        {
            // Initialize
            this.context = this.MainWindow.DataContext as MainWindowViewModel;

            InteropImage.WindowOwner = (new System.Windows.Interop.WindowInteropHelper(context.MainWindow)).Handle;
            game = new GameWpf();
            game.Instance.VerticalSyncEnabled = false;
            game.Instance.Settings.Fullscreen = false;
            game.OnInitialized += Game_OnInitialized;
            
            try
            {
                game.Initialize();
            }
            catch (Exception ex)
            {
                string msg = "DirectX11 window could not initialize correctly";
                Log.Error(msg, ex);
                MessageBox.Show(ex.Message, msg);
                return;
            }

            double dpiScale = processDpi();
            width = (int)(renderView.RenderSize.Width * dpiScale);
            height = (int)(renderView.RenderSize.Height * dpiScale);
            InteropImage.SetPixelSize(width, height);
        }

        private void render_sizeChangedCommandExecution(SizeChangedEventArgs size)
        {
            double dpiScale = processDpi();

            if (size == null)
            {
                width = (int)(context.MainWindow.ActualWidth < 0 ? 0 : Math.Ceiling(context.MainWindow.ActualWidth * dpiScale));
                height = (int)(context.MainWindow.ActualHeight < 0 ? 0 : Math.Ceiling(context.MainWindow.ActualHeight * dpiScale));
            }else
            {
                width = (int)(size.NewSize.Width * dpiScale);
                height = (int)(size.NewSize.Height * dpiScale);
            }
            
            // Notify the D3D11Image of the pixel size desired for the DirectX rendering.
            // The D3DRendering component will determine the size of the new surface it is given, at that point.
            InteropImage.SetPixelSize(width, height);

            // Stop rendering if the D3DImage isn't visible - currently just if width or height is 0
            // TODO: more optimizations possible (scrolled off screen, etc...)
            bool isVisible = (width != 0 && height != 0);
            if (lastVisible != isVisible)
            {
                lastVisible = isVisible;
                if (lastVisible)
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                else
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
            }
        }

        private double processDpi()
        {
            double dpiScale = 1.0; // default value for 96 dpi

            // Determine DPI (as of .NET 4.6.1, this returns the DPI of the primary monitor, if you have several different DPIs)
            var hwndTarget = PresentationSource.FromVisual(context.MainWindow).CompositionTarget as HwndTarget;
            if (hwndTarget != null)
                dpiScale = hwndTarget.TransformToDevice.M11;

            return dpiScale;
        }

        /// <summary>
        /// Set the flag to capture the mouse movement.
        /// </summary>
        /// <param name="args"></param>
        private void render_mouseLeftButtonDownCommandExecution(MouseButtonEventArgs args)
        {
            captureMouse = true;
            initialMousePosition = args.GetPosition(MainWindow);
        }

        /// <summary>
        /// Set the flag to stop capturing the mouse movement.
        /// </summary>
        /// <param name="args"></param>
        private void render_mouseLeftButtonUpCommandExecution(MouseButtonEventArgs args)
        {
            captureMouse = false;
        }

        /// <summary>
        /// Set the camera position.
        /// </summary>
        /// <param name="args"></param>
        private void render_mouseMoveCommandExecution(MouseEventArgs args)
        {
            if (captureMouse)
            {
                Point currentMousePoint = args.GetPosition(MainWindow);
                Vector delta = (currentMousePoint - initialMousePosition) * 0.1f;
                camera.Position = new SharpDX.Vector3(camera.Position.X - (float)delta.X, camera.Position.Y + (float)delta.Y, camera.Position.Z);

                initialMousePosition = args.GetPosition(MainWindow);
            }
        }

        /// <summary>
        /// Changes the camera zoom.
        /// </summary>
        /// <param name="args"></param>
        private void render_mouseWheelCommandExecution(MouseWheelEventArgs args)
        {
            camera.Position = new SharpDX.Vector3(camera.Position.X, camera.Position.Y, camera.Position.Z + (args.Delta * 0.01f));
        }

        #endregion

        #region Events
        private void Game_OnInitialized()
        {
            game.AddGameScreen(new RenderScreen(game.Instance));
            camera = GameWpf.CurrentScreen.Components["MainCamera"] as Camera;
            InteropImage.OnRender = this.DoRender;
            InteropImage.RequestRender();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;

            // It's possible for Rendering to call back twice in the same frame 
            // so only render when we haven't already rendered in this frame.
            if (this.lastRender != args.RenderingTime)
            {
                InteropImage.RequestRender();
                this.lastRender = args.RenderingTime;
            }
        }

        private void DoRender(IntPtr surface, bool isNewSurface)
        {
            game.Update();
            game.Render(surface, isNewSurface, new System.Drawing.Size(width, height));
        }
        #endregion
    }
}
