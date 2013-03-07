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
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
using DS.Sirius.Core.Common;
using DS.Sirius.Core.SqlServer.Exceptions;
using IsolationLevel = System.Data.IsolationLevel;

#pragma warning disable 1591

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// This class implements database query and data manipulation operations.
    /// </summary>
    public class Database : IDatabase, ICommonDataOperations
    {
        private const string PARAM_PREFIX = "@";

        private readonly string _nameOrConnectionString;
        private string _connectionString;
        int _sharedConnectionDepth;
        int _transactionDepth;
        bool _transactionCancelled;
        string _lastSql;
        object[] _lastArgs;

        /// <summary>
        /// Creates a new database instance with the specified connection string and provider name
        /// </summary>
        /// <param name="nameOrConnectionString">Database connection string</param>
        public Database(string nameOrConnectionString)
        {
            VersionException = VersionExceptionHandling.Exception;
            _nameOrConnectionString = nameOrConnectionString;
            OperationCompleted = false;
            _transactionDepth = 0;
            ForceDateTimesToUtc = true;
            EnableAutoSelect = true;
        }

        /// <summary>
        /// Gets the connection string used to initialize this database
        /// </summary>
        public string NameOrConnectionString { get { return _nameOrConnectionString; } }

        /// <summary>
        /// Gets the database connection string 
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString ?? (_connectionString = SqlHelper.GetConnectionString(_nameOrConnectionString)); }
        }

        /// <summary>
        /// Gets the flag telling if connection should be kept alive.
        /// </summary>
        /// <remarks>
        /// Set to true to keep the first opened connection alive until this object is disposed
        /// </remarks>
        public bool KeepConnectionAlive { get; set; }

        /// <summary>
        /// Gets the flag indicating if the current operation is completed.
        /// </summary>
        public bool OperationCompleted { get; private set; }

        /// <summary>
        /// Gets the current transactionscope used to encapsulate context tracking
        /// </summary>
        public TransactionScope TransactionScope { get; private set; }

        /// <summary>
        /// Gets or sets the mode of handling version exceptions.
        /// </summary>
        public VersionExceptionHandling VersionException { get; set; }

        /// <summary>
        /// Gets the shared connection belonging to this database instance.
        /// </summary>
        public SqlConnection Connection { get; private set; }

        /// <summary>
        /// Gets the transaction object belongign to this database.
        /// </summary>
        public SqlTransaction Transaction { get; private set; }

        /// <summary>
        /// Forces using UTC dtae and time values
        /// </summary>
        public bool ForceDateTimesToUtc { get; set; }

        /// <summary>
        /// Enables auto select
        /// </summary>
        public bool EnableAutoSelect { get; set; }

        /// <summary>
        /// Adds a select clause to the specified SQL command text
        /// </summary>
        /// <typeparam name="T">Type of entity used in the query</typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        string AddSelectClause<T>(string sql)
        {
            // --- Remove the starting semicolon
            if (sql.StartsWith(";")) return sql.Substring(1);

            if (!s_RxSelect.IsMatch(sql))
            {
                var pd = PocoData.ForType(typeof(T));
                var tableName = EscapeTableName(pd.TableInfo.TableName);
                var cols = string.Join(", ", (from c in pd.QueryColumns select EscapeSqlIdentifier(c)).ToArray());
                sql = !s_RxFrom.IsMatch(sql) 
                          ? string.Format("SELECT {0} FROM {1} {2}", cols, tableName, sql) 
                          : string.Format("SELECT {0} {1}", cols, sql);
            }
            return sql;
        }

        /// <summary>
        /// Ensures that there is an existing SqlConnectionInstance
        /// </summary>
        /// <returns></returns>
        private void EnsureConnection()
        {
            if (Connection == null)
            {
                Connection = SqlHelper.CreateSqlConnection(_nameOrConnectionString);
            }
        }

        /// <summary>
        /// Signs that the current sequence of operations can be completed.
        /// </summary>
        public void CompleteOperation()
        {
            if (OperationCompleted)
            {
                throw new InvalidOperationException("The current scope has already been completed.");
            }
            OperationCompleted = true;
        }

        /// <summary>
        /// Creates a transaction scope for this context.
        /// </summary>
        /// <param name="option">TransactionScope options</param>
        /// <param name="level">Transaction isolation level</param>
        public void CreateTransactionScope(TransactionScopeOption? option = null, System.Transactions.IsolationLevel? level = null)
        {
            if (TransactionScope != null)
            {
                throw new InvalidOperationException("There is already a transaction scope for this context.");
            }
            TransactionScope = option.HasValue
                                   ? (level.HasValue ?
                                      new TransactionScope(option.Value, new TransactionOptions { IsolationLevel = level.Value })
                                    : new TransactionScope(option.Value)
                                    )
                                   : new TransactionScope();
        }

        /// <summary>
        /// Automatically close one open shared connection 
        /// </summary>
        public void Dispose()
        {
            // --- Check if the current operation should be aborted
            if (!OperationCompleted)
            {
                if (TransactionScope != null)
                {
                    TransactionScope.Dispose();
                    TransactionScope = null;
                }
                return;
            }

            // --- Transaction has been committed successfully, so log changes.
            CloseSharedConnection();
            if (TransactionScope == null) return;
            TransactionScope.Complete();
            TransactionScope = null;
        }

        /// <summary>
        /// Opens a connection.
        /// </summary>
        /// <remarks>The call of this method can be nested.</remarks>
        public void OpenSharedConnection()
        {
            if (_sharedConnectionDepth == 0)
            {
                EnsureConnection();
                if (Connection.State == ConnectionState.Broken) Connection.Close();
                if (Connection.State == ConnectionState.Closed) Connection.Open();
                Connection = OnConnectionOpened(Connection);
                if (KeepConnectionAlive) _sharedConnectionDepth++;
            }
            _sharedConnectionDepth++;
        }

        /// <summary>
        /// Override this method to respond to the event when a connection has been opened.
        /// </summary>
        /// <param name="conn">Connection instance</param>
        /// <returns>The connection to be used</returns>
        protected virtual SqlConnection OnConnectionOpened(SqlConnection conn)
        {
            return conn;
        }

        /// <summary>
        /// Close a previously opened connection 
        /// </summary>
        public void CloseSharedConnection()
        {
            if (_sharedConnectionDepth <= 0) return;
            _sharedConnectionDepth--;
            if (_sharedConnectionDepth != 0) return;
            OnConnectionClosing(Connection);
            Connection.Dispose();
            Connection = null;
        }

        /// <summary>
        /// Overrid this method to respond to the event when a connection has been closed.
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void OnConnectionClosing(SqlConnection conn)
        {
        }

        /// <summary>
        /// Creates a new data parameter.
        /// </summary>
        /// <returns>The new, empty data parameter</returns>
        public SqlParameter CreateParameter()
        {
            using (var conn = SqlHelper.CreateSqlConnection(_nameOrConnectionString))
            {
                using (var comm = conn.CreateCommand()) 
                    return comm.CreateParameter();
            }
        }

        // Helper to create a transaction scope
        /// <summary>
        /// Gets a new transaction scope.
        /// </summary>
        /// <returns>The new transaction scope</returns>
        public Transaction GetTransaction()
        {
            return GetTransaction(null);
        }

        /// <summary>
        /// Gets a new transaction scope using the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level</param>
        /// <returns>The new transaction scope</returns>
        public Transaction GetTransaction(IsolationLevel? isolationLevel)
        {
            return new Transaction(this, isolationLevel);
        }

        /// <summary>
        /// Override this method to handle additional activites when a new transaction is started.
        /// </summary>
        public virtual void OnBeginTransaction() { }

        /// <summary>
        /// Override this method to handle additional activites when a new transaction is ended.
        /// </summary>
        public virtual void OnEndTransaction() { }

        /// <summary>
        /// Starts a new transaction.
        /// </summary>
        public void BeginTransaction()
        {
            BeginTransaction(null);
        }

        /// <summary>
        /// Starts a new transaction with the specified optional isolation level.
        /// </summary>
        /// <param name="isolationLevel">Isolation level to use.</param>
        public void BeginTransaction(IsolationLevel? isolationLevel)
        {
            _transactionDepth++;
            if (_transactionDepth != 1) return;
            OpenSharedConnection();
            Transaction = isolationLevel == null
                ? Connection.BeginTransaction()
                : Connection.BeginTransaction(isolationLevel.Value);
            _transactionCancelled = false;
            OnBeginTransaction();
        }

        /// <summary>
        /// Internal helper to cleanup transaction stuff 
        /// </summary>
        void CleanupTransaction()
        {
            OnEndTransaction();
            if (_transactionCancelled) Transaction.Rollback();
            else Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
            CloseSharedConnection();
        }

        /// <summary>
        /// Aborts the current transaction.
        /// </summary>
        public void AbortTransaction()
        {
            if (_transactionDepth == 0)
            {
                throw new InvalidOperationException("There is no open transaction to abort.");
            }
            _transactionCancelled = true;
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        /// <summary>
        /// Completes the current transaction.
        /// </summary>
        public void CompleteTransaction()
        {
            if (_transactionDepth == 0)
            {
                throw new InvalidOperationException("There is no open transaction to complete.");
            }
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        // --- Helper to handle named parameters from object properties
        static readonly Regex s_RxParams = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);

        /// <summary>
        /// Processes the specified named parameters
        /// </summary>
        /// <param name="sql">SQL statement with parameter placeholders</param>
        /// <param name="argsSrc">Array of source parameters</param>
        /// <param name="argsDest">List of destination arguments</param>
        /// <returns>SQL statement with processed parameters</returns>
        public static string ProcessParams(string sql, object[] argsSrc, List<object> argsDest)
        {
            return s_RxParams.Replace(
                sql,
                m =>
                    {
                        var param = m.Value.Substring(1);
                        object argVal;
                        int paramIndex;
                        if (int.TryParse(param, out paramIndex))
                        {
                            // --- Numbered parameter
                            if (paramIndex < 0 || paramIndex >= argsSrc.Length)
                                throw new ArgumentOutOfRangeException(
                                    string.Format(
                                        "Parameter '@{0}' specified but only {1} parameters supplied (in `{2}`)",
                                        paramIndex, argsSrc.Length, sql));
                            argVal = argsSrc[paramIndex];
                        }
                        else
                        {
                            // --- Look for a property on one of the arguments with this name
                            var found = false;
                            argVal = null;
                            foreach (var o in argsSrc)
                            {
                                var pi = o.GetType().GetProperty(param);
                                if (pi == null) continue;
                                argVal = pi.GetValue(o, null);
                                found = true;
                                break;
                            }

                            if (!found)
                                throw new ArgumentException(
                                    string.Format(
                                        "Parameter '@{0}' specified but none of the passed arguments have " +
                                        "a property with this name (in '{1}')",
                                        param, sql));
                        }

                        // --- Expand collections to parameter lists
                        if ((argVal as System.Collections.IEnumerable) != null &&
                            (argVal as string) == null &&
                            (argVal as byte[]) == null)
                        {
                            var sb = new StringBuilder();
                            foreach (
                                var i in
                                    argVal as System.Collections.IEnumerable)
                            {
                                var indexOfExistingValue = argsDest.IndexOf(i);
                                if (indexOfExistingValue >= 0)
                                {
                                    sb.Append((sb.Length == 0 ? "@" : ",@") + indexOfExistingValue);
                                }
                                else
                                {
                                    sb.Append((sb.Length == 0 ? "@" : ",@") + argsDest.Count);
                                    argsDest.Add(i);
                                }
                            }
                            if (sb.Length == 0)
                            {
                                sb.AppendFormat(
                                    "select 1 /*peta_dual*/ where 1 = 0");
                            }
                            return sb.ToString();
                        }
                        var indexOfValue = argsDest.IndexOf(argVal);
                        if (indexOfValue >= 0) return "@" + indexOfValue;
                        argsDest.Add(argVal);
                        return "@" + (argsDest.Count - 1).ToString(CultureInfo.InvariantCulture);
                    }
                );
        }

        /// <summary>
        /// Adds a parameter to a DB command
        /// </summary>
        /// <param name="cmd">Command to add the parameter for</param>
        /// <param name="item">Parameter object</param>
        /// <param name="parameterPrefix">Parameter prefix</param>
        void AddParam(SqlCommand cmd, object item, string parameterPrefix)
        {
            // --- Convert value to from poco type to db type
            if (DbObjectMapper != null && item != null)
            {
                var fn = DbObjectMapper.GetToDbConverter(item.GetType());
                if (fn != null) item = fn(item);
            }

            // --- Support passed in parameters
            var idbParam = item as SqlParameter;
            if (idbParam != null)
            {
                idbParam.ParameterName = string.Format("{0}{1}", parameterPrefix, cmd.Parameters.Count);
                cmd.Parameters.Add(idbParam);
                return;
            }
            var p = cmd.CreateParameter();
            p.ParameterName = string.Format("{0}{1}", parameterPrefix, cmd.Parameters.Count);

            if (item == null)
            {
                p.Value = DBNull.Value;
            }
            else
            {
                var t = item.GetType();
                if (t.IsEnum)
                {
                    p.Value = (int)item;
                }
                else if (t == typeof(Guid))
                {
                    p.Value = item.ToString();
                    p.DbType = DbType.String;
                    p.Size = 40;
                }
                else if (t == typeof(string))
                {
                    p.Size = Math.Max((item.ToString()).Length + 1, 4000);		// Help query plan caching by using common size
                    p.Value = item;
                }
                else if (t == typeof(bool))
                {
                    p.Value = ((bool)item) ? 1 : 0;
                }
                else if (item.GetType().Name == "SqlGeography") // --- SqlGeography is a CLR Type
                {
                    // --- 'geography' is the equivalent SQL Server Type
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geography", null); 
                    p.Value = item;
                }
                else if (item.GetType().Name == "SqlGeometry") // --- SqlGeometry is a CLR Type
                {
                    // --- 'geometry' is the equivalent SQL Server Type
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geometry", null); 
                    p.Value = item;
                }
                else
                {
                    p.Value = item;
                }
            }
            cmd.Parameters.Add(p);
        }

        /// <summary>
        /// Create a command
        /// </summary>
        /// <param name="connection">Connection instance</param>
        /// <param name="sql">Sql command text</param>
        /// <param name="args">Sql command arguments</param>
        /// <returns></returns>
       private SqlCommand CreateCommand(SqlConnection connection, string sql, params object[] args)
        {
            // --- Double @@ escapes a single @
            sql = sql.Replace("@@", "@");		   

            // --- Create the command and add parameters
            var cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = sql;
            cmd.Transaction = Transaction;
            foreach (var item in args) AddParam(cmd, item, PARAM_PREFIX);
            if (!String.IsNullOrWhiteSpace(sql)) DoPreExecute(cmd);
            return cmd;
        }

        /// <summary>
        /// Override this method to log exceptions
        /// </summary>
        /// <param name="ex"></param>
        public virtual void OnException(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            System.Diagnostics.Debug.WriteLine(LastCommand);
        }

        /// <summary>
        /// Override this method to respond to the event when a command is about to be executed.
        /// </summary>
        /// <param name="cmd">Command to execute</param>
        protected virtual void OnExecutingCommand(SqlCommand cmd) { }

        /// <summary>
        /// Override this method to respond to the event when a command has been executed.
        /// </summary>
        /// <param name="cmd">Command executed</param>
        protected virtual void OnExecutedCommand(SqlCommand cmd) { }

        /// <summary>
        /// Execute a non-query command
        /// </summary>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command parameters</param>
        /// <returns></returns>
        public int Execute(string sql, params object[] args)
        {
            return Execute(new Sql(sql, args));
        }

        /// <summary>
        /// Execute the specified non-query command
        /// </summary>
        /// <param name="sqlStatement">Command to execute</param>
        /// <returns></returns>
        public int Execute(Sql sqlStatement)
        {
            var sql = sqlStatement.SQL;
            var args = sqlStatement.Arguments;

            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(Connection, sql, args))
                    {
                        var result = cmd.ExecuteNonQuery();
                        OnExecutedCommand(cmd);
                        return result;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
                throw;
            }
        }

        // Execute and cast a scalar property

        /// <summary>
        /// Execute the command and cast it to a scalar expression
        /// </summary>
        /// <typeparam name="T">Result scalar type</typeparam>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <returns>Expected scalar values</returns>
        public T ExecuteScalar<T>(string sql, params object[] args)
        {
            return ExecuteScalar<T>(new Sql(sql, args));
        }

        /// <summary>
        /// Execute the command and cast it to a scalar expression
        /// </summary>
        /// <typeparam name="T">Result scalar type</typeparam>
        /// <param name="sqlStatement">Sql statement</param>
        /// <returns>Expected scalar values</returns>
        public T ExecuteScalar<T>(Sql sqlStatement)
        {
            var sql = sqlStatement.SQL;
            var args = sqlStatement.Arguments;
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(Connection, sql, args))
                    {
                        var val = cmd.ExecuteScalar();
                        OnExecutedCommand(cmd);

                        var resultType = typeof(T);
                        var underlyingType = Nullable.GetUnderlyingType(resultType);
                        return underlyingType == null
                                   ? (T) Convert.ChangeType(val, resultType)
                                   : (val == null || val == DBNull.Value
                                          ? default(T)
                                          : (T) Convert.ChangeType(val, underlyingType));
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
                throw;
            }
        }

        static readonly Regex s_RxSelect = 
            new Regex(@"\A\s*(SELECT|EXECUTE|CALL|EXEC)\s", RegexOptions.Compiled | RegexOptions.Singleline 
                | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static readonly Regex s_RxFrom = 
            new Regex(@"\A\s*FROM\s", RegexOptions.Compiled | RegexOptions.Singleline 
                | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Retrieve a typed list of pocos
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <returns>List of pocos</returns>
        public List<T> Fetch<T>(string sql, params object[] args)
        {
            return Fetch<T>(new Sql(sql, args));
        }

        /// <summary>
        /// Retrieve a typed list of pocos
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL statement</param>
        /// <returns>List of pocos</returns>
        public List<T> Fetch<T>(Sql sql)
        {
            return Query<T>(sql).ToList();
        }

        /// <summary>
        /// Retrieve a typed list of all pocos of the specified type
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <returns>List of pocos</returns>
        public List<T> Fetch<T>()
        {
            return Fetch<T>(String.Empty);
        }

        static readonly Regex s_RxColumns = 
            new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | 
                RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex s_RxOrderBy = 
            new Regex(@"\bORDER\s+BY\s+(?!.*?(?:\)|\s+)AS\s)(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", 
                RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex s_RxDistinct = 
            new Regex(@"\ADISTINCT\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | 
                RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Splits the specified SQL command text for paging queries
        /// </summary>
        /// <param name="sql">Command text</param>
        /// <param name="sqlCount">SQL COUNT clause</param>
        /// <param name="sqlSelectRemoved">SELECT list removed</param>
        /// <param name="sqlOrderBy">SQL ORDER BY clause</param>
        /// <returns>True, if split is successful; otherwise, false.</returns>
        protected static bool SplitSqlForPaging(string sql, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
        {
            sqlSelectRemoved = null;
            sqlCount = null;
            sqlOrderBy = null;

            // --- Extract the columns from "SELECT <whatever> FROM"
            var m = s_RxColumns.Match(sql);
            if (!m.Success) return false;

            // --- Save column list and replace with COUNT(*)
            var g = m.Groups[1];
            sqlSelectRemoved = sql.Substring(g.Index);
            sqlCount = s_RxDistinct.IsMatch(sqlSelectRemoved)
                           ? sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " +
                             sql.Substring(g.Index + g.Length)
                           : sql.Substring(0, g.Index) + "COUNT(*) " + sql.Substring(g.Index + g.Length);

            // Look for an "ORDER BY <whatever>" clause
            m = s_RxOrderBy.Match(sqlCount);
            if (m.Success)
            {
                g = m.Groups[0];
                sqlOrderBy = g.ToString();
                sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
            }
            return true;
        }

        /// <summary>
        /// Builds a query to page the result set
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <param name="sqlCount">SQL COUNT clause</param>
        /// <param name="sqlPage">SQL paged query</param>
        protected void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, 
            out string sqlCount, out string sqlPage)
        {
            // --- Add auto select clause
            sql = AddSelectClause<T>(sql);

            // --- Split the SQL into the bits we need
            string sqlSelectRemoved;
            string sqlOrderBy;
            if (!SplitSqlForPaging(sql, out sqlCount, out sqlSelectRemoved, out sqlOrderBy))
                throw new Exception("Unable to parse SQL statement for paged query");
            
            // --- Build the SQL for the actual final result
            sqlSelectRemoved = s_RxOrderBy.Replace(sqlSelectRemoved, "");
            if (s_RxDistinct.IsMatch(sqlSelectRemoved))
            {
                sqlSelectRemoved = "peta_inner.* FROM (SELECT " + sqlSelectRemoved + ") peta_inner";
            }
            sqlPage = string.Format(
                "SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged " +
                "WHERE peta_rn>@{2} AND peta_rn<=@{3}",
            sqlOrderBy ?? "ORDER BY (SELECT NULL)", sqlSelectRemoved, args.Length, args.Length + 1);
            args = args.Concat(new object[] { skip, skip + take }).ToArray();
        }

        /// <summary>
        /// Fetches the specified page from the table represented by the poco type
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <returns>Resulting page</returns>
        public Page<T> Page<T>(long skip, long take, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>(skip, take, sql, ref args, out sqlCount, out sqlPage);

            // --- Save the one-time command time out and use it for both queries
            var saveTimeout = OneTimeCommandTimeout;

            // --- Setup the paged result
            var result = new Page<T>
                {
                    CurrentPage = skip,
                    ItemsPerPage = take,
                    TotalItems = ExecuteScalar<long>(sqlCount, args)
                };
            result.TotalPages = result.TotalItems / take;
            if ((result.TotalItems % take) != 0) result.TotalPages++;

            OneTimeCommandTimeout = saveTimeout;

            // --- Get the records
            result.Items = Fetch<T>(sqlPage, args);
            return result;
        }

        /// <summary>
        /// Fetches the specified page from the table represented by the poco type
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="sql">SQL statement</param>
        /// <returns>Resulting page</returns>
        public Page<T> Page<T>(long skip, long take, Sql sql)
        {
            return Page<T>(skip, take, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// Fetches the specified page from the table represented by the poco type
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="page">Page number</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <returns>Resulting page</returns>
        public List<T> Fetch<T>(long page, long take, string sql, params object[] args)
        {
            return SkipTake<T>((page - 1) * take, take, sql, args);
        }

        /// <summary>
        /// Fetches the specified page from the table represented by the poco type
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="page">Page number</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="sql">SQL statement</param>
        /// <returns>Resulting page</returns>
        public List<T> Fetch<T>(long page, long take, Sql sql)
        {
            return SkipTake<T>((page - 1) * take, take, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// Fetches the specified page from the table represented by the poco type
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <returns>Resulting page</returns>
        public List<T> SkipTake<T>(long skip, long take, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>(skip, take, sql, ref args, out sqlCount, out sqlPage);
            return Fetch<T>(sqlPage, args);
        }

        /// <summary>
        /// Fetches the specified page from the table represented by the poco type
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="sql">SQL statement</param>
        /// <returns>Resulting page</returns>
        public List<T> SkipTake<T>(long skip, long take, Sql sql)
        {
            return SkipTake<T>(skip, take, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// Retrieves a dictionary from the specified SQL statement according to the specified key and value types
        /// </summary>
        /// <typeparam name="TKey">Type of dictionary key</typeparam>
        /// <typeparam name="TValue">Type of dictionary value</typeparam>
        /// <param name="sql">SQL statement</param>
        /// <returns>Dictionary represented by the query</returns>
        public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(Sql sql)
        {
            return Dictionary<TKey, TValue>(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// Retrieves a dictionary from the specified SQL command according to the specified key and value types
        /// </summary>
        /// <typeparam name="TKey">Type of dictionary key</typeparam>
        /// <typeparam name="TValue">Type of dictionary value</typeparam>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <returns>Dictionary represented by the query</returns>
        public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(string sql, params object[] args)
        {
            var newDict = new Dictionary<TKey, TValue>();
            var isConverterSet = false;
            Func<object, object> converter1 = x => x, converter2 = x => x;

            foreach (var line in Query<Dictionary<string, object>>(sql, args))
            {
                var key = line.ElementAt(0).Value;
                var value = line.ElementAt(1).Value;

                if (isConverterSet == false)
                {
                    converter1 = PocoData.GetConverter(ForceDateTimesToUtc, null, typeof(TKey), key.GetType()) ?? (x => x);
                    converter2 = PocoData.GetConverter(ForceDateTimesToUtc, null, typeof(TValue), value.GetType()) ?? (x => x);
                    isConverterSet = true;
                }

                var keyConverted = (TKey)Convert.ChangeType(converter1(key), typeof(TKey));

                var valueType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
                var valConv = converter2(value);
                var valConverted = valConv != null ? (TValue)Convert.ChangeType(valConv, valueType) : default(TValue);

                // ReSharper disable CompareNonConstrainedGenericWithNull
                if (keyConverted != null)
                // ReSharper restore CompareNonConstrainedGenericWithNull
                {
                    newDict.Add(keyConverted, valConverted);
                }
            }
            return newDict;
        }

        /// <summary>
        /// Retrieves an enumerable collection of poco instances
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command parameters</param>
        /// <returns>Enumeration of poco instances</returns>
        public IEnumerable<T> Query<T>(string sql, params object[] args)
        {
            return Query<T>(new Sql(sql, args));
        }

        /// <summary>
        /// Retrieves an enumerable collection of poco instances
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="sql">SQL statement</param>
        /// <returns>Enumeration of poco instances</returns>
        public IEnumerable<T> Query<T>(Sql sql)
        {
            return Query(default(T), sql);
        }

        /// <summary>
        /// Retrieves an enumerable collection of poco instances
        /// </summary>
        /// <typeparam name="T">Poco type</typeparam>
        /// <param name="instance">Poco instance</param>
        /// <param name="sqlStatement">SQL statement</param>
        /// <returns>Enumeration of poco instances</returns>
        private IEnumerable<T> Query<T>(T instance, Sql sqlStatement)
        {
            var sql = sqlStatement.SQL;
            var args = sqlStatement.Arguments;

            if (EnableAutoSelect) sql = AddSelectClause<T>(sql);

            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(Connection, sql, args))
                {
                    IDataReader r;
                    var pd = PocoData.ForType(typeof(T));
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        OnException(x);
                        throw;
                    }

                    using (r)
                    {
                        var factory = pd.GetFactory(cmd.CommandText, Connection.ConnectionString, ForceDateTimesToUtc, 0, r.FieldCount, r, instance) as Func<IDataReader, T, T>;
                        while (true)
                        {
                            T poco;
                            try
                            {
                                if (!r.Read()) yield break;
                                // ReSharper disable PossibleNullReferenceException
                                poco = factory(r, instance);
                                // ReSharper restore PossibleNullReferenceException
                            }
                            catch (Exception ex)
                            {
                                OnException(ex);
                                throw;
                            }
                            yield return poco;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }

        /// <summary>
        /// Fetches a list of compound poco entities
        /// </summary>
        /// <typeparam name="T1">Entity type 1</typeparam>
        /// <typeparam name="T2">Entity type 2</typeparam>
        /// <typeparam name="TRet">Compound entity type</typeparam>
        /// <param name="cb">converion function</param>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <returns>List of compound poco instances</returns>
        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args)
        {
            return Query(cb, sql, args).ToList();
        }

        /// <summary>
        /// Fetches a list of compound poco entities
        /// </summary>
        /// <typeparam name="T1">Entity type 1</typeparam>
        /// <typeparam name="T2">Entity type 2</typeparam>
        /// <typeparam name="T3">Entity type 3</typeparam>
        /// <typeparam name="TRet">Compound entity type</typeparam>
        /// <param name="cb">converion function</param>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <returns>List of compound poco instances</returns>
        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args)
        {
            return Query(cb, sql, args).ToList();
        }

        /// <summary>
        /// Fetches a list of compound poco entities
        /// </summary>
        /// <typeparam name="T1">Entity type 1</typeparam>
        /// <typeparam name="T2">Entity type 2</typeparam>
        /// <typeparam name="T3">Entity type 3</typeparam>
        /// <typeparam name="T4">Entity type 4</typeparam>
        /// <typeparam name="TRet">Compound entity type</typeparam>
        /// <param name="cb">converion function</param>
        /// <param name="sql">Command text</param>
        /// <param name="args">Command arguments</param>
        /// <returns>List of compound poco instances</returns>
        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args) { return Query<T1, T2, T3, T4, TRet>(cb, sql, args).ToList(); }

        // Multi QueryTableChangeLog
        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, new Sql(sql, args)); }
        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb, new Sql(sql, args)); }
        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, new Sql(sql, args)); }

        // Multi Fetch (SQL builder)
        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql) { return Query<T1, T2, TRet>(cb, sql).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql) { return Query<T1, T2, T3, TRet>(cb, sql).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql) { return Query<T1, T2, T3, T4, TRet>(cb, sql).ToList(); }

        // Multi QueryTableChangeLog (SQL builder)
        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, sql); }
        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb, sql); }
        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, sql); }

        // Multi Fetch (Simple)
        public List<T1> Fetch<T1, T2>(string sql, params object[] args) { return Query<T1, T2>(sql, args).ToList(); }
        public List<T1> Fetch<T1, T2, T3>(string sql, params object[] args) { return Query<T1, T2, T3>(sql, args).ToList(); }
        public List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args) { return Query<T1, T2, T3, T4>(sql, args).ToList(); }

        // Multi QueryTableChangeLog (Simple)
        public IEnumerable<T1> Query<T1, T2>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, new Sql(sql, args)); }
        public IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, new Sql(sql, args)); }
        public IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, new Sql(sql, args)); }

        // Multi Fetch (Simple) (SQL builder)
        public List<T1> Fetch<T1, T2>(Sql sql) { return Query<T1, T2>(sql).ToList(); }
        public List<T1> Fetch<T1, T2, T3>(Sql sql) { return Query<T1, T2, T3>(sql).ToList(); }
        public List<T1> Fetch<T1, T2, T3, T4>(Sql sql) { return Query<T1, T2, T3, T4>(sql).ToList(); }

        // Multi QueryTableChangeLog (Simple) (SQL builder)
        public IEnumerable<T1> Query<T1, T2>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, sql); }
        public IEnumerable<T1> Query<T1, T2, T3>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, sql); }
        public IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, sql); }

        // Automagically guess the property relationships between various POCOs and create a delegate that will set them up
        object GetAutoMapper(Type[] types)
        {
            // Build a key
            var kb = new StringBuilder();
            foreach (var t in types)
            {
                kb.Append(t.ToString());
                kb.Append(":");
            }
            var key = kb.ToString();

            // Check cache
            RWLock.EnterReadLock();
            try
            {
                object mapper;
                if (AutoMappers.TryGetValue(key, out mapper))
                    return mapper;
            }
            finally
            {
                RWLock.ExitReadLock();
            }

            // Create it
            RWLock.EnterWriteLock();
            try
            {
                // Try again
                object mapper;
                if (AutoMappers.TryGetValue(key, out mapper))
                    return mapper;

                // Create a method
                var m = new DynamicMethod("petapoco_automapper", types[0], types, true);
                var il = m.GetILGenerator();

                for (int i = 1; i < types.Length; i++)
                {
                    bool handled = false;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        // Find the property
                        var candidates = from p in types[j].GetProperties() where p.PropertyType == types[i] select p;
                        if (candidates.Count() == 0)
                            continue;
                        if (candidates.Count() > 1)
                            throw new InvalidOperationException(string.Format("Can't auto join {0} as {1} has more than one property of type {0}", types[i], types[j]));

                        // Generate code
                        il.Emit(OpCodes.Ldarg_S, j);
                        il.Emit(OpCodes.Ldarg_S, i);
                        il.Emit(OpCodes.Callvirt, candidates.First().GetSetMethod(true));
                        handled = true;
                    }

                    if (!handled)
                        throw new InvalidOperationException(string.Format("Can't auto join {0}", types[i]));
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);

                // Cache it
                var del = m.CreateDelegate(Expression.GetFuncType(types.Concat(types.Take(1)).ToArray()));
                AutoMappers.Add(key, del);
                return del;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }
        }

        // Find the split point in a result set for two different pocos and return the poco factory for the first
        Delegate FindSplitPoint(Type typeThis, Type typeNext, string sql, IDataReader r, ref int pos)
        {
            // Last?
            if (typeNext == null)
                return PocoData.ForType(typeThis).GetFactory(sql, Connection.ConnectionString, ForceDateTimesToUtc, pos, r.FieldCount - pos, r, null);

            // Get PocoData for the two types
            PocoData pdThis = PocoData.ForType(typeThis);
            PocoData pdNext = PocoData.ForType(typeNext);

            // Find split point
            int firstColumn = pos;
            var usedColumns = new Dictionary<string, bool>();
            for (; pos < r.FieldCount; pos++)
            {
                // Split if field name has already been used, or if the field doesn't exist in current poco but does in the next
                string fieldName = r.GetName(pos);
                if (usedColumns.ContainsKey(fieldName) || (!pdThis.Columns.ContainsKey(fieldName) && pdNext.Columns.ContainsKey(fieldName)))
                {
                    return pdThis.GetFactory(sql, Connection.ConnectionString, ForceDateTimesToUtc, firstColumn, pos - firstColumn, r, null);
                }
                usedColumns.Add(fieldName, true);
            }

            throw new InvalidOperationException(string.Format("Couldn't find split point between {0} and {1}", typeThis, typeNext));
        }

        // Instance data used by the Multipoco factory delegate - essentially a list of the nested poco factories to call
        class MultiPocoFactory
        {
            public List<Delegate> m_Delegates;
            public Delegate GetItem(int index) { return m_Delegates[index]; }
        }

        // Create a multi-poco factory
        Func<IDataReader, object, TRet> CreateMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
        {
            var m = new DynamicMethod("petapoco_multipoco_factory", typeof(TRet), new Type[] { typeof(MultiPocoFactory), typeof(IDataReader), typeof(object) }, typeof(MultiPocoFactory));
            var il = m.GetILGenerator();

            // Load the callback
            il.Emit(OpCodes.Ldarg_2);

            // Call each delegate
            var dels = new List<Delegate>();
            int pos = 0;
            for (int i = 0; i < types.Length; i++)
            {
                // Add to list of delegates to call
                var del = FindSplitPoint(types[i], i + 1 < types.Length ? types[i + 1] : null, sql, r, ref pos);
                dels.Add(del);

                // Get the delegate
                il.Emit(OpCodes.Ldarg_0);													// callback,this
                il.Emit(OpCodes.Ldc_I4, i);													// callback,this,Index
                il.Emit(OpCodes.Callvirt, typeof(MultiPocoFactory).GetMethod("GetItem"));	// callback,Delegate
                il.Emit(OpCodes.Ldarg_1);													// callback,delegate, datareader
                il.Emit(OpCodes.Ldnull);                                                    // callback,delegate, datareader,null

                // Call Invoke
                var tDelInvoke = del.GetType().GetMethod("Invoke");
                il.Emit(OpCodes.Callvirt, tDelInvoke);										// Poco left on stack
            }

            // By now we should have the callback and the N pocos all on the stack.  Call the callback and we're done
            il.Emit(OpCodes.Callvirt, Expression.GetFuncType(types.Concat(new Type[] { typeof(TRet) }).ToArray()).GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);

            // Finish up
            return (Func<IDataReader, object, TRet>)m.CreateDelegate(typeof(Func<IDataReader, object, TRet>), new MultiPocoFactory() { m_Delegates = dels });
        }

        // Various cached stuff
        static Dictionary<string, object> MultiPocoFactories = new Dictionary<string, object>();
        static Dictionary<string, object> AutoMappers = new Dictionary<string, object>();
        static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();

        // Get (or create) the multi-poco factory for a query
        Func<IDataReader, object, TRet> GetMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
        {
            // Build a key string  (this is crap, should address this at some point)
            var kb = new StringBuilder();
            kb.Append(typeof(TRet).ToString());
            kb.Append(":");
            foreach (var t in types)
            {
                kb.Append(":");
                kb.Append(t.ToString());
            }
            kb.Append(":"); kb.Append(Connection.ConnectionString);
            kb.Append(":"); kb.Append(ForceDateTimesToUtc);
            kb.Append(":"); kb.Append(sql);
            string key = kb.ToString();

            // Check cache
            RWLock.EnterReadLock();
            try
            {
                object oFactory;
                if (MultiPocoFactories.TryGetValue(key, out oFactory))
                    return (Func<IDataReader, object, TRet>)oFactory;
            }
            finally
            {
                RWLock.ExitReadLock();
            }

            // Cache it
            RWLock.EnterWriteLock();
            try
            {
                // Check again
                object oFactory;
                if (MultiPocoFactories.TryGetValue(key, out oFactory))
                    return (Func<IDataReader, object, TRet>)oFactory;

                // Create the factory
                var Factory = CreateMultiPocoFactory<TRet>(types, sql, r);

                MultiPocoFactories.Add(key, Factory);
                return Factory;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }

        }

        // Actual implementation of the multi-poco query
        public IEnumerable<TRet> Query<TRet>(Type[] types, object cb, Sql sql)
        {
            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(Connection, sql.SQL, sql.Arguments))
                {
                    IDataReader r;
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        OnException(x);
                        throw;
                    }
                    var factory = GetMultiPocoFactory<TRet>(types, sql.SQL, r);
                    if (cb == null)
                        cb = GetAutoMapper(types.ToArray());
                    bool bNeedTerminator = false;
                    using (r)
                    {
                        while (true)
                        {
                            TRet poco;
                            try
                            {
                                if (!r.Read())
                                    break;
                                poco = factory(r, cb);
                            }
                            catch (Exception x)
                            {
                                OnException(x);
                                throw;
                            }

                            if (poco != null)
                                yield return poco;
                            else
                                bNeedTerminator = true;
                        }
                        if (bNeedTerminator)
                        {
                            var poco = (TRet)(cb as Delegate).DynamicInvoke(new object[types.Length]);
                            if (poco != null)
                                yield return poco;
                            else
                                yield break;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }

        public TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, string sql, params object[] args) { return FetchMultiple<T1, T2, DontMap, DontMap, TRet>(new[] { typeof(T1), typeof(T2) }, cb, new Sql(sql, args)); }
        public TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, string sql, params object[] args) { return FetchMultiple<T1, T2, T3, DontMap, TRet>(new[] { typeof(T1), typeof(T2), typeof(T3) }, cb, new Sql(sql, args)); }
        public TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, string sql, params object[] args) { return FetchMultiple<T1, T2, T3, T4, TRet>(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, new Sql(sql, args)); }
        public TRet FetchMultiple<T1, T2, TRet>(Func<List<T1>, List<T2>, TRet> cb, Sql sql) { return FetchMultiple<T1, T2, DontMap, DontMap, TRet>(new[] { typeof(T1), typeof(T2) }, cb, sql); }
        public TRet FetchMultiple<T1, T2, T3, TRet>(Func<List<T1>, List<T2>, List<T3>, TRet> cb, Sql sql) { return FetchMultiple<T1, T2, T3, DontMap, TRet>(new[] { typeof(T1), typeof(T2), typeof(T3) }, cb, sql); }
        public TRet FetchMultiple<T1, T2, T3, T4, TRet>(Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet> cb, Sql sql) { return FetchMultiple<T1, T2, T3, T4, TRet>(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, sql); }

#if PETAPOCO_NO_DYNAMIC
        public Tuple<List<T1>, List<T2>> FetchMultiple<T1, T2>(string sql, params object[] args) { return FetchMultiple<T1, T2, DontMap, DontMap, Tuple<List<T1>, List<T2>>>(new[] { typeof(T1), typeof(T2) }, new Func<List<T1>, List<T2>, Tuple<List<T1>, List<T2>>>((y, z) => new Tuple<List<T1>, List<T2>>(y, z)), new Sql(sql, args)); }
        public Tuple<List<T1>, List<T2>, List<T3>> FetchMultiple<T1, T2, T3>(string sql, params object[] args) { return FetchMultiple<T1, T2, T3, DontMap, Tuple<List<T1>, List<T2>, List<T3>>>(new[] { typeof(T1), typeof(T2), typeof(T3) }, new Func<List<T1>, List<T2>, List<T3>, Tuple<List<T1>, List<T2>, List<T3>>>((x, y, z) => new Tuple<List<T1>, List<T2>, List<T3>>(x, y, z)), new Sql(sql, args)); }
        public Tuple<List<T1>, List<T2>, List<T3>, List<T4>> FetchMultiple<T1, T2, T3, T4>(string sql, params object[] args) { return FetchMultiple<T1, T2, T3, T4, Tuple<List<T1>, List<T2>, List<T3>, List<T4>>>(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, new Func<List<T1>, List<T2>, List<T3>, List<T4>, Tuple<List<T1>, List<T2>, List<T3>, List<T4>>>((w, x, y, z) => new Tuple<List<T1>, List<T2>, List<T3>, List<T4>>(w, x, y, z)), new Sql(sql, args)); }
        public Tuple<List<T1>, List<T2>> FetchMultiple<T1, T2>(Sql sql) { return FetchMultiple<T1, T2, DontMap, DontMap, Tuple<List<T1>, List<T2>>>(new[] { typeof(T1), typeof(T2) }, new Func<List<T1>, List<T2>, Tuple<List<T1>, List<T2>>>((y, z) => new Tuple<List<T1>, List<T2>>(y, z)), sql); }
        public Tuple<List<T1>, List<T2>, List<T3>> FetchMultiple<T1, T2, T3>(Sql sql) { return FetchMultiple<T1, T2, T3, DontMap, Tuple<List<T1>, List<T2>, List<T3>>>(new[] { typeof(T1), typeof(T2), typeof(T3) }, new Func<List<T1>, List<T2>, List<T3>, Tuple<List<T1>, List<T2>, List<T3>>>((x, y, z) => new Tuple<List<T1>, List<T2>, List<T3>>(x, y, z)), sql); }
        public Tuple<List<T1>, List<T2>, List<T3>, List<T4>> FetchMultiple<T1, T2, T3, T4>(Sql sql) { return FetchMultiple<T1, T2, T3, T4, Tuple<List<T1>, List<T2>, List<T3>, List<T4>>>(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, new Func<List<T1>, List<T2>, List<T3>, List<T4>, Tuple<List<T1>, List<T2>, List<T3>, List<T4>>>((w, x, y, z) => new Tuple<List<T1>, List<T2>, List<T3>, List<T4>>(w, x, y, z)), sql); }

        public class Tuple<T1, T2>
        {
            public T1 Item1 { get; set; }
            public T2 Item2 { get; set; }
            public Tuple(T1 item1, T2 item2) { Item1 = item1; Item2 = item2; }
        }

        public class Tuple<T1, T2, T3>
        {
            public T1 Item1 { get; set; }
            public T2 Item2 { get; set; }
            public T3 Item3 { get; set; }
            public Tuple(T1 item1, T2 item2, T3 item3) { Item1 = item1; Item2 = item2; Item3 = item3; }
        }

        public class Tuple<T1, T2, T3, T4>
        {
            public T1 Item1 { get; set; }
            public T2 Item2 { get; set; }
            public T3 Item3 { get; set; }
            public T4 Item4 { get; set; }
            public Tuple(T1 item1, T2 item2, T3 item3, T4 item4) { Item1 = item1; Item2 = item2; Item3 = item3; Item4 = item4; }
        }
#else
        public Tuple<List<T1>, List<T2>> FetchMultiple<T1, T2>(string sql, params object[] args) { return FetchMultiple<T1, T2, DontMap, DontMap, Tuple<List<T1>, List<T2>>>(new[] { typeof(T1), typeof(T2) }, new Func<List<T1>, List<T2>, Tuple<List<T1>, List<T2>>>((y, z) => new Tuple<List<T1>, List<T2>>(y, z)), new Sql(sql, args)); }
        public Tuple<List<T1>, List<T2>, List<T3>> FetchMultiple<T1, T2, T3>(string sql, params object[] args) { return FetchMultiple<T1, T2, T3, DontMap, Tuple<List<T1>, List<T2>, List<T3>>>(new[] { typeof(T1), typeof(T2), typeof(T3) }, new Func<List<T1>, List<T2>, List<T3>, Tuple<List<T1>, List<T2>, List<T3>>>((x, y, z) => new Tuple<List<T1>, List<T2>, List<T3>>(x, y, z)), new Sql(sql, args)); }
        public Tuple<List<T1>, List<T2>, List<T3>, List<T4>> FetchMultiple<T1, T2, T3, T4>(string sql, params object[] args) { return FetchMultiple<T1, T2, T3, T4, Tuple<List<T1>, List<T2>, List<T3>, List<T4>>>(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, new Func<List<T1>, List<T2>, List<T3>, List<T4>, Tuple<List<T1>, List<T2>, List<T3>, List<T4>>>((w, x, y, z) => new Tuple<List<T1>, List<T2>, List<T3>, List<T4>>(w, x, y, z)), new Sql(sql, args)); }
        public Tuple<List<T1>, List<T2>> FetchMultiple<T1, T2>(Sql sql) { return FetchMultiple<T1, T2, DontMap, DontMap, Tuple<List<T1>, List<T2>>>(new[] { typeof(T1), typeof(T2) }, new Func<List<T1>, List<T2>, Tuple<List<T1>, List<T2>>>((y, z) => new Tuple<List<T1>, List<T2>>(y, z)), sql); }
        public Tuple<List<T1>, List<T2>, List<T3>> FetchMultiple<T1, T2, T3>(Sql sql) { return FetchMultiple<T1, T2, T3, DontMap, Tuple<List<T1>, List<T2>, List<T3>>>(new[] { typeof(T1), typeof(T2), typeof(T3) }, new Func<List<T1>, List<T2>, List<T3>, Tuple<List<T1>, List<T2>, List<T3>>>((x, y, z) => new Tuple<List<T1>, List<T2>, List<T3>>(x, y, z)), sql); }
        public Tuple<List<T1>, List<T2>, List<T3>, List<T4>> FetchMultiple<T1, T2, T3, T4>(Sql sql) { return FetchMultiple<T1, T2, T3, T4, Tuple<List<T1>, List<T2>, List<T3>, List<T4>>>(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, new Func<List<T1>, List<T2>, List<T3>, List<T4>, Tuple<List<T1>, List<T2>, List<T3>, List<T4>>>((w, x, y, z) => new Tuple<List<T1>, List<T2>, List<T3>, List<T4>>(w, x, y, z)), sql); }
#endif

        public class DontMap { }

        // Actual implementation of the multi query
        private TRet FetchMultiple<T1, T2, T3, T4, TRet>(Type[] types, object cb, Sql Sql)
        {
            var sql = Sql.SQL;
            var args = Sql.Arguments;

            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(Connection, sql, args))
                {
                    IDataReader r;
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        OnException(x);
                        throw;
                    }

                    using (r)
                    {
                        var typeIndex = 1;
                        var list1 = new List<T1>();
                        var list2 = new List<T2>();
                        var list3 = new List<T3>();
                        var list4 = new List<T4>();
                        do
                        {
                            if (typeIndex > types.Length)
                                break;

                            var pd = PocoData.ForType(types[typeIndex - 1]);
                            var factory = pd.GetFactory(cmd.CommandText, Connection.ConnectionString, ForceDateTimesToUtc, 0, r.FieldCount, r, null);

                            while (true)
                            {
                                try
                                {
                                    if (!r.Read())
                                        break;

                                    switch (typeIndex)
                                    {
                                        case 1:
                                            list1.Add(((Func<IDataReader, T1, T1>)factory)(r, default(T1)));
                                            break;
                                        case 2:
                                            list2.Add(((Func<IDataReader, T2, T2>)factory)(r, default(T2)));
                                            break;
                                        case 3:
                                            list3.Add(((Func<IDataReader, T3, T3>)factory)(r, default(T3)));
                                            break;
                                        case 4:
                                            list4.Add(((Func<IDataReader, T4, T4>)factory)(r, default(T4)));
                                            break;
                                    }
                                }
                                catch (Exception x)
                                {
                                    OnException(x);
                                    throw;
                                }
                            }

                            typeIndex++;
                        } while (r.NextResult());

                        switch (types.Length)
                        {
                            case 2:
                                return ((Func<List<T1>, List<T2>, TRet>)cb)(list1, list2);
                            case 3:
                                return ((Func<List<T1>, List<T2>, List<T3>, TRet>)cb)(list1, list2, list3);
                            case 4:
                                return ((Func<List<T1>, List<T2>, List<T3>, List<T4>, TRet>)cb)(list1, list2, list3, list4);
                        }

                        return default(TRet);
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }

        public bool Exists<T>(object primaryKey)
        {
            var index = 0;
            var primaryKeyValuePairs = GetPrimaryKeyValues(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey, primaryKey);
            return FirstOrDefault<T>(string.Format("WHERE {0}", BuildPrimaryKeySql(primaryKeyValuePairs, ref index)), primaryKeyValuePairs.Select(x => x.Value).ToArray()) != null;
        }
        public T SingleById<T>(object primaryKey)
        {
            var index = 0;
            var primaryKeyValuePairs = GetPrimaryKeyValues(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey, primaryKey);
            return Single<T>(string.Format("WHERE {0}", BuildPrimaryKeySql(primaryKeyValuePairs, ref index)), primaryKeyValuePairs.Select(x => x.Value).ToArray());
        }
        public T SingleOrDefaultById<T>(object primaryKey)
        {
            var index = 0;
            var primaryKeyValuePairs = GetPrimaryKeyValues(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey, primaryKey);
            return SingleOrDefault<T>(string.Format("WHERE {0}", BuildPrimaryKeySql(primaryKeyValuePairs, ref index)), primaryKeyValuePairs.Select(x => x.Value).ToArray());
        }
        public T Single<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).Single();
        }
        public T SingleInto<T>(T instance, string sql, params object[] args)
        {
            return Query<T>(instance, new Sql(sql, args)).Single();
        }
        public T SingleOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).SingleOrDefault();
        }
        public T SingleOrDefaultInto<T>(T instance, string sql, params object[] args)
        {
            return Query<T>(instance, new Sql(sql, args)).SingleOrDefault();
        }
        public T First<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).First();
        }
        public T FirstInto<T>(T instance, string sql, params object[] args)
        {
            return Query<T>(instance, new Sql(sql, args)).First();
        }
        public T FirstOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).FirstOrDefault();
        }
        public T FirstOrDefaultInto<T>(T instance, string sql, params object[] args)
        {
            return Query<T>(instance, new Sql(sql, args)).FirstOrDefault();
        }
        public T Single<T>(Sql sql)
        {
            return Query<T>(sql).Single();
        }
        public T SingleInto<T>(T instance, Sql sql)
        {
            return Query<T>(instance, sql).Single();
        }
        public T SingleOrDefault<T>(Sql sql)
        {
            return Query<T>(sql).SingleOrDefault();
        }
        public T SingleOrDefaultInto<T>(T instance, Sql sql)
        {
            return Query<T>(instance, sql).SingleOrDefault();
        }
        public T First<T>(Sql sql)
        {
            return Query<T>(sql).First();
        }
        public T FirstInto<T>(T instance, Sql sql)
        {
            return Query<T>(instance, sql).First();
        }
        public T FirstOrDefault<T>(Sql sql)
        {
            return Query<T>(sql).FirstOrDefault();
        }
        public T FirstOrDefaultInto<T>(T instance, Sql sql)
        {
            return Query<T>(instance, sql).FirstOrDefault();
        }
        public string EscapeTableName(string str)
        {
            // Assume table names with "dot" are already escaped
            return str.IndexOf('.') >= 0 ? str : EscapeSqlIdentifier(str);
        }

        public string EscapeSqlIdentifier(string str)
        {
            return string.Format("[{0}]", str);
        }

        public object Insert(string tableName, string primaryKeyName, object poco)
        {
            return Insert(tableName, primaryKeyName, true, poco);
        }

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
        public object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    // --- Prepare columns for insertion
                    var pd = PocoData.ForObject(poco, primaryKeyName);
                    var names = new List<string>();
                    var values = new List<string>();
                    var rawvalues = new List<object>();
                    var index = 0;
                    var versionName = "";
                    var keys = primaryKeyName.Split(',').Select(i => i.Trim()).ToArray();

                    foreach (var column in pd.Columns)
                    {
                        // --- Don't insert result columns and version columns
                        if (column.Value.ResultColumn || column.Value.VersionColumn)
                            continue;

                        object val = column.Value.GetValue(poco);

                        // --- Manage DateTime issues with SQL Server
                        if (val is DateTime)
                        {
                            var dateTimeValue = (DateTime) val;
                            if (dateTimeValue.Year <= 1754)
                            {
                                val = new DateTime(1754, 1, 1);
                            }
                        }

                        // --- Don't insert the primary key (except under oracle where we need bring in the next sequence value)
                        if (autoIncrement && primaryKeyName != null && string.Compare(column.Key, primaryKeyName, true) == 0)
                        {
                            continue;
                        }

                        names.Add(EscapeSqlIdentifier(column.Key));
                        values.Add(string.Format("{0}{1}", PARAM_PREFIX, index++));

                        if (column.Value.VersionColumn)
                        {
                            val = 1;
                            versionName = column.Key;
                        }
                        rawvalues.Add(val);
                    }

                    // --- Create and excute the insert command
                    using (var cmd = CreateCommand(Connection, ""))
                    {
                        var sql = names.Count > 0 
                            ? string.Format("INSERT INTO {0} ({1}) VALUES ({2})", EscapeTableName(tableName), 
                                string.Join(",", names.ToArray()), string.Join(",", values.ToArray())) 
                            : string.Format("INSERT INTO {0} DEFAULT VALUES", EscapeTableName(tableName));

                        cmd.CommandText = sql;
                        rawvalues.ForEach(x => AddParam(cmd, x, PARAM_PREFIX));
                        object id;
                        if (!autoIncrement)
                        {
                            DoPreExecute(cmd);
                            cmd.ExecuteNonQuery();
                            OnExecutedCommand(cmd);
                            id = -1;
                        }
                        else
                        {
                            cmd.CommandText += ";\nSELECT SCOPE_IDENTITY() AS NewID;";
                            DoPreExecute(cmd);
                            id = cmd.ExecuteScalar();
                            OnExecutedCommand(cmd);

                            // --- Assign the ID back to the primary key property
                            if (primaryKeyName != null)
                            {
                                PocoColumn pc;
                                if (pd.Columns.TryGetValue(primaryKeyName, out pc))
                                {
                                    pc.SetValue(poco, pc.ChangeType(id));
                                }
                            }
                        }

                        // --- Assign the Version column
                        if (!string.IsNullOrEmpty(versionName))
                        {
                            PocoColumn pc;
                            if (pd.Columns.TryGetValue(versionName, out pc))
                            {
                                pc.SetValue(poco, pc.ChangeType(1));
                            }
                        }
                        return id;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        // Insert an annotated poco object
        public object Insert(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Insert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, pd.TableInfo.AutoIncrement, poco);
        }

        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            return Update(tableName, primaryKeyName, poco, primaryKeyValue, null);
        }

        // Update a record with values from a poco.  primary key value can be either supplied or read from the poco
        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columnsEnum)
        {
            var columns = columnsEnum == null ? new string[0] : columnsEnum.ToArray();
            if (columnsEnum != null && !columns.Any()) return 0;

            var sb = new StringBuilder();
            var index = 0;
            var rawvalues = new List<object>();
            var pd = PocoData.ForObject(poco, primaryKeyName);
            string versionName = null;
            object versionValue = null;

            var primaryKeyValuePairs = GetPrimaryKeyValues(primaryKeyName, primaryKeyValue);

            foreach (var column in pd.Columns)
            {
                var value = column.Value.GetValue(poco);

                // --- Manage DateTime issues with SQL Server
                if (value is DateTime)
                {
                    var dateTimeValue = (DateTime) value;
                    if (dateTimeValue.Year <= 1754)
                    {
                        value = new DateTime(1754, 1, 1);
                    }
                }

                // Don't update the primary key, but grab the value if we don't have it
                if (primaryKeyValue == null && primaryKeyValuePairs.ContainsKey(column.Key))
                {
                    primaryKeyValuePairs[column.Key] = column.Value.GetValue(poco);
                    continue;
                }

                // --- Store version column info
                if (column.Value.VersionColumn)
                {
                    versionName = column.Key;
                    versionValue = value;
                }

                // -- Don't update result only columns and version columns
                if (column.Value.ResultColumn || column.Value.VersionColumn)
                    continue;

                if (!column.Value.VersionColumn && columnsEnum != null && !columns.Contains(column.Value.ColumnName, StringComparer.OrdinalIgnoreCase))
                    continue;

                // Build the sql
                if (index > 0)
                    sb.Append(", ");
                sb.AppendFormat("{0} = @{1}", EscapeSqlIdentifier(column.Key), index++);

                rawvalues.Add(value);
            }

            if (columns != null && columns.Any() && sb.Length == 0)
                throw new ArgumentException("There were no columns in the columns list that matched your table", "columns");

            var sql = string.Format("UPDATE {0} SET {1} WHERE {2}", EscapeTableName(tableName), sb, BuildPrimaryKeySql(primaryKeyValuePairs, ref index));

            rawvalues.AddRange(primaryKeyValuePairs.Select(keyValue => keyValue.Value));

            if (!string.IsNullOrEmpty(versionName))
            {
                sql += string.Format(" AND {0} = @{1}", EscapeSqlIdentifier(versionName), index++);
                rawvalues.Add(versionValue);
            }

            var result = Execute(sql, rawvalues.ToArray());

            if (result == 0 && !string.IsNullOrEmpty(versionName) && VersionException == VersionExceptionHandling.Exception)
            {
                throw new DBConcurrencyException(string.Format("A Concurrency update occurred in table '{0}' for primary key value(s) = '{1}' and version = '{2}'",
                    tableName,
                    string.Join(",", primaryKeyValuePairs.Values.Select(x => x.ToString()).ToArray()),
                    TypeConversionHelper.ByteArrayToString((byte[])versionValue)));
            }

            return result;
        }

        private string BuildPrimaryKeySql(Dictionary<string, object> primaryKeyValuePair, ref int index)
        {
            var tempIndex = index;
            index += primaryKeyValuePair.Count;
            return string.Join(" AND ", primaryKeyValuePair.Select((x, i) => string.Format("{0} = @{1}", EscapeSqlIdentifier(x.Key), tempIndex + i)).ToArray());
        }

        private Dictionary<string, object> GetPrimaryKeyValues(string primaryKeyName, object primaryKeyValue)
        {
            Dictionary<string, object> primaryKeyValues;

            var multiplePrimaryKeysNames = primaryKeyName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (primaryKeyValue != null)
            {
                if (multiplePrimaryKeysNames.Length == 1)
                    primaryKeyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { primaryKeyName, primaryKeyValue } };
                else
                    primaryKeyValues = multiplePrimaryKeysNames.ToDictionary(x => x,
                                                                             x => primaryKeyValue.GetType().GetProperties()
                                                                                      .Where(y => string.Equals(x, y.Name, StringComparison.OrdinalIgnoreCase))
                                                                                      .Single().GetValue(primaryKeyValue, null), StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                primaryKeyValues = multiplePrimaryKeysNames.ToDictionary(x => x, x => (object)null, StringComparer.OrdinalIgnoreCase);
            }
            return primaryKeyValues;
        }

        public int Update(string tableName, string primaryKeyName, object poco)
        {
            return Update(tableName, primaryKeyName, poco, null);
        }

        public int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string> columns)
        {
            return Update(tableName, primaryKeyName, poco, null, columns);
        }

        public int Update(object poco, IEnumerable<string> columns)
        {
            return Update(poco, null, columns);
        }

        public int Update(object poco)
        {
            return Update(poco, null, null);
        }

        public int Update(object poco, object primaryKeyValue)
        {
            return Update(poco, primaryKeyValue, null);
        }

        public int Update(object poco, object primaryKeyValue, IEnumerable<string> columns)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Update(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco, primaryKeyValue, columns);
        }

        public int Update<T>(string sql, params object[] args)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(string.Format("UPDATE {0} {1}", EscapeTableName(pd.TableInfo.TableName), sql), args);
        }

        public int Update<T>(Sql sql)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(new Sql(string.Format("UPDATE {0}", EscapeTableName(pd.TableInfo.TableName))).Append(sql));
        }

        public int Delete(string tableName, string primaryKeyName, object poco)
        {
            return Delete(tableName, primaryKeyName, poco, null);
        }

        public int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            var primaryKeyValuePairs = GetPrimaryKeyValues(primaryKeyName, primaryKeyValue);
            // --- If primary key value not specified, pick it up from the object
            if (primaryKeyValue == null)
            {
                var pd = PocoData.ForObject(poco, primaryKeyName);
                foreach (var i in pd.Columns)
                {
                    var value = i.Value.GetValue(poco);
                    if (!primaryKeyValuePairs.ContainsKey(i.Key)) continue;
                    primaryKeyValuePairs[i.Key] = i.Value.GetValue(poco);
                }
            }

            // --- Do it
            var index = 0;
            var sql = string.Format("DELETE FROM {0} WHERE {1}", EscapeTableName(tableName), BuildPrimaryKeySql(primaryKeyValuePairs, ref index));
            var rows = Execute(sql, primaryKeyValuePairs.Select(x => x.Value).ToArray());
            return rows;
        }

        public int Delete(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        }

        public int Delete<T>(object pocoOrPrimaryKey)
        {
            if (pocoOrPrimaryKey.GetType() == typeof(T))
                return Delete(pocoOrPrimaryKey);
            var pd = PocoData.ForType(typeof(T));
            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, null, pocoOrPrimaryKey);
        }

        public int Delete<T>(string sql, params object[] args)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(string.Format("DELETE FROM {0} {1}", EscapeTableName(pd.TableInfo.TableName), sql), args);
        }

        public int Delete<T>(Sql sql)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(new Sql(string.Format("DELETE FROM {0}", EscapeTableName(pd.TableInfo.TableName))).Append(sql));
        }

        // Check if a poco represents a new record
        public bool IsNew(string primaryKeyName, object poco)
        {
            var pd = PocoData.ForObject(poco, primaryKeyName);
            object pk;
            PocoColumn pc;
            if (pd.Columns.TryGetValue(primaryKeyName, out pc))
            {
                pk = pc.GetValue(poco);
            }
#if !PETAPOCO_NO_DYNAMIC
            else if (poco.GetType() == typeof(System.Dynamic.ExpandoObject))
            {
                return true;
            }
#endif
            else
            {
                var pi = poco.GetType().GetProperty(primaryKeyName);
                if (pi == null)
                    throw new ArgumentException(string.Format("The object doesn't have a property matching the primary key column name '{0}'", primaryKeyName));
                pk = pi.GetValue(poco, null);
            }

            if (pk == null)
                return true;

            var type = pk.GetType();

            if (type.IsValueType)
            {
                // Common primary key types
                if (type == typeof(long))
                    return (long)pk == default(long);
                else if (type == typeof(ulong))
                    return (ulong)pk == default(ulong);
                else if (type == typeof(int))
                    return (int)pk == default(int);
                else if (type == typeof(uint))
                    return (uint)pk == default(uint);
                else if (type == typeof(Guid))
                    return (Guid)pk == default(Guid);

                // Create a default instance and compare
                return pk == Activator.CreateInstance(pk.GetType());
            }
            else
            {
                return pk == null;
            }
        }

        public bool IsNew(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            if (!pd.TableInfo.AutoIncrement)
                throw new InvalidOperationException("IsNew() and Save() are only supported on tables with auto-increment/identity primary key columns");
            return IsNew(pd.TableInfo.PrimaryKey, poco);
        }

        // Insert new record or Update existing record
        public void Save(string tableName, string primaryKeyName, object poco)
        {
            if (IsNew(primaryKeyName, poco))
            {
                Insert(tableName, primaryKeyName, true, poco);
            }
            else
            {
                Update(tableName, primaryKeyName, poco);
            }
        }

        public void Save(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            Save(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        }

        public int CommandTimeout { get; set; }
        public int OneTimeCommandTimeout { get; set; }

        void DoPreExecute(SqlCommand cmd)
        {
            // Setup command timeout
            if (OneTimeCommandTimeout != 0)
            {
                cmd.CommandTimeout = OneTimeCommandTimeout;
                OneTimeCommandTimeout = 0;
            }
            else if (CommandTimeout != 0)
            {
                cmd.CommandTimeout = CommandTimeout;
            }

            // Call hook
            OnExecutingCommand(cmd);

            // Save it
            _lastSql = cmd.CommandText;
            _lastArgs = (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray();
        }

        public string LastSQL { get { return _lastSql; } }
        public object[] LastArgs { get { return _lastArgs; } }
        public string LastCommand
        {
            get { return FormatCommand(_lastSql, _lastArgs); }
        }

        public string FormatCommand(IDbCommand cmd)
        {
            return FormatCommand(cmd.CommandText, (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray());
        }

        public string FormatCommand(string sql, object[] args)
        {
            var sb = new StringBuilder();
            if (sql == null)
                return "";
            sb.Append(sql);
            if (args != null && args.Length > 0)
            {
                sb.Append("\n");
                for (int i = 0; i < args.Length; i++)
                {
                    sb.AppendFormat("\t -> {0}{1} [{2}] = \"{3}\"\n", PARAM_PREFIX, i, args[i].GetType().Name, args[i]);
                }
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        /// <summary>
        /// This enum specifies the mode of version exception handling
        /// </summary>
        public enum VersionExceptionHandling
        {
            /// <summary>Ignore exception</summary>
            Ignore,

            /// <summary>Handle exception</summary>
            Exception
        }

        public static IMapper DbObjectMapper { get; set; }

        public class PocoColumn
        {
            public string ColumnName;
            public PropertyInfo PropertyInfo;
            public bool ResultColumn;
            public bool VersionColumn;
            public virtual void SetValue(object target, object val) { PropertyInfo.SetValue(target, val, null); }
            public virtual object GetValue(object target) { return PropertyInfo.GetValue(target, null); }
            public virtual object ChangeType(object val) { return Convert.ChangeType(val, PropertyInfo.PropertyType); }
        }
        public class ExpandoColumn : PocoColumn
        {
            public override void SetValue(object target, object val) { ((IDictionary<string, object>)target)[ColumnName] = val; }
            public override object GetValue(object target)
            {
                object val = null;
                ((IDictionary<string, object>)target).TryGetValue(ColumnName, out val);
                return val;
            }
            public override object ChangeType(object val) { return val; }
        }

        public static Func<Type, PocoData> PocoDataFactory = type => new PocoData(type);
        public class PocoData
        {
            static readonly EnumMapper EnumMapper = new EnumMapper();

            public static PocoData ForObject(object o, string primaryKeyName)
            {
                var t = o.GetType();
#if !PETAPOCO_NO_DYNAMIC
                if (t == typeof(System.Dynamic.ExpandoObject))
                {
                    var pd = new PocoData();
                    pd.TableInfo = new TableInfo();
                    pd.Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
                    pd.Columns.Add(primaryKeyName, new ExpandoColumn() { ColumnName = primaryKeyName });
                    pd.TableInfo.PrimaryKey = primaryKeyName;
                    pd.TableInfo.AutoIncrement = true;
                    foreach (var col in ((IDictionary<string, object>)o).Keys)
                    {
                        if (col != primaryKeyName)
                            pd.Columns.Add(col, new ExpandoColumn() { ColumnName = col });
                    }
                    return pd;
                }
                else
#endif
                    return ForType(t);
            }
            static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();
            public static PocoData ForType(Type t)
            {
#if !PETAPOCO_NO_DYNAMIC
                if (t == typeof(System.Dynamic.ExpandoObject))
                    throw new InvalidOperationException("Can't use dynamic types with this method");
#endif
                // Check cache
                RWLock.EnterReadLock();
                PocoData pd;
                try
                {
                    if (m_PocoDatas.TryGetValue(t, out pd))
                        return pd;
                }
                finally
                {
                    RWLock.ExitReadLock();
                }


                // Cache it
                RWLock.EnterWriteLock();
                try
                {
                    // Check again
                    if (m_PocoDatas.TryGetValue(t, out pd))
                        return pd;

                    // Create it
                    pd = PocoDataFactory(t);
                    m_PocoDatas.Add(t, pd);
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }

                return pd;
            }

            public PocoData()
            {
            }

            public PocoData(Type t)
            {
                type = t;
                TableInfo = new TableInfo();

                // Get the table name
                var a = t.GetCustomAttributes(typeof(TableNameAttribute), true);
                TableInfo.TableName = a.Length == 0 ? t.Name : (a[0] as TableNameAttribute).Value;

                // Get the primary key
                a = t.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
                TableInfo.PrimaryKey = a.Length == 0 ? "ID" : (a[0] as PrimaryKeyAttribute).Value;
                TableInfo.SequenceName = a.Length == 0 ? null : (a[0] as PrimaryKeyAttribute).SequenceName;
                TableInfo.AutoIncrement = a.Length == 0 ? true : (a[0] as PrimaryKeyAttribute).AutoIncrement;

                // Set autoincrement false if primary key has multiple columns
                TableInfo.AutoIncrement = TableInfo.AutoIncrement ? !TableInfo.PrimaryKey.Contains(',') : TableInfo.AutoIncrement;

                // Call column mapper
                if (Database.DbObjectMapper != null)
                    Database.DbObjectMapper.GetTableInfo(t, TableInfo);

                // Work out bound properties
                bool ExplicitColumns = t.GetCustomAttributes(typeof(ExplicitColumnsAttribute), true).Length > 0;
                Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
                foreach (var pi in t.GetProperties())
                {
                    // Work out if properties is to be included
                    var ColAttrs = pi.GetCustomAttributes(typeof(ColumnAttribute), true);
                    if (ExplicitColumns)
                    {
                        if (ColAttrs.Length == 0)
                            continue;
                    }
                    else
                    {
                        if (pi.GetCustomAttributes(typeof(IgnoreColumnAttribute), true).Length != 0)
                            continue;
                    }

                    var pc = new PocoColumn();
                    pc.PropertyInfo = pi;

                    // Work out the DB column name
                    if (ColAttrs.Length > 0)
                    {
                        var colattr = (ColumnAttribute)ColAttrs[0];
                        pc.ColumnName = colattr.Name;
                        if ((colattr as ResultColumnAttribute) != null)
                            pc.ResultColumn = true;
                        if ((colattr as VersionColumnAttribute) != null)
                            pc.VersionColumn = true;
                    }
                    if (pc.ColumnName == null)
                    {
                        pc.ColumnName = pi.Name;
                        if (Database.DbObjectMapper != null && !Database.DbObjectMapper.MapPropertyToColumn(pi, ref pc.ColumnName, ref pc.ResultColumn))
                            continue;
                    }

                    // Store it
                    Columns.Add(pc.ColumnName, pc);
                }

                // Build column list for automatic select
                QueryColumns = (from c in Columns where !c.Value.ResultColumn select c.Key).ToArray();

            }

            static bool IsIntegralType(Type t)
            {
                var tc = Type.GetTypeCode(t);
                return tc >= TypeCode.SByte && tc <= TypeCode.UInt64;
            }

            static object GetDefault(Type type)
            {
                if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }
                return null;
            }

            // Create factory function that can convert a IDataReader record into a POCO
            public Delegate GetFactory(string sql, string connString, bool ForceDateTimesToUtc, int firstColumn, int countColumns, IDataReader r, object instance)
            {
                // Check cache
                var key = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", sql, connString, ForceDateTimesToUtc, firstColumn, countColumns, instance != GetDefault(type));
                RWLock.EnterReadLock();
                try
                {
                    // Have we already created it?
                    Delegate factory;
                    if (PocoFactories.TryGetValue(key, out factory))
                        return factory;
                }
                finally
                {
                    RWLock.ExitReadLock();
                }

                // Take the writer lock
                RWLock.EnterWriteLock();

                try
                {
                    // Check again, just in case
                    Delegate factory;
                    if (PocoFactories.TryGetValue(key, out factory))
                        return factory;

                    // Create the method
                    var m = new DynamicMethod("petapoco_factory_" + PocoFactories.Count.ToString(), type, new Type[] { typeof(IDataReader), type }, true);
                    var il = m.GetILGenerator();

#if !PETAPOCO_NO_DYNAMIC
                    if (type == typeof(object))
                    {
                        // var poco=new T()
                        il.Emit(OpCodes.Newobj, typeof(System.Dynamic.ExpandoObject).GetConstructor(Type.EmptyTypes));			// obj

                        MethodInfo fnAdd = typeof(IDictionary<string, object>).GetMethod("Add");

                        // Enumerate all fields generating a set assignment for the column
                        for (int i = firstColumn; i < firstColumn + countColumns; i++)
                        {
                            var srcType = r.GetFieldType(i);

                            il.Emit(OpCodes.Dup);						// obj, obj
                            il.Emit(OpCodes.Ldstr, r.GetName(i));		// obj, obj, fieldname

                            // Get the converter
                            Func<object, object> converter = null;
                            if (Database.DbObjectMapper != null)
                                converter = Database.DbObjectMapper.GetFromDbConverter(null, srcType);
                            if (ForceDateTimesToUtc && converter == null && srcType == typeof(DateTime))
                                converter = delegate(object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };

                            // Setup stack for call to converter
                            AddConverterToStack(il, converter);

                            // r[i]
                            il.Emit(OpCodes.Ldarg_0);					// obj, obj, fieldname, converter?,    rdr
                            il.Emit(OpCodes.Ldc_I4, i);					// obj, obj, fieldname, converter?,  rdr,i
                            il.Emit(OpCodes.Callvirt, fnGetValue);		// obj, obj, fieldname, converter?,  value

                            // Convert DBNull to null
                            il.Emit(OpCodes.Dup);						// obj, obj, fieldname, converter?,  value, value
                            il.Emit(OpCodes.Isinst, typeof(DBNull));	// obj, obj, fieldname, converter?,  value, (value or null)
                            var lblNotNull = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse_S, lblNotNull);		// obj, obj, fieldname, converter?,  value
                            il.Emit(OpCodes.Pop);						// obj, obj, fieldname, converter?
                            if (converter != null)
                                il.Emit(OpCodes.Pop);					// obj, obj, fieldname, 
                            il.Emit(OpCodes.Ldnull);					// obj, obj, fieldname, null
                            if (converter != null)
                            {
                                var lblReady = il.DefineLabel();
                                il.Emit(OpCodes.Br_S, lblReady);
                                il.MarkLabel(lblNotNull);
                                il.Emit(OpCodes.Callvirt, fnInvoke);
                                il.MarkLabel(lblReady);
                            }
                            else
                            {
                                il.MarkLabel(lblNotNull);
                            }

                            il.Emit(OpCodes.Callvirt, fnAdd);
                        }
                    }
                    else
#endif
                        if (type.IsValueType || type == typeof(string) || type == typeof(byte[]))
                        {
                            // Do we need to install a converter?
                            var srcType = r.GetFieldType(0);
                            var converter = GetConverter(ForceDateTimesToUtc, null, srcType, type);

                            // "if (!rdr.IsDBNull(i))"
                            il.Emit(OpCodes.Ldarg_0);										// rdr
                            il.Emit(OpCodes.Ldc_I4_0);										// rdr,0
                            il.Emit(OpCodes.Callvirt, fnIsDBNull);							// bool
                            var lblCont = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse_S, lblCont);
                            il.Emit(OpCodes.Ldnull);										// null
                            var lblFin = il.DefineLabel();
                            il.Emit(OpCodes.Br_S, lblFin);

                            il.MarkLabel(lblCont);

                            // Setup stack for call to converter
                            AddConverterToStack(il, converter);

                            il.Emit(OpCodes.Ldarg_0);										// rdr
                            il.Emit(OpCodes.Ldc_I4_0);										// rdr,0
                            il.Emit(OpCodes.Callvirt, fnGetValue);							// value

                            // Call the converter
                            if (converter != null)
                                il.Emit(OpCodes.Callvirt, fnInvoke);

                            il.MarkLabel(lblFin);
                            il.Emit(OpCodes.Unbox_Any, type);								// value converted
                        }
                        else if (type == typeof(Dictionary<string, object>))
                        {
                            Func<IDataReader, object, Dictionary<string, object>> func = (reader, inst) =>
                            {
                                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                                for (int i = firstColumn; i < firstColumn + countColumns; i++)
                                {
                                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    var name = reader.GetName(i);
                                    if (!dict.ContainsKey(name))
                                        dict.Add(name, value);
                                }
                                return dict;
                            };

                            var delegateType = typeof(Func<,,>).MakeGenericType(typeof(IDataReader), type, typeof(Dictionary<string, object>));
                            var localDel = Delegate.CreateDelegate(delegateType, func.Target, func.Method);
                            PocoFactories.Add(key, localDel);
                            return localDel;
                        }
                        else if (type == typeof(object[]))
                        {
                            Func<IDataReader, object, object[]> func = (reader, inst) =>
                            {
                                var obj = new object[countColumns - firstColumn];
                                for (int i = firstColumn; i < firstColumn + countColumns; i++)
                                {
                                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    obj[i - firstColumn] = value;
                                }
                                return obj;
                            };

                            var delegateType = typeof(Func<,,>).MakeGenericType(typeof(IDataReader), type, typeof(object[]));
                            var localDel = Delegate.CreateDelegate(delegateType, func.Target, func.Method);
                            PocoFactories.Add(key, localDel);
                            return localDel;
                        }
                        else
                        {
                            if (instance != null)
                                il.Emit(OpCodes.Ldarg_1);
                            else
                                // var poco=new T()
                                il.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));

                            // Enumerate all fields generating a set assignment for the column
                            for (int i = firstColumn; i < firstColumn + countColumns; i++)
                            {
                                // Get the PocoColumn for this db column, ignore if not known
                                PocoColumn pc;
                                if (!Columns.TryGetValue(r.GetName(i), out pc) && !Columns.TryGetValue(r.GetName(i).Replace("_", ""), out pc))
                                {
                                    continue;
                                }

                                // Get the source type for this column
                                var srcType = r.GetFieldType(i);
                                var dstType = pc.PropertyInfo.PropertyType;

                                // "if (!rdr.IsDBNull(i))"
                                il.Emit(OpCodes.Ldarg_0);										// poco,rdr
                                il.Emit(OpCodes.Ldc_I4, i);										// poco,rdr,i
                                il.Emit(OpCodes.Callvirt, fnIsDBNull);							// poco,bool
                                var lblNext = il.DefineLabel();
                                il.Emit(OpCodes.Brtrue_S, lblNext);								// poco

                                il.Emit(OpCodes.Dup);											// poco,poco

                                // Do we need to install a converter?
                                var converter = GetConverter(ForceDateTimesToUtc, pc, srcType, dstType);

                                // Fast
                                bool Handled = false;
                                if (converter == null)
                                {
                                    var valuegetter = typeof(IDataRecord).GetMethod("Get" + srcType.Name, new Type[] { typeof(int) });
                                    if (valuegetter != null
                                        && valuegetter.ReturnType == srcType
                                        && (valuegetter.ReturnType == dstType || valuegetter.ReturnType == Nullable.GetUnderlyingType(dstType)))
                                    {
                                        il.Emit(OpCodes.Ldarg_0);										// *,rdr
                                        il.Emit(OpCodes.Ldc_I4, i);										// *,rdr,i
                                        il.Emit(OpCodes.Callvirt, valuegetter);							// *,value

                                        // Convert to Nullable
                                        if (Nullable.GetUnderlyingType(dstType) != null)
                                        {
                                            il.Emit(OpCodes.Newobj, dstType.GetConstructor(new Type[] { Nullable.GetUnderlyingType(dstType) }));
                                        }

                                        il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true));		// poco
                                        Handled = true;
                                    }
                                }

                                // Not so fast
                                if (!Handled)
                                {
                                    // Setup stack for call to converter
                                    AddConverterToStack(il, converter);

                                    // "value = rdr.GetValue(i)"
                                    il.Emit(OpCodes.Ldarg_0);										// *,rdr
                                    il.Emit(OpCodes.Ldc_I4, i);										// *,rdr,i
                                    il.Emit(OpCodes.Callvirt, fnGetValue);							// *,value

                                    // Call the converter
                                    if (converter != null)
                                        il.Emit(OpCodes.Callvirt, fnInvoke);

                                    // Assign it
                                    il.Emit(OpCodes.Unbox_Any, pc.PropertyInfo.PropertyType);		// poco,poco,value
                                    il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true));		// poco
                                }

                                il.MarkLabel(lblNext);
                            }

                            var fnOnLoaded = RecurseInheritedTypes<MethodInfo>(type, (x) => x.GetMethod("OnLoaded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));
                            if (fnOnLoaded != null)
                            {
                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Callvirt, fnOnLoaded);
                            }
                        }

                    il.Emit(OpCodes.Ret);

                    // Cache it, return it
                    var del = m.CreateDelegate(Expression.GetFuncType(typeof(IDataReader), type, type));
                    PocoFactories.Add(key, del);
                    return del;
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }
            }

            private static void AddConverterToStack(ILGenerator il, Func<object, object> converter)
            {
                if (converter != null)
                {
                    // Add the converter
                    int converterIndex = m_Converters.Count;
                    m_Converters.Add(converter);

                    // Generate IL to push the converter onto the stack
                    il.Emit(OpCodes.Ldsfld, fldConverters);
                    il.Emit(OpCodes.Ldc_I4, converterIndex);
                    il.Emit(OpCodes.Callvirt, fnListGetItem);					// Converter
                }
            }

            public static Func<object, object> GetConverter(bool forceDateTimesToUtc, PocoColumn pc, Type srcType, Type dstType)
            {
                Func<object, object> converter = null;

                // Get converter from the mapper
                if (Database.DbObjectMapper != null)
                {
                    if (pc != null)
                    {
                        converter = Database.DbObjectMapper.GetFromDbConverter(pc.PropertyInfo, srcType);
                    }
                    else
                    {
                        var m2 = Database.DbObjectMapper as IMapper2;
                        if (m2 != null)
                        {
                            converter = m2.GetFromDbConverter(dstType, srcType);
                        }
                    }
                }

                // Standard DateTime->Utc mapper
                if (forceDateTimesToUtc && converter == null && srcType == typeof(DateTime) && (dstType == typeof(DateTime) || dstType == typeof(DateTime?)))
                {
                    converter = delegate(object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };
                }

                // Forced type conversion including integral types -> enum
                if (converter == null)
                {
                    if (dstType.IsEnum && IsIntegralType(srcType))
                    {
                        if (srcType != typeof(int))
                        {
                            converter = src => Convert.ChangeType(src, typeof(int), null);
                        }
                    }
                    else if (!dstType.IsAssignableFrom(srcType))
                    {
                        if (dstType.IsEnum && srcType == typeof(string))
                        {
                            converter = src => EnumMapper.EnumFromString(dstType, (string)src);
                        }
                        else
                        {
                            converter = src => Convert.ChangeType(src, dstType, null);
                        }
                    }
                }
                return converter;
            }


            static T RecurseInheritedTypes<T>(Type t, Func<Type, T> cb)
            {
                while (t != null)
                {
                    T info = cb(t);
                    if (info != null)
                        return info;
                    t = t.BaseType;
                }
                return default(T);
            }


            static Dictionary<Type, PocoData> m_PocoDatas = new Dictionary<Type, PocoData>();
            static List<Func<object, object>> m_Converters = new List<Func<object, object>>();
            static MethodInfo fnGetValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });
            static MethodInfo fnIsDBNull = typeof(IDataRecord).GetMethod("IsDBNull");
            static FieldInfo fldConverters = typeof(PocoData).GetField("m_Converters", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
            static MethodInfo fnListGetItem = typeof(List<Func<object, object>>).GetProperty("Item").GetGetMethod();
            static MethodInfo fnInvoke = typeof(Func<object, object>).GetMethod("Invoke");
            public Type type;
            public string[] QueryColumns { get; protected set; }
            public TableInfo TableInfo { get; protected set; }
            public Dictionary<string, PocoColumn> Columns { get; protected set; }
            Dictionary<string, Delegate> PocoFactories = new Dictionary<string, Delegate>();
        }

        class EnumMapper : IDisposable
        {
            readonly Dictionary<Type, Dictionary<string, object>> _stringsToEnums = new Dictionary<Type, Dictionary<string, object>>();
            readonly Dictionary<Type, Dictionary<int, string>> _enumNumbersToStrings = new Dictionary<Type, Dictionary<int, string>>();
            readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

            public object EnumFromString(Type type, string value)
            {
                PopulateIfNotPresent(type);
                return _stringsToEnums[type][value];
            }

            public string StringFromEnum(object theEnum)
            {
                Type typeOfEnum = theEnum.GetType();
                PopulateIfNotPresent(typeOfEnum);
                return _enumNumbersToStrings[typeOfEnum][(int)theEnum];
            }

            void PopulateIfNotPresent(Type type)
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    if (!_stringsToEnums.ContainsKey(type))
                    {
                        _lock.EnterWriteLock();
                        try
                        {
                            Populate(type);
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }

            void Populate(Type type)
            {
                Array values = Enum.GetValues(type);
                _stringsToEnums[type] = new Dictionary<string, object>(values.Length);
                _enumNumbersToStrings[type] = new Dictionary<int, string>(values.Length);

                for (int i = 0; i < values.Length; i++)
                {
                    object value = values.GetValue(i);
                    _stringsToEnums[type].Add(value.ToString(), value);
                    _enumNumbersToStrings[type].Add((int)value, value.ToString());
                }
            }

            public void Dispose()
            {
                _lock.Dispose();
            }
        }


        // Member variables

        #region Helper methods added to the framework

        /// <summary>
        /// Invokes the specified data operation and converts generic SqlExceptions to
        /// typed exceptions.
        /// </summary>
        /// <typeparam name="T">Type of result set</typeparam>
        /// <param name="action">Data operation</param>
        /// <returns>Result of the data operation.</returns>
        protected static T InvokeDataOperation<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (SqlException ex)
            {
                throw DatabaseExceptionHelper.TransformSqlException(ex);
            }
        }

        #endregion

    }
}
