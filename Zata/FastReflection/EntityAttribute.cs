using System;

namespace Zata.FastReflection
{
    /// <summary>
    /// Entity属性, 用于定义字段的别名
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class EntityAttribute : Attribute
    {
        public string Name { get; private set; }

        public EntityAttribute(string name)
        {
            Name = name;
        }
    }
}
