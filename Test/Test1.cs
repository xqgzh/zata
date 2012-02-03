using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Zata.Dynamic;
using System.Threading;

namespace TestConsole
{
    class Test1
    {
        public static void TestPerf()
        {

            //这次测试中文1234
            Stopwatch sw = new Stopwatch();

            int MaxTimes = 100;
            TT t = new TT();
            MethodInfo mi = t.GetType().GetMethod("Test");
            MethodWrapper invoker = new MethodWrapper(t.GetType().GetCustomAttributes(false), true, () => { return new TT(); }, t, mi, mi.GetCustomAttributes(false));

            ActionBuilder builder = new ActionBuilder();
            builder.RegistType<TT>();

            IAction action = builder.FindAction("TT.Test");

            ActionContext context = new ActionContext()
            {
                Arguments = new object[] { "1", "2" },
                oInstance = t
            };

            Test(5, MaxTimes,
                (i, w) => {
                    string s = string.Empty;

                    switch (i)
                    {
                        case 0:
                            s = "方法直接调用";
                            break;
                        case 1:
                            s = "动态代理方法调用";
                            break;
                        case 2:
                            s = "Zata封装方法调用";
                            break;
                        case 3:
                            s = "反射调用";
                            break;
                    }

                    Console.WriteLine("{0}: {1}毫秒",s, w.ElapsedMilliseconds); },
                i => { t.Test("1", "2"); },
                i => invoker.Execute(context),
                i => { action.Execute(context); },
                i => mi.Invoke(t, new object[] { "1", "2"})
                );
        }

        static void Test(int RepeatTimes, int MaxTimes, Action<int, Stopwatch> watcher, params Action<int>[] funcList)
        {
            Console.WriteLine("执行{0}次, 重复{1}遍", MaxTimes, RepeatTimes);
            Stopwatch sw = new Stopwatch();

            if (watcher == null)
                watcher = (a, b) => { Console.WriteLine("{0} : {1}", a, b.Elapsed); };

            for (int v = 0; v < RepeatTimes; v++)
            {
                Console.WriteLine("================= 第 {0} 遍 ===================", v+1);
                for (int r = 0, j = funcList.Length; r < j; r++)
                {
                    var a = funcList[r];
                    if (a != null)
                    {
                        //初始化
                        a(0);

                        sw.Reset();
                        sw.Start();
                        for (int i = 0; i < MaxTimes; i++)
                        {
                            a(i);
                        }
                        sw.Stop();

                        watcher(r, sw);
                    }
                }
            }
        }


    }


    public class TT
    {
        public string Test(
            string a,
            string b)
        {
            Thread.Sleep(10);
            return a + b;
            //return a + b;
        }
    }

    public class UserInfo
    {
        public string UserName;

        public string vvv = "2342";
    }

    public class C
    {
        public string name;
    }

    public class C1
    {
        public string name;
    }

    public class C2
    {
        public string name;
    }
}
