using PoncheToolkit.Core;
using PoncheToolkit.Util.Exceptions;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoncheToolkit.Util.Reflection
{
    /// <summary>
    /// Class that manage the serialization and deserialization of objects.
    /// The properties or fields that can be serialized must have the attribute <see cref="PTSerializablePropertyAttribute"/>,
    /// only these properties will be serialized.
    /// </summary>
    public class PTSerializer : ILoggable
    {
        /// <summary>
        /// The custom separator to distinguish between types when serializing and deserializing.
        /// </summary>
        public static string SYMBOL_COMPLEX_TYPE = "=";
        /// <summary>
        /// The custom separator to distinguish between types when serializing and deserializing.
        /// </summary>
        public static string SYMBOL_NEW_INSTANCE_LIST = "=+";
        /// <summary>
        /// The custom separator to distinguish between types when serializing and deserializing.
        /// </summary>
        public static string SYMBOL_LIST_END = "+";
        /// <summary>
        /// The custom separator to distinguish between types when serializing and deserializing.
        /// </summary>
        public static string SYMBOL_CONSTRUCTOR_VALUE = "!";
        /// <summary>
        /// The custom separator to distinguish between types when serializing and deserializing.
        /// </summary>
        public static string SYMBOL_INSTANCE_PROPERTY_NAME = "#";
        /// <summary>
        /// The custom separator to distinguish between types when serializing and deserializing.
        /// This symbol represents a public field inside a struct or a class.
        /// </summary>
        public static string SYMBOL_FIELD_VALUE = "%";
        /// <summary>
        /// The custom separator to distinguish between types when serializing and deserializing.
        /// </summary>
        public static string SYMBOL_OUTER_VALUE = "$";
        /// <summary>
        /// The custom separator to distinguish between types when serializing and deserializing.
        /// </summary>
        public static string SYMBOL_LIST_INDEX = "*";
        /// <summary>
        /// The custom separator to distinguish between types when serializing and deserializing.
        /// </summary>
        public static string SYMBOL_PARENT_GUID = "@";
        /// <summary>
        /// The default value for the first guid.
        /// </summary>
        public static string BEGIN_GUID = "0";

        private int guid;
        private bool silentErrors;

        /// <inheritdoc/>
        public Logger Log { get; set; }

        /// <summary>
        /// Constructor.
        /// Silent errors false.
        /// </summary>
        public PTSerializer()
            : this(false)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="silentErrors">If the errors like when a class is not marked as serialized are thrown or not.</param>
        public PTSerializer(bool silentErrors)
        {
            this.silentErrors = silentErrors;
            this.guid = Convert.ToInt32(BEGIN_GUID);
            this.Log = new Logger(GetType());
        }

        #region Public Methods
        /// <summary>
        /// Serialize the given object using the custom serialization.
        /// Set the file into the output path.
        /// </summary>
        /// <param name="instance">The object to be serialized.</param>
        public void Serialize(object instance)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, instance);
            }
        }

        /// <summary>
        /// Serialize the given object.
        /// Set the file into the output path.
        /// </summary>
        /// <param name="instance">The object to be serialized.</param>
        /// <param name="outputPath">The relative output path with file name.</param>
        /// <param name="saveFile">Value to indicate if the serialized file should be saved in a physical path.</param>
        public void Serialize(object instance, bool saveFile, string outputPath)
        {
            // Search if the class is decorated with the PTSerializableClassAttribute attribute.
            CustomAttributeData att = instance.GetType().CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(PTSerializableClassAttribute));
            if (att == null)
                throw new CustomSerializationException("The storage type -{0}- is not decorated with the -{1}- attribute.", instance.GetType().Name, typeof(PTSerializableClassAttribute).Name);

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.SurrogateSelector = new PTSerializableSurrogateSelector();
                formatter.Serialize(stream, instance);

                if (saveFile)
                {
                    if (string.IsNullOrEmpty(outputPath))
                        throw new ArgumentNullException("outputPath", "The path for the file to be saved is null");
                    File.WriteAllBytes(outputPath, stream.ToArray());
                }
            }
        }

        /// <summary>
        /// Deserialize the file from the path given.
        /// </summary>
        /// <param name="file">The relative path to the file.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Throws File not found.</exception>
        public T Deserialize<T>(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("File not found - " + file);

            // Convert a byte array to an Object
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] bytes = File.ReadAllBytes(file);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.SurrogateSelector = new PTSerializableSurrogateSelector();

                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                object obj = formatter.Deserialize(stream);
                return (T)obj;
            }
        }

        /// <summary>
        /// Create a recursive list with the name of the fields and the values to be serialized and fill the <see cref="SerializationInfo"/> object.
        /// This method is automatically called inside the <see cref="PTSerializableSurrogate.GetObjectData(object, SerializationInfo, StreamingContext)"/> method,
        /// that is called when the <see cref="Serialize(object)"/> method is invoked.
        /// </summary>
        /// <param name="instance">The instance of the object to be serialized.</param>
        /// <param name="info">The <see cref="SerializationInfo"/> object to store the values to be serialized.</param>
        /// <returns></returns>
        internal PTSerializationElement CreateSerializationTree(object instance, ref SerializationInfo info)
        {
            PTSerializationElement tree = assignPropertyTreeRecursive(instance, null, null, ref info);

            return tree;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Method that add all the values from its properties to the current <see cref="PTSerializationElement"/> object.
        /// </summary>
        /// <param name="instance">The current instance of the object from where the values will be retrieved recuresively.</param>
        /// <param name="currentProperty">The <see cref="PropertyInfo"/> object of the current property evaluated. Null for the root element.</param>
        /// <param name="parent">The parent of the current processed element. Null for the root element.</param>
        /// <param name="info">The <see cref="SerializationInfo"/> object to store the values to be serialized.</param>
        private PTSerializationElement assignPropertyTreeRecursive(object instance, PropertyInfo currentProperty, PTSerializationElement parent, ref SerializationInfo info)
        {
            PTSerializationElement current = new PTSerializationElement();
            current.PropertyInstance = instance;
            current.Parent = parent;
            current.Guid = nextGuid();
            current.CurrentLevel = parent != null ? parent.CurrentLevel : -1;
            current.CurrentLevel++;
            string parentGuid = parent != null ? parent.Guid : "";

            Type currentType = instance.GetType();
            current.BaseName = (parent != null ? parent.BaseName : instance.GetType().FullName) + SYMBOL_INSTANCE_PROPERTY_NAME +
                    currentType.FullName +
                    (currentProperty != null ? SYMBOL_INSTANCE_PROPERTY_NAME + currentProperty.Name : string.Empty);

            // The type is a list.
            // This list can contain complex classes or simple values(primitive).
            if (IsList(currentType))
            {
                // Add the list type.
                //string key = SYMBOL_NEW_INSTANCE_LIST + SYMBOL_PARENT_GUID + parentGuid + SYMBOL_PARENT_GUID + current.Guid;
                //current.SerializationTree.Add(key, currentType.FullName);
                //info.AddValue(key, currentType.FullName);

                // Add the name of the list property.
                string propertyGuid = nextGuid();
                string key = SYMBOL_INSTANCE_PROPERTY_NAME + SYMBOL_PARENT_GUID + current.Guid + SYMBOL_PARENT_GUID + propertyGuid;
                current.SerializationTree.Add(key, currentProperty.Name);
                info.AddValue(key, currentProperty.Name);

                PropertyInfo listIndexer = currentType.GetProperty("Item");

                if (IsSimpleType(listIndexer.PropertyType)) // Primitive value types, save them.
                {
                    IEnumerable list = (instance as IEnumerable);
                    int indexName = 0;
                    foreach (object obj in list) // The primitives values : e.g. name="modelName"
                    {
                        assignPrimitiveValue(obj, indexName.ToString(), current, propertyGuid, ref info);
                        indexName++;
                    }
                }
                else if (IsStruct(listIndexer.PropertyType)) // If the elements type of the list is a struct, save their values.
                {
                    IEnumerable list = (instance as IEnumerable);
                    Type listType = list.GetType().GetGenericArguments()[0];
                    int index = 0;
                    foreach (object obj in list) // The values of the struct: e.g. Vector3 -> X=0.5, Y=0.8, Z=1
                    {
                        //string listKey = SYMBOL_COMPLEX_TYPE + SYMBOL_LIST_INDEX + index + SYMBOL_LIST_INDEX + SYMBOL_GUID + nextGuid() + SYMBOL_GUID + propertyGuid;
                        string objGuid = nextGuid();
                        string listKey = SYMBOL_COMPLEX_TYPE + SYMBOL_LIST_INDEX + index + SYMBOL_LIST_INDEX + SYMBOL_PARENT_GUID + propertyGuid + SYMBOL_PARENT_GUID + objGuid;
                        current.SerializationTree.Add(listKey, listType.AssemblyQualifiedName);
                        info.AddValue(listKey, listType.AssemblyQualifiedName);

                        assignStructValues(obj, index.ToString(), obj.GetType(), current, objGuid, ref info);
                        index++;
                    }
                }
                else // It is a complex type. Recursively iterate over its properties.
                {
                    // Get the list of Properties that have the PTSerializable attribute.
                    List<PropertyInfo> properties = getSerializableProperties(listIndexer.PropertyType);

                    IEnumerable list = (instance as IEnumerable);
                    Type listType = list.GetType().GetGenericArguments()[0];
                    int index = 0;
                    foreach (object obj in list)
                    {
                        string listKey = SYMBOL_COMPLEX_TYPE + SYMBOL_LIST_INDEX + index + SYMBOL_LIST_INDEX + SYMBOL_PARENT_GUID + nextGuid() + SYMBOL_PARENT_GUID + propertyGuid;
                        current.SerializationTree.Add(listKey, listType.FullName);
                        info.AddValue(listKey, listType);

                        object instanceFromList = listIndexer.GetValue(instance, new object[] { index++ });
                        createPropertiesTree(instanceFromList, properties, ref current, propertyGuid, ref info);
                    }
                }

                // Add a symbol to get when the list has finished.
                key = SYMBOL_LIST_END + SYMBOL_PARENT_GUID + parentGuid + SYMBOL_PARENT_GUID + current.Guid;
                current.SerializationTree.Add(key, currentType.FullName);
                info.AddValue(key, currentType.FullName);
            }
            else // It is a complex object like a custom class. Recursively iterate over its properties.
            {
                string key = SYMBOL_COMPLEX_TYPE + SYMBOL_PARENT_GUID + parentGuid + SYMBOL_PARENT_GUID + current.Guid;
                current.SerializationTree.Add(key, currentType.FullName);
                info.AddValue(key, currentType.FullName);

                // Get the list of Properties that have the PTSerializable attribute.
                List<PropertyInfo> properties = getSerializableProperties(currentType);
                createPropertiesTree(instance, properties, ref current, current.Guid, ref info);
            }

            return current;
        }

        /// <summary>
        /// Method that add all the values from its properties to the current <see cref="PTSerializationElement"/> object.
        /// </summary>
        /// <param name="instance">The current instance of the object where the values will be retrieved.</param>
        /// <param name="properties">The <see cref="PropertyInfo"/> list that contain the information from all the properties to be evaluated.</param>
        /// <param name="current">The current instance of the serialization element to store the retrieved information.</param>
        /// <param name="parentGuid">The guid of the current parent.</param>
        /// <param name="info">The <see cref="SerializationInfo"/> object to store the values to be serialized.</param>
        private void createPropertiesTree(object instance, List<PropertyInfo> properties, ref PTSerializationElement current, string parentGuid, ref SerializationInfo info)
        {
            // Iterate over the properties and add them to the result or check if they are a complex class to iterate recursively over its properties.
            foreach (PropertyInfo prop in properties)
            {
                Type propertyValueType = prop.PropertyType;
                // Get the parent property value.
                object propertyValue = (prop as PropertyInfo).GetValue(instance);

                // Check if the value is primitive. (These are the values saved).
                if (IsSimpleType(propertyValueType))
                {
                    assignPrimitiveValue(propertyValue, prop.Name, current, parentGuid, ref info);
                }
                else if (IsStruct(propertyValueType)) // Check if it is a struct. In structs only public fields are serialized.
                {
                    string guid = nextGuid();
                    string key = SYMBOL_INSTANCE_PROPERTY_NAME + SYMBOL_PARENT_GUID + parentGuid + SYMBOL_PARENT_GUID + guid;
                    current.SerializationTree.Add(key, prop.Name);
                    info.AddValue(key, prop.Name);

                    assignStructValues(propertyValue, prop.Name, propertyValueType, current, guid, ref info);
                }
                else // If the property is a complex class, make the process again recursively for this new type.
                {
                    //TODO: en proceso para guardar los valores de las propiedades que representan parámetros de un constructor complejo.

                    current.Children.Add(assignPropertyTreeRecursive(propertyValue, prop, current, ref info));

                    //if (isSerializableClass(current, prop, propertyValue, ref info))
                    //    current.Children.Add(assignPropertyTreeRecursive(propertyValue, prop, current, ref info));
                    //else
                    //{
                    //    string message = string.Format("The type {0} is not marked with the PTSerializableClass attribute.", prop.PropertyType.FullName);
                    //    Log.Warning(message);
                    //    if (!silentErrors)
                    //        throw new CustomSerializationException(message);
                    //}
                }
            }
        }

        private void assignPrimitiveValue(object propertyValue, string currentPropertyName, PTSerializationElement current, string parentGuid, ref SerializationInfo info)
        {
            string key = SYMBOL_OUTER_VALUE + currentPropertyName + SYMBOL_OUTER_VALUE + SYMBOL_PARENT_GUID + parentGuid + SYMBOL_PARENT_GUID + nextGuid();
            current.SerializationTree.Add(key, propertyValue);
            info.AddValue(key, propertyValue);
        }

        private void assignStructValues(object propertyValue, string currentPropertyName, Type propertyValueType, PTSerializationElement current, string parentGuid, ref SerializationInfo info)
        {
            FieldInfo[] fields = propertyValueType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (FieldInfo field in fields) // The fields must be primitive.
            {
                string key = SYMBOL_FIELD_VALUE + field.Name + SYMBOL_FIELD_VALUE + SYMBOL_PARENT_GUID + parentGuid + SYMBOL_PARENT_GUID + nextGuid();
                current.SerializationTree.Add(key, field.GetValue(propertyValue));
                info.AddValue(key, field.GetValue(propertyValue));
            }
        }

        /// <summary>
        /// Get the guid for the next key.
        /// </summary>
        /// <returns></returns>
        private string nextGuid()
        {
            return (guid++).ToString();
        }

        /// <summary>
        /// Get if the type provided is simple type.
        /// This is if the type is primitive, enum, string or decimal.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns></returns>
        public static bool IsSimpleType(Type type)
        {
            if (!type.IsGenericType &&
                    (type.IsPrimitive
                    || type.IsEnum
                    || type.Equals(typeof(string))
                    || type.Equals(typeof(decimal)))
                )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the given type is a value type. This is a struct.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns></returns>
        public static bool IsStruct(Type type)
        {
            return (!type.IsPrimitive && type.IsValueType && !type.IsEnum);
        }

        /// <summary>
        /// Check if the given type is a generic <see cref="List{T}"/>.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns></returns>
        public static bool IsList(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>));
        }

        /// <summary>
        /// Get the properties that has the PTSerializableAttribute attribute for a given type.
        /// </summary>
        /// <param name="type">The type to get the properties from.</param>
        /// <returns></returns>
        private List<PropertyInfo> getSerializableProperties(Type type)
        {
            CustomAttributeData att = type.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(PTSerializableClassAttribute));
            if (att != null)
            {
                SerializationKind kind = (SerializationKind)att.ConstructorArguments[1].Value;
                if (kind == SerializationKind.All)
                    return type.GetProperties().ToList();
            }

            return type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PTSerializablePropertyAttribute))).ToList();
        }

        /// <summary>
        /// Get if the type given is decorated with the <see cref="PTSerializableClassAttribute"/> attribute.
        /// </summary>
        /// <param name="current">The current <see cref="PTSerializationElement"/> object.</param>
        /// <param name="currentProp">The current property processed.</param>
        /// <param name="currentInstance">The current instance to process.</param>
        /// <param name="info">The Serialization info.</param>
        /// <returns></returns>
        private bool isSerializableClass(PTSerializationElement current, PropertyInfo currentProp, object currentInstance, ref SerializationInfo info)
        {
            Type type = currentProp.PropertyType;
            bool isList = false;
            if (IsList(type))
            {
                type = type.GetGenericArguments()[0];
                isList = true;
            }

            if (IsSimpleType(type) || IsStruct(type))
                return true;

            // Check for the valid constructors.

            // ========== Search for parameterless constructor.
            ConstructorInfo constructor = type.GetConstructor(new Type[] { });
            if (constructor != null)
                return true;
            
            // ========== Search for constructor with just a Game instance parameter.
#if DX11
            constructor = type.GetConstructor(new Type[] { typeof(Game11) });
#elif DX12
            constructor = instanceType.GetConstructor(new Type[] { typeof(Game12) });
#endif
            if (constructor != null)
                return true;

            // ========== Search for complex constructor with primitive parameters define in the PTSerializableClassAttribute decorated class.
            CustomAttributeData att = type.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(PTSerializableClassAttribute));

            if (att == null)
                throw new CustomSerializationException("The complex type -{0}- is not decorated with the -{1}- attribute.", type.Name, typeof(PTSerializableClassAttribute).Name);

            // Get the type of parameters defined in the PTSerializableClassAttribute decorated class.
            IReadOnlyCollection<CustomAttributeTypedArgument> args = (IReadOnlyCollection<CustomAttributeTypedArgument>)att.ConstructorArguments[2].Value;
            PropertyInfo[] props = args.Select(arg => type.GetProperty(arg.Value.ToString())).ToArray();
            Type[] constructorTypes = new Type[props.Length];
            for (int i = 0; i < props.Length; i++)
                constructorTypes[i] = props[i].PropertyType;

            // Check if there are constructor types declared in the PTSerializableClassAttribute decorated class.
            if (constructorTypes == null || constructorTypes.Length == 0)
                throw new CustomSerializationException("The complex type -{0}- must define the types inside the -{1}- attribute to determine the constructor to be used when deserializing.", type.Name, typeof(PTSerializableClassAttribute).Name);

            // Get if any of the current object constructors matches the defined in the PTSerializableClassAttribute class.
            bool matchConstructor = false;
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            foreach (ConstructorInfo cons in constructors)
            {
                ParameterInfo[] consParams = cons.GetParameters();
                matchConstructor = consParams.Select(p => p.ParameterType).SequenceEqual(constructorTypes);
                if (matchConstructor)
                    break;
            }

            if (!matchConstructor)
                throw new CustomSerializationException("No matching constructor for the defined parameters. The complex type -{0}- must declare the parameters for the constructor to be used", type.Name);

            // TODO: EN PROCESO DE AGREGAR EL GUID DEL OBJETO PADRE CUANDO ES UN OBJETO COMPLEJO (ej: PTMesh(PTModel, name))... PARA QUE SE GUARDE Y SE UTILICE COMO PARAMETRO DEL CONSTRUCTOR.
            // Instantiate the constructor.
            constructor = type.GetConstructor(constructorTypes);
            if (constructor != null)
            {
                // If the instance is a list, iterate over all its elements.
                if (isList)
                {
                    IEnumerable list = (currentInstance as IEnumerable);
                    int index = 0;
                    foreach (object obj in list) // The values of the struct: e.g. Vector3 -> X=0.5, Y=0.8, Z=1
                    {
                        // Save the current parameter values.
                        for (int i = 0; i < props.Length; i++)
                        {
                            string key = SYMBOL_CONSTRUCTOR_VALUE + SYMBOL_LIST_INDEX + index + SYMBOL_LIST_INDEX + SYMBOL_PARENT_GUID + (current.Parent == null ? 0.ToString() : current.Parent.Guid) + SYMBOL_PARENT_GUID + nextGuid();
                            object value = props[i].GetValue(obj);
                            current.SerializationTree.Add(key, value);
                            info.AddValue(key, value);
                        }
                        index++;
                    }
                }else
                {
                    // Save the current parameter values.
                    for (int i = 0; i < props.Length; i++)
                    {
                        string key = SYMBOL_CONSTRUCTOR_VALUE + SYMBOL_PARENT_GUID + (current.Parent == null ? 0.ToString() : current.Parent.Guid) + SYMBOL_PARENT_GUID + nextGuid();
                        object value = props[i].GetValue(currentInstance);
                        current.SerializationTree.Add(key, value);
                        info.AddValue(key, value);
                    }
                }
                return true;
            }

            // ========== NO compatible constructors found.
            Log.Warning("Elements of type -{0}- cannot be serialized. Doesn't have Parameterless constructor neither a constructor with just a Game11 or Game12 parameter, nor a constructor that matches the parameter types defined in the PTSerializableClassAttribute.", type.Name);

            return false;
        }
        #endregion
    }
}
