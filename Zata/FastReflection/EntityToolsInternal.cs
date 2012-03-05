using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Data;

namespace Zata.FastReflection
{
    static class EntityToolsInternal
    {

        #region SetValueFunction

        /// <summary>
        /// 获取设置函数
        /// </summary>
        /// <returns></returns>
        public static Func<T, string, bool, TValue, bool> SetValueFunction<T, TValue>()
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
                Expression.Call(Method_GetHashCodeLower, nameParameterExpr),
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
                var SwitchCaseBlock = AssignPropertyOrFieldNew(
                    objParameterExpr, member, 
                    targetMember, valueParameterExpression);
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
            var methodBodyExpr = Expression.Block(typeof(bool), new[] { v }, methodBody);

            var expr = Expression.Lambda<Func<T, string, bool, TValue, bool>>(methodBodyExpr, objParameterExpr, nameParameterExpr, ignoreCaseParameterExpr, valueParameterExpression);

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
                Expression.Call(Method_GetHashCodeLower, nameParameterExpr),
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

                ConverterExpr = GetMemberValue(returnType, nameParameterExpr, memberValue, ConverterExpr);

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

        private static Expression GetMemberValue(Type returnType, ParameterExpression nameParameterExpr, Expression memberValue, Expression ConverterExpr)
        {
            if (memberValue.Type == returnType)
                ConverterExpr = memberValue;
            else if (returnType == typeof(object))
            {
                ConverterExpr = Expression.Convert(memberValue, returnType);
            }
            else if (returnType == typeof(string))
            {
                ConverterExpr = Expression.Call(Method_ConvertString, Expression.Convert(memberValue, typeof(object)));
            }
            else if (returnType == typeof(Int32))
            {
                ConverterExpr = Expression.Call(Method_ConvertString, Expression.Convert(memberValue, typeof(Int32)));
            }
            else
            {
                ConverterExpr = Expression.Call(Method_ConvertObject, nameParameterExpr, Expression.Constant(returnType), Expression.Constant(IsIConvertible(returnType)), memberValue);
            }
            return ConverterExpr;
        }

        #endregion

        #region FieldOrPropertys

        public static void GetFieldPropertys<T>(
            ref int FieldCount, ref int PropertyCount, 
            ref string[] Fields, ref string[] Propertys,
            ref string[] FieldPropertys)
        {
            Type type = typeof(T);
            List<string> FieldList = new List<string>();
            List<string> PropertyList = new List<string>();
            List<string> FieldOrPropertyList = new List<string>();

            MemberInfo[] memberList = type.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default);

            foreach (var member in memberList)
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    PropertyList.Add(member.Name);
                    FieldOrPropertyList.Add(member.Name);
                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    FieldList.Add(member.Name);
                    FieldOrPropertyList.Add(member.Name);
                }
            }

            FieldCount = FieldList.Count;
            PropertyCount = PropertyList.Count;
            

            Fields = FieldList.ToArray();
            Propertys = PropertyList.ToArray();
            FieldPropertys = FieldOrPropertyList.ToArray();
        }

        #endregion

        #region 工具类方法

        /// <summary>
        /// 获取IDataParameter的名称
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static string GetParameterName(IDataParameter p)
        {
            string pName = p.SourceColumn;

            if (string.IsNullOrEmpty(pName))
            {
                pName = p.ParameterName.Substring(1);
            }

            if (pName.StartsWith("Original_"))
                pName = pName.Remove(0, "Original_".Length);

            return pName;
        }

        #endregion

        #region 内部方法

        #region GetHashCode(返回小写的HashCode)

        private static Expression[] GetEntityNameArrayExpression(MemberInfo member)
        {
            var testCases = new List<Expression>();

            
            // 计算HashCode的过程放在了代码生成阶段，使用Dictionary的方式时HashCode是执行时计算的。
            StringLowerTable.GetLowerHashCode(member.Name);
            StringLowerTable.GetLowerHashCode(member.Name.ToUpperInvariant());
            int a = member.Name.GetHashCode();
            int b = member.Name.ToLowerInvariant().GetHashCode();

            testCases.Add(Expression.Constant(a));
            testCases.Add(Expression.Constant(b));

            foreach (var attribute in member.GetCustomAttributes(true))
            {
                if (attribute is EntityAliasAttribute)
                {
                    var compatibleName = (attribute as EntityAliasAttribute).Name;

                    StringLowerTable.GetLowerHashCode(compatibleName);
                    StringLowerTable.GetLowerHashCode(compatibleName.ToLowerInvariant());

                    int c = compatibleName.GetHashCode();
                    int d = compatibleName.ToLowerInvariant().GetHashCode();

                    testCases.Add(Expression.Constant(c));
                    testCases.Add(Expression.Constant(d));
                }
            }

            return testCases.ToArray();
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
            Expression Convertor = null;

            
            //判断target是否字符串
            if (target.Type == value.Type)
            {
                Convertor = value;
            }
            else if (target.Type == typeof(string) && value.Type == typeof(object))
            {

                Convertor = Expression.Condition(
                    Expression.TypeIs(value, typeof(string)),
                    Expression.TypeAs(value, typeof(string)),
                    Expression.Call(Method_ConvertString, value));
            }
            else if (target.Type == typeof(Int32) && value.Type == typeof(object))
            {
                Convertor = Expression.Condition(
                    Expression.TypeIs(value, typeof(Int32)),
                    Expression.Convert(value, typeof(Int32)),
                    Expression.Call(Method_ConvertInt32, value));
            }
            else if (target.Type == typeof(DateTime) && value.Type == typeof(object))
            {
                Convertor = Expression.Condition(
                    Expression.TypeIs(value, typeof(DateTime)),
                    Expression.Convert(value, typeof(DateTime)),
                    Expression.Call(Method_ConvertDateTime, value));
            }
            else
            {
                Convertor = Expression.Call(typeof(EntityToolsInternal), "ConvertObject", new Type[]{target.Type}, Expression.Constant(target.ToString()), Expression.Constant(target.Type), Expression.Constant(IsIConvertible(target.Type)), value);
                //Convertor = Expression.Convert(Expression.Call(Method_ConvertObject, Expression.Constant(target.ToString()), Expression.Constant(target.Type), Expression.Constant(IsIConvertible(target.Type)), value), target.Type);
            }

            //Field则直接转换, 如果是Property则调用方法
            var AssignExpr = (member is FieldInfo) ? 
                (Expression.Assign(target, Convertor) as Expression) :
                Expression.Call(obj, (member as PropertyInfo).GetSetMethod(), Convertor);

            return Expression.Block(AssignExpr, Expression.Constant(true));
        }

        #endregion

        /// <summary>
        /// 参数类型转换
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        static TTarget ConvertObject<TTarget>(string name, Type targetType, bool IsConvertible, object o)
        {
            if (o == null)
            {
                if (targetType.IsValueType)
                    throw new FormatException(name + "(" + targetType.Name + ")无法赋值为空");
                return default(TTarget);
            }

            if (o.GetType() == targetType) return (TTarget)o;

            if (IsConvertible)
            {
                if (targetType.IsEnum)
                    return (TTarget)Enum.Parse(targetType, o.ToString(), true);

                if (o is IConvertible)
                    return (TTarget)System.Convert.ChangeType(o, targetType);

                return (TTarget)System.Convert.ChangeType(o, targetType);
            }

            return (TTarget)o;
        }

        /// <summary>
        /// 参数类型转换
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        static object ConvertObject2(string name, Type targetType, bool IsConvertible, object o)
        {
            if (o == null)
            {
                if (targetType.IsValueType)
                    throw new FormatException(name + "(" + targetType.Name + ")无法赋值为空");
                return null;
            }

            if (o.GetType() == targetType) return o;

            if (IsConvertible)
            {
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, o.ToString(), true);

                if (o is IConvertible)
                    return System.Convert.ChangeType(o, targetType);

                return System.Convert.ChangeType(o, targetType);
            }

            return o;
        }

        static Type IConvertibleType = typeof(IConvertible);

        static bool IsIConvertible(Type type)
        {
            foreach (var t in type.GetInterfaces())
                if (t == IConvertibleType) return true;

            return false;
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

        #region DEBUG相关方法

        static MethodInfo Method_ConvertString = typeof(System.Convert).GetMethod("ToString", new Type[] { typeof(object) });
        static MethodInfo Method_ConvertInt32 = typeof(System.Convert).GetMethod("ToInt32", new Type[] { typeof(object) });
        static MethodInfo Method_ConvertInt64 = typeof(System.Convert).GetMethod("ToInt64", new Type[] { typeof(object) });
        static MethodInfo Method_ConvertDateTime = typeof(System.Convert).GetMethod("ToDateTime", new Type[] { typeof(object) });
        static MethodInfo Method_ConvertObject = typeof(EntityToolsInternal).GetMethod("ConvertObject2", BindingFlags.NonPublic | BindingFlags.Static);
        static MethodInfo Method_GetHashCodeLower = typeof(StringLowerTable).GetMethod("GetLowerHashCode", BindingFlags.Static | BindingFlags.Public);

        static MethodInfo Method_TraceWriteLineString = typeof(Trace).GetMethod("WriteLine", new[] { typeof(string) });
        static MethodInfo Method_ConsoleWriteLineString = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
        static MethodInfo Method_ConsoleWriteLineInt32 = typeof(Console).GetMethod("WriteLine", new[] { typeof(Int32) });
        static MethodInfo Method_ConsoleWriteLineObject = typeof(Console).GetMethod("WriteLine", new[] { typeof(object) });

        #endregion
    }
}
