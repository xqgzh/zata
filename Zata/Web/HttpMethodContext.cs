using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Dynamic;

namespace Zata.Web
{
    public abstract class HttpMethodContext : ActionContext
    {
        protected IAction CurrentAction;

        protected HttpContext HttpContext;

        protected Exception Error;

        public bool IsRenderView { get; set; }

        public virtual void Execute()
        {
            try
            {
                CurrentAction.Execute(this);
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }

        //public abstract HttpMethodContext Accept(HttpContext httpContext, ActionBuilder actionBuilder);

        public abstract void RenderView();


    }
}
