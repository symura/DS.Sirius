using System;
using DS.Sirius.Core.SqlServer;
using DS.Sirius.Core.WindowsEventLog;

namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This class implements a diganostics logger using an SQL Server table
    /// </summary>
    public class SqlTableDiagnosticsLogger : DiagnosticsLoggerBase
    {
        private readonly string _connectionInfo;

        /// <summary>
        /// Creates a new instance of this class using the specified SQL connection.
        /// </summary>
        /// <param name="nameOrConnectionString">SQL connection description</param>
        public SqlTableDiagnosticsLogger(string nameOrConnectionString)
        {
            _connectionInfo = nameOrConnectionString;
        }

        /// <summary>
        /// Override this method to carry out writing the entry to the log.
        /// </summary>
        /// <param name="entry">Log entry to be written to the log.</param>
        protected override void OnLogging(DiagnosticsLogItem entry)
        {
            using (var db = new Database(_connectionInfo))
            {
                var logRecord = new DiagnosticsLogRecord
                    {
                        OperationInstanceId = entry.OperationInstanceId,
                        DetailedMessage = entry.DetailedMessage,
                        InstanceName = entry.InstanceName,
                        Message = entry.Message,
                        ServerName = entry.ServerName,
                        Source = entry.Source,
                        TenantId = entry.TenantId,
                        ThreadId = entry.ThreadId,
                        Timestamp = entry.Timestamp,
                        Type = (int) entry.Type
                    };
                db.Insert(logRecord);
            }
        }

        /// <summary>
        /// Override this method to handle the exception raised during the log
        /// operation.
        /// </summary>
        /// <param name="entry">Entry to log</param>
        /// <param name="exception">Exception raised during logging</param>
        /// <remarks>
        /// This operation hides the exception be default.
        /// </remarks>
        protected override void OnLogException(DiagnosticsLogItem entry, Exception exception)
        {
            WindowsEventLogger.Log<SqlTableDiagnosticsLoggerError>("Logging operation failed: {0}", exception);
        }

        /// <summary>
        /// This record represents the data table holding diagnostics log
        /// </summary>
        [TableName("DiagnosticsLog")]
        [PrimaryKey("Id")]
        [ExplicitColumns]
        public class DiagnosticsLogRecord: Record<DiagnosticsLogRecord>
        {
            [Column] public long Id { get; set; }
            [Column] public string TenantId { get; set; }
            [Column] public Guid OperationInstanceId { get; set; }
            [Column] public DateTime Timestamp { get; set; }
            [Column] public string Source { get; set; }
            [Column] public string Message { get; set; }
            [Column] public string DetailedMessage { get; set; }
            [Column] public int Type { get; set; }
            [Column] public string ServerName { get; set; }
            [Column] public string InstanceName { get; set; }
            [Column] public int ThreadId { get; set; }
        }
    }
}