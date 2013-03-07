using System;

namespace DS.Sirius.Core.Aspects
{
    /// <summary>
    /// Represents the ID of the tenant with the current logical context
    /// </summary>
    [Serializable]
    public class TenantIdContextItem : ContextItemBase
    {
        /// <summary>
        /// Gets the operation instance identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Instantiates an empty instance
        /// </summary>
        public TenantIdContextItem()
        {
        }

        /// <summary>
        /// Creates a new instance with the specified identifier.
        /// </summary>
        /// <param name="id"></param>
        public TenantIdContextItem(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets the data item related to the context.
        /// </summary>
        public override object Data
        {
            get { return Id; }
            set { Id = (string)value; }
        }
    }
}