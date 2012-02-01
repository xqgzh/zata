using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Zata.FastReflection.Accessors.NonGenerics
{
    public class GetPropertyExecutor
    {
        private Func<object, object> m_execute;

        public GetPropertyExecutor(MethodInfo methodInfo)
        {
            this.m_execute = this.GetExecuteDelegate(methodInfo);
        }

        public object Execute(object instance)
        {
            return this.m_execute(instance);
        }

        private Func<object, object> GetExecuteDelegate(MethodInfo methodInfo)
        {
            // 尚不支持泛型方法的调用
            if (methodInfo.ContainsGenericParameters)
                throw new NotSupportedException();

            // parameters to execute
            ParameterExpression instanceParameter =
                Expression.Parameter(typeof(object), "instance");

            // non-instance for static method, or ((TInstance)instance)
            Expression instanceCast = methodInfo.IsStatic ? null :
                Expression.Convert(instanceParameter, methodInfo.ReflectedType);

            // static invoke or ((TInstance)instance).Method
            MethodCallExpression methodCall = Expression.Call(instanceCast, methodInfo);

            // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
            if (methodCall.Type == typeof(void))
            {
                Expression<Action<object>> lambda =
                    Expression.Lambda<Action<object>>(methodCall, instanceParameter);

                Action<object> execute = lambda.Compile();
                return (instance) =>
                {
                    execute(instance);
                    return null;
                };
            }
            else
            {
                UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                Expression<Func<object, object>> lambda =
                    Expression.Lambda<Func<object, object>>(castMethodCall, instanceParameter);

                return lambda.Compile();
            }
        }
    }
}
