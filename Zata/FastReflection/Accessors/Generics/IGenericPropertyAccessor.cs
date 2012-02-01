using Zata.FastReflection.Accessors.NonGenerics;

namespace Zata.FastReflection.Accessors.Generics
{
    public interface IGenericPropertyAccessor<T, P> : IPropertyAccessor
    {
        P GetProperty(T obj);

        bool SetProperty(T obj, P value);
    }
}
