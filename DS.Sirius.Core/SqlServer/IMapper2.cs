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
    /// This will be merged with IMapper in the next major version
    /// </summary>
    public interface IMapper2 : IMapper
    {
        // TODO: Complete this commant
        /// <summary>
        /// ???
        /// </summary>
        /// <param name="destType"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        Func<object, object> GetFromDbConverter(Type destType, Type sourceType);
    }
}