using System;

namespace DS.Sirius.Core.WindowsEventLog
{
    /// <summary>
    /// Use this attribute to specify a name for a <see cref="LogEventBase"/>
    /// </summary>
    /// <remarks>
    /// This attribute is required for the usage of <see cref="LogEventBase"/>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class EventLogNameAttribute : EventAttributeBase
    {
        /// <summary>
        /// Gets the the log name value.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Creates a new instance with the specified log name.
        /// </summary>
        /// <param name="logName">Log name value</param>
        public EventLogNameAttribute(string logName)
        {
            Value = logName;
        }

        /// <summary>
        /// Use this attribute to specify a name with a derived class of <see cref="EventLogNameBase"/> 
        /// for a <see cref="LogEventBase"/>
        /// </summary>
        /// <param name="logNameClass">
        /// The type of a derived class of <see cref="EventLogNameBase"/>.
        /// </param>
        public EventLogNameAttribute(Type logNameClass)
        {
            if (!logNameClass.IsSubclassOf(typeof(EventLogNameBase)))
            {
                throw new InvalidOperationException(
                    "The given type must derive from EventLogNameBase");
            }

            //Create instance, for the contructor of the base class must run
            var constructed = Activator.CreateInstance(logNameClass);
            Value = ((EventLogNameBase)constructed).LogName;
        }
    }
}