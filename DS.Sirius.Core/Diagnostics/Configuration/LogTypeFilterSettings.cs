using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DS.Sirius.Core.Configuration;

namespace DS.Sirius.Core.Diagnostics.Configuration
{
    /// <summary>
    /// This filter checks whether the log entry matches with the filter's type.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="DiagnosticsLogItemType.Undefined"/> value to allow any kind of
    /// log entry items.
    /// </remarks>
    public sealed class LogTypeFilterSettings : LogFilterSettingsBase
    {
        private const string ITEM = "Item";
        private const string TYPE = "type";

        /// <summary>
        /// Filter section name
        /// </summary>
        public const string ROOT = "LogTypeFilter";

        /// <summary>
        /// Gets the configuration name for the filter section
        /// </summary>
        public override string SettingsRootName
        {
            get { return ROOT; }
        }

        /// <summary>
        /// Creates a filter instance with the specified log item type.
        /// </summary>
        /// <param name="itemTypes">Log item type to filter on</param>
        public LogTypeFilterSettings(params DiagnosticsLogItemType[] itemTypes)
        {
            ItemTypes = new List<DiagnosticsLogItemType>(itemTypes);
        }

        /// <summary>
        /// Creates an instance from the specified XML element
        /// </summary>
        /// <param name="element">XML element to create this instance from</param>
        public LogTypeFilterSettings(XElement element)
        {
            ItemTypes = new List<DiagnosticsLogItemType>();
            ReadFromXml(element);
        }

        /// <summary>
        /// Gets the type of the filter.
        /// </summary>
        public List<DiagnosticsLogItemType> ItemTypes { get; private set; }

        /// <summary>
        /// Checks whether the specified log entry matches with the filter.
        /// </summary>
        /// <param name="entry">Entry to check</param>
        /// <returns>True, if the entry matches with the filter; otherwise, false.</returns>
        public override bool MatchesEntry(DiagnosticsLogItem entry)
        {
            return ItemTypes.Any(item => item == DiagnosticsLogItemType.Undefined || item == entry.Type);
        }

        /// <summary>
        /// Writes the object to an XElement instance using the specified root element name.
        /// </summary>
        /// <param name="rootElement">Root element name</param>
        /// <returns>XElement representation of the object</returns>
        public override XElement WriteToXml(XName rootElement)
        {
            return new XElement(rootElement,
                    from item in ItemTypes
                    select new XElement(ITEM, new XAttribute(TYPE, item)));
        }

        /// <summary>
        /// Parse the specified configuration settings
        /// </summary>
        /// <param name="element">Element holding configuration settings</param>
        protected override void ParseFrom(XElement element)
        {
            ItemTypes.Clear();
            foreach (var item in element.Descendants())
            {
                ItemTypes.Add(item.EnumAttribute<DiagnosticsLogItemType>(TYPE));
            }
        }
    }
}