using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zata.Dynamic;
using System.Web.Caching;

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
                    //缓存未命中
                    Response();

                    //CacheProvider.AddCache(WebResult);

                    Format(WebResult);
                }
            }
            else
            {
                //如果没有的话, 则直接执行
                Response();
            }
        }

        private void Format(HttpMethodResult CacheResult)
        {
            throw new NotImplementedException();
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
