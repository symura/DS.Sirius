using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Linq;
using DS.Sirius.Core.Common;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// Stores a collection of PropertySettings items using Name and Value only
    /// </summary>
    public class PropertySettingsCollection : 
        KeyedCollection<string, PropertySettings>,
        IXElementRepresentable 
    {
        private const string PROPERTY = "Property";
        protected const string NAME = "name";
        protected const string VALUE = "value";

        protected readonly XName PropertyElementName;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="propertyElementName"></param>
        public PropertySettingsCollection(XName propertyElementName = null)
        {
            PropertyElementName = propertyElementName ?? PROPERTY;
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <returns>
        /// The key for the specified element.
        /// </returns>
        /// <param name="item">The element from which to extract the key.</param>
        protected override string GetKeyForItem(PropertySettings item)
        {
            return item.Name;
        }

        /// <summary>
        /// Gets the dictionary of propertysSettings
        /// </summary>
        public IReadOnlyDictionary<string, PropertySettings> Properties
        {
            get { return new ReadOnlyDictionary<string, PropertySettings>(
                Dictionary ?? new Dictionary<string, PropertySettings>()); }
        }

        /// <summary>
        /// Writes the object to an XElement instance.
        /// </summary>
        /// <returns>XElement representation of the object</returns>
        public virtual XElement WriteToXml(XName name)
        {
            return new XElement(name,
                                from property in this
                                select new XElement(PropertyElementName,
                                                    new XAttribute(NAME, property.Name),
                                                    new XAttribute(VALUE, property.Value)));
        }

        /// <summary>
        /// Reads the object from an XElement instance.
        /// </summary>
        /// <param name="element">XElement object</param>
        /// <returns>Object read from the XElement</returns>
        public virtual void ReadFromXml(XElement element)
        {
            Clear();
            element.ProcessItems(PropertyElementName, item => Add(new PropertySettings
                {
                    Name = item.StringAttribute(NAME),
                    Value = item.StringAttribute(VALUE)
                }));
        }
    }
}