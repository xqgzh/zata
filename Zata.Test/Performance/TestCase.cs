using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Zata.Test.Performance
{
    public class TestCase
    {
        public Dictionary<string, Action> Methods = new Dictionary<string, Action>();

        public Action<int, string, Stopwatch> Watcher;
        public Action<int> StepWatcher;

        public TestCase Build(string name, Action action)
        {
            Methods.Add(name, action);
            return this;
        }

        public void Execute(params int[] MaxTimeList)
        {
            Stopwatch sw = new Stopwatch();

            int iCount = Methods.Keys.Count;

            for (int Step = 0; Step < MaxTimeList.Length; Step++)
            {
                int MaxTimes = MaxTimeList[Step];

                if (StepWatcher != null)
                    StepWatcher(MaxTimes);

                foreach (var name in Methods.Keys)
                {
                    var action = Methods[name];
                    action();
                    sw.Reset();
                    sw.Start();
                    for (int i = 0; i < MaxTimes; i++)
                    {
                        action();
                    }
                    sw.Stop();

                    Watcher(Step, name, sw);
                }

            }
        }
    }
}
