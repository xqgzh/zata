using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Dynamic;

namespace Zata.Web
{
    public abstract class HttpMethodProtocol : ActionContext, IAction
    {
        protected IAction NextAction;

        public HttpContext HttpContext;

        protected Exception Error;

        public bool IsFormat { get; set; }

        public abstract void Format();

        public MethodWrapper Proxy { get; set; }

        protected abstract void Config();

        public IAction Init(MethodWrapper methodWrapper, IAction nextAction)
        {
            Proxy = methodWrapper;
            NextAction = nextAction;

            Config();

            return this;
        }

        public virtual void Execute(ActionContext Context)
        {
            if (NextAction != null)
                NextAction.Execute(Context);
            else if (Proxy != null)
                Proxy.Execute(Context);
        }

        public string Accept(System.Web.HttpContext httpContext)
        {
            string s = GetMethodKey(httpContext.Request);

            if(!string.IsNullOrEmpty(s))
                HttpContext = httpContext;
            return s;
        }

        protected abstract string GetMethodKey(System.Web.HttpRequest httpRequest);
    }
}
