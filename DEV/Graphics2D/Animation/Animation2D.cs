using PoncheToolkit.Core;
using PoncheToolkit.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics2D.Animation
{
    /// <summary>
    /// Class that represents a single frame image from the texture. 
    /// Has its own properties as animationTime to be able to set individual times per frame or color.
    /// </summary>
    public class Animation2D : IInitializable
    {
        private readonly Animation2DFrameRange frameRange;
        private readonly int animationsPerSecond;
        private string name;
        private float totalLoopDuration;
        private int currentFrameIndex;
        private bool paused;
        private bool loop;
        private int frameWidth;
        private int frameHeight;
        
        private float totalElapsedTime;

        private Color color;
        private AnimatedSprite spriteSheet;
        private List<Animation2DSingleFrame> frames;
        private Animation2DSingleFrame currentFrame;

        /// <inheritdoc/>
        public event EventHandlers.OnInitializedHandler OnInitialized;

        /// <summary>
        /// How much time this animation last.
        /// Sum the duration of all the frames.
        /// </summary>
        public float TotalLoopDuration
        {
            get
            {
                float duration = 0;
                foreach (Animation2DSingleFrame frame in frames)
                    duration += frame.Duration;
                return duration;
            }
        }

        /// <summary>
        /// The name to distinguish the animation in the spritesheet.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// The number of animationFrames to show per second.
        /// This represent how fast the animation will be.
        /// </summary>
        public float Fps
        {
            get { return animationsPerSecond; }
        }

        /// <summary>
        /// The color (tint) for this single animation.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// The frame width to get the animation from the <see cref="AnimatedSprite"/>.
        /// </summary>
        public int FrameWidth
        {
            get { return frameWidth; }
            set { frameWidth = value; }
        }

        /// <summary>
        /// The frame height to get the animation from the <see cref="AnimatedSprite"/>.
        /// </summary>
        public int FrameHeight
        {
            get { return frameHeight; }
            set { frameHeight = value; }
        }

        /// <summary>
        /// Get or set the index of the current frame being rendered.
        /// </summary>
        public int CurrentFrameIndex
        {
            get { return currentFrameIndex; }
            set { currentFrameIndex = value; }
        }

        /// <summary>
        /// The total elapsed time for the animation
        /// </summary>
        public float TotalElapsedTime
        {
            get { return totalElapsedTime; }
            set { totalElapsedTime = value; }
        }

        /// <summary>
        /// The <see cref="FrameRange"/> from which the animation is taken.
        /// </summary>
        public Animation2DFrameRange FrameRange
        {
            get { return frameRange; }
        }

        /// <summary>
        /// Get the set of single drawings that represent this Animation.
        /// </summary>
        public List<Animation2DSingleFrame> Frames
        {
            get { return frames; }
        }

        /// <summary>
        /// Get the value that indicates if the animation is paused.
        /// </summary>
        public bool Paused
        {
            get { return paused; }
        }

        /// <summary>
        /// Get the value that indicates if the animation is looped.
        /// </summary>
        public bool Loop
        {
            get { return loop; }
            set { loop = value; }
        }

        /// <summary>
        /// Get or set the current single frame animation.
        /// This property represent the current Frame being rendered.
        /// This current frame is updated in the <see cref="Animation2DManager"/>.
        /// </summary>
        public Animation2DSingleFrame CurrentFrame
        {
            get { return frames[currentFrameIndex]; }
            set { currentFrame = value; }
        }

        /// <inheritdoc/>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <param name="spriteSheet">The <see cref="AnimatedSprite"/> owner.</param>
        /// <param name="frameRange">Range of images to be drawn from the texture.</param>
        /// <param name="animationsPerSecond">Default number of animations per second. (How fluid the animation will be).
        /// With a bigger number, the animations will be faster.</param>
        public Animation2D(AnimatedSprite spriteSheet, string name, Animation2DFrameRange frameRange, int animationsPerSecond)
        {
            this.name = name;
            this.spriteSheet = spriteSheet;
            this.animationsPerSecond = animationsPerSecond;
            this.frameRange = frameRange;
            //this.timePerFrame = 1.0f / animationsPerSecond;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            this.color = Color.White;
            this.frames = new List<Animation2DSingleFrame>();
            this.totalElapsedTime = 0;
            this.paused = true;
            this.loop = true;
            this.frameWidth = frameRange.FrameWidth;
            this.frameHeight = frameRange.FrameHeight;

            int columns = (frameRange.LastFrameX - frameRange.FirstFrameX) / frameWidth;
            int rows = (frameRange.LastFrameY - frameRange.FirstFrameY) / frameHeight;

            columns = columns == 0 ? 1 : columns + 1;
            rows = rows == 0 ? 1 : rows + 1;

            for (int i = 0; i < columns * rows; i++)
            {
                //int column = i % spriteSheet.FramesPerRow;
                //int row = i / spriteSheet.FramesPerRow;
                //string current = column.ToString() + row.ToString();
                frames.Add(new Animation2DSingleFrame(animationsPerSecond));
            }

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Set the frame time value for a single frame.
        /// </summary>
        /// <param name="columnRowNumber">The number of the frame.</param>
        /// <param name="frameTime">The new frame time.</param>
        public void SetSingleFrameTime(int columnRowNumber, float frameTime)
        {
            frames[columnRowNumber].Duration = 1.0f / frameTime;
        }

        /// <summary>
        /// Set the frame time value for a single frame.
        /// </summary>
        /// <param name="column">The number of the frame.</param>
        /// <param name="row">The number of the frame.</param>
        /// <param name="frameTime">The new frame time.</param>
        public void SetSingleFrameTime(int column, int row, float frameTime)
        {
            frames[column * row].Duration = 1.0f / frameTime;
        }

        /// <summary>
        /// Pause the animation.
        /// </summary>
        public void Pause()
        {
            paused = true;
        }

        /// <summary>
        /// Resume the animation.
        /// </summary>
        public void Resume()
        {
            paused = false;
        }

        /// <summary>
        /// Converts a number of frame to its representation in ColumnRow format.
        /// </summary>
        /// <example>
        /// Example: Texture with 4 columns and 3 rows.
        /// 0 1 2  3
        /// 4 5 6  7
        /// 8 9 10 11
        /// If it is passed a 5, is returned a 11. (First number for column, second number for row)
        /// </example>
        /// <param name="numberOfFrame">The number of frame from the texture, starting from the left up corner in 0 index format.</param>
        /// <returns>Representation of the number of frame in ColumnRow format.</returns>
        public string ConvertToColumnRow(int numberOfFrame)
        {
            int column = numberOfFrame % spriteSheet.FramesPerRow;
            int row = numberOfFrame / spriteSheet.FramesPerRow;
            string result = column.ToString() + row.ToString();
            return result;
        }

        /// <summary>
        /// Converts a number of frame to its representation in ColumnRow format.
        /// Example: texture with 4 columns and 3 rows.
        /// 0 1  2  3
        /// 4 5  6  7
        /// 8 9 10 11
        /// It receives the number of column and the number of row, just returns the representing string.
        /// </summary>
        /// <param name="column">The column of the frame.</param>
        /// <param name="row">The row of the frame.</param>
        /// <returns>Representation of the number of frame in ColumnRow format.</returns>
        public string ConvertToColumnRow(int column, int row)
        {
            string result = column.ToString() + row.ToString();
            return result;
        }
    }
}
