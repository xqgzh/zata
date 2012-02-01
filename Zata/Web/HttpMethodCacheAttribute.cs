using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zata.Dynamic;

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
            if (!base.Config())
            {
                return false;
            }

            CacheKeyFormater = this.GetType().Name + "_" + CacheKeyFormater;

            return true;
        }

        public override void Execute(ActionContext Context)
        {
            base.Execute(Context);
        }

        internal void AddCache(HttpMethodResult WebResult)
        {
            throw new NotImplementedException();
        }
    }
}
