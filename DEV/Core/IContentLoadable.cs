using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Interface to set the LoadContent method.
    /// </summary>
    public interface IContentLoadable
    {
        /// <summary>
        /// Get or set if the content has finished loading.
        /// </summary>
        bool IsContentLoaded { get; set; }

        /// <summary>
        /// Method to load all the content of the component.
        /// The content is any file that must be compiled or set into memory like
        /// textures, models, etc.
        /// </summary>
        /// <param name="contentManager">The implementation of the <see cref="IContentManager"/> interface.</param>
        void LoadContent(IContentManager contentManager);

        /// <summary>
        /// Event raised when finished loading content.
        /// It is recommended to add any other functionality for loading content using this event,
        /// to ensure that the any previous loading has completed.
        /// </summary>
        event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
    }
}
