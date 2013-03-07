namespace DS.Sirius.Core.Logging
{
    /// <summary>
    /// This class defines the responsibilities of a generic log formatter that
    /// converts an <see cref="ILoggable"/> to a formatted string.
    /// </summary>
    /// <typeparam name="T">Type of the entry to format</typeparam>
    public interface ILogFormatter<in T>
        where T : ILoggable
    {
        /// <summary>
        /// Formats the specified <paramref name="entry"/> into a string.
        /// </summary>
        /// <param name="entry">Log entry to be formatted</param>
        /// <returns>The string representation of the log entry</returns>
        string Format(T entry);
    }
}