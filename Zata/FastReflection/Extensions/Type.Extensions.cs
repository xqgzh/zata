using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zata.FastReflection.Extensions
{
    static class TypeExtensions
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
    }
}
