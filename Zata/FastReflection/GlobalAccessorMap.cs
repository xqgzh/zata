using Zata.FastReflection.Accessors.Factories;
using Zata.FastReflection.Caching;

namespace Zata.FastReflection
{
    public static class GlobalAccessorMap
    {
        private static IAccessorCache map = new DictionaryAccessorCache(new GenericAccessorFactory(), GlobalAccessorKeyStrategy.Instance);

        public static IAccessorCache Current
        {
            get { return map; }
        }
    }
}
