namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This class defines the data structure representing a parameter of the audit log entry.
    /// </summary>
    public class AuditLogParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public AuditLogParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets the name of the parameter definition.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value of the parameter definition.
        /// </summary>
        public object Value { get; private set; }
    }
}