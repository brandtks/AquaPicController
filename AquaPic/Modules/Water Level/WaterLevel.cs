using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        private static AnalogSensor analogSensor;
        private static AutoTopOff ato;
        private static List<FloatSwitch> floatSwitches;

        private static int highSwitchAlarmIndex;
        private static int lowSwitchAlarmIndex;
//        private static int switchAnalogMismatchAlarmIndex;

        /**************************************************************************************************************/
        /* Analog water sensor                                                                                        */
        /**************************************************************************************************************/
        public static float highAnalogLevelAlarmSetpoint {
            get {
                return analogSensor.highAlarmStpnt;
            }
            set {
                analogSensor.highAlarmStpnt = value;
            }
        }

        public static float lowAnalogLevelAlarmSetpoint {
            get {
                return analogSensor.lowAlarmStpnt;
            }
            set {
                analogSensor.lowAlarmStpnt = value;
            }
        }

        public static float analogWaterLevel {
            get {
                return analogSensor.waterLevel;
            }
        }

        public static float analogSensorZeroCalibrationValue {
            get {
                return analogSensor.zeroValue;
            }
        }

        public static float analogSensorFullScaleCalibrationActual {
            get {
                return analogSensor.fullScaleActual;
            }
        }

        public static float analogSensorFullScaleCalibrationValue {
            get {
                return analogSensor.fullScaleValue;
            }
        }

        public static IndividualControl analogSensorChannel {
            get {
                return analogSensor.sensorChannel;
            }
            set {
                if (analogSensor.sensorChannel.IsNotEmpty ()) {
                    AquaPicDrivers.AnalogInput.RemoveChannel (analogSensor.sensorChannel);
                }
                analogSensor.sensorChannel = value;
                AquaPicDrivers.AnalogInput.AddChannel (analogSensor.sensorChannel, "Water Level");
                    
            }
        }

        public static bool analogSensorEnabled {
            get {
                return analogSensor.enable;
            }
            set {
                analogSensor.enable = value;
                if (analogSensor.enable) {
                    analogSensor.SubscribeToAlarms ();
                
                    try {
                        AquaPicDrivers.AnalogInput.AddChannel (analogSensor.sensorChannel, "Water Level");
                    } catch (Exception) {
                        ; //channel already added
                    }
                } else {
                    if (Alarm.CheckAlarming (analogSensor.highAnalogAlarmIndex))
                        Alarm.Clear (analogSensor.highAnalogAlarmIndex);

                    if (Alarm.CheckAlarming (analogSensor.lowAnalogAlarmIndex))
                        Alarm.Clear (analogSensor.lowAnalogAlarmIndex);

                    if (Alarm.CheckAlarming (analogSensor.sensorDisconnectedAlarmIndex))
                        Alarm.Clear (analogSensor.sensorDisconnectedAlarmIndex);
                }
            }
        }

        public static DataLogger dataLogger {
            get {
                return analogSensor.dataLogger;
            }
        }
            
        /**************************************************************************************************************/
        /* Auto Top-off                                                                                               */
        /**************************************************************************************************************/
        public static bool atoEnabled {
            get {
                return ato.enable;
            }
            set {
                ato.enable = value;
                if (ato.enable) {
                    ato.atoFailAlarmIndex = Alarm.Subscribe ("Auto top off failed");

                    try {
                        Coil c = Power.AddOutlet (ato.pumpOutlet, "ATO pump", MyState.Off, "ATO");
                        c.ConditionChecker = () => {
                            return ato.pumpOnRequest;
                        };
                    } catch (Exception) {
                        ; // Outlet already added
                    }
                } else {
                    if (Alarm.CheckAlarming (ato.atoFailAlarmIndex))
                        Alarm.Clear (ato.atoFailAlarmIndex);
                }
            }
        }

        public static bool atoUseAnalogSensor {
            get {
                return ato.useAnalogSensor;
            }
            set {
                ato.useAnalogSensor = value;
            }
        }

        public static bool atoUseFloatSwitch {
            get {
                return ato.useFloatSwitch;
            }
            set {
                ato.useFloatSwitch = value;
            }
        }

        public static AutoTopOffState atoState {
            get {
                return ato.state;
            }
        }

        public static uint atoTime {
            get {
                if (atoState == AutoTopOffState.Filling)
                    return ato.maxPumpOnTime - ato.pumpTimer.secondsRemaining;
                else if (atoState == AutoTopOffState.Cooldown)
                    return ato.pumpTimer.secondsRemaining;
                else
                    return 0;
            }
        }

        public static uint atoMaxRuntime {
            get {
                return ato.maxPumpOnTime;
            }
            set {
                ato.maxPumpOnTime = value;
            }
        }

        public static uint atoCooldown {
            get {
                return ato.minPumpOffTime;
            }
            set {
                ato.minPumpOffTime = value;
            }
        }

        public static IndividualControl atoPumpOutlet {
            get {
                return ato.pumpOutlet;
            }
            set {
                if (ato.pumpOutlet.IsNotEmpty ())
                    Power.RemoveOutlet (ato.pumpOutlet);
                ato.pumpOutlet = value;
                Coil c = Power.AddOutlet (ato.pumpOutlet, "ATO pump", MyState.Off, "ATO");
                c.ConditionChecker = () => { return ato.pumpOnRequest; };
            }
        }

        public static float atoAnalogOnSetpoint {
            get {
                return ato.analogOnSetpoint;
            }
            set {
                ato.analogOnSetpoint = value;
            }
        }

        public static float atoAnalogOffSetpoint {
            get {
                return ato.analogOffSetpoint;
            }
            set {
                ato.analogOffSetpoint = value;
            }
        }

        public static int atoFailedAlarmIndex {
            get {
                return ato.atoFailAlarmIndex;
            }
        }

        /**************************************************************************************************************/
        /* Float Switches                                                                                             */
        /**************************************************************************************************************/
        public static int floatSwitchCount {
            get {
                return floatSwitches.Count;
            }
        }


        /**************************************************************************************************************/
        /* Constructor                                                                                                */
        /**************************************************************************************************************/
        static WaterLevel () {
            string path = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "waterLevelProperties.json");

            using (StreamReader reader = File.OpenText (path)) {
                JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));

                bool enable = Convert.ToBoolean (jo ["enableAnalogSensor"]);

                float highAlarmSetpoint;
                string text = (string)jo ["highAnalogLevelAlarmSetpoint"];
                if (string.IsNullOrWhiteSpace (text)) {
                    highAlarmSetpoint = 0.0f;
                    enable = false;
                } else
                    highAlarmSetpoint = Convert.ToSingle (text);


                float lowAlarmSetpoint;
                text = (string)jo ["lowAnalogLevelAlarmSetpoint"];
                if (string.IsNullOrWhiteSpace (text)) {
                    lowAlarmSetpoint = 0.0f;
                    enable = false;
                } else
                    lowAlarmSetpoint = Convert.ToSingle (text);

                IndividualControl ic;
                text = Convert.ToString (jo ["inputCard"]);
                if (string.IsNullOrWhiteSpace (text)) {
                    ic = IndividualControl.Empty;
                    enable = false;
                } else
                    ic.Group = AquaPicDrivers.AnalogInput.GetCardIndex (text);

                text = (string)jo ["channel"];
                if (string.IsNullOrWhiteSpace (text)) {
                    ic = IndividualControl.Empty;
                    enable = false;
                } else
                    ic.Individual = Convert.ToInt32 (text);

                analogSensor = new AnalogSensor (enable, highAlarmSetpoint, lowAlarmSetpoint, ic);

                text = (string)jo ["zeroCalibrationValue"];
                if (string.IsNullOrWhiteSpace (text))
                    analogSensor.zeroValue = 819.2f;
                else {
                    try {
                        analogSensor.zeroValue = Convert.ToSingle (text);
                    } catch {
                        analogSensor.zeroValue = 819.2f;
                    }
                }

                text = (string)jo ["fullScaleCalibrationActual"];
                if (string.IsNullOrWhiteSpace (text))
                    analogSensor.fullScaleActual = 15.0f;
                else {
                    try {
                        analogSensor.fullScaleActual = Convert.ToSingle (text);
                    } catch {
                        analogSensor.fullScaleActual = 15.0f;
                    }
                }

                text = (string)jo ["fullScaleCalibrationValue"];
                if (string.IsNullOrWhiteSpace (text))
                    analogSensor.fullScaleValue = 4096.0f;
                else {
                    try {
                        analogSensor.fullScaleValue = Convert.ToSingle (text);
                    } catch {
                        analogSensor.fullScaleValue = 4096.0f;
                    }
                }

                //Float Switches
                floatSwitches = new List<FloatSwitch> ();
                JArray ja = (JArray)jo ["floatSwitches"];
                foreach (var jt in ja) {
                    JObject obj = jt as JObject;

                    string name = (string)obj ["name"];
                    ic.Group = AquaPicDrivers.DigitalInput.GetCardIndex ((string)obj ["inputCard"]);
                    ic.Individual = Convert.ToInt32 (obj ["channel"]);
                    float physicalLevel = Convert.ToSingle (obj ["physicalLevel"]);
                    SwitchType type = (SwitchType)Enum.Parse (typeof (SwitchType), (string)obj ["switchType"]);
                    SwitchFunction function = (SwitchFunction)Enum.Parse (typeof (SwitchFunction), (string)obj ["switchFuntion"]);
                    string tString = (string)obj ["timeOffset"];
                    uint timeOffset = Timer.ParseTime (tString);

                    if ((function == SwitchFunction.HighLevel) && (type != SwitchType.NormallyClosed))
                        Logger.AddWarning ("High level switch should be normally closed");
                    else if ((function == SwitchFunction.LowLevel) && (type != SwitchType.NormallyClosed))
                        Logger.AddWarning ("Low level switch should be normally closed");
                    else if ((function == SwitchFunction.ATO) && (type != SwitchType.NormallyOpened))
                        Logger.AddWarning ("ATO switch should be normally opened");
                    else if (function == SwitchFunction.None) {
                        Logger.AddWarning ("Can't add a float switch with no function");
                        continue; // skip adding float switch since it does nothing
                    }

                    AddFloatSwitch (name, ic, physicalLevel, type, function, timeOffset);
                }

                //Auto Top Off
                JObject joAto = (JObject)jo ["AutoTopOff"];

                enable = Convert.ToBoolean (joAto ["enableAto"]);

                bool useAnalogSensor = Convert.ToBoolean (joAto ["useAnalogSensor"]);

                float analogOnSetpoint;
                text = (string)joAto ["analogOnSetpoint"];
                if (string.IsNullOrWhiteSpace (text)) {
                    analogOnSetpoint = 0.0f;
                    useAnalogSensor = false;
                } else 
                    analogOnSetpoint = Convert.ToSingle (text);

                float analogOffSetpoint;
                text = (string)joAto ["analogOffSetpoint"];
                if (string.IsNullOrWhiteSpace (text)) {
                    analogOffSetpoint = 0.0f;
                    useAnalogSensor = false;
                } else 
                    analogOffSetpoint = Convert.ToSingle (text);

                bool useFloatSwitch = Convert.ToBoolean (joAto ["useFloatSwitch"]);

                if (!useFloatSwitch && !useAnalogSensor)
                    enable = false;

                text = (string)joAto ["powerStrip"];
                if (string.IsNullOrWhiteSpace (text)) {
                    ic = IndividualControl.Empty;
                    enable = false;
                } else
                    ic.Group = Power.GetPowerStripIndex (text);

                text = (string)joAto ["outlet"];
                if (string.IsNullOrWhiteSpace (text)) {
                    ic = IndividualControl.Empty;
                    enable = false;
                } else
                    ic.Individual = Convert.ToInt32 (text);

                uint maxPumpOnTime;
                text = (string)joAto ["maxPumpOnTime"];
                if (string.IsNullOrWhiteSpace (text)) {
                    maxPumpOnTime = 0U;
                    enable = false;
                } else
                    maxPumpOnTime = Timer.ParseTime (text) / 1000;

                uint minPumpOffTime;
                text = (string)joAto ["minPumpOffTime"];
                if (string.IsNullOrWhiteSpace (text)) {
                    minPumpOffTime = uint.MaxValue;
                    enable = false;
                } else
                    minPumpOffTime = Timer.ParseTime (text) / 1000;

                ato = new AutoTopOff (enable, useAnalogSensor, analogOnSetpoint, analogOffSetpoint, useFloatSwitch, ic, maxPumpOnTime, minPumpOffTime);
            }

            lowSwitchAlarmIndex = Alarm.Subscribe ("Low Water Level, Float Switch");
            highSwitchAlarmIndex = Alarm.Subscribe ("High Water Level, Float Switch");
//            switchAnalogMismatchAlarmIndex = Alarm.Subscribe ("Float switch and analog water sensor mismatch");

            TaskManager.AddCyclicInterrupt ("Water Level", 1000, Run);
        }

        public static void Init () {
            Logger.Add ("Initializing Water Level");
        }

        public static void Run () {
            analogSensor.Run ();

//            bool mismatch = false;
            ato.useFloatSwitch = false; //set 'use float switch' to false, if no ATO float switch in found it remains false
            foreach (var s in floatSwitches) {
                bool state = AquaPicDrivers.DigitalInput.GetChannelValue (s.channel);
                bool activated;

                if (s.type == SwitchType.NormallyClosed)
                    state = !state; //normally closed switches are reversed

                activated = s.odt.Evaluate (s.activated != state); // if current state and switch activation do not match start timer
                if (activated) // once timer has finished, toggle switch activation
                    s.activated = !s.activated;

//                if ((s.activated) && (analogSensor.enable) && (analogSensor.connected)) {
//                    if (s.type == SwitchType.NormallyClosed) {
//                        if (analogSensor.waterLevel > (s.physicalLevel + 1.0f)) {
//                            mismatch = true;
//                            Logger.AddInfo ("Float switch {0} is reporting a mismatch with analog sensor", s.name);
//                        }
//                    } else {
//                        if (analogSensor.waterLevel < (s.physicalLevel - 1.0f)) {
//                            mismatch = true;
//                            Logger.AddInfo ("Float switch {0} is reporting a mismatch with analog sensor", s.name);
//                        }
//                    }
//                }

                if (s.function == SwitchFunction.HighLevel) {
                    if (s.activated)
                        Alarm.Post (highSwitchAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (highSwitchAlarmIndex))
                            Alarm.Clear (highSwitchAlarmIndex);
                    }
                } else if (s.function == SwitchFunction.LowLevel) {
                    if (s.activated)
                        Alarm.Post (lowSwitchAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (lowSwitchAlarmIndex))
                            Alarm.Clear (lowSwitchAlarmIndex);
                    }
                } else if (s.function == SwitchFunction.ATO) {
                    ato.useFloatSwitch = true; //we found an ATO float switch use it
                    ato.floatSwitchActivated = s.activated;
                }
            }

//            if (mismatch)
//                Alarm.Post (switchAnalogMismatchAlarmIndex);
//            else {
//                if (Alarm.CheckAlarming (switchAnalogMismatchAlarmIndex))
//                    Alarm.Clear (switchAnalogMismatchAlarmIndex);
//            }

            ato.Run ();
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

        /**************************************************************************************************************/
        /* Analog Sensor                                                                                              */
        /**************************************************************************************************************/
        public static void SetCalibrationData (float zeroValue, float fullScaleActual, float fullScaleValue) {
            if (fullScaleValue <= zeroValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            if (fullScaleActual > 15.0f)
                throw new ArgumentException ("Full scale actual can't be greater than 15");

            analogSensor.zeroValue = zeroValue;
            analogSensor.fullScaleActual = fullScaleActual;
            analogSensor.fullScaleValue = fullScaleValue;

            string path = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "waterLevelProperties.json");

            string jstring = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (jstring);

            jo ["zeroCalibrationValue"] = analogSensor.zeroValue.ToString ();
            jo ["fullScaleCalibrationActual"] = analogSensor.fullScaleActual.ToString ();
            jo ["fullScaleCalibrationValue"] = analogSensor.fullScaleValue.ToString ();

            File.WriteAllText (path, jo.ToString ());
        }

        /**************************************************************************************************************/
        /* Float Switches                                                                                             */
        /**************************************************************************************************************/
        public static void AddFloatSwitch (
            string name, 
            IndividualControl ic, 
            float physicalLevel, 
            SwitchType type,
            SwitchFunction function,
            uint timeOffset)
        {
            if (FloatSwitchNameOk (name)) {
                if (FloatSwitchFunctionOk (function)) {
                    FloatSwitch fs = new FloatSwitch (timeOffset);
                    fs.name = name;
                    fs.channel = ic;
                    fs.physicalLevel = physicalLevel;
                    fs.type = type;
                    fs.function = function;

                    AquaPicDrivers.DigitalInput.AddChannel (fs.channel, name);

                    floatSwitches.Add (fs);
                } else {
                    throw new Exception (string.Format ("Float Switch: {0} function already exists", function));
                }
            } else {
                throw new Exception (string.Format ("Float Switch: {0} already exists", name));
            }
        }

        public static void RemoveFloatSwitch (int floatSwitchId) {
            if ((floatSwitchId >= 0) && (floatSwitchId < floatSwitches.Count)) {
                FloatSwitch fs = floatSwitches [floatSwitchId];
                AquaPicDrivers.DigitalInput.RemoveChannel (fs.channel);
                floatSwitches.Remove (fs);
                return;
            }

            throw new ArgumentOutOfRangeException ("floatSwitchId");
        }

        public static void SetFloatSwitchName (int switchId, string name) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            if (FloatSwitchNameOk (name)) {
                floatSwitches [switchId].name = name;
            } else 
                throw new Exception (string.Format ("Float Switch: {0} already exists", name));
        }

        public static bool FloatSwitchNameOk (string name) {
            try {
                GetFloatSwitchIndex (name);
                return false;
            } catch (ArgumentException) {
                return true;
            }
        }

        public static bool FloatSwitchFunctionOk (SwitchFunction function) {
            if (function == SwitchFunction.None)
                return true;

            foreach (var fs in floatSwitches) {
                if (function == fs.function)
                    return false;
            }

            return true;
        }

        public static string[] GetAllFloatSwitches () {
            string[] names = new string[floatSwitches.Count];
            for (int i = 0; i < floatSwitches.Count; ++i)
                names [i] = floatSwitches [i].name;
            return names;
        }

        public static string GetFloatSwitchName (int switchId) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            return floatSwitches [switchId].name;
        }

        public static int GetFloatSwitchIndex (string name) {
            for (int i = 0; i < floatSwitches.Count; ++i) {
                if (string.Equals (name, floatSwitches [i].name, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (name + " does not exists");
        }

        public static IndividualControl GetFloatSwitchIndividualControl (int switchId) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            return floatSwitches [switchId].channel;
        }

        public static void SetFloatSwitchIndividualControl (int switchId, IndividualControl ic) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            AquaPicDrivers.DigitalInput.RemoveChannel (ic);
            floatSwitches [switchId].channel = ic;
            AquaPicDrivers.DigitalInput.AddChannel (floatSwitches [switchId].channel, floatSwitches [switchId].name);
        }

        public static float GetFloatSwitchPhysicalLevel (int switchId) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            return floatSwitches [switchId].physicalLevel; 
        }

        public static void SetFloatSwitchPhysicalLevel (int switchId, float physicalLevel) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            floatSwitches [switchId].physicalLevel = physicalLevel;
        }

        public static SwitchType GetFloatSwitchType (int switchId) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            return floatSwitches [switchId].type; 
        }

        public static void SetFloatSwitchType (int switchId, SwitchType type) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            if (floatSwitches [switchId].type != type) // if swapping between NO and NC activation is reversed
                floatSwitches [switchId].activated = !floatSwitches [switchId].activated;

            floatSwitches [switchId].type = type;
        }

        public static SwitchFunction GetFloatSwitchFunction (int switchId) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            return floatSwitches [switchId].function; 
        }

        public static void SetFloatSwitchFunction (int switchId, SwitchFunction function) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            if (FloatSwitchFunctionOk (function))
                floatSwitches [switchId].function = function;
            else 
                throw new Exception (string.Format ("Float Switch: {0} function already exists", function));
        }

        public static uint GetFloatSwitchTimeOffset (int switchId) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            return floatSwitches [switchId].odt.timerInterval;
        }

        public static void SetFloatSwitchTimeOffset (int switchId, uint timeOffset) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            floatSwitches [switchId].odt.timerInterval = timeOffset;
        }

        public static bool GetFloatSwitchState (int switchId) {
            if ((switchId < 0) || (switchId >= floatSwitches.Count))
                throw new ArgumentOutOfRangeException ("switchId");

            return floatSwitches [switchId].activated;
        }
    }
}