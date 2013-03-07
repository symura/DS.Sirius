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

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// Specifies the table name of the poco
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        /// <summary>
        /// Instantiates the attribute with the specified name.
        /// </summary>
        /// <param name="tableName">Data table name</param>
        public TableNameAttribute(string tableName)
        {
            Value = tableName;
        }

        /// <summary>
        /// Gets the table name of the poco
        /// </summary>
        public string Value { get; private set; }
    }
}