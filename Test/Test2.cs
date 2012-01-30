using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestConsole
{
    public class Test2<T>
    {
        public static int X = 1;

        static Test2()
        {
            X = X + 1;
        }

        public T t;
    }

    public class Test21 : Test2<string>
    {

    }

    public class Test22 : Test2<string>
    {

    }
}
