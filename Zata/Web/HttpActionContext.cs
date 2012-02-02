using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zata.Dynamic;
using System.Web;
using System.IO;

namespace Zata.Web
{
    public class HttpActionContext : ActionContext, IDisposable
    {
        public HttpActionContext()
        {

        }

        public HttpActionContext(HttpActionContext context) : base(context)
        {
            HttpContext = context.HttpContext;
            ResponseStream = context.ResponseStream;
        }

        public HttpActionContext(ActionContext context) : base(context)
        {
            HttpActionContext thisContext = context as HttpActionContext;

            if (thisContext != null)
            {
                HttpContext = thisContext.HttpContext;
                ResponseStream = thisContext.ResponseStream;
            }
        }

        public HttpContext HttpContext { get; set; }

        public MemoryStream ResponseStream { get; set; }

        public void Dispose()
        {
            ResponseStream.Dispose();
        }
    }
}
