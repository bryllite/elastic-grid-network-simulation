using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Utils
{
    public class BrylliteTimer
    {
        private DateTime BeginTime = new DateTime(0);

        public BrylliteTimer(bool auto = false)
        {
            if (auto) Start();
        }

        public long TotalMilliseconds
        {
            get
            {
                return ((DateTime.Now.Ticks - BeginTime.Ticks) / 10000);
            }
        }

        public void Start()
        {
            BeginTime = DateTime.Now;
        }

        public void Reset()
        {
            BeginTime = DateTime.Now;
        }

        public bool TimeOut(int msTimeOut, bool resetIfTimeOut = true)
        {
            bool timeout = TotalMilliseconds >= msTimeOut;
            if (timeout && resetIfTimeOut)
                Reset();

            return timeout;
        }
    }
}
