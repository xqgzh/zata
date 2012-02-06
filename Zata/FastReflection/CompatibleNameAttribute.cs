using System;

namespace Zata.FastReflection
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class CompatibleNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public CompatibleNameAttribute(string name)
        {
            Name = name;
        }
    }
}
