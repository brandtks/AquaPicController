using System;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.PowerDriver;
using AquaPic.SerialBus;
using AquaPic.AlarmRuntime;
using AquaPic.TemperatureModule;
using AquaPic.CoilCondition;

namespace AquaPic.LightingModule
{
    public partial class Lighting 
    {
        private class LightingFixture
    	{
            public string name;
            public int timeOnOffsetMinutes;
            public int timeOffOffsetMinutes;
            public TimeDate timeOn;
            public TimeDate timeOff;
            public LightingTime lightingTime;
            public Mode mode;
            public MyState lightingOn;
            public bool highTempLockout;
            public IndividualControl plug;
            public Coil plugControl;

            public LightingFixture (
                byte powerID,
                byte plugID,
                string name,
                int timeOnOffsetMinutes,
                int timeOffOffsetMinutes,
                LightingTime lightingTime,
                bool highTempLockout
            ) {
                // Power Plug Setup
                this.plug.Group = powerID;
                this.plug.Individual = plugID;
                plugControl = Power.AddPlug (this.plug, name, MyState.Off, true);
                Power.AddHandlerOnStateChange (this.plug, LightingPlugStateChange);

                Condition plugControlCondition = new Condition (name + " plug control");
                plugControlCondition.CheckHandler += PlugControlHandler;
                plugControl.Conditions.Script = plugControlCondition.Name;

                this.name = name;
                this.timeOnOffsetMinutes = timeOnOffsetMinutes;
                this.timeOffOffsetMinutes = timeOffOffsetMinutes;
                this.lightingTime = lightingTime;
                this.mode = Mode.Auto;
                this.lightingOn = MyState.Off;
                this.highTempLockout = highTempLockout;

                timeOn = TimeDate.Now;
                timeOff = TimeDate.Now;

                if (this.highTempLockout) {
                    int alarmIdx = Temperature.HighTemperatureAlarmIndex;
                    Alarm.AddPostHandler (alarmIdx, sender => Power.AlarmShutdownPlug (plug));
                }
    		}

            public void SetOnOffTime (TimeDate onTime, TimeDate offTime) {
                if (mode == Mode.Auto) {
                    timeOn.setTimeDate (onTime);
                    timeOff.setTimeDate (offTime);

                    timeOn.addMinToTime (timeOnOffsetMinutes);
                    timeOff.addMinToTime (timeOffOffsetMinutes);
                }
            }

            public void SetOnTime (TimeDate onTime) {
                if (mode == Mode.Auto) {
                    timeOn.setTimeDate (onTime);

                    timeOn.addMinToTime (timeOnOffsetMinutes);
                }
            }

            public void SetOffTime (TimeDate offTime) {
                if (mode == Mode.Auto) {
                    timeOff.setTimeDate (offTime);

                    timeOff.addMinToTime (timeOffOffsetMinutes);
                }
            }

            protected bool PlugControlHandler () {
                if (Power.GetPlugMode (plug) == Mode.Manual) {
                    if (Power.GetManualPlugState (plug) == MyState.On)
                        return true;
                    else
                        return false;
                } else {
                    if (highTempLockout && Alarm.CheckAlarming (Temperature.HighTemperatureAlarmIndex))
                        return false;

                    TimeDate now = TimeDate.Now;
                    if ((timeOn.compareTo (now) > 0) && (timeOff.compareTo (now) < 0)) {
                        //time is after sun rise and before sun set
                        return true;
                    } else {
                        return false;
                    }
                }
            }

            public void LightingPlugStateChange (object sender, StateChangeEventArgs args) {
                lightingOn = args.state;
            }
    	}
    }
}

