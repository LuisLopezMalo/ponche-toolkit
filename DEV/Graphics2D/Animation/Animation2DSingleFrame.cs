using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics2D.Animation
{
    /// <summary>
    /// Represent a single drawing taken from an Animation.
    /// </summary>
    public class Animation2DSingleFrame
    {
        private float duration;
        private RectangleF sourceRectangle;

        /// <summary>
        /// How much time this animation frame last.
        /// </summary>
        public float Duration
        {
            get { return duration; }
            set { duration = 1.0f / value; }
        }

        /// <summary>
        /// Get or set the current source rectangle to draw the current frame animation.
        /// This property represent the current Frame being rendered.
        /// This current animation is updated in the <see cref="Animation2DManager"/>.
        /// </summary>
        public RectangleF SourceRectangle
        {
            get { return sourceRectangle; }
            set { sourceRectangle = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Animation2DSingleFrame(float duration)
        {
            this.duration = 1.0f / duration;
        }
    }
}
