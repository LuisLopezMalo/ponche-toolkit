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
    /// Save the parameters that are out of date and update them within the update state method.
    /// </summary>
    public abstract class UpdatableState
    {
        #region Properties
        /// <summary>
        /// The value that represent if the object has the last possible value.
        /// </summary>
        public bool IsUpdated { get; set; }
        #endregion

        /// <summary>
        /// Method to update the values to their last possible value.
        /// </summary>
        /// <returns>The boolean value if the state is updated or not.</returns>
        public abstract bool UpdateState();

        /// <summary>
        /// Event raised when a property has changed and does not represent the last value.
        /// </summary>
        public event Util.DelegateHandlers.IUpdatableStateOnValueChangedHandler OnValueChanged;

        /// <summary>
        /// Event raised when it has been updated successfully.
        /// </summary>
        public event EventHandler OnUpdatedState;

        /// <summary>
        /// Hold the objects that will be updated when the <see cref="UpdateState"/> method is called.
        /// </summary>
        public Dictionary<string, List<object>> UpdatableObjects;

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public UpdatableState()
        {
            UpdatableObjects = new Dictionary<string, List<object>>();
            this.OnValueChanged += UpdatableState_OnValueChanged;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Raised when a value has changed.
        /// Update the state of the object.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        private void UpdatableState_OnValueChanged(string propertyName, object value)
        {
            IsUpdated = false;

            // If the key exist, add the value to the list of objects.
            if (UpdatableObjects.ContainsKey(propertyName.ToLower()))
            {
                UpdatableObjects[propertyName.ToLower()].Add(value);
                return;
            }

            // If it does not exist, create a list with the object value inside.
            UpdatableObjects.Add(propertyName.ToLower(), new List<object>() { value });
        }
        #endregion
    }
}
