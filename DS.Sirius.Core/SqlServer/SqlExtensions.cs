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
    /// This class contains useful extensions for Sql and SqlBuilder classes.
    /// </summary>
    public static class SqlExtensions
    {
        /// <summary>
        /// Converts the specified template instance into a Sql object.
        /// </summary>
        /// <param name="template">Template instance</param>
        /// <returns>
        /// Sql instance representing the SQL statement described by the template
        /// </returns>
        public static Sql ToSql(this SqlBuilder.Template template)
        {
            return new Sql(true, template.RawSql, template.Parameters);
        }
    }
}
