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
    public abstract class HttpMethodProtocol : ActionContext
    {
        public HttpContext WebContext;

        protected IAction CurrentAction;

        protected HttpMethodCacheAttribute CacheProvider;

        public bool IsSkipResponse { get; set; }

        #region 检查是否能够处理当前Http上下文

        /// <summary>
        /// 检查是否能够处理当前Http上下文
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public string Accept(HttpContext httpContext)
        {
            string s = GetMethodKey(httpContext.Request);


            if (!string.IsNullOrEmpty(s))
                WebContext = httpContext;
            return s;
        }

        /// <summary>
        /// 获取方法名称
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        protected abstract string GetMethodKey(HttpRequest httpRequest);

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public HttpMethodProtocol Init(IAction action)
        {
            CurrentAction = action;

            Config();

            //HttpWriter x = WebContext.Response.GetType().GetField("_httpWriter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(WebContext.Response) as HttpWriter;

            //FieldInfo filedStream = x.GetType().GetField("_stream", BindingFlags.NonPublic | BindingFlags.Instance);

            //Stream stream = filedStream.GetValue(x) as Stream;

            //HttpResponseFilter filter = new HttpResponseFilter(stream);

            //filedStream.SetValue(x, filter);

            ////x.GetType().GetField("_stream").SetValue(x, filter);
            

            //WebContext.Response.Write("!324");


            //检查缓存配置
            foreach (object attr in CurrentAction.Proxy.MethodAttributes)
            {
                CacheProvider = attr as HttpMethodCacheAttribute;

                if (CacheProvider != null)
                {
                    (CacheProvider as IAction).Init(CurrentAction.Proxy, CurrentAction);
                    break;
                }
            }

            return this;
        }

        #endregion

        /// <summary>
        /// 执行Action
        /// </summary>
        public virtual void ExecuteAction()
        {
            if (CacheProvider != null)
            {
                //当前方法需要缓存, 执行缓存, 从缓存中加载相关信息
                CacheProvider.Execute(this);
            }
            else
            {
                CurrentAction.Execute(this);
            }
        }

        public void FormatResponse()
        {
            if (CacheProvider != null)
            {
                HttpMethodResult WebResult = Result as HttpMethodResult;

                if (WebResult != null)
                {
                    //缓存已命中
                    Format(WebResult);
                }
                else
                {
                    WebResult = new HttpMethodResult();
                    //缓存未命中

                    Response();

                    //WebResult.ByteStream = filter.ToByteArray();

                    //CacheProvider.AddCache(WebResult);
                    Format(WebResult);

                    CacheProvider.AddCache(this, WebResult);
                }
            }
            else
            {
                //如果没有的话, 则直接执行
                Response();
            }
        }

        void Format(HttpMethodResult WebResult)
        {
            if(WebResult != null && WebResult.ByteStream != null)
            WebContext.Response.OutputStream.Write(WebResult.ByteStream, 0, WebResult.ByteStream.Length);
        }

        /// <summary>
        /// 格式化输出
        /// </summary>
        protected abstract void Response();

        /// <summary>
        /// 配置上下文
        /// </summary>
        protected abstract void Config();
    }
}
