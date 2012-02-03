using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Dynamic;

namespace Zata.Web.Protocols
{
    class BasicHttpMethodProtocol : AbstractHttpProtocol
    {
        #region 格式化输出

        /// <summary>
        /// 格式化输出
        /// </summary>
        /// <param name="context"></param>
        public override void Response(HttpContext httpContext, HttpActionContext context)
        {
            new BasicHttpMethodResponse().ProcessResponse(httpContext, context);
        }

        #endregion

        #region 配置

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HttpActionContext GetActionContext(HttpContext httpContext, IAction action)
        {
            HttpActionContext context = base.GetActionContext(httpContext, action);
            MethodWrapper proxy = action.Proxy;

            //初始化参数
            context.Arguments = new object[proxy.Parameters.Length];
            for (int i = 0, j = proxy.Parameters.Length; i < j; i++)
            {
                var p = proxy.Parameters[i];
                string para = httpContext.Request[p.Name];

                context.Arguments[i] = para;
            }

            return context;
        }

        #endregion

        #region 通过HTTP上下文获取方法名称

        static string RestPrefix = "/Rest/";
        static string[] MethodNameKeys = { "ActionID", "method", "methodname", "m" };
        static string[] RegistedHandlerPaths = { "/resthandler.ashx" };

        /// <summary>
        /// 通过HTTP上下文获取方法名称
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public override string FindActionName(HttpRequest httpRequest)
        {
            string s = string.Empty;

            //首先判断是否文件访问, 如果是文件访问, 则无需从路径中查询参数
            if (string.IsNullOrEmpty(httpRequest.CurrentExecutionFilePathExtension) == false)
            {
                //文件访问, 根据Get方式处理流程
                //首先处理兼容模式, 检查ActionID的问题
                foreach (var m in MethodNameKeys)
                {
                    s = httpRequest[m];

                    if (string.IsNullOrEmpty(s) == false)
                        break;
                }
            }
            else
            {
                //路径模式

                if (httpRequest.Path.StartsWith(RestPrefix, StringComparison.OrdinalIgnoreCase))
                    s = httpRequest.Path.Substring(RestPrefix.Length).Trim('/');
                else
                    s = httpRequest.Path.Trim('/');

            }

            return s;
        }

        #endregion

        #region 检查是否已经存在输出结果处理器

        /// <summary>
        /// 检查是否已经存在输出结果处理器
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="httpRequest"></param>
        /// <param name="httpResponse"></param>
        /// <remarks>此时已经找到Web方法并且执行成功, 只需要进行格式化即可, 不属于框架控制返回之内的路径, 不会调用此方法</remarks>
        /// <returns></returns>
        public override bool IsHandlResponse(HttpContext httpContext)
        {
            //如果为虚拟路径, 则由HttpMethod框架处理
            if (string.IsNullOrEmpty(httpContext.Request.CurrentExecutionFilePathExtension))
                return true;

            //检查相关上下文参数
            if (RegistedHandlerPaths.Contains(httpContext.Request.CurrentExecutionFilePath.ToLower()))
                return true;

            return false;
        }

        #endregion
    }
}
