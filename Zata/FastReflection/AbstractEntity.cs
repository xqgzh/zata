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
            GetValueX = EntityTools<T>.GetValueFunction();
            SetValueX = EntityTools<T>.SetValueFunction();
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
