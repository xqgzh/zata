using System;
using System.Collections.Generic;
using Zata.FastReflection.Caching;
using System.Reflection;

namespace Zata.FastReflection
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExtensionMethods
    {
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            return obj.GetType().FindAccessorCache().FindPropertyAccessor(obj.GetType(), propertyName).GetProperty(obj);
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
            obj.GetType().FindAccessorCache().FindPropertyAccessor(obj.GetType(), propertyName).SetProperty(obj, value);
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

        /// <summary>
        /// 将一个对象的同名值赋值到目标对象，如果对象是Object类型的，请使用非泛型版本
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        public static void CopyValueTo<S, D>(this S obj, D target)
        {
            var sourceType = typeof(S);
            foreach (var memberInfo in sourceType.GetProperties())
            {
                target.SetPropertyValue(memberInfo.Name, obj.GetPropertyValue(memberInfo.Name));
            }
            foreach (var memberInfo in sourceType.GetFields())
            {
                target.SetPropertyValue(memberInfo.Name, obj.GetPropertyValue(memberInfo.Name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyValueTo(this object source, object target)
        {
            var sourceType = source.GetType();
            foreach (var memberInfo in sourceType.GetProperties())
            {
                target.SetPropertyValue(memberInfo.Name, target.GetPropertyValue(memberInfo.Name));
            }
            foreach (var memberInfo in sourceType.GetFields())
            {
                target.SetPropertyValue(memberInfo.Name, target.GetPropertyValue(memberInfo.Name));
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
