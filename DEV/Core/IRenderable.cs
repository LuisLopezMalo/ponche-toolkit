using PoncheToolkit.Graphics2D;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Interface that represent an object that can be rendered on screen.
    /// </summary>
    public interface IRenderable
    {
        ///// <summary>
        ///// The effect used and applied to render this component.
        ///// This effect represents a physical .fx shader.
        ///// </summary>
        //PTForwardRenderEffect Effect { get; set; }

        /// <summary>
        /// The list of effectsused and applied to render this component.
        /// Each effect represents a physical .fx shader.
        /// This is the way to render "Multi-pass" effects. Just add as many effects as multi-passes needed.
        /// </summary>
        List<PTEffect> Effects { get; set; }

        /// <summary>
        /// Render the content, it can be 2D using the <see cref="SpriteBatch"/> or 3D using the <see cref="SharpDX.Direct3D11.DeviceContext1"/>.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="context">The optional context to render, used for deferred rendering.</param>
        void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null);
    }
}
