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
    /// For explicit pocos, marks property as a result column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ResultColumnAttribute : ColumnAttribute
    {
        /// <summary>
        /// Isntantiates the attribute
        /// </summary>
        public ResultColumnAttribute() { }

        /// <summary>
        /// Instantiates the attribute with the specified name
        /// </summary>
        /// <param name="name">Column name in the database</param>
        public ResultColumnAttribute(string name) : base(name) { }
    }
}