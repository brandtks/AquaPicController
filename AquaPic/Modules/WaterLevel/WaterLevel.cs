#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Globals;
using AquaPic.Sensors;
using AquaPic.DataLogging;

namespace AquaPic.Modules
{
    public partial class WaterLevel
    {
        private static Dictionary<string, WaterLevelGroup> waterLevelGroups;
        private static Dictionary<string, WaterLevelSensor> analogLevelSensors;
        private static Dictionary<string, FloatSwitch> floatSwitches;

        /**************************************************************************************************************/
        /* Analog Water Sensors                                                                                       */
        /**************************************************************************************************************/
        public static int analogLevelSensorCount {
            get {
                return analogLevelSensors.Count;
            }
        }

        public static string firstAnalogLevelSensor {
            get {
                if (analogLevelSensors.Count > 0) {
                    var first = analogLevelSensors.First ();
                    return first.Key;
                }

                return string.Empty;
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

        public static string firstFloatSwitch {
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
        /* Default water level properties                                                                             */
        /**************************************************************************************************************/
        public static int waterLevelGroupCount {
            get {
                return waterLevelGroups.Count;
            }
        }

        public static string firstWaterLevelGroup {
            get {
                if (waterLevelGroups.Count > 0) {
                    var first = waterLevelGroups.First ();
                    return first.Key;
                } else {
                    return string.Empty;
                }
            }
        }

        /**************************************************************************************************************/
        /* Water Level                                                                                                */
        /**************************************************************************************************************/
        static WaterLevel () { }

        public static void Init () {
            Logger.Add ("Initializing Water Level");

            waterLevelGroups = new Dictionary<string, WaterLevelGroup> ();
            analogLevelSensors = new Dictionary<string, WaterLevelSensor> ();
            floatSwitches = new Dictionary<string, FloatSwitch> ();

            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, "waterLevelProperties.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    JObject jo = (JObject)JToken.ReadFrom (new JsonTextReader (reader));

                    /**************************************************************************************************/
                    /* Analog Sensors                                                                                 */
                    /**************************************************************************************************/
                    var ja = (JArray)jo["analogSensors"];
                    foreach (var jt in ja) {
                        JObject obj = jt as JObject;

                        var name = (string)obj["name"];
                        var waterLevelGroupName = (string)obj["waterLevelGroupName"];

                        var ic = IndividualControl.Empty;
                        var text = (string)obj["inputCard"];
                        if (text.IsNotEmpty ()) {
                            try {
                                ic.Group = text;
                            } catch {
                                //
                            }
                        }

						if (ic.Group.IsNotEmpty ()) {
                            text = (string)obj["channel"];
                            if (text.IsEmpty ()) {
                                ic = IndividualControl.Empty;
                            } else {
                                try {
                                    ic.Individual = Convert.ToInt32 (text);
                                } catch {
                                    ic = IndividualControl.Empty;
                                }
                            }
                        }

                        var zeroScaleCalibrationValue = 819.2f;
                        text = (string)obj["zeroScaleCalibrationValue"];
                        if (text.IsNotEmpty ()) {
                            try {
                                zeroScaleCalibrationValue = Convert.ToSingle (text);
                            } catch {
                                //
                            }
                        }

                        var fullScaleCalibrationActual = 10.0f;
                        text = (string)obj["fullScaleCalibrationActual"];
                        if (text.IsNotEmpty ()) {
                            try {
                                fullScaleCalibrationActual = Convert.ToSingle (text);
                            } catch {
                                //
                            }
                        }

                        var fullScaleCalibrationValue = 3003.73f;
                        text = (string)obj["fullScaleCalibrationValue"];
                        if (text.IsNotEmpty ()) {
                            try {
                                fullScaleCalibrationValue = Convert.ToSingle (text);
                            } catch {
                                //
                            }
                        }

                        AddAnalogLevelSensor (
                            name,
                            waterLevelGroupName,
                            ic,
                            zeroScaleCalibrationValue,
                            fullScaleCalibrationActual,
                            fullScaleCalibrationValue
                        );
                    }

                    /**************************************************************************************************/
                    /* Water Level Groups                                                                             */
                    /**************************************************************************************************/
                    ja = (JArray)jo["waterLevelGroups"];
                    foreach (var jt in ja) {
                        JObject obj = jt as JObject;

                        var name = (string)obj["name"];

                        var highAnalogAlarmSetpoint = 0f;
                        var text = (string)obj["highAnalogAlarmSetpoint"];
                        if (text.IsNotEmpty ()) {
                            try {
                                highAnalogAlarmSetpoint = Convert.ToSingle (text);
                            } catch {
                                //
                            }
                        }

                        var enableHighAnalogAlarm = true;
                        text = (string)obj["enableHighAnalogAlarm"];
                        if (text.IsNotEmpty ()) {
                            try {
                                enableHighAnalogAlarm = Convert.ToBoolean (text);
                            } catch {
                                //
                            }
                        }

                        var lowAnalogAlarmSetpoint = 0f;
                        text = (string)obj["lowAnalogAlarmSetpoint"];
                        if (text.IsNotEmpty ()) {
                            try {
                                lowAnalogAlarmSetpoint = Convert.ToSingle (text);
                            } catch {
                                //
                            }
                        }

                        var enableLowAnalogAlarm = true;
                        text = (string)obj["enableLowAnalogAlarm"];
                        if (text.IsNotEmpty ()) {
                            try {
                                enableLowAnalogAlarm = Convert.ToBoolean (text);
                            } catch {
                                //
                            }
                        }

                        AddWaterLevelGroup (
                            name, 
                            highAnalogAlarmSetpoint,
                            enableHighAnalogAlarm,
                            lowAnalogAlarmSetpoint,
                            enableLowAnalogAlarm);
                    }

                    /**************************************************************************************************/
                    /* Float Switches                                                                                 */
                    /**************************************************************************************************/
                    ja = (JArray)jo["floatSwitches"];
                    foreach (var jt in ja) {
                        JObject obj = jt as JObject;

                        var name = (string)obj["name"];

                        var ic = IndividualControl.Empty;
                        var text = (string)obj["inputCard"];
                        if (text.IsNotEmpty ()) {
                            try {
                                ic.Group = text;
                            } catch {
                                //
                            }
                        }

						if (ic.Group.IsNotEmpty ()) {
                            text = (string)obj["channel"];
                            if (text.IsEmpty ()) {
                                ic = IndividualControl.Empty;
                            } else {
                                try {
                                    ic.Individual = Convert.ToInt32 (text);
                                } catch {
                                    ic = IndividualControl.Empty;
                                }
                            }
                        }

                        var physicalLevel = 0f;
                        text = (string)obj["physicalLevel"];
                        if (text.IsNotEmpty ()) {
                            try {
                                physicalLevel = Convert.ToSingle (text);
                            } catch {
                                //
                            }
                        }

                        var type = SwitchType.NormallyOpened;
                        text = (string)obj["switchType"];
                        if (text.IsNotEmpty ()) {
                            try {
                                type = (SwitchType)Enum.Parse (typeof (SwitchType), text);
                            } catch {
                                //
                            }
                        }

                        var function = SwitchFunction.Other;
                        text = (string)obj["switchFuntion"];
                        if (text.IsNotEmpty ()) {
                            try {
                                function = (SwitchFunction)Enum.Parse (typeof (SwitchFunction), text);
                            } catch {
                                //
                            }
                        }

                        var timeOffset = 0u;
                        text = (string)obj["timeOffset"];
                        if (text.IsNotEmpty ()) {
                            try {
                                timeOffset = Timer.ParseTime (text);
                            } catch {
                                //
                            }
                        }

                        var waterLevelGroupName = (string)obj["waterLevelGroupName"];

                        if ((function == SwitchFunction.HighLevel) && (type != SwitchType.NormallyClosed)) {
                            Logger.AddWarning ("High level switch should be normally closed");
                        } else if ((function == SwitchFunction.LowLevel) && (type != SwitchType.NormallyClosed)) {
                            Logger.AddWarning ("Low level switch should be normally closed");
                        } else if ((function == SwitchFunction.ATO) && (type != SwitchType.NormallyOpened)) {
                            Logger.AddWarning ("ATO switch should be normally opened");
                        }

                        AddFloatSwitch (name, ic, physicalLevel, type, function, timeOffset, waterLevelGroupName);
                    }
                }
            } else {
                Logger.Add ("Water level settings file did not exist, created new water level settings");
                var file = File.Create (path);
                file.Close ();

                var jo = new JObject ();
                jo.Add (new JProperty ("waterLevelGroups", new JArray ()));
                jo.Add (new JProperty ("defaultWaterLevelGroup", string.Empty));
                jo.Add (new JProperty ("analogSensors", new JArray ()));
                jo.Add (new JProperty ("floatSwitches", new JArray ()));

                File.WriteAllText (path, jo.ToString ());
            }

            TaskManager.AddCyclicInterrupt ("Water Level", 1000, Run);
        }

        public static void Run () {
            foreach (var waterLevelGroup in waterLevelGroups.Values) {
                waterLevelGroup.GroupRun ();
            }

            // Get the value of the all the analog sensors no assigned to a group
            foreach (var analogSensor in analogLevelSensors.Values) {
                if (!CheckWaterLevelGroupKeyNoThrow (analogSensor.waterLevelGroupName)) {
                    analogSensor.Get ();
                }
            }

            // Get the value of the all the float switches no assigned to a group
            foreach (var floatSwitch in floatSwitches.Values) {
                if (!CheckWaterLevelGroupKeyNoThrow (floatSwitch.waterLevelGroupName)) {
                    floatSwitch.Get ();
                }
            }
        }

        /**************************************************************************************************************/
        /* Water Level Groups                                                                                         */
        /**************************************************************************************************************/
        public static void AddWaterLevelGroup (
            string name, 
            float highAnalogAlarmSetpoint,
            bool enableHighAnalogAlarm,
            float lowAnalogAlarmSetpoint,
            bool enableLowAnalogAlarm
        ) {
            if (!WaterLevelGroupNameOk (name)) {
                throw new Exception (string.Format ("Water Level Group: {0} already exists", name));
            }

            waterLevelGroups.Add (
                name, 
                new WaterLevelGroup (
                    name, 
                    highAnalogAlarmSetpoint,
                    enableHighAnalogAlarm,
                    lowAnalogAlarmSetpoint,
                    enableLowAnalogAlarm));
        }

        public static void RemoveWaterLevelGroup (string name) {
            CheckWaterLevelGroupKey (name);
            waterLevelGroups.Remove (name);
        }

        public static void CheckWaterLevelGroupKey (string name) {
            if (!waterLevelGroups.ContainsKey (name)) {
                throw new ArgumentException ("name");
            }
        }

        public static bool CheckWaterLevelGroupKeyNoThrow (string name) {
            try {
                CheckWaterLevelGroupKey (name);
                return true;
            } catch {
                return false;
            }
        }

        public static bool WaterLevelGroupNameOk (string name) {
            return !CheckWaterLevelGroupKeyNoThrow (name);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllWaterLevelGroupNames () {
            List<string> names = new List<string> ();
            foreach (var waterLevelGroup in waterLevelGroups.Values) {
                names.Add (waterLevelGroup.name);
            }
            return names.ToArray ();
        }

        /***Level***/
        public static float GetWaterLevelGroupLevel (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].level;
        }

		public static bool GetWaterLevelGroupSwitchesActivated (string name, SwitchFunction function = SwitchFunction.ATO) {
            CheckWaterLevelGroupKey (name);
            bool activated = true;
            foreach (var s in floatSwitches.Values) {
				if ((s.waterLevelGroupName == name) && (s.function == function)){
                    // Using AND because we want all the switches to be activated
                    activated &= s.activated;
                }
            }
            return activated;
        }

        /***Analog sensors connected***/
        public static bool GetWaterLevelGroupAnalogSensorConnected (string name) {
            CheckWaterLevelGroupKey (name);
            bool connected = false;
            foreach (var sensor in analogLevelSensors.Values) {
                if (sensor.waterLevelGroupName == name) {
                    // Using OR because we really only care that at least one sensor is connected
                    connected |= sensor.connected;
                }
            }
            return connected;
        }

        /***High analog alarm setpoint**/
        public static float GetWaterLevelGroupHighAnalogAlarmSetpoint (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].highAnalogAlarmSetpoint;
        }

        /***High analog alarm enable**/
        public static bool GetWaterLevelGroupHighAnalogAlarmEnable (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].enableHighAnalogAlarm;
        }

        /***High analog alarm index**/
        public static int GetWaterLevelGroupHighAnalogAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].highAnalogAlarmIndex;
        }      

        /***High switch alarm index***/
        public static int GetWaterLevelGroupHighSwitchAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].highSwitchAlarmIndex;
        }

		/***High alarming***/
		public static bool GetWaterLevelGroupHighAlarming (string name) {
            CheckWaterLevelGroupKey (name);
            bool alarming = false;
            if (waterLevelGroups[name].enableHighAnalogAlarm) {
                alarming = Alarm.CheckAlarming (waterLevelGroups[name].highAnalogAlarmIndex);
            }
            alarming |= Alarm.CheckAlarming (waterLevelGroups[name].highSwitchAlarmIndex);
            return alarming;
        }

        /***Low analog alarm setpoint**/
        public static float GetWaterLevelGroupLowAnalogAlarmSetpoint (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].lowAnalogAlarmSetpoint;
        }

        /***Low analog alarm enable**/
        public static bool GetWaterLevelGroupLowAnalogAlarmEnable (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].enableLowAnalogAlarm;
        }

        /***Low analog alarm index**/
        public static int GetWaterLevelGroupLowAnalogAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].lowAnalogAlarmIndex;
        }

        /***Low switch alarm index***/
        public static int GetWaterLevelGroupLowSwitchAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].lowSwitchAlarmIndex;
        }

		/***Low alarming***/
        public static bool GetWaterLevelGroupLowAlarming (string name) {
            CheckWaterLevelGroupKey (name);
            bool alarming = false;
			if (waterLevelGroups[name].enableLowAnalogAlarm) {
				alarming = Alarm.CheckAlarming (waterLevelGroups[name].lowAnalogAlarmIndex);
            }
            alarming |= Alarm.CheckAlarming (waterLevelGroups[name].lowSwitchAlarmIndex);
            return alarming;
        }

        /***Data logger***/
        public static IDataLogger GetWaterLevelGroupDataLogger (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].dataLogger;
        }

        /***Setters****************************************************************************************************/
        /***High analog alarm setpoint**/
        public static void SetWaterLevelGroupHighAnalogAlarmSetpoint (string name, float highAnalogAlarmSetpoint) {
            CheckWaterLevelGroupKey (name);
            waterLevelGroups[name].highAnalogAlarmSetpoint = highAnalogAlarmSetpoint;
        }

        /***High analog alarm enable**/
        public static void SetWaterLevelGroupHighAnalogAlarmEnable (string name, bool enableHighAnalogAlarm) {
            CheckWaterLevelGroupKey (name);
            waterLevelGroups[name].enableHighAnalogAlarm = enableHighAnalogAlarm;
        }

        /***Low analog alarm setpoint**/
        public static void SetWaterLevelGroupLowAnalogAlarmSetpoint (string name, float lowAnalogAlarmSetpoint) {
            CheckWaterLevelGroupKey (name);
            waterLevelGroups[name].lowAnalogAlarmSetpoint = lowAnalogAlarmSetpoint;
        }

        /***Low analog alarm enable**/
        public static void SetWaterLevelGroupLowAnalogAlarmEnable (string name, bool enableLowAnalogAlarm) {
            CheckWaterLevelGroupKey (name);
            waterLevelGroups[name].enableLowAnalogAlarm = enableLowAnalogAlarm;
        }

        /**************************************************************************************************************/
        /* Analog Level Sensor                                                                                        */
        /**************************************************************************************************************/
        public static void AddAnalogLevelSensor (
            string name,
            string waterLevelGroupName, 
            IndividualControl ic,
            float zeroScaleCalibrationValue,
            float fullScaleCalibrationActual,
            float fullScaleCalibrationValue
        ) {
            if (!AnalogLevelSensorNameOk (name)) {
                throw new Exception (string.Format ("Water Level Group: {0} already exists", name));
            }

            analogLevelSensors.Add (name, new WaterLevelSensor (
                name,
                ic,
                waterLevelGroupName
            ));

            analogLevelSensors[name].zeroScaleValue = zeroScaleCalibrationValue;
            analogLevelSensors[name].fullScaleActual = fullScaleCalibrationActual;
            analogLevelSensors[name].fullScaleValue = fullScaleCalibrationValue;
        }

        public static void AddAnalogLevelSensor (
            string name,
            bool enable,
            string waterLevelGroupName, 
            IndividualControl ic
        ) {
            if (!AnalogLevelSensorNameOk (name)) {
                throw new Exception (string.Format ("Water Level Group: {0} already exists", name));
            }

            analogLevelSensors.Add (name, new WaterLevelSensor (
                name,
                ic,
                waterLevelGroupName
            ));
        }

        public static void RemoveAnalogLevelSensor (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            analogLevelSensors[analogLevelSensorName].Remove ();
            Alarm.Clear (analogLevelSensors[analogLevelSensorName].sensorDisconnectedAlarmIndex);
            analogLevelSensors.Remove (analogLevelSensorName);

        }

        public static void CheckAnalogLevelSensorKey (string analogLevelSensorName) {
            if (!analogLevelSensors.ContainsKey (analogLevelSensorName)) {
                throw new ArgumentException ("analogLevelSensorName");
            }
        }

        public static bool CheckAnalogLevelSensorKeyNoThrow (string analogLevelSensorName) {
            try {
                CheckAnalogLevelSensorKey (analogLevelSensorName);
                return true;
            } catch {
                return false;
            }
        }

        public static bool AnalogLevelSensorNameOk (string analogLevelSensorName) {
            return !CheckAnalogLevelSensorKeyNoThrow (analogLevelSensorName);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllAnalogLevelSensors () {
            List<string> names = new List<string> ();
            foreach (var analogLevelSensor in analogLevelSensors.Values) {
                names.Add (analogLevelSensor.name);
            }
            return names.ToArray ();
        }

        /***Individual Control***/
        public static IndividualControl GetAnalogLevelSensorIndividualControl (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].channel;
        }

        /***Level***/
        public static float GetAnalogLevelSensorLevel (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].level;
        }

        /***Water Level Group Name***/
        public static string GetAnalogLevelSensorWaterLevelGroupName (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].waterLevelGroupName;
        }

        /***Connected***/
        public static bool GetAnalogLevelSensorConnected (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].connected;
        }

        /***Disconnected alarm index***/
        public static int GetAnalogLevelSensorDisconnectedAlarmIndex (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].sensorDisconnectedAlarmIndex;
        }

        /***Zero scale value***/
        public static float GetAnalogLevelSensorZeroScaleValue (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].zeroScaleValue;
        }

        /***Full scale actual***/
        public static float GetAnalogLevelSensorFullScaleActual (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].fullScaleActual;
        }

        /***Full scale value***/
        public static float GetAnalogLevelSensorFullScaleValue (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].fullScaleValue;
        }

        /***Setters****************************************************************************************************/
        /***Name***/
        public static void SetAnalogLevelSensorName (string oldAnalogLevelSensorName, string newAnalogLevelSensorName) {
            CheckAnalogLevelSensorKey (oldAnalogLevelSensorName);
            if (!AnalogLevelSensorNameOk (newAnalogLevelSensorName)) {
                throw new Exception (string.Format ("Water Level Group: {0} already exists", newAnalogLevelSensorName));
            }

            var analogLevelSensor = analogLevelSensors[oldAnalogLevelSensorName];
            analogLevelSensor.SetName (newAnalogLevelSensorName);
            analogLevelSensors.Remove (oldAnalogLevelSensorName);
            analogLevelSensors.Add(newAnalogLevelSensorName, analogLevelSensor);
        }

        /***Water Level Group Name***/
        public static void SetAnalogLevelSensorWaterLevelGroupName (string analogLevelSensorName, string waterLevelGroupName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            analogLevelSensors[analogLevelSensorName].waterLevelGroupName = waterLevelGroupName;
        }

        /***Individual Control***/
        public static void SetAnalogLevelSensorIndividualControl (string analogLevelSensorName, IndividualControl channel) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            analogLevelSensors[analogLevelSensorName].Remove ();
            analogLevelSensors[analogLevelSensorName].Add (channel);
        }

        /***Calibration data***/
        public static bool SetCalibrationData (string name, float zeroValue, float fullScaleActual, float fullScaleValue) {
            CheckAnalogLevelSensorKey (name);

            if (fullScaleValue <= zeroValue)
                throw new ArgumentException ("Full scale value can't be less than or equal to zero value");

            if (fullScaleActual < 0.0f)
                throw new ArgumentException ("Full scale actual can't be less than zero");

            if (fullScaleActual > 15.0f)
                throw new ArgumentException ("Full scale actual can't be greater than 15");

            analogLevelSensors[name].zeroScaleValue = zeroValue;
            analogLevelSensors[name].fullScaleActual = fullScaleActual;
            analogLevelSensors[name].fullScaleValue = fullScaleValue;

            var path = Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = Path.Combine (path, "waterLevelProperties.json");

            string json = File.ReadAllText (path);
            JObject jo = (JObject)JToken.Parse (json);
            JArray ja = jo["analogSensors"] as JArray;

            int arrayIndex = -1;
            for (int i = 0; i < ja.Count; ++i) {
                string n = (string)ja[i]["name"];
                if (name == n) {
                    arrayIndex = i;
                    break;
                }
            }

            if (arrayIndex == -1) {
                return false;
            }

            ja[arrayIndex]["zeroScaleCalibrationValue"] = analogLevelSensors[name].zeroScaleValue.ToString ();
            ja[arrayIndex]["fullScaleCalibrationActual"] = analogLevelSensors[name].fullScaleActual.ToString ();
            ja[arrayIndex]["fullScaleCalibrationValue"] = analogLevelSensors[name].fullScaleValue.ToString ();

            File.WriteAllText (path, jo.ToString ());

            return true;
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
            uint timeOffset,
            string waterLevelGroupName
        ) {
            if (!FloatSwitchNameOk (name)) {
                throw new Exception (string.Format ("Float Switch: {0} already exists", name));
            }

            floatSwitches.Add (name, new FloatSwitch (
                name,
                type,
                function,
                physicalLevel,
                channel,
                timeOffset,
                waterLevelGroupName));
        }

        public static void RemoveFloatSwitch (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].Remove ();   // this removes the physical digital input
            floatSwitches.Remove (floatSwitchName);     // this removes the entry from the dictionary
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

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllFloatSwitches () {
            List<string> names = new List<string> ();
            foreach (var floatSwitch in floatSwitches.Values) {
                names.Add (floatSwitch.name);
            }
            return names.ToArray ();
        }

        /***Individual Control***/
        public static IndividualControl GetFloatSwitchIndividualControl (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].channel;
        }

        /***State***/
        public static bool GetFloatSwitchState (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].activated;
        }

        /***Water level group name***/
        public static string GetFloatSwitchWaterLevelGroupName (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].waterLevelGroupName;
        }

        /***Type***/
        public static SwitchType GetFloatSwitchType (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].type;
        }

        /***Function***/
        public static SwitchFunction GetFloatSwitchFunction (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].function;
        }

        /***Physical Level***/
        public static float GetFloatSwitchPhysicalLevel (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].physicalLevel;
        }

        /***Time Offset***/
        public static uint GetFloatSwitchTimeOffset (string floatSwitchName) {
            CheckFloatSwitchKey (floatSwitchName);
            return floatSwitches[floatSwitchName].onDelayTimer.timerInterval;
        }

        /***Setters****************************************************************************************************/
        /***Name***/
        public static void SetFloatSwitchName (string oldSwitchName, string newSwitchName) {
            CheckFloatSwitchKey (oldSwitchName);
            if (!FloatSwitchNameOk (newSwitchName)) {
                throw new Exception (string.Format ("Float Switch: {0} already exists", newSwitchName));
            }

            var floatSwitch = floatSwitches[oldSwitchName];
            
            floatSwitch.SetName (newSwitchName);

            floatSwitches.Remove (oldSwitchName);
            floatSwitches[newSwitchName] = floatSwitch;
        }

        /***Individual Control***/
        public static void SetFloatSwitchIndividualControl (string floatSwitchName, IndividualControl ic) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].Remove ();
            floatSwitches[floatSwitchName].Add (ic);
        }

        /***Water level group name***/
        public static void SetFloatSwitchWaterLevelGroupName (string floatSwitchName, string waterLevelGroupName) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].waterLevelGroupName = waterLevelGroupName;
        }

        /***Type***/
        public static void SetFloatSwitchType (string floatSwitchName, SwitchType type) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].type = type;
        }

        /***Function***/
        public static void SetFloatSwitchFunction (string floatSwitchName, SwitchFunction function) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].function = function;
        }

        /***Physical Level***/
        public static void SetFloatSwitchPhysicalLevel (string floatSwitchName, float physicalLevel) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].physicalLevel = physicalLevel;
        }

        /***Time Offset***/
        public static void SetFloatSwitchTimeOffset (string floatSwitchName, uint timeOffset) {
            CheckFloatSwitchKey (floatSwitchName);
            floatSwitches[floatSwitchName].onDelayTimer.timerInterval = timeOffset;
        }
    }
}

