using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace Zata.FastReflection
{
    public class EntityTools<T>
    {
        public static readonly Func<T, string, object> GetValue;
        public static readonly Action<T, string, object> SetValue;
        public static readonly Func<T, string, object> GetValueIgnoreCase;
        public static readonly Action<T, string, object> SetValueIgnoreCase;

        static EntityTools()
        {
            GetValue = EntityToolsInternal.GetValueFunction<T>();
            SetValue = EntityToolsInternal.SetValueFunction<T>();
            GetValueIgnoreCase = EntityToolsInternal.GetValueFunction<T>(true);
            SetValueIgnoreCase = EntityToolsInternal.SetValueFunction<T>(true);
        }
    }

}
