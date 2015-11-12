using System;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Modules
{
    public enum AutoTopOffState {
        Standby,
        Filling,
        Cooldown,
        Error
    }

    public partial class WaterLevel
    {
        private class AutoTopOff
        {
            public bool enable;

            public bool useAnalogSensor;
            public float analogOnSetpoint;
            public float analogOffSetpoint;

            public bool useFloatSwitch;
            public bool floatSwitchActivated;

            public IndividualControl pumpOutlet;
            public bool pumpOnRequest;
            public DeluxeTimer pumpTimer;
            public uint maxPumpOnTime;
            public uint minPumpOffTime;

            public AutoTopOffState state;
            public int atoFailAlarmIndex;

            public AutoTopOff (
                bool enable,
                bool useAnalogSensor,
                float analogOnSetpoint,
                float analogOffSetpoint,
                bool useFloatSwitch,
                IndividualControl pumpPlug,
                uint maxPumpOnTime,
                uint minPumpOffTime) 
            {
                this.enable = enable;

                this.useAnalogSensor = useAnalogSensor;
                this.analogOnSetpoint = analogOnSetpoint;
                this.analogOffSetpoint = analogOffSetpoint;

                this.useFloatSwitch = useFloatSwitch;
                floatSwitchActivated = false;

                this.pumpOutlet = pumpPlug;
                pumpOnRequest = false;
                pumpTimer = DeluxeTimer.GetTimer ("ATO");
                pumpTimer.TimerElapsedEvent += OnTimerElapsed;
                this.maxPumpOnTime = maxPumpOnTime;
                this.minPumpOffTime = minPumpOffTime;

                state = AutoTopOffState.Standby;
                atoFailAlarmIndex = Alarm.Subscribe ("Auto top off failed");

                if (this.enable) {
                    var c = Power.AddOutlet (pumpPlug, "ATO pump", MyState.Off, "ATO");
                    c.ConditionChecker = () => { return pumpOnRequest; };
                }
            }

            public void Run () {
                if (enable) {
                    if ((!Alarm.CheckAlarming (highSwitchAlarmIndex)) || (!Alarm.CheckAlarming (analogSensor.highAnalogAlarmIndex))) {
                        switch (state) {
                        case AutoTopOffState.Standby:
                            {
                                pumpOnRequest = false;
                                bool usedAnalog = false;

                                if ((analogSensor.enable) && (useAnalogSensor)) {
                                    if (!Alarm.CheckAlarming (analogSensor.sensorDisconnectedAlarmIndex)) {
                                        usedAnalog = true;

                                        if (analogSensor.waterLevel < analogOnSetpoint)
                                            pumpOnRequest = true;
                                    }
                                } 

                                if (useFloatSwitch) {
                                    if (usedAnalog)
                                        pumpOnRequest &= floatSwitchActivated; // set during water level run function
                                    else
                                        pumpOnRequest = floatSwitchActivated;
                                }

                                if (pumpOnRequest) {
                                    state = AutoTopOffState.Filling;
                                    Logger.Add ("Starting auto top off");
                                    pumpTimer.totalSeconds = maxPumpOnTime;
                                    pumpTimer.Start ();
                                }

                                break;
                            }
                        case AutoTopOffState.Filling:
                            pumpOnRequest = true;

                            if ((analogSensor.enable) && (useAnalogSensor)) {
                                if (!Alarm.CheckAlarming (analogSensor.sensorDisconnectedAlarmIndex)) { 
                                    if (analogSensor.waterLevel > analogOffSetpoint)
                                        pumpOnRequest = false;
                                }
                            }

                            if ((useFloatSwitch) && (!floatSwitchActivated))
                                pumpOnRequest = false;

                            if (!pumpOnRequest) {
                                state = AutoTopOffState.Cooldown;
                                pumpTimer.Reset ();
                                Logger.Add (string.Format ("Stopping auto top off. Runtime: {0} secs", pumpTimer.totalSeconds - pumpTimer.secondsRemaining));
                                pumpTimer.totalSeconds = minPumpOffTime;
                                pumpTimer.Start ();
                            }

                            break;
                        case AutoTopOffState.Cooldown:
                        case AutoTopOffState.Error:
                        default:
                            pumpOnRequest = false;
                            break;
                        }
                    } else {
                        state = AutoTopOffState.Standby;
                        pumpOnRequest = false;
                    }
                } else {
                    state = AutoTopOffState.Standby;
                    pumpOnRequest = false;
                }
            }

            protected void OnTimerElapsed (object sender, TimerElapsedEventArgs args) {
                if (state == AutoTopOffState.Filling) {
                    state = AutoTopOffState.Error;
                    pumpOnRequest = false;
                    Alarm.Post (atoFailAlarmIndex);
                } else if (state == AutoTopOffState.Cooldown) {
                    state = AutoTopOffState.Standby;
                }
            }
        }
    }
}

