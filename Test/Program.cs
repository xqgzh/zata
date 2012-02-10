using System;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Zata.Web;
using Zata.FastReflection;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ClassA a = new ClassA();

            a.testEnum = TestEnum.Test1;

            EntityTools<ClassA>.SetValue(a, "testEnum", false, 2);

            Console.WriteLine("Set: {0}", a.testEnum);

            a.testEnum = TestEnum.Test3;

            Console.WriteLine(EntityTools<ClassA>.GetValue(a, "testEnum", false));

            ClassB b = new ClassB();

            //b.SetEntityValue(
        }
    }

    public class ClassA
    {
        public int Age;

        public string UserName { get; set; }

        public DateTime dt;

        public TestEnum testEnum;
    }

    public enum TestEnum
    {
        Test1 = 1,
        Test2,
        Test3
    }

    public class ClassB : IEntity<ClassB>
    {
        public string UserName { get; set; }
    }
}


