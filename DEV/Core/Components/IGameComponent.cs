using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Util;

namespace PoncheToolkit.Core.Components
{
    /// <summary>
    /// Main component interface.
    /// In this first approach, all the components that will be included in the game will implement
    /// this interface.
    /// </summary>
    /// <typeparam name="T">The type of the Children for this component.</typeparam>
    public interface IGameComponent : IDisposable, ILoggable, IInitializable, IContentLoadable, IInputReceivable, IUpdatableLogic
    {
        #region Properties
        /// <summary>
        /// The unique name of the component so it can be retrieved from the collection.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The instance of the DirectX11 game running.
        /// </summary>
#if DX11
        Game11 Game { get; set; }
#elif DX12
        Game12 Game { get; set; }
#endif
        #endregion

        #region Public Methods
        /// <summary>
        /// Just unload the content preserving the object reference.
        /// The Dispose method must destroy all the resources references used.
        /// </summary>
        void UnloadContent();
        #endregion
    }
}
