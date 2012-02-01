using System;

namespace Zata.FastReflection.Caching
{
    public class GlobalAccessorKeyStrategy : IAccessorKeyStrategy
    {
        private GlobalAccessorKeyStrategy() { }
        private static GlobalAccessorKeyStrategy instance = new GlobalAccessorKeyStrategy();

        public static GlobalAccessorKeyStrategy Instance
        {
            get { return instance; }
        }

        #region IAccessorKeyStrategy Members

        public int GetAccessorKey(Type type, string name)
        {
            return (type.FullName + name).GetHashCode();
        }

        #endregion
    }
}
