using System;

namespace DS.Sirius.Core.SqlServer.Exceptions
{
    /// <summary>
    /// This class represents the data base operation exception when a foreign key 
    /// constriant is violated.
    /// </summary>
    public class ForeignKeyViolationException : Exception
    {
        /// <summary>
        /// Gets the name of the foreign key.
        /// </summary>
        public string KeyName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error keyName.
        /// </summary>
        /// <param name="keyName">The keyName that describes the error. </param>
        public ForeignKeyViolationException(string keyName)
            : this(keyName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error keyName and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="keyName">The error keyName that explains the reason for the exception. </param><param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public ForeignKeyViolationException(string keyName, Exception innerException) :
            base(String.Format("Foreign key violation on constraint {0}.", keyName), innerException)
        {
            KeyName = keyName;
        }
    }
}