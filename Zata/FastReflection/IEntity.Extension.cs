using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.FastReflection
{
    public static class IEntityExtension
    {
        public static object GetEntityValue<T>(this IEntity<T> entity, string name)
        {
            return EntityTools<T>.GetValue((T)entity, name);
        }

        public static void SetEntityValue<T>(this IEntity<T> entity, string name, object obj)
        {
            EntityTools<T>.SetValue((T)entity, name, obj);
        }
    }

}
