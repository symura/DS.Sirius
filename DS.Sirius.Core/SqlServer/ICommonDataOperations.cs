using System;
using System.Transactions;

namespace DS.Sirius.Core.SqlServer
{
    /// <summary>
    /// This interface defines common data operations such as transaction management and
    /// context tracking.
    /// </summary>
    public interface ICommonDataOperations: IDisposable
    {
        /// <summary>
        /// Signs that the current sequence of operations can be completed.
        /// </summary>
        void CompleteOperation();

        /// <summary>
        /// Creates a transaction scope for this context.
        /// </summary>
        /// <param name="option">TransactionScope options</param>
        /// <param name="level">Transaction isolation level</param>
        void CreateTransactionScope(TransactionScopeOption? option = null, IsolationLevel? level = null);
    }
}