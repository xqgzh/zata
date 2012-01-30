using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Dynamic;
//using Bambook.Common.Util;
//using Bambook.Common.Rest;

namespace Zata.Web.Basic
{
    class BasicHttpMethodContext : HttpMethodContext
    {
        public static HttpMethodContext Accept(HttpContext httpContext, ActionBuilder actionBuilder)
        {
            //标准HTTP调用
            string methodKey = GetMethodKey(httpContext.Request);

            if (string.IsNullOrEmpty(methodKey))
                return null;

            BasicHttpMethodContext CurrentContext = new BasicHttpMethodContext();
            CurrentContext.HttpContext = httpContext;
            CurrentContext.CurrentAction = actionBuilder.FindAction(methodKey);

            //检查是否能找到对应的方法代理
            if (CurrentContext.CurrentAction == null)
                return null;

            //初始化参数
            CurrentContext.InitArguments();

            CurrentContext.IsRenderView = IsWebMethodHandleResponse(CurrentContext.HttpContext);

            return CurrentContext;
        }

        public override void RenderView()
        {
            new BasicHttpMethodResponse().ProcessResponse(HttpContext, this);
        }

        protected virtual void InitArguments()
        {
            Arguments = new object[CurrentAction.Proxy.Parameters.Length];
            for (int i = 0, j = CurrentAction.Proxy.Parameters.Length; i < j; i++)
            {
                var p = CurrentAction.Proxy.Parameters[i];
                string para = HttpContext.Request[p.Name];

                Arguments[i] = para;
            }
        }

        #region 通过HTTP上下文获取方法名称

        static string BambookRest = "/Rest/";
        static string[] MethodNameKeys = { "ActionID", "method", "methodname", "m" };
        static string[] RegistedHandlerPaths = { "/resthandler.ashx" };

        /// <summary>
        /// 通过HTTP上下文获取方法名称
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        static string GetMethodKey(HttpRequest httpRequest)
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

                if (httpRequest.Path.StartsWith(BambookRest, StringComparison.OrdinalIgnoreCase))
                    s = httpRequest.Path.Substring(BambookRest.Length).Trim('/');
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
        protected static bool IsWebMethodHandleResponse(HttpContext httpContext)
        {
            

            //如果为虚拟路径, 则由HttpMethod框架处理
            if (string.IsNullOrEmpty(httpContext.Request.CurrentExecutionFilePathExtension))
                return true;

            //兼容Bambook逻辑, 检查相关上下文参数
            if (RegistedHandlerPaths.Contains(httpContext.Request.CurrentExecutionFilePath.ToLower()))
                return true;

            return false;
        }

        #endregion
    }
}
