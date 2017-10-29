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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.Sensors;
using AquaPic.Equipment;

namespace AquaPic.Modules
{
    public enum AutoTopOffState {
        Off,
        Standby,
        Filling,
        Cooldown,
        Error
    }

    public class AutoTopOff
    {
        public static bool _enable;
        public static bool enabled {
            get {
                return _enable;
            }
            set {
                _enable = value;

                if (!pump.outlet.IsNotEmpty ()) {
                    _enable = false;
                }

                if (!_enable) {
                    Alarm.Clear (_failAlarmIndex);
                }
            }
        }

        public static bool useAnalogSensor;
        public static float analogOnSetpoint;
        public static float analogOffSetpoint;

        public static bool useFloatSwitch;
        public static bool floatSwitchActivated;

        public static Pump pump;
        public static IndividualControl pumpOutlet {
            get {
                return pump.outlet;
            }
            set {
                pump.Add (value);
                if (pump.outlet.IsNotEmpty ()) {
                    pump.SetGetter (() => pumpOnRequest);
                } else {
                    _enable = false;
                }
            }
        }

        public static bool pumpOnRequest;
        public static uint maximumRuntime;
        public static uint minimumCooldown;

        private static IntervalTimer timer;
        public static uint atoTime {
            get {
                if (_state == AutoTopOffState.Filling)
                    return maximumRuntime - timer.secondsRemaining;
                if (_state == AutoTopOffState.Cooldown)
                    return timer.secondsRemaining;

                return 0;
            }
        }

        private static AutoTopOffState _state;
        public static AutoTopOffState state {
            get {
                return _state;
            }
        }

        private static int _failAlarmIndex;
        public static int failedAlarmIndex {
            get {
                return _failAlarmIndex;
            }
        }

        static AutoTopOff () {
            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, "autoTopOffProperties.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));

                    try {
                        _enable = Convert.ToBoolean (jo["enable"]);
                    } catch {
                        _enable = false;
                    }

                    try {
                        useAnalogSensor = Convert.ToBoolean (jo["useAnalogSensor"]);
                    } catch {
                        useAnalogSensor = false;
                    }

                    string text = (string)jo["analogOnSetpoint"];
                    if (text.IsEmpty ()) {
                        analogOnSetpoint = 0.0f;
                        useAnalogSensor = false;
                    } else {
                        try {
                            analogOnSetpoint = Convert.ToSingle (text);
                        } catch {
                            analogOnSetpoint = 0.0f;
                            useAnalogSensor = false;
                        }
                    }

                    text = (string)jo["analogOffSetpoint"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        analogOffSetpoint = 0.0f;
                        useAnalogSensor = false;
                    } else {
                        try {
                            analogOffSetpoint = Convert.ToSingle (text);
                        } catch {
                            analogOnSetpoint = 0.0f;
                            useAnalogSensor = false;
                        }
                    }

                    try {
                        useFloatSwitch = Convert.ToBoolean (jo["useFloatSwitch"]);
                    } catch {
                        useFloatSwitch = false;
                    }

                    if (!useFloatSwitch && !useAnalogSensor)
                        _enable = false;

                    var ic = IndividualControl.Empty;
                    text = (string)jo["powerStrip"];
                    if (text.IsEmpty ()) {
                        _enable = false;
                    } else {
                        try {
                            ic.Group = Power.GetPowerStripIndex (text);
                        } catch {
                            ;
                        }
                    }

                    text = (string)jo["outlet"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        ic = IndividualControl.Empty;
                        _enable = false;
                    } else {
                        try {
                            ic.Individual = Convert.ToInt32 (text);
                        } catch {
                            ic = IndividualControl.Empty;
                            _enable = false;
                        }
                    }

                    uint maxPumpOnTime;
                    text = (string)jo["maxPumpOnTime"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        maxPumpOnTime = 0U;
                        _enable = false;
                    } else {
                        try {
                            maxPumpOnTime = Timer.ParseTime (text) / 1000;
                        } catch {
                            maxPumpOnTime = 0U;
                            _enable = false;
                        }
                    }

                    uint minPumpOffTime;
                    text = (string)jo["minPumpOffTime"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        minPumpOffTime = uint.MaxValue;
                        _enable = false;
                    } else {
                        try {
                            minPumpOffTime = Timer.ParseTime (text) / 1000;
                        } catch {
                            minPumpOffTime = uint.MaxValue;
                            _enable = false;
                        }
                    }
                }
            }

            if (_enable) {
                _state = AutoTopOffState.Standby;
            } else {
                _state = AutoTopOffState.Off;
            }

            _failAlarmIndex = Alarm.Subscribe ("Auto top off failed");

            floatSwitchActivated = false;

            timer = IntervalTimer.GetTimer ("ATO");
            timer.TimerElapsedEvent += OnTimerElapsed;

            pump = new Pump (pumpOutlet, "ATO pump", MyState.Off, "ATO");
            if (pump.outlet.IsNotEmpty ()) {
                pump.SetGetter (() => pumpOnRequest);
            }
            pumpOnRequest = false;
        }

        public static void Run () {
            if (_enable) {
                switch (state) {
                case AutoTopOffState.Standby: 
                    {
                        pumpOnRequest = false;
                        bool usedAnalog = false;

                        if ((WaterLevel.analogSensorEnabled) && (useAnalogSensor)) {
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

                        if (pumpOnRequest) {
                            _state = AutoTopOffState.Filling;
                            Logger.Add ("Starting auto top off");
                            WaterLevel.dataLogger.AddEntry ("ato started"); 
                            timer.Reset ();
                            timer.totalSeconds = maximumRuntime;
                            timer.Start ();
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
                        atoTimer.Reset ();
                        Logger.Add ("Stopping auto top off. Runtime: {0} secs", atoTimer.totalSeconds - atoTimer.secondsRemaining);
                        dataLogger.AddEntry ("ato stopped"); 
                        atoTimer.totalSeconds = minPumpOffTime;
                        atoTimer.Start ();
                    }

                    break;
                case AutoTopOffState.Cooldown:
                case AutoTopOffState.Error:
                default:
                    pumpOnRequest = false;
                    break;
                }
            } else {
                _state = AutoTopOffState.Off;
                pumpOnRequest = false;
            }
        }

        protected static void OnTimerElapsed (object sender, TimerElapsedEventArgs args) {
            if (state == AutoTopOffState.Filling) {
                pumpOnRequest = false;
                _state = AutoTopOffState.Error;
                Alarm.Post (_failAlarmIndex);
            } else if (state == AutoTopOffState.Cooldown) {
                _state = AutoTopOffState.Standby;
            }
        }

        /**************************************************************************************************************/
        /* Auto Topoff                                                                                                */
        /**************************************************************************************************************/
        public static bool ClearAtoAlarm () {
            if (ato.state == AutoTopOffState.Error) {
                if (Alarm.CheckAcknowledged (atoFailedAlarmIndex)) {
                    Alarm.Clear (atoFailedAlarmIndex);
                    ato.state = AutoTopOffState.Standby;
                    return true;
                }
            }

            return false;
        }

        public static void SetAtoReservoirCalibrationData (float zeroValue, float fullScaleActual, float fullScaleValue) {
            if (fullScaleValue <= zeroValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            if (fullScaleActual > 15.0f)
                throw new ArgumentException ("Full scale actual can't be greater than 15");

            ato.reservoirLevel.zeroValue = zeroValue;
            ato.reservoirLevel.fullScaleActual = fullScaleActual;
            ato.reservoirLevel.fullScaleValue = fullScaleValue;

            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, "waterLevelProperties.json");

            string jstring = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (jstring);

            jo["AutoTopOff"]["reservoirZeroCalibrationValue"] = ato.reservoirLevel.zeroValue.ToString ();
            jo["AutoTopOff"]["reservoirFullScaleCalibrationActual"] = ato.reservoirLevel.fullScaleActual.ToString ();
            jo["AutoTopOff"]["reservoirFullScaleCalibrationValue"] = ato.reservoirLevel.fullScaleValue.ToString ();

            File.WriteAllText (path, jo.ToString ());
        }
    }
}

