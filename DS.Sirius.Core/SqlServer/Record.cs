using System;
using System.Collections.Generic;

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// This generic class describes a record that can be used by the <see cref="Database"/>
    /// object for CRUD operations.
    /// </summary>
    /// <typeparam name="T">Type of the record</typeparam>
    public class Record<T> where T : new()
    {
        private readonly HashSet<string> _modifiedColumns = new HashSet<string>();

        /// <summary>
        /// Marks the specified column modified
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        protected void MarkColumnModified(string columnName)
        {
            _modifiedColumns.Add(columnName);
        }

        /// <summary>
        /// Resets all changes in this record
        /// </summary>
        public void ResetChanges()
        {
            _modifiedColumns.Clear();
        }

        /// <summary>
        /// The <see cref="Database"/> object uses this method to reset the entity state
        /// </summary>
        private void OnLoaded()
        {
            _modifiedColumns.Clear();
        }

        /// <summary>
        /// Gets the type of the poco represented by this record
        /// </summary>
        /// <returns></returns>
        public Type GetRecordType()
        {
            return typeof(T);
        }

        /// <summary>
        /// Gets the names of columns modified
        /// </summary>
        public IEnumerable<string> ModifiedColumns
        {
            get { return _modifiedColumns; }
        }
    }
}