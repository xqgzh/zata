using System;

namespace Zata.FastReflection
{
    /// <summary>
    /// Entity属性, 用于定义字段的别名
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class EntityAliasAttribute : Attribute
    {
        public string Name { get; private set; }

        /// <summary>
        /// 自定义的方法名称， 要求必须在当前类或父类之上定义的Action&lt;object&gt;
        /// </summary>
        public string SetMethod;

        public string GetMethod;

        public EntityAliasAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="setMethod"></param>
        /// <param name="getMethod"></param>
        public EntityAliasAttribute(string name, string setMethod, string getMethod)
        {
            Name = name;
            SetMethod = setMethod;
            GetMethod = getMethod;
        }
    }
}
