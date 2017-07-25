using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Represent an object that has properties that can be notified when changed.
    /// </summary>
    public interface IUpdatableProperties
    {
        #region Properties
        /// <summary>
        /// Event raised when a property has been changed.
        /// </summary>
        event EventHandler OnPropertyChangedEvent;

        /// <summary>
        /// List with the properties that have changed.
        /// </summary>
        //List<string> DirtyProperties { get; set; }
        Dictionary<string, object> DirtyProperties { get; set; }
        #endregion
    }
}
