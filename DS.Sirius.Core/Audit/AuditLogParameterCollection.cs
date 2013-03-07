using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.Common;

namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This class implements a collection of <see cref="AuditLogParameter" /> items
    /// that can be addressed by the Name property.
    /// </summary>
    public class AuditLogParameterCollection : IXElementRepresentable
    {
        private const string PARAMETER = "Parameter";
        private const string NAME = "name";
        private const string VALUE = "value";

        private readonly List<AuditLogParameter> _items;

        /// <summary>
        /// Root XML element name
        /// </summary>
        public const string ROOT = "ConstructorParameters";

        /// <summary>
        /// Initializes a new instance of this class that uses the default equality comparer.
        /// </summary>
        public AuditLogParameterCollection(List<AuditLogParameter> items)
        {
            _items = items;
        }

        /// <summary>
        /// Creates a new object definition instance by deserializing the specified XElement.
        /// </summary>
        /// <param name="element">XElement representation</param>
        public AuditLogParameterCollection(XElement element)
        {
            _items = new List<AuditLogParameter>();
            ((IXElementRepresentable)this).ReadFromXml(element);
        }

        /// <summary>
        /// Gest or sets the audit log parameter items.
        /// </summary>
        public List<AuditLogParameter> Items
        {
            get { return _items; }
        }

        public XElement WriteToXml(XName name)
        {
            return
                new XElement(name,
                from item in _items
                select new XElement(PARAMETER,
                    new XAttribute(NAME, item.Name),
                    item.Value == null ? null : new XAttribute(VALUE, item.Value)));
        }

        /// <summary>
        /// Reads the object from an XElement instance.
        /// </summary>
        /// <param name="element">XElement object</param>
        /// <returns>Object read from the XElement</returns>
        void IXElementRepresentable.ReadFromXml(XElement element)
        {
            try
            {
                _items.Clear();
                element.ProcessItems(PARAMETER, item =>
                    {
                        var name = item.OptionalStringAttribute(NAME, null);
                        var value = item.OptionalStringAttribute(VALUE, null);
                        _items.Add(new AuditLogParameter(name, value));
                    });
            }
            catch (Exception ex)
            {
                throw new XmlException("Audit log parameter definition cannot be parsed from " +
                    "the provided XML information", ex);
            }
        }
    }
}