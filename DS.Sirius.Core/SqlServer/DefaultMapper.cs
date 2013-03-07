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
using System.Reflection;

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// Default implementation of IMapper2
    /// </summary>
    public abstract class DefaultMapper : IMapper2
    {
        /// <summary>
        /// Gets table information
        /// </summary>
        /// <param name="t">Type to get the table information</param>
        /// <param name="ti">Object to fill with table information</param>
        public virtual void GetTableInfo(Type t, TableInfo ti) { }

        /// <summary>
        /// Maps the specified property to a column
        /// </summary>
        /// <param name="pi">Property information</param>
        /// <param name="columnName">Name of the data column</param>
        /// <param name="resultColumn">Is a result column?</param>
        /// <returns></returns>
        public virtual bool MapPropertyToColumn(PropertyInfo pi, ref string columnName, ref bool resultColumn)
        {
            return true;
        }

        // TODO: Complete this comment
        /// <summary>
        /// ???
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public virtual Func<object, object> GetFromDbConverter(PropertyInfo pi, Type sourceType)
        {
            return GetFromDbConverter(pi.PropertyType, sourceType);
        }

        // TODO: Complete this comment
        /// <summary>
        /// ???
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public virtual Func<object, object> GetToDbConverter(Type sourceType)
        {
            return null;
        }

        // TODO: Complete this comment
        /// <summary>
        /// ???
        /// </summary>
        /// <param name="destType"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public virtual Func<object, object> GetFromDbConverter(Type destType, Type sourceType)
        {
            return null;
        }
    }
}