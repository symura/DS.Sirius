using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Services;
using DS.Sirius.Core.WindowsEventLog;

namespace DS.Sirius.Core.Aspects
{
    /// <summary>
    /// This proxy class can inject aspects bwtween the operations of a contex-bound
    /// object and the callers of its operations.
    /// </summary>
    /// <remarks>
    /// To add aspects to a class, derive the class directly or indirectly from
    /// <see cref="ContextBoundObject"/>.
    /// </remarks>
    public abstract class AspectInjectionProxy : RealProxy
    {
        /// <summary>
        /// Creates a new instance of this class and stores the target object.
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="proxiedType">Target object type</param>
        protected AspectInjectionProxy(MarshalByRefObject target, Type proxiedType)
            : base(proxiedType)
        {
            Target = target;
            ProxiedType = proxiedType;
        }

        /// <summary>
        /// Gets the target object assigned to this proxy object.
        /// </summary>
        protected MarshalByRefObject Target { get; private set; }

        /// <summary>
        /// Gets the type that activates this proxy (type of constructing the proxy)
        /// </summary>
        protected Type ProxiedType { get; private set; }

        /// <summary>
        /// Invokes the message specified by the request parameter.
        /// </summary>
        /// <param name="request">Request to invoke</param>
        /// <returns>IMessage instance representing the return message</returns>
        public override IMessage Invoke(IMessage request)
        {
            IMethodReturnMessage response;
            var call = (IMethodCallMessage)request;
            var ctor = call as IConstructionCallMessage;
            if (ctor != null)
            {
                // ReSharper disable PossibleNullReferenceException
                // --- This is the call to the constructor, initialize the object
                var defaultProxy = RemotingServices.GetRealProxy(Target);
                response = defaultProxy.InitializeServerObject(ctor);
                if (response == null)
                {
                    var returnException = new AspectInfrastructureException(
                      "InitializeServerObject failed in AspectInjectionProxy.Invoke");
                    WindowsEventLogger.Log<AspectInfrastructureErrorEvent>(
                        "The {0} constructor raised a non-business exception:{1}",
                        ctor.MethodBase.DeclaringType.FullName,
                        returnException.ToString());
                    return new ReturnMessage(returnException, call);
                }
                if (response.Exception != null)
                {
                    WindowsEventLogger.Log<AspectInfrastructureErrorEvent>(
                        "The {0} constructor raised a non-business exception:{1}",
                        ctor.MethodBase.DeclaringType.FullName,
                        response.Exception.ToString());
                    return new ReturnMessage(response.Exception, call);
                }

                // --- At this point the server object is initialized, and so can be used
                // --- Call the contructor behind the transparent proxy.
                var tp = (ContextBoundObject)GetTransparentProxy();
                response = EnterpriseServicesHelper.CreateConstructionReturnMessage(ctor, tp);

                // --- Allow configuring the proxy
                try
                {
                    InitializeProxy();
                }
                catch (Exception ex)
                {
                    var returnException = new AspectInfrastructureException(
                      "Calling context setup failed in AspectInjectionProxy.Invoke", ex);
                    return new ReturnMessage(returnException, call);
                }
                // ReSharper restore PossibleNullReferenceException
            }
            else
            {
                // --- This is a call to a method
                // --- Check, if the method is an aspected operation or not
                response = IsAspectedMessage(call)
                  ? OperationMethodCall(call)
                  : RemotingServices.ExecuteMessage(Target, call);
            }
            return response;
        }

        /// <summary>
        /// Executes the operation method.
        /// </summary>
        /// <param name="call">Method call with parameters, to execute</param>
        /// <returns>IMethodReturnMessage representing the result of the call</returns>
        private IMethodReturnMessage OperationMethodCall(IMethodCallMessage call)
        {
            try
            {
                // --- Manage the "before" aspects
                var response = BeforeOperationAspect(call);

                // --- Call the target method, assuming there is no result yet
                response = response ?? RemotingServices.ExecuteMessage(Target, call);

                // --- Manage the "after" aspects including exceptions
                AfterOperationAspect(call, response);
                if (response.Exception != null)
                {
                    response = new ReturnMessage(HandleExceptionAspect(call, response.Exception), call);
                }
                if (response.Exception == null) AfterSuccessfulOperationAspect(call, response);
                return response;
            }
            catch (Exception ex)
            {
                try
                {
                    var handledEx = HandleExceptionAspect(call, ex);
                    return new ReturnMessage(handledEx, call);
                }
                catch (Exception ex2)
                {
                    var innerException =
                        new AspectInfrastructureException("HandleExceptionAspect failed.", ex2);
                    innerException.Data.Add("ExceptionBeinghandled", ex);
                    var returnException = new AspectInfrastructureException(
                      "Exception handling failed in AspectInjectionProxy.OperationMethodCall", innerException);
                    return new ReturnMessage(returnException, call);
                }
            }
        }

        /// <summary>
        /// Override this method to allow proxy object setup.
        /// </summary>
        protected virtual void InitializeProxy()
        {
        }

        /// <summary>
        /// Checks whether the method to call is an aspected call
        /// </summary>
        /// <param name="call">Object defining the method call</param>
        /// <returns></returns>
        protected virtual bool IsAspectedMessage(IMethodCallMessage call)
        {
            return true;
        }

        /// <summary>
        /// Executes aspects that should be called before the method body.
        /// </summary>
        /// <param name="call">Object defining the method call</param>
        protected virtual IMethodReturnMessage BeforeOperationAspect(IMethodCallMessage call)
        {
            var aspectedObject = Target as IOperationAspect;
            return aspectedObject != null
                ? aspectedObject.OnEntry(call, null, ProxiedType)
                : null;
        }

        /// <summary>
        /// Executes aspects that should be called after the method body.
        /// </summary>
        /// <param name="call">Object defining the method call</param>
        /// <param name="response">Response object coming from the method call</param>
        protected virtual void AfterOperationAspect(IMethodCallMessage call, IMethodReturnMessage response)
        {
            var aspectedObject = Target as IOperationAspect;
            if (aspectedObject != null)
            {
                aspectedObject.OnExit(call, response, ProxiedType);
            }
        }

        /// <summary>
        /// Executes aspects that should be called after the successful call of
        /// the method body.
        /// </summary>
        /// <param name="call">Object defining the method call</param>
        /// <param name="response">Response object coming from the method call</param>
        protected virtual IMethodReturnMessage AfterSuccessfulOperationAspect(IMethodCallMessage call,
            IMethodReturnMessage response)
        {
            var aspectedObject = Target as IOperationAspect;
            return aspectedObject != null
                ? aspectedObject.OnSuccess(call, response, ProxiedType)
                : null;
        }

        /// <summary>
        /// Transforms the exection raised during the method call
        /// </summary>
        /// <param name="call">Object defining the method call</param>
        /// <param name="ex">Exception raised during the method call, including aspects</param>
        /// <returns>Execption that should be returned to the caller</returns>
        protected virtual Exception HandleExceptionAspect(IMethodCallMessage call, Exception ex)
        {
            var aspectedObject = Target as IOperationAspect;
            return aspectedObject != null
                ? aspectedObject.OnException(call, ex, ProxiedType)
                : null;
        }
    }
}