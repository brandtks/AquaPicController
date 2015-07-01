using System;
using System.Collections.Generic;

namespace AquaPic.Runtime
{
    public partial class Timer
    {
        protected static Dictionary<string, OnDelayTimer> odts = new Dictionary<string, OnDelayTimer> ();

        public static bool OnDelay (string name, string time, bool enable) {
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

