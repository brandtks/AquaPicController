using System;
using AquaPic.Globals;
using AquaPic.Utilites;
using AquaPic.PowerDriver;
using AquaPic.SerialBus;
using AquaPic.AlarmDriver;
using AquaPic.TemperatureDriver;
using AquaPic.CoilCondition;

namespace AquaPic.LightingDriver
{
    public partial class Lighting 
    {
        private class LightingFixture
    	{
            public string name;
            public int sunRiseOffset;
            public int sunSetOffset;
            public Time minSunRise;
            public Time maxSunSet;
            public TimeDate sunRise;
            public TimeDate sunSet;
            public Mode mode;
            public MyState lightingOn;
            public bool highTempLockout;
            public IndividualControl plug;
            public Coil PlugControl;
            //public Condition requestedState;

            public LightingFixture (
                byte powerID,
                byte plugID,
                string name,               
                int sunRiseOffset, 
                int sunSetOffset, 
                Time minSunRise, 
                Time maxSunSet,  
                bool highTempLockout) 
            {
                this.plug.Group = powerID;
                this.plug.Individual = plugID;
                PlugControl = Power.AddPlug (this.plug, name, MyState.Off, true);
                Power.AddHandlerOnStateChange (this.plug, LightingPlugStateChange);

                this.name = name;
                this.sunRiseOffset = sunRiseOffset;
                this.sunSetOffset = sunSetOffset;
                this.minSunRise = minSunRise;
                this.maxSunSet = maxSunSet;
                this.sunRise = TimeDate.Zero;
                this.sunSet = TimeDate.Zero;
                this.mode = Mode.Auto;
                this.lightingOn = MyState.Off;
                this.highTempLockout = highTempLockout;

                Condition rs = new Condition (name + " requested state");
                rs.CheckHandler += OnRequestedState;
//                PlugControl.Conditions.Add (requestedState);

                PlugControl.Conditions.Script = "AND " + rs.Name + " AND NOT loss of power";

//                Condition c = ConditionLocker.GetCondition ("Loss of power");
//                if (c != null)
//                    PlugControl.Conditions.Add (c);

                if (this.highTempLockout) {
//                    PlugControl.Conditions.Add (ConditionLocker.GetCondition ("High Temperature"));

                    PlugControl.Conditions.Script += " AND NOT high temperature";

                    int alarmIdx = Temperature.HighTemperatureAlarmIndex;
                    if (alarmIdx == -1)
                        return;
                    Alarm.AddPostHandler (alarmIdx, sender => Power.AlarmShutdownPlug (plug));
                }

                // @test
                this.sunRise.setTimeDate (this.minSunRise);
                this.sunSet.setTimeDate (this.maxSunSet);
    		}

            public void SetSunRiseSet (TimeDate rise, TimeDate sSet) {
                if (mode == Mode.Manual) {
                    sunRise.updateDateToToday ();
                    sunSet.updateDateToToday ();
                    return;
                }

                if (rise.compareToTime (minSunRise) < 0) // rise is before min
                    sunRise.setTimeDate (minSunRise);
                else
                    sunRise.setTimeDate (rise);

                if (sSet.compareToTime (maxSunSet) > 0) // sSet is after max
                    sunSet.setTimeDate (maxSunSet);
                else
                    sunSet.setTimeDate (sSet);

                sunRise.addMinToDate (sunRiseOffset);
                sunSet.addMinToDate (sunSetOffset);
            }

            public void SetSunRise (TimeDate rise) {
                if (mode == Mode.Manual) {
                    sunSet.updateDateToToday ();
                    return;
                }
                    
                if (rise.compareToTime (minSunRise) < 0) // rise is before min
                    sunRise.setTimeDate (minSunRise);
                else
                    sunRise.setTimeDate (rise);

                sunRise.addMinToDate (sunRiseOffset);
            }

            public void SetSunSet (TimeDate sSet) {
                if (mode == Mode.Manual) {
                    sunSet.updateDateToToday ();
                    return;
                }

                if (sSet.compareToTime (maxSunSet) > 0) // sSet is after max
                    sunSet.setTimeDate (maxSunSet);
                else
                    sunSet.setTimeDate (sSet);

                sunSet.addMinToDate (sunSetOffset);
            }

            protected bool OnRequestedState () {
                if ((mode == Mode.AutoAuto) || (mode == Mode.Auto)) {
                    TimeDate now = TimeDate.Now;
                    if ((sunRise.compareTo (now) > 0) && (sunSet.compareTo (now) < 0)) {
                        //time is after sun rise and before sun set
                        return true;
                    } else {
                        return false;
                    }
                }
                return false;
            }

            // set to protected since I don't want direct control of setting plugs
//            protected virtual void OnLightsOnOutput () {
//                if (Power.GetPlugState (plug) == MyState.Off)
//                    Power.SetPlugState (plug, MyState.On);
//            }
//
//            protected virtual void OnLightsOffOutput () {
//                if (Power.GetPlugState (plug) == MyState.On)
//                    Power.SetPlugState (plug, MyState.Off);
//            }

            public void LightingPlugStateChange (object sender, StateChangeEventArgs args) {
                lightingOn = args.state;
            }
    	}
    }
}

