using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Graphics2D;
using SharpDX;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;

namespace PoncheToolkit.Graphics2D.Animation
{
    /// <summary>
    /// Class that manages a set of animations.
    /// Contains a dictionary of FrameAnimation objects.
    /// </summary>
    public class Animation2DManager : GameService
    {
        private Dictionary<string, AnimatedSprite> spriteSheets;

        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public Animation2DManager(Game game)
            : base(game)
        {
        }

        #region Properties
        /// <summary>
        /// The current sprite sheets loaded.
        /// </summary>
        public Dictionary<string, AnimatedSprite> SpriteSheets
        {
            get { return spriteSheets; }
            set { spriteSheets = value; }
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            spriteSheets = new Dictionary<string, AnimatedSprite>();

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Creates a new FrameAnimation and adds it to the Animations dictionary.
        /// This method uses the <see cref="ContentManager11.LoadTexture2D(string, string)"/> method so it is recommended to be used in the LoadContent method.
        /// </summary>
        /// <param name="spriteSheetName">The key to distinguish the animation, if it exists it reinitializes the animation with the given parameters.</param>
        /// <param name="textureAsset">Name of the texture to be loaded.</param>
        /// <param name="frameCount">The <see cref="Animation2DFrameCount"/> to load the animations of the spriteSheet.</param>
        /// <param name="animationFramesPerSecond">Frames per second for the animation.</param>
        /// <param name="deviceContext">The Device context that will be used to create the content and where the rendering will take place. 
        /// If it is not assigned the default <see cref="PoncheToolkit.Graphics3D.GraphicsRenderer.Context2D"/> is used, this context renders to the back buffer.</param>
        public AnimatedSprite LoadSpriteSheet(string spriteSheetName, string textureAsset, Animation2DFrameCount frameCount, int animationFramesPerSecond, SharpDX.Direct2D1.DeviceContext deviceContext = null)
        {
            spriteSheetName = spriteSheetName.ToLower();

            AnimatedSprite spriteSheet = new AnimatedSprite(Game, spriteSheetName, textureAsset, deviceContext);
            spriteSheet.LoadContent(Game.ContentManager);
            
            int frameWidth = spriteSheet.Width / frameCount.NumberOfColumns;
            spriteSheet.FramesPerRow = spriteSheet.Width / frameWidth;

            if (spriteSheets.ContainsKey(spriteSheetName))
                spriteSheets[spriteSheetName] = spriteSheet;
            else
                spriteSheets.Add(spriteSheetName, spriteSheet);
            
            return spriteSheet;
        }

        /// <summary>
        /// Creates a new FrameAnimation and adds it to the Animations dictionary.
        /// This method uses the <see cref="ContentManager11.LoadTexture2D(string, string)"/> method so it is recommended to be used in the LoadContent method.
        /// </summary>
        /// <param name="spriteSheetName">The key to distinguish the animation, if it exists it reinitializes the animation with the given parameters.</param>
        /// <param name="textureAsset">Name of the texture to be loaded.</param>
        /// <param name="frameWidth">Width of each frame.</param>
        /// <param name="frameHeight">Height of each frame.</param>
        /// <param name="numberOfFrames">Number of frames that conforms the animation.</param>
        /// <param name="animationFramesPerSecond">Frames per second for the animation.</param>
        public AnimatedSprite LoadSpriteSheet(string spriteSheetName, string textureAsset, int frameWidth, 
            int frameHeight, int numberOfFrames, int animationFramesPerSecond)
        {
            spriteSheetName = spriteSheetName.ToLower();
            AnimatedSprite spriteSheet = new AnimatedSprite(Game, spriteSheetName, textureAsset);
            
            spriteSheet.LoadContent(Game.ContentManager);
            spriteSheet.FramesPerRow = spriteSheet.Width / frameWidth;
            
            if (spriteSheets.ContainsKey(spriteSheetName))
                spriteSheets[spriteSheetName.ToLower()] = spriteSheet;
            else
                spriteSheets.Add(spriteSheetName.ToLower(), spriteSheet);

            return spriteSheet;
        }

        /// <summary>
        /// Creates a new <see cref="Animation2D"/> from a <see cref="AnimatedSprite"/>.
        /// </summary>
        /// <param name="spriteSheet">The <see cref="AnimatedSprite"/> object to own this animation.</param>
        /// <param name="animationName">The name of the animation. This is unique per <see cref="AnimatedSprite"/></param>
        /// <param name="frameCount">The <see cref="Animation2DFrameCount"/> count to extract the animation.</param>
        /// <param name="animationFramesPerSecond">The number of animations per second.</param>
        /// <returns></returns>
        public Animation2D GenerateAnimation(AnimatedSprite spriteSheet, string animationName, Animation2DFrameCount frameCount, int animationFramesPerSecond)
        {
            int frameWidth = spriteSheet.Width / frameCount.NumberOfColumns;
            int frameHeight = spriteSheet.Height / frameCount.NumberOfRows;
            int numberOfFrames = frameCount.NumberOfColumns * frameCount.NumberOfRows;

            animationName = animationName.ToLower();
            Animation2DFrameRange range = new Animation2DFrameRange(1, 1, frameCount.NumberOfColumns, frameCount.NumberOfRows, frameWidth, frameHeight);
            Animation2D animation = new Animation2D(spriteSheet, animationName, range, animationFramesPerSecond);
            animation.Initialize();

            if (spriteSheet.Animations.ContainsKey(animationName))
                spriteSheet.Animations[animationName] = animation;
            else
                spriteSheet.Animations.Add(animationName, animation);

            return animation;
        }

        /// <summary>
        /// Creates a new <see cref="Animation2D"/> from a <see cref="AnimatedSprite"/>.
        /// </summary>
        /// <param name="animationName">The name of the animation. This is unique per <see cref="AnimatedSprite"/></param>
        /// <param name="spriteSheet">The <see cref="AnimatedSprite"/> object to own this animation.</param>
        /// <param name="frameRange">Range of frames that conforms the animation.</param>
        /// <param name="animationFramesPerSecond">The number of animations per second.</param>
        /// <returns></returns>
        public Animation2D GenerateAnimation(AnimatedSprite spriteSheet, string animationName, Animation2DFrameRange frameRange, int animationFramesPerSecond)
        {
            animationName = animationName.ToLower();
            
            Animation2D animation = new Animation2D(spriteSheet, animationName, frameRange, animationFramesPerSecond);
            animation.Initialize();

            if (spriteSheet.Animations.ContainsKey(animationName))
                spriteSheet.Animations[animationName] = animation;
            else
                spriteSheet.Animations.Add(animationName, animation);

            return animation;
        }

        /// <summary>
        /// Updates the time animation of a single frame within the FrameAnimation.
        /// </summary>
        /// <param name="spriteSheet">The <see cref="AnimatedSprite"/> owner.</param>
        /// <param name="animationKey">The key to distinguish the animation.</param>
        /// <param name="numberOfFrame">The specific number of frame</param>
        /// <param name="animationTime">The frames per second for that specific frame.</param>
        public void UpdateSingleFrameAnimationTime(AnimatedSprite spriteSheet, string animationKey, int numberOfFrame, float animationTime)
        {
            Animation2D animation = spriteSheet.Animations[animationKey.ToLower()];
            animation.Frames[numberOfFrame].Duration = animationTime;
        }

        /// <summary>
        /// Updates the time animation of a single frame within the FrameAnimation.
        /// </summary>
        /// <param name="spriteSheet">The <see cref="AnimatedSprite"/> owner.</param>
        /// <param name="animation">The <see cref="Animation2D"/> object.</param>
        /// <param name="numberOfFrame">The specific number of frame</param>
        /// <param name="animationTime">The frames per second for that specific frame.</param>
        public void UpdateSingleFrameAnimationTime(AnimatedSprite spriteSheet, Animation2D animation, int numberOfFrame, float animationTime)
        {
            animation.Frames[numberOfFrame].Duration = animationTime;
        }

        /// <summary>
        /// Updates the time animation of a single frame within the FrameAnimation.
        /// </summary>
        /// <param name="spriteSheet">The <see cref="AnimatedSprite"/> owner.</param>
        /// <param name="animationKey">The key to distinguish the animation.</param>
        /// <param name="column">Number of column for the wanted frame.</param>
        /// <param name="row">Number of row for the wanted frame.</param>
        /// <param name="animationTime">The frames per second for that specific frame.</param>
        public void UpdateSingleFrameAnimationTime(AnimatedSprite spriteSheet, string animationKey, int column, int row, float animationTime)
        {
            Animation2D animation = spriteSheet.Animations[animationKey.ToLower()];
            animation.Frames[column * row].Duration = animationTime;
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            foreach (AnimatedSprite spriteSheet in spriteSheets.Values)
            {
                foreach (Animation2D animation in spriteSheet.Animations.Values)
                {
                    if (animation.Paused)
                        continue;
                    
                    animation.TotalElapsedTime += Game.GameTime.DeltaTime;
                    if (animation.TotalElapsedTime > animation.CurrentFrame.Duration)
                    {
                        float lastAnimationTime = animation.CurrentFrame.Duration;
                        if (animation.Loop)
                            animation.CurrentFrameIndex = (++animation.CurrentFrameIndex) % animation.Frames.Count;
                        else
                            animation.CurrentFrameIndex = Math.Max(++animation.CurrentFrameIndex, animation.Frames.Count);
                        animation.TotalElapsedTime -= lastAnimationTime;

                        // Update the current source rectangle.
                        int x = (animation.CurrentFrameIndex % spriteSheet.FramesPerRow) * animation.FrameWidth;

                        int occupiedRows = (animation.CurrentFrameIndex / spriteSheet.FramesPerRow) == 0 ? 1 : (animation.CurrentFrameIndex / spriteSheet.FramesPerRow);
                        int currentRow = (animation.FrameRange.FirstFrameY / animation.FrameHeight) * occupiedRows;
                        int y = (currentRow * animation.FrameHeight);

                        animation.CurrentFrame.SourceRectangle = new RectangleF(x, y, animation.FrameWidth, animation.FrameHeight);
                    }
                }
            }
        }

        #region Draw by animation object
        /// <summary>
        /// Render the Current Animation for the  given <see cref="AnimatedSprite"/>.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, SharpDX.Direct2D1.DeviceContext context = null)
        {
            Draw(sprite, spriteBatch, gameTime, sprite.CurrentAnimation, sprite.Rotation, sprite.Scale, sprite.Origin, Color.White);
        }

        /// <summary>
        /// Render the given animation.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animation">The animation to render.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, Animation2D animation)
        {
            Draw(sprite, spriteBatch, gameTime, animation, sprite.Rotation, sprite.Scale, sprite.Origin, Color.White);
        }

        /// <summary>
        /// Render the given animation key.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animation">The animation to render.</param>
        /// <param name="rotation">The rotation of the sprite.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, Animation2D animation, float rotation)
        {
            Draw(sprite, spriteBatch, gameTime, animation, rotation, sprite.Scale, sprite.Origin, Color.White);
        }

        /// <summary>
        /// Render the given animation key.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animation">The animation to render.</param>
        /// <param name="rotation">The rotation of the sprite.</param>
        /// <param name="scale">The scale of the original texture size.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, Animation2D animation, float rotation, Vector2 scale)
        {
            Draw(sprite, spriteBatch, gameTime, animation, rotation, scale, sprite.Origin, Color.White);
        }

        /// <summary>
        /// Render the given animation key.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animation">The animation to render.</param>
        /// <param name="rotation">The rotation of the sprite.</param>
        /// <param name="scale">The origin of the sprite where the rotation is made.</param>
        /// <param name="origin">The origin of the sprite where the rotation is made.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, Animation2D animation, float rotation, Vector2 scale, Vector2 origin)
        {
            Draw(sprite, spriteBatch, gameTime, animation, rotation, scale, origin, Color.White);
        }
        #endregion

        #region Draw by animation Key
        /// <summary>
        /// Render the given animation key.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animationKey">The animation key to render.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, string animationKey)
        {
            Animation2D animation = sprite.Animations[animationKey];
            Draw(sprite, spriteBatch, gameTime, animationKey, sprite.Rotation, sprite.Scale, sprite.Origin, Color.White);
        }

        /// <summary>
        /// Render the given animation key.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animationKey">The animation key to render.</param>
        /// <param name="rotation">The rotation of the sprite.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, string animationKey, float rotation)
        {
            Draw(sprite, spriteBatch, gameTime, animationKey, rotation, sprite.Scale, sprite.Origin, Color.White);
        }

        /// <summary>
        /// Render the given animation key.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animationKey">The animation key to render.</param>
        /// <param name="rotation">The rotation of the sprite.</param>
        /// <param name="scale">The scale of the original texture size.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, string animationKey, float rotation, Vector2 scale)
        {
            Animation2D animation = sprite.Animations[animationKey];
            Draw(sprite, spriteBatch, gameTime, animationKey, rotation, scale, sprite.Origin, Color.White);
        }

        /// <summary>
        /// Render the given animation key.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animationKey">The animation key to render.</param>
        /// <param name="rotation">The rotation of the sprite.</param>
        /// <param name="scale">The origin of the sprite where the rotation is made.</param>
        /// <param name="origin">The origin of the sprite where the rotation is made.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, string animationKey, float rotation, Vector2 scale, Vector2 origin)
        {
            Draw(sprite, spriteBatch, gameTime, animationKey, rotation, scale, origin, Color.White);
        }

        /// <summary>
        /// Render the given animation key.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animationKey">The animation key to render.</param>
        /// <param name="rotation">The rotation of the sprite.</param>
        /// <param name="scale">The origin of the sprite where the rotation is made.</param>
        /// <param name="origin">The origin of the sprite where the rotation is made.</param>
        /// <param name="color">The color to tint the texture.</param>
        /// <param name="opacity">The opacity of the texture.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, string animationKey, float rotation, Vector2 scale, Vector2 origin, Color color, float opacity = 1)
        {
            if (!sprite.Animations.ContainsKey(animationKey))
                return;

            Animation2D animation = sprite.Animations[animationKey];
            Draw(sprite, spriteBatch, gameTime, animation, rotation, scale, origin, Color.White);
        }

        /// <summary>
        /// Render the given animation key.
        /// </summary>
        /// <param name="sprite">The <see cref="AnimatedSprite"/> instance.</param>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> instance that holds the current elapsed delta time.</param>
        /// <param name="animation">The animation to render.</param>
        /// <param name="rotation">The rotation of the sprite.</param>
        /// <param name="scale">The origin of the sprite where the rotation is made.</param>
        /// <param name="origin">The origin of the sprite where the rotation is made.</param>
        /// <param name="color">The color to tint the texture.</param>
        /// <param name="opacity">The opacity of the texture.</param>
        public void Draw(AnimatedSprite sprite, SpriteBatch spriteBatch, GameTime gameTime, Animation2D animation, float rotation, Vector2 scale, Vector2 origin, Color color, float opacity = 1)
        {
            //int x = (animation.CurrentFrameIndex % sprite.FramesPerRow) * animation.FrameWidth;

            //int occupiedRows = (animation.CurrentFrameIndex / sprite.FramesPerRow) == 0 ? 1 : (animation.CurrentFrameIndex / sprite.FramesPerRow);
            //int currentRow = (animation.FrameRange.FirstFrameY / animation.FrameHeight) * occupiedRows;
            //int y = (currentRow * animation.FrameHeight);

            //RectangleF source = new RectangleF(x, y, animation.FrameWidth, animation.FrameHeight);
            //spriteBatch.DrawTexture(sprite.Texture, sprite.Position, source, color, rotation, origin, scale, opacity);
            spriteBatch.DrawTexture(sprite.Texture, sprite.Position, animation.CurrentFrame.SourceRectangle, color, rotation, origin, scale, opacity);
        }
        #endregion

        /// <inheritdoc/>
        public override void Dispose()
        {
            foreach (AnimatedSprite sheet in spriteSheets.Values)
                sheet.Dispose();

            spriteSheets.Clear();
        }
        #endregion
    }
}
