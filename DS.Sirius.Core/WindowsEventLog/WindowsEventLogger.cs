using System;
using System.Diagnostics;
using System.Text;
using DS.Sirius.Core.Common;
using DS.Sirius.Core.Configuration;

namespace DS.Sirius.Core.WindowsEventLog
{
    /// <summary>
    /// This class can be used to write events to the windows event log.
    /// </summary>
    /// <remarks>
    /// The generic parameter must be the derived class of <see cref="LogEventBase"/>
    /// </remarks>
    public static class WindowsEventLogger
    {
        private const int MAX_MESSAGE_LENGTH = 31800;

        /// <summary>
        /// Mapper for Event Log names
        /// </summary>
        public static INameMapper LogNameMapper { get; set; }

        /// <summary>
        /// Mapper for Event Source names
        /// </summary>
        public static INameMapper LogSourceMapper { get; set; }

        /// <summary>
        /// Sets up the static members of this class
        /// </summary>
        static WindowsEventLogger()
        {
            LogNameMapper = new IdentityNameMapper();
            LogSourceMapper = new InstanceNameMapper();
        }

        /// <summary>
        /// Logs the message into the EventLog defined by <typeparamref name="TEventDefinition"/>
        /// </summary>
        /// <typeparam name="TEventDefinition">
        /// Must be a derived class of <see cref="LogEventBase"/>
        /// </typeparam>
        /// <param name="message">The string message to log</param>
        /// <param name="messageParams">The parameters of the message</param>
        public static void Log<TEventDefinition>(string message = null, params object[] messageParams)
            where TEventDefinition : LogEventBase, new()
        {
            var eventClass = new TEventDefinition();
            var logger = new EventLog(LogNameMapper.Map(eventClass.LogName))
            {
                Source = LogSourceMapper.Map(eventClass.Source),
                MachineName = ".",
            };
            var msg = message != null ? String.Format(message, messageParams) : eventClass.Message;
            if (msg.Length > MAX_MESSAGE_LENGTH)
            {
                msg = msg.Substring(0, MAX_MESSAGE_LENGTH);
            }
            logger.WriteEntry(msg, eventClass.Type, eventClass.EventId, eventClass.CategoryId);
        }

        /// <summary>
        /// Logs the message into the EventLog defined by <typeparamref name="TEventDefinition"/>
        /// </summary>
        /// <typeparam name="TEventDefinition">
        /// Must be a derived class of <see cref="LogEventBase"/>
        /// </typeparam>
        /// <param name="ex">Exception to log</param>
        public static void Log<TEventDefinition>(Exception ex)
            where TEventDefinition : LogEventBase, new()
        {
            Log<TEventDefinition>("An unexcepted exception has been raised", ex);
        }

        /// <summary>
        /// Logs the message into the EventLog defined by <typeparamref name="TEventDefinition"/>
        /// </summary>
        /// <typeparam name="TEventDefinition">
        /// Must be a derived class of <see cref="LogEventBase"/>
        /// </typeparam>
        /// <param name="message">The string message to log</param>
        /// <param name="ex">Exception to log</param>
        public static void Log<TEventDefinition>(string message, Exception ex)
            where TEventDefinition : LogEventBase, new()
        {
            var eventClass = new TEventDefinition();
            var logger = new EventLog(LogNameMapper.Map(eventClass.LogName))
            {
                Source = LogSourceMapper.Map(eventClass.Source),
                MachineName = ".",
            };
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(message);
            messageBuilder.AppendFormat("{0}: {1}", ex.GetType(), ex.Message);
            messageBuilder.AppendLine();
            messageBuilder.AppendLine(ex.StackTrace);
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                messageBuilder.AppendFormat("{0}: ", ex);
                messageBuilder.AppendLine(ex.Message);
                messageBuilder.AppendLine(ex.StackTrace);
                innerEx = innerEx.InnerException;
            }
            var finalMessage = messageBuilder.ToString();
            if (finalMessage.Length > MAX_MESSAGE_LENGTH)
            {
                finalMessage = finalMessage.Substring(0, MAX_MESSAGE_LENGTH);
            }
            logger.WriteEntry(finalMessage, eventClass.Type, eventClass.EventId, eventClass.CategoryId);
        }

        /// <summary>
        /// Maps names to themselves
        /// </summary>
        private class IdentityNameMapper : INameMapper
        {
            /// <summary>
            /// Maps the specified name to itself.
            /// </summary>
            /// <param name="name">Source name</param>
            /// <returns>Mapped name</returns>
            public string Map(string name)
            {
                return name;
            }
        }

        /// <summary>
        /// Maps names to themselves with an instance prefix
        /// </summary>
        private class InstanceNameMapper : INameMapper
        {
            /// <summary>
            /// Prepends <see cref="InstanceConfiguration.InstancePrefix"/> to the specified name.
            /// </summary>
            /// <param name="name">Source name</param>
            /// <returns>Mapped name</returns>
            public string Map(string name)
            {
                return InstanceConfiguration.InstancePrefix + name;
            }
        }
    }
}