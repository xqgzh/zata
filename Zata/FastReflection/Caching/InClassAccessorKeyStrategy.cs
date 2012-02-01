using System;

namespace Zata.FastReflection.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public class InClassAccessorKeyStrategy : IAccessorKeyStrategy
    {
        private InClassAccessorKeyStrategy() { }
        private static InClassAccessorKeyStrategy instance = new InClassAccessorKeyStrategy();

        public static InClassAccessorKeyStrategy Instance
        {
            get { return instance; }
        }

        #region IAccessorKeyStrategy Members

        public int GetAccessorKey(Type type, string name)
        {
            return name.GetHashCode();
        }

        #endregion
    }
}
