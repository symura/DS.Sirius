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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// Simple helper class for building SQL statements.
    /// </summary>
    [DebuggerDisplay("{SQL}")]
    public class Sql
    {
        // --- Current SQL statement
        private string _sql;

        // --- SQL statement parameters
        private object[] _args;
        
        // --- Right hand size SQL statement
        Sql _rhs;

        // --- Final SQL statement
        string _sqlFinal;

        // --- Final SQL statement parameters
        object[] _argsFinal;

        /// <summary>
        /// Creates an instance with an empty SQL statement.
        /// </summary>
        public Sql()
        {
        }

        /// <summary>
        /// Creates an instance with the specified SQL statement.
        /// </summary>
        /// <param name="sql">SQL statement with parameter placeholders</param>
        /// <param name="args">Parameter arguments</param>
        public Sql(string sql, params object[] args)
        {
            _sql = sql;
            _args = args;
        }

        /// <summary>
        /// Creates an instance with the specified SQL statement and build status.
        /// </summary>
        /// <param name="isBuilt">Flag indicating whether the statement is already built</param>
        /// <param name="sql">SQL statement with parameter placeholders</param>
        /// <param name="args">Parameter arguments</param>
        public Sql(bool isBuilt, string sql, params object[] args)
        {
            _sql = sql;
            _args = args;
            if (!isBuilt) return;
            _sqlFinal = _sql;
            _argsFinal = _args;
        }

        /// <summary>
        /// Factory method to retrieve a new Sql instance.
        /// </summary>
        public static Sql Builder
        {
            get { return new Sql(); }
        }

        /// <summary>
        /// Gets the current SQL statement.
        /// </summary>
        public string SQL
        {
            get
            {
                Build();
                return _sqlFinal;
            }
        }

        /// <summary>
        /// Gets the current array of parameters.
        /// </summary>
        public object[] Arguments
        {
            get
            {
                Build();
                return _argsFinal;
            }
        }

        /// <summary>
        /// Appends a new chunk of SQL to the current statement
        /// </summary>
        /// <param name="sql">Sql object representing the statement to append</param>
        /// <returns>The Sql object representing the merged SQL statement</returns>
        public Sql Append(Sql sql)
        {
            _sqlFinal = null;

            if (_rhs != null)
            {
                _rhs.Append(sql);
            }
            else if (_sql != null)
            {
                _rhs = sql;
            }
            else
            {
                _sql = sql._sql;
                _args = sql._args;
                _rhs = sql._rhs;
            }
            return this;
        }

        /// <summary>
        /// Appends a new chunk of SQL to the current statement
        /// </summary>
        /// <param name="sql">Sql object representing the statement to append</param>
        /// <param name="args">Arguments in the appended SQL statement</param>
        /// <returns>The Sql object representing the merged SQL statement</returns>
        public Sql Append(string sql, params object[] args)
        {
            return Append(new Sql(sql, args));
        }

        /// <summary>
        /// Checks if the specified Sql instance is the given clause
        /// </summary>
        /// <param name="sql">Sql instance</param>
        /// <param name="sqltype">SQL clause (e.g. WHERE, ORDER BY, etc.)</param>
        /// <returns>True, if the Sql instance is the specified clause; otherwise, false.</returns>
        static bool Is(Sql sql, string sqltype)
        {
            return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Adds a new WHERE clause to the SQL statement.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns>The Sql object representing the merged SQL statement</returns>
        public Sql Where(string sql, params object[] args)
        {
            return Append(new Sql("WHERE (" + sql + ")", args));
        }

        /// <summary>
        /// Adds a new ORDER BY clause with the specified columns.
        /// </summary>
        /// <param name="columns">Columns of the ORDER BY clause</param>
        /// <returns>The Sql object representing the merged SQL statement</returns>
        public Sql OrderBy(params object[] columns)
        {
            return Append(new Sql("ORDER BY " + String.Join(", ", 
                (from x in columns select x.ToString()).ToArray())));
        }

        /// <summary>
        /// Adds a new SELECT clause with the specified columns.
        /// </summary>
        /// <param name="columns">Columns of the SELECT clause</param>
        /// <returns>The Sql object representing the merged SQL statement</returns>
        public Sql Select(params object[] columns)
        {
            return Append(new Sql("SELECT " + String.Join(", ", 
                (from x in columns select x.ToString()).ToArray())));
        }

        /// <summary>
        /// Adds a new FROM clause with the specified tables.
        /// </summary>
        /// <param name="tables">Columns of the FROM clause</param>
        /// <returns>The Sql object representing the merged SQL statement</returns>
        public Sql From(params object[] tables)
        {
            return Append(new Sql("FROM " + String.Join(", ", (from x in tables select x.ToString()).ToArray())));
        }


        /// <summary>
        /// Adds a new GROUP BY clause with the specified columns.
        /// </summary>
        /// <param name="columns">Columns of the GROUP BY clause</param>
        /// <returns>The Sql object representing the merged SQL statement</returns>
        public Sql GroupBy(params object[] columns)
        {
            return Append(new Sql("GROUP BY " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        /// <summary>
        /// Creates a new SqlJoinClause from the specified join type and table.
        /// </summary>
        /// <param name="joinType">Type of join (e.g., inner join, left join, etc.)</param>
        /// <param name="table">Table to use in the join clause.</param>
        /// <returns>The SqlJoinClause object representing the join</returns>
        private SqlJoinClause Join(string joinType, string table)
        {
            return new SqlJoinClause(Append(new Sql(joinType + table)));
        }

        /// <summary>
        /// Gets an inner join with the specified table.
        /// </summary>
        /// <param name="table">Table to use in the join cluase</param>
        /// <returns>The SqlJoinClause object representing the join</returns>
        public SqlJoinClause InnerJoin(string table) { return Join("INNER JOIN ", table); }

        /// <summary>
        /// Gets a left join with the specified table.
        /// </summary>
        /// <param name="table">Table to use in the join cluase</param>
        /// <returns>The SqlJoinClause object representing the join</returns>
        public SqlJoinClause LeftJoin(string table) { return Join("LEFT JOIN ", table); }

        /// <summary>
        /// This class represents an SQL join clause.
        /// </summary>
        public class SqlJoinClause
        {
            // --- Sql instance this join belongs to
            private readonly Sql _sql;

            /// <summary>
            /// Creates a new instance using the specified Sql instance
            /// </summary>
            /// <param name="sql">Sql instance this instance belongs to</param>
            public SqlJoinClause(Sql sql)
            {
                _sql = sql;
            }

            /// <summary>
            /// Adds an ON clause to the join instance.
            /// </summary>
            /// <param name="onClause">SQL describing the ON clause</param>
            /// <param name="args">SQL parameters</param>
            /// <returns>The Sql object representing the merged SQL statement</returns>
            public Sql On(string onClause, params object[] args)
            {
                return _sql.Append("ON " + onClause, args);
            }
        }

        /// <summary>
        /// Implicitly converts the specified template to an Sql object.
        /// </summary>
        /// <param name="template"></param>
        /// <returns>
        /// The converted Sql object.
        /// </returns>
        public static implicit operator Sql(SqlBuilder.Template template)
        {
            return new Sql(true, template.RawSql, template.Parameters);
        }

        #region Helper methods

        /// <summary>
        /// Builds and finalizes the SQL statement
        /// </summary>
        private void Build()
        {
            // --- Already built?
            if (_sqlFinal != null) return;

            // --- Build it
            var sb = new StringBuilder();
            var args = new List<object>();
            Build(sb, args, null);
            _sqlFinal = sb.ToString();
            _argsFinal = args.ToArray();
        }

        /// <summary>
        /// Builds and finalizes the SQL statement
        /// </summary>
        /// <param name="sb">StringBuilder that contains the final SQL statement</param>
        /// <param name="args">SQL statement parameters</param>
        /// <param name="lhs">Left hand side SQL statement</param>
        private void Build(StringBuilder sb, List<object> args, Sql lhs)
        {
            if (!String.IsNullOrEmpty(_sql))
            {
                // --- Add SQL to the string
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }

                var sql = Database.ProcessParams(_sql, _args, args);

                if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
                    sql = "AND " + sql.Substring(6);
                if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
                    sql = ", " + sql.Substring(9);

                sb.Append(sql);
            }

            // Now do rhs
            if (_rhs != null)
                _rhs.Build(sb, args, this);
        }

        #endregion
    }
}
