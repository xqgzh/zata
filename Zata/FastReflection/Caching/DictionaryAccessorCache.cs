using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zata.FastReflection.Accessors.Factories;
using Zata.FastReflection.Accessors.Generics;
using Zata.FastReflection.Accessors.NonGenerics;

namespace Zata.FastReflection.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public class DictionaryAccessorCache : IAccessorCache
    {
        private Dictionary<int, IPropertyAccessor> accessors = new Dictionary<int, IPropertyAccessor>();

        public IAccessorKeyStrategy KeyStrategy { get; set; }

        public IAccessorFactory AccessorFactory { get; set; }

        public DictionaryAccessorCache(IAccessorFactory factory, IAccessorKeyStrategy keyStrategy)
        {
            AccessorFactory = factory;
            KeyStrategy = keyStrategy;
        }

        public void Regist<T>()
        {
            Regist(typeof(T));
        }

        public void Regist(Type type)
        {
            foreach (var property in type.GetProperties().Cast<MemberInfo>().Concat(type.GetFields().Cast<MemberInfo>()))
            {
                var key = GetAccessorKey(type, property.Name);
                if (!accessors.ContainsKey(key))
                {
                    var propertyAccessor = AccessorFactory.GeneratePropertyAccessor(type, property.Name);
                    if (propertyAccessor != null)
                    {
                        accessors.Add(key, propertyAccessor);
                    }
                }
            }
        }

        public IPropertyAccessor FindPropertyAccessor(Type type, string propertyName)
        {
            var key = GetAccessorKey(type, propertyName);
            IPropertyAccessor accessor = null;
            accessors.TryGetValue(key, out accessor);

            return accessor;
        }

        public IGenericPropertyAccessor<T, P> FindPropertyAccessor<T, P>(string propertyName)
        {
            var key = GetAccessorKey(typeof(T), propertyName);
            IPropertyAccessor accessor = null;
            accessors.TryGetValue(key, out accessor);

            return accessor as IGenericPropertyAccessor<T, P>;
        }

        private int GetAccessorKey(Type type, string propertyName)
        {
            return KeyStrategy.GetAccessorKey(type, propertyName);
        }
    }
}
