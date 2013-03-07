using System;

namespace DS.Sirius.Core.Logging
{
    /// <summary>
    /// This interface can be added to data structures that represent log data.
    /// </summary>
    public interface ILoggable
    {
        /// <summary>Gets or sets the time of the event</summary>
        DateTime Timestamp { get; set; }
    }
}