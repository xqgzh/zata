using Zata.FastReflection.Accessors;

namespace Zata.FastReflection.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAccessorCacheHost
    {
        IAccessorCache AccessorCache { get; }
    }
}
