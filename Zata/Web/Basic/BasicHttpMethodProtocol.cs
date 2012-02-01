using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Dynamic;

namespace Zata.Web.Basic
{
    class BasicHttpMethodProtocol : HttpMethodProtocol
    {
        protected override void Response()
        {
            new BasicHttpMethodResponse().ProcessResponse(WebContext, this);
        }

        protected virtual void InitArguments()
        {
            Arguments = new object[CurrentAction.Proxy.Parameters.Length];
            for (int i = 0, j = CurrentAction.Proxy.Parameters.Length; i < j; i++)
            {
                var p = CurrentAction.Proxy.Parameters[i];
                string para = WebContext.Request[p.Name];

                Arguments[i] = para;
            }
        }

        protected override void Config()
        {
            //初始化参数
            InitArguments();

            //装配Action
            IsSkipResponse = IsWebMethodHandleResponse(WebContext);
        }

        #region 通过HTTP上下文获取方法名称

        static string RestPrefix = "/Rest/";
        static string[] MethodNameKeys = { "ActionID", "method", "methodname", "m" };
        static string[] RegistedHandlerPaths = { "/resthandler.ashx" };

        /// <summary>
        /// 通过HTTP上下文获取方法名称
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        protected override string GetMethodKey(HttpRequest httpRequest)
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
        protected static bool IsWebMethodHandleResponse(HttpContext httpContext)
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
