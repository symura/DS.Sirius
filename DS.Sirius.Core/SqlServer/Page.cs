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

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// This class represents a result from a paged request
    /// </summary>
    /// <typeparam name="T">Typy of the poco</typeparam>
    public class Page<T>
    {
        /// <summary>
        /// Gets or sets the cardinal number of the current page.
        /// </summary>
        public long CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the number of total pages.
        /// </summary>
        public long TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the number of total items.
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public long ItemsPerPage { get; set; }

        /// <summary>
        /// Gets the list of items.
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Gets the object representing the page's context.
        /// </summary>
        public object Context { get; set; }
    }
}