using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace Zata.FastReflection
{
    static class EntityToolsInternal
    {

        #region SetValueFunction

        /// <summary>
        /// 获取设置函数
        /// </summary>
        /// <returns></returns>
        public static Action<T, string, object> SetValueFunction<T>(bool ignoreCase = false)
        {
            Type type = typeof(T);

            // public void Set(T obj, string name, object value)
            var Argument = Expression.Parameter(type, "obj");
            var nameExpression = Expression.Parameter(typeof(string), "name");
            var valueExpression = Expression.Parameter(typeof(object), "value");

            var methodBody = new List<Expression>();

            // int v = name.GetHashCode()
            // 参数需要在最后定义methodBody时传入
            var v = Expression.Variable(typeof(int));
            var hashCode = Expression.Call(ignoreCase ? Expression.Call(nameExpression, typeof(string).GetMethod("ToUpperInvariant")) as Expression : nameExpression, typeof(string).GetMethod("GetHashCode"));
            var eva = Expression.Assign(v, hashCode);

            /*
             * switch(v)
             * {
             *   case 3455: //field1.Name.HashCode
             *      obj.[field1.Name] = (field1.FieldType)obj;
             *      break;
             *   case 64646:
             *      obj.[field1.Name] = (field1.FieldType)obj;
             *      break;
             *   case 575857:
             *      obj.[field1.Name] = (field1.FieldType)obj;
             *      break;
             *   default:
             *      break;
             * }
             * 
             * 
             */
            MemberInfo[] memberList = type.GetMembers(BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public);
            List<SwitchCase> caseExpressions = new List<SwitchCase>();
            foreach (var member in memberList)
            {
                if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
                    continue;

                if (!IsMemberHaveSet(member))
                    continue;

                var targetMember = Expression.PropertyOrField(Argument, member.Name);

                // 类型正确
                var SwitchCaseBlock = AssignPropertyOrFieldNew(Argument, member, targetMember, valueExpression);
                SwitchCase caseExpr = Expression.SwitchCase(SwitchCaseBlock, GetEntityNameArrayExpression(member, ignoreCase));

                caseExpressions.Add(caseExpr);
            }
            //var switchExpression = Expression.Switch(eva, Expression.Throw(Expression.New(typeof(ArgumentException))), caseExpressions.ToArray());
            var switchExpression = Expression.Switch(eva, Expression.Constant(false), caseExpressions.ToArray());
            methodBody.Add(switchExpression);

            //组装函数, 注意局部变量在第二个参数注册
            var methodBodyExpr = Expression.Block(typeof(void), new[] { v }, methodBody);

            return Expression.Lambda<Action<T, string, object>>(methodBodyExpr, Argument, nameExpression, valueExpression).Compile();
        }

       

        #endregion

        #region GetValueFunction

        /// <summary>
        /// 根据名字获取指定的值
        /// </summary>
        /// <returns></returns>
        internal static Func<T, string, object> GetValueFunction<T>(bool ignoreCase = false)
        {
            var type = typeof(T);
            var Argument = Expression.Parameter(type, "obj");
            var nameExpression = Expression.Parameter(typeof(string), "name");
            var v = Expression.Variable(typeof(int));
            var methodBody = new List<Expression>();
            var hashCode = Expression.Call(ignoreCase ? Expression.Call(nameExpression, typeof(string).GetMethod("ToUpperInvariant")) as Expression : nameExpression, typeof(string).GetMethod("GetHashCode"));
            var eva = Expression.Assign(v, hashCode);
            methodBody.Add(eva);

            MemberInfo[] memberList = type.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default);
            List<SwitchCase> caseExpressions = new List<SwitchCase>();
            foreach (var member in memberList)
            {
                if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
                    continue;

                var memberValue = Expression.Convert(Expression.PropertyOrField(Argument, member.Name), typeof(object));
                var caseExpr = Expression.SwitchCase(memberValue, GetEntityNameArrayExpression(member, ignoreCase));
                caseExpressions.Add(caseExpr);
            }

            var switchExpression = Expression.Switch(v, Expression.Constant(null), caseExpressions.ToArray());

            methodBody.Add(switchExpression);

            var methodBodyExpr = Expression.Block(typeof(object), new[] { v }, methodBody);

            return Expression.Lambda<Func<T, string, object>>(methodBodyExpr, Argument, nameExpression).Compile();
        }

        #endregion

        #region 内部方法

        #region 数据类型转换

        private static Expression AssignPropertyOrField(MemberInfo member, MemberExpression target, ParameterExpression value)
        {
            UnaryExpression convertExpression = Expression.Convert(value, target.Type);
            var assignExpression = Expression.Assign(target, convertExpression);
            var blockExpr = Expression.Block(typeof(void), assignExpression);

            // 类型转移失败
            // field = (field.Type)((IConvertible)value).ToType(field.Type, null)
            var callConvert = Expression.Assign(target, Expression.Convert(Expression.Call(Expression.Convert(value, typeof(IConvertible)), typeof(IConvertible).GetMethod("ToType"), Expression.Constant(target.Type), Expression.Constant(null, typeof(IFormatProvider))), target.Type));
            // if (value is IConvertible)
            var checkConvertable = Expression.IfThen(Expression.TypeIs(value, typeof(IConvertible)), callConvert);
            CatchBlock catchBlock = Expression.Catch(typeof(InvalidCastException), checkConvertable);

            var tryConvert = Expression.TryCatch(blockExpr, catchBlock);


            return tryConvert;
        }

        static Expression AssignPropertyOrFieldNew(Expression obj, MemberInfo member, Expression target, Expression value)
        {
            //数据类型一致
            if (target.Type == value.Type)
                return value;

            Expression Convertor = null;
            //判断target是否字符串
            if (target.Type == typeof(string))
            {
                Convertor = Expression.Condition(
                    Expression.TypeIs(value, typeof(string)),
                    Expression.TypeAs(value, typeof(string)),
                    Expression.Call(ConvertStringMethod, value));
            }
            else if(target.Type == typeof(Int32))
            {
                Convertor = Expression.Condition(
                    Expression.TypeIs(value, typeof(Int32)),
                    Expression.Convert(value, typeof(Int32)),
                    Expression.Call(ConvertInt32Method, value));
            }
            else if (target.Type == typeof(DateTime))
            {
                Convertor = Expression.Condition(
                    Expression.TypeIs(value, typeof(DateTime)),
                    Expression.Convert(value, typeof(DateTime)),
                    Expression.Call(ConvertDateTimeMethod, value));
            }
            else
                Convertor = Expression.Convert(Expression.Call(ConvertObjectMethod, Expression.Constant(target.Type), value), target.Type);

            //Field则直接转换, 如果是Property则调用方法
            var AssignExpr = (member is FieldInfo) ? 
                (Expression.Assign(target, Convertor) as Expression) :
                Expression.Call(obj, (member as PropertyInfo).GetSetMethod(), Convertor);
            
            return Expression.Block(AssignExpr, Expression.Constant(true));
        }

        static MethodInfo ConvertStringMethod = typeof(System.Convert).GetMethod("ToString", new Type[]{typeof(object)});
        static MethodInfo ConvertInt32Method = typeof(System.Convert).GetMethod("ToInt32", new Type[] { typeof(object) });
        static MethodInfo ConvertDateTimeMethod = typeof(System.Convert).GetMethod("ToDateTime", new Type[] { typeof(object) });
        static MethodInfo ConvertObjectMethod = typeof(EntityToolsInternal).GetMethod("ConvertObject", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// 参数类型转换
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        static object ConvertObject(Type targetType, object o)
        {
            if (o == null) return null;

            if (o.GetType() == targetType)
                return o;

            if (targetType is IConvertible)
            {
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, o.ToString(), true);

                if (o is IConvertible)
                    return System.Convert.ChangeType(o, targetType);

                return System.Convert.ChangeType(o.ToString(), targetType);
            }
            
            return o;
        }

        #endregion

        private static Expression[] GetEntityNameArrayExpression(MemberInfo member, bool ignoreCase)
        {
            var testCases = new List<Expression>();
            // 计算HashCode的过程放在了代码生成阶段，使用Dictionary的方式时HashCode是执行时计算的。
            testCases.Add(Expression.Constant(ignoreCase ? member.Name.ToUpperInvariant().GetHashCode() : member.Name.GetHashCode()));
            foreach (var attribute in member.GetCustomAttributes(true))
            {
                if (attribute is EntityAttribute)
                {
                    var compatibleName = (attribute as EntityAttribute).Name;
                    testCases.Add(Expression.Constant(ignoreCase ? compatibleName.ToUpperInvariant().GetHashCode() : compatibleName.GetHashCode()));
                }
            }

            return testCases.ToArray();
        }

        public static bool IsMemberHaveSet(MemberInfo m)
        {
            PropertyInfo p = m as PropertyInfo;

            if (p != null)
                return p.CanWrite;

            FieldInfo f = m as FieldInfo;

            return true;
        }

        #endregion

        static MethodInfo ConsoleWriteLineString = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
        static MethodInfo ConsoleWriteLineInt32 = typeof(Console).GetMethod("WriteLine", new[] { typeof(Int32) });
        static MethodInfo ConsoleWriteLineObject = typeof(Console).GetMethod("WriteLine", new[] { typeof(object) });
    }
}
