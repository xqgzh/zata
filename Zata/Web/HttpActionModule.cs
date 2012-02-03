using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Zata.Dynamic;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;

namespace Zata.Web
{
    /// <summary>
    /// 此类型用于通过HTTP模块加载的方式构造方法调用链
    /// </summary>
    public class HttpActionModule : IHttpModule
    {
        public static HttpActionBuilder MethodBuilder = new HttpActionBuilder();

        #region IHttpModule && HttpApplication事件

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(BeginRequest);
            context.EndRequest += new EventHandler(EndRequest);
        }


        void BeginRequest(object sender, EventArgs e)
        {
            HttpApplication httpApplication = (HttpApplication)sender;
            HttpContext httpContext = httpApplication.Context;

            //处理多语言问题
            InitCulture(httpContext.Request);

            HttpActionBuilder.ActionRequest = MethodBuilder.FindMethod(httpContext);

            if (HttpActionBuilder.ActionRequest != null)
            {
                HttpActionBuilder.ActionRequest.ExecuteAction(HttpActionBuilder.ActionRequest.Context);
                
                //根据上下文决定是否由框架处理请求
                if (HttpActionBuilder.ActionRequest.IsHandlResponse)
                    httpApplication.CompleteRequest();
            }
        }

        void EndRequest(object sender, EventArgs e)
        {
            try
            {
                HttpActionRequest actionRequest = HttpActionBuilder.ActionRequest;

                if (actionRequest != null && actionRequest.IsHandlResponse)
                {
                    HttpApplication httpApplication = (HttpApplication)sender;

                    HttpContext httpContext = httpApplication.Context;

                    actionRequest.Response(httpContext);
                }
            }
            catch(Exception ex)
            {
                Trace.Write(ex);
            }
        }

        #endregion

        #region 国际化支持

        /// <summary>
        /// 将参数传递的区域参数转换为当前线程区域性设置
        /// </summary>
        /// <param name="request"></param>
        void InitCulture(HttpRequest request)
        {
            string strCulture = request["lo"] ?? request["language"];

            if (string.IsNullOrEmpty(strCulture) == false)
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(strCulture);
                    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
                }
                catch { }
            }
        }

        #endregion
    }
}
