using System;

namespace Zata.FastReflection.Caching
{
    public interface IAccessorKeyStrategy
    {
        int GetAccessorKey(Type type, string name);
    }
}
