using System;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.Sensors;

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
            public IntervalTimer pumpTimer;
            public uint maxPumpOnTime;
            public uint minPumpOffTime;

            public bool disableOnLowResevoirLevel;

            public AnalogLevelSensor reservoirLevel;

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
                uint minPumpOffTime
            ) {
                this.enable = enable;

                this.useAnalogSensor = useAnalogSensor;
                this.analogOnSetpoint = analogOnSetpoint;
                this.analogOffSetpoint = analogOffSetpoint;

                this.useFloatSwitch = useFloatSwitch;
                floatSwitchActivated = false;

                pumpOutlet = pumpPlug;
                pumpOnRequest = false;
                pumpTimer = IntervalTimer.GetTimer ("ATO");
                pumpTimer.TimerElapsedEvent += OnTimerElapsed;
                this.maxPumpOnTime = maxPumpOnTime;
                this.minPumpOffTime = minPumpOffTime;

                disableOnLowResevoirLevel = false;

                reservoirLevel = new AnalogLevelSensor ("Top Off Reservoir", IndividualControl.Empty, 0.0f, 0.0f, false, false, false);

                state = AutoTopOffState.Standby;
                atoFailAlarmIndex = Alarm.Subscribe ("Auto top off failed");

                if (this.enable) {
                    var c = Power.AddOutlet (pumpPlug, "ATO pump", MyState.Off, "ATO");
                    c.ConditionChecker = () => { return pumpOnRequest; };
                }
            }

            public void Run () {
                if (enable) {
                    if (reservoirLevel.sensorChannel.IsNotEmpty ()) {
                        reservoirLevel.enable = true;
                        reservoirLevel.Run ();
                    } else {
                        disableOnLowResevoirLevel = false;
                    }
                    
                    if ((!Alarm.CheckAlarming (highSwitchAlarmIndex)) || (!Alarm.CheckAlarming (analogSensor.highAnalogAlarmIndex))) {
                        switch (state) {
                        case AutoTopOffState.Standby:
                            {
                                pumpOnRequest = false;
                                bool usedAnalog = false;

                                if ((analogSensor.enable) && (useAnalogSensor)) {
                                    if (analogSensor.connected) {
                                        usedAnalog = true;

                                        if (analogSensor.level < analogOnSetpoint) {
                                            pumpOnRequest = true;
                                        }
                                    }
                                } 

                                if (useFloatSwitch) {
                                    // floatSwitchActivated is set by water level run function
                                    if (usedAnalog) {
                                        pumpOnRequest &= floatSwitchActivated; 
                                    } else {
                                        pumpOnRequest = floatSwitchActivated;
                                    }
                                }

                                if ((disableOnLowResevoirLevel) && (Alarm.CheckAlarming (reservoirLevel.lowAnalogAlarmIndex))) {
                                    Console.WriteLine ("ATO alarming based upon low reservoir level");
                                    pumpOnRequest = false;
                                    state = AutoTopOffState.Error;
                                    Alarm.Post (atoFailAlarmIndex);
                                }

                                if (pumpOnRequest) {
                                    state = AutoTopOffState.Filling;
                                    Logger.Add ("Starting auto top off");
                                    dataLogger.AddEntry ("ato started"); 
                                    pumpTimer.Reset ();
                                    pumpTimer.totalSeconds = maxPumpOnTime;
                                    pumpTimer.Start ();
                                }

                                break;
                            }
                        case AutoTopOffState.Filling:
                            pumpOnRequest = true;

                            // check analog sensor
                            if ((analogSensor.enable) && (useAnalogSensor)) {
                                if (!Alarm.CheckAlarming (analogSensor.sensorDisconnectedAlarmIndex)) { 
                                    if (analogSensor.level > analogOffSetpoint)
                                        pumpOnRequest = false;
                                }
                            }

                            // check float switch
                            if ((useFloatSwitch) && (!floatSwitchActivated)) {
                                pumpOnRequest = false;
                            }
                            
                            if (!pumpOnRequest) {
                                state = AutoTopOffState.Cooldown;
                                pumpTimer.Reset ();
                                Logger.Add ("Stopping auto top off. Runtime: {0} secs", pumpTimer.totalSeconds - pumpTimer.secondsRemaining);
                                dataLogger.AddEntry ("ato stopped"); 
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
                    pumpOnRequest = false;
                    state = AutoTopOffState.Error;
                    Alarm.Post (atoFailAlarmIndex);
                } else if (state == AutoTopOffState.Cooldown) {
                    state = AutoTopOffState.Standby;
                }
            }
        }
    }
}

