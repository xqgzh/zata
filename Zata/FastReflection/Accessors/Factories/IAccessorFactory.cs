using System;
using Zata.FastReflection.Accessors.NonGenerics;

namespace Zata.FastReflection.Accessors.Factories
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAccessorFactory
    {
        IPropertyAccessor GeneratePropertyAccessor(Type type, string propertyName);
    }
}
