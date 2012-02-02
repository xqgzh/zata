using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Zata.Web;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            TY t = new TY();

            TY Current = t;

            for (int i = 1; i < 10; i++)
            {
                TY x = new TY(){Number = i};

                Current.Inner = x;

                Current = x;
            }

            foreach (var y in For(t))
            {
                Console.WriteLine(y.Number);
            }
            
        }

        static List<TY> For(TY t)
        {
            List<TY> list = new List<TY>();

            TY Current = t;

            while (Current != null)
            {
                list.Add(Current);

                Current = Current.Inner;
            }

            return list;
        }

    }

    public class TY
    {
        public int Number = 0;

        public TY Inner;
    }
}


