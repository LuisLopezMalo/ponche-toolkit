using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Core.Management.Screen;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Cameras;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Graphics3D.Primitives;
using PoncheToolkit.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.EffectsCreator.Entities.SharpDX
{
    /// <summary>
    /// The screen that has the model(s) to be rendered and applied the shader.
    /// </summary>
    public class RenderScreen : GameScreen
    {
        private ThirdPersonCamera currentCamera;
        private Triangle triangle;
        private PTEffect effect;

        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadRenderableComponentsHandler OnFinishLoadRenderableComponents;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public RenderScreen(Game11 game)
            : base(game)
        {
            currentCamera = new ThirdPersonCamera(game);
            triangle = new Triangle(game);

            OnInitialized += ContentScreen_OnInitialized;
        }

        private void ContentScreen_OnInitialized()
        {
            Components.AddComponent<Camera>(currentCamera, "MainCamera");

            currentCamera.Position = new Vector3(0, 0, -20f);
            currentCamera.Target = triangle;
            triangle.Size = new Vector3(5, 5, 5);
            triangle.AcceptInput = true;
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                Log.Debug("UpdateState - updating state in screen - " + this.Name);
                IsStateUpdated = true;
                OnStateUpdated();
            }
            return IsStateUpdated;
        }

        /// <inheritdoc/>
        public override void LoadShadersAndMaterials(IContentManager contentManager)
        {
            effect = contentManager.LoadEffect<PTEffect>("Effects/PTTextureEffect.fx", null);
        }

        /// <inheritdoc/>
        public override void AddRenderableScreenComponents()
        {
            //AddRenderableComponentWithEffect(ref triangle, ref effect, "triangle1");
        }
    }
}
