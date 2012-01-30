using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zata.Dynamic
{
    public class ParamInfo
    {
        static readonly Type IConvertibleType = typeof(IConvertible);

        public ParameterInfo ParameterInfo;
        public Type ParameterType;
        public Type[] Interfaces;
        public object[] Attributes;
        public bool IsEnum;
        public bool IsIConvertible;
        public string Name;
        public RestParameterAttribute Setting;

        public ParamInfo()
        {
        }

        public ParamInfo(ParameterInfo info)
        {
            ParameterInfo = info;
            ParameterType = info.ParameterType;
            Interfaces = ParameterType.GetInterfaces();
            Attributes = info.GetCustomAttributes(false);
            IsEnum = ParameterType.IsEnum;
            Name = info.Name;

            foreach (object o in Attributes)
            {
                if (o is RestParameterAttribute)
                    Setting = (RestParameterAttribute)o;
            }

            foreach (Type t in Interfaces)
            {
                if (t == IConvertibleType)
                {
                    IsIConvertible = true;
                }
            }
        }
    }
}
