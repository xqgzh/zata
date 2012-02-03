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
        public string CacheKey;

        public bool IsCached = false;

        public HttpActionResponse ResponseWrapper { get; set; }

        public HttpActionContext()
        {

        }

        public HttpActionContext(HttpActionContext context) : base(context)
        {
            ResponseWrapper = context.ResponseWrapper;
        }

        public HttpActionContext(ActionContext context) : base(context)
        {
            HttpActionContext thisContext = context as HttpActionContext;

            if (thisContext != null)
            {
                ResponseWrapper = thisContext.ResponseWrapper;
            }
        }

        public void Dispose()
        {
            if(ResponseWrapper != null)
                ResponseWrapper.Dispose();
        }
    }
}
