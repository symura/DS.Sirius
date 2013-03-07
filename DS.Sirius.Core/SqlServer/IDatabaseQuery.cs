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
using System.Collections.Generic;

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// This interface defines the database query operations
    /// </summary>
    public interface IDatabaseQuery
    {
        /// <summary>
        /// Opens a shared connection.
        /// </summary>
        void OpenSharedConnection();

        /// <summary>
        /// Closes an open shared connection.
        /// </summary>
        void CloseSharedConnection();

        /// <summary>
        /// Executes the specified SQL batch with the provided parameters.
        /// </summary>
        /// <param name="sql">SQL string</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>Number of rows affected</returns>
        int Execute(string sql, params object[] args);

        /// <summary>
        /// Executes the specified SQL batch.
        /// </summary>
        /// <param name="sql">SQL string</param>
        /// <returns>Number of rows affected</returns>
        int Execute(Sql sql);

        /// <summary>
        /// Executes an SQL batch that returns a scalar value.
        /// </summary>
        /// <typeparam name="T">Type of scalar value</typeparam>
        /// <param name="sql">SQL string</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The value resulted from executing this SQL batch</returns>
        T ExecuteScalar<T>(string sql, params object[] args);

        /// <summary>
        /// Executes an SQL batch that returns a scalar value.
        /// </summary>
        /// <typeparam name="T">Type of scalar value</typeparam>
        /// <param name="sql">SQL string</param>
        /// <returns>The value resulted from executing this SQL batch</returns>
        T ExecuteScalar<T>(Sql sql);

        /// <summary>
        /// Fetches records from the database.
        /// </summary>
        /// <typeparam name="T">Type of poco to fetch.</typeparam>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T> Fetch<T>();

        /// <summary>
        /// Fetches records from the database with the specified SQL fragment.
        /// </summary>
        /// <typeparam name="T">Type of poco to fetch.</typeparam>
        /// <param name="sql">SQL fragment</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T> Fetch<T>(string sql, params object[] args);

        /// <summary>
        /// Fetches records from the database with the specified SQL fragment.
        /// </summary>
        /// <typeparam name="T">Type of poco to fetch.</typeparam>
        /// <param name="sql">SQL fragment</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T> Fetch<T>(Sql sql);

        /// <summary>
        /// Fetches a page of records from the database with the specified SQL fragment.
        /// </summary>
        /// <typeparam name="T">Type of poco to fetch.</typeparam>
        /// <param name="page">Page index</param>
        /// <param name="take">Number of items per page</param>
        /// <param name="sql">SQL fragment</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T> Fetch<T>(long page, long take, string sql, params object[] args);

        /// <summary>
        /// Fetches a page of records from the database with the specified SQL fragment.
        /// </summary>
        /// <typeparam name="T">Type of poco to fetch.</typeparam>
        /// <param name="page">Page index</param>
        /// <param name="take">Number of items per page</param>
        /// <param name="sql">SQL fragment</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T> Fetch<T>(long page, long take, Sql sql);

        /// <summary>
        /// Fetches a page of records from the database with the specified SQL fragment.
        /// </summary>
        /// <typeparam name="T">Type of poco to fetch.</typeparam>
        /// <param name="skip">Page index</param>
        /// <param name="take">Number of items per page</param>
        /// <param name="sql">SQL fragment</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The paged result of the query.</returns>
        Page<T> Page<T>(long skip, long take, string sql, params object[] args);

        /// <summary>
        /// Fetches a page of records from the database with the specified SQL fragment.
        /// </summary>
        /// <typeparam name="T">Type of poco to fetch.</typeparam>
        /// <param name="skip">Page index</param>
        /// <param name="take">Number of items per page</param>
        /// <param name="sql">SQL fragment</param>
        /// <returns>The paged result of the query.</returns>
        Page<T> Page<T>(long skip, long take, Sql sql);

        /// <summary>
        /// Fetches a chunk of records from the database with the specified SQL fragment.
        /// </summary>
        /// <typeparam name="T">Type of poco to fetch.</typeparam>
        /// <param name="skip">Number of records to skip from the beginning of the record set</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="sql">SQL fragment</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The paged result of the query.</returns>
        List<T> SkipTake<T>(long skip, long take, string sql, params object[] args);

        /// <summary>
        /// Fetches a chunk of records from the database with the specified SQL fragment.
        /// </summary>
        /// <typeparam name="T">Type of poco to fetch.</typeparam>
        /// <param name="skip">Number of records to skip from the beginning of the record set</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="sql">SQL fragment</param>
        /// <returns>The paged result of the query.</returns>
        List<T> SkipTake<T>(long skip, long take, Sql sql);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        /// <remarks>
        /// More details: http://www.toptensoftware.com/Articles/115/PetaPoco-Mapping-One-to-Many-and-Many-to-One-Relationships
        /// </remarks>
        List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        /// <remarks>
        /// More details: http://www.toptensoftware.com/Articles/115/PetaPoco-Mapping-One-to-Many-and-Many-to-One-Relationships
        /// </remarks>
        List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        /// <remarks>
        /// More details: http://www.toptensoftware.com/Articles/115/PetaPoco-Mapping-One-to-Many-and-Many-to-One-Relationships
        /// </remarks>
        List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T1> Fetch<T1, T2>(string sql, params object[] args);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T1> Fetch<T1, T2, T3>(string sql, params object[] args);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<T1> Query<T1, T2>(string sql, params object[] args);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="types"></param>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<TRet> Query<TRet>(Type[] types, object cb, Sql sql);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T1> Fetch<T1, T2>(Sql sql);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T1> Fetch<T1, T2, T3>(Sql sql);

        /// <summary>
        /// Retrieves a resultset that can be mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        List<T1> Fetch<T1, T2, T3, T4>(Sql sql);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<T1> Query<T1, T2>(Sql sql);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<T1> Query<T1, T2, T3>(Sql sql);

        /// <summary>
        /// Retrieves an enumeration mapped to multiple poco types.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql);

        /// <summary>
        /// Retrieves an enumeration mapped to the specified poco type.
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<T> Query<T>(string sql, params object[] args);

        /// <summary>
        /// Retrieves an enumeration mapped to the specified poco type, using the specified SQL object.
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL object</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        IEnumerable<T> Query<T>(Sql sql);

        /// <summary>
        /// Retrieves a single object of the specified poco by primary key.
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="primaryKey">Object representing the primary key</param>
        /// <returns>The single object retrieved</returns>
        /// <remarks>Raises an exception, if not a single object found</remarks>
        T SingleById<T>(object primaryKey);

        /// <summary>
        /// Retrieves a single object of the specified poco by primary key.
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="primaryKey">Object representing the primary key</param>
        /// <returns>The single object retrieved, or the poco's default value, if not found in database.</returns>
        T SingleOrDefaultById<T>(object primaryKey);

        /// <summary>
        /// Retrieves a single object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The single object retrieved</returns>
        /// <remarks>Raises an exception, if not a single object found</remarks>
        T Single<T>(string sql, params object[] args);

        /// <summary>
        /// Retrieves a single object and maps it into another object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="instance">Poco instance to map the column values to</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The single object retrieved</returns>
        /// <remarks>Raises an exception, if not a single object found</remarks>
        T SingleInto<T>(T instance, string sql, params object[] args);

        /// <summary>
        /// Retrieves a single object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The single object retrieved, or the poco's default value, if not found in database.</returns>
        T SingleOrDefault<T>(string sql, params object[] args);

        /// <summary>
        /// Retrieves a single object and maps it into another object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="instance">Poco instance to map the column values to</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The single object retrieved</returns>
        T SingleOrDefaultInto<T>(T instance, string sql, params object[] args);

        /// <summary>
        /// Retrieves the object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The single object retrieved, or the poco's default value, if not found in database.</returns>
        T First<T>(string sql, params object[] args);

        /// <summary>
        /// Retrieves the first object and maps it into another object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="instance">Poco instance to map the column values to</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The single object retrieved</returns>
        /// <remarks>Raises an exception, if not a single object found</remarks>
        T FirstInto<T>(T instance, string sql, params object[] args);

        /// <summary>
        /// Retrieves the first object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The single object retrieved, or the poco's default value, if not found in database.</returns>
        T FirstOrDefault<T>(string sql, params object[] args);

        /// <summary>
        /// Retrieves a single object and maps it into another object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="instance">Poco instance to map the column values to</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The single object retrieved</returns>
        T FirstOrDefaultInto<T>(T instance, string sql, params object[] args);

        /// <summary>
        /// Retrieves a single object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The single object retrieved</returns>
        /// <remarks>Raises an exception, if not a single object found</remarks>
        T Single<T>(Sql sql);

        /// <summary>
        /// Retrieves a single object and maps it into another object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="instance">Poco instance to map the column values to</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The single object retrieved</returns>
        /// <remarks>Raises an exception, if not a single object found</remarks>
        T SingleInto<T>(T instance, Sql sql);

        /// <summary>
        /// Retrieves a single object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The single object retrieved, or the poco's default value, if not found in database.</returns>
        T SingleOrDefault<T>(Sql sql);

        /// <summary>
        /// Retrieves a single object and maps it into another object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="instance">Poco instance to map the column values to</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The single object retrieved</returns>
        T SingleOrDefaultInto<T>(T instance, Sql sql);

        /// <summary>
        /// Retrieves the object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The single object retrieved, or the poco's default value, if not found in database.</returns>
        T First<T>(Sql sql);

        /// <summary>
        /// Retrieves the first object and maps it into another object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="instance">Poco instance to map the column values to</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The single object retrieved</returns>
        /// <remarks>Raises an exception, if not a single object found</remarks>
        T FirstInto<T>(T instance, Sql sql);

        /// <summary>
        /// Retrieves the first object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The single object retrieved, or the poco's default value, if not found in database.</returns>
        T FirstOrDefault<T>(Sql sql);

        /// <summary>
        /// Retrieves a single object and maps it into another object using the specified SQL batch
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="instance">Poco instance to map the column values to</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The single object retrieved</returns>
        T FirstOrDefaultInto<T>(T instance, Sql sql);

        /// <summary>
        /// Gets a dictionary of key/value pairs from the result set.
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="sql">SQL object</param>
        /// <returns></returns>
        Dictionary<TKey, TValue> Dictionary<TKey, TValue>(Sql sql);

        /// <summary>
        /// Gets a dictionary of key/value pairs from the result set.
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>Dictionary mapped from the result set.</returns>
        Dictionary<TKey, TValue> Dictionary<TKey, TValue>(string sql, params object[] args);
        
        /// <summary>
        /// Checks whether the record specified by the primary key value exists in the database.
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="primaryKey">Primary key value</param>
        /// <returns>True, if the record exists; otherwise, false</returns>
        bool Exists<T>(object primaryKey);

        /// <summary>
        /// Gets or sets the value of the command timeout for a single database call.
        /// </summary>
        int OneTimeCommandTimeout { get; set; }

        
        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, string sql, params object[] args);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, string sql, params object[] args);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, string sql, params object[] args);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, Sql sql);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, Sql sql);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <typeparam name="TRet">Poco type to return</typeparam>
        /// <param name="cb">Custom relator to connect pocos</param>
        /// <param name="sql">SQL batch</param>
        /// <returns>The list of pocos fetched from the database.</returns>
        TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Sql sql);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The tuple that holds lists of pocos fetched from the database.</returns>
        Tuple<List<T1>, List<T2>> FetchMultiple<T1, T2>(string sql, params object[] args);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The tuple that holds lists of pocos fetched from the database.</returns>
        Tuple<List<T1>, List<T2>, List<T3>> FetchMultiple<T1, T2, T3>(string sql, params object[] args);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <param name="args">Array of query parameters</param>
        /// <returns>The tuple that holds lists of pocos fetched from the database.</returns>
        Tuple<List<T1>, List<T2>, List<T3>, List<T4>> FetchMultiple<T1, T2, T3, T4>(string sql, params object[] args);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The tuple that holds lists of pocos fetched from the database.</returns>
        Tuple<List<T1>, List<T2>> FetchMultiple<T1, T2>(Sql sql);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The tuple that holds lists of pocos fetched from the database.</returns>
        Tuple<List<T1>, List<T2>, List<T3>> FetchMultiple<T1, T2, T3>(Sql sql);

        /// <summary>
        /// Fetches multiple result sets in one query.
        /// </summary>
        /// <typeparam name="T1">Poco type 1</typeparam>
        /// <typeparam name="T2">Poco type 2</typeparam>
        /// <typeparam name="T3">Poco type 3</typeparam>
        /// <typeparam name="T4">Poco type 4</typeparam>
        /// <param name="sql">SQL batch</param>
        /// <returns>The tuple that holds lists of pocos fetched from the database.</returns>
        Tuple<List<T1>, List<T2>, List<T3>, List<T4>> FetchMultiple<T1, T2, T3, T4>(Sql sql);
    }
}