using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;

namespace Zata.FastReflection
{
    public class EntityTools<T>
    {
        public static Func<T, string, object> GetValue;
        public static Action<T, string, object> SetValue;

        static EntityTools()
        {
            GetValue = GetValueFunction();
            SetValue = SetValueFunction();
        }

        public object GetValueInstance(T obj, string name)
        {
            return GetValue(obj, name);
        }

        public void SetValueInstance(T obj, string name, object o)
        {
            SetValue(obj, name, o);
        }

        #region Set

        /// <summary>
        /// 获取设置函数
        /// </summary>
        /// <returns></returns>
        public static Action<T, string, object> SetValueFunction()
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
            var hashCode = Expression.Call(nameExpression, typeof(string).GetMethod("GetHashCode"));
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
             * 
             */
            MemberInfo[] list = type.GetMembers(BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public);
            List<SwitchCase> caseExpressions = new List<SwitchCase>();
            foreach (var field1 in list)
            {
                if (field1.MemberType != MemberTypes.Property && field1.MemberType != MemberTypes.Field)
                    continue;

                if (!IsCanSet(field1))
                    continue;

                var filedExpression1 = Expression.PropertyOrField(Argument, field1.Name);

                UnaryExpression convertExpression = null;
                convertExpression = Expression.Convert(valueExpression, filedExpression1.Type);

                var assignExpression = Expression.Assign(filedExpression1, convertExpression);
                var blockExpr = Expression.Block(assignExpression, Expression.Constant(true));
                var caseExpr = Expression.SwitchCase(blockExpr, Expression.Constant(field1.Name.GetHashCode()));

                caseExpressions.Add(caseExpr);
            }
            var switchExpression = Expression.Switch(eva, Expression.Constant(true), caseExpressions.ToArray());

            methodBody.Add(switchExpression);

            //组装函数, 注意局部变量在第二个参数注册
            var methodBodyExpr = Expression.Block(
                typeof(void),
                new[]{v},
                methodBody);

            return Expression.Lambda<Action<T, string, object>>(methodBodyExpr, Argument, nameExpression, valueExpression).Compile();
        }

        static bool IsGAC(MemberInfo m)
        {
            return m.DeclaringType.Assembly.GlobalAssemblyCache;
        }

        static bool IsValueType(MemberInfo m)
        {
            PropertyInfo p = m as PropertyInfo;

            if (p != null)
                return p.PropertyType.IsValueType;

            FieldInfo f = m as FieldInfo;

            if (f != null)
                return f.FieldType.IsValueType;

            return false;
        }

        static bool IsCanSet(MemberInfo m)
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
        internal static Func<T, string, object> GetValueFunction()
        {
            Type type = typeof(T);

            var Argument = Expression.Parameter(type, "obj");
            var nameExpression = Expression.Parameter(typeof(string), "name");

            var v = Expression.Variable(typeof(int));

            var methodBody = new List<Expression>();

            var hashCode = Expression.Call(nameExpression, typeof(string).GetMethod("GetHashCode"));
            var eva = Expression.Assign(v, hashCode);

            methodBody.Add(eva);

            MemberInfo[] list = type.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default);

            List<SwitchCase> caseExpressions = new List<SwitchCase>();

            LabelTarget returnTarget = Expression.Label();

            foreach (var field1 in list)
            {
                if (field1.MemberType != MemberTypes.Property && field1.MemberType != MemberTypes.Field)
                    continue;

                var filedExpression1 = Expression.Convert(Expression.PropertyOrField(Argument, field1.Name), typeof(object));

                var caseExpr = Expression.SwitchCase(filedExpression1, Expression.Constant(field1.Name.GetHashCode()));

                caseExpressions.Add(caseExpr);
            }

            var switchExpression = Expression.Switch(v, Expression.Constant(null), caseExpressions.ToArray());

            methodBody.Add(switchExpression);

            var methodBodyExpr = Expression.Block(
                typeof(object),
                new []{v},
                methodBody);

            return Expression.Lambda<Func<T, string, object>>(methodBodyExpr, Argument, nameExpression).Compile();
        }

        #endregion

        static MethodInfo ConsoleWriteLineString = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
        static MethodInfo ConsoleWriteLineInt32 = typeof(Console).GetMethod("WriteLine", new[] { typeof(Int32) });
        static MethodInfo ConsoleWriteLineObject = typeof(Console).GetMethod("WriteLine", new[] { typeof(object) });

        public static void CopyTo<T1>(T a, T1 b)
        {
            
        }
    }
}
