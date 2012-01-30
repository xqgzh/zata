using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zata.Dynamic;
using System.Reflection;

namespace Zata.Dynamic
{
    public class ArgumentConvertAction : AbstractAction
    {
        /// <summary>
        /// 方法对应的参数列表
        /// </summary>
        ParamInfo[] Parameters { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        string MethodName;

        public override bool Config()
        {
            var parameters = Proxy.methodInfo.GetParameters();
            Parameters = new ParamInfo[parameters.Length];
            for (int i = 0, j = parameters.Length; i < j; i++)
            {
                Parameters[i] = new ParamInfo(parameters[i]);
            }

            MethodName = Proxy.methodInfo.Name;

            return true;
        }

        public override ActionContext Execute(ActionContext Context)
        {
            Context.Arguments = PrepareParameters(Context.Arguments);
            return base.Execute(Context);
        }

        #region 准备参数, 设置缺省值, 类型转换

        /// <summary>
        /// 准备参数, 设置缺省值, 类型转换
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        object[] PrepareParameters(object[] objs)
        {
            int ParaLength = Parameters.Length;
            if (ParaLength != objs.Length)
            {
                throw new ArgumentException("输入参数数量不匹配");
            }

            object[] values = new object[objs.Length];

            for (int i = 0; i < ParaLength; i++)
            {
                ParamInfo p = Parameters[i];

                try
                {
                    object preArgument = objs[i];

                    if (preArgument == null && p.Setting != null && p.Setting.IsRequired)
                    {
                        if (p.Setting.DefaultValue == null)
                        {
                            throw new ArgumentException("参数错误:" + p.Setting.Description, p.Name);
                        }
                        else
                            preArgument = p.Setting.DefaultValue;
                    }

                    object o = (preArgument != null) ? Convert(p, preArgument) : null;
                    values[i] = o;
                }
                catch (ArgumentException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(MethodName, p.Name, ex);
                }
            }
            return values;
        }

        #endregion

        #region 参数类型转换

        /// <summary>
        /// 参数类型转换
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        static object Convert(ParamInfo targetType, object o)
        {
            if (o == null) return null;

            if (o.GetType() == targetType.ParameterType)
                return o;

            if (targetType.IsIConvertible)
            {
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType.ParameterType, o.ToString(), true);
                }
                else
                {
                    if (o is IConvertible)
                        return System.Convert.ChangeType(o, targetType.ParameterType);
                    else
                        return System.Convert.ChangeType(o.ToString(), targetType.ParameterType);
                }
            }
            else
                return o;
        }


        #endregion
    }
}
