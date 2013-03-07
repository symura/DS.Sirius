using System;

namespace DS.Sirius.Core.Aspects
{
    /// <summary>
    /// Represents the operation instance associated with the current logical context
    /// </summary>
    [Serializable]
    public class OperationInstanceIdContextItem : ContextItemBase
    {
        /// <summary>
        /// Gets the operation instance identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Instantiates an empty instance
        /// </summary>
        public OperationInstanceIdContextItem()
        {
        }

        /// <summary>
        /// Creates a new instance with the specified identifier.
        /// </summary>
        /// <param name="id"></param>
        public OperationInstanceIdContextItem(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets the data item related to the context.
        /// </summary>
        public override object Data
        {
            get { return Id; }
            set { Id = (Guid)value; }
        }
    }
}