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
            ClassB b = new ClassB();
            EntityTools<ClassA>.CopyTo<ClassB>(a, b);
        }
    }

    public class ClassA
    {

    }

    public class ClassB
    {

    }
}


