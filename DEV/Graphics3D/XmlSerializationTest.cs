using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PoncheToolkit.Graphics3D
{
    [XmlRootAttribute("PurchaseOrder", Namespace = "http://www.ponche.com", IsNullable = false)]
    public class XmlSerializationTest
    {
        public PTTexturePath Path;
        public string OrderDate;
        // The XmlArrayAttribute changes the XML element name
        // from the default of "OrderedItems" to "Items".
        //[XmlArray("Items")]
        //public OrderedItem[] OrderedItems;
        public decimal SubTotal;
        public decimal ShipCost;
        public decimal TotalCost;
    }
}
