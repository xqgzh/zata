using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.Dynamic
{
    public class ActionContext
    {


        /// <summary>
        /// 动态方法所存在的实例, 如果为null, 则使用内置实例, 对于静态函数, 可以不需要此实例
        /// </summary>
        public object oInstance;

        /// <summary>
        /// 带顺序的参数列表
        /// </summary>
        public object[] Arguments;

        /// <summary>
        /// 执行结果
        /// </summary>
        public object Result;

        public ActionContext() { }

        public ActionContext(ActionContext context)
        {
            oInstance = context.oInstance;
            Arguments = context.Arguments;
            Result = context.Result;
        }
    }
}
