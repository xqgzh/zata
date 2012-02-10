using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Zata.FastReflection.Extensions;

namespace Zata.FastReflection
{
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

                var bMember = FindCompatibleMember(bMemberList, memberInfo.Name, memberInfo.MemberType, ignoreCase);
                if (bMember == null || !EntityToolsInternal.IsMemberHaveSet(bMember))
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

        private static MemberInfo FindCompatibleMember(MemberInfo[] memberList, string targetName, MemberTypes types, bool ignoreCase)
        {
            var bMember = memberList.FirstOrDefault(m => IsMemberMatchName(m, targetName, ignoreCase) && m.MemberType == types);
            if (bMember == null)
                memberList.FirstOrDefault(m => IsMemberMatchName(m, targetName, ignoreCase));

            return bMember;
        }

        private static bool IsMemberMatchName(MemberInfo member, string targetName, bool ignoreCase)
        {
            var compatibleNames = new List<string>() { member.Name };
            compatibleNames.AddRange(from a in member.GetCustomAttributes(true)
                                     where a is EntityAliasAttribute
                                     select (a as EntityAliasAttribute).Name);

            return compatibleNames.Any(name => String.Compare(name, targetName, ignoreCase) == 0);
        }
    }
}
