using PoncheToolkit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util.Reflection
{
    public enum SerializationKind
    {
        All,
        None
    }

    /// <summary>
    /// Main attribute to enable custom serialization.
    /// Just the parent class must be decorated with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
    public sealed class PTSerializableClassAttribute : Attribute, ILoggable
    {
        /// <summary>
        /// The type of the object to create the instance when deserializing.
        /// </summary>
        public Type CurrentType { get; set; }
        /// <inheritdoc/>
        public Logger Log { get; set; }
        /// <summary>
        /// The current kind of serialization, if all the properties will be serialized or none, so they are individually marked with the <see cref="PTSerializablePropertyAttribute"/>.
        /// </summary>
        public SerializationKind SerializationKind { get; set; }

        /// <summary>
        /// The names of the properties that will be used as constrctor parameters.
        /// </summary>
        public string[] ConstructorParamPropertiesNames;

        /// <summary>
        /// Constructor. Needs the type of the class to serialize.
        /// Check if the type given has a parameterless constructor or a constructor with one <see cref="Game11"/> or <see cref="Game12"/> paramater.
        /// </summary>
        /// <param name="currentType">The current type to be serialized.</param>
        /// <param name="serializationKind">The current kind of serialization, if all the properties will be serialized or none, so they are individually marked with the <see cref="PTSerializablePropertyAttribute"/>.</param>
        /// /// <param name="constructorParamPropertiesNames">The names of the properties that will be used as constrctor parameters.</param>
        public PTSerializableClassAttribute(Type currentType, SerializationKind serializationKind, params string[] constructorParamPropertiesNames)
        {
            this.CurrentType = currentType;
            this.SerializationKind = serializationKind;
            this.ConstructorParamPropertiesNames = constructorParamPropertiesNames;
            Log = new Logger(GetType());
        }
    }

    #region Surrogate classes
    /// <summary>
    /// Serializable surrogate that implements the serialization and deserialization.
    /// </summary>
    public class PTSerializableSurrogate : ISerializationSurrogate, ILoggable
    {
        private Dictionary<Type, ConstructorInfo> constructors;

        /// <inheritdoc/>
        public Logger Log { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTSerializableSurrogate()
        {
            Log = new Logger(GetType());
        }

        /// <summary>
        /// Automatically called when serializing.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="info">The Serialization Info.</param>
        /// <param name="context">The Streaming context.</param>
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            PTSerializer serializer = new PTSerializer();
            PTSerializationElement tree = serializer.CreateSerializationTree(obj, ref info);
        }

        /// <summary>
        /// Automatically called when deserializing.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="info">The Serialization Info.</param>
        /// <param name="context">The Streaming context.</param>
        /// <param name="selector">The surrogate selector used.</param>
        /// <returns>The object deserialized.</returns>
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            constructors = new Dictionary<Type, ConstructorInfo>();

            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            enumerator.MoveNext(); // Advance the first entry that only has the main type that is this.Type.
            deserializeInfo(enumerator, obj, obj, obj.GetType(), null, true, -1, PTSerializer.BEGIN_GUID, PTSerializer.BEGIN_GUID);

            return obj;
        }

        private void deserializeInfo(SerializationInfoEnumerator enumerator, object currentInstance, object ownerInstance, Type ownerType, PropertyInfo currentProperty,
            bool moveNext, int listIndex, string lastGuid, string lastParentGuid)
        {
            bool finished = false;
            if (moveNext && !enumerator.MoveNext())
                finished = true;

            if (!finished)
            {
                bool callRecursive = true;
                bool isList = false;
                moveNext = true;
                SerializationEntry currentEntry = enumerator.Current;
                string[] guids = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_PARENT_GUID }, StringSplitOptions.None);
                string parentGuid = guids[1];
                string currentGuid = guids[2];

                //// Update object.
                //if (lastParentGuid != parentGuid && ownerInstance != currentInstance)
                //{
                //    if (listIndex >= 0)
                //        ownerType.GetMethod("Add").Invoke(ownerInstance, new[] { currentInstance });
                //    else
                //        currentProperty.SetValue(ownerInstance, currentInstance);
                //}

                if (isNewInstance(currentEntry)) // Create new instance of current type. Typically an instance of a list element.
                {
                    Type instanceType = Type.GetType(currentEntry.Value.ToString());

                    // TODO: aquí el problema es que se va metiendo de niveles, se necesita que se quede estático el ownerInstance cuando sea una lista.
                    //    ownerInstance = currentInstance;
                    //ownerType = ownerInstance.GetType();

                    ConstructorInfo constructor = null;
                    if (constructors.ContainsKey(instanceType))
                        constructor = constructors[instanceType];
                    else // Check for a Game only parameter constructor.
                    {
#if DX11
                        constructor = instanceType.GetConstructor(new Type[] { typeof(Game11) });
#elif DX12
                    constructor = instanceType.GetConstructor(new Type[] { typeof(Game12) });
#endif
                        if (constructor == null) // Check for a parameterless constructor.
                            constructor = instanceType.GetConstructor(new Type[] { });

                        // Check for constructor with primitive values.
                        // Get the first valid constructor for the type.
                        if (constructor == null)
                        {
                            ConstructorInfo[] constructors = instanceType.GetConstructors();
                            foreach (ConstructorInfo cons in constructors)
                            {
                                ParameterInfo[] parameters = cons.GetParameters();
                                bool hasPrimitiveParams = true;
                                foreach (ParameterInfo param in parameters)
                                {
                                    if (!PTSerializer.IsSimpleType(param.ParameterType))
                                    {
                                        hasPrimitiveParams = false;
                                        return;
                                    }
                                }

                                if (hasPrimitiveParams)
                                {
                                    constructor = cons;
                                    break;
                                }
                            }
                        }

                        if (constructor != null)
                            constructors.Add(instanceType, constructor);
                    }

                    if (constructor != null)
                    {
                        currentInstance = Activator.CreateInstance(instanceType);
                        ownerType = ownerInstance.GetType();

                        // == To get if the current property is a list.
                        isList = PTSerializer.IsList(currentProperty.PropertyType);
                        listIndex = -1;
                        if (isList)
                        {
                            string[] indexSplit = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_LIST_INDEX }, StringSplitOptions.None);
                            bool hasIndex = indexSplit.Length > 1;
                            if (hasIndex)
                                listIndex = Convert.ToInt32(indexSplit[1]);
                        }
                        // ==============
                    }
                    else
                    {
                        Log.Warning("Elements of type -{0}- cannot be dserialized. The type doesn't have parameterless constructor neither a constructor with just primitive values nor a constructor with just a Game11 or Game12 parameter.", instanceType.Name);
                        callRecursive = false;
                    }
                }
                else if (isPropertyName(currentEntry)) // This also creates an instance if the type is a list.
                {
                    string propertyName = currentEntry.Value.ToString();
                    currentProperty = ownerType.GetProperty(propertyName);
                    currentInstance = Activator.CreateInstance(currentProperty.PropertyType);

                    //if (PTSerializer.IsList(currentInstance.GetType()))
                    //    ownerInstance = currentInstance;

                    //deserializeInfo(enumerator, currentInstance, ownerInstance, ownerType, currentProperty, true, listIndex, currentGuid, parentGuid);
                }
                // These values are final and do not represent a complex object.
                else if (isOuterValue(currentEntry))
                {
                    string propName = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_OUTER_VALUE }, StringSplitOptions.None)[1];
                    PropertyInfo property = ownerType.GetProperty(propName);
                    property.SetValue(ownerInstance, currentEntry.Value);
                }
                else if (isFieldValue(currentEntry))
                {
                    updateFieldValue(enumerator, currentEntry, currentProperty, currentInstance, ownerInstance, ownerType, parentGuid, listIndex >= 0);
                    moveNext = false; // Do not call the enumerator.Next
                }
                else if (isListEnd(currentEntry))
                {
                    //moveNext = false;
                    callRecursive = false;
                }

                if (callRecursive)
                {
                    if (PTSerializer.IsList(currentInstance.GetType()))
                        deserializeInfo(enumerator, currentInstance, currentInstance, ownerType, currentProperty, true, listIndex, currentGuid, parentGuid);
                    else
                        deserializeInfo(enumerator, currentInstance, ownerInstance, ownerType, currentProperty, true, listIndex, currentGuid, parentGuid);
                }

                //if (callRecursive)
                //    deserializeInfo(enumerator, currentInstance, ownerInstance, ownerType, currentProperty, moveNext, listIndex, currentGuid, parentGuid);
            }

            if (PTSerializer.IsList(ownerInstance.GetType()))
                ownerType.GetMethod("Add").Invoke(ownerInstance, new[] { currentInstance });
            else
                currentProperty.SetValue(ownerInstance, currentInstance);

            //if (PTSerializer.IsList(currentInstance.GetType()) && ownerInstance != currentInstance)
            //{
            //    currentProperty.SetValue(ownerInstance, currentInstance);
            //}
        }

        private void updateFieldValue(SerializationInfoEnumerator enumerator, SerializationEntry currentEntry, PropertyInfo currentProperty,
            object currentInstance, object ownerInstance, Type ownerType, string parentGuid, bool isList)
        {
            string lastParentGuid = parentGuid;
            
            // Update the inner vlues of a struct. (e.g. X,Y,Z)
            while (lastParentGuid == parentGuid)
            {
                lastParentGuid = parentGuid;
                string propName = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_FIELD_VALUE }, StringSplitOptions.None)[1];
                FieldInfo field = null;
                if (isList)
                {
                    Type typeFromList = currentInstance.GetType();
                    field = typeFromList.GetField(propName);
                }
                else
                    field = currentProperty.PropertyType.GetField(propName);
                field.SetValue(currentInstance, currentEntry.Value);

                enumerator.MoveNext();
                currentEntry = enumerator.Current;
                // Update the parent guid.
                string[] guids = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_PARENT_GUID }, StringSplitOptions.None);
                parentGuid = guids[1];
                string currentGuid = guids[2];
            }

            //if (isList)
            //    ownerType.GetMethod("Add").Invoke(ownerInstance, new[] { currentInstance });
            //else
            //    currentProperty.SetValue(ownerInstance, currentInstance);
        }


        //        private void deserializeInfo(SerializationInfoEnumerator enumerator, object currentInstance, object ownerInstance, Type ownerType, PropertyInfo currentProperty,
        //            bool moveNext, bool callRecursive, int listIndex)
        //        {
        //            if (moveNext && !enumerator.MoveNext())
        //                return;

        //            moveNext = true;
        //            callRecursive = true;
        //            SerializationEntry currentEntry = enumerator.Current;
        //            string[] guids = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_PARENT_GUID }, StringSplitOptions.None);
        //            string parentGuid = guids[1];
        //            string currentGuid = guids[2];

        //            if (isNewInstance(currentEntry)) // Create new instance of current type. Typically an instance of a list element.
        //            {
        //                listIndex = 0;
        //                Type instanceType = Type.GetType(currentEntry.Value.ToString());
        //                ownerInstance = currentInstance;
        //                ownerType = ownerInstance.GetType();

        //                ConstructorInfo constructor = null;
        //                if (constructors.ContainsKey(instanceType))
        //                    constructor = constructors[instanceType];
        //                else // Check for a Game only parameter constructor.
        //                {
        //#if DX11
        //                    constructor = instanceType.GetConstructor(new Type[] { typeof(Game11) });
        //#elif DX12
        //                    constructor = instanceType.GetConstructor(new Type[] { typeof(Game12) });
        //#endif
        //                    if (constructor == null) // Check for a parameterless constructor.
        //                        constructor = instanceType.GetConstructor(new Type[] { });

        //                    // Get the first valid constructor for the type.
        //                    // Check for constructor with primitive values.
        //                    if (constructor == null)
        //                    {
        //                        constructor = instanceType.GetConstructor(new Type[] { });

        //                        ConstructorInfo[] constructors = instanceType.GetConstructors();
        //                        foreach (ConstructorInfo cons in constructors)
        //                        {
        //                            ParameterInfo[] parameters = cons.GetParameters();
        //                            bool hasPrimitiveParams = true;
        //                            foreach (ParameterInfo param in parameters)
        //                            {
        //                                if (!PTSerializer.IsSimpleType(param.ParameterType))
        //                                {
        //                                    hasPrimitiveParams = false;
        //                                    return;
        //                                }
        //                            }

        //                            if (hasPrimitiveParams)
        //                            {
        //                                constructor = cons;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }

        //                if (constructor != null)
        //                {
        //                    constructors.Add(instanceType, constructor);
        //                    //currentInstance = Activator.CreateInstance(instanceType);
        //                    ownerInstance = Activator.CreateInstance(instanceType);
        //                    ownerType = ownerInstance.GetType();
        //                }
        //                else
        //                {
        //                    Log.Warning("Elements of type -{0}- cannot be dserialized. The type doesn't have parameterless constructor neither a constructor with just primitive values nor a constructor with just a Game11 or Game12 parameter.", instanceType.Name);
        //                    callRecursive = false;
        //                }

        //            }
        //            else if (isPropertyName(currentEntry)) // This also creates an instance if the type is a list.
        //            {
        //                string propertyName = currentEntry.Value.ToString();
        //                currentProperty = ownerType.GetProperty(propertyName);
        //                currentInstance = Activator.CreateInstance(currentProperty.PropertyType);

        //                deserializeInfo(enumerator, currentInstance, ownerInstance, ownerType, currentProperty, true, true, listIndex);
        //            }
        //            // These values are final and do not represent a complex object.
        //            else if (isOuterValue(currentEntry))
        //            {
        //                string propName = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_OUTER_VALUE }, StringSplitOptions.None)[1];
        //                PropertyInfo property = ownerType.GetProperty(propName);
        //                property.SetValue(ownerInstance, currentEntry.Value);

        //                //string[] indexSplit = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_LIST_INDEX }, StringSplitOptions.None);
        //                //bool hasIndex = indexSplit.Length > 1;
        //                //if (hasIndex)
        //                //    property.SetValue(currentInstance, currentEntry.Value, new object[] { indexSplit[1] });
        //                //else
        //                //property.SetValue(currentInstance, currentEntry.Value);
        //            }
        //            else if (isInnerValue(currentEntry))
        //            {
        //                string lastParentGuid = parentGuid;
        //                bool isList = PTSerializer.IsList(currentProperty.PropertyType);
        //                bool hasIndex = false;
        //                Type typeFromList = null;
        //                if (isList)
        //                {
        //                    typeFromList = currentProperty.PropertyType.GenericTypeArguments[0];
        //                    string[] indexSplit = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_LIST_INDEX }, StringSplitOptions.None);
        //                    hasIndex = indexSplit.Length > 1;
        //                }

        //                while (lastParentGuid == parentGuid)
        //                {
        //                    lastParentGuid = parentGuid;
        //                    string propName = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_INNER_VALUE }, StringSplitOptions.None)[1];
        //                    FieldInfo field = null;
        //                    if (hasIndex)
        //                        field = typeFromList.GetField(propName);
        //                    else
        //                        field = currentProperty.PropertyType.GetField(propName);
        //                    field.SetValue(currentInstance, currentEntry.Value);
        //                    enumerator.MoveNext();
        //                    currentEntry = enumerator.Current;
        //                    guids = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_PARENT_GUID }, StringSplitOptions.None);
        //                    parentGuid = guids[1];
        //                    currentGuid = guids[2];
        //                }
        //                moveNext = false;

        //                currentProperty.SetValue(ownerInstance, currentInstance);
        //            }

        //            // Update object if it is a list.
        //            if (PTSerializer.IsList(ownerType))
        //            {
        //                string[] indexSplit = currentEntry.Name.Split(new string[] { PTSerializer.SYMBOL_LIST_INDEX }, StringSplitOptions.None);
        //                bool hasIndex = indexSplit.Length > 1;
        //                if (hasIndex)
        //                    currentProperty.SetValue(ownerInstance, currentInstance, new object[] { indexSplit[1] });
        //            }

        //            if (callRecursive)
        //                deserializeInfo(enumerator, currentInstance, ownerInstance, ownerType, currentProperty, moveNext, true, listIndex);

        //            //currentInstance = recursiveInstance(enumerator, currentEntry, ownerInstance, currentInstance, parentGuid, currentGuid, currentProperty);

        //            // Set the most outer instance value.
        //            //currentProperty.SetValue(ownerInstance, currentInstance);
        //        }

        #region Private methods
        private bool isNewInstance(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_COMPLEX_TYPE);
        }

        private bool isOuterValue(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_OUTER_VALUE);
        }

        /// <summary>
        /// Method to check if the current entry value is a field
        /// </summary>
        /// <param name="currentEntry"></param>
        /// <returns></returns>
        private bool isFieldValue(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_FIELD_VALUE);
        }

        private bool isListEnd(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_LIST_END);
        }

        private bool isPropertyName(SerializationEntry currentEntry)
        {
            return currentEntry.Name.StartsWith(PTSerializer.SYMBOL_INSTANCE_PROPERTY_NAME);
        }
        #endregion
    }


    /// <summary>
    /// Returns PTSerializableSurrogate for all types marked PTSerializableAttribute
    /// </summary>
    public class PTSerializableSurrogateSelector : ISurrogateSelector
    {
        /// <inheritdoc/>
        public void ChainSelector(ISurrogateSelector selector)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ISurrogateSelector GetNextSelector()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (!type.IsDefined(typeof(PTSerializableClassAttribute), false))
            {
                selector = null;
                return null;
            }
            selector = this;
            return new PTSerializableSurrogate();
        }
    }

    #endregion
}
