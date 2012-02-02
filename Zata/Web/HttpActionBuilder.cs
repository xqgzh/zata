using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Web.Protocols;
using Zata.Dynamic;
using System.Reflection;
using Zata.Web.Actions;

namespace Zata.Web
{
    /// <summary>
    /// Web方法代理
    /// </summary>
    public class HttpActionBuilder : ActionBuilder
    {
        /// <summary>
        /// WebContext
        /// </summary>
        protected List<Type> HttpProtocolList = new List<Type>();

        /// <summary>
        /// HttpAction列表
        /// </summary>
        protected List<Type> HttpActionList = new List<Type>();

        public HttpActionBuilder()
        {
            HttpProtocolList.Add(typeof(BasicHttpMethodProtocol));
            base.ActionTypeList.Insert(0, typeof(HttpCacheAction));
        }


        public HttpActionRequest FindMethod(HttpContext httpContext)
        {
            HttpActionRequest request = new HttpActionRequest();

            //添加协议支持
            foreach (var protocolType in HttpProtocolList)
            {
                AbstractHttpProtocol protocol = Activator.CreateInstance(protocolType) as AbstractHttpProtocol;

                string MethodKey = protocol.FindActionName(httpContext.Request);

                if (!string.IsNullOrEmpty(MethodKey))
                {
                    IAction action = FindAction(MethodKey);

                    if (action != null)
                    {
                        request.Context = protocol.GetActionContext(httpContext, action);
                        request.Action = action;
                        request.Protocol = protocol;
                        request.IsHandlResponse = protocol.IsHandlResponse(httpContext);
                    }
                }
            }

            //添加其他类型的Action
            if (request.Action == null || request.Context == null || request.Protocol == null)
                return null;

            InitHttpAction(request);

            return request;
        }

        HttpActionRequest InitHttpAction(HttpActionRequest request)
        {
            IHttpAction httpAction = request.Action as IHttpAction;

            if (httpAction != null)
            {
                httpAction.InitContext(request);
            }

            //foreach (Type actionType in HttpActionList)
            //{
            //    IHttpAction action = base.CreateAction<IHttpAction>(actionType, package.Action.Proxy.TypeAtrributes, package.Action.Proxy.MethodAttributes);

            //    if (action == null)
            //        continue;

            //    action.InitContext(package);
            //}

            return request;
        }



        [ThreadStatic]
        public static HttpActionRequest ActionRequest;

    }
}
