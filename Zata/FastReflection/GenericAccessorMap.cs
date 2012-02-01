using Zata.FastReflection.Accessors.Factories;
using Zata.FastReflection.Caching;

namespace Zata.FastReflection
{
    public class GenericAccessorMap<T>
    {
        private static IAccessorCache accessorMap = new DictionaryAccessorCache(new GenericAccessorFactory(), InClassAccessorKeyStrategy.Instance);

        static GenericAccessorMap()
        {
            accessorMap.Regist(typeof(T));
        }

        public static IAccessorCache Current
        {
            get { return accessorMap; }
        }
    }
}
