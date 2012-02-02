using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web;
using Zata.Dynamic;

namespace Zata.Web.Actions
{
    /// <summary>
    /// Http缓存实现
    /// </summary>
    /// <remarks>
    /// 用于在确定了protocol之后, 对IAction.Execute和AbstractHttpProtocol.Response进行拦截, 以便处理处理缓存对象
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpCacheAction : Attribute, IHttpAction
    {
        protected static Cache cacheManager = HttpRuntime.Cache;

        private IAction NextAction;

        /// <summary>
        /// 默认过期时间
        /// </summary>
        public int DefaultExpireSeconds = 30;

        /// <summary>
        /// 缓存键设置
        /// </summary>
        string CacheKeyFormater = string.Empty;

        #region IHttpAction Members

        public void InitContext(HttpActionRequest request)
        {
            //这里处理检查是否缓存
            HttpActionCacheContext cacheContext = null;

            string CacheKey = request.Protocol.GetType().Name + string.Format(CacheKeyFormater, request.Context.Arguments);

            cacheContext = cacheManager.Get(CacheKey) as HttpActionCacheContext;

            if (cacheContext == null)
            {
                cacheContext = new HttpActionCacheContext(request.Context);
                cacheContext.CacheKey = CacheKey;
            }
            else
            {
                cacheContext.HttpContext = request.Context.HttpContext;
            }

            request.Context = cacheContext;

            IHttpAction httpAction = NextAction as IHttpAction;

            if (httpAction != null)
                httpAction.InitContext(request);
        }

        public void Response(HttpActionContext context)
        {
            HttpActionCacheContext cacheContext = context as HttpActionCacheContext;

            if (cacheContext != null && cacheContext.IsCached == false)
            {
                cacheContext.IsCached = true;

                cacheManager.Add(
                    cacheContext.CacheKey, 
                    cacheContext, 
                    null, DateTime.Now.AddSeconds(this.DefaultExpireSeconds), TimeSpan.Zero, CacheItemPriority.Default, null);
            }

            IHttpAction httpAction = NextAction as IHttpAction;

            if (httpAction != null)
                httpAction.Response(context);
        }

        #endregion

        #region IAction Members

        public MethodWrapper Proxy { get; set; }

        public IAction Init(MethodWrapper methodWrapper, IAction nextAction)
        {
            Proxy = methodWrapper;

            NextAction = nextAction;

            #region 构造缓存键

            CacheKeyFormater += methodWrapper.methodInfo.Name;


            ParamInfo[] paras = methodWrapper.Parameters;

            for (int i = 0, j = paras.Length; i < j; i++)
            {
                if (CachedKeyAttribute.IsDefined(paras[i].ParameterInfo, typeof(CachedKeyAttribute)))
                {
                    CacheKeyFormater += "_{" + i + "}";
                }
            }

            #endregion

            return this;
        }

        public void Execute(ActionContext Context)
        {
            HttpActionCacheContext cacheContext = Context as HttpActionCacheContext;

            if (cacheContext == null || cacheContext.IsCached == false)
            {
                //缓存未命中, 需要执行
                NextAction.Execute(Context);
            }
        }

        #endregion
    }

    public class HttpActionCacheContext : HttpActionContext
    {
        public string CacheKey;

        public bool IsCached = false;

        public HttpActionCacheContext(HttpActionContext x) : base(x)
        {

        }
    }
}
