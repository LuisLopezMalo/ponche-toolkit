using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using Device = SharpDX.Direct3D11.Device;

namespace PoncheToolkit.Graphics2D
{
    /// <summary>
    /// Class that replace the Direct3D texture and add IConvertible implementation.
    /// </summary>
    public class PTTexture2D : UpdatableStateObject, IContentLoadable, IContentItem
    {
        /// <summary>
        /// Type of texture.
        /// </summary>

        public enum TextureType
        {
            /// <summary>
            /// Represent a texture that will be sent to the gpu to be rendered.
            /// </summary>
            Render = 0,
            /// <summary>
            /// Represent a texture that will be used as a Bump map. (Normal map).
            /// </summary>
            BumpMap,
            /// <summary>
            /// Represent a texture that will be used as a Specular map.
            /// </summary>
            SpecularMap,
            /// <summary>
            /// Represent a texture that will be used as a Reflection.
            /// </summary>
            Reflective,
        }

        private ShaderResourceView shaderResourceView;
        private PTTexturePath path;
        private Device device;
        private Bitmap bitmap;
        private SharpDX.Direct2D1.Effects.BitmapSource bitmapSourceEffect;
        private Dictionary<SharpDX.Direct2D1.DeviceContext, Bitmap> bitmaps;
        private TextureType textureType;
        private Vector2 translation;
        private bool generateMipMaps;

        #region Properties
        /// <summary>
        /// SamplerState to send to the Shader to sample the texture.
        /// </summary>
        public SamplerState Sampler;

        /// <summary>
        /// If the texture will generate mip maps.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public bool GenerateMipMaps
        {
            get { return generateMipMaps; }
            set { SetProperty(ref generateMipMaps, value); }
        }

        /// <summary>
        /// Get Width of the texture.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public int Width { get { return Texture.Description.Width; } }

        /// <summary>
        /// Get Height of the texture.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public int Height { get { return Texture.Description.Height; } }

        /// <summary>
        /// Get the original size of the texture.
        /// </summary>
        public Vector2 Size
        {
            get { return new Vector2(Width, Height); }
        }

        /// <summary>
        /// Get or set the translation of the UV coordinates inside the gpu.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public Vector2 Translation
        {
            get { return translation; }
            internal set { SetProperty(ref translation, value); }
        }

        /// <summary>
        /// Get the path where the texture was loaded.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public PTTexturePath Path
        {
            get { return path; }
            internal set { SetProperty(ref path, value); }
        }

        /// <summary>
        /// Get or set the bitmap property.
        /// This is used mainly when rendering this texture with Direct2D.
        /// When using the <see cref="ContentManager11.LoadTexture2D(string, string, bool)"/> this bitmap is automatically assigned.
        /// </summary>
        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { SetProperty(ref bitmap, value); }
        }

        /// <summary>
        /// Get or set the bitmap property.
        /// This is used mainly when rendering this texture with Direct2D.
        /// When using the <see cref="ContentManager11.LoadTexture2D(string, string, bool)"/> this bitmap is automatically assigned.
        /// </summary>
        public SharpDX.Direct2D1.Effects.BitmapSource BitmapSourceEffect
        {
            get { return bitmapSourceEffect; }
            set { SetProperty(ref bitmapSourceEffect, value); }
        }

        /// <summary>
        /// Get or set the shader resource view.
        /// If it is get before it is set, it is created with default properties. (NO ShaderResourceViewDescription).
        /// </summary>
        public ShaderResourceView ShaderResourceView
        {
            get
            {
                if (shaderResourceView == null)
                {
                    ShaderResourceViewDescription desc = new ShaderResourceViewDescription();
                    // TODO: if settings = MSAA, set dimension to multisampled.
                    desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
                    desc.Texture2D.MipLevels = Texture.Description.MipLevels;
                    desc.Format = Texture.Description.Format;
                    shaderResourceView = new ShaderResourceView(Texture.Device, Texture, desc);
                }

                return shaderResourceView;
            }
            set { SetProperty(ref shaderResourceView, value); }
        }

        /// <summary>
        /// Type of texture to identify how to process it before it is sent to the gpu.
        /// Default: <see cref="TextureType.Render"/>.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public TextureType Type
        {
            get { return textureType; }
            internal set { SetProperty(ref textureType, value); }
        }

        /// <inheritdoc/>
        public bool IsContentLoaded { get; set; }

        /// <summary>
        /// The DirectX Texture.
        /// </summary>
        public SharpDX.Direct3D11.Texture2D Texture;
        #endregion

        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">The device used to draw.</param>
        /// <param name="texture"></param>
        public PTTexture2D(Device device, Texture2D texture)
        {
            this.device = device;
            this.Texture = texture;
            this.bitmaps = new Dictionary<SharpDX.Direct2D1.DeviceContext, Bitmap>();
            this.Type = TextureType.Render;
            this.translation = Vector2.Zero;
        }

        /// <inheritdoc/>
        public void LoadContent(IContentManager contentManager)
        {
            // Create the Texture sampler.
            SamplerStateDescription samplerDescription = new SamplerStateDescription();
            //samplerDescription.Filter = SharpDX.Direct3D11.Filter.MinMagMipLinear;
            samplerDescription.Filter = SharpDX.Direct3D11.Filter.MinMagMipLinear;
            samplerDescription.AddressU = TextureAddressMode.Wrap;
            samplerDescription.AddressV = TextureAddressMode.Wrap;
            samplerDescription.AddressW = TextureAddressMode.Wrap;
            samplerDescription.MipLodBias = 0.0f;
            samplerDescription.MaximumAnisotropy = 16;
            samplerDescription.ComparisonFunction = Comparison.Never;
            samplerDescription.BorderColor = SharpDX.Color.Black;
            samplerDescription.MinimumLod = 0;
            samplerDescription.MaximumLod = float.MaxValue;

            Sampler = new SamplerState(device, samplerDescription);

            ToDispose(Texture);
            ToDispose(Sampler);
            ToDispose(bitmap);

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Add a texture path.
        /// This method fire the OnPropertyChanged event.
        /// </summary>
        /// <param name="texturePath">The texture path to be added.</param>
        /// <param name="asDirty">The value to indicate if the property must be set dirty, so when the <see cref="UpdateState"/>
        /// method is called, the textures are created. If it set to false, the textures must be manually loaded into memory.</param>
        internal void SetTexturePath(PTTexturePath texturePath, bool asDirty)
        {
            path = texturePath;
            if (asDirty)
                SetPropertyAsDirty(ref path, path, nameof(Path));
        }

        /// <summary>
        /// Add a texture path.
        /// This method fire the OnPropertyChanged event.
        /// </summary>
        /// <param name="texturePath">The texture path to be added.</param>
        /// <param name="type">The type of texture.</param>
        /// <param name="asDirty">The value to indicate if the property must be set dirty, so when the <see cref="UpdateState"/>
        /// method is called, the textures are created. If it set to false, the textures must be manually loaded into memory.</param>
        internal void SetTexturePath(PTTexturePath texturePath, PTTexture2D.TextureType type, bool asDirty)
        {
            this.Type = type;
            path = texturePath;
            if (asDirty)
                SetPropertyAsDirty(ref path, path, nameof(Path));
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (IsStateUpdated)
                return IsStateUpdated;

            IsStateUpdated = true;
            OnStateUpdated();
            return IsStateUpdated;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Utilities.Dispose(ref shaderResourceView);
            Utilities.Dispose(ref bitmapSourceEffect);
        }
    }
}
