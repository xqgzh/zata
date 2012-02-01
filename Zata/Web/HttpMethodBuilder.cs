using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Web.Basic;
using Zata.Dynamic;
using System.Reflection;

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
        protected List<Type> HttpProtocolList = new List<Type>();

        public HttpMethodBuilder()
        {
            HttpProtocolList.Add(typeof(BasicHttpMethodProtocol));
        }


        public HttpMethodProtocol FindMethod(HttpContext httpContext)
        {
            //添加协议支持
            foreach (var protocolType in HttpProtocolList)
            {
                HttpMethodProtocol protocol = Activator.CreateInstance(protocolType) as HttpMethodProtocol;

                string MethodKey = protocol.Accept(httpContext);

                if (!string.IsNullOrEmpty(MethodKey))
                {
                    IAction action = FindAction(MethodKey);

                    if (action != null)
                    {
                        protocol.Init(action);

                        return protocol;
                    }
                }
            }

            //添加其他类型的Action
            return null;
        }

        [ThreadStatic]
        public static HttpMethodProtocol CurrentProtocol;

    }
}
