using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zata.Dynamic;
using System.Web.Caching;

namespace Zata.Web
{
    /// <summary>
    /// HTTP的Cache方法属性
    /// </summary>
    /// <remarks>
    /// HTTP缓存与方法缓存不一样, HTTP缓存是缓存序列化之后的内容, 包括返回对象序列化, Content-Type, Content-Encoding等.
    /// HTTPCache的时机是在HttpMethodProtocol的Execute, Format之间, 
    /// 当Execute时, 进行缓存检查, 将Format结果加载到当前上下文之中
    /// 当Format时, 检查是否已经包含缓存内容, 如果包含, 则直接写入Response
    /// 这意味着HTTP缓存, 不仅应当包含对象缓存, 还应当包含对Response的所有写入结果, 包括Cookies, Http Header, Http Content
    /// 由于Execute和Format是两个不同的操作, 所以HTTP缓存与方法缓存的区别在于其缓存写入, 加载, 返回都处于不同的层次
    /// </remarks>
    public class HttpMethodCacheAttribute : CachedMethodAttribute
    {
        protected override bool Config()
        {
            base.Config();

            CacheKeyFormater = this.GetType().Name + "_" + CacheKeyFormater;

            return true;
        }

        public override void Execute(ActionContext Context)
        {
            string CacheKey = string.Format(CacheKeyFormater, Context.Arguments);

            Context.Result = cacheManager.Get(CacheKey);

            if (Context.Result == null)
            {
                base.Execute(Context);
            }
        }

        internal void AddCache(ActionContext Context, HttpMethodResult WebResult)
        {
            string CacheKey = string.Format(CacheKeyFormater, Context.Arguments);
            DateTime CurrentTime = DateTime.Now;
            cacheManager.Add(
                CacheKey,
                WebResult,
                null,
                DateTime.Now.AddSeconds(DefaultExpireSeconds),
                TimeSpan.Zero, CacheItemPriority.Default, null);
        }
    }
}
