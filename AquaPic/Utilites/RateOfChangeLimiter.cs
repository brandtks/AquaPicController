using System;

namespace AquaPic.Utilites
{
    public class RateOfChangeLimiter
    {
        private float oldValue;
        public float maxRateOfChange;

        public RateOfChangeLimiter (float maxRateOfChange) {
            this.oldValue = 0.0f;
            this.maxRateOfChange = maxRateOfChange;
        }

        public float RateOfChange (float newValue) {
            float rtnValue;
            float diff = Math.Abs (newValue - oldValue);

            if (diff > maxRateOfChange) { // difference is greater than the max allowed
                if (newValue > oldValue) // value is increasing
                    rtnValue = oldValue + maxRateOfChange;
                else // value is decreasing
                    rtnValue = oldValue - maxRateOfChange;
            } else
                rtnValue = newValue;
            
            oldValue = rtnValue;
            return rtnValue;
        }
    }
}

