namespace DS.Sirius.Core.Logging
{
    /// <summary>
    /// This class represents a default log entry formatter.
    /// </summary>
    public class DefaultLogFormatter<TLogData> : ILogFormatter<TLogData>
        where TLogData : ILoggable
    {
        ///<summary>
        ///Formats the specified <paramref name="entry"/> into a string.
        ///</summary>
        ///<param name="entry">Log entry to be formatted</param>
        ///<returns>The string representation of the log entry</returns>
        public string Format(TLogData entry)
        {
            return entry.ToString();
        }
    }
}