using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Class that represent and wrap the functionality of a graphics device.
    /// </summary>
    public class GraphicsDevice : IDisposable, ILoggable
    {
        public const int MultisampleCountMaximum = 32;
        protected internal DeviceContext immediateContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="driverType"></param>
        public GraphicsDevice(DriverType driverType)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="adapter">Adapter.</param>
        public GraphicsDevice(Adapter adapter)
        {

        }

        /// <summary>
        /// Constructor. See <see cref="SharpDX.Direct3D11.D3D11.CreateDevice(Adapter, DriverType, IntPtr, DeviceCreationFlags, FeatureLevel[], int, int, SharpDX.Direct3D11.Device, out FeatureLevel, out DeviceContext)"/>
        /// for more information.
        /// </summary>
        /// <param name="driverType">Type of driver.</param>
        /// <param name="flags">Creation flags.</param>
        public GraphicsDevice(DriverType driverType, DeviceCreationFlags flags)
        {

        }

        /// <summary>
        /// Constructor. <see cref="SharpDX.Direct3D11.D3D11.CreateDevice(Adapter, DriverType, IntPtr, DeviceCreationFlags, FeatureLevel[], int, int, SharpDX.Direct3D11.Device, out FeatureLevel, out DeviceContext)"/>
        /// for more information.
        /// </summary>
        /// <param name="adapter">Adapter</param>
        /// <param name="flags">Creation flags.</param>
        public GraphicsDevice(Adapter adapter, DeviceCreationFlags flags)
        {

        }

        /// <summary>
        /// Constructor. See <see cref="SharpDX.Direct3D11.D3D11.CreateDevice(Adapter, DriverType, IntPtr, DeviceCreationFlags, FeatureLevel[], int, int, SharpDX.Direct3D11.Device, out FeatureLevel, out DeviceContext)"/>
        /// for more information.
        /// </summary>
        /// <param name="driverType">Type of driver.</param>
        /// <param name="flags">Creation flags.</param>
        /// <param name="featureLevels">The feature levels to allow when creating the device.</param>
        public GraphicsDevice(DriverType driverType, DeviceCreationFlags flags, params FeatureLevel[] featureLevels)
        {

        }

        /// <summary>
        /// Get the flags used during the call to create the device with <see cref="SharpDX.Direct3D11.D3D11.CreateDevice(Adapter, DriverType, IntPtr, DeviceCreationFlags, FeatureLevel[], int, int, SharpDX.Direct3D11.Device, out FeatureLevel, out DeviceContext)"/>
        /// </summary>
        public DeviceCreationFlags CreationFlags { get; }

        /// <summary>
        /// Gets or sets the debug-name for this object.
        /// </summary>
        public string DebugName { get; set; }
        
        /// <summary>
        /// Get or sets the exception-mode flags.
        /// </summary>
        /// <remarks>
        /// An exception-mode flag is used to elevate an error condition to a non-continuable
        //     exception. Windows?Phone?8: This API is supported.
        /// </remarks>
        public int ExceptionMode { get; set; }
        //
        // Summary:
        //     Gets the feature level of the hardware device.
        //
        // Remarks:
        //     Feature levels determine the capabilities of your device.Windows?Phone?8: This
        //     API is supported.
        public FeatureLevel FeatureLevel { get; }
        //
        // Summary:
        //     Gets an immediate context, which can play back command lists.
        //
        // Remarks:
        //     The GetImmediateContext method returns an SharpDX.Direct3D11.DeviceContext object
        //     that represents an immediate context which is used to perform rendering that
        //     you want immediately submitted to a device. For most applications, an immediate
        //     context is the primary object that is used to draw your scene.The GetImmediateContext
        //     method increments the reference count of the immediate context by one. Therefore,
        //     you must call Release on the returned interface reference when you are done with
        //     it to avoid a memory leak. Windows?Phone?8: This API is supported.
        public DeviceContext ImmediateContext
        {
            get { return immediateContext; }
        }

        /// <inheritdoc/>
        public Logger Log { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Utilities.Dispose(ref immediateContext);
        }
    }
}
