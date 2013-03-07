using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.Security;
using DS.Sirius.Core.SqlServer;

namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This class implements an audit logger tha signs audit log entries and uses an SQL Server table
    /// </summary>
    public class SqlTableSignedAuditLogger : SqlTableAuditLogger
    {
        private X509Certificate2 _certificate;

        /// <summary>
        /// Gets the resource key used as the certificate key
        /// </summary>
        public string CertificateKey { get; set; }

        /// <summary>
        /// Creates a new instance of this class using the specified SQL connection.
        /// </summary>
        /// <param name="nameOrConnectionString">SQL connection description</param>
        public SqlTableSignedAuditLogger(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        /// <summary>
        /// This method signs the audit log record.
        /// </summary>
        /// <param name="record">Audit log record instance</param>
        protected override void SignAuditRecord(AuditLogRecord record)
        {
            if (_certificate == null)
            {
                _certificate = AppConfigurationManager.CreateResourceConnection<X509Certificate2>(CertificateKey);
            }
            record.Signature = record.SignDocument(_certificate);
        }

        /// <summary>
        /// This method can postprocess the audit record written to the database
        /// </summary>
        /// <param name="db">Database instance</param>
        /// <param name="newRow">Audit record</param>
        protected override void OnAuditRowSaved(Database db, AuditLogRecord newRow)
        {
            // --- Segment ID is the leftmost 22 bits, HashBase is the rightmost 10 bits
            int segmentId;
            int hashId;
            GetIntegrityRowIds(newRow.Id, out segmentId, out hashId);

            // --- Get the specified row
            var integrityRow = db.FirstOrDefault<AuditLogIntegrityRecord>("where [SegmentId]=@0 and [HashId]=@1",
                                                                          segmentId, hashId);
            if (integrityRow == null)
            {
                // --- Insert a new integrity row
                integrityRow = new AuditLogIntegrityRecord
                    {
                        SegmentId = segmentId,
                        HashId = hashId,
                        IdAggregate = newRow.Id,
                        IdCount = 1
                    };
                integrityRow.Signature = integrityRow.SignDocument(_certificate);
                db.Insert(integrityRow);
            }
            else
            {
                integrityRow.IdAggregate += newRow.Id;
                integrityRow.IdCount++;
                integrityRow.Signature = integrityRow.SignDocument(_certificate);
                db.Update(integrityRow);
            }
        }

        /// <summary>
        /// Gets the segment ID and hash ID of an integroty record by its audit log record id
        /// </summary>
        /// <param name="id">Audit log record ID</param>
        /// <param name="segmentId">Segment ID</param>
        /// <param name="hashId">Hash ID</param>
        public void GetIntegrityRowIds(long id, out int segmentId, out int hashId)
        {
            segmentId = (int)(id >> 20);
            var hashBase = (int)(id & 0x3ff);
            hashId = ((hashBase & 0x03f) << 4) | ((hashBase & 0x3c0) >> 6);
        }

        /// <summary>
        /// Gets the boundaries of an audit log segment.
        /// </summary>
        /// <param name="segmentId">Id of the segment</param>
        /// <param name="lowerBound">Lower bound of the segment</param>
        /// <param name="upperBound">Upper bound of the segment</param>
        /// <remarks>Audit log segments contain 1024*1024 record</remarks>
        public void GetSegmentBoundaries(long segmentId, out long lowerBound, out long upperBound)
        {
            lowerBound = segmentId * 1024 * 1024;
            upperBound = lowerBound + 1024 * 1024 - 1;
        }

        /// <summary>
        /// Validates the specified audit log segment
        /// </summary>
        /// <param name="segmentId">Id of the audit log segment</param>
        /// <returns>Segment validation info</returns>
        public SegmentValidationInfo ValidateLogSegment(int segmentId)
        {
            long lowerId;
            long upperId;
            GetSegmentBoundaries(segmentId, out lowerId, out upperId);
            var result = new SegmentValidationInfo(segmentId, lowerId, upperId);
            var aggregates = new long[1024];
            var counts = new int[1024];

            using (var db = new Database(ConnectionInfo))
            {
                // --- Go through the segment page-by-page
                for (int i = 0; i < 1024; i++)
                {
                    var auditRecords = db.Fetch<AuditLogRecord>("where [Id] between @0 and @1",
                                                                lowerId + 1024*i, lowerId + 1024*i + 1023);
                    result.RecordCount += auditRecords.Count;
                    foreach (var record in auditRecords)
                    {
                        // --- Check each record
                        if (!record.VerifyDocument(_certificate))
                        {
                            result.SignRecordTampered(record.Id);
                        }

                        // --- Calculate aggregate
                        int segment, hashId;
                        GetIntegrityRowIds(record.Id, out segment, out hashId);
                        aggregates[hashId] += record.CorrelationId == 0 ? record.Id : record.Id*2;
                        counts[hashId] += record.CorrelationId == 0 ? 1 : 2;
                    }
                }

                // --- Check integrity values
                for (int i = 0; i < 1024; i++)
                {
                    var integrityRecord = db.FirstOrDefault<AuditLogIntegrityRecord>(
                        "where [SegmentId] = @0 and [HashId] = @1", segmentId, i);
                    if (integrityRecord == null)
                    {
                        // --- No record found, so aggregate must be null
                        if (aggregates[i] != 0)
                        {
                            result.SignAggregateTampered(i);
                        }
                    }
                    else
                    {
                        // --- Check the integrity record
                        if (!integrityRecord.VerifyDocument(_certificate))
                        {
                            result.SignAggregateTampered(i);
                        }
                        else if (aggregates[i] != integrityRecord.IdAggregate || counts[i] != integrityRecord.IdCount)
                        {
                            result.SignAggregateTampered(i);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This record represents the data table holding diagnostics log
        /// </summary>
        [TableName("AuditLogIntegrity")]
        [PrimaryKey("SegmentId, HashId")]
        [ExplicitColumns]
        public class AuditLogIntegrityRecord : Record<AuditLogIntegrityRecord>, ISignableDocument
        {
            private int _segmentId;
            private int _hashId;
            private long _idAggregate;
            private int _idCount;
            private string _signature;
            private byte[] _rowVersion;

            [Column]
            public int SegmentId
            {
                get { return _segmentId; }
                set { _segmentId = value; MarkColumnModified("SegmentId"); }
            }

            [Column]
            public int HashId
            {
                get { return _hashId; }
                set { _hashId = value; MarkColumnModified("HashId"); }
            }

            [Column]
            public long IdAggregate
            {
                get { return _idAggregate; }
                set { _idAggregate = value; MarkColumnModified("IdAggregate"); }
            }

            [Column]
            public int IdCount
            {
                get { return _idCount; }
                set { _idCount = value; MarkColumnModified("IdCount"); }
            }

            [Column]
            public string Signature
            {
                get { return _signature; }
                set { _signature = value; MarkColumnModified("Signature"); }
            }

            [VersionColumn]
            public byte[] RowVersion
            {
                get { return _rowVersion; }
                set { _rowVersion = value; MarkColumnModified("RowVersion"); }
            }

            /// <summary>
            /// Extracts the document from the object to sign.
            /// </summary>
            /// <returns>String representation of the document</returns>
            public string GetDocument()
            {
                return IdAggregate.ToString(CultureInfo.InvariantCulture) + 
                    IdCount.ToString(CultureInfo.InvariantCulture);
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

        /// <summary>
        /// This class provides information about the log segment validation
        /// </summary>
        public class SegmentValidationInfo
        {
            private readonly List<long> _recordsTampered = new List<long>();
            private readonly List<int> _aggregatesTampered = new List<int>();

            /// <summary>
            /// Gets the segment ID
            /// </summary>
            public long SegmentId { get; private set; }

            /// <summary>
            /// Gets the Lower ID of the segment
            /// </summary>
            public long LowerId { get; private set; }

            /// <summary>
            /// Gets the upper ID of the segment
            /// </summary>
            public long UpperId { get; private set; }

            /// <summary>
            /// Gets the number of records found in the segment
            /// </summary>
            public int RecordCount { get; internal set; }

            /// <summary>
            /// Gets the list of tampered audit records
            /// </summary>
            public IReadOnlyCollection<long> RecordsTampered
            {
                get { return new ReadOnlyCollection<long>(_recordsTampered); }
            }

            public IReadOnlyCollection<int> AggregatesTampered
            {
                get { return new ReadOnlyCollection<int>(_aggregatesTampered);}
            }

            /// <summary>
            /// Creates a new instance of the class with the specified identifiers
            /// </summary>
            /// <param name="segmentId">Segment identifier</param>
            /// <param name="lowerId">Lower boundary ID</param>
            /// <param name="upperId">Upper boundary ID</param>
            public SegmentValidationInfo(int segmentId, long lowerId, long upperId)
            {
                SegmentId = segmentId;
                LowerId = lowerId;
                UpperId = upperId;
            }

            /// <summary>
            /// Signs that the specified record has been tampered.
            /// </summary>
            /// <param name="id">Record ID</param>
            public void SignRecordTampered(long id)
            {
                _recordsTampered.Add(id);
            }

            /// <summary>
            /// Signs that an audit log integrity record has been tampered.
            /// </summary>
            /// <param name="id">Aggregate ID</param>
            public void SignAggregateTampered(int id)
            {
                _aggregatesTampered.Add(id);
            }

            /// <summary>
            /// Gets the flag indicating if the specified audit log segment is valid or not
            /// </summary>
            public bool IsValid
            {
                get { return _aggregatesTampered.Count == 0 && _recordsTampered.Count == 0; }
            }
        }
    }
}