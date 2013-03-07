namespace DS.Sirius.Core.Logging
{
    /// <summary>
    /// This interface defines the responsibility of a generic logger.
    /// </summary>
    /// <typeparam name="T">Data structure to log</typeparam>
    public interface ILogger<in T>
        where T : ILoggable
    {
        /// <summary>
        /// Writes the specified <paramref name="entry"/> to the log.
        /// </summary>
        void Log(T entry);
    }
}