using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.Diagnostics.Configuration;

namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This static object is responsible for managing log entries in the framework.
    /// </summary>
    public static class DiagnosticsManager
    {
        private static DiagnosticsConfigurationSettings s_Settings;

        /// <summary>
        /// Initializes the static members of this class
        /// </summary>
        static DiagnosticsManager()
        {
            Reset();
        }

        /// <summary>
        /// Resets the configuration of DiagnosticsManager
        /// </summary>
        public static void Reset()
        {
            try
            {
                var config = ConfigurationManager.GetSection(DiagnosticsConfigurationSettings.ROOT) as XElement;
                Settings = new DiagnosticsConfigurationSettings(config);
            }
            catch (Exception)
            {
                Settings = new DiagnosticsConfigurationSettings(false, new List<LogRouteSettings>());
            }
        }

        /// <summary>
        /// This event is raised when the application connfiguration is changed.
        /// </summary>
        public static event EventHandler<DiagnosticsConfigurationSettings> OnConfigurationChanged;

        /// <summary>
        /// This flag indicates if configuration error was logged (it is logged only once).
        /// </summary>
        public static DiagnosticsConfigurationSettings Settings
        {
            get { return s_Settings; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (s_Settings == value) return;
                s_Settings = value;
                RaiseOnConfigurationChanged();
            }
        }

        /// <summary>
        /// This method logs an event to the appropriate log according to the current 
        /// logging configuration.
        /// </summary>
        /// <param name="entry">LogData entry</param>
        public static void Log(DiagnosticsLogItem entry)
        {
            if (!s_Settings.Enabled) return;
            foreach (var route in s_Settings.Routes
                .Where(route => route.Enabled).Where(route => route.MatchesFilters(entry) 
                    && route.DiagnosticsLogger.Instance != null))
            {
                route.DiagnosticsLogger.Instance.Log(entry);
            }
        }

        /// <summary>
        /// Disposes the resources held by the manager.
        /// </summary>
        public static void Dispose()
        {
            if (s_Settings.Routes == null) return;
            foreach (var route in s_Settings.Routes)
            {
                if (route.DiagnosticsLogger == null || route.DiagnosticsLogger.Instance == null) continue;
                var disposable = route.DiagnosticsLogger.Instance as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
            s_Settings = null;
        }

        /// <summary>
        /// Traces the specified message
        /// </summary>
        /// <param name="message">Message to trace</param>
        public static void Trace(string message)
        {
            var logItem = new DiagnosticsLogItem
                {
                    DetailedMessage = message,
                    InstanceName = "",
                    Message = "Trace Info",
                    ServerName = AppConfigurationManager.GetMachineName(),
                    Source = "DiagnosticsManager",
                    TenantId = "<none>",
                    Timestamp = AppConfigurationManager.GetCurrentDateTimeUtc(),
                    ThreadId = 0,
                    Type = DiagnosticsLogItemType.Trace
                };
            Log(logItem);
        }

        #region Helper methods

        private static void RaiseOnConfigurationChanged()
        {
            if (OnConfigurationChanged != null)
            {
                OnConfigurationChanged(null, s_Settings);
            }
        }

        #endregion
    }
}
