using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Class that contain all the global settings a game can have.
    /// </summary>
    public class GameSettings
    {
        /// <summary>
        /// The type of driver to create the device.
        /// <para /> Default: Hardware.
        /// </summary>
        public DriverType DeviceDriverType { get; set; } = DriverType.Hardware;
        /// <summary>
        /// Flags to create the device.
        /// <para /> Default: None.
        /// </summary>
        public DeviceCreationFlags DeviceCreationFlags { get; set; } = DeviceCreationFlags.None;
        /// <summary>
        /// Feature Level to create the device.
        /// <para /> Default: 1280 x 720 (720p).
        /// </summary>
        public Size WindowSize { get; set; } = new Size(1280, 720);
        /// <summary>
        /// Feature Level to create the device.
        /// <para /> Default: Level_11_0
        /// </summary>
        public FeatureLevel FeatureLevel { get; set; } = FeatureLevel.Level_11_0;
        /// <summary>
        /// Value to set the game in full-screen mode.
        /// <para /> Default: false
        /// </summary>
        public bool Fullscreen { get; set; } = false;

        /// <summary>
        /// Constructor.
        /// Set the device creation flags to debug if the DEBUG preprocessor variable is defined.
        /// </summary>
        public GameSettings()
        {
#if DEBUG
            DeviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
        }
    }
}
