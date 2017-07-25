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
    public interface IGameComponent : IDisposable, ILoggable
    {
        #region Properties
        /// <summary>
        /// Property set to true when the component has finished initialization
        /// </summary>
        bool IsInitialized { get; set; }

        /// <summary>
        /// The unique name of the component so it can be retrieved from the collection.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The instance of the DirectX11 game running.
        /// </summary>
        Game11 Game { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Method to initialize all the necessary objects of the component.
        /// </summary>
        void Initialize();
        /// <summary>
        /// Method to load all the content of the component.
        /// The content is any file that must be compiled or set into memory like
        /// textures, models, etc.
        /// </summary>
        void LoadContent();
        /// <summary>
        /// Just unload the content preserving the object reference.
        /// The Dispose method must destroy all the resources references used.
        /// </summary>
        void UnloadContent();
        /// <summary>
        /// Main method to update the logic of the component.
        /// </summary>
        void Update();
        #endregion
    }
}
