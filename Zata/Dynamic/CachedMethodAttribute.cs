using System;
using System.Reflection;
using System.Web.Caching;
using System.Web;

namespace Zata.Dynamic
{
    /// <summary>
    /// Rest类属性实体
    /// </summary>
    public class CachedMethodAttribute : AbstractMethodAttribute
    {
        static Cache cacheManager = HttpRuntime.Cache;

        /// <summary>
        /// 是否缓存
        /// </summary>
        public bool IsCached { get; set; }

        /// <summary>
        /// 默认过期时间
        /// </summary>
        public int DefaultExpireSeconds { get; set; }

        /// <summary>
        /// 缓存键设置
        /// </summary>
        protected string CacheKeyFormater = string.Empty;

        /// <summary>
        /// 默认构造
        /// </summary>
        public CachedMethodAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isCached"></param>
        /// <param name="defaultExpireSeconds"></param>
        public CachedMethodAttribute(int defaultExpireSeconds)
        {
            this.DefaultExpireSeconds = defaultExpireSeconds;
        }

        protected override bool Config()
        {
            #region 构造缓存键

            CacheKeyFormater = Proxy.methodInfo.Name;

            ParamInfo[] paras = Proxy.Parameters;

            for (int i = 0, j = paras.Length; i < j; i++)
            {
                if (CachedKeyAttribute.IsDefined(paras[i].ParameterInfo, typeof(CachedKeyAttribute)))
                {
                    CacheKeyFormater += "_{" + i + "}";
                }
            }

            #endregion

            return true;
        }

        public override void Execute(ActionContext Context)
        {
            string CacheKey = string.Format(CacheKeyFormater, Context.Arguments);

            Context.Result = cacheManager.Get(CacheKey);

            if (Context.Result == null)
            {
                base.Execute(Context);

                DateTime CurrentTime = DateTime.Now;
                cacheManager.Add(
                    CacheKey,
                    Context.Result,
                    null,
                    DateTime.Now.AddSeconds(DefaultExpireSeconds),
                    TimeSpan.Zero, CacheItemPriority.Default, null);
            }
        }

    }
}
