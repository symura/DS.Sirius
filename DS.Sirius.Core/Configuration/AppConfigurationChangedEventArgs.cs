using System;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// Represents the event arguments for application configuration change.
    /// </summary>
    public class AppConfigurationChangedEventArgs: EventArgs
    {
        /// <summary>
        /// Gets the application configuration settings
        /// </summary>
        public AppConfigurationSettings Settings { get; private set; }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="settings">New application configuration settings</param>
        public AppConfigurationChangedEventArgs(AppConfigurationSettings settings)
        {
            Settings = settings;
        }
    }
}