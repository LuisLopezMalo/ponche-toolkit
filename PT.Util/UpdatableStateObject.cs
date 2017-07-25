using PoncheToolkit.Core;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PT.Util
{
    /// <summary>
    /// Class that represent an object that can have properties that will be notified upon any change.
    /// The new properties are added to the <see cref="DirtyProperties"/> list so they can be updated when the <see cref="UpdateState"/> method is called.
    /// When inheriting from this class, all the properties set with the <see cref="SetPropertyAsDirty{T}(ref T, T, string, bool)"/> will be automatically
    /// set as changed, so the <see cref="Game11"/> class will update its status as soon as possible.
    /// If a manual check need to be made for a determined property, it must be made using the <see cref="DirtyProperties"/> dictionary, 
    /// and the key to search can be made using the C# 6 'nameof(Property)' expression.
    /// </summary>
    public abstract class UpdatableStateObject : IUpdatableState, IUpdatableProperties, IDisposable, ILoggable
    {
        #region Fields
        private Dictionary<string, object> dirtyProperties;
        private bool isUpdated;
        private Game11 game;
        private List<IDisposable> disposableObjects;
        #endregion

        #region Properties
        /// <inheritdoc/>
        //public List<string> DirtyProperties { get { return dirtyProperties; } set { SetPropertyAsDirty(ref dirtyProperties, value); } }
        public Dictionary<string, object> DirtyProperties { get { return dirtyProperties; } set { SetPropertyAsDirty(ref dirtyProperties, value); } }

        /// <inheritdoc/>
        public bool IsStateUpdated { get { return isUpdated; } set { isUpdated = value; } }

        /// <inheritdoc/>
        public Logger Log { get; set; }
        #endregion

        #region Events
        /// <inheritdoc/>
        public event EventHandler OnPropertyChangedEvent;
        /// <inheritdoc/>
        public event EventHandler OnStateUpdatedEvent;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public UpdatableStateObject()
        {
            Log = new Logger(GetType());
            dirtyProperties = new Dictionary<string, object>();
            disposableObjects = new List<IDisposable>();

            // By default when a property is set, the IsUpdated property change to false.
            OnPropertyChangedEvent += (sender, e) =>
            {
                IsStateUpdated = false;
            };

            // Clear the dirty properties when the OnStateUpdated event has been called.
            OnStateUpdatedEvent += (sender, e) =>
            {
                dirtyProperties.Clear();
                Game11.RemoveDirtyObject(this);
            };
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public abstract bool UpdateState();

        /// <summary>
        /// Add an element to the list of objects that will be destroyed when the Dispose method is called.
        /// </summary>
        /// <param name="disposable"></param>
        public T ToDispose<T>(T disposable) where T : IDisposable
        {
            if (disposable != null && !disposableObjects.Contains(disposable))
                this.disposableObjects.Add(disposable);

            return disposable;
        }

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// If the property is new, it will be set as dirty, and added using the <see cref="Game11.AddDirtyObject(IUpdatableState)"/> method.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="field">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers that
        ///     support CallerMemberName.
        /// </param>
        /// <param name="addDirtyObject">Set if when changing a property, it adds this UpdatableObject to the list of DirtyObjects
        /// from the <see cref="Game11"/> instance. There the dirty objects will automatically update their dirty properties.
        /// This can be set to false when changing properties in the <see cref="Game11"/> class, so it is not looped inside the UpdateState method.</param>
        /// <returns>
        ///     True if the value was changed, false if the existing value matched the
        ///     desired value.
        /// </returns>
        protected bool SetPropertyAsDirty<T>(ref T field, T value, [CallerMemberName] string propertyName = null, bool addDirtyObject = true)
        {
            if (Equals(field, value))
                return false;

            field = value;
            this.addDirtyProperty(propertyName, ref field, addDirtyObject);
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Just set the property without any other validation or notification.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="field">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers that
        ///     support CallerMemberName.
        /// </param>
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            this.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Method to call the property changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(object sender = null, [CallerMemberName] string propertyName = null)
        {
            OnPropertyChangedEvent?.Invoke(sender == null ? this : sender, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Method to call the State Updated event.
        /// This event add the object to the DirtyObjects of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnStateUpdated(EventArgs args = null)
        {
            OnStateUpdatedEvent?.Invoke(this, args);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            dirtyProperties.Clear();

            for (int i = 0; i < disposableObjects.Count; i++)
            {
                IDisposable disposable = disposableObjects[i];
                try
                {
                    if (disposable != null)
                        disposable.Dispose();
                    Utilities.Dispose(ref disposable);
                }
                catch (Exception ex)
                {
                    Log.Error("Error disposing object. {0}", ex, disposable);
                }
            }

            disposableObjects.Clear();
        }

        /// <summary>
        /// Clone the object calling the native <see cref="object.MemberwiseClone"/> method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Clone<T>()
        {
            return (T)this.MemberwiseClone();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Add a new dirty property to the properties that will be updated the next time the UpdateState is called.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="addDirtyObject">See the description from the <see cref="SetPropertyAsDirty{T}(ref T, T, string, bool)"/> </param>
        /// <param name="newValue">The new value of the property.</param>
        private void addDirtyProperty<T>(string propertyName, ref T newValue, bool addDirtyObject)
        {
            if (!dirtyProperties.ContainsKey(propertyName))
            {
                dirtyProperties.Add(propertyName, newValue);
                if (this is Game11)
                    return;

                if (addDirtyObject)
                    Game11.AddDirtyObject(this);
            }
        }
        #endregion
    }
}
