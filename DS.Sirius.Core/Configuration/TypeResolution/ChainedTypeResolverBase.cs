using System;
using DS.Sirius.Core.Common;

namespace DS.Sirius.Core.Configuration.TypeResolution
{
    /// <summary>
    /// This class implements a chained type resolver that turns to a default type resolver
    /// if it is unable to resolve a type by its name.
    /// </summary>
    public abstract class ChainedTypeResolverBase: ITypeResolver
    {
        private readonly ITypeResolver _defaultResolver;

        /// <summary>
        /// Creates a type resolver using a <see cref="TrivialTypeResolver"/> instance as
        /// a fallback.
        /// </summary>
        protected ChainedTypeResolverBase(): this(new TrivialTypeResolver())
        {
        }

        /// <summary>
        /// Creates a type resolver using the specified type resolver instance as
        /// a fallback.
        /// </summary>
        /// <param name="defaultResolver">Fallback type resolver</param>
        protected ChainedTypeResolverBase(ITypeResolver defaultResolver)
        {
            _defaultResolver = defaultResolver;
        }

        /// <summary>
        /// Resolves the specified name to a <see cref="Type"/> instance.
        /// </summary>
        /// <param name="name">Name to resolve</param>
        /// <returns><see cref="Type"/>Type instance if resolutions is OK; otherwise, false</returns>
        Type ITypeResolver.Resolve(string name)
        {
            var result = Resolve(name);
            return result ?? _defaultResolver.Resolve(name);
        }

        /// <summary>
        /// Resolves the specified name to a <see cref="Type"/> instance.
        /// </summary>
        /// <param name="name">Name to resolve</param>
        /// <returns><see cref="Type"/>This implementation always return null</returns>
        public virtual Type Resolve(string name)
        {
            return null;
        }
    }
}
