using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Web.Basic;
using Zata.Dynamic;

namespace Zata.Web
{
    /// <summary>
    /// Web方法代理
    /// </summary>
    public class HttpMethodBuilder : ActionBuilder
    {
        /// <summary>
        /// WebContext
        /// </summary>
        protected List<HttpMethodContext> HttpActionList = new List<HttpMethodContext>();

        /// <summary>
        /// WebContext
        /// </summary>
        protected List<Func<HttpContext, ActionBuilder, HttpMethodContext>> HttpActionList2 = new List<Func<HttpContext, ActionBuilder, HttpMethodContext>>();

        public HttpMethodBuilder()
        {
            //HttpActionList.Add(new Basic.BasicHttpMethodContext());
            HttpActionList2.Add(Basic.BasicHttpMethodContext.Accept);
        }


        public HttpMethodContext FindMethod(HttpContext httpContext)
        {
            HttpActionList2.Select(fun => fun.Method);


            foreach (var contextFinder in HttpActionList2)
            {
                HttpMethodContext CurrentAction = contextFinder(httpContext, this);

                if (CurrentAction != null)
                    return CurrentAction;
            }

            return null;
        }

        [ThreadStatic]
        public static HttpMethodContext CurrentAction;
    }
}
