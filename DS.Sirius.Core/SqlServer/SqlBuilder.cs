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
using System.Linq;
using System.Text.RegularExpressions;

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// Complex class to build SQL statements with clauses
    /// </summary>
    public class SqlBuilder
    {
        // --- Dictionary storing the SQL clauses
        readonly Dictionary<string, Clauses> _data = new Dictionary<string, Clauses>();

        // --- ???
        int _seq;

        /// <summary>
        /// This class describes a clause.
        /// </summary>
        class Clause
        {
            /// <summary>SQL statement of the clause</summary>
            public string Sql { get; set; }

            /// <summary>Paramters of the SQL clause</summary>
            public List<object> Parameters { get; set; }
        }

        /// <summary>
        /// This class represents a list of clauses.
        /// </summary>
        class Clauses : List<Clause>
        {
            readonly string _joiner;
            readonly string _prefix;
            readonly string _postfix;

            /// <summary>
            /// Creates a clause list from the specified joiner, prefix and postfix.
            /// </summary>
            /// <param name="joiner">Joiner string</param>
            /// <param name="prefix">Prefix string</param>
            /// <param name="postfix">Postfix string</param>
            public Clauses(string joiner, string prefix, string postfix)
            {
                _joiner = joiner;
                _prefix = prefix;
                _postfix = postfix;
            }

            /// <summary>
            /// Resolves the SQL clauses using the specified final parameters
            /// </summary>
            /// <param name="finalParams">Final SQL parameters</param>
            /// <returns></returns>
            public string ResolveClauses(List<object> finalParams)
            {
                foreach (var item in this)
                {
                    item.Sql = Database.ProcessParams(item.Sql, item.Parameters.ToArray(), 
                        finalParams);
                }
                return _prefix + string.Join(_joiner, 
                    this.Select(c => c.Sql).ToArray()) + _postfix;
            }
        }

        /// <summary>
        /// This class represents an SQL statement template
        /// </summary>
        public class Template
        {
            // --- SQL string
            readonly string _sql;

            // --- SQL builder object holding this template
            readonly SqlBuilder _builder;

            // --- Final SQL statement parameters
            private readonly List<object> _finalParams = new List<object>();
            
            // --- ???
            int _dataSeq;

            // --- Raw SQL statement
            string _rawSql;

            // --- Regular expression representing comments
            static readonly Regex s_Regex = new Regex(@"\/\*\*.+\*\*\/", 
                RegexOptions.Compiled | RegexOptions.Multiline);

            /// <summary>
            /// Creates a new template instance with the specified builder from the given
            /// SQL statement.
            /// </summary>
            /// <param name="builder">Builder instance</param>
            /// <param name="sql">SQL statement</param>
            /// <param name="parameters">SQL statement parameters</param>
            public Template(SqlBuilder builder, string sql, params object[] parameters)
            {
                _sql = Database.ProcessParams(sql, parameters, _finalParams);
                _builder = builder;
            }

            /// <summary>
            /// Resolves the SQL statement
            /// </summary>
            void ResolveSql()
            {
                _rawSql = _sql;
                if (_dataSeq != _builder._seq)
                {
                    foreach (var pair in _builder._data)
                    {
                        _rawSql = _rawSql.Replace("/**" + pair.Key + "**/", pair.Value.ResolveClauses(_finalParams));
                    }
                    ReplaceDefaults();
                    _dataSeq = _builder._seq;
                }
                if (_builder._seq == 0)
                {
                    ReplaceDefaults();
                }
            }

            /// <summary>
            /// Replaces defaults
            /// </summary>
            private void ReplaceDefaults()
            {
                foreach (var pair in _builder._defaultsIfEmpty)
                {
                    _rawSql = _rawSql.Replace("/**" + pair.Key + "**/", " " + pair.Value + " ");
                }
                // --- Replace all that is left with empty
                _rawSql = s_Regex.Replace(_rawSql, "");
            }

            /// <summary>
            /// Gets the raw SQL statement
            /// </summary>
            public string RawSql { get { ResolveSql(); return _rawSql; } }

            /// <summary>
            /// Gets the parameters of the raw SQL statement
            /// </summary>
            public object[] Parameters { get { ResolveSql(); return _finalParams.ToArray(); } }
        }


        /// <summary>
        /// Adds a new template to this SQL statement.
        /// </summary>
        /// <param name="sql">Template SQL statement</param>
        /// <param name="parameters">Template SQL parameters</param>
        /// <returns>The new template object representing the SQL statement</returns>
        public Template AddTemplate(string sql, params object[] parameters)
        {
            return new Template(this, sql, parameters);
        }

        /// <summary>
        /// Adds a clause to the builder.
        /// </summary>
        /// <param name="name">Clause name</param>
        /// <param name="sql">Clause SQL statement</param>
        /// <param name="parameters">SQL statement parameters</param>
        /// <param name="joiner">Joiner string</param>
        /// <param name="prefix">Prefix string</param>
        /// <param name="postfix">Postfix string</param>
        void AddClause(string name, string sql, IEnumerable<object> parameters, 
            string joiner, string prefix, string postfix)
        {
            Clauses clauses;
            if (!_data.TryGetValue(name, out clauses))
            {
                clauses = new Clauses(joiner, prefix, postfix);
                _data[name] = clauses;
            }
            clauses.Add(new Clause { Sql = sql, Parameters = new List<object>(parameters) });
            _seq++;
        }

        /// <summary>
        /// Default clauses
        /// </summary>
        private readonly Dictionary<string, string> _defaultsIfEmpty =
            new Dictionary<string, string>
                {
                    {"where", "1=1"},
                    {"select", "1"}
                };

        /// <summary>
        /// Adds a select clause with the specified columns
        /// </summary>
        /// <param name="columns">Column names</param>
        /// <returns>This builder object</returns>
        public SqlBuilder Select(params string[] columns)
        {
            AddClause("select", string.Join(", ", columns), new object[] { }, ", ", "", "");
            return this;
        }

        /// <summary>
        /// Adds a join clause with the specified statement.
        /// </summary>
        /// <param name="sql">SQL statement</param>
        /// <param name="parameters">SQL statement parameters</param>
        /// <returns>This builder object</returns>
        public SqlBuilder Join(string sql, params object[] parameters)
        {
            AddClause("join", sql, parameters, "\nINNER JOIN ", "\nINNER JOIN ", "\n");
            return this;
        }

        /// <summary>
        /// Adds a LEFT JOIN clause with the specified statement.
        /// </summary>
        /// <param name="sql">SQL statement</param>
        /// <param name="parameters">SQL statement parameters</param>
        /// <returns>This builder object</returns>
        public SqlBuilder LeftJoin(string sql, params object[] parameters)
        {
            AddClause("leftjoin", sql, parameters, "\nLEFT JOIN ", "\nLEFT JOIN ", "\n");
            return this;
        }

        /// <summary>
        /// Adds a WHERE clause with the specified statement.
        /// </summary>
        /// <param name="sql">SQL statement</param>
        /// <param name="parameters">SQL statement parameters</param>
        /// <returns>This builder object</returns>
        public SqlBuilder Where(string sql, params object[] parameters)
        {
            AddClause("where", sql, parameters, " AND ", " ( ", " )\n");
            return this;
        }

        /// <summary>
        /// Adds an ORDER BY clause with the specified statement.
        /// </summary>
        /// <param name="sql">SQL statement</param>
        /// <param name="parameters">SQL statement parameters</param>
        /// <returns>This builder object</returns>
        public SqlBuilder OrderBy(string sql, params object[] parameters)
        {
            AddClause("orderby", sql, parameters, ", ", "ORDER BY ", "\n");
            return this;
        }

        /// <summary>
        /// Adds an ORDER BY clause with the specified columns.
        /// </summary>
        /// <param name="columns">Column names</param>
        /// <returns>This builder object</returns>
        public SqlBuilder OrderByCols(params string[] columns)
        {
            AddClause("orderbycols", string.Join(", ", columns), new object[] { }, ", ", ", ", "");
            return this;
        }

        /// <summary>
        /// Adds a GROUP BY clause with the specified statement.
        /// </summary>
        /// <param name="sql">SQL statement</param>
        /// <param name="parameters">SQL statement parameters</param>
        /// <returns>This builder object</returns>
        public SqlBuilder GroupBy(string sql, params object[] parameters)
        {
            AddClause("groupby", sql, parameters, " , ", "\nGROUP BY ", "\n");
            return this;
        }

        /// <summary>
        /// Adds a HAVING clause with the specified statement.
        /// </summary>
        /// <param name="sql">SQL statement</param>
        /// <param name="parameters">SQL statement parameters</param>
        /// <returns>This builder object</returns>
        public SqlBuilder Having(string sql, params object[] parameters)
        {
            AddClause("having", sql, parameters, "\nAND ", "HAVING ", "\n");
            return this;
        }
    }
}