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
        private static List<FloatSwitch> floatSwitches;

        private static int highSwitchAlarmIndex;
        private static int lowSwitchAlarmIndex;
        private static int switchAnalogMismatchAlarmIndex;

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

        public static IndividualControl analogSensorChannel {
            get {
                return analogSensor.sensorChannel;
            }
        }

        public static bool analogSensorEnabled {
            get {
                return analogSensor.enable;
            }
        }

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
                text = Convert.ToString (jo ["analogSensorChannel"] ["Group"]);
                if (string.IsNullOrWhiteSpace (text)) {
                    ic = IndividualControl.Empty;
                    enable = false;
                } else
                    ic.Group = AnalogInput.GetCardIndex (text);

                text = (string)jo ["analogSensorChannel"] ["Individual"];
                if (string.IsNullOrWhiteSpace (text)) {
                    ic = IndividualControl.Empty;
                    enable = false;
                } else
                    ic.Individual = Convert.ToInt32 (jo ["analogSensorChannel"] ["Individual"]);

                analogSensor = new AnalogSensor (enable, highAlarmSetpoint, lowAlarmSetpoint, ic);

                floatSwitches = new List<FloatSwitch> ();
                JArray ja = (JArray)jo ["floatSwitches"];
                foreach (var jt in ja) {
                    JObject obj = jt as JObject;

                    string name = (string)obj ["name"];
                    ic.Group = DigitalInput.GetCardIndex ((string)obj ["inputCard"]);
                    ic.Individual = Convert.ToInt32 (obj ["channel"]);
                    float physicalLevel = Convert.ToSingle (obj ["physicalLevel"]);
                    SwitchType type = (SwitchType)Enum.Parse (typeof (SwitchType), (string)obj ["switchType"]);
                    SwitchFunction function = (SwitchFunction)Enum.Parse (typeof (SwitchFunction), (string)obj ["switchFuntion"]);

                    AddFloatSwitch (name, ic, physicalLevel, type, function);
                }
            }

            lowSwitchAlarmIndex = Alarm.Subscribe ("Low Water Level, Float Switch");
            highSwitchAlarmIndex = Alarm.Subscribe ("High Water Level, Float Switch");
            switchAnalogMismatchAlarmIndex = Alarm.Subscribe ("Float switch and analog water sensor mismatch");

            TaskManager.AddCyclicInterrupt ("Water Level", 1000, Run);
        }

        public static void Init () {
            Logger.Add ("Initializing Water Level");
        }

        public static void Run () {
            analogSensor.Run ();

            bool mismatch = false;
            foreach (var s in floatSwitches) {
                bool state = DigitalInput.GetState (s.channel);
                bool activated;

                if (s.type == SwitchType.NormallyClosed)
                    activated = !state;
                else
                    activated = state;

                if ((activated) && (analogSensor.enable)) {
                    if (analogSensor.waterLevel > (s.physicalLevel + 1))
                        mismatch = true;
                    if (analogSensor.waterLevel < (s.physicalLevel - 1))
                        mismatch = true;
                }

                if (s.function == SwitchFunction.HighLevel) {
                    if (activated)
                        Alarm.Post (highSwitchAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (highSwitchAlarmIndex))
                            Alarm.Clear (highSwitchAlarmIndex);
                    }
                } else if (s.function == SwitchFunction.LowLevel) {
                    if (activated)
                        Alarm.Post (lowSwitchAlarmIndex);
                    else {
                        if (Alarm.CheckAlarming (lowSwitchAlarmIndex))
                            Alarm.Clear (lowSwitchAlarmIndex);
                    }
                }
            }

            if (mismatch)
                Alarm.Post (switchAnalogMismatchAlarmIndex);
            else {
                if (Alarm.CheckAlarming (switchAnalogMismatchAlarmIndex))
                    Alarm.Clear (switchAnalogMismatchAlarmIndex);
            }
        }

        /**************************************************************************************************************/
        /* Float Switch                                                                                               */
        /**************************************************************************************************************/
        public static void AddFloatSwitch (
            string name, 
            IndividualControl ic, 
            float physicalLevel, 
            SwitchType type,
            SwitchFunction function)
        {
            if (FloatSwitchNameOk (name)) {
                if (FloatSwitchFunctionOk (function)) {
                    FloatSwitch fs = new FloatSwitch ();
                    fs.name = name;
                    fs.channel = ic;
                    fs.physicalLevel = physicalLevel;
                    fs.type = type;
                    fs.function = function;

                    DigitalInput.AddInput (fs.channel, name);

                    floatSwitches.Add (fs);
                } else
                    throw new Exception (string.Format ("Float Switch: {0} function already exists", function));
            } else
                throw new Exception (string.Format ("Float Switch: {0} already exists", name));
        }

        public static void RemoveFloatSwitch (int floatSwitchId) {
            if ((floatSwitchId >= 0) && (floatSwitchId < floatSwitches.Count)) {
                FloatSwitch fs = floatSwitches [floatSwitchId];
                DigitalInput.RemoveInput (fs.channel);
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
            } catch {
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

            DigitalInput.RemoveInput (ic);
            floatSwitches [switchId].channel = ic;
            DigitalInput.AddInput (floatSwitches [switchId].channel, floatSwitches [switchId].name);
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

        public static int GetFloatSwitchCount () {
            return floatSwitches.Count;
        }

        /**************************************************************************************************************/
        /* Analog Sensor                                                                                              */
        /**************************************************************************************************************/
        public static void SetAnalogSensorIndividualControl (IndividualControl ic) {
            if (analogSensor.sensorChannel.IsNotEmpty ())
                AnalogInput.RemoveChannel (analogSensor.sensorChannel);
            analogSensor.sensorChannel = ic;
            AnalogInput.AddChannel (analogSensor.sensorChannel, "Water Level");
        }

        public static void SetAnalogSensorEnable (bool enable) {
            analogSensor.enable = enable;
            if (enable)
                analogSensor.SubscribeToAlarms ();
            else {
                if (analogSensor.sensorChannel.IsNotEmpty ())
                    AnalogInput.RemoveChannel (analogSensor.sensorChannel);
                analogSensor.sensorChannel = IndividualControl.Empty;

                analogSensor.lowAlarmStpnt = 0.0f;
                analogSensor.highAlarmStpnt = 0.0f;

                Alarm.Clear (analogSensor.highAnalogAlarmIndex);
                Alarm.Clear (analogSensor.lowAnalogAlarmIndex);
                Alarm.Clear (analogSensor.sensorAlarmIndex);
            }
        }
    }
}