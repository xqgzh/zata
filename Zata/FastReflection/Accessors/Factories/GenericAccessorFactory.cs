using System;
using Zata.FastReflection.Accessors.Generics;
using Zata.FastReflection.Accessors.NonGenerics;

namespace Zata.FastReflection.Accessors.Factories
{
    public class GenericAccessorFactory : IAccessorFactory
    {
        #region IAccessorFactory Members

        public IPropertyAccessor GeneratePropertyAccessor(Type type, string propertyName)
        {
            Type accessorType = typeof(GenericPropertyAccessor<,>)
                .MakeGenericType(type, type.GetProperty(propertyName).PropertyType);

            return Activator.CreateInstance(accessorType, propertyName) as IPropertyAccessor;
        }

        #endregion
    }
}
