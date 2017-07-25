using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics2D.Effects
{
    /// <summary>
    /// Properties for the 2D shadow map creation effect.
    /// </summary>
    public enum PTShadowMapEffectProperties
    {
        /// <summary>
        /// Property to upscale the shadow map.
        /// </summary>
        Upscale = 0,
        /// <summary>
        /// Property to set the resolution of the shadow map.
        /// </summary>
        Resolution = 1,
    }

    /// <summary>
    /// Properties for the 2D shadow render effect.
    /// </summary>
    public enum PTShadowRenderEffectProperties
    {
        /// <summary>
        /// Color of the shadow
        /// </summary>
        Color = 0,
        /// <summary>
        /// The resolution of the texture to create the shadows.
        /// </summary>
        Resolution = 1,
        /// <summary>
        /// If the 2D shadow renderer cast soft shadows
        /// </summary>
        SoftShadows = 2,
    }

    /// <summary>
    /// Properties for the 2D ripple effect
    /// </summary>
    public enum PTRippleEffectProperties
    {
        /// <summary>
        /// Frequency of the ripple.
        /// </summary>
        Frequency = 0,
        /// <summary>
        /// Phase of the ripple.
        /// </summary>
        Phase = 1,
        /// <summary>
        /// Amplitude of the ripple.
        /// </summary>
        Amplitude = 2,
        /// <summary>
        /// Spread of the ripple.
        /// </summary>
        Spread = 3,
        /// <summary>
        /// Center of the ripple.
        /// </summary>
        Center = 4
    }

    /// <summary>
    /// Properties for the 2D edge detection effect
    /// </summary>
    public enum PTEdgeDetectionEffectProperties
    {
        /// <summary>
        /// Screen size to make calculations.
        /// </summary>
        ScreenSize = 0,
        /// <summary>
        /// Thickness of the outline.
        /// </summary>
        Thickness = 1,
        /// <summary>
        /// Threshold of the outline, if it is too low, the lines will look more 'edgy'.
        /// </summary>
        Threshold = 2
    }
}
