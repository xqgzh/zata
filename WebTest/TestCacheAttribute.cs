using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Zata.Web.Actions;
using Zata.Web;

namespace WebUI
{
    public class TestCacheAttribute : Zata.Web.Actions.HttpCacheAction
    {
        public override void InitContext(Zata.Web.HttpActionRequest request)
        {
            //这里处理检查是否缓存
            string CacheKey = "Test" +  request.Protocol.GetType().Name + string.Format(CacheKeyFormater, request.Context.Arguments);

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
    }
}