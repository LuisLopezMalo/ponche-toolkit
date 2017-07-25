using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using SharpDX.Direct3D11;
using SharpDX.WIC;

namespace PoncheToolkit.Util
{
    public class WICHelper
    {
        private static readonly ImagingFactory factory = new ImagingFactory();

        /// <summary>
        /// Loads a bitmap using WIC.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static SharpDX.WIC.BitmapSource LoadBitmap(string filename)
        {
            BitmapDecoder bitmapDecoder = new SharpDX.WIC.BitmapDecoder(
                factory,
                filename,
                DecodeOptions.CacheOnDemand);

            FormatConverter formatConverter = new FormatConverter(factory);

            formatConverter.Initialize(
                bitmapDecoder.GetFrame(0),
                SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                SharpDX.WIC.BitmapDitherType.None,
                null,
                0.0,
                SharpDX.WIC.BitmapPaletteType.Custom);

            return formatConverter;
        }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> from a WIC <see cref="SharpDX.WIC.BitmapSource"/>
        /// </summary>
        /// <param name="device">The Direct3D11 device</param>
        /// <param name="bitmapSource">The WIC bitmap source</param>
        /// <returns>A Texture2D</returns>
        public static Texture2D CreateTexture2DFromBitmap(SharpDX.Direct3D11.Device device, SharpDX.WIC.BitmapSource bitmapSource)
        {
            // Allocate DataStream to receive the WIC image pixels
            int stride = bitmapSource.Size.Width * 4;
            using (var buffer = new SharpDX.DataStream(bitmapSource.Size.Height * stride, true, true))
            {
                // Copy the content of the WIC to the buffer
                bitmapSource.CopyPixels(stride, buffer);
                return new SharpDX.Direct3D11.Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = bitmapSource.Size.Width,
                    Height = bitmapSource.Size.Height,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                }, new SharpDX.DataRectangle(buffer.DataPointer, stride));
            }
        }

        /// <summary>
        /// Load a Texture from file name.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="fileName"></param>
        /// <returns>Return a custom Texture2D PoncheToolkit object with the texture inside.</returns>
        public static Graphics3D.Texture2D LoadTextureFromFile(Device device, string fileName)
        {
            BitmapSource bitmap = WICHelper.LoadBitmap(fileName);
            Texture2D texture = WICHelper.CreateTexture2DFromBitmap(device, bitmap);
            Graphics3D.Texture2D result = new Graphics3D.Texture2D(texture);

            return result;
        }

    }
}