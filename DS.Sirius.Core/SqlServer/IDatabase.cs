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

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// This interface defines the set of data manipulation operations in a database
    /// </summary>
    public interface IDatabase : IDatabaseQuery
    {
        /// <summary>
        /// Disposes any resources held by modifications
        /// </summary>
        void Dispose();

        /// <summary>
        /// Gets the connection object belongign to the database
        /// </summary>
        SqlConnection Connection { get; }
        
        /// <summary>
        /// Gets the transaction object representing the current transaction
        /// </summary>
        SqlTransaction Transaction { get; }
        
        /// <summary>
        /// Creates a new data parameter object
        /// </summary>
        /// <returns>The newly created data parameter</returns>
        SqlParameter CreateParameter();
        
        /// <summary>
        /// Gets a new Petapoco transaction object.
        /// </summary>
        /// <returns>The newly created transaction object</returns>
        Transaction GetTransaction();

        /// <summary>
        /// Gets a new Petapoco transaction object with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level</param>
        /// <returns>The newly created transaction object</returns>
        Transaction GetTransaction(IsolationLevel? isolationLevel);

        /// <summary>
        /// Starts a new transaction
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Starts a new transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">Isolation level to use.</param>
        void BeginTransaction(IsolationLevel? isolationLevel);

        /// <summary>
        /// Aborts the current transaction.
        /// </summary>
        void AbortTransaction();

        /// <summary>
        /// Completes the current transaction.
        /// </summary>
        void CompleteTransaction();

        /// <summary>
        /// Inserts a poco into the database.
        /// </summary>
        /// <param name="tableName">Name of the table to insert the data</param>
        /// <param name="primaryKeyName">
        /// Name of primary key (compound key names separated by comma)
        /// </param>
        /// <param name="autoIncrement">Flag indicating if auto increment is used</param>
        /// <param name="poco">Poco instance to insert</param>
        /// <returns>
        /// The id of the newly inserted object.
        /// </returns>
        object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco);

        /// <summary>
        /// Inserts a poco into the database.
        /// </summary>
        /// <param name="tableName">Name of the table to insert the data</param>
        /// <param name="primaryKeyName">
        /// Name of primary key (compound key names separated by comma)
        /// </param>
        /// <param name="poco">Poco instance to insert</param>
        /// <returns>
        /// The id of the newly inserted object.
        /// </returns>
        object Insert(string tableName, string primaryKeyName, object poco);

        /// <summary>
        /// Inserts a poco into the database.
        /// </summary>
        /// <param name="poco">Poco instance to insert</param>
        /// <returns>
        /// The id of the newly inserted object.
        /// </returns>
        object Insert(object poco);

        /// <summary>
        /// Updates the specified poco in the database.
        /// </summary>
        /// <param name="tableName">Name of the table to update the data in</param>
        /// <param name="primaryKeyName">
        /// Name of primary key (compound key names separated by comma)
        /// </param>
        /// <param name="poco">Poco instance to update</param>
        /// <param name="primaryKeyValue">Object representing the primary key value</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue);

        /// <summary>
        /// Updates the specified poco in the database.
        /// </summary>
        /// <param name="tableName">Name of the table to update the data in</param>
        /// <param name="primaryKeyName">
        /// Name of primary key (compound key names separated by comma)
        /// </param>
        /// <param name="poco">Poco instance to update</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update(string tableName, string primaryKeyName, object poco);

        /// <summary>
        /// Updates the specified poco in the database.
        /// </summary>
        /// <param name="tableName">Name of the table to update the data in</param>
        /// <param name="primaryKeyName">
        /// Name of primary key (compound key names separated by comma)
        /// </param>
        /// <param name="poco">Poco instance to update</param>
        /// <param name="primaryKeyValue">Object representing the primary key value</param>
        /// <param name="columns">Names of columns to update</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, 
            IEnumerable<string> columns);

        /// <summary>
        /// Updates the specified poco in the database.
        /// </summary>
        /// <param name="tableName">Name of the table to update the data in</param>
        /// <param name="primaryKeyName">
        /// Name of primary key (compound key names separated by comma)
        /// </param>
        /// <param name="poco">Poco instance to update</param>
        /// <param name="columns">Names of columns to update</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string> columns);

        /// <summary>
        /// Updates the specified poco in the database.
        /// </summary>
        /// <param name="poco">Poco instance to update</param>
        /// <param name="columns">Names of columns to update</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update(object poco, IEnumerable<string> columns);

        /// <summary>
        /// Updates the specified poco in the database.
        /// </summary>
        /// <param name="poco">Poco instance to update</param>
        /// <param name="primaryKeyValue">Object representing the primary key value</param>
        /// <param name="columns">Names of columns to update</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update(object poco, object primaryKeyValue, IEnumerable<string> columns);

        /// <summary>
        /// Updates the specified poco in the database.
        /// </summary>
        /// <param name="poco">Poco instance to update</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update(object poco);

        /// <summary>
        /// Updates the specified poco in the database.
        /// </summary>
        /// <param name="poco">Poco instance to update</param>
        /// <param name="primaryKeyValue">Object representing the primary key value</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update(object poco, object primaryKeyValue);

        /// <summary>
        /// Updates the poco with the specified type in the database, using a SQL statement.
        /// </summary>
        /// <typeparam name="T">Type of poco</typeparam>
        /// <param name="sql">SQL statement</param>
        /// <param name="args">Statement parameters</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update<T>(string sql, params object[] args);

        /// <summary>
        /// Updates the poco with the specified type in the database, using a SQL statement.
        /// </summary>
        /// <typeparam name="T">Type of poco</typeparam>
        /// <param name="sql">SQL statement</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Update<T>(Sql sql);

        /// <summary>
        /// Deletes the specified poco from the database
        /// </summary>
        /// <param name="tableName">Name of the table to delete the data from</param>
        /// <param name="primaryKeyName">
        /// Name of primary key (compound key names separated by comma)
        /// </param>
        /// <param name="poco">Poco instance to delete</param>
        /// <returns>
        /// Number of rows affected by the delete operation.
        /// </returns>
        int Delete(string tableName, string primaryKeyName, object poco);

        /// <summary>
        /// Deletes the specified poco from the database
        /// </summary>
        /// <param name="tableName">Name of the table to delete the data from</param>
        /// <param name="primaryKeyName">
        /// Name of primary key (compound key names separated by comma)
        /// </param>
        /// <param name="poco">Poco instance to delete</param>
        /// <param name="primaryKeyValue">Object representing the primary key value</param>
        /// <returns>
        /// Number of rows affected by the delete operation.
        /// </returns>
        int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue);

        /// <summary>
        /// Deletes the specified poco from the database
        /// </summary>
        /// <param name="poco">Poco instance to delete</param>
        /// <returns>
        /// Number of rows affected by the delete operation.
        /// </returns>
        int Delete(object poco);

        /// <summary>
        /// Deletes the poco with the specified type from the database, using a SQL statement.
        /// </summary>
        /// <typeparam name="T">Type of poco</typeparam>
        /// <param name="sql">SQL statement</param>
        /// <param name="args">Statement parameters</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Delete<T>(string sql, params object[] args);

        /// <summary>
        /// Deletes the poco with the specified type from the database, using a SQL statement.
        /// </summary>
        /// <typeparam name="T">Type of poco</typeparam>
        /// <param name="sql">SQL statement</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Delete<T>(Sql sql);

        /// <summary>
        /// Deletes the poco with the specified type from the database, using the
        /// poco's primary key.
        /// </summary>
        /// <typeparam name="T">Type of poco</typeparam>
        /// <param name="pocoOrPrimaryKey">Primary key value</param>
        /// <returns>
        /// Number of rows affected by the update operation.
        /// </returns>
        int Delete<T>(object pocoOrPrimaryKey);

        /// <summary>
        /// Saves the specified poco into the database.
        /// </summary>
        /// <param name="tableName">Name of the table to delete the data from</param>
        /// <param name="primaryKeyName">
        /// Name of primary key (compound key names separated by comma)
        /// </param>
        /// <param name="poco">Poco instance to delete</param>
        void Save(string tableName, string primaryKeyName, object poco);

        /// <summary>
        /// Saves the specified poco into the database.
        /// </summary>
        /// <param name="poco">Poco instance to delete</param>
        void Save(object poco);
    }
}