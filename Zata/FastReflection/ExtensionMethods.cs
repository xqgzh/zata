using System;
using System.Collections.Generic;
using System.Reflection;

namespace Zata.FastReflection
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static Type GetPropertyOrFieldType(this Type type, string memberName)
        {
            Type memberType = null;
            var propertyInfo = type.GetProperty(memberName);
            if (propertyInfo != null)
                memberType = propertyInfo.PropertyType;
            else
            {
                var fieldType = type.GetField(memberName);
                if (fieldType != null)
                    memberType = fieldType.FieldType;
            }

            return memberType;
        }

        public static Type GetReturnType(this MemberInfo info)
        {
            var propertyInfo = info as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.PropertyType;
            else
            {
                var fieldInfo = info as FieldInfo;
                if (fieldInfo != null)
                    return fieldInfo.FieldType;
                else
                    return null;
            }
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
