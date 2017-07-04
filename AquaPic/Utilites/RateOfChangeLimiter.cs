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

