using System;
using System.Runtime.Remoting.Messaging;

namespace DS.Sirius.Core.Aspects
{
    /// <summary>
    /// This interface defines the responsibilities of an aspect assigned to an operation. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is to be realized by attribute classes.
    /// </para>
    /// <para>
    /// Although this interface defines operation-related functions (related to methods),
    /// the attributes implementing this interface can be assigned to assemblies and 
    /// classes as well.
    /// </para>
    /// </remarks>
    public interface IOperationAspect
    {
        /// <summary>
        /// This method is called before the body of the aspected method is about to be
        /// invoked.
        /// </summary>
        /// <param name="argsMessage">Message representing the method call</param>
        /// <param name="returnMessage">Return message coming from the previous aspect.</param>
        /// <param name="proxiedType">Type activating the aspect</param>
        /// <returns>
        /// This method should return null value indicating that the aspected method's body should
        /// be called. If the method body invocation should be omitted, this method returns the
        /// return message substituting the result coming from the invocation of the method body.
        /// </returns>
        /// <remarks></remarks>
        IMethodReturnMessage OnEntry(IMethodCallMessage argsMessage, IMethodReturnMessage returnMessage,
            Type proxiedType);

        /// <summary>
        /// This method is called right after the body of the aspected method has been
        /// executed, independently whether it was successful or failed.
        /// </summary>
        /// <param name="argsMessage">Message representing the method call</param>
        /// <param name="returnMessage">Message representing the return values of the call</param>
        /// <param name="proxiedType">Type activating the aspect</param>
        void OnExit(IMethodCallMessage argsMessage, IMethodReturnMessage returnMessage, Type proxiedType);

        /// <summary>
        /// This method is called right after <see cref="OnExit"/>, when the method body
        /// invocation was successful. Otherwise, the <see cref="OnException"/> method is
        /// called.
        /// </summary>
        /// <param name="argsMessage">Message representing the method call</param>
        /// <param name="returnMessage">Message representing the return values of the call</param>
        /// <param name="proxiedType">Type activating the aspect</param>
        /// <returns>
        /// This method should return the value of <paramref name="returnMessage"/> by defult, or it
        /// can modify the original result on return that value.
        /// </returns>
        IMethodReturnMessage OnSuccess(IMethodCallMessage argsMessage, IMethodReturnMessage returnMessage,
            Type proxiedType);

        /// <summary>
        /// This method is called right after <see cref="OnExit"/>, when the method body
        /// invocation raised an exception. Otherwise, the <see cref="OnSuccess"/> method is
        /// called.
        /// </summary>
        /// <param name="argsMessage">Message representing the method call</param>
        /// <param name="exceptionRaised">Exception raised by the method body</param>
        /// <returns>Exception instance to be raised by the caller of the aspected method</returns>
        /// <param name="proxiedType">Type activating the aspect</param>
        Exception OnException(IMethodCallMessage argsMessage, Exception exceptionRaised,
            Type proxiedType);
    }
}