using System;
using Zata.Dynamic;
using System.Web;
namespace Zata.Web
{
    /// <summary>
    /// 用于在AbstractHttpProtocol的行为基础上进行过滤和封装
    /// </summary>
    /// <remarks>基本模式与IAction类似, <seealso cref="Zata.Dynamic.IAction">IAction</seealso>是针对单一Execute行为进行过滤, 而IHttpAction则是针对Protocol的Request/Response行为进行过滤</remarks>
    public interface IHttpAction : IAction
    {
        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        void InitContext(HttpActionRequest request);

        /// <summary>
        /// 格式化输出
        /// </summary>
        /// <param name="context"></param>
        void Response(HttpActionContext context);
    }
}
