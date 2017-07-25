using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Represent an object that has updatable components.
    /// That can be out of date for some reason.
    /// </summary>
    public interface IUpdatableState
    {
        #region Properties
        /// <summary>
        /// The value that represent if the object has the last possible value.
        /// This value must be updated inside the <see cref="UpdateState"/> method.
        /// </summary>
        bool IsStateUpdated { get; set; }
        #endregion

        /// <summary>
        /// Method to update the values to their last possible value.
        /// </summary>
        /// <returns>The boolean value if the state is updated or not.</returns>
        bool UpdateState();

        /// <summary>
        /// Event raised when it has been updated successfully.
        /// </summary>
        event EventHandler OnStateUpdatedEvent;
    }
}
