using System;
using Zata.FastReflection.Accessors;
using Zata.FastReflection.Accessors.Factories;
using Zata.FastReflection.Caching;

namespace Zata.Test
{
    /// <summary>
    /// 
    /// </summary>
    public class DataObjectModel : DynamicModel, IDataObject
    {
        private static DictionaryAccessorCache cache = new DictionaryAccessorCache(new GenericAccessorFactory(), InClassAccessorKeyStrategy.Instance);
        static DataObjectModel()
        {
            cache.Regist(typeof(DataObjectModel));
        }

        public override IAccessorCache AccessorCache
        {
            get { return cache; }
        }

        #region IDataObject Members

        public bool SetValue(string dataName, object dataValue)
        {
            switch (dataName)
            {
                case "Property": Property = Convert.ToString(dataValue); break;
                case "Name": Name = Convert.ToString(dataValue); break;
                case "Key": Key = Convert.ToString(dataValue); break;
                case "Namespace": Namespace = Convert.ToString(dataValue); break;
                case "Hash": Hash = Convert.ToString(dataValue); break;
                case "ReferenceCount": ReferenceCount = Convert.ToString(dataValue); break;
                default:
                    return false;
            }

            return true;
        }

        public object GetValue(string dataName)
        {
            switch (dataName)
            {
                case "Property": return Property;
                case "Name": return Name;
                case "Key": return Key;
                case "Namespace": return Namespace;
                case "Hash": return Hash;
                case "ReferenceCount": return ReferenceCount;
                default:
                    return null;
            }
        }

        #endregion
    }

    public class DynamicModel : IAccessorCacheHost
    {
        private static DictionaryAccessorCache cache = new DictionaryAccessorCache(new GenericAccessorFactory(), InClassAccessorKeyStrategy.Instance);
        static DynamicModel()
        {
            cache.Regist(typeof(DynamicModel));
        }

        public string Property { get; set; }

        public string Name { get; set; }

        public string Key { get; set; }

        public string Namespace { get; set; }

        public string Hash { get; set; }

        public string ReferenceCount { get; set; }

        #region IAccessorCacheHost Members

        public virtual IAccessorCache AccessorCache
        {
            get { return cache; }
        }

        #endregion
    }
}
