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
            var memberType = type.GetPropertyOrFieldType(propertyName);
            if (memberType != null)
            {
                Type accessorType = typeof(GenericPropertyAccessor<,>).MakeGenericType(type, memberType);

                return Activator.CreateInstance(accessorType, propertyName) as IPropertyAccessor;
            }
            else
                return null;
        }

        #endregion
    }
}
