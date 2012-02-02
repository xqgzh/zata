using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zata.Dynamic;
using System.Web;

namespace Zata.Web
{
    /// <summary>
    /// 抽象HTTP协议描述, 定义了一个协议需要实现的所有功能
    /// 此协议的目的是将IAction的两阶段执行(Init->Execute)变为实际协议解析所需要的三阶段执行
    /// 三个阶段分别为
    /// 1. 发现与配置:  寻找适配的HttpProtocol, 对查找到的协议进行配置, 包括初始化HttpActionContext
    /// 2. 格式化: 对返回结果进行处理, 写入到HttpResponse中
    /// </summary>
    public abstract class AbstractHttpProtocol
    {
        #region 发现与配置

        /// <summary>
        /// 在Http上下文中寻找适配的方法名称
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public abstract string FindActionName(HttpRequest httpRequest);

        /// <summary>
        /// 从HttpContext初始化ActionContext
        /// </summary>
        public virtual HttpActionContext GetActionContext(HttpContext httpContext, IAction action)
        {
            HttpActionContext Context = new HttpActionContext() { HttpContext = httpContext };

            return Context;
        }

        #endregion

        #region 格式化

        public virtual bool IsHandlResponse(HttpContext httpContext)
        {
            return true;
        }

        /// <summary>
        /// 格式化当前对结果, 写入到HttpResponse
        /// </summary>
        public virtual void Response(HttpActionContext context)
        {
            if (context != null && context.HttpContext != null && context.Result != null)
                context.HttpContext.Response.Write(context.Result);
        }

        #endregion
    }
}
