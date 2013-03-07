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
    /// For explicit pocos, marks property as a column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Instantiates an attribute
        /// </summary>
        public ColumnAttribute() { }

        /// <summary>
        /// Instantiates an attribute with the specified database column name.
        /// </summary>
        /// <param name="name">Column name in the database</param>
        public ColumnAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the column
        /// </summary>
        public string Name { get; private set; }
    }
}