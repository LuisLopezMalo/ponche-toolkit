using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using SharpDX.Direct2D1;
using SharpDX.IO;
using SharpDX;
using PoncheToolkit.Graphics3D;

namespace PT.Util
{
    /// <summary>
    /// Class that contain methods to load textures into memory.
    /// </summary>
    public class WICHelper
    {
        /// <summary>
        /// Enumeration with the supported image types to decode or encode.
        /// </summary>
        public enum SupportedImageTypes
        {
            /// <summary>
            /// Jpg
            /// </summary>
            Jpeg = 0,
            /// <summary>
            /// Bmp
            /// </summary>
            Bmp,
            /// <summary>
            /// Png
            /// </summary>
            Png,
            /// <summary>
            /// Dds
            /// </summary>
            Dds,
            /// <summary>
            /// Tiff
            /// </summary>
            Tiff,
            /// <summary>
            /// Ico
            /// </summary>
            Ico,
            /// <summary>
            /// Gif
            /// </summary>
            Gif
        }

        private static readonly ImagingFactory2 factory = new ImagingFactory2();

        #region Decode Images
        /// <summary>
        /// Loads a bitmap using WIC.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static SharpDX.WIC.BitmapSource LoadBitmapSource(string filename)
        {
            using (BitmapDecoder bitmapDecoder = GetDecoder(filename))
            {
                FormatConverter converter = new FormatConverter(factory);

                converter.Initialize(bitmapDecoder.GetFrame(0), SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                    SharpDX.WIC.BitmapDitherType.None, null, 0.0, SharpDX.WIC.BitmapPaletteType.Custom);

                return converter;
            }
        }

        /// <summary>
        /// Loads a bitmap source using WIC.
        /// Creates a <see cref="Bitmap1"/>.
        /// </summary>
        /// <param name="bitmapDecoder"></param>
        /// <param name="context">The <see cref="SharpDX.Direct2D1.DeviceContext"/> used to create the bitmap.</param>
        /// <param name="bitmap">The out bitmap created from the BitmapSource</param>
        /// <returns></returns>
        public static SharpDX.WIC.BitmapSource LoadBitmapSource(BitmapDecoder bitmapDecoder, SharpDX.Direct2D1.DeviceContext context, out SharpDX.Direct2D1.Bitmap bitmap)
        {
            FormatConverter converter = new FormatConverter(factory);
            
            converter.Initialize(bitmapDecoder.GetFrame(0), SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                SharpDX.WIC.BitmapDitherType.None, null, 0.0, SharpDX.WIC.BitmapPaletteType.Custom);

            // To get a Bitmap
            bitmap = context == null ? null : SharpDX.Direct2D1.Bitmap.FromWicBitmap(context, converter);

            return converter;
        }

        /// <summary>
        /// Loads a bitmap source using WIC.
        /// Creates a <see cref="Bitmap1"/>.
        /// </summary>
        /// <param name="bitmapDecoder"></param>
        /// <param name="renderTarget">The <see cref="SharpDX.Direct2D1.RenderTarget"/> used to create the bitmap.</param>
        /// <param name="bitmap">The out bitmap created from the BitmapSource</param>
        /// <returns></returns>
        public static SharpDX.WIC.BitmapSource LoadBitmapSource(BitmapDecoder bitmapDecoder, SharpDX.Direct2D1.RenderTarget renderTarget, out SharpDX.Direct2D1.Bitmap bitmap)
        {
            FormatConverter converter = new FormatConverter(factory);

            converter.Initialize(bitmapDecoder.GetFrame(0), SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                SharpDX.WIC.BitmapDitherType.None, null, 0.0, SharpDX.WIC.BitmapPaletteType.Custom);

            // To get a Bitmap
            bitmap = renderTarget == null ? null : SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTarget, converter);

            return converter;
        }

        /// <summary>
        /// Loads a bitmap using WIC.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filestream"></param>
        /// <returns></returns>
        public static SharpDX.WIC.BitmapSource LoadBitmapSource(SupportedImageTypes type, SharpDX.IO.NativeFileStream filestream)
        {
            using (BitmapDecoder bitmapDecoder = GetDecoder(type, filestream))
            {
                FormatConverter converter = new FormatConverter(factory);

                converter.Initialize(bitmapDecoder.GetFrame(0), SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                    SharpDX.WIC.BitmapDitherType.None, null, 0.0, SharpDX.WIC.BitmapPaletteType.Custom);

                return converter;
            }
        }

        /// <summary>
        /// Loads a bitmap for the specified <see cref="SharpDX.Direct2D1.DeviceContext"/> using WIC.
        /// </summary>
        /// <param name="context">The <see cref="SharpDX.Direct2D1.DeviceContext"/> context to be used when rendering this bitmap.</param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static SharpDX.Direct2D1.Bitmap CreateBitmap(SharpDX.Direct2D1.DeviceContext context, string filename)
        {
            using (BitmapDecoder bitmapDecoder = new SharpDX.WIC.BitmapDecoder(factory, filename, DecodeOptions.CacheOnDemand))
            {
                FormatConverter converter = new FormatConverter(factory);
                
                // To get a Bitmap
                BitmapFrameDecode frame = bitmapDecoder.GetFrame(0);
                converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA);
                SharpDX.Direct2D1.Bitmap bitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(context, converter);

                return bitmap;
            }
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
                    OptionFlags = ResourceOptionFlags.Shared, // OptionFlags = ResourceOptionFlags.SharedNthandle | ResourceOptionFlags.SharedKeyedmutex,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                }, new SharpDX.DataRectangle(buffer.DataPointer, stride));
            }
        }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> from a WIC <see cref="SharpDX.WIC.BitmapSource"/>
        /// </summary>
        /// <param name="device">The Direct3D11 device</param>
        /// <param name="bitmapSources">The WIC bitmap sources</param>
        /// <returns>A Texture2D</returns>
        public static Texture2D CreateTexture2DFromBitmap(SharpDX.Direct3D11.Device device, List<SharpDX.WIC.BitmapSource> bitmapSources)
        {
            SharpDX.DataRectangle[] dataRectangles = new SharpDX.DataRectangle[bitmapSources.Count];
            for (int i = 0; i < bitmapSources.Count; i++)
            {
                // Allocate DataStream to receive the WIC image pixels
                int stride = bitmapSources[i].Size.Width * 4;
                //using (var buffer = new SharpDX.DataStream(bitmapSources[i].Size.Height * stride, true, true))
                SharpDX.DataStream buffer = new SharpDX.DataStream(bitmapSources[i].Size.Height * stride, true, true);
                {
                    // Copy the content of the WIC to the buffer
                    bitmapSources[i].CopyPixels(stride, buffer);
                    dataRectangles[i] = new DataRectangle(buffer.DataPointer, stride);
                }
                buffer.Dispose();
                buffer = null;
            }

            return new SharpDX.Direct3D11.Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
            {
                Width = bitmapSources[0].Size.Width,
                Height = bitmapSources[0].Size.Height,
                ArraySize = bitmapSources.Count,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Immutable,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.Shared, // OptionFlags = ResourceOptionFlags.SharedNthandle | ResourceOptionFlags.SharedKeyedmutex,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            }, dataRectangles);
        }

        /// <summary>
        /// Load a Texture from file name.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="fileName"></param>
        /// <returns>Return a custom Texture2D PoncheToolkit object with the texture inside.</returns>
        public static Graphics2D.PTTexture2D LoadTextureFromFile(SharpDX.Direct3D11.Device device, string fileName)
        {
            BitmapSource bitmap = WICHelper.LoadBitmapSource(fileName);
            Texture2D texture = WICHelper.CreateTexture2DFromBitmap(device, bitmap);
            bitmap.Dispose();

            Graphics2D.PTTexture2D result = new Graphics2D.PTTexture2D(device, texture);

            return result;
        }

        /// <summary>
        /// Load a Texture from file name.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="context">The <see cref="SharpDX.Direct2D1.DeviceContext"/> to be used to load the Bitmap.</param>
        /// <param name="texturePaths">The array with the file names to create as a Texture2D array.</param>
        /// <returns>Return a custom Texture2D PoncheToolkit object with the texture inside.</returns>
        public static Graphics2D.PTTexture2D LoadTextureFromFile(SharpDX.Direct3D11.Device device, SharpDX.Direct2D1.DeviceContext context, string[] texturePaths)
        {
            List<BitmapSource> bitmaps = new List<BitmapSource>();
            SharpDX.Direct2D1.Bitmap bitmapResult = null;
            Graphics2D.PTTexture2D result = new Graphics2D.PTTexture2D(device, null);
            foreach (string path in texturePaths)
            {
                result.AddTexturePath(new TexturePath(path), false);
                //using (BitmapDecoder bitmapDecoder = new SharpDX.WIC.BitmapDecoder(factory, path, DecodeOptions.CacheOnDemand))
                using (BitmapDecoder bitmapDecoder = GetDecoder(path))
                {
                    BitmapSource bitmap = WICHelper.LoadBitmapSource(bitmapDecoder, context, out bitmapResult);
                    bitmaps.Add(bitmap);
                }
            }

            Texture2D texture = WICHelper.CreateTexture2DFromBitmap(device, bitmaps);
            result.Texture = texture;
            result.Bitmap = bitmapResult;

            for (int i = 0; i < bitmaps.Count; i++)
                bitmaps[i].Dispose();

            return result;
        }

        /// <summary>
        /// Load a Texture from file name.
        /// Create the Bitmap for the <see cref="Graphics2D.PTTexture2D"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="context">The <see cref="SharpDX.Direct2D1.DeviceContext"/> to be used to load the Bitmap.</param>
        /// <param name="fileName">File name of the texture to be loaded.</param>
        /// <returns>Return a custom Texture2D PoncheToolkit object with the texture inside.</returns>
        public static Graphics2D.PTTexture2D LoadTextureFromFile(SharpDX.Direct3D11.Device device, SharpDX.Direct2D1.DeviceContext context, string fileName)
        {
            SharpDX.Direct2D1.Bitmap bitmapResult = null;
            using (BitmapDecoder bitmapDecoder = new SharpDX.WIC.BitmapDecoder(factory, fileName, DecodeOptions.CacheOnDemand))
            {
                //using (BitmapSource bitmap = WICHelper.LoadBitmapSource(bitmapDecoder, context, out bitmapResult))
                //{
                //    Texture2D texture = WICHelper.CreateTexture2DFromBitmap(device, bitmap);
                //    Graphics2D.PTTexture2D result = new Graphics2D.PTTexture2D(device, texture);
                //    result.Bitmap = bitmapResult;
                //    result.BitmapSourceEffect = new SharpDX.Direct2D1.Effects.BitmapSource(context);
                //    result.BitmapSourceEffect.WicBitmapSource = bitmap;
                //    result.BitmapSourceEffect.Cached = true;
                //    return result;
                //}

                BitmapSource bitmap = WICHelper.LoadBitmapSource(bitmapDecoder, context, out bitmapResult);
                Texture2D texture = WICHelper.CreateTexture2DFromBitmap(device, bitmap);
                Graphics2D.PTTexture2D result = new Graphics2D.PTTexture2D(device, texture);
                result.AddTexturePath(new Graphics3D.TexturePath(fileName), false);
                result.Bitmap = bitmapResult;
                result.BitmapSourceEffect = new SharpDX.Direct2D1.Effects.BitmapSource(context);
                result.BitmapSourceEffect.WicBitmapSource = bitmap;
                result.BitmapSourceEffect.Cached = true;
                return result;
            }
        }
        #endregion

        #region Encode Images
        /// <summary>
        /// Save a texture to the given output path.
        /// </summary>
        /// <param name="device">The <see cref="SharpDX.Direct2D1.DeviceContext"/> where the image will be saved.</param>
        /// <param name="target">The rendered image to save.</param>
        /// <param name="outputPath">The physical path where the image will be saved.</param>
        /// <param name="boundaries">The size of the output image.</param>
        public static void SaveTexture(SharpDX.Direct2D1.Device device, SharpDX.Direct2D1.Image target, string outputPath, Rectangle boundaries)
        {
            SharpDX.Direct2D1.PixelFormat format = new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied);
            
            // use the appropiate overload to write either to stream or to a file
            using (var stream = new WICStream(factory, outputPath, NativeFileAccess.Write))
            {
                using (var encoder = new SharpDX.WIC.PngBitmapEncoder(factory))
                {
                    encoder.Initialize(stream);

                    using (BitmapFrameEncode bitmapFrameEncode = new BitmapFrameEncode(encoder))
                    {
                        bitmapFrameEncode.Initialize();
                        bitmapFrameEncode.SetSize(boundaries.Width, boundaries.Height);
                        Guid pixelFormat = SharpDX.WIC.PixelFormat.Format32bppPRGBA;
                        bitmapFrameEncode.SetPixelFormat(ref pixelFormat);

                        // Write the frame using WIC into the given FrameEncode using the target.
                        using (var imageEncoder = new ImageEncoder(factory, device))
                        {
                            imageEncoder.WriteFrame(target, bitmapFrameEncode,
                                new ImageParameters(format, 96, 96, boundaries.Left, boundaries.Top, boundaries.Width, boundaries.Height));
                        }

                        bitmapFrameEncode.Commit();
                    }
                    encoder.Commit();
                }
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get the correct BitmapEncoder by its guid.
        /// Get <see cref="SupportedImageTypes"/> by its extension.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static SharpDX.WIC.BitmapDecoder GetDecoder(string filename)
        {
            Guid bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Jpeg;
            string extension = System.IO.Path.GetExtension(filename.ToLower().Trim());

            switch (extension)
            {
                case ".bmp":
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Bmp;
                    break;
                case ".dds":
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Dds;
                    break;
                case ".jpeg":
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Jpeg;
                    break;
                case ".jpg":
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Jpeg;
                    break;
                case ".png":
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Png;
                    break;
                case ".tiff":
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Tiff;
                    break;
                case ".ico":
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Ico;
                    break;
                case ".gif":
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Gif;
                    break;
            }

            return new BitmapDecoder(factory, filename, bitmapDecoderGuid, NativeFileAccess.Read, DecodeOptions.CacheOnDemand);
        }

        /// <summary>
        /// Get the correct BitmapEncoder by its guid.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileStream">The filestream of the file</param>
        /// <returns></returns>
        private static SharpDX.WIC.BitmapDecoder GetDecoder(SupportedImageTypes type, SharpDX.IO.NativeFileStream fileStream)
        {
            Guid bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Jpeg;
            switch (type)
            {
                case SupportedImageTypes.Bmp:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Bmp;
                    break;
                case SupportedImageTypes.Dds:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Dds;
                    break;
                case SupportedImageTypes.Jpeg:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Jpeg;
                    break;
                case SupportedImageTypes.Png:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Png;
                    break;
                case SupportedImageTypes.Tiff:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Tiff;
                    break;
                case SupportedImageTypes.Ico:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Ico;
                    break;
                case SupportedImageTypes.Gif:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Gif;
                    break;
            }

            return new BitmapDecoder(factory, fileStream, bitmapDecoderGuid, DecodeOptions.CacheOnDemand);
        }

        /// <summary>
        /// Get the correct BitmapEncoder by its guid.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static SharpDX.WIC.BitmapDecoder GetDecoder(SupportedImageTypes type, string filename)
        {
            Guid bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Jpeg;
            switch (type)
            {
                case SupportedImageTypes.Bmp:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Bmp;
                    break;
                case SupportedImageTypes.Dds:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Dds;
                    break;
                case SupportedImageTypes.Jpeg:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Jpeg;
                    break;
                case SupportedImageTypes.Png:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Png;
                    break;
                case SupportedImageTypes.Tiff:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Tiff;
                    break;
                case SupportedImageTypes.Ico:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Ico;
                    break;
                case SupportedImageTypes.Gif:
                    bitmapDecoderGuid = SharpDX.WIC.BitmapDecoderGuids.Gif;
                    break;
            }

            return new BitmapDecoder(factory, filename, bitmapDecoderGuid, NativeFileAccess.Read, DecodeOptions.CacheOnDemand);
        }

        #endregion

        #region Async methods
        ///// <summary>
        ///// Loads a bitmap using WIC.
        ///// Creates a <see cref="Bitmap1"/>.
        ///// </summary>
        ///// <param name="filename"></param>
        ///// <param name="bitmapDecoder"></param>
        ///// <param name="context">The <see cref="SharpDX.Direct2D1.DeviceContext"/> used to create the bitmap.</param>
        ///// <param name="bitmap">The out bitmap created from the BitmapSource</param>
        ///// <returns></returns>
        //public static async Task<SharpDX.WIC.BitmapSource> LoadBitmapSourceAsync(string filename, BitmapDecoder bitmapDecoder, SharpDX.Direct2D1.DeviceContext context, out Bitmap1 bitmap)
        //{
        //    Func<SharpDX.WIC.BitmapSource> func = new Func<BitmapSource>(() =>
        //    {
        //        FormatConverter converter = new FormatConverter(factory);

        //        converter.Initialize(bitmapDecoder.GetFrame(0), SharpDX.WIC.PixelFormat.Format32bppPRGBA,
        //            SharpDX.WIC.BitmapDitherType.None, null, 0.0, SharpDX.WIC.BitmapPaletteType.Custom);

        //        // To get a Bitmap
        //        bitmap = context == null ? null : Bitmap1.FromWicBitmap(context, converter);

        //        return converter;
        //    });

        //    Task<SharpDX.WIC.BitmapSource> t = Task<SharpDX.WIC.BitmapSource>.Factory.StartNew(func);
        //    return await t;
        //}

        ///// <summary>
        ///// Load a Texture from file name.
        ///// Create the Bitmap for the <see cref="Graphics2D.PTTexture2D"/>.
        ///// </summary>
        ///// <param name="device"></param>
        ///// <param name="context">The <see cref="SharpDX.Direct2D1.DeviceContext"/> to be used to load the Bitmap.</param>
        ///// <param name="fileName">File name of the texture to be loaded.</param>
        ///// <returns>Return a custom Texture2D PoncheToolkit object with the texture inside.</returns>
        //public static async Task<Graphics2D.PTTexture2D> LoadTextureFromFileAsync(SharpDX.Direct3D11.Device device, SharpDX.Direct2D1.DeviceContext context, string fileName)
        //{
        //    Bitmap1 bitmapResult = null;
        //    using (BitmapDecoder bitmapDecoder = new SharpDX.WIC.BitmapDecoder(factory, fileName, DecodeOptions.CacheOnDemand))
        //    {
        //        using (BitmapSource bitmap = WICHelper.LoadBitmapSource(fileName, bitmapDecoder, context, out bitmapResult))
        //        {
        //            Texture2D texture = WICHelper.CreateTexture2DFromBitmap(device, bitmap);
        //            Graphics2D.PTTexture2D result = new Graphics2D.PTTexture2D(device, texture);
        //            result.Bitmap = bitmapResult;
        //            return result;
        //        }
        //    }
        //}
        #endregion

    }
}