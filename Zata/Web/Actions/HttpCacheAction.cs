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

        protected IAction NextAction;

        /// <summary>
        /// 默认过期时间
        /// </summary>
        public int DefaultExpireSeconds = 30;

        /// <summary>
        /// 缓存键设置
        /// </summary>
        protected string CacheKeyFormater = string.Empty;

        #region IHttpAction Members

        public virtual void InitContext(HttpActionRequest request)
        {
            //这里处理检查是否缓存
            //这里处理检查是否缓存
            string CacheKey = request.Protocol.GetType().Name + string.Format(CacheKeyFormater, request.Context.Arguments);

            HttpActionContext cacheContext = cacheManager.Get(CacheKey) as HttpActionContext;

            if (cacheContext != null)
            {
                request.Context = cacheContext;
            }
            else
                request.Context.CacheKey = CacheKey;

            IHttpAction httpAction = NextAction as IHttpAction;

            if (httpAction != null)
                httpAction.InitContext(request);
        }

        public virtual void Response(HttpActionContext context)
        {
            if (context != null && context.IsCached == false && !string.IsNullOrEmpty(context.CacheKey) )
            {
                context.IsCached = true;

                cacheManager.Add(
                    context.CacheKey,
                    context, 
                    null, DateTime.Now.AddSeconds(this.DefaultExpireSeconds), TimeSpan.Zero, CacheItemPriority.Default, null);
            }

            IHttpAction httpAction = NextAction as IHttpAction;

            if (httpAction != null)
                httpAction.Response(context);
        }

        #endregion

        #region IAction Members

        public MethodWrapper Proxy { get; set; }

        public virtual IAction Init(MethodWrapper methodWrapper, IAction nextAction)
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

        public virtual void Execute(ActionContext Context)
        {
            HttpActionContext context = Context as HttpActionContext;
            if (context != null && context.IsCached == false)
            {
                //缓存未命中, 需要执行
                NextAction.Execute(Context);
            }
        }

        #endregion
    }
}
