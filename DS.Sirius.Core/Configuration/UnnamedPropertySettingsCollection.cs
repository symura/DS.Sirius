using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// Stores a collection of PropertySettings items using Name, Value, and Type
    /// </summary>
    public class UnnamedPropertySettingsCollection : PropertySettingsCollection
    {
        protected const string TYPE = "type";

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="propertyElementName"></param>
        public UnnamedPropertySettingsCollection(XName propertyElementName = null) : base(propertyElementName)
        {
        }

        /// <summary>
        /// Writes the object to an XElement instance.
        /// </summary>
        /// <returns>XElement representation of the object</returns>
        public override XElement WriteToXml(XName name)
        {
            return new XElement(name,
                                from property in this
                                select new XElement(PropertyElementName,
                                                    new XAttribute(VALUE, property.Value),
                                                    new XAttribute(TYPE, property.Type)));
        }

        /// <summary>
        /// Reads the object from an XElement instance.
        /// </summary>
        /// <param name="element">XElement object</param>
        /// <returns>Object read from the XElement</returns>
        public override void ReadFromXml(XElement element)
        {
            Clear();
            var counter = 0;
            element.ProcessItems(PropertyElementName, item => Add(new PropertySettings
                {
                    Name = (++counter).ToString(CultureInfo.InvariantCulture),
                    Value = item.StringAttribute(VALUE),
                    Type = item.TypeAttribute(TYPE)
                }));
        }
    }
}