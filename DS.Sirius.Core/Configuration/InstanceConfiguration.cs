using System;
using System.Reflection;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// This static class describes the instance configuration of a the current service process.
    /// It's aim is to allow installing and running several instances on the same machine.
    /// </summary>
    public static class InstanceConfiguration
    {
        private static Version s_Version;

        /// <summary>
        /// Gets or sets the prefix that will be given to all
        /// PerformanceCounterCategory and WindowsEventLog Source
        /// </summary>
        public static string InstancePrefix { get; set; }

        /// <summary>
        /// Gets or sets the that name can be used
        /// for debugging and diagnostics information
        /// </summary>
        public static string InstanceName { get; set; }

        /// <summary>
        /// Returns the current version of the deployment
        /// </summary>
        public static Version InstanceVersion
        {
            get { return s_Version ?? (s_Version = Assembly.GetExecutingAssembly().GetName().Version); }
            set { s_Version = value; }
        }
    }
}