using System;
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

            EntityTools<ClassA>.SetValue(a, "UserName", "Test1");

            Console.WriteLine(EntityTools<ClassA>.GetValue(a, "UserName"));
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
        Test1,
        Test2
    }

    public class ClassB
    {

    }
}


