using System;
using System.Linq;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Diagnostics;

using System.Threading;
using Zata.Web;
using Zata.FastReflection;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
        //        var x = Expression.Parameter(typeof(object), "x");
        //        var y = Expression.Parameter(typeof(object), "y");
        //        var binder = Binder.BinaryOperation(
        //            CSharpBinderFlags.None, ExpressionType.Add, typeof(Program),
        //            new CSharpArgumentInfo[] { 
        //CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), 
        //CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)});
        //        Func<dynamic, dynamic, dynamic> f =
        //            Expression.Lambda<Func<object, object, object>>(
        //                Expression.Dynamic(binder, typeof(object), x, y),
        //                new[] { x, y }
        //            ).Compile();


        //        int c = 12;
        //        byte b = 3;

        //        Console.WriteLine(f(c, b));

                Run();
                //ClassA a = new ClassA();
                //ClassC c = new ClassC();

                //c.UserName = "gzh";

                //a.UserName = "temp";
                //a.testEnum = TestEnum.Test3;

                //EntityTools<ClassA>.SetValue(a, "testEnum", true, "1234");

                //EntityTools<ClassA>.SetValue(a, "age", true, "123");

                //Console.WriteLine(a.Age == 123);
                //Type t = typeof(Int64);
                //Type x = typeof(Int16);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            //b.SetEntityValue(
        }

        static double Test(int loop, Student stu, Func<Student, string> action)
        {
            action(stu);
            var watch = Stopwatch.StartNew();
            string s = null;
            for (var i = 0; i < loop; i++)
                s = action(stu);

            return watch.ElapsedTicks;
        }

        static Func<Student, string> NativeGetter()
        {
            return s => s.Name;
        }

        static Func<Student, string> ReflectedGetter()
        {
            var type = typeof(Student);
            var prop = type.GetProperty("Name");

            return s => (string)prop.GetValue(s, null);
        }

        static Func<Student, string> EmittedGetter()
        {
            var dm = new DynamicMethod(name: "EmittedGetter", returnType: typeof(string), parameterTypes: new[] { typeof(Student) }, owner: typeof(Student));

            var type = typeof(Student);
            var prop = type.GetMethod("get_Name");
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, prop);
            il.Emit(OpCodes.Ret);

            return (Func<Student, string>)dm.CreateDelegate(typeof(Func<Student, string>));
        }

        static Func<Student, string> ExpressionGetter()
        {
            var type = typeof(Student);
            var prop = type.GetMethod("get_Name");

            ParameterExpression pa = Expression.Parameter(typeof(Student));
            Expression body = Expression.Call(pa, prop);

            return Expression.Lambda<Func<Student, string>>(body, pa).Compile();
        }

        static Func<Student, string> DynamicGetter()
        {
            return s => { dynamic d = s; return d.Name; };
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void Run()
        {
            const int loop = 5000000;

            var stu = new Student { Name = "Mike" };

            var dynamic =
                Test(loop, stu, DynamicGetter());

            var expression =
                Test(loop, stu, ExpressionGetter());

            var native =
                Test(loop, stu, NativeGetter());

            var emitted =
                Test(loop, stu, EmittedGetter());

            var reflected =
                Test(loop, stu, ReflectedGetter());

            Console.WriteLine("native:{0}\ndynamic:{1}\nemit:{2}\nexpression:{3}\nreflection:{4}", 1, dynamic / native, emitted / native, expression / native, reflected / native);

        }
    }

    public class ClassA
    {
        public int Age;

        public long Money;

        [EntityAlias("pro")]
        public string UserName { get; set; }

        public DateTime dt;

        public TestEnum testEnum;

        public ClassB b;


    }

    public enum TestEnum
    {
        Test1 = 1,
        Test2,
        Test3
    }
    
    public class ClassC : ClassB
    {

    }


    public class ClassB : IEntity<ClassB>
    {
        public string UserName { get; set; }
    }


    class Student
    {            
        public string Name { get; set; }
    }
}


