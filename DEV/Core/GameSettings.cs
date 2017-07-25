using System;
using System.Drawing;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using PoncheToolkit.Util;
using PoncheToolkit.Core.Services;
using System.Collections.Generic;
using SharpDX;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Class that contain all the global settings a game can have.
    /// </summary>
    public class GameSettings : UpdatableStateObject
    {
        /// <summary>
        /// The windows versions supported.
        /// </summary>
        public enum OSVersion
        {
            /// <summary>
            /// Windows 7
            /// </summary>
            Windows7,
            /// <summary>
            /// Windows 8
            /// </summary>
            Windows8,
            /// <summary>
            /// Windows 10
            /// </summary>
            Windows10
        }

        #region Fields
        private DriverType deviceDriverType;
        private DeviceCreationFlags deviceCreationFlags;
        private Size resolution;
        private List<Size> availableResolutions;
        private FeatureLevel featureLevel;
        private FeatureLevel[] featureLevels;
        private bool fullscreen;
        private bool depthBufferEnabled;
        private BlendingState blendState;
        private bool lockFramerate;
        private bool debugMode;
        private bool draw2DShadows;
        private float gamma;
        private SharpDX.DXGI.Format globalRenderTargetsFormat;
        #endregion

        #region Properties
        /// <summary>
        /// The type of driver to create the device.
        /// <para /> Default: Hardware.
        /// </summary>
        public DriverType DeviceDriverType
        {
            get { return deviceDriverType; }
            set { SetPropertyAsDirty(ref deviceDriverType, value); }
        }
        /// <summary>
        /// Flags to create the device.
        /// <para /> Default: None.
        /// </summary>
        public DeviceCreationFlags DeviceCreationFlags
        {
            get { return deviceCreationFlags; }
            set { SetPropertyAsDirty(ref deviceCreationFlags, value); }
        }
        /// <summary>
        /// The resolution of the game in pixels.
        /// <para /> Default: 1280 x 720 (720p).
        /// </summary>
        public Size Resolution
        {
            get { return resolution; }
            set { SetPropertyAsDirty(ref resolution, value); }
        }
        /// <summary>
        /// The resolution of the game in pixels.
        /// The list is sorted by the Width resolution first.
        /// This list is filled when creating the Graphics Device.
        /// <para /> Default: 1280 x 720 (720p).
        /// </summary>
        public List<Size> AvailableResolutions
        {
            get { return availableResolutions; }
            set { SetPropertyAsDirty(ref availableResolutions, value); }
        }
        /// <summary>
        /// Feature Level to create the device.
        /// <para /> Default: Level_11_1
        /// </summary>
        public FeatureLevel[] FeatureLevels
        {
            get { return featureLevels; }
            set { SetPropertyAsDirty(ref featureLevels, value); }
        }
        /// <summary>
        /// Value to set the game in full-screen mode.
        /// <para /> Default: false
        /// </summary>
        public bool Fullscreen
        {
            get { return fullscreen; }
            set { SetPropertyAsDirty(ref fullscreen, value); }
        }

        /// <summary>
        /// Get or set if the depth buffer is enabled.
        /// This value creates a new <see cref="DepthStencilState"/> with the IsDepthEnabled property set.
        /// <para /> Default: true
        /// </summary>
        public bool DepthBufferEnabled
        {
            get { return depthBufferEnabled; }
            set { SetPropertyAsDirty(ref depthBufferEnabled, value); }
        }

        /// <summary>
        /// Get or set the blend state for the rendering.
        /// <para /> Default: <see cref="BlendingState.Disabled"/>.
        /// </summary>
        public BlendingState BlendState
        {
            get { return blendState; }
            set { SetPropertyAsDirty(ref blendState, value); }
        }

        /// <summary>
        /// Get or set if the debug mode is enabled.
        /// If this mode is enabled, some debugging info will be rendered on screen.
        /// This information comes from the <see cref="DebuggerRenderableService"/> service.
        /// There will not exist a <see cref="DebuggerRenderableService"/> service in the Services list
        /// until this DebugMode is set to true the first time.
        /// Default: disabled.
        /// </summary>
        public bool DebugMode
        {
            get { return debugMode; }
            set { SetPropertyAsDirty(ref debugMode, value); }
        }

        /// <summary>
        /// Get or set the value if the Framerate should be locked.
        /// This will create a smooth stable experience but even if the framerate is high,
        /// it is simulated to keep at 60 FPS.
        /// If the framerate is low, the game will suffer and the renders will be less, but it wil still keep at 60 FPS.
        /// <para /> Default: false
        /// </summary>
        public bool LockFramerate
        {
            get { return lockFramerate; }
            set { SetPropertyAsDirty(ref lockFramerate, value); }
        }

        /// <summary>
        /// Get or set the value if the Engine will render 2D shadows.
        /// To render 2D shadows the engine uses the <see cref="Dynamic2DLightManagerService"/> service.
        /// </summary>
        public bool Draw2DShadows
        {
            get { return draw2DShadows; }
            set { SetPropertyAsDirty(ref draw2DShadows, value); }
        }

        /// <summary>
        /// Get the current operating system.
        /// If the Engine does not support this version of OS, it will throw an exception.
        /// </summary>
        public OSVersion OperatingSystemVersion
        {
            get
            {
                if (Environment.OSVersion.Version.Major == 6)
                {
                        switch (Environment.OSVersion.Version.Minor)
                        {
                            case 1:
                                return OSVersion.Windows7;
                            case 2:
                                return OSVersion.Windows8;
                            case 3:
                                return OSVersion.Windows8;
                        }
                }else if (Environment.OSVersion.Version.Major == 6)
                    return OSVersion.Windows10;

                Log.Error("Your operating system is not supported. -{0}-", Environment.OSVersion.VersionString);
                throw new Exception("Your operating system is not supported. - " + Environment.OSVersion.VersionString);
            }
        }

        /// <summary>
        /// The global gamma value to be sent to the shaders, depending on the monitor.
        /// Default: 2.0.
        /// </summary>
        public float Gamma
        {
            get { return gamma; }
            set { SetPropertyAsDirty(ref gamma, value); }
        }

        /// <summary>
        /// The default format to be used for the textures used as RenderTargets like the main back buffer.
        /// The default value is <see cref="SharpDX.DXGI.Format.R8G8B8A8_UNorm"/>. This consumes 32 bits per pixel.
        /// If there are some bandwith restrictions or low performance, try with <see cref="SharpDX.DXGI.Format.B5G6R5_UNorm"/>
        /// that uses only 16 bits per pixel. The color range will be lower but the performance can be somewhat better.
        /// </summary>
        public SharpDX.DXGI.Format GlobalRenderTargetsFormat
        {
            get { return globalRenderTargetsFormat; }
            set { SetPropertyAsDirty(ref globalRenderTargetsFormat, value); }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// Set the device creation flags to debug if the DEBUG preprocessor variable is defined.
        /// </summary>
        public GameSettings()
            : base()
        {
            DeviceCreationFlags = DeviceCreationFlags.None;
            DeviceCreationFlags |= DeviceCreationFlags.BgraSupport;
#if DEBUG
            DeviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
            deviceDriverType = DriverType.Hardware;
            resolution = new Size(1280, 720);
            availableResolutions = new List<Size>();
            featureLevels = new[] { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 };
            fullscreen = false;
            depthBufferEnabled = true;
            blendState = BlendingState.Disabled;
            lockFramerate = false;
            debugMode = false;
            draw2DShadows = false;
            gamma = 2f;
            globalRenderTargetsFormat = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                IsStateUpdated = true;
                OnStateUpdated();
            }
            return IsStateUpdated;
        }
    }
}
