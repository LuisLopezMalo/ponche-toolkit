using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Core.Management.Content;
using System.Runtime.Serialization;

namespace PoncheToolkit.Core.Components
{
    /// <summary>
    /// Abstract class that implements a component that will only be updated and drawn.
    /// </summary>
    public abstract class GameRenderableComponent : GameComponent, IRenderable
    {
        // TODO
        ///// <summary>
        ///// Get or set the input values for the vertex shader. (InputLayout)
        ///// </summary>
        //public VertexShaderInputStruct? ShaderInputLayout { get; set; }

        /// <inheritdoc/>
        //public List<PTEffect> Effects { get; set; }
        public List<PTEffect> Effects { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
#if DX11
        public GameRenderableComponent(Game11 game)
#elif DX12
        public GameRenderableComponent(Game12 game)
#endif
            : base(game)
        {
            Effects = new List<PTEffect>();
        }

        #region Abstract Methods
        /// <inheritdoc/>
        public abstract void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null);
        #endregion

        /// <inheritdoc/>
        public override void LoadContent(IContentManager contentManager)
        {
            base.LoadContent(contentManager);
        }
    }
}
