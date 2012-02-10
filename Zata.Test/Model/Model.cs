using System;
using Zata.FastReflection;

namespace Zata.Test
{
    /// <summary>
    /// 
    /// </summary>
    public class DataObjectModel : DynamicModel, IDataObject, ICloneable, IEntity<DataObjectModel>
    {
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
                case "NameField": NameField = Convert.ToString(dataValue); break;
                case "KeyField": KeyField = Convert.ToString(dataValue); break;
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
                case "NameField": return NameField;
                case "KeyField": return KeyField;
                default:
                    return null;
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public class DynamicModel
    {
        [Entity("Prop")]
        public string Property { get; set; }

        public string Name { get; set; }

        public string Key { get; set; }

        public string Namespace { get; set; }

        public string Hash { get; set; }

        public string ReferenceCount { get; set; }

        public string NameField;

        public string KeyField;
    }
}
