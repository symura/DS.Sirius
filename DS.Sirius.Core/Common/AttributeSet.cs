using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace DS.Sirius.Core.Common
{
    /// <summary>
    /// This class describes an attribute set belongign to a type
    /// </summary>
    public sealed class AttributeSet
    {
        private readonly Dictionary<Type, List<Attribute>> _attributes = new Dictionary<Type, List<Attribute>>();

        /// <summary>
        /// Gets the type owning this attribute set.
        /// </summary>
        public Type OwnerType { get; private set; }

        /// <summary>
        /// Creates a new attribute set
        /// </summary>
        /// <param name="type">Type to scan for attributes</param>
        /// <param name="attrType">Attributes deriving from this type are collected only</param>
        /// <param name="scanBaseTypes">Scahn the inheritance chain?</param>
        public AttributeSet(Type type, Type attrType = null, bool scanBaseTypes = false)
        {
            if (type == null) throw new ArgumentNullException("type");
            OwnerType = type;
            var attrs = (attrType == null
                            ? type.GetCustomAttributes(scanBaseTypes)
                            : type.GetCustomAttributes(attrType, scanBaseTypes));
            Debug.Assert(attrs != null, "attrs != null");
            foreach (var attr in attrs)
            {
                List<Attribute> attrList;
                if (!_attributes.TryGetValue(attr.GetType(), out attrList))
                {
                    attrList = new List<Attribute>();
                    _attributes.Add(attr.GetType(), attrList);
                }
                attrList.Add(attr as Attribute);
            }
        }

        /// <summary>
        /// Gets the value of the specified attribute
        /// </summary>
        /// <typeparam name="TAttr">Attribute type</typeparam>
        /// <returns>
        /// The specified attribute instance
        /// </returns>
        public TAttr Single<TAttr>()
            where TAttr: class
        {
            List<Attribute> attrs;
            if (!_attributes.TryGetValue(typeof (TAttr), out attrs))
            {
                throw new KeyNotFoundException(
                    String.Format("The specified {0} attribute type cannot be found in {1}",
                    typeof(TAttr), OwnerType));
            }
            if (attrs.Count != 1)
            {
                throw new InvalidOperationException(
                    String.Format("More than one {0} attribute instance found in {1}",
                    typeof(TAttr), OwnerType));
            }
            return attrs[0] as TAttr;
        }

        /// <summary>
        /// Gets the value of the specified optional attribute
        /// </summary>
        /// <typeparam name="TAttr">Attribute type</typeparam>
        /// <returns>
        /// The specified attribute instance
        /// </returns>
        public TAttr Optional<TAttr>(TAttr defaultValue = null)
            where TAttr : class
        {
            List<Attribute> attrs;
            if (!_attributes.TryGetValue(typeof(TAttr), out attrs))
            {
                return defaultValue;
            }
            if (attrs.Count != 1)
            {
                throw new InvalidOperationException(
                    String.Format("More than one {0} attribute instance found in {1}",
                    typeof(TAttr), OwnerType));
            }
            return attrs[0] as TAttr;
        }
    }
}