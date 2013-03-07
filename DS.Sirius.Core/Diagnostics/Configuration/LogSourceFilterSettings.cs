using System.Xml.Linq;
using DS.Sirius.Core.Configuration;

namespace DS.Sirius.Core.Diagnostics.Configuration
{
    /// <summary>
    /// Defines a log route filter based on the source of the filter-
    /// </summary>
    /// <remarks>
    /// The pattern is matched with the source of the log event. If the first character of the 
    /// pattern is an '!' mark, the filter logic is inverted. The pattern can be an "*" that 
    /// matches with any source, or an expression used for prefix matching.
    /// </remarks>
    public sealed class LogSourceFilterSettings : LogFilterSettingsBase
    {
        private const string PATTERN = "Pattern";

        /// <summary>
        /// Filter section root
        /// </summary>
        public const string ROOT = "LogSourceFilter";

        /// <summary>
        /// Initializes a new source filter definition with the specified pattern.
        /// </summary>
        /// <param name="pattern">Source filter pattern.</param>
        public LogSourceFilterSettings(string pattern)
        {
            Pattern = pattern.Trim();
        }

        /// <summary>
        /// Creates an instance from the specified XML element.
        /// </summary>
        /// <param name="element">XML element to create this instance from</param>
        public LogSourceFilterSettings(XElement element)
        {
            ReadFromXml(element);
        }

        /// <summary>
        /// Gets or sets the pattern used by the filter.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// Gets the configuration name for the filter section
        /// </summary>
        public override string SettingsRootName
        {
            get { return ROOT; }
        }

        /// <summary>
        /// Checks whether the specified log entry matches with the filter.
        /// </summary>
        /// <param name="entry">Entry to check</param>
        /// <returns>True, if the entry matches with the filter; otherwise, false.</returns>
        public override bool MatchesEntry(DiagnosticsLogItem entry)
        {
            if (string.IsNullOrWhiteSpace(Pattern)) return true;
            var invert = Pattern[0] == '!';
            var pattern = invert ? Pattern.Substring(1) : Pattern;
            var result = pattern == "*" || (entry.Source == pattern || entry.Source.StartsWith(pattern + "."));
            return invert ? !result : result;
        }

        /// <summary>
        /// Writes the object to an XElement instance using the specified root element name.
        /// </summary>
        /// <param name="rootElement">Root element name</param>
        /// <returns>XElement representation of the object</returns>
        public override XElement WriteToXml(XName rootElement)
        {
            return new XElement(rootElement, new XAttribute(PATTERN, Pattern));
        }

        /// <summary>
        /// Parse the specified configuration settings
        /// </summary>
        /// <param name="element">Element holding configuration settings</param>
        protected override void ParseFrom(XElement element)
        {
                Pattern = element.StringAttribute(PATTERN);
        }
    }
}