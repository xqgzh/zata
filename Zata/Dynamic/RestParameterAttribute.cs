using System;
using System.Collections.Generic;
using System.Text;

namespace Zata.Dynamic
{
    /// <summary>
    /// Rest类属性实体
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RestParameterAttribute : System.Attribute
    {
        /// <summary>
        /// 是否必须
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Rest参数属性构造函数
        /// </summary>
        /// <param name="isRequired"></param>
        public RestParameterAttribute(bool isRequired)
        {
            this.IsRequired = isRequired;
        }
    }
}
