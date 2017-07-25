using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Graphics3D;

namespace PoncheToolkit.Core.Components
{
    /// <summary>
    /// Abstract class that implements a component that will only be updated and drawn.
    /// </summary>
    public abstract class GameDrawableComponent : GameComponent, IDrawable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public GameDrawableComponent(Game11 game)
            : base(game)
        {
        }

        #region Abstract Methods
        /// <inheritdoc/>
        public override abstract void Initialize();

        /// <summary>
        /// Method to load the shaders of the component.
        /// Is called before the <see cref="LoadContent"/>, so the LoadContent method has the
        /// shaders objects already initialized.
        /// </summary>
        public abstract void LoadShaders();

        /// <inheritdoc/>
        public override abstract void UnloadContent();

        /// <inheritdoc/>
        public override abstract void Update();

        /// <inheritdoc/>
        public override abstract void Dispose();

        /// <inheritdoc/>
        public abstract void Render();
        #endregion

        /// <inheritdoc/>
        public override void LoadContent()
        {
            base.LoadContent();
        }
    }
}
