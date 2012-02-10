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

        #region Set

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

                if (!IsCanSet(member))
                    continue;

                var targetMember = Expression.PropertyOrField(Argument, member.Name);

                // 类型正确
                UnaryExpression convertExpression = Expression.Convert(valueExpression, targetMember.Type);
                var assignExpression = Expression.Assign(targetMember, convertExpression);
                var blockExpr = Expression.Block(typeof(void), assignExpression);

                // 类型转移失败
                // field = (field.Type)((IConvertible)value).ToType(field.Type, null)
                var callConvert = Expression.Assign(targetMember, Expression.Convert(Expression.Call(Expression.Convert(valueExpression, typeof(IConvertible)), typeof(IConvertible).GetMethod("ToType"), Expression.Constant(targetMember.Type), Expression.Constant(null, typeof(IFormatProvider))), targetMember.Type));
                // if (value is IConvertible)
                var checkConvertable = Expression.IfThen(Expression.TypeIs(valueExpression, typeof(IConvertible)), callConvert);
                CatchBlock catchBlock = Expression.Catch(typeof(InvalidCastException), checkConvertable);

                var tryConvert = Expression.TryCatch(blockExpr, catchBlock);

                var caseExpr = Expression.SwitchCase(tryConvert, GetMemberCompitableNames(member, ignoreCase));

                caseExpressions.Add(caseExpr);
            }
            var switchExpression = Expression.Switch(eva, Expression.Throw(Expression.New(typeof(ArgumentException))), caseExpressions.ToArray());
            methodBody.Add(switchExpression);

            //组装函数, 注意局部变量在第二个参数注册
            var methodBodyExpr = Expression.Block(typeof(void), new[] { v }, methodBody);

            return Expression.Lambda<Action<T, string, object>>(methodBodyExpr, Argument, nameExpression, valueExpression).Compile();
        }

        private static Expression[] GetMemberCompitableNames(MemberInfo member, bool ignoreCase)
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

        private static bool IsGAC(MemberInfo m)
        {
            return m.DeclaringType.Assembly.GlobalAssemblyCache;
        }

        private static bool IsValueType(MemberInfo m)
        {
            PropertyInfo p = m as PropertyInfo;

            if (p != null)
                return p.PropertyType.IsValueType;

            FieldInfo f = m as FieldInfo;

            if (f != null)
                return f.FieldType.IsValueType;

            return false;
        }

        public static bool IsCanSet(MemberInfo m)
        {
            PropertyInfo p = m as PropertyInfo;

            if (p != null)
                return p.CanWrite;

            FieldInfo f = m as FieldInfo;

            return true;
        }

        #endregion

        #region Get

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
                var caseExpr = Expression.SwitchCase(memberValue, GetMemberCompitableNames(member, ignoreCase));
                caseExpressions.Add(caseExpr);
            }

            var switchExpression = Expression.Switch(v, Expression.Constant(null), caseExpressions.ToArray());

            methodBody.Add(switchExpression);

            var methodBodyExpr = Expression.Block(typeof(object), new[] { v }, methodBody);

            return Expression.Lambda<Func<T, string, object>>(methodBodyExpr, Argument, nameExpression).Compile();
        }

        #endregion

        static MethodInfo ConsoleWriteLineString = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
        static MethodInfo ConsoleWriteLineInt32 = typeof(Console).GetMethod("WriteLine", new[] { typeof(Int32) });
        static MethodInfo ConsoleWriteLineObject = typeof(Console).GetMethod("WriteLine", new[] { typeof(object) });
    }
}
