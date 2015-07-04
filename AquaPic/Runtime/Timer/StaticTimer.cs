using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AquaPic.Runtime
{
    public partial class Timer
    {
        protected static Dictionary<string, OnDelayTimer> odts = new Dictionary<string, OnDelayTimer> ();

        // method is not inlined because we are saving the calling method as the KEY for the dictionary of on delay timers
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static bool OnDelay (string time, bool enable) {
            StackTrace stackTrace = new StackTrace();
            string name = stackTrace.GetFrame (1).GetMethod ().Name;

            Console.WriteLine ("OnDelay caller is {0}", name);

            if (!odts.ContainsKey (name)) {
                uint timeDelay = ParseTime (time);
                odts.Add (name, new OnDelayTimer (timeDelay));
            }

            return odts [name].Evaluate (enable);
        }

        public static uint ParseTime (string timeString) {
            char[] seperator = new char[1] {':'};
            string[] t = timeString.Split (seperator, 3);

            uint time = 0;
            if (t.Length == 3) {
                //milliseconds
                time += Convert.ToUInt32 (t [2]);

                //seconds
                time += (Convert.ToUInt32 (t [1]) * 60);

                //minutes
                time += (Convert.ToUInt32 (t [0]) * 60000);
            }

            return time;
        }
    }
}

