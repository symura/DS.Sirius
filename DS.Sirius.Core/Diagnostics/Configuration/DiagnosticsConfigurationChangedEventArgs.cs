using System;

namespace DS.Sirius.Core.Diagnostics.Configuration
{
    /// <summary>
    /// Represents the event arguments for disgnostics configuration change.
    /// </summary>
    public class DiagnosticsConfigurationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the disgnostics configuration settings
        /// </summary>
        public DiagnosticsConfigurationSettings Settings { get; private set; }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="settings">New diagnostics configuration settings</param>
        public DiagnosticsConfigurationChangedEventArgs(DiagnosticsConfigurationSettings settings)
        {
            Settings = settings;
        }
    }
}