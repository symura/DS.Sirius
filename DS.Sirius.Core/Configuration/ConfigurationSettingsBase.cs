using System;
using System.Xml;
using System.Xml.Linq;
using DS.Sirius.Core.Common;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// This class is intended to be the base class of a type describing
    /// configuration settings.
    /// </summary>
    public abstract class ConfigurationSettingsBase: IXElementRepresentable
    {
        /// <summary>
        /// Gets the default root node name.
        /// </summary>
        public XName DefaultRoot { get; protected set; }
        
        /// <summary>
        /// Creates an isntace of the class, using the "Settings" default name.
        /// </summary>
        protected ConfigurationSettingsBase()
        {
            DefaultRoot = "Settings";
        }

        protected ConfigurationSettingsBase(XName element)
        {
            DefaultRoot = element;
        }

        /// <summary>
        /// Writes the object to an XElement instance using the specified root element name.
        /// </summary>
        /// <param name="rootElement">Root element name</param>
        /// <returns>XElement representation of the object</returns>
        public abstract XElement WriteToXml(XName rootElement);

        /// <summary>
        /// Writes the object to an XElement instance.
        /// </summary>
        /// <returns>XElement representation of the object</returns>
        public XElement WriteToXml()
        {
            return WriteToXml(DefaultRoot);
        }

        /// <summary>
        /// Reads the configuration elements from the specified element
        /// </summary>
        /// <param name="element">Element to read the configuration from</param>
        /// <param name="rootName">Root element name to check</param>
        public void ReadFromXml(XElement element, XName rootName)
        {
            try
            {
                ParseFrom(element);
            }
            catch (Exception ex)
            {
                throw new XmlException(
                    "An exception has been caught when reading an XML configuration setting", ex);
            }    
        }

        /// <summary>
        /// Reads the configuration elements from the specified element
        /// </summary>
        /// <param name="element"></param>
        public void ReadFromXml(XElement element)
        {
            ReadFromXml(element, DefaultRoot);
        }

        /// <summary>
        /// Parse the specified configuration settings
        /// </summary>
        /// <param name="element">Element holding configuration settings</param>
        protected abstract void ParseFrom(XElement element);

        /// <summary>
        /// Clones this configutarion element to the specified type.
        /// </summary>
        /// <typeparam name="T">Destination setting type</typeparam>
        /// <returns>Clone of the settings class</returns>
        protected T Clone<T>()
            where T : class, IXElementRepresentable
        {
            var parameters = new object[] { WriteToXml() };
            var clone = (T)Activator.CreateInstance(typeof (T), parameters);
            return clone;
        }
    }
}
