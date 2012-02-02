using System.Collections.Generic;
using Zata.FastReflection.Accessors;
using Zata.FastReflection.Caching;

namespace Zata.FastReflection
{
    public static class ExtensionMethods
    {
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            return GlobalAccessorMap.Current.FindPropertyAccessor(obj.GetType(), propertyName).GetProperty(obj);
        }

        /// <summary>
        /// 推荐
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyValue<T>(this T obj, string propertyName)
        {
            return GenericAccessorMap<T>.Current.FindPropertyAccessor(obj.GetType(), propertyName).GetProperty(obj);
        }

        /// <summary>
        /// 这个速度更慢
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static P GetPropertyValue<T, P>(this T obj, string propertyName)
        {
            return GenericAccessorMap<T>.Current.FindPropertyAccessor<T, P>(propertyName).GetProperty(obj);
        }

        public static object GetPropertyValue(this IAccessorCacheHost obj, string propertyName)
        {
            return obj.AccessorCache.FindPropertyAccessor(obj.GetType(), propertyName).GetProperty(obj);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            GlobalAccessorMap.Current.FindPropertyAccessor(obj.GetType(), propertyName).SetProperty(obj, value);
        }

        public static void SetPropertyValue<T>(this T obj, string propertyName, object value)
        {
            GenericAccessorMap<T>.Current.FindPropertyAccessor(obj.GetType(), propertyName).SetProperty(obj, value);
        }

        public static void SetPropertyValue<T, P>(this T obj, string propertyName, P value)
        {
            GenericAccessorMap<T>.Current.FindPropertyAccessor<T, P>(propertyName).SetProperty(obj, value);
        }

        public static void SetPropertyValue(this IAccessorCacheHost obj, string propertyName, object value)
        {
            obj.AccessorCache.FindPropertyAccessor(obj.GetType(), propertyName).SetProperty(obj, value);
        }

        public static object GetPropertyValueByReflection<T>(this T obj, string propertyName)
        {
            return obj.CallByReflection("get_" + propertyName, new object[0]);
        }

        public static object SetPropertyValueByReflection<T>(this T obj, string propertyName, object value)
        {
            return obj.CallByReflection("set_" + propertyName, new[] { value });
        }

        public static object CallByReflection<T>(this T obj, string methodName, object[] parameters)
        {
            var methodInfo = obj.GetType().GetMethod(methodName);
            if (methodInfo == null)
                throw new KeyNotFoundException();

            return methodInfo.Invoke(obj, parameters);
        }
    }
}
