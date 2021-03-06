﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DS.Sirius.Core.Common;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// This class contains helper methods for managing configuration settings
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Checks if the specified element has a single chils element with the specified name
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        public static XElement ExpectSingleChild(this XElement element, XName name)
        {
            var children = element.Elements(name).ToList();
            if (children.Count == 0)
            {
                throw new XmlException(
                    String.Format("Expected '{0}' element is missing.", name));
            }
            return children[0];
        }

        /// <summary>
        /// Gets the value of the specified element
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        public static string StringElement(this XElement element, XName name)
        {
            return element.ExpectSingleChild(name).Value;
        }

        /// <summary>
        /// Gets the value of the specified element given in base64 encoding
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        public static string Base64StringElement(this XElement element, XName name)
        {
            var value = element.ExpectSingleChild(name).Value;
            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Gets the value of the specified element
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        /// <param name="defaultValue">Default value</param>
        public static string OptionalStringElement(this XElement element, XName name, string defaultValue = null)
        {
            var children = element.Elements(name).ToList();
            return children.Count == 0 ? defaultValue : children[0].Value;
        }

        /// <summary>
        /// Converts the specified string to its Base64 encoded representation.
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>Base64 representation</returns>
        public static string ToBase64String(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Gets the value of the specified element in base 64 encoding
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        /// <param name="defaultValue">Default value</param>
        public static string OptionalBase64StringElement(this XElement element, XName name, string defaultValue = null)
        {
            var children = element.Elements(name).ToList();
            if (children.Count == 0) return defaultValue;
            var bytes = Convert.FromBase64String(children[0].Value);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Gets a Boolean value from the specified element
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        public static bool BooleanElement(this XElement element, XName name)
        {
            return bool.Parse(element.ExpectSingleChild(name).Value);
        }

        /// <summary>
        /// Gets the optional Boolean value of the specified element
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        /// <param name="defaultValue">Default value</param>
        public static bool OptionalBoleanElement(this XElement element, XName name, bool defaultValue = false)
        {
            var children = element.Elements(name).ToList();
            return children.Count == 0 ? defaultValue : bool.Parse(children[0].Value);
        }

        /// <summary>
        /// Gets an Int32 value from the specified element
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        public static int Int32Element(this XElement element, XName name)
        {
            return int.Parse(element.ExpectSingleChild(name).Value);
        }

        /// <summary>
        /// Gets the optional Int32 value of the specified element
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        /// <param name="defaultValue">Default value</param>
        public static int OptionalInt32Element(this XElement element, XName name, int defaultValue = 0)
        {
            var children = element.Elements(name).ToList();
            return children.Count == 0 ? defaultValue : int.Parse(children[0].Value);
        }

        /// <summary>
        /// Gets the value of the specified element
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        public static DateTime DateTimeElement(this XElement element, XName name)
        {
            return DateTime.Parse(element.ExpectSingleChild(name).Value);
        }

        /// <summary>
        /// Gets the value of the specified element
        /// </summary>
        /// <param name="element">Root element</param>
        /// <param name="name">Child element name</param>
        /// <param name="defaultValue">Default value</param>
        public static DateTime? OptionalDateTimeElement(this XElement element, XName name, DateTime? defaultValue = null)
        {
            var children = element.Elements(name).ToList();
            return children.Count == 0 || string.IsNullOrWhiteSpace(children[0].Value)
                ? defaultValue : DateTime.Parse(children[0].Value);
        }

        /// <summary>
        /// Processes the items of a container.
        /// </summary>
        /// <param name="element">Element holding the container</param>
        /// <param name="containerName">Name of the container element</param>
        /// <param name="itemName">Name of the items</param>
        /// <param name="action">Action to force on elements</param>
        /// <param name="skipInvalidItems">Set to true to skip the procession of invalid items</param>
        public static void ProcessContainerItems(this XElement element, XName containerName, XName itemName,
                                                 Action<XElement> action, bool skipInvalidItems = false)
        {
            var items = element.ExpectSingleChild(containerName);
            ProcessItems(items, itemName,action, skipInvalidItems);
        }

        /// <summary>
        /// Processes an optional element.
        /// </summary>
        /// <param name="element">Element holding the optional element</param>
        /// <param name="item">Optional element to process</param>
        /// <param name="action">Action to process the optional element</param>
        public static void ProcessOptionalElement(this XElement element, XName item, Action<XElement> action)
        {
            var children = element.Elements(item).ToList();
            if (children.Count > 1)
            {
                throw new XmlException(
                    String.Format("Expected '{0}' element has more than one occurrance.", item));
            }
            if (children.Count == 1)
            {
                action(children[0]);
            }
        }

        /// <summary>
        /// Processes the items of the specified elements.
        /// </summary>
        /// <param name="element">Element holding the container</param>
        /// <param name="itemName">Name of the items</param>
        /// <param name="action">Action to force on elements</param>
        /// <param name="skipInvalidItems">Set to true to skip the procession of invalid items</param>
        public static void ProcessItems(this XElement element, XName itemName,
                                                 Action<XElement> action, bool skipInvalidItems = false)
        {
            foreach (var item in element.Elements())
            {
                if (item.Name != itemName && !skipInvalidItems)
                {
                    throw new XmlException(
                        String.Format("Invalid element '{0}' found where '{1}' is expected.",
                        item.Name, itemName));
                }
                action(item);
            }
        }

        /// <summary>
        /// Processes the items of an optional container.
        /// </summary>
        /// <param name="element">Element holding the container</param>
        /// <param name="containerName">Name of the container element</param>
        /// <param name="itemName">Name of the items</param>
        /// <param name="action">Action to force on elements</param>
        /// <param name="skipInvalidItems">Set to true to skip the procession of invalid items</param>
        public static void ProcessOptionalContainer(this XElement element, XName containerName, XName itemName,
                                                    Action<XElement> action, bool skipInvalidItems = false)
        {
            element.ProcessOptionalElement(containerName, 
                item => item.ProcessItems(itemName, action, skipInvalidItems));
        }

        /// <summary>
        /// Gets a string attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <returns>Attribute value</returns>
        public static string StringAttribute(this XElement element, XName name)
        {
            var attr = element.Attribute(name);
            if (attr == null)
            {
                throw new XmlException(
                   String.Format("Expected '{0}' attribute is missing.", name));
            }
            return attr.Value;
        }

        /// <summary>
        /// Gets a non-whitespace string attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <returns>Attribute value</returns>
        public static string NonWhiteSpaceStringAttribute(this XElement element, XName name)
        {
            var value = StringAttribute(element, name);
            if (String.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException(
                    String.Format("'{0}' attribute must contain non-whitespace characters", name));
            }
            return value;
        }

        /// <summary>
        /// Gets a string attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns></returns>
        public static string OptionalStringAttribute(this XElement element, XName name, 
            string defaultValue = "")
        {
            var attr = element.Attribute(name);
            return attr == null ? defaultValue : attr.Value;
        }

        /// <summary>
        /// Gets a string attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <param name="resolver">Type name resolver</param>
        /// <returns>Attribute value</returns>
        public static Type TypeAttribute(this XElement element, XName name, ITypeResolver resolver)
        {
            var attr = element.Attribute(name);
            if (attr == null)
            {
                throw new XmlException(
                   String.Format("Expected '{0}' attribute is missing.", name));
            }
            return resolver.Resolve(attr.Value);
        }

        /// <summary>
        /// Gets a string attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <returns>Attribute value</returns>
        public static Type TypeAttribute(this XElement element, XName name)
        {
            return TypeAttribute(element, name, AppConfigurationManager.TypeResolver);
        }

        /// <summary>
        /// Gets a string attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <param name="resolver">Type name resolver</param>
        /// <param name="defaultType">Default type if not specified</param>
        /// <returns>Attribute value</returns>
        public static Type OptionalTypeAttribute(this XElement element, XName name, ITypeResolver resolver,
            Type defaultType = null)
        {
            var attr = element.Attribute(name);
            return attr == null ? defaultType : resolver.Resolve(attr.Value);
        }

        /// <summary>
        /// Gets a string attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <param name="defaultType">Default type if not specified</param>
        /// <returns>Attribute value</returns>
        public static Type OptionalTypeAttribute(this XElement element, XName name, Type defaultType = null)
        {
            return OptionalTypeAttribute(element, name, AppConfigurationManager.TypeResolver, defaultType);
        }

        /// <summary>
        /// Gets a bool attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <returns>Attribute value</returns>
        public static bool BoolAttribute(this XElement element, XName name)
        {
            var attr = element.Attribute(name);
            if (attr == null)
            {
                throw new XmlException(
                   String.Format("Expected '{0}' attribute is missing.", name));
            }
            return bool.Parse(attr.Value);
        }

        /// <summary>
        /// Gets an optional bool attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <param name="defaultValue">Default attribute value</param>
        /// <returns>Attribute value</returns>
        public static bool OptionalBoolAttribute(this XElement element, XName name, bool defaultValue = false)
        {
            var attr = element.Attribute(name);
            return attr == null ? defaultValue : bool.Parse(attr.Value);
        }

        /// <summary>
        /// Gets an integer attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <returns>Attribute value</returns>
        public static int IntAttribute(this XElement element, XName name)
        {
            var attr = element.Attribute(name);
            if (attr == null)
            {
                throw new XmlException(
                   String.Format("Expected '{0}' attribute is missing.", name));
            }
            return int.Parse(attr.Value);
        }

        /// <summary>
        /// Gets an optional int attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <param name="defaultValue">Default attribute value</param>
        /// <returns>Attribute value</returns>
        public static int OptionalIntAttribute(this XElement element, XName name, int defaultValue = 0)
        {
            var attr = element.Attribute(name);
            return attr == null ? defaultValue : int.Parse(attr.Value);
        }

        /// <summary>
        /// Gets a bool attribute from the specified element with the given name
        /// </summary>
        /// <param name="element">Element to get the attribute from</param>
        /// <param name="name">Attribute name</param>
        /// <returns>Attribute value</returns>
        public static T EnumAttribute<T>(this XElement element, XName name)
        {
            var attr = element.Attribute(name);
            if (attr == null)
            {
                throw new XmlException(
                   String.Format("Expected '{0}' attribute is missing.", name));
            }
            return (T)Enum.Parse(typeof(T), attr.Value);
        }

        /// <summary>
        /// Instantiates an object and sets its properties as provided.
        /// </summary>
        /// <param name="type">Type to instantiate</param>
        /// <param name="properties">Collection of properties to set up</param>
        /// <returns>The newly instantiated object</returns>
        public static object PrepareInstance(Type type, PropertySettingsCollection properties)
        {
            // --- Instantiate the object
            var tempInstance = Activator.CreateInstance(type);
            InjectProperties(ref tempInstance, properties);
            return tempInstance;
        }

        /// <summary>
        /// Instantiates an object and sets its properties as provided.
        /// </summary>
        /// <param name="type">Type to instantiate</param>
        /// <param name="constructorParams">Collection of construction parameters</param>
        /// <param name="properties">Collection of properties to set up</param>
        /// <returns>The newly instantiated object</returns>
        public static object PrepareInstance(Type type, UnnamedPropertySettingsCollection constructorParams, 
            PropertySettingsCollection properties)
        {
            // --- Create the constructor parameter array
            var parameters = new object[constructorParams.Count];
            for (var i = 0; i < constructorParams.Count; i++)
            {
                var converter = TypeDescriptor.GetConverter(constructorParams[i].Type);
                parameters[i] = converter.ConvertFromString(constructorParams[i].Value);
            }

            // --- Instantiate the object
            var tempInstance = Activator.CreateInstance(type, parameters);
            InjectProperties(ref tempInstance, properties);
            return tempInstance;
        }

        /// <summary>
        /// Sets the properties of an object as provided.
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="properties">Object properties</param>
        public static void InjectProperties(ref object instance, IEnumerable<PropertySettings> properties)
        {
            if (properties == null) return;
            var type = instance.GetType();
            foreach (var parameter in properties)
            {
                // --- Set up the initial values of properties
                var propInfo = type.GetProperty(parameter.Name, BindingFlags.Instance | BindingFlags.Public);
                if (propInfo == null)
                {
                    // --- Undefined property used
                    throw new ConfigurationErrorsException(
                        string.Format("Type {0} does not have '{1}' public property.",
                                      type, parameter.Name));
                }
                object objectValue;
                if (propInfo.PropertyType.IsEnum)
                {
                    objectValue = Enum.Parse(propInfo.PropertyType, parameter.Value);
                }
                else
                {
                    objectValue = Convert.ChangeType(parameter.Value, propInfo.PropertyType,
                        CultureInfo.InvariantCulture);
                }
                propInfo.SetValue(instance, objectValue, null);
            }
        }
    }
}