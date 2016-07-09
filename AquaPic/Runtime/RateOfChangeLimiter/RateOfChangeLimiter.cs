using System;
using System.Collections.Generic;
using AquaPic.Runtime;

namespace AquaPic.Utilites
{
    public class RateOfChangeLimiter
    {
        private class rocl
        {
            public static List<RateOfChangeLimiter> list;

            static rocl () {
                list = new List<RateOfChangeLimiter> ();
                TaskManager.AddCyclicInterrupt ("RateOfChange", 1000, Run);
            }

            protected static void Run () {
                foreach (var r in list)
                    r.Run ();
            }
        }

        private float oldValue;
        private float newValue;
        private float rtnValue;
        public float maxRateOfChange;

        public RateOfChangeLimiter (float maxRateOfChange) {
            oldValue = 0.0f;
            this.maxRateOfChange = maxRateOfChange;

            rocl.list.Add (this);
        }

        protected void Run () {
            float diff = Math.Abs (newValue - oldValue);

            if (diff > maxRateOfChange) { // difference is greater than the max allowed
                if (newValue > oldValue) // value is increasing
                    rtnValue = oldValue + maxRateOfChange;
                else // value is decreasing
                    rtnValue = oldValue - maxRateOfChange;
            } else
                rtnValue = newValue;

            oldValue = rtnValue;
        }

        public float RateOfChange (float newValue) {
            this.newValue = newValue;
            return rtnValue;
        }

        public void Reset () {
            oldValue = 0.0f;
        }
    }
}

