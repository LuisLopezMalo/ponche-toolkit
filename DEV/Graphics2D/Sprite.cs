using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoncheToolkit.Graphics2D
{
    /// <summary>
    /// Represent a single sprite to be drawn.
    /// Has properties like size, scale, origin or position.
    /// </summary>
    public class Sprite : UpdatableStateObject, IInitializable, IContentLoadable
    {
        private Game game;
        private PTTexture2D texture;
        private Vector2 position;
        private Vector2 origin;
        private Vector2 scale;
        private float rotation;
        private readonly string texturePath;
        private SharpDX.Direct2D1.DeviceContext deviceContext;

        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        /// <inheritdoc/>
        public event EventHandlers.OnInitializedHandler OnInitialized;

        #region Properties
        /// <inheritdoc/>
        public bool IsContentLoaded { get; set; }

        /// <inheritdoc/>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// The <see cref="PTTexture2D"/> that holds the image to be drawn.
        /// </summary>
        public PTTexture2D Texture
        {
            get { return texture; }
            set { SetPropertyAsDirty(ref texture, value); }
        }

        /// <summary>
        /// Position of the sprite.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Origin of the sprite, to create rotations and other calculations.
        /// This value is a x,y coordinate taken from the original size of the texture.
        /// The value set must be in reference of the original size of the texture.
        /// When obtained, it is multiplied by the scale.
        /// </summary>
        public Vector2 Origin
        {
            get { return origin * scale; }
            set { SetPropertyAsDirty(ref origin, value); }
        }

        /// <summary>
        /// The scale of the sprite.
        /// This is the property used to change the size of the Sprite.
        /// </summary>
        public Vector2 Scale
        {
            get { return scale; }
            set { SetPropertyAsDirty(ref scale, value); }
        }

        /// <summary>
        /// Get the original size of the texture representing the sprite.
        /// </summary>
        public Vector2 Size
        {
            get { return texture.Size; }
        }

        /// <summary>
        /// Rotation of the sprite.
        /// </summary>
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// The <see cref="SharpDX.Direct2D1.DeviceContext"/> used to render this sprite into.
        /// </summary>
        public SharpDX.Direct2D1.DeviceContext DeviceContext
        {
            get { return deviceContext; }
            internal set { deviceContext = value; }
        }

        /// <summary>
        /// Path where the texture is held for this <see cref="Sprite"/>.
        /// </summary>
        public string TexturePath
        {
            get { return texturePath; }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public Sprite(Game game, string texturePath)
        {
            this.texturePath = texturePath;
            this.game = game;

            position = Vector2.Zero;
            origin = Vector2.Zero;
            scale = new Vector2(1, 1);
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public void LoadContent(IContentManager contentManager)
        {
            Texture = game.ContentManager.LoadTexture2D(texturePath, DeviceContext);
            ToDispose(Texture);

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
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
