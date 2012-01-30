using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zata.Dynamic
{
    public abstract class AbstractAction : IAction
    {
        /// <summary>
        /// 下一个待执行的Action
        /// </summary>
        IAction NextAction;
        
        /// <summary>
        /// 业务代理对象
        /// </summary>
        public MethodWrapper Proxy { get; set; }

        public abstract bool Config();

        IAction IAction.Init(MethodWrapper methodWrapper, IAction nextAction)
        {
            Proxy = methodWrapper;
            NextAction = nextAction;

            if (!Config())
                return nextAction;



            return this;
        }

        public virtual ActionContext Execute(ActionContext Context)
        {
            //做自己的事
            if(NextAction != null)
                NextAction.Execute(Context);
            else if (Proxy != null)
                Proxy.Execute(Context);

            return null;
        }
    }
}
