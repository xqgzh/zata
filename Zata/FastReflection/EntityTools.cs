using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Zata.FastReflection
{
    public class EntityTools<T>
    {
        public static Func<T, string, object> GetValue;
        public static Action<T, string, object> SetValue;
        public static Func<T, string, object> GetValueIgnoreCase;
        public static Action<T, string, object> SetValueIgnoreCase;

        static EntityTools()
        {
            GetValue = GetValueFunction();
            SetValue = SetValueFunction();
            GetValueIgnoreCase = GetValueFunction(true);
            SetValueIgnoreCase = SetValueFunction(true);
        }

        public static object GetValueInstance(T obj, string name, bool ignoreCase = false)
        {
            return (ignoreCase ? GetValueIgnoreCase : GetValue)(obj, name);
        }

        public static void SetValueInstance(T obj, string name, object o, bool ignoreCase = false)
        {
            (ignoreCase ? SetValueIgnoreCase : SetValue)(obj, name, o);
        }

        #region Set

        /// <summary>
        /// 获取设置函数
        /// </summary>
        /// <returns></returns>
        public static Action<T, string, object> SetValueFunction(bool ignoreCase = false)
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
                var caseExpr = Expression.SwitchCase(blockExpr, Expression.Constant(ignoreCase ? field1.Name.ToUpperInvariant().GetHashCode() : field1.Name.GetHashCode()));

                caseExpressions.Add(caseExpr);
            }
            var switchExpression = Expression.Switch(eva, Expression.Constant(true), caseExpressions.ToArray());

            methodBody.Add(switchExpression);

            //组装函数, 注意局部变量在第二个参数注册
            var methodBodyExpr = Expression.Block(
                typeof(void),
                new[] { v },
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

        internal static bool IsCanSet(MemberInfo m)
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
        internal static Func<T, string, object> GetValueFunction(bool ignoreCase = false)
        {
            Type type = typeof(T);

            var Argument = Expression.Parameter(type, "obj");
            var nameExpression = Expression.Parameter(typeof(string), "name");

            var v = Expression.Variable(typeof(int));

            var methodBody = new List<Expression>();

            var hashCode = Expression.Call(ignoreCase ? Expression.Call(nameExpression, typeof(string).GetMethod("ToUpperInvariant")) as Expression : nameExpression, typeof(string).GetMethod("GetHashCode"));
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

                // 计算HashCode的过程放在了代码生成阶段，使用Dictionary的方式时HashCode是执行时计算的。
                var caseExpr = Expression.SwitchCase(filedExpression1, Expression.Constant(ignoreCase ? field1.Name.ToUpperInvariant().GetHashCode() : field1.Name.GetHashCode()));

                caseExpressions.Add(caseExpr);
            }

            var switchExpression = Expression.Switch(v, Expression.Constant(null), caseExpressions.ToArray());

            methodBody.Add(switchExpression);

            var methodBodyExpr = Expression.Block(
                typeof(object),
                new[] { v },
                methodBody);

            return Expression.Lambda<Func<T, string, object>>(methodBodyExpr, Argument, nameExpression).Compile();
        }

        #endregion

        static MethodInfo ConsoleWriteLineString = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
        static MethodInfo ConsoleWriteLineInt32 = typeof(Console).GetMethod("WriteLine", new[] { typeof(Int32) });
        static MethodInfo ConsoleWriteLineObject = typeof(Console).GetMethod("WriteLine", new[] { typeof(object) });
    }

    public class EntityTools<S, D>
    {
        private static Action<S, D> copyTo;
        private static Action<S, D> copyToIgnoreCase;
        private static Action<S, D> emtptyAction = new Action<S, D>((s, d) => { });

        static EntityTools()
        {
            copyTo = CreateCopyToDelegate();
            copyToIgnoreCase = CreateCopyToDelegate(true);
        }

        public static void CopyTo(S a, D b, bool ignoreCase = false)
        {
            (ignoreCase ? copyToIgnoreCase : copyTo)(a, b);
        }

        internal static Action<S, D> CreateCopyToDelegate(bool ignoreCase = false)
        {
            var aType = typeof(S);
            var bType = typeof(D);

            var a = Expression.Parameter(aType, "a");
            var b = Expression.Parameter(bType, "b");
            var methodBody = new List<Expression>();

            var aMemberList = aType.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default);
            var bMemberList = bType.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default);
            foreach (var memberInfo in aMemberList)
            {
                if (memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property)
                    continue;

                var bMember = bMemberList.FirstOrDefault(m => String.Compare(m.Name, memberInfo.Name, ignoreCase) == 0 && m.MemberType == memberInfo.MemberType);
                if (bMember == null)
                    bMemberList.FirstOrDefault(m => String.Compare(m.Name, memberInfo.Name, ignoreCase) == 0);

                if (bMember == null || !EntityTools<D>.IsCanSet(bMember))
                    continue;

                var retType = memberInfo.GetReturnType();
                if (retType != bMember.GetReturnType())
                    continue;

                var aField = Expression.PropertyOrField(a, memberInfo.Name);
                var bField = Expression.PropertyOrField(b, bMember.Name);
                // 值类型直接拷贝
                if (retType.IsValueType || retType == typeof(string))
                {
                    methodBody.Add(Expression.Assign(bField, aField));
                }
                // 引用类型优先使用IClonable接口进行对象拷贝，如果没有实现IClonable接口，则执行浅拷贝。
                else
                {
                    if (retType.GetInterface("ICloneable") != null)
                    {
                        var aClone = Expression.Convert(Expression.Call(aField, retType.GetMethod("Clone")), retType);
                        methodBody.Add(Expression.Assign(bField, aClone));
                    }
                    else
                        methodBody.Add(Expression.Assign(bField, aField));
                }
            }

            if (methodBody.Count > 0)
            {
                var methodBodyExpr = Expression.Block(typeof(void), null, methodBody);

                return Expression.Lambda<Action<S, D>>(methodBodyExpr, a, b).Compile();
            }
            else
                return emtptyAction;
        }
    }
}
