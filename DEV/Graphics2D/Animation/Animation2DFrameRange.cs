using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics2D.Animation
{
    /// <summary>
    /// Represents the frame range to be selected in a Texture.
    /// </summary>
    public struct Animation2DFrameRange
    {
        /// <summary>
        /// The 'X' position of the first frame.
        /// </summary>
        public int FirstFrameX;
        /// <summary>
        /// The 'Y' position of the last frame.
        /// </summary>
        public int FirstFrameY;
        /// <summary>
        /// The 'X' position of the first frame.
        /// </summary>
        public int LastFrameX;
        /// <summary>
        /// The 'Y' position of the last frame.
        /// </summary>
        public int LastFrameY;

        /// <summary>
        /// Width of the frames.
        /// </summary>
        public int FrameWidth;
        /// <summary>
        /// Height of the frames.
        /// </summary>
        public int FrameHeight;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="firstFrameX">The 'X' position of the first frame.</param>
        /// <param name="firstFrameY">The 'Y' position of the last frame.</param>
        /// <param name="lastFrameX">The 'X' position of the first frame.</param>
        /// <param name="lastFrameY">The 'Y' position of the last frame.</param>
        /// <param name="frameWidth">The frame width.</param>
        /// <param name="frameHeight">The frame height.</param>
        public Animation2DFrameRange(int firstFrameX, int firstFrameY, int lastFrameX, int lastFrameY, int frameWidth, int frameHeight)
        {
            this.FirstFrameX = firstFrameX;
            this.FirstFrameY = firstFrameY;
            this.LastFrameX = lastFrameX;
            this.LastFrameY = lastFrameY;
            this.FrameWidth = frameWidth;
            this.FrameHeight = frameHeight;
        }
    }
}
