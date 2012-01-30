using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zata.Dynamic
{
    public interface IAction
    {
        MethodWrapper Proxy { get;}

        IAction Init(MethodWrapper methodWrapper, IAction nextAction);

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        void Execute(ActionContext Context);
    }
}
