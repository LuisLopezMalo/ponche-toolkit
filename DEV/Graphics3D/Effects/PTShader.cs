using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// Class that wraps shader functionality like type (Vertex, Pixel, Hull, etc)
    /// And contain other properties like ShaderSignature.
    /// </summary>
    public class PTShader : UpdatableStateObject, IContentLoadable
    {
        /// <summary>
        /// The shader signature.
        /// </summary>
        public ShaderSignature VertexShaderSignature;
        /// <summary>
        /// The pixel shader compiled object.
        /// </summary>
        public PixelShader PixelShader;
        /// <summary>
        /// The vertex shader compiled object.
        /// </summary>
        public VertexShader VertexShader;
        /// <summary>
        /// The content to send to the gpu in its first stage: Input Layout.
        /// </summary>
        public InputLayout InputLayout;
        /// <summary>
        /// The linkage to create abstract effects.
        /// </summary>
        public ClassLinkage Linkage;
        /// <summary>
        /// The slot of the custom pixel shader to be used.
        /// If no custom pixel shader will be used, this value is -1.
        /// 0 index based.
        /// </summary>
        public int CustomPixelShaderSlot;
        /// <summary>
        /// The physical paths where the .fx files are.
        /// These dictionary can have all the paths for the different shaders like vertex, pixel, tessel, etc.
        /// </summary>
        public Dictionary<ShaderType, string> Paths;

        /// <summary>
        /// Abstract declarations of HLSL instances that will permute their values dynamically.
        /// </summary>
        public ClassInstance[] ClassInstances;

        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;

        /// <inheritdoc/>
        public bool IsContentLoaded { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTShader()
        {
            Paths = new Dictionary<ShaderType, string>();
            CustomPixelShaderSlot = -1;
            //ClassInstances = new ClassInstance[1 + 1 + PTLight.FORWARD_SHADING_MAX_LIGHTS]; // Material + GlobalData + Lights
            //ClassInstances = new ClassInstance[1 + 1 + 1]; // Material + GlobalData + Lights
        }

        /// <inheritdoc/>
        public void LoadContent(IContentManager contentManager)
        {
            Linkage = new ClassLinkage(contentManager.Game.Renderer.Device);

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Utilities.Dispose(ref VertexShaderSignature);
            Utilities.Dispose(ref PixelShader);
            Utilities.Dispose(ref VertexShader);
            Utilities.Dispose(ref InputLayout);
            Utilities.Dispose(ref Linkage);
            Paths.Clear();
            if (ClassInstances != null)
                Array.Clear(ClassInstances, 0, ClassInstances.Length);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            throw new NotImplementedException();
        }
    }
}
