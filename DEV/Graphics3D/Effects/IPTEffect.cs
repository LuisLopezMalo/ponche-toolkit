using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Graphics2D.Effects;
using PoncheToolkit.Graphics3D;
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
    /// Main interface to implement to create an effect.
    /// This interface has properties that DirectX11 and DirectX12 share.
    /// </summary>
    public interface IPTEffect : IInitializable, IContentLoadable, IContentItem
    {
        /// <summary>
        /// The game instance.
        /// </summary>
#if DX11
        Game11 Game { get; }
#elif DX12
        Game12 Game { get; }
#endif

        /// <summary>
        /// The materials dictionary that this effect have.
        /// The key is the name of the material, there cannot be two materials with the same name.
        /// </summary>
        //IReadOnlyDictionary<string, PTMaterial> Materials { get; }
        SortedList<int, KeyValuePair<string, PTMaterial>> Materials { get; }

        #region Methods

        #endregion
    }
}