using System;
using System.Globalization;
using System.Text;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.Diagnostics;
using DS.Sirius.Core.Security;
using DS.Sirius.Core.SqlServer;
using DS.Sirius.Core.WindowsEventLog;

namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This class implements an audit logger using an SQL Server table
    /// </summary>
    public class SqlTableAuditLogger : AuditLoggerBase
    {
        protected readonly string ConnectionInfo;

        /// <summary>
        /// Creates a new instance of this class using the specified SQL connection.
        /// </summary>
        /// <param name="nameOrConnectionString">SQL connection description</param>
        public SqlTableAuditLogger(string nameOrConnectionString)
        {
            ConnectionInfo = nameOrConnectionString;
        }

        /// <summary>
        /// Override this method to carry out writing the entry to the log.
        /// </summary>
        /// <param name="entry">Log entry to be written to the log.</param>
        protected override void OnLogging(AuditLogItem entry)
        {
            using (var db = new Database(ConnectionInfo))
            {
                db.BeginTransaction();
                try
                {
                    if (!string.IsNullOrEmpty(entry.CorrelationId))
                    {
                        // --- Handle correlation
                        var row = db.FirstOrDefault<AuditLogRecord>(
                            "where [Id]=@0", long.Parse(entry.CorrelationId));
                        if (row != null)
                        {
                            // --- Correlated row found, modify and save it
                            row.Timestamp = entry.Timestamp;
                            row.IsSuccessful = entry.IsSuccessfull;
                            row.ErrorInfo = entry.ErrorInfo;
                            row.CorrelationId = row.Id;
                            row.ExecutionTime = entry.ExecutionTime.HasValue ? entry.ExecutionTime.Value : -1.0;
                            SignAuditRecord(row);
                            db.Update(row);
                            OnAuditRowSaved(db, row);
                            db.CompleteTransaction();
                            return;
                        }
                    }
                    var newRow = new AuditLogRecord
                        {
                            TenantId = entry.TenantId,
                            Timestamp = AppConfigurationManager.GetCurrentDateTimeUtc(),
                            OperationInstanceId = entry.OperationInstanceId,
                            OperationId = entry.OperationId,
                            Parameters = AuditLogManager.GetAuditLogParametersString(entry.Parameters),
                            ClientInfo = entry.ClientInfo,
                            ServerName = entry.ServerName,
                            InstanceName = entry.InstanceName,
                            UserInfo = entry.UserInfo,
                            IsSuccessful = entry.IsSuccessfull,
                            ErrorInfo = entry.ErrorInfo,
                            ExecutionTime = entry.ExecutionTime.HasValue ? entry.ExecutionTime.Value : -1.0
                        };
                    SignAuditRecord(newRow);
                    db.Insert(newRow);
                    OnAuditRowSaved(db, newRow);
                    entry.CorrelationId = newRow.Id.ToString(CultureInfo.InvariantCulture);
                    db.CompleteTransaction();
                }
                catch (Exception ex)
                {
                    DiagnosticsManager.Trace(ex.ToString());
                    db.AbortTransaction();
                    throw;
                }
            }
        }

        /// <summary>
        /// This method can be used to log the item point of an operation
        /// </summary>
        /// <param name="item">Audit log item to record</param>
        /// <returns>Correlation identifier</returns>
        /// <remarks>
        /// The correlation identifier makes it possible to update the audit
        /// event log when an operation has been completed (either with success
        /// or failure).
        /// </remarks>
        public override string LogStartEvent(AuditLogItem item)
        {
            Log(item);
            return item.CorrelationId;
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
        protected override void OnLogException(AuditLogItem entry, Exception exception)
        {
            WindowsEventLogger.Log<SqlTableAuditLoggerError>("Logging operation failed: {0}", exception);
        }

        /// <summary>
        /// This method signs the audit log record.
        /// </summary>
        /// <param name="record">Audit log record instance</param>
        protected virtual void SignAuditRecord(AuditLogRecord record)
        {
        }

        /// <summary>
        /// This method can postprocess the audit record written to the database
        /// </summary>
        /// <param name="db">Database instance</param>
        /// <param name="newRow">Audit record</param>
        protected virtual void OnAuditRowSaved(Database db, AuditLogRecord newRow)
        {
        }

        /// <summary>
        /// This record represents the data table holding diagnostics log
        /// </summary>
        [TableName("AuditLog")]
        [PrimaryKey("Id")]
        [ExplicitColumns]
        public class AuditLogRecord : Record<AuditLogRecord>, ISignableDocument
        {
            private long _id;
            private string _tenantId;
            private DateTime _timestamp;
            private Guid _operationInstanceId;
            private long _correlationId;
            private string _operationId;
            private string _parameters;
            private string _userInfo;
            private bool _isSuccessful;
            private string _clientInfo;
            private string _serverName;
            private string _instanceName;
            private string _errorInfo;
            private double? _executionTime;
            private string _signature;

            [Column] public long Id 
            { 
                get { return _id; }
                set { _id = value; MarkColumnModified("Id"); }
            }

            [Column] public string TenantId
            {
                get { return _tenantId; }
                set { _tenantId = value; MarkColumnModified("TenantId"); }
            }

            [Column] public DateTime Timestamp
            {
                get { return _timestamp; }
                set { _timestamp = value; MarkColumnModified("Timestamp"); }
            }

            [Column] public Guid OperationInstanceId
            {
                get { return _operationInstanceId; }
                set { _operationInstanceId = value; MarkColumnModified("OperationInstanceId"); }
            }

            [Column] public long CorrelationId
            {
                get { return _correlationId; }
                set { _correlationId = value; MarkColumnModified("CorrelationId"); }
            }

            [Column] public string OperationId
            {
                get { return _operationId; }
                set { _operationId = value; MarkColumnModified("OperationId"); }
            }

            [Column] public string Parameters
            {
                get { return _parameters; }
                set { _parameters = value; MarkColumnModified("Parameters"); }
            }

            [Column] public string UserInfo
            {
                get { return _userInfo; }
                set { _userInfo = value; MarkColumnModified("UserInfo"); }
            }

            [Column] public bool IsSuccessful
            {
                get { return _isSuccessful; }
                set { _isSuccessful = value; MarkColumnModified("IsSuccessful"); }
            }

            [Column] public string ClientInfo
            {
                get { return _clientInfo; }
                set { _clientInfo = value; MarkColumnModified("ClientInfo"); }
            }

            [Column] public string ServerName
            {
                get { return _serverName; }
                set { _serverName = value; MarkColumnModified("ServerName"); }
            }

            [Column] public string InstanceName
            {
                get { return _instanceName; }
                set { _instanceName = value; MarkColumnModified("InstanceName"); }
            }

            [Column] public string ErrorInfo
            {
                get { return _errorInfo; }
                set { _errorInfo = value; MarkColumnModified("ErrorInfo"); }
            }

            [Column] public double? ExecutionTime
            {
                get { return _executionTime; }
                set { _executionTime = value; MarkColumnModified("ExecutionTime"); }
            }

            [Column] public string Signature
            {
                get { return _signature; }
                set { _signature = value; MarkColumnModified("Signature"); }
            }

            /// <summary>
            /// Extracts the document from the object to sign.
            /// </summary>
            /// <returns>String representation of the document</returns>
            public string GetDocument()
            {
                var sb = new StringBuilder();
                if (TenantId != null) sb.Append(TenantId);
                sb.Append(OperationInstanceId.ToString("D"));
                if (Parameters != null) sb.Append(Parameters);
                if (OperationId != null) sb.Append(OperationId);
                if (UserInfo != null) sb.Append(UserInfo);
                sb.Append(IsSuccessful.ToString());
                if (ClientInfo != null) sb.Append(ClientInfo);
                if (ServerName != null) sb.Append(ServerName);
                if (InstanceName != null) sb.Append(InstanceName);
                if (ErrorInfo != null) sb.Append(ErrorInfo);
                sb.Append(ExecutionTime);
                return sb.ToString();
            }

            /// <summary>
            /// Gets the signature string of a signed document.
            /// </summary>
            /// <returns>The string representing the signature of the document</returns>
            public string GetSignatureString()
            {
                return Signature;
            }
        }
    }
}