using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Zata.Dynamic;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using Zata.Web.Basic;

namespace Zata.Web
{
    /// <summary>
    /// 此类型用于通过HTTP模块加载的方式构造方法调用链
    /// </summary>
    public class HttpMethodModule : IHttpModule
    {
        public static HttpMethodBuilder MethodBuilder = new HttpMethodBuilder();

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

            HttpMethodBuilder.CurrentProtocol = MethodBuilder.FindMethod(httpContext);

            if (HttpMethodBuilder.CurrentProtocol != null)
            {
                HttpMethodBuilder.CurrentProtocol.ExecuteAction();
                
                //根据上下文决定是否由框架处理请求
                if (HttpMethodBuilder.CurrentProtocol.IsSkipResponse)
                    httpApplication.CompleteRequest();
            }
        }

        void EndRequest(object sender, EventArgs e)
        {
            try
            {
                HttpApplication httpApplication = (HttpApplication)sender;

                HttpContext httpContext = httpApplication.Context;
                HttpRequest httpRequest = httpApplication.Request;
                HttpResponse httpResponse = httpApplication.Response;

                HttpMethodProtocol httpAction = HttpMethodBuilder.CurrentProtocol;

                if (httpAction != null && httpAction.IsSkipResponse)
                {
                    httpAction.Result = Convert.ToString(httpAction.Result) + DateTime.Now.Second;
                    httpAction.FormatResponse();
                }
            }
            catch
            {

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
