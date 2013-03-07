/* PetaPoco v4.0.3.12 - A Tiny ORMish thing for your POCO's.
 * Copyright 2011-2012 Topten Software.  All Rights Reserved.
 * 
 * Apache License 2.0 - http://www.toptensoftware.com/petapoco/license
 * 
 * Special thanks to Rob Conery (@robconery) for original inspiration (ie:Massive) and for 
 * use of Subsonic's T4 templates, Rob Sullivan (@DataChomp) for hard core DBA advice 
 * and Adam Schroder (@schotime) for lots of suggestions, improvements and Oracle support
 */

// Revised and refactored by Istvan Novak
// THE ORIGINAL SOURCE FILE HAVE BEEN CHANGED

using System;
using System.Data;

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// This class represents a database transaction
    /// </summary>
    public class Transaction : IDisposable
    {
        private Database _db;

        /// <summary>
        /// Creates a transaction instance working on the specified database.
        /// </summary>
        /// <param name="db">Database instance</param>
        public Transaction(Database db) : this(db, null) { }

        /// <summary>
        /// Creates a transaction instance working on the specified database.
        /// </summary>
        /// <param name="db">Database instance</param>
        /// <param name="isolationLevel">Transaction isolation level to use</param>
        public Transaction(Database db, IsolationLevel? isolationLevel)
        {
            _db = db;
            _db.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        public virtual void Complete()
        {
            _db.CompleteTransaction();
            _db = null;
        }

        /// <summary>
        /// Aborts the transaction if not committed yet.
        /// </summary>
        public void Dispose()
        {
            if (_db != null)
                _db.AbortTransaction();
        }
    }
}