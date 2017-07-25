using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util.Reflection
{
    /// <summary>
    /// Class that represent a single element to be serialized.
    /// The <see cref="serializationTree"/> dictionary contain, as the key, the name of the property
    /// and as the value the value of that property.
    /// </summary>
    public class PTSerializationElement
    {
        private string baseName;
        private string guid;
        private int currentLevel;
        private object propertyInstance;
        private PTSerializationElement parent;
        private List<PTSerializationElement> children;
        private Dictionary<string, object> serializationTree;

        /// <summary>
        /// The name formatted to get the properties to which this element belongs.
        /// </summary>
        public string BaseName
        {
            get { return baseName; }
            set { baseName = value; }
        }

        /// <summary>
        /// The guid used to identify this element.
        /// </summary>
        public string Guid
        {
            get { return guid; }
            set { guid = value; }
        }

        /// <summary>
        /// The current level in the hierarchy tree to obtain its value.
        /// </summary>
        public int CurrentLevel
        {
            get { return currentLevel; }
            set { currentLevel = value; }
        }

        /// <summary>
        /// The main object from where all the properties are being retrieved.
        /// </summary>
        public object PropertyInstance
        {
            get { return propertyInstance; }
            set { propertyInstance = value; }
        }

        /// <summary>
        /// The parent of this object. Only if it is not the top element.
        /// </summary>
        public PTSerializationElement Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// The children of this element, this will be filled if the element is a IEnumerable.
        /// </summary>
        public List<PTSerializationElement> Children
        {
            get { return children; }
            set { children = value; }
        }

        /// <summary>
        /// The values that will be serialized.
        /// Contain as the key the name of the property
        /// and as the value the value of that property.
        /// </summary>
        public Dictionary<string, object> SerializationTree
        {
            get { return serializationTree; }
            set { serializationTree = value; }
        }

        ///// <summary>
        ///// The members to be serialized, this is just informative.
        ///// </summary>
        //public Dictionary<MemberInfo, List<MemberInfo>> SerializationMembers
        //{
        //    get { return serializationMembers; }
        //    set { serializationMembers = value; }
        //}

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTSerializationElement()
        {
            children = new List<PTSerializationElement>();
            SerializationTree = new Dictionary<string, object>();
        }

        public string GetRecursiveParentBaseName()
        {
            PTSerializationElement current = this;
            StringBuilder result = new StringBuilder();
            int currentLevel = 0;

            while (current.Parent != null)
            {
                //result.Append(current.Parent.BaseName);
                ////result.Append(PTSerializer.SEPARATOR);
                //result.Append(PTSerializer.GetSeparator(1));
                current = current.Parent;
            }

            return result.ToString();
        }
    }
}
