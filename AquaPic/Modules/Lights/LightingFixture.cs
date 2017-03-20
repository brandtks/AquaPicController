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

﻿using System;
using AquaPic.Utilites;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Modules;

namespace AquaPic.Modules
{
    public partial class Lighting
    {
        public class LightingFixture
    	{
            public string name;
            public int onTimeOffset;
            public int offTimeOffset;
            public TimeDate onTime;
            public TimeDate offTime;
            public LightingTime lightingTime;
            public MyState lightingOn;
            public bool highTempLockout;
            public IndividualControl powerOutlet;
            public Mode mode;

            public LightingFixture (
                string name, 
                IndividualControl plug, 
                Time onTime, 
                Time offTime, 
                LightingTime lightingTime,
                bool highTempLockout
            ) {
                this.name = name;
                this.powerOutlet = plug;

                // sets time to today and whatever onTime and offTime are
                this.onTime = new TimeDate (onTime);
                this.offTime = new TimeDate (offTime);

                this.lightingTime = lightingTime;

                this.highTempLockout = highTempLockout;

                mode = Mode.Manual;

                onTimeOffset = 0;
                offTimeOffset = 0;

                lightingOn = MyState.Off;

                var plugControl = Power.AddOutlet (plug, this.name, MyState.Off, "Lighting");
                plugControl.ConditionChecker = OnPlugControl;
                Power.AddHandlerOnStateChange (plug, OnLightingPlugStateChange);
            }

            public bool OnPlugControl () {
                if (highTempLockout && Alarm.CheckAlarming (Temperature.highTemperatureAlarmIndex))
                    return false;

                TimeDate now = TimeDate.Now;
                if ((now.CompareTo (onTime) > 0) && (now.CompareTo (offTime) < 0)) {
                    //now is after on time and before off time
                    return true;
                } else {
                    if (mode == Mode.Auto) { // only update times if mode is auto
                        if (lightingOn == MyState.On) { // lights are on and are supposed to be off, update next on/off times
                            if (lightingTime == LightingTime.Daytime) {
                                // its dusk and daytime lighting on/off time is rise and set tomorrow respectfully
                                SetOnTime (sunRiseTomorrow);
                                SetOffTime (sunSetTomorrow);
                            } else { // lighting time is nighttime
                                // its dawn and nighttime lighting on/off time is set today because we're already on the current day,
                                // and rise time tomorrow respectfully
                                SetOnTime (sunSetToday);
                                SetOffTime (sunRiseTomorrow);
                            }
                        }
                    }
                    return false;
                }
            }

            public void OnLightingPlugStateChange (object sender, StateChangeEventArgs args) {
                lightingOn = args.state;
            }

            public void SetOnTime (TimeDate newOnTime) {
                onTime.SetTimeDate (newOnTime);
                onTime.AddMinutes (onTimeOffset);
            }

            public void SetOffTime (TimeDate newOffTime) {
                offTime.SetTimeDate (newOffTime);
                offTime.AddMinutes (offTimeOffset);
            }
    	}
    }
}

