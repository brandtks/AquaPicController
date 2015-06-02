using System;
using AquaPic.Utilites;
using AquaPic.PowerDriver;
using AquaPic.AlarmRuntime;
using AquaPic.TemperatureModule;
using AquaPic.CoilRuntime;

namespace AquaPic.LightingModule
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
            public IndividualControl plug;
            public Coil plugControl;
            public Mode mode;

            public LightingFixture (
                string name, 
                byte powerID, 
                byte plugID, 
                Time onTime, 
                Time offTime, 
                LightingTime lightingTime,
                bool highTempLockout)
            {
                this.name = name;

                plug.Group = powerID;
                plug.Individual = plugID;

                // sets time to today and whatever onTime and offTime are
                this.onTime = new TimeDate (onTime);
                this.offTime = new TimeDate (offTime);

                this.lightingTime = lightingTime;

                this.highTempLockout = highTempLockout;

                mode = Mode.Manual;

                onTimeOffset = 0;
                offTimeOffset = 0;

                lightingOn = MyState.Off;

                plugControl = Power.AddOutlet (plug, this.name, MyState.Off);
                plugControl.ConditionChecker = PlugControlHandler;
                Power.AddHandlerOnStateChange (plug, LightingPlugStateChange);

                if (this.highTempLockout) {
                    int alarmIdx = Temperature.HighTemperatureAlarmIndex;
                    Alarm.AddPostHandler (alarmIdx, sender => Power.AlarmShutdownOutlet (plug));
                }
            }

            protected bool PlugControlHandler () {
                if (highTempLockout && Alarm.CheckAlarming (Temperature.HighTemperatureAlarmIndex))
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

            public void LightingPlugStateChange (object sender, StateChangeEventArgs args) {
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

