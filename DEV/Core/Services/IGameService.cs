using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Services
{
    /// <summary>
    /// Main service interface.
    /// In this first approach, all the services that will be included in the game will implement this interface.
    /// The services are objects that will live throughout all the game, and can be retrieved anywhere.
    /// All the services will automatically be updated every frame as necessary.
    /// </summary>
    public interface IGameService : IDisposable, ILoggable, IUpdatableLogic, IInitializable
    {
        #region Properties
        /// <summary>
        /// The name of the service. (Optional)
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The game instance.
        /// </summary>
#if DX11
        Game11 Game { get; set; }
#elif DX12
        Game12 Game { get; set; }
#endif
        #endregion
    }
}
