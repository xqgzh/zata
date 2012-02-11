using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace Zata.FastReflection
{
    public abstract class EntityTools<T>
    {
        public static readonly Func<T, string, bool, object> GetValue;
        public static readonly Action<T, string, bool, object> SetValue;
        public static readonly Func<T, string, bool, string> GetValueString;
        public static readonly Action<T, string, bool, string> SetValueString;

        public static int PropertyCount;
        public static int FieldCount;
        public static int FieldOrPropertyCount;

        public static FieldInfo[] Feilds;
        public static PropertyInfo[] Propertys;

        static EntityTools()
        {
            GetValue = EntityToolsInternal.GetValueFunction<T, object>();
            SetValue = EntityToolsInternal.SetValueFunction<T, object>();
            GetValueString = EntityToolsInternal.GetValueFunction<T, string>();
            SetValueString = EntityToolsInternal.SetValueFunction<T, string>();
        }
    }

}
