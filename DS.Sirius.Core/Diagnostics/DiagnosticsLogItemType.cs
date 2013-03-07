namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This enumeration defines the types of diagnostics log items
    /// </summary>
    public enum DiagnosticsLogItemType
    {
        /// <summary>Empty item type</summary>
        Undefined = 0,

        /// <summary>Trace message</summary>
        Trace = 1,

        /// <summary>Informational message</summary>
        Informational = 2,

        /// <summary>Success message</summary>
        Success = 3,

        /// <summary>Warning message</summary>
        Warning = 4,

        /// <summary>Error message</summary>
        Error = 5,

        /// <summary>Fatal error message</summary>
        Fatal = 6
    }
}