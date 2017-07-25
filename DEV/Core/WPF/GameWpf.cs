#define VERSION_11

using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using SharpDX.DXGI;
using Device11 = SharpDX.Direct3D11.Device;
using System.Diagnostics;
using PoncheToolkit.Util;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Util.Exceptions;
using System.Reflection;
using PoncheToolkit.Graphics3D;
using System.Collections.Generic;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Core.Management.Input;
using Microsoft.Wpf.Interop.DirectX;
using System.Runtime.ExceptionServices;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using PoncheToolkit.Graphics3D.Primitives;
using PoncheToolkit.Core.Management.Screen;
using System.Drawing;

namespace PoncheToolkit.Core.WPF
{
    /// <inheritdoc />
    /// <summary>
    /// Class that use Interop to show the rendered contents inside WPF.
    /// </summary>
    public class GameWpf : ILoggable, IDisposable
    {
        #region Fields
        private Game11 game;

        /// <summary>
        /// Handler to create custom Initialized event.
        /// </summary>
        public delegate void OnInitializedHandler();
        /// <summary>
        /// Event raised when the Initialize method has finished.
        /// </summary>
        public event OnInitializedHandler OnInitialized;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public Logger Log { get; set; }

        /// <summary>
        /// The game instance that is running.
        /// </summary>
        public Game11 Instance
        {
            get { return game; }
        }

        /// <summary>
        /// Get the last screen added.
        /// </summary>
        public GameScreen CurrentScreen
        {
            get { return game.ScreenManager.LastScreen; }
        }

        /// <summary>
        /// The current device used to render content.
        /// </summary>
        public Device11 Device
        {
            get { return game.Renderer.Device; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Instantiates the objects needed.
        /// </summary>
        public GameWpf()
            : this(new GameSettings())
        {
        }

        /// <summary>
        /// Instantiates the objects needed.
        /// </summary>
        public GameWpf(GameSettings settings)
        {
            Log = new Logger(typeof(GameWpf));
            Log.Info("Using DirectX11\n");
            Log.Info("| Settings:");
            foreach (PropertyInfo prop in settings.GetType().GetProperties())
            {
                Log.Info("| {0}: {1}", prop.Name, prop.GetValue(settings));
            }

            game = new Game11(settings);
            game.IsInterop = true;

            game.OnInitialized += () =>
            {
                OnInitialized?.Invoke();
            };
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the components for the rendering.
        /// </summary>
        public void Initialize()
        {
            game.Run();
        }

        /// <inheritdoc/>
        public void Update()
        {
            game.Update();
        }

        /// <summary>
        /// Render the <see cref="D3D11Image"/> surface with the given size.
        /// This call comes from a WPF application.
        /// Internally it creates a <see cref="SharpDX.Direct3D11.Texture2D"/> texture from the surface pointer
        /// and make the adjustments in the buffers so it can be rendered correctly.
        /// </summary>
        /// <param name="surface">The pointer to the <see cref="D3D11Image"/> object. This is obtained from
        /// the <see cref="D3D11Image.OnRender"/> method. </param>
        /// <param name="isNewSurface">The value indicating if the surface must be recreated, or it is the same.</param>
        /// <param name="size">The size of the surface to recreate the buffers correctly.</param>
        [HandleProcessCorruptedStateExceptions]
        public void Render(IntPtr surface, bool isNewSurface, Size size)
        {
            if (isNewSurface)
            {
                try
                {
                    SharpDX.DXGI.Resource dxgiResource;
                    using (var r = new SharpDX.ComObject(surface))
                        dxgiResource = r.QueryInterface<SharpDX.DXGI.Resource>();

                    var directx11Resource = game.Renderer.Device.OpenSharedResource<SharpDX.Direct3D11.Resource>(dxgiResource.SharedHandle);
                    var directx11Texture = directx11Resource.QueryInterface<SharpDX.Direct3D11.Texture2D>();

                    game.RecreateBuffersWpf(directx11Texture, size);
                }
                catch (Exception ex)
                {
                    Log.Error("Error loading new surface.", ex);
                    return;
                }
            }

            game.Render();
        }

        /// <summary>
        /// Add a screen to the game.
        /// </summary>
        /// <param name="screen"></param>
        public void AddGameScreen(GameScreen screen)
        {
            game.ScreenManager.AddScreen(screen);
        }

        /// <summary>
        /// Dispose the <see cref="Game11"/> instance. 
        /// </summary>
        public void Dispose()
        {
            game.Dispose();
        }
        #endregion
    }
}
