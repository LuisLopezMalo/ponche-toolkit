using Assimp.Unmanaged;
using System;
using System.Diagnostics;
using System.Text;

namespace PT.Graphics3D
{
    using SharpDX;

    /// <summary>
    /// A key-value pairing that represents some material property.
    /// </summary>
    internal sealed class MaterialProperty : ModelPiece
    {
        private string m_name;
        private AssimpPropertyType m_type;
        private byte[] m_rawValue;
        private AssimpTextureType m_texType;
        private int m_texIndex;
        private string m_fullyQualifiedName;
        private bool m_fullQualifiedNameNeedsUpdate = true;

        /// <summary>
        /// Gets or sets the property key name. E.g. $tex.file. This corresponds to the
        /// "AiMatKeys" base name constants.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                m_fullQualifiedNameNeedsUpdate = true;

                AssertIsBaseName();
            }
        }

        /// <summary>
        /// Gets or sets the type of property.
        /// </summary>
        public AssimpPropertyType PropertyType { get { return m_type; } set { m_type = value; } }

        /// <summary>
        /// Gets the raw byte data count.
        /// </summary>
        public int ByteCount { get { return (m_rawValue == null) ? 0 : m_rawValue.Length; } }

        /// <summary>
        /// Checks if the property has data.
        /// </summary>
        public bool HasRawData { get { return m_rawValue != null; } }

        /// <summary>
        /// Gets the raw byte data. To modify/read this data, see the Get/SetXXXValue methods.
        /// </summary>
        public byte[] RawData { get { return m_rawValue; } }

        /// <summary>
        /// Gets or sets the texture type semantic, for non-texture properties this is always <see cref="Assimp.TextureType.None"/>.
        /// </summary>
        public AssimpTextureType TextureType
        {
            get
            {
                return m_texType;
            }
            set
            {
                m_texType = value;
                m_fullQualifiedNameNeedsUpdate = true;
            }
        }

        /// <summary>
        /// Gets or sets the texture index, for non-texture properties this is always zero.
        /// </summary>
        public int TextureIndex
        {
            get
            {
                return m_texIndex;
            }
            set
            {
                m_texIndex = value;
                m_fullQualifiedNameNeedsUpdate = true;
            }
        }

        /// <summary>
        /// Gets the property's fully qualified name. Format: "{base name},{texture type semantic},{texture index}". E.g. "$clr.diffuse,0,0". This
        /// is the key that is used to index the property in the material property map.
        /// </summary>
        public string FullyQualifiedName
        {
            get
            {
                if (m_fullQualifiedNameNeedsUpdate)
                {
                    m_fullyQualifiedName = AssimpMaterial.CreateFullyQualifiedName(m_name, m_texType, m_texIndex);
                    m_fullQualifiedNameNeedsUpdate = false;
                }

                return m_fullyQualifiedName;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class.
        /// </summary>
        public MaterialProperty()
        {
            m_name = string.Empty;
            m_type = AssimpPropertyType.Buffer;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = null;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Constructs a buffer property.
        /// </summary>
        /// <param name="baseName">Base name of the property</param>
        /// <param name="buffer">Property value</param>
        public MaterialProperty(string baseName, byte[] buffer)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.Buffer;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = buffer;

            AssertIsBaseName();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Constructs a float property.
        /// </summary>
        /// <param name="baseName">Base name of the property</param>
        /// <param name="value">Property value</param>
        public MaterialProperty(string baseName, float value)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.Float;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = null;

            SetFloatValue(value);
            AssertIsBaseName();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Constructs an integer property.
        /// </summary>
        /// <param name="baseName">Base name of the property</param>
        /// <param name="value">Property value</param>
        public MaterialProperty(string baseName, int value)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.Integer;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = null;

            SetIntegerValue(value);
            AssertIsBaseName();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Constructs a boolean property.
        /// </summary>
        /// <param name="baseName">Name of the property</param>
        /// <param name="value">Property value</param>
        public MaterialProperty(string baseName, bool value)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.Integer;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = null;

            SetBooleanValue(value);
            AssertIsBaseName();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Creates a string property.
        /// </summary>
        /// <param name="baseName">Base name of the property</param>
        /// <param name="value">Property value</param>
        public MaterialProperty(string baseName, string value)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.String;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = null;

            SetStringValue(value);
            AssertIsBaseName();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Creates a texture property.
        /// </summary>
        /// <param name="baseName">Base name of the property</param>
        /// <param name="value">Property value</param>
        /// <param name="texType">Texture type</param>
        /// <param name="textureIndex">Texture index</param>
        public MaterialProperty(string baseName, string value, AssimpTextureType texType, int textureIndex)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.String;
            m_texIndex = textureIndex;
            m_texType = texType;
            m_rawValue = null;

            SetStringValue(value);
            AssertIsBaseName();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Creates a float array property.
        /// </summary>
        /// <param name="baseName">Base name of the property</param>
        /// <param name="values">Property values</param>
        public MaterialProperty(string baseName, float[] values)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.Float;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = null;

            SetFloatArrayValue(values);
            AssertIsBaseName();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Creates a int array property.
        /// </summary>
        /// <param name="baseName">Base name of the property</param>
        /// <param name="values">Property values</param>
        public MaterialProperty(string baseName, int[] values)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.Integer;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = null;

            SetIntegerArrayValue(values);
            AssertIsBaseName();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Creates a Color3 property.
        /// </summary>
        /// <param name="baseName">Base name of the property</param>
        /// <param name="value">Property value</param>
        public MaterialProperty(string baseName, Color3 value)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.Float;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = null;

            SetColor3DValue(value);
            AssertIsBaseName();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="MaterialProperty"/> class. Creates a Color property.
        /// </summary>
        /// <param name="baseName">Base name of the property</param>
        /// <param name="value">Property value</param>
        public MaterialProperty(string baseName, Color value)
        {
            m_name = baseName;
            m_type = AssimpPropertyType.Float;
            m_texIndex = 0;
            m_texType = AssimpTextureType.None;
            m_rawValue = null;

            SetColor4DValue(value);
            AssertIsBaseName();
        }

        /// <summary>
        /// Gets the property raw data as a float.
        /// </summary>
        /// <returns>Float</returns>
        public float GetFloatValue()
        {
            if (m_type == AssimpPropertyType.Float || m_type == AssimpPropertyType.Integer)
                return GetValueAs<float>();

            return 0;
        }

        /// <summary>
        /// Sets the property raw data with a float.
        /// </summary>
        /// <param name="value">Float.</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetFloatValue(float value)
        {
            if (m_type != AssimpPropertyType.Float && m_type != AssimpPropertyType.Integer)
                return false;

            return SetValueAs<float>(value);
        }

        /// <summary>
        /// Gets the property raw data as an integer.
        /// </summary>
        /// <returns>Integer</returns>
        public int GetIntegerValue()
        {
            if (m_type == AssimpPropertyType.Float || m_type == AssimpPropertyType.Integer)
                return GetValueAs<int>();

            return 0;
        }

        /// <summary>
        /// Sets the property raw data as an integer.
        /// </summary>
        /// <param name="value">Integer</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetIntegerValue(int value)
        {
            if (m_type != AssimpPropertyType.Float && m_type != AssimpPropertyType.Integer)
                return false;

            return SetValueAs<int>(value);
        }

        /// <summary>
        /// Gets the property raw data as a string.
        /// </summary>
        /// <returns>string</returns>
        public string GetStringValue()
        {
            if (m_type != AssimpPropertyType.String)
                return null;

            return GetMaterialString(m_rawValue);
        }

        /// <summary>
        /// Sets the property raw data as string.
        /// </summary>
        /// <param name="value">string</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetStringValue(string value)
        {
            if (m_type != AssimpPropertyType.String)
                return false;

            m_rawValue = SetMaterialString(value, m_rawValue);
            return true;
        }

        /// <summary>
        /// Gets the property raw data as a float array.
        /// </summary>
        /// <param name="count">Number of elements to get</param>
        /// <returns>Float array</returns>
        public float[] GetFloatArrayValue(int count)
        {
            if (m_type == AssimpPropertyType.Float || m_type == AssimpPropertyType.Integer)
                return GetValueArrayAs<float>(count);

            return null;
        }

        /// <summary>
        /// Gets the property raw data as a float array.
        /// </summary>
        /// <returns>Float array</returns>
        public float[] GetFloatArrayValue()
        {
            if (m_type == AssimpPropertyType.Float || m_type == AssimpPropertyType.Integer)
            {
                int count = ByteCount / sizeof(float);
                return GetValueArrayAs<float>(count);
            }

            return null;
        }

        /// <summary>
        /// Sets the property raw data as a float array.
        /// </summary>
        /// <param name="values">Values to set</param>
        /// <returns>True if successful, otherwise false</returns>
        public bool SetFloatArrayValue(float[] values)
        {
            if (m_type != AssimpPropertyType.Float && m_type != AssimpPropertyType.Integer)
                return false;

            return SetValueArrayAs<float>(values);
        }

        /// <summary>
        /// Gets the property raw data as an integer array.
        /// </summary>
        /// <param name="count">Number of elements to get</param>
        /// <returns>Integer array</returns>
        public int[] GetIntegerArrayValue(int count)
        {
            if (m_type == AssimpPropertyType.Float || m_type == AssimpPropertyType.Integer)
                return GetValueArrayAs<int>(count);

            return null;
        }

        /// <summary>
        /// Gets the property raw data as an integer array.
        /// </summary>
        /// <returns>Integer array</returns>
        public int[] GetIntegerArrayValue()
        {
            if (m_type == AssimpPropertyType.Float || m_type == AssimpPropertyType.Integer)
            {
                int count = ByteCount / sizeof(int);
                return GetValueArrayAs<int>(count);
            }

            return null;
        }

        /// <summary>
        /// Sets the property raw data as an integer array.
        /// </summary>
        /// <param name="values">Values to set</param>
        /// <returns>True if successful, otherwise false</returns>
        public bool SetIntegerArrayValue(int[] values)
        {
            if (m_type != AssimpPropertyType.Float && m_type != AssimpPropertyType.Integer)
                return false;

            return SetValueArrayAs<int>(values);
        }

        /// <summary>
        /// Gets the property raw data as a boolean.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool GetBooleanValue()
        {
            return (GetIntegerValue() == 0) ? false : true;
        }

        /// <summary>
        /// Sets the property raw data as a boolean.
        /// </summary>
        /// <param name="value">Boolean value</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetBooleanValue(bool value)
        {
            return SetIntegerValue((value == false) ? 0 : 1);
        }

        /// <summary>
        /// Gets the property raw data as a Color3.
        /// </summary>
        /// <returns>Color3</returns>
        public Color3 GetColor3DValue()
        {
            if (m_type != AssimpPropertyType.Float)
                return new Color3();

            return GetValueAs<Color3>();
        }

        /// <summary>
        /// Sets the property raw data as a Color3.
        /// </summary>
        /// <param name="value">Color3</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetColor3DValue(Color3 value)
        {
            if (m_type != AssimpPropertyType.Float)
                return false;

            return SetValueAs<Color3>(value);
        }

        /// <summary>
        /// Gets the property raw data as a Color.
        /// </summary>
        /// <returns>Color</returns>
        public Color GetColor4DValue()
        {
            if (m_type != AssimpPropertyType.Float || m_rawValue == null)
                return new Color();

            //We may have a Color that's RGB, so still read it and set alpha to 1.0
            unsafe
            {
                fixed (byte* ptr = m_rawValue)
                {

                    if (m_rawValue.Length >= Assimp.MemoryHelper.SizeOf<Color>())
                    {
                        return Assimp.MemoryHelper.Read<Color>(new IntPtr(ptr));
                    }
                    else if (m_rawValue.Length >= Assimp.MemoryHelper.SizeOf<Color3>())
                    {
                        return new Color(Assimp.MemoryHelper.Read<Color3>(new IntPtr(ptr)), 1.0f);
                    }

                }
            }

            return new Color();
        }

        /// <summary>
        /// Sets the property raw data as a Color.
        /// </summary>
        /// <param name="value">Color</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SetColor4DValue(Color value)
        {
            if (m_type != AssimpPropertyType.Float)
                return false;

            return SetValueAs<Color>(value);
        }

        private unsafe T[] GetValueArrayAs<T>(int count) where T : struct
        {
            int size = Assimp.MemoryHelper.SizeOf<T>();

            if (m_rawValue != null && (m_rawValue.Length >= (size * count)))
            {
                T[] array = new T[count];
                fixed (byte* ptr = m_rawValue)
                {
                    Assimp.MemoryHelper.Read<T>(new IntPtr(ptr), array, 0, count);
                }

                return array;
            }

            return null;
        }

        private unsafe T GetValueAs<T>() where T : struct
        {
            int size = Assimp.MemoryHelper.SizeOf<T>();

            if (m_rawValue != null && m_rawValue.Length >= size)
            {
                fixed (byte* ptr = m_rawValue)
                {
                    return Assimp.MemoryHelper.Read<T>(new IntPtr(ptr));
                }
            }

            return default(T);
        }

        private unsafe bool SetValueArrayAs<T>(T[] data) where T : struct
        {
            if (data == null || data.Length == 0)
                return false;

            int size = Assimp.MemoryHelper.SizeOf<T>(data);

            //Resize byte array if necessary
            if (m_rawValue == null || m_rawValue.Length != size)
                m_rawValue = new byte[size];

            fixed (byte* ptr = m_rawValue)
            {
                Assimp.MemoryHelper.Write<T>(new IntPtr(ptr), data, 0, data.Length);
            }

            return true;
        }

        private unsafe bool SetValueAs<T>(T value) where T : struct
        {
            int size = Assimp.MemoryHelper.SizeOf<T>();

            //Resize byte array if necessary
            if (m_rawValue == null || m_rawValue.Length != size)
                m_rawValue = new byte[size];

            fixed (byte* ptr = m_rawValue)
            {
                Assimp.MemoryHelper.Write<T>(new IntPtr(ptr), ref value);
            }

            return true;
        }

        private static unsafe string GetMaterialString(byte[] matPropData)
        {
            if (matPropData == null)
                return string.Empty;

            fixed (byte* ptr = &matPropData[0])
            {
                //string is stored as 32 bit length prefix THEN followed by zero-terminated UTF8 data (basically need to reconstruct an AiString)
                AiString aiString;
                aiString.Length = new UIntPtr((uint)Assimp.MemoryHelper.Read<int>(new IntPtr(ptr)));

                //Memcpy starting at dataPtr + sizeof(int) for length + 1 (to account for null terminator)
                Assimp.MemoryHelper.CopyMemory(new IntPtr(aiString.Data), Assimp.MemoryHelper.AddIntPtr(new IntPtr(ptr), sizeof(int)), (int)aiString.Length.ToUInt32() + 1);

                return aiString.GetString();
            }
        }

        private static unsafe byte[] SetMaterialString(string value, byte[] existing)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            int stringSize = Encoding.UTF8.GetByteCount(value);

            if (stringSize < 0)
                return null;

            int size = stringSize + 1 + sizeof(int);
            byte[] data = existing;

            if (existing == null || existing.Length != size)
                data = new byte[size];

            fixed (byte* bytePtr = &data[0])
            {
                Assimp.MemoryHelper.Write<int>(new IntPtr(bytePtr), ref stringSize);
                byte[] utfBytes = Encoding.UTF8.GetBytes(value);
                Assimp.MemoryHelper.Write<byte>(new IntPtr(bytePtr + sizeof(int)), utfBytes, 0, utfBytes.Length);
                //Last byte should be zero
            }

            return data;
        }

        [Conditional("DEBUG")]
        private void AssertIsBaseName()
        {
            Debug.Assert(!m_name.Contains(","));
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            throw new NotImplementedException();
        }
    }
}
