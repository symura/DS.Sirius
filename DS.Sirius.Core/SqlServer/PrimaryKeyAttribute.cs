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
    /// Specific the primary key of a poco class (and optional sequence name for Oracle)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Instantiates the attribute with the specified primary key name.
        /// </summary>
        /// <param name="primaryKey">Primary key fields separated by a comma</param>
        public PrimaryKeyAttribute(string primaryKey)
        {
            Value = primaryKey;
            AutoIncrement = true;
        }

        /// <summary>
        /// Gets the primary key fields
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Gets or sets the Oracle sequence name
        /// </summary>
        public string SequenceName { get; set; }

        /// <summary>
        /// Gets the flag indicating if auto increment is to be used or not.
        /// </summary>
        public bool AutoIncrement { get; set; }
    }
}