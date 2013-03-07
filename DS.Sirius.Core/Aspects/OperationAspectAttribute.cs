using System;
using System.Runtime.Remoting.Messaging;

namespace DS.Sirius.Core.Aspects
{
    /// <summary>
    /// This class is intended to be the base class of all attributes that signify
    /// operations related to aspects.
    /// </summary>
    public abstract class OperationAspectAttribute : Attribute, IOperationAspect
    {
        /// <summary>
        /// Gets the intended order of execution when an operation uses more than one aspects.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets the flag signing whether this aspect instance overrides the same type of 
        /// aspect instances.
        /// </summary>
        /// <remarks>
        /// Aspect attributes can be assigned to assemblies, classes and methods (operations).
        /// If this property is <code>true</code>, an aspect overrides the aspect defined in a higher level.
        /// Class attrbute overrides assembly level attribute, method attribute overrides class
        /// and assembly level attribute. Using <code>false</code>, the aspect does not override
        /// other aspects assigned to a higher level, instead, both aspects are executed.
        /// </remarks>
        public bool Override { get; set; }

        /// <summary>
        /// Initializes the attribute instance.
        /// </summary>
        protected OperationAspectAttribute()
        {
            Order = 0;
            Override = false;
        }

        /// <summary>
        /// This method is called before the body of the aspected method is about to be
        /// invoked.
        /// </summary>
        /// <param name="argsMessage">Message representing the method call</param>
        /// <param name="returnMessage">Return message coming from the previous aspect.</param>
        /// <param name="proxiedType">Type activating the aspect</param>
        public virtual IMethodReturnMessage OnEntry(IMethodCallMessage argsMessage,
            IMethodReturnMessage returnMessage, Type proxiedType)
        {
            return returnMessage;
        }

        /// <summary>
        /// This method is called right after the body of the aspected method has been
        /// executed, independently whether it was successful or failed.
        /// </summary>
        /// <param name="argsMessage">Message representing the method call</param>
        /// <param name="returnMessage">Message representing the return values of the call</param>
        /// <param name="proxiedType">Type activating the aspect</param>
        public virtual void OnExit(IMethodCallMessage argsMessage, IMethodReturnMessage returnMessage,
            Type proxiedType)
        {
        }

        /// <summary>
        /// This method is called right after <see cref="IOperationAspect.OnExit"/>, when the method body
        /// invocation was successful. Otherwise, the <see cref="IOperationAspect.OnException"/> method is
        /// called.
        /// </summary>
        /// <param name="argsMessage">Message representing the method call</param>
        /// <param name="returnMessage">Message representing the return values of the call</param>
        /// <param name="proxiedType">Type activating the aspect</param>
        public virtual IMethodReturnMessage OnSuccess(IMethodCallMessage argsMessage,
            IMethodReturnMessage returnMessage, Type proxiedType)
        {
            return returnMessage;
        }

        /// <summary>
        /// This method is called right after <see cref="IOperationAspect.OnExit"/>, when the method body
        /// invocation raised an exception. Otherwise, the <see cref="IOperationAspect.OnSuccess"/> method is
        /// called.
        /// </summary>
        /// <param name="argsMessage">Message representing the method call</param>
        /// <param name="exceptionRaised">Exception raised by the method body</param>
        /// <returns>Exception instance to be raised by the caller of the aspected method</returns>
        /// <param name="proxiedType">Type activating the aspect</param>
        public virtual Exception OnException(IMethodCallMessage argsMessage, Exception exceptionRaised,
            Type proxiedType)
        {
            return exceptionRaised;
        }
    }
}