using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Xml.Linq;
using DS.Sirius.Core.Common;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// Defines an abstract class that serves as a base class for a configuration definition-
    /// </summary>
    /// <typeparam name="T">Type of the service object described by this configuration.</typeparam>
    public abstract class ObjectSettingsBase<T> :
        ConfigurationSettingsBase,
        IDisposable
        where T : class
    {
        private const string CONSTRUCT = "Construct";
        private const string PROPERTIES = "Properties";
        private const string NAME = "name";
        private const string TYPE = "type";
        private const string PARAM = "Param";

        /// <summary>Stores the parameters of the logger definition.</summary>
        private readonly PropertySettingsCollection _params = new PropertySettingsCollection();

        /// <summary>Caches the logger instance</summary>
        private T _instance;

        /// <summary>Mutex managing instantiation</summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets the name of the root for XML serialization
        /// </summary>
        protected virtual string XmlRootName
        {
            get { return GetType().Name; }
        }

        protected ObjectSettingsBase()
        {
            ConstructorParameters = new UnnamedPropertySettingsCollection(PARAM);
            Properties = new PropertySettingsCollection();
        }

        /// <summary>
        /// Creates a new instance using the specified name and type.
        /// </summary>
        /// <param name="name">Name of the definition</param>
        /// <param name="type">Type implementing the definition</param>
        protected ObjectSettingsBase(string name, Type type): this()
        {
            CheckConstructionParameters(name, type);
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Creates a new instance using the specified name and instance.
        /// </summary>
        /// <param name="name">Name of the definition</param>
        /// <param name="instance">Object instance</param>
        protected ObjectSettingsBase(string name, T instance) :
            this(name, typeof(T))
        {
            _instance = instance;
        }

        /// <summary>
        /// Creates a new object definition instance by deserializing the specified XElement.
        /// </summary>
        /// <param name="element">XElement representation</param>
        protected ObjectSettingsBase(XElement element): this()
        {
            ((IXElementRepresentable)this).ReadFromXml(element);
            CheckConstructionParameters(Name, Type);
        }

        private static void CheckConstructionParameters(string name, Type type)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    string.Format("{0} object name cannot be null, empty or only whitespace.", typeof(T)), "name");
            }
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException("The T type parameter must be an interface type.", "type");
            }
            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new InvalidOperationException(
                    String.Format("Type {0} does not implement {1}.", type, typeof(T)));
            }
        }

        /// <summary>
        /// Gets the name of the logger definition.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the implementing type of the logger definition.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets constructor parameters
        /// </summary>
        public UnnamedPropertySettingsCollection ConstructorParameters { get; set; }

        /// <summary>
        /// Gets propertu settings
        /// </summary>
        public PropertySettingsCollection Properties { get; set; }

        /// <summary>
        /// Gets the read-only list of parameters.
        /// </summary>
        public IReadOnlyList<PropertySettings> Parameters
        {
            get { return new ReadOnlyCollection<PropertySettings>(_params); }
        }

        /// <summary>
        /// Adds a new parameter to the logger definition.
        /// </summary>
        /// <param name="parameter">Parameter definition to add</param>
        public void AddParameter(PropertySettings parameter)
        {
            _params.Add(parameter);
        }

        /// <summary>
        /// Adds a new parameter to the logger definition.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public void AddParameter(string name, string value)
        {
            _params.Add(new PropertySettings { Name = name, Value = value });
        }

        /// <summary>
        /// Removes a parameter from the logger definition
        /// </summary>
        /// <param name="parameter">Parameter definition to remove</param>
        /// <returns>True, if the parametes has been removed; otherwise, false.</returns>
        public bool RemoveParameter(PropertySettings parameter)
        {
            return _params.Remove(parameter);
        }

        /// <summary>
        /// Fluent interface method to use the specified parameter.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns>This instance</returns>
        public ObjectSettingsBase<T> Use(string name, string value)
        {
            _params.Add(new PropertySettings { Name = name, Value = value});
            return this;
        }

        /// <summary>
        /// Creates an instance of the specified logger.
        /// </summary>
        /// <returns></returns>
        public T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // --- Only one thread can initialize the instance
                    lock (_lockObject)
                    {
                        try
                        {
                            // --- Check whether the instance has been created by another thread
                            if (_instance == null)
                            {
                                _instance = CreateInstance();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new ConfigurationErrorsException(
                                string.Format("Error creating a {0} instance", Type), ex);
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Creates the specified object instance
        /// </summary>
        /// <returns>The newly created object instance</returns>
        protected virtual T CreateInstance()
        {
            return (T) ConfigurationHelper.PrepareInstance(Type, ConstructorParameters, Properties);
        }

        /// <summary>
        /// Writes the object to an XElement instance using the specified root element name.
        /// </summary>
        /// <param name="rootElement">Root element name</param>
        /// <returns>XElement representation of the object</returns>
        public override XElement WriteToXml(XName rootElement)
        {
            return new XElement(rootElement,
                                new XAttribute(NAME, Name),
                                new XAttribute(TYPE, Type == null ? "" : Type.AssemblyQualifiedName ?? ""),
                                ConstructorParameters.WriteToXml(CONSTRUCT),
                                Properties.WriteToXml(PROPERTIES));
        }

        /// <summary>
        /// Reads the object from an XElement instance.
        /// </summary>
        /// <param name="element">XElement object</param>
        /// <returns>Object read from the XElement</returns>
        protected override void ParseFrom(XElement element)
        {
            Name = element.StringAttribute(NAME);
            Type = element.TypeAttribute(TYPE, AppConfigurationManager.TypeResolver);
            _params.Clear();
            element.ProcessOptionalElement(CONSTRUCT, item => ConstructorParameters.ReadFromXml(item));
            element.ProcessOptionalElement(PROPERTIES, item => Properties.ReadFromXml(item));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}