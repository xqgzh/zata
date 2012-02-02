using System;
using System.Linq;
using Zata.FastReflection.Accessors.Generics;
using Zata.FastReflection.Accessors.NonGenerics;

namespace Zata.FastReflection.Accessors.Factories
{
    public class GenericAccessorFactory : IAccessorFactory
    {
        #region IAccessorFactory Members

        public IPropertyAccessor GeneratePropertyAccessor(Type type, string propertyName)
        {
            Type memberType = null;
            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo != null)
                memberType = propertyInfo.PropertyType;
            else
            {
                var fieldType = type.GetField(propertyName);
                if (fieldType != null)
                    memberType = fieldType.FieldType;
            }

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
