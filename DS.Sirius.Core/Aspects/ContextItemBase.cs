using System.Runtime.Remoting.Messaging;

namespace DS.Sirius.Core.Aspects
{
    /// <summary>
    /// This class defines a base class for context data items than can be put into
    /// logical context calls that work accross appdomain boundary.
    /// </summary>
    /// <remarks>
    /// For sake of simplicity, use intrinsic data types to represent <see cref="Data"/>
    /// </remarks>
    public abstract class ContextItemBase : ILogicalThreadAffinative
    {
        /// <summary>
        /// Gets or sets the data item related to the context.
        /// </summary>
        public abstract object Data { get; set; }
    }
}