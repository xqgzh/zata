using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.FastReflection
{
    public abstract class AbstractEntity<T> where T : AbstractEntity<T>
    {
        private static Func<T, string, object> GetValueX;
        private static Action<T, string, object> SetValueX;

        static AbstractEntity()
        {
            GetValueX = EntityToolsInternal.GetValueFunction<T>();
            SetValueX = EntityToolsInternal.SetValueFunction<T>();
        }

        public object GetValueEntity(string name)
        {
            return GetValueX(this as T, name);
        }

        public void SetValueEntity(string name, object obj)
        {
            SetValueX(this as T, name, obj);
        }

    }
}
