using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Zata.FastReflection
{
    static class EntityToolsInternal
    {

        #region SetValueFunction

        /// <summary>
        /// 获取设置函数
        /// </summary>
        /// <returns></returns>
        public static Action<T, string, bool, TValue> SetValueFunction<T, TValue>()
        {
            Type type = typeof(T);
            Type valueType = typeof(TValue);

            

            // public void Set(T obj, string name, object value)
            var objParameterExpr = Expression.Parameter(type, "obj");
            var nameParameterExpr = Expression.Parameter(typeof(string), "name");
            var ignoreCaseParameterExpr = Expression.Parameter(typeof(bool), "ignoreCase");
            var valueParameterExpression = Expression.Parameter(valueType, "value");

            var methodBody = new List<Expression>();

            var v = Expression.Variable(typeof(int));
            var hashCode = Expression.Condition(
                Expression.IsTrue(ignoreCaseParameterExpr), 
                Expression.Call(GetNameHashCodeMethod, nameParameterExpr),
                Expression.Call(nameParameterExpr, typeof(string).GetMethod("GetHashCode")));
            var eva = Expression.Assign(v, hashCode);

            methodBody.Add(eva);

            MemberInfo[] memberList = type.GetMembers(BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public);
            List<SwitchCase> caseExpressions = new List<SwitchCase>();
            foreach (var member in memberList)
            {
                if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
                    continue;

                if (!IsMemberHaveSet(member))
                    continue;

                var targetMember = Expression.PropertyOrField(objParameterExpr, member.Name);

                // 类型正确
                var SwitchCaseBlock = AssignPropertyOrFieldNew(objParameterExpr, member, targetMember, valueParameterExpression);
                SwitchCase caseExpr = Expression.SwitchCase(SwitchCaseBlock, GetEntityNameArrayExpression(member));

                caseExpressions.Add(caseExpr);
            }
            //var switchExpression = Expression.Switch(eva, Expression.Throw(Expression.New(typeof(ArgumentException))), caseExpressions.ToArray());

            Expression defaultCaseExpr = null;

            if (valueType == typeof(object))
                defaultCaseExpr = Expression.Constant(null);
            else
                defaultCaseExpr = Expression.Constant(string.Empty);

            var switchExpression = Expression.Switch(v, Expression.Constant(false), caseExpressions.ToArray());
            methodBody.Add(switchExpression);

            //组装函数, 注意局部变量在第二个参数注册
            var methodBodyExpr = Expression.Block(typeof(void), new[] { v }, methodBody);

            var expr = Expression.Lambda<Action<T, string, bool, TValue>>(methodBodyExpr, objParameterExpr, nameParameterExpr, ignoreCaseParameterExpr, valueParameterExpression);

            return expr.Compile();
        }

       

        #endregion

        #region GetValueFunction

        /// <summary>
        /// 根据名字获取指定的值
        /// </summary>
        /// <returns></returns>
        internal static Func<T, string, bool, TReturn> GetValueFunction<T, TReturn>()
        {
            var type = typeof(T);
            var returnType = typeof(TReturn);
            var objParameterExpr = Expression.Parameter(type, "obj");
            var nameParameterExpr = Expression.Parameter(typeof(string), "name");
            var ignoreCaseParameterExpr = Expression.Parameter(typeof(bool), "ignoreCase");
            
            var methodBody = new List<Expression>();

            var v = Expression.Variable(typeof(int));
            var hashCode = Expression.Condition(
                Expression.IsTrue(ignoreCaseParameterExpr),
                Expression.Call(GetNameHashCodeMethod, nameParameterExpr),
                Expression.Call(nameParameterExpr, typeof(string).GetMethod("GetHashCode")));
            var eva = Expression.Assign(v, hashCode);
            methodBody.Add(eva);

            MemberInfo[] memberList = type.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default);
            List<SwitchCase> caseExpressions = new List<SwitchCase>();
            foreach (var member in memberList)
            {
                if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
                    continue;

                Expression memberValue = Expression.PropertyOrField(objParameterExpr, member.Name);
                
                Expression ConverterExpr = null;
                
                if(memberValue.Type == returnType)
                    ConverterExpr = memberValue;
                else if (returnType == typeof(object))
                {
                    ConverterExpr = Expression.Convert(memberValue, returnType);
                }
                else if(returnType == typeof(string))
                {
                    ConverterExpr = Expression.Call(ConvertStringMethod, Expression.Convert(memberValue, typeof(object)));
                }
                else
                {
                    ConverterExpr = Expression.Call(ConvertObjectMethod, memberValue);
                }

                var caseExpr = Expression.SwitchCase(ConverterExpr, GetEntityNameArrayExpression(member));
                caseExpressions.Add(caseExpr);
            }

            Expression defaultCaseExpr = null;

            if (returnType == typeof(object))
                defaultCaseExpr = Expression.Constant(null);
            else
                defaultCaseExpr = Expression.Constant(string.Empty);


            var switchExpression = Expression.Switch(v,

                defaultCaseExpr, 
                
                caseExpressions.ToArray());

            methodBody.Add(switchExpression);

            var methodBodyExpr = Expression.Block(returnType, new[] { v }, methodBody);

            return Expression.Lambda<Func<T, string, bool, TReturn>>(methodBodyExpr, objParameterExpr, nameParameterExpr, ignoreCaseParameterExpr).Compile();
        }

        #endregion

        #region 内部方法

        #region GetHashCode(返回小写的HashCode)

        static Dictionary<int, int> NameDict = new Dictionary<int, int>();

        static MethodInfo GetNameHashCodeMethod = typeof(EntityToolsInternal).GetMethod("GetNameHashCode", BindingFlags.Static | BindingFlags.NonPublic);

        static int GetNameHashCode(string name)
        {
            int i = name.GetHashCode();

            if (NameDict.ContainsKey(i))
                return NameDict[i];

            int j = name.ToUpperInvariant().GetHashCode();
            lock (NameDict)
            {
                if (!NameDict.ContainsKey(i))
                    NameDict.Add(i, j);
            }

            return j;
        }

        #endregion

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
                return Expression.Block(value, Expression.Constant(true));

            Expression Convertor = null;
            //判断target是否字符串
            if (target.Type == typeof(string)  && value.Type == typeof(object))
            {
                Convertor = Expression.Condition(
                    Expression.TypeIs(value, typeof(string)),
                    Expression.TypeAs(value, typeof(string)),
                    Expression.Call(ConvertStringMethod, value));
            }
            else if (target.Type == typeof(Int32) && value.Type == typeof(object))
            {
                Convertor = Expression.Condition(
                    Expression.TypeIs(value, typeof(Int32)),
                    Expression.Convert(value, typeof(Int32)),
                    Expression.Call(ConvertInt32Method, value));
            }
            else if (target.Type == typeof(DateTime) && value.Type == typeof(object))
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

            if (o is IConvertible)
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

        

        private static Expression[] GetEntityNameArrayExpression(MemberInfo member)
        {
            var testCases = new List<Expression>();

            // 计算HashCode的过程放在了代码生成阶段，使用Dictionary的方式时HashCode是执行时计算的。
            GetNameHashCode(member.Name);
            GetNameHashCode(member.Name.ToUpperInvariant());
            int a = member.Name.GetHashCode();
            int b = member.Name.ToUpperInvariant().GetHashCode();

            testCases.Add(Expression.Constant(a));
            testCases.Add(Expression.Constant(b));
            
            foreach (var attribute in member.GetCustomAttributes(true))
            {
                if (attribute is EntityAliasAttribute)
                {
                    var compatibleName = (attribute as EntityAliasAttribute).Name;

                    GetNameHashCode(compatibleName);
                    GetNameHashCode(compatibleName.ToUpperInvariant());

                    int c = compatibleName.GetHashCode();
                    int d = compatibleName.GetHashCode();
                    
                    testCases.Add(Expression.Constant(c));
                    testCases.Add(Expression.Constant(d));
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
