using DS.Sirius.Core.Configuration;

namespace DS.Sirius.Core.Diagnostics.Configuration
{
    /// <summary>
    /// This class defines an abstract log route filter.
    /// </summary>
    public abstract class LogFilterSettingsBase: ConfigurationSettingsBase
    {
        /// <summary>
        /// Gets the configuration name for the filter section
        /// </summary>
        public abstract string SettingsRootName { get; }

        /// <summary>
        /// Checks whether the specified log entry matches with the filter.
        /// </summary>
        /// <param name="entry">Entry to check</param>
        /// <returns>True, if the entry matches with the filter; otherwise, false.</returns>
        public abstract bool MatchesEntry(DiagnosticsLogItem entry);
    }
}