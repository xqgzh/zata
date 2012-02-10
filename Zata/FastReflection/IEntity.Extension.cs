using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.FastReflection
{
    public static class IEntityExtension
    {
        public static object GetEntityValue<T>(this IEntity<T> entity, string name, bool IgnoreCase)
        {
            return EntityTools<T>.GetValue((T)entity, name, IgnoreCase);
        }

        public static void SetEntityValue<T>(this IEntity<T> entity, string name, bool IgnoreCase, object obj)
        {
            EntityTools<T>.SetValue((T)entity, name, IgnoreCase, obj);
        }
    }

}
