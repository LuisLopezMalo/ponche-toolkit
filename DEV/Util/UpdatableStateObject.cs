using PoncheToolkit.Core;
using PoncheToolkit.Util.Reflection;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util
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
        Type thisType;
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

        #region Not used!
        /*
        private void updateDataFromSerialization(SerializationInfo info)
        {
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            enumerator.MoveNext(); // Advance the first entry that only has the main type that is thisType.

            object instance = this;
            object parent = null;
            Type currentType = this.GetType();
            Type parentType = this.GetType();
            PropertyInfo currentProperty = null;
            bool updateValue = false;

            //while (enumerator.MoveNext())
            //{
            //    SerializationEntry currentEntry = enumerator.Current;


            //    //// Apply accumulated value to the parent instance property.
            //    ////if (parent != instance)
            //    //if (updateValue)
            //    //{
            //    //    currentProperty.SetValue(parent, instance);
            //    //    currentType = instance.GetType();
            //    //    parentType = parent.GetType();
            //    //}

            //    //updateValue = false; // Reset value.

            //    //if (isNewInstance(currentEntry)) // Create new instance of current type.
            //    //{
            //    //    // Create the new instance.
            //    //    Type instanceType = Type.GetType(currentEntry.Value.ToString());
            //    //    //parent = instance;
            //    //    instance = Activator.CreateInstance(instanceType);
            //    //}
            //    //else if (isList(currentEntry))
            //    //{
            //    //    // Create the instance of the list.
            //    //    Type instanceType = Type.GetType(currentEntry.Value.ToString());
            //    //    instance = Activator.CreateInstance(instanceType);
            //    //}
            //    //else if (isPropertyName(currentEntry))
            //    //{
            //    //    string propertyName = currentEntry.Value.ToString();
            //    //    currentProperty = parentType.GetProperty(propertyName);
            //    //    instance = Activator.CreateInstance(currentProperty.PropertyType);
            //    //}
            //    //else if (isOuterValue(currentEntry))
            //    //{
            //    //    string propName = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_OUTER_VALUE }, StringSplitOptions.None)[1];
            //    //    PropertyInfo property = parentType.GetProperty(propName);
            //    //    property.SetValue(parent, currentEntry.Value);
            //    //    updateValue = true;
            //    //}
            //    //else if (isInnerValue(currentEntry))
            //    //{
            //    //    string propName = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_INNER_VALUE }, StringSplitOptions.None)[1];
            //    //    FieldInfo field = currentProperty.PropertyType.GetField(propName);
            //    //    field.SetValue(instance, currentEntry.Value);
            //    //    updateValue = true;
            //    //}
            //}


            //updateValuesRecursive(enumerator, enumerator.Current, currentInstance, currentInstance.GetType(), currentProperty);

            //enumerator.MoveNext();
            //string guid = enumerator.Current.Name.Split(new string[] { PTSerializer.SYMBOL_PARENT_GUID }, StringSplitOptions.None)[1];
            //assignChildrenRecursive(enumerator, enumerator.Current, this, this, guid, currentProperty);


            saveInstances(enumerator, this, this, this.GetType(), null, true, true);
        }

        private void saveInstances(SerializationInfoEnumerator enumerator, object currentInstance, object ownerInstance, Type ownerType, PropertyInfo currentProperty,
            bool moveNext, bool callRecursive)
        {
            if (moveNext && !enumerator.MoveNext())
                return;

            moveNext = true;
            callRecursive = true;
            SerializationEntry currentEntry = enumerator.Current;
            //ownerType = ownerInstance.GetType();
            string[] guids = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_GUID }, StringSplitOptions.None);
            string parentGuid = guids[1];
            string currentGuid = guids[2];

            if (isNewInstance(currentEntry)) // Create new instance of current type. Typically an instance of a list element.
            {
                Type instanceType = Type.GetType(currentEntry.Value.ToString());
                ownerInstance = currentInstance;
#if DX11
                ConstructorInfo constructor = instanceType.GetConstructor(new Type[] { typeof(Game11) });
#elif DX12
                ConstructorInfo constructor = instanceType.GetConstructor(new Type[] { typeof(Game12) });
#endif
                if (constructor != null)
                {
                    //currentInstance = constructor.Invoke(new object[] { Game.Instance });
                    //ownerType = currentInstance.GetType();
                    ownerInstance = constructor.Invoke(new object[] { Game.Instance });
                    ownerType = ownerInstance.GetType();
                }
                else
                {
                    constructor = instanceType.GetConstructor(new Type[] { });
                    if (constructor != null)
                    {
                        ownerInstance = constructor.Invoke(new object[] { });
                        ownerType = ownerInstance.GetType();
                    }
                    else
                    {
                        Log.Warning("Elements of type -{0}- cannot be serialized. Doesn't have Parameterless constructor neither a constructor with just a Game11 or Game12 parameter.", instanceType.Name);
                        callRecursive = false;
                    }

                }
                //currentInstance = Activator.CreateInstance(instanceType);
            }
            else if (isPropertyName(currentEntry)) // This also creates an instance if the type is a list.
            {
                string propertyName = currentEntry.Value.ToString();
                currentProperty = ownerType.GetProperty(propertyName);
                currentInstance = Activator.CreateInstance(currentProperty.PropertyType);

                saveInstances(enumerator, currentInstance, ownerInstance, ownerType, currentProperty, true, true);
            }
            // These values are final and do not represent a complex object.
            else if (isOuterValue(currentEntry))
            {
                string propName = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_OUTER_VALUE }, StringSplitOptions.None)[1];
                PropertyInfo property = ownerType.GetProperty(propName);
                property.SetValue(ownerInstance, currentEntry.Value);

                //string[] indexSplit = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_LIST_INDEX }, StringSplitOptions.None);
                //bool hasIndex = indexSplit.Length > 1;
                //if (hasIndex)
                //    property.SetValue(currentInstance, currentEntry.Value, new object[] { indexSplit[1] });
                //else
                //property.SetValue(currentInstance, currentEntry.Value);
            }
            else if (isInnerValue(currentEntry))
            {
                string lastParentGuid = parentGuid;

                while (lastParentGuid == parentGuid)
                {
                    lastParentGuid = parentGuid;
                    string propName = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_INNER_VALUE }, StringSplitOptions.None)[1];
                    FieldInfo field = currentProperty.PropertyType.GetField(propName);
                    field.SetValue(currentInstance, currentEntry.Value);
                    enumerator.MoveNext();
                    currentEntry = enumerator.Current;
                    guids = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_GUID }, StringSplitOptions.None);
                    parentGuid = guids[1];
                    currentGuid = guids[2];
                }
                moveNext = false;

                currentProperty.SetValue(ownerInstance, currentInstance);
            }

            // Update object if it is a list.
            if (PTSerializer.IsList(ownerType))
            {
                string[] indexSplit = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_LIST_INDEX }, StringSplitOptions.None);
                bool hasIndex = indexSplit.Length > 1;
                if (hasIndex)
                    currentProperty.SetValue(ownerInstance, currentInstance, new object[] { indexSplit[1] });
            }

            if (callRecursive)
                saveInstances(enumerator, currentInstance, ownerInstance, ownerType, currentProperty, moveNext, true);

            //currentInstance = recursiveInstance(enumerator, currentEntry, ownerInstance, currentInstance, parentGuid, currentGuid, currentProperty);

            // Set the most outer instance value.
            //currentProperty.SetValue(ownerInstance, currentInstance);
        }

        private object recursiveInstance(SerializationInfoEnumerator enumerator, SerializationEntry currentEntry, object parentInstance, object currentInstance,
            string mainParentGuid, string parentGuid, PropertyInfo currentProperty)
        {
            if (!enumerator.MoveNext())
                return currentInstance;

            SerializationEntry newEntry = enumerator.Current;
            Type parentType = parentInstance.GetType();
            string[] guids = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_GUID }, StringSplitOptions.None);
            parentGuid = guids[1];
            string currentGuid = guids[2];

            if (isNewInstance(newEntry)) // Create new instance of current type.
            {
                Type instanceType = Type.GetType(newEntry.Value.ToString());
                currentInstance = Activator.CreateInstance(instanceType);
            }
            else if (isPropertyName(newEntry)) // This also creates an instance if the type is a list.
            {
                string propertyName = newEntry.Value.ToString();
                currentProperty = parentType.GetProperty(propertyName);
                currentInstance = Activator.CreateInstance(currentProperty.PropertyType);
                saveInstances(enumerator, currentInstance, parentInstance, parentType, currentProperty, true, true);
            }
            // These values are final and do not represent a complex object.
            else if (isOuterValue(newEntry))
            {
                string propName = newEntry.Name.Split(new string[] { PTSerializer.SYMBOL_OUTER_VALUE }, StringSplitOptions.None)[1];
                PropertyInfo property = parentType.GetProperty(propName);
                property.SetValue(parentInstance, newEntry.Value);
                recursiveInstance(enumerator, newEntry, parentInstance, currentInstance, parentGuid, currentGuid, currentProperty);
            }
            else if (isInnerValue(newEntry))
            {
                string propName = newEntry.Name.Split(new string[] { PTSerializer.SYMBOL_INNER_VALUE }, StringSplitOptions.None)[1];
                FieldInfo field = currentProperty.PropertyType.GetField(propName);
                field.SetValue(currentInstance, newEntry.Value);
                recursiveInstance(enumerator, newEntry, parentInstance, currentInstance, parentGuid, currentGuid, currentProperty);
            }

            //recursiveInstance(enumerator, newEntry, parentInstance, currentInstance, parentGuid, currentGuid, currentProperty);

            return currentInstance;
        }

        private void updateInnerValues(SerializationInfoEnumerator enumerator, string parentGuid, object instance, PropertyInfo property)
        {
            string currentParentGuid = parentGuid;
            while (parentGuid == currentParentGuid)
            {
                enumerator.MoveNext();
                SerializationEntry entry = enumerator.Current;

                string[] guids = entry.Name.Split(new string[] { PTSerializer.SYMBOL_GUID }, StringSplitOptions.None);
                currentParentGuid = guids[1];
                string propertyGuid = guids[2];

                string propName = entry.Name.Split(new string[] { PTSerializer.SYMBOL_INNER_VALUE }, StringSplitOptions.None)[1];
                FieldInfo field = property.PropertyType.GetField(propName);
                field.SetValue(instance, entry.Value);
            }
        }

        #region Private methods
        private bool isNewInstance(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_COMPLEX_TYPE);
        }

        private bool isOuterValue(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_OUTER_VALUE);
        }

        private bool isInnerValue(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_INNER_VALUE);
        }

        private bool isList(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_LIST);
        }

        private bool isPropertyName(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_INSTANCE_PROPERTY_NAME);
        }
        #endregion

        /// <summary>
        /// Method that is called when the serialization process is made.
        /// Here the data to be serialized is filled.
        /// </summary>
        /// <param name="info">The SerializationInfo where the data to be serialized is filled.</param>
        /// <param name="context">The Streaming context.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PTSerializer serializer = new PTSerializer();
            PTSerializationElement tree = serializer.CreateSerializationTree(this, ref info);
        }
        */
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
            if (dirtyProperties == null)
                return;

            if (!dirtyProperties.ContainsKey(propertyName))
            {
                dirtyProperties.Add(propertyName, newValue);
                if (this is Game11)
                    return;

                if (addDirtyObject)
                    Game11.AddDirtyObject(this);
            }
            else
            {
                // Set the new value to the existing property.
                dirtyProperties[propertyName] = newValue;
            }
        }
        #endregion
    }
}
