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
    public interface IGameService : IDisposable
    {
        #region Properties
        /// <summary>
        /// Property set to true when the service has finished initialization
        /// </summary>
        bool IsInitialized { get; set; }

        /// <summary>
        /// The name of the service. (Optional)
        /// </summary>
        string Name { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Method to initialize all the necessary objects of the service.
        /// </summary>
        void Initialize();
        /// <summary>
        /// Main method to update the logic of the service.
        /// </summary>
        void Update();
        #endregion
    }
}
