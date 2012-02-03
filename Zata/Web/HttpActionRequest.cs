using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Dynamic;
using System.Web.Caching;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Zata.Web
{
    public class HttpActionRequest
    {
        /// <summary>
        /// 协议上下文
        /// </summary>
        public HttpActionContext Context;

        /// <summary>
        /// 当前Action方法
        /// </summary>
        public IAction Action;

        public AbstractHttpProtocol Protocol;

        /// <summary>
        /// 定义是否需要执行Response
        /// </summary>
        public bool IsHandlResponse { set; get; }

        #region 执行Action

        /// <summary>
        /// 执行Action
        /// </summary>
        public void ExecuteAction(HttpActionContext context)
        {
            Action.Execute(context);
        }

        #endregion

        #region 格式化输出

        /// <summary>
        /// 格式化当前对结果, 写入到HttpResponse
        /// </summary>
        public bool Response(HttpContext httpContext)
        {
            if (Context != null && Context.Result != null && Protocol != null)
            {
                IHttpAction httpAction = Action as IHttpAction;

                if (httpAction != null)
                    httpAction.Response(Context);


                if (Context.ResponseWrapper == null)
                {
                    Protocol.Response(httpContext, Context);
                }

                if (Context.ResponseWrapper != null)
                {
                    Context.ResponseWrapper.Flush(httpContext.Response);
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}
