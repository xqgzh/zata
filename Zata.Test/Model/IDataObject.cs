using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.Test
{
    /// <summary>
    /// 数据访问接口, 实现此接口的对象,可以通过名称获取/得到成员的值
    /// </summary>
    public interface IDataObject
    {
        /// <summary>
        /// 设置名称为DataName的成员的值为DataValue
        /// </summary>
        /// <param name="DataName">属性名称</param>
        /// <param name="DataValue"></param>
        /// <returns></returns>
        bool SetValue(string DataName, object DataValue);

        /// <summary>
        /// 根据名称获取成员的值
        /// </summary>
        /// <param name="DataName">属性名称</param>
        /// <returns></returns>
        object GetValue(string DataName);
    }
}
