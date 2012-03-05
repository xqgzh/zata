using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Zata.FastReflection
{
    /// <summary>
    /// Entity对象接口， 用于标识某个对象属于实体类
    /// </summary>
    public interface IEntity
    {
    }

    public interface IEntity<T> : IEntity
    {

    }
}
