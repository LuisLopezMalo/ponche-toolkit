using PoncheToolkit.Core;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;

namespace PoncheToolkit.Graphics2D.Animation
{
    /// <summary>
    /// Class that represent a sprite sheet animations.
    /// </summary>
    public class AnimatedSprite : Sprite
    {
        #region Fields
        private string spriteSheetName;
        private int framesPerRow;
        
        private Animation2D currentAnimation;
        private Dictionary<string, Animation2D> animations = new Dictionary<string, Animation2D>();
        #endregion

        #region Properties
        /// <summary>
        /// Get or set the current single frame animation.
        /// This property represent the current Frame being rendered.
        /// This current animation is updated in the <see cref="Animation2DManager"/>.
        /// </summary>
        public Animation2D CurrentAnimation
        {
            get { return currentAnimation; }
            set { currentAnimation = value; }
        }

        /// <summary>
        /// Get the current Animation total time. For a complete loop.
        /// </summary>
        public float CurrentAnimationTime
        {
            get { return currentAnimation.TotalLoopDuration; }
        }

        /// <summary>
        /// Get the frames per row to read from the sprite sheet.
        /// </summary>
        public int FramesPerRow
        {
            get { return framesPerRow; }
            internal set { SetProperty(ref framesPerRow, value); }
        }

        /// <summary>
        /// Get the dictionary with the animations for this spriteSheet.
        /// Each animation is distinguished by its name.
        /// </summary>
        public Dictionary<string, Animation2D> Animations
        {
            get { return animations; }
        }

        /// <summary>
        /// Get the width of the spritesheet. Obtained from the texture loaded.
        /// </summary>
        public int Width { get { return Texture.Width; } }

        /// <summary>
        /// Get the height of the spritesheet. Obtained from the texture loaded.
        /// </summary>
        public int Height { get { return Texture.Height; } }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="spriteSheetName">Name of the spriteSheet.</param>
        /// <param name="texturePath">Path of the texture.</param>
        /// <param name="deviceContext">The device context to render the sprite.</param>
        internal AnimatedSprite(Game game, string spriteSheetName, string texturePath, SharpDX.Direct2D1.DeviceContext deviceContext = null)
            : base(game, texturePath)
        {
            this.spriteSheetName = spriteSheetName;
            this.DeviceContext = deviceContext;
        }
        #endregion

        #region Public Methods
        

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();

            animations.Clear();
        }
        #endregion
    }
}
