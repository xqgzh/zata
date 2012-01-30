using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zata.Dynamic
{

    /// <summary>
    /// 对业务方法封装的动态代理类, 用于提供业务方法的动态调用接口
    /// </summary>
    public class MethodWrapper
    {
        Func<object, object[], object> Executer;

        /// <summary>
        /// 全局对象
        /// </summary>
        internal object GlobalInstance;

        /// <summary>
        /// 全局对象创建器
        /// </summary>
        /// <remarks>如果业务方法非静态方法, 则需要提供一个实例对象以执行, 可以通过定义全局对象来处理</remarks>
        internal Func<object> CreateInstance;

        /// <summary>
        /// 业务方法的上下文信息
        /// </summary>
        internal MethodInfo methodInfo;

        /// <summary>
        /// 方法对应的参数列表
        /// </summary>
        public Zata.Dynamic.ParamInfo[] Parameters { get; private set; }

        /// <summary>
        /// 调用时是否每次都创建新对象
        /// </summary>
        internal bool IsCreateIfRequest = false;

        /// <summary>
        /// 业务方法所在类的自定义属性列表
        /// </summary>
        public object[] TypeAtrributes;

        /// <summary>
        /// 业务方法的自定义属性列表
        /// </summary>
        public object[] MethodAttributes;

        public MethodWrapper(
            object[] typeAtrributes, 
            bool isCreateIfRequest, 
            Func<object> creater, 
            object globaInstance,
            MethodInfo method, object[] methodAttributes)
        {
            methodInfo = method;
            IsCreateIfRequest = isCreateIfRequest;
            CreateInstance = creater;
            GlobalInstance = globaInstance;
            TypeAtrributes = typeAtrributes;
            MethodAttributes = methodAttributes;

            Executer = Zata.Dynamic.MethodGenerater.GetExpressionInvoker(methodInfo);

            ParameterInfo[] parameters = method.GetParameters();

            //保存方法信息和自定义属性, 由于GetCustomAttributes每次调用会产生一个新的实例, 所以必须缓存这些自定义属性
            Parameters = new Zata.Dynamic.ParamInfo[parameters.Length];
            for (int i = 0, j = parameters.Length; i < j; i++)
            {
                Parameters[i] = new Zata.Dynamic.ParamInfo(parameters[i]);
            }
        }

        /// <summary>
        /// 调用业务方法
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public ActionContext Execute(ActionContext Context)
        {
            object o = Context.oInstance ?? ((IsCreateIfRequest && CreateInstance != null) ? CreateInstance() : GlobalInstance);

            if (methodInfo.IsStatic == false && o == null)
            {
                throw new System.Configuration.ConfigurationErrorsException(methodInfo.Name + "方法必须运行于实例之中");
            }

            Context.Result = Executer(o, Context.Arguments);

            return Context;
        }
    }
}