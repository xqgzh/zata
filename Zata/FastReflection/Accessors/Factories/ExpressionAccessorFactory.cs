using System;
using Zata.FastReflection.Accessors.NonGenerics;

namespace Zata.FastReflection.Accessors.Factories
{
    public class ExpressionAccessorFactory : IAccessorFactory
    {
        #region IAccessorFactory Members

        public IPropertyAccessor GeneratePropertyAccessor(Type type, string propertyName)
        {
            return new ExpressionPropertyAccessor(type, propertyName);
        }

        #endregion
    }
}
