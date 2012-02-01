using System;
using System.Diagnostics;

namespace Zata.Test
{
    /// <summary>
    /// 
    /// </summary>
    public class PerformanceTimer
    {
        private Action action = null;

        public PerformanceTimer(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.action = action;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="times"></param>
        public TimeSpan Run(int times)
        {
            Stopwatch watch = Stopwatch.StartNew();
            int i = 0;
            while (i++ < times)
            {
                action();
            }
            watch.Stop();

            return watch.Elapsed;
        }
    }
}
