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

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// This class is used by IMapper to override table binding information.
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// Gets or sets the name of the database table.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the primary key fields (separated by comma)
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets the AutoIncrement value.
        /// </summary>
        public bool AutoIncrement { get; set; }

        /// <summary>
        /// Gets or sets the sequence name.
        /// </summary>
        public string SequenceName { get; set; }
    }
}