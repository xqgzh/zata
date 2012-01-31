using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.Dynamic
{
    /// <summary>
    /// 抽象方法的自定义属性基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class AbstractMethodAttribute : Attribute, IAction
    {
        /// <summary>
        /// 下一个待执行的Action
        /// </summary>
        IAction NextAction;

        /// <summary>
        /// 业务代理对象
        /// </summary>
        public MethodWrapper Proxy { get; protected set; }

        IAction IAction.Init(MethodWrapper methodWrapper, IAction nextAction)
        {
            Proxy = methodWrapper;
            NextAction = nextAction;

            Config();

            return this;
        }

        public virtual void Execute(ActionContext Context)
        {
            //做自己的事
            if (NextAction != null)
                NextAction.Execute(Context);
            else if (Proxy != null)
                Proxy.Execute(Context);
        }

        protected abstract bool Config();
    }
}
