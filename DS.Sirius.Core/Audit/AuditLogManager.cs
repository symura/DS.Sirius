using System;
using System.Collections.Generic;
using System.Text;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.WindowsEventLog;

namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This static object is responsible for managing audit log entries in the framework.
    /// </summary>
    public static class AuditLogManager
    {
        /// <summary>
        /// This flag indicates if configuration error was logged (it is logged only once).
        /// </summary>
        private static bool s_ConfigurationErrorIsLogged;

        /// <summary>
        /// This method logs a table change event to the appropriate log according to the current 
        /// logging configuration.
        /// </summary>
        /// <param name="entry">LogData entry</param>
        public static string LogStartEvent(AuditLogItem entry)
        {
            var logger = AppConfigurationManager.AuditLogger;
            if (logger == null)
            {
                CheckIfLoggerConfigurationErrorIsNoted();
                return null;
            }
            try
            {
                return logger.LogStartEvent(entry);
            }
            catch (Exception ex)
            {
                WindowsEventLogger.Log<AuditLogFailed>("LogStartEvent failed: {0}\n{1}",
                                                       ex, GetAuditLogString(entry));
                return null;
            }
        }

        /// <summary>
        /// This method logs a table change event to the appropriate log according to the current 
        /// logging configuration.
        /// </summary>
        /// <param name="entry">LogData entry</param>
        public static void Log(AuditLogItem entry)
        {
            var logger = AppConfigurationManager.AuditLogger;
            if (logger == null)
            {
                CheckIfLoggerConfigurationErrorIsNoted();
                return;
            }
            try
            {
                logger.Log(entry);
            }
            catch (Exception ex)
            {
                WindowsEventLogger.Log<AuditLogFailed>("LogStartEvent failed: {0}\n{1}",
                    ex, GetAuditLogString(entry));
            }
        }

        /// <summary>
        /// Checks whether the audit logger is configured.
        /// </summary>
        private static void CheckIfLoggerConfigurationErrorIsNoted()
        {
            if (!s_ConfigurationErrorIsLogged)
            {
                WindowsEventLogger.Log<AuditLogManagerWasNotConfigured>();
            }
            s_ConfigurationErrorIsLogged = true;
        }

        /// <summary>
        /// Creates a string from the audit log item.
        /// </summary>
        /// <param name="entry">Audit log item</param>
        /// <returns>Audit log item string representation</returns>
        public static string GetAuditLogString(AuditLogItem entry)
        {
            var message = new StringBuilder();
            message.Append("Audit message: \n");
            message.AppendFormat("  OperationInstanceId: {0}", entry.OperationInstanceId);
            message.AppendFormat("  OperationId: {0}", entry.OperationId);
            message.AppendFormat("  Timestamp: {0}", entry.Timestamp);
            message.AppendFormat("  UserInfo: {0}", entry.UserInfo ?? "<no user info>");
            message.AppendFormat("  CorrelationId: {0}", entry.CorrelationId ?? "<null>");
            message.AppendFormat("  IsSuccessful: {0}", entry.IsSuccessfull);
            message.AppendFormat("  ErrorInfo: {0}", entry.ErrorInfo ?? "<no error info>");
            message.AppendFormat("  CustomerId: {0}", entry.TenantId);
            message.AppendFormat("  ClientInfo: {0}", entry.ClientInfo ?? "<no client info>");
            message.AppendFormat("  Changes (Parameter and value)\n");
            message.Append(GetAuditLogParametersString(entry.Parameters));
            return message.ToString();
        }

        /// <summary>
        /// Creates a string from the audit log item parameters.
        /// </summary>
        /// <param name="parameters">Audit log item parameters</param>
        /// <returns>Audit log item parameters string representation</returns>
        public static string GetAuditLogParametersString(IEnumerable<AuditLogParameter> parameters)
        {
            var message = new StringBuilder();
            foreach (var item in parameters)
            {
                message.AppendFormat("    {0}: {1}\n", item.Name ?? "<no name>", item.Value ?? "<null>");
            }
            return message.ToString();
        }
    }
}