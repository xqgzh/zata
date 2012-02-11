using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.FastReflection
{
    /// <summary>
    /// 提供一个缓存小写字符串HashCode的字典
    /// </summary>
    public static class StringLowerTable
    {
        static Dictionary<int, int> StringUpperDict = new Dictionary<int, int>();

        /// <summary>
        /// 获取输入字符串的小写的hashCode
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetLowerHashCode(string name)
        {
            int i = name.GetHashCode();

            if (StringUpperDict.ContainsKey(i))
                return StringUpperDict[i];

            int j = name.ToLowerInvariant().GetHashCode();
            lock (StringUpperDict)
            {
                if (!StringUpperDict.ContainsKey(i))
                    StringUpperDict.Add(i, j);
            }

            return j;
        }
    }
}
