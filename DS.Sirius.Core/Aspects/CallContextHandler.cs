using System;
using System.Runtime.Remoting.Messaging;

namespace DS.Sirius.Core.Aspects
{
    /// <summary>
    /// This class is responsible for managing data passed among call contexts
    /// </summary>
    public static class CallContextHandler
    {
        /// <summary>
        /// Passes the specified data to the current logical call context.
        /// </summary>
        /// <typeparam name="T">Type of data to pass</typeparam>
        /// <param name="data">Data instance to pass.</param>
        public static void SetData<T>(T data)
            where T : ContextItemBase, new()
        {
            // ReSharper disable AssignNullToNotNullAttribute
            CallContext.LogicalSetData(typeof(T).FullName, data.Data);
            // ReSharper restore AssignNullToNotNullAttribute
        }

        /// <summary>
        /// Checks whether the contex has data for the specified type
        /// </summary>
        /// <param name="type">Type representing context data</param>
        /// <returns></returns>
        public static bool ContextHasData(Type type)
        {
            // ReSharper disable AssignNullToNotNullAttribute
            var data = CallContext.LogicalGetData(type.FullName);
            // ReSharper restore AssignNullToNotNullAttribute
            return data != null;
        }

        /// <summary>
        /// Gets the data instance passed to the logical call context.
        /// </summary>
        /// <typeparam name="T">Type of data to obtain</typeparam>
        /// <returns>Data instance obtained from the contex if exists; otherwise, null</returns>
        public static T GetData<T>()
            where T : ContextItemBase, new()
        {
            // ReSharper disable AssignNullToNotNullAttribute
            var data = CallContext.LogicalGetData(typeof(T).FullName);
            // ReSharper restore AssignNullToNotNullAttribute
            return data == null ? null : new T { Data = data };
        }

        /// <summary>
        /// Removes the data instance from the current logical call context.
        /// </summary>
        /// <typeparam name="T">Type of dat to remove from the data context</typeparam>
        public static void FreeData<T>()
            where T : ContextItemBase, new()
        {
            // ReSharper disable AssignNullToNotNullAttribute
            CallContext.FreeNamedDataSlot(typeof(T).FullName);
            // ReSharper restore AssignNullToNotNullAttribute
        }
    }
}