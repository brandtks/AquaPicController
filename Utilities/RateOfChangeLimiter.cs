#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;

namespace GoodtimeDevelopment.Utilites
{
    public class RateOfChangeLimiter
    {
        private float oldValue;
        private DateSpan lastTime;
        public float maxRateOfChange;

        public RateOfChangeLimiter (float maxRateOfChange) {
            oldValue = 0.0f;
            this.maxRateOfChange = maxRateOfChange;
            lastTime = DateSpan.Now;
        }

        public float RateOfChange (float newValue) {
            var now = DateSpan.Now;
            var secondDifference = DateSpan.Now.DifferenceInSeconds (lastTime).ToInt ();
            lastTime = now;

            var maxAllowedChange = secondDifference * maxRateOfChange;
            var valueDifference = Math.Abs (newValue - oldValue);

            if (valueDifference > maxAllowedChange) { // difference is greater than the max allowed
                if (newValue > oldValue) { // value is increasing
                    oldValue += maxAllowedChange;
                } else { // value is decreasing
                    oldValue -= maxAllowedChange;
                }
            } else {
                oldValue = newValue;
            }

            return oldValue;
        }

        public void Reset () {
            oldValue = 0.0f;
        }
    }
}

  }

        public void Reset () {
            oldValue = 0.0f;
        }
    }
}

