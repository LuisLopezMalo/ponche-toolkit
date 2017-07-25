using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Interface to set the Initialize method.
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Method to initialize all the necessary objects.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Property set to true when the component has finished initialization
        /// </summary>
        bool IsInitialized { get; set; }

        /// <summary>
        /// Event raised when finished initialization.
        /// It is recommended to add any other functionality for initialization using this event,
        /// to ensure that the initialization has completed.
        /// </summary>
        event EventHandlers.OnInitializedHandler OnInitialized;
    }
}
