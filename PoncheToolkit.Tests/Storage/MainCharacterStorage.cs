using PoncheToolkit.Util.Reflection;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Tests.Storage
{
    [PTSerializableClass(typeof(MainCharacterStorage), SerializationKind.All)]
    public class MainCharacterStorage
    {
        public Vector3 Positon { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Size { get; set; }
        public string Name { get; set; }

        public List<Vector3> ListTest { get; set; }
    }
}
