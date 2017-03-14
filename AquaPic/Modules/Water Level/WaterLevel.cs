using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Utilites;
using AquaPic.Sensors;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        private static WaterLevelSensor analogSensor;
        private static AutoTopOff ato;
        private static Dictionary<string,FloatSwitch> floatSwitches;

        private static int highSwitchAlarmIndex;
        private static int lowSwitchAlarmIndex;

        /**************************************************************************************************************/
        /* Analog water sensor                                                                                        */
        /**************************************************************************************************************/
        public static float highAnalogLevelAlarmSetpoint {
            get {
                return analogSensor.highAlarmSetpoint;
            }
            set {
                analogSensor.highAlarmSetpoint = value;
            }
        }

        public static float lowAnalogLevelAlarmSetpoint {
            get {
                return analogSensor.lowAlarmSetpoint;
            }
            set {
                analogSensor.lowAlarmSetpoint = value;
            }
        }

        public static float analogWaterLevel {
            get {
                return analogSensor.level;
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
                return analogSensor.channel;
            }
            set {
                if (analogSensor.channel.IsNotEmpty ()) {
                    analogSensor.Remove ();
                }

                if (value.IsNotEmpty ()) {
                    analogSensor.Add (value);
                }
            }
        }

        public static bool analogSensorEnabled {
            get {
                return analogSensor.enable;
            }
            set {
                analogSensor.enable = value;
                if (analogSensor.enable) {
                    analogSensor.enableHighAlarm = true;
                    analogSensor.enableLowAlarm = true;
                
                    try {
                        AquaPicDrivers.AnalogInput.AddChannel (analogSensor.channel, "Water Level");
                    } catch (Exception) {
                        ; //channel already added
                    }
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

        public static bool atoReservoirLevelEnabled {
            get {
                return ato.reservoirLevel.channel.IsNotEmpty ();
            }
        }

        public static float atoReservoirLevel {
            get {
                return ato.reservoirLevel.level;
            }
        }

        public static IndividualControl atoReservoirLevelChannel {
            get {
                return ato.reservoirLevel.channel;
            }
            set {
                if (ato.reservoirLevel.channel.IsNotEmpty ()) {
                    AquaPicDrivers.AnalogInput.RemoveChannel (ato.reservoirLevel.channel);
                }
                
                if (value.IsNotEmpty ()) {
                    ato.reservoirLevel.Add (value);
                }
            }
        }

        public static bool atoReservoirDisableOnLowLevel {
            get {
                return ato.disableOnLowResevoirLevel;
            }
            set {
                ato.disableOnLowResevoirLevel = value;
            }
        }

        public static float atoReservoirLowLevelSetpoint {
            get {
                return ato.reservoirLowLevelAlarmSetpoint;
            }
            set {
                ato.reservoirLowLevelAlarmSetpoint = value;
            }
        }

        public static float atoReservoirLevelSensorZeroCalibrationValue {
            get {
                return ato.reservoirLevel.zeroValue;
            }
        }

        public static float atoReservoirLevelSensorFullScaleCalibrationActual {
            get {
                return ato.reservoirLevel.fullScaleActual;
            }
        }

        public static float atoReservoirLevelSensorFullScaleCalibrationValue {
            get {
                return ato.reservoirLevel.fullScaleValue;
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

        public static string defaultFloatSwitch {
            get {
                if (floatSwitches.Count > 0) {
                    var first = floatSwitches.First ();
                    return first.Key;
                } else {
                    return string.Empty;
                }
            }
        }

        /**************************************************************************************************************/
        /* Water Level                                                                                                */
        /**************************************************************************************************************/
        static WaterLevel () {
            floatSwitches = new Dictionary<string, FloatSwitch> ();

            string path = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "waterLevelProperties.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));

                    /******************************************************************************************************/
                    /* Analog Sensor                                                                                      */
                    /******************************************************************************************************/
                    bool enable = Convert.ToBoolean (jo["enableAnalogSensor"]);

                    float highAlarmSetpoint;
                    string text = (string)jo["highAnalogLevelAlarmSetpoint"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        highAlarmSetpoint = 0.0f;
                        enable = false;
                    } else
                        highAlarmSetpoint = Convert.ToSingle (text);


                    float lowAlarmSetpoint;
                    text = (string)jo["lowAnalogLevelAlarmSetpoint"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        lowAlarmSetpoint = 0.0f;
                        enable = false;
                    } else
                        lowAlarmSetpoint = Convert.ToSingle (text);

                    IndividualControl ic;
                    text = Convert.ToString (jo["inputCard"]);
                    if (string.IsNullOrWhiteSpace (text)) {
                        ic = IndividualControl.Empty;
                        enable = false;
                    } else
                        ic.Group = AquaPicDrivers.AnalogInput.GetCardIndex (text);

                    text = (string)jo["channel"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        ic = IndividualControl.Empty;
                        enable = false;
                    } else
                        ic.Individual = Convert.ToInt32 (text);

                    analogSensor = new WaterLevelSensor ("Water Level", ic, highAlarmSetpoint, lowAlarmSetpoint, enable);

                    text = (string)jo["zeroCalibrationValue"];
                    if (string.IsNullOrWhiteSpace (text))
                        analogSensor.zeroValue = 819.2f;
                    else {
                        try {
                            analogSensor.zeroValue = Convert.ToSingle (text);
                        } catch {
                            analogSensor.zeroValue = 819.2f;
                        }
                    }

                    text = (string)jo["fullScaleCalibrationActual"];
                    if (string.IsNullOrWhiteSpace (text))
                        analogSensor.fullScaleActual = 15.0f;
                    else {
                        try {
                            analogSensor.fullScaleActual = Convert.ToSingle (text);
                        } catch {
                            analogSensor.fullScaleActual = 15.0f;
                        }
                    }

                    text = (string)jo["fullScaleCalibrationValue"];
                    if (string.IsNullOrWhiteSpace (text))
                        analogSensor.fullScaleValue = 4096.0f;
                    else {
                        try {
                            analogSensor.fullScaleValue = Convert.ToSingle (text);
                        } catch {
                            analogSensor.fullScaleValue = 4096.0f;
                        }
                    }

                    /******************************************************************************************************/
                    /* Float Switches                                                                                     */
                    /******************************************************************************************************/
                    JArray ja = (JArray)jo["floatSwitches"];
                    foreach (var jt in ja) {
                        JObject obj = jt as JObject;

                        string name = (string)obj["name"];
                        ic.Group = AquaPicDrivers.DigitalInput.GetCardIndex ((string)obj["inputCard"]);
                        ic.Individual = Convert.ToInt32 (obj["channel"]);
                        float physicalLevel = Convert.ToSingle (obj["physicalLevel"]);
                        SwitchType type = (SwitchType)Enum.Parse (typeof (SwitchType), (string)obj["switchType"]);
                        SwitchFunction function = (SwitchFunction)Enum.Parse (typeof (SwitchFunction), (string)obj["switchFuntion"]);
                        string tString = (string)obj["timeOffset"];
                        uint timeOffset = Timer.ParseTime (tString);

                        if ((function == SwitchFunction.HighLevel) && (type != SwitchType.NormallyClosed)) {
                            Logger.AddWarning ("High level switch should be normally closed");
                        } else if ((function == SwitchFunction.LowLevel) && (type != SwitchType.NormallyClosed)) {
                            Logger.AddWarning ("Low level switch should be normally closed");
                        } else if ((function == SwitchFunction.ATO) && (type != SwitchType.NormallyOpened)) {
                            Logger.AddWarning ("ATO switch should be normally opened");
                        }

                        AddFloatSwitch (name, ic, physicalLevel, type, function, timeOffset);
                    }

                    /******************************************************************************************************/
                    /* Auto Top Off                                                                                       */
                    /******************************************************************************************************/
                    JObject joAto = (JObject)jo["AutoTopOff"];

                    try {
                        enable = Convert.ToBoolean (joAto["enableAto"]);
                    } catch {
                        enable = false;
                    }

                    bool useAnalogSensor;
                    try {
                        useAnalogSensor = Convert.ToBoolean (joAto["useAnalogSensor"]);
                    } catch {
                        useAnalogSensor = false;
                    }

                    float analogOnSetpoint;
                    text = (string)joAto["analogOnSetpoint"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        analogOnSetpoint = 0.0f;
                        useAnalogSensor = false;
                    } else
                        analogOnSetpoint = Convert.ToSingle (text);

                    float analogOffSetpoint;
                    text = (string)joAto["analogOffSetpoint"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        analogOffSetpoint = 0.0f;
                        useAnalogSensor = false;
                    } else
                        analogOffSetpoint = Convert.ToSingle (text);

                    bool useFloatSwitch = Convert.ToBoolean (joAto["useFloatSwitch"]);

                    if (!useFloatSwitch && !useAnalogSensor)
                        enable = false;

                    text = (string)joAto["powerStrip"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        ic = IndividualControl.Empty;
                        enable = false;
                    } else {
                        ic.Group = Power.GetPowerStripIndex (text);
                    }

                    text = (string)joAto["outlet"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        ic = IndividualControl.Empty;
                        enable = false;
                    } else {
                        ic.Individual = Convert.ToInt32 (text);
                    }

                    uint maxPumpOnTime;
                    text = (string)joAto["maxPumpOnTime"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        maxPumpOnTime = 0U;
                        enable = false;
                    } else
                        maxPumpOnTime = Timer.ParseTime (text) / 1000;

                    uint minPumpOffTime;
                    text = (string)joAto["minPumpOffTime"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        minPumpOffTime = uint.MaxValue;
                        enable = false;
                    } else
                        minPumpOffTime = Timer.ParseTime (text) / 1000;

                    ato = new AutoTopOff (
                        enable,
                        useAnalogSensor,
                        analogOnSetpoint,
                        analogOffSetpoint,
                        useFloatSwitch,
                        ic,
                        maxPumpOnTime,
                        minPumpOffTime);

                    text = (string)joAto["reservoirInputCard"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        ic = IndividualControl.Empty;
                    } else {
                        ic.Group = AquaPicDrivers.AnalogInput.GetCardIndex (text);
                    }

                    text = (string)joAto["reservoirChannel"];
                    if (string.IsNullOrWhiteSpace (text)) {
                        ic = IndividualControl.Empty;
                    } else {
                        ic.Individual = Convert.ToInt32 (text);
                    }

                    if (ic.IsNotEmpty ()) {
                        ato.reservoirLevel.Add (ic);
                    }

                    text = (string)joAto["reservoirZeroCalibrationValue"];
                    if (string.IsNullOrWhiteSpace (text))
                        ato.reservoirLevel.zeroValue = 819.2f;
                    else {
                        try {
                            ato.reservoirLevel.zeroValue = Convert.ToSingle (text);
                        } catch {
                            ato.reservoirLevel.zeroValue = 819.2f;
                        }
                    }

                    text = (string)joAto["reservoirFullScaleCalibrationActual"];
                    if (string.IsNullOrWhiteSpace (text))
                        ato.reservoirLevel.fullScaleActual = 15.0f;
                    else {
                        try {
                            ato.reservoirLevel.fullScaleActual = Convert.ToSingle (text);
                        } catch {
                            ato.reservoirLevel.fullScaleActual = 15.0f;
                        }
                    }

                    text = (string)joAto["reservoirFullScaleCalibrationValue"];
                    if (string.IsNullOrWhiteSpace (text))
                        ato.reservoirLevel.fullScaleValue = 4096.0f;
                    else {
                        try {
                            ato.reservoirLevel.fullScaleValue = Convert.ToSingle (text);
                        } catch {
                            ato.reservoirLevel.fullScaleValue = 4096.0f;
                        }
                    }

                    text = (string)joAto["reservoirLowLevelSetpoint"];
                    if (string.IsNullOrWhiteSpace (text))
                        ato.reservoirLowLevelAlarmSetpoint = 0.0f;
                    else {
                        try {
                            ato.reservoirLowLevelAlarmSetpoint = Convert.ToSingle (text);
                        } catch {
                            ato.reservoirLowLevelAlarmSetpoint = 0.0f;
                        }
                    }

                    try {
                        ato.disableOnLowResevoirLevel = Convert.ToBoolean (joAto["disableOnLowResevoirLevel"]);
                    } catch {
                        ato.disableOnLowResevoirLevel = false;
                    }
                }
            } else {
                analogSensor = new WaterLevelSensor ("Water Level", IndividualControl.Empty, 0.0f, 0.0f, false);

                ato = new AutoTopOff (
                        false,
                        false,
                        0.0f,
                        0.0f,
                        false,
                        IndividualControl.Empty,
                        0,
                        uint.MaxValue);

                Logger.Add ("Water level settings file did not exist, created new water level settings");
                var file = File.Create (path);
                file.Close ();

                var jo = new JObject ();
                jo.Add (new JProperty ("enableAnalogSensor", "false"));
                jo.Add (new JProperty ("highAnalogLevelAlarmSetpoint", "0.0"));
                jo.Add (new JProperty ("lowAnalogLevelAlarmSetpoint", "0.0"));
                jo.Add (new JProperty ("zeroCalibrationValue", "819.2"));
                jo.Add (new JProperty ("fullScaleCalibrationActual", "15.0"));
                jo.Add (new JProperty ("fullScaleCalibrationValue", "4096.0"));
                jo.Add (new JProperty ("inputCard", string.Empty));
                jo.Add (new JProperty ("channel", string.Empty));

                jo.Add (new JProperty ("floatSwitches", new JArray ()));

                var joato = new JObject ();
                joato.Add (new JProperty ("enableAto", "false"));
                joato.Add (new JProperty ("useAnalogSensor", "false"));
                joato.Add (new JProperty ("analogOnSetpoint", "0.0"));
                joato.Add (new JProperty ("analogOffSetpoint", "0.0"));
                joato.Add (new JProperty ("useFloatSwitch", "false"));
                joato.Add (new JProperty ("powerStrip", string.Empty));
                joato.Add (new JProperty ("outlet", string.Empty));
                joato.Add (new JProperty ("maxPumpOnTime", "0"));
                joato.Add (new JProperty ("minPumpOffTime", uint.MaxValue.ToString ()));
                joato.Add (new JProperty ("reservoirInputCard", string.Empty));
                joato.Add (new JProperty ("reservoirChannel", string.Empty));
                joato.Add (new JProperty ("reservoirZeroCalibrationValue", "819.2"));
                joato.Add (new JProperty ("reservoirFullScaleCalibrationActual", "15.0"));
                joato.Add (new JProperty ("reservoirFullScaleCalibrationValue", "4096.0"));
                joato.Add (new JProperty ("reservoirLowLevelSetpoint", "0.0"));
                joato.Add (new JProperty ("disableOnLowResevoirLevel", "false"));
                jo.Add (new JProperty ("AutoTopOff", joato));

                File.WriteAllText (path, jo.ToString ());
            }

            lowSwitchAlarmIndex = Alarm.Subscribe ("Low Water Level, Float Switch");
            highSwitchAlarmIndex = Alarm.Subscribe ("High Water Level, Float Switch");

            TaskManager.AddCyclicInterrupt ("Water Level", 1000, Run);
        }

        public static void Init () {
            Logger.Add ("Initializing Water Level");
        }

        public static void Run () {
            analogSensor.UpdateWaterLevel ();

            ato.useFloatSwitch = false; //set 'use float switch' to false, if no ATO float switch in found it remains false
            foreach (var s in floatSwitches.Values) {
                s.Get ();

                if ((s.activated) && (analogSensor.enable) && (analogSensor.connected)) {
                    if (s.type == SwitchType.NormallyClosed) {
                        if (analogSensor.level > (s.physicalLevel + 1.0f)) {
                            Logger.AddInfo ("Float switch {0} is reporting a mismatch with analog sensor", s.name);
                        }
                    } else {
                        if (analogSensor.level < (s.physicalLevel - 1.0f)) {
                            Logger.AddInfo ("Float switch {0} is reporting a mismatch with analog sensor", s.name);
                        }
                    }
                }

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

            string path = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = Path.Combine (path, "Settings");
            path = Path.Combine (path, "waterLevelProperties.json");

            string jstring = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (jstring);

            jo["AutoTopOff"]["reservoirZeroCalibrationValue"] = ato.reservoirLevel.zeroValue.ToString ();
            jo["AutoTopOff"]["reservoirFullScaleCalibrationActual"] = ato.reservoirLevel.fullScaleActual.ToString ();
            jo["AutoTopOff"]["reservoirFullScaleCalibrationValue"] = ato.reservoirLevel.fullScaleValue.ToString ();

            File.WriteAllText (path, jo.ToString ());
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

            string path = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
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
            IndividualControl channel, 
            float physicalLevel, 
            SwitchType type,
            SwitchFunction function,
            uint timeOffset
        ) {
            if (!FloatSwitchNameOk (name)) {
                throw new Exception (string.Format ("Float Switch: {0} already exists", name));
            }

            if (!FloatSwitchFunctionOk (function)) {
                throw new Exception (string.Format ("Float Switch: {0} function already exists", function));
            }

            floatSwitches[name] = new FloatSwitch (
                name,
                type,
                function,
                physicalLevel,
                channel,
                timeOffset);
        }

        public static void RemoveFloatSwitch (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].Remove ();
            floatSwitches.Remove (floatSwitchName);
        }

        public static void CheckFloatSwitchKey (string floatSwitchName) {
            if (!floatSwitches.ContainsKey (floatSwitchName)) {
                throw new ArgumentException ("floatSwitchName");
            }
        }

        public static bool CheckFloatSwitchKeyNoThrow (string floatSwitchName) {
            try {
                CheckFloatSwitchKey (floatSwitchName);
                return true;
            } catch {
                return false;
            }
        }

        public static bool FloatSwitchNameOk (string floatSwitchName) {
            return !CheckFloatSwitchKeyNoThrow (floatSwitchName);
        }

        public static bool FloatSwitchFunctionOk (SwitchFunction function) {
            foreach (var fs in floatSwitches.Values) {
                if (function == fs.function)
                    return false;
            }

            return true;
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllFloatSwitches () {
            List<string> names = new List<string> ();
            foreach (var floatSwitch in floatSwitches.Values) {
                names.Add (floatSwitch.name);
            }
            return names.ToArray ();
        }

        public static bool GetFloatSwitchState (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].activated;
        }

        public static SwitchType GetFloatSwitchType (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].type;
        }

        public static SwitchFunction GetFloatSwitchFunction (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].function;
        }

        public static float GetFloatSwitchPhysicalLevel (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].physicalLevel;
        }

        public static uint GetFloatSwitchTimeOffset (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].onDelayTimer.timerInterval;
        }

        public static IndividualControl GetFloatSwitchIndividualControl (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].channel;
        }

        /***Setters****************************************************************************************************/
        /***Name***/
        public static void SetFloatSwitchName (string oldSwitchName, string newSwitchName) {
            CheckFloatSwitchKey (oldSwitchName);
            if (!FloatSwitchNameOk (newSwitchName)) {
                throw new Exception (string.Format ("Float Switch: {0} already exists", newSwitchName));
            }

            var floatSwitch = floatSwitches[oldSwitchName];
            
            floatSwitch.ChangeName (newSwitchName);

            floatSwitches.Remove (oldSwitchName);
            floatSwitches[newSwitchName] = floatSwitch;
        }

        public static void SetFloatSwitchIndividualControl (string floatSwitchName, IndividualControl ic) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].Remove ();
            floatSwitches[floatSwitchName].Add (ic);
        }

        public static void SetFloatSwitchPhysicalLevel (string floatSwitchName, float physicalLevel) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].physicalLevel = physicalLevel;
        }

        public static void SetFloatSwitchType (string floatSwitchName, SwitchType type) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].type = type;
        }

        public static void SetFloatSwitchFunction (string floatSwitchName, SwitchFunction function) {
            CheckFloatSwitchKey (floatSwitchName);
            if (FloatSwitchFunctionOk (function)) {
                throw new Exception (string.Format ("Float Switch: {0} function already exists", function));
            }

            floatSwitches[floatSwitchName].function = function;
        }

        public static void SetFloatSwitchTimeOffset (string floatSwitchName, uint timeOffset) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].onDelayTimer.timerInterval = timeOffset;
        }
    }
}