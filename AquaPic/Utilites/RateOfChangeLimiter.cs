#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller. 

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

using System;

namespace AquaPic.Utilites
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

