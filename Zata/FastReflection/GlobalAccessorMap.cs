using System;
using System.Collections.Generic;
using System.Reflection;
using Zata.FastReflection.Caching;

namespace Zata.FastReflection
{
    public static class GlobalAccessorMap
    {
        private static Func<Type, Type[], Type> makeGTDelegate = null;
        private static Func<PropertyInfo, object, object[], object> getValueDelegate = null;
        private static Dictionary<int, IAccessorCache> genericAccessorCacheMap = new Dictionary<int, IAccessorCache>();

        static GlobalAccessorMap()
        {
            var typeType = typeof(Type);
            var makeGTInfo = typeType.GetMethod("MakeGenericType");
            makeGTDelegate = Delegate.CreateDelegate(typeof(Func<Type, Type[], Type>), makeGTInfo) as Func<Type, Type[], Type>;

            var propertyType = typeof(PropertyInfo);
            var getValueInfo = propertyType.GetMethod("GetValue", new [] { typeof(object), typeof(object[]) });
            getValueDelegate = Delegate.CreateDelegate(typeof(Func<PropertyInfo, object, object[], object>), getValueInfo) as Func<PropertyInfo, object, object[], object>;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IAccessorCache FindAccessorCache(this Type type)
        {
            IAccessorCache value = null;
            if (!genericAccessorCacheMap.TryGetValue(type.GetHashCode(), out value))
            {
                var accessorType = makeGTDelegate(typeof(GenericAccessorMap<>), new[] { type });
                var currentValue = accessorType.GetProperty("Current");
                if (currentValue != null)
                {
                    value = getValueDelegate(currentValue, null, null) as IAccessorCache;

                    genericAccessorCacheMap.Add(type.GetHashCode(), value);
                }
                else
                    return null;
            }

            return value;
        }
    }
}
