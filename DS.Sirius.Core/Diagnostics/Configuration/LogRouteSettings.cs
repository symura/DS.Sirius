using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using DS.Sirius.Core.Configuration;

namespace DS.Sirius.Core.Diagnostics.Configuration
{
    /// <summary>
    /// Defines a route that sends a log item to an appropriate logger.
    /// </summary>
    public sealed class LogRouteSettings : ConfigurationSettingsBase 
    {
        private const string LOGGER = "Logger";
        private const string NAME = "name";
        private const string ENABLED = "enabled";
        private const string FILTERS = "Filters";

        private readonly List<LogFilterSettingsBase> _filters =
            new List<LogFilterSettingsBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LogRouteSettings"/> class.
        /// </summary>
        public LogRouteSettings(string name, bool enabled, DiagnosticsLoggerSettings diagnosticsLogger)
        {
            Name = name;
            Enabled = enabled;
            DiagnosticsLogger = diagnosticsLogger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogRouteSettings"/> class.
        /// </summary>
        public LogRouteSettings(DiagnosticsLoggerSettings diagnosticsLogger)
            : this("default", true, diagnosticsLogger)
        {
        }

        /// <summary>
        /// Creates an instance from the specified XML element.
        /// </summary>
        /// <param name="element">XML element to create this instance from</param>
        public LogRouteSettings(XElement element)
        {
            ReadFromXml(element);
        }

        /// <summary>
        /// Gets the name of the log route definition
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the flag indicating whether the route is enabled.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// Gets the logger that is associated with this route.
        /// </summary>
        public DiagnosticsLoggerSettings DiagnosticsLogger { get; private set; }

        /// <summary>
        /// Adds a new filter to the route.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void AddFilter(LogFilterSettingsBase filter)
        {
            _filters.Add(filter);
        }

        /// <summary>
        /// Fluent method to add a filter to the log route.
        /// </summary>
        /// <param name="filter">Filter instance</param>
        /// <returns>The log route instance itself</returns>
        public LogRouteSettings WithFilter(LogFilterSettingsBase filter)
        {
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// Removes the specified filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>The log route instance itself</returns>
        public bool RemoveFilter(LogFilterSettingsBase filter)
        {
            return _filters.Remove(filter);
        }

        /// <summary>
        /// Gets the filters defined for this route
        /// </summary>
        public ReadOnlyCollection<LogFilterSettingsBase> Filters
        {
            get { return new ReadOnlyCollection<LogFilterSettingsBase>(_filters); }
        }

        /// <summary>
        /// Checks whether the specified entry mathces with all filters within the route.
        /// </summary>
        /// <param name="entry">Log data entry</param>
        /// <returns>True, if the entry matches with all filters; otherwise, false.</returns>
        public bool MatchesFilters(DiagnosticsLogItem entry)
        {
            return _filters.All(filter => filter.MatchesEntry(entry));
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
                new XAttribute(ENABLED, Enabled),
                DiagnosticsLogger.WriteToXml(LOGGER),
                new XElement(FILTERS,
                    from filter in Filters
                    select filter.WriteToXml(filter.SettingsRootName)));
        }

        /// <summary>
        /// Parse the specified configuration settings
        /// </summary>
        /// <param name="element">Element holding configuration settings</param>
        protected override void ParseFrom(XElement element)
        {
            Name = element.StringAttribute(NAME);
            Enabled = element.BoolAttribute(ENABLED);
            DiagnosticsLogger = new DiagnosticsLoggerSettings(element.Element(LOGGER));
            if (element.Element(FILTERS) == null) return;
            // ReSharper disable PossibleNullReferenceException
            foreach (var filter in element.Element(FILTERS).Descendants())
            // ReSharper restore PossibleNullReferenceException
            {
                if (filter.Name == LogTypeFilterSettings.ROOT)
                {
                    _filters.Add(new LogTypeFilterSettings(filter));
                }
                else if (filter.Name == LogSourceFilterSettings.ROOT)
                {
                    _filters.Add(new LogSourceFilterSettings(filter));
                }
            }
        }
    }
}