using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.FastReflection.Extensions
{
    static class ObjectExtensions
    {
        public static object CallByReflection<T>(this T obj, string methodName, object[] parameters)
        {
            var methodInfo = obj.GetType().GetMethod(methodName);
            if (methodInfo == null)
                throw new KeyNotFoundException();

            return methodInfo.Invoke(obj, parameters);
        }

        public static object GetPropertyValueByReflection<T>(this T obj, string propertyName)
        {
            return CallByReflection(obj, "get_" + propertyName, new object[0]);
        }

        public static object SetPropertyValueByReflection<T>(this T obj, string propertyName, object value)
        {
            return CallByReflection(obj, "set_" + propertyName, new[] { value });
        }

    }
}
