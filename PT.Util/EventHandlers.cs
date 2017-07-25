using PoncheToolkit.Core.Management.Screen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Util
{
    /// <summary>
    /// Util class that contain EventHandlers to set into interfaces.
    /// </summary>
    public class EventHandlers
    {
        /// <summary>
        /// Handler to create custom Initialized event.
        /// </summary>
        public delegate void OnInitializedHandler();

        /// <summary>
        /// Handler to create custom Initialized event.
        /// </summary>
        public delegate void OnLoadContentHandler();

        /// <summary>
        /// Event raised when finished loading content.
        /// It is recommended to add any other functionality for loading content using this event,
        /// to ensure that the any previous loading has completed.
        /// </summary>
        public delegate void OnFinishLoadContentHandler();

        /// <summary>
        /// Event that must be raised last when implementing the <see cref="GameScreen.AddRenderableComponentWithEffect{T, Eff}(ref T, ref Eff, string)"/>
        /// method.
        /// </summary>
        public delegate void OnFinishLoadRenderableComponentsHandler();
    }
}
