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

                        bool enable = false;
                        string text = (string)obj["enable"];
                        if (text.IsNotEmpty ()) {
                            try {
                                enable = Convert.ToBoolean (text);
                            } catch {
                                //
                            }
                        }

                        bool enableHighLevelAlarm = true;
                        text = (string)obj["enableHighLevelAlarm"];
                        if (text.IsNotEmpty ()) {
                            try {
                                enableHighLevelAlarm = Convert.ToBoolean (text);
                            } catch {
                                //
                            }
                        }

                        float highLevelAlarmSetpoint = 0.0f;
                        text = (string)obj["highLevelAlarmSetpoint"];
                        if (text.IsNotEmpty ()) {
                            try {
                                highLevelAlarmSetpoint = Convert.ToSingle (text);
                            } catch {
                                //
                            }
                        }

                        bool enableLowLevelAlarm = true;
                        text = (string)obj["enableLowLevelAlarm"];
                        if (text.IsNotEmpty ()) {
                            try {
                                enableLowLevelAlarm = Convert.ToBoolean (text);
                            } catch {
                                //
                            }
                        }

                        float lowLevelAlarmSetpoint = 0.0f;
                        text = (string)obj["lowLevelAlarmSetpoint"];
                        if (text.IsNotEmpty ()) {
                            try {
                                lowLevelAlarmSetpoint = Convert.ToSingle (text);
                            } catch {
                                //
                            }
                        }

                        var ic = IndividualControl.Empty;
                        text = (string)obj["inputCard"];
                        if (text.IsEmpty ()) {
                            enable = false;
                        } else {
                            try {
                                ic.Group = AquaPicDrivers.AnalogInput.GetCardIndex (text);
                            } catch {
                                //
                            }
                        }

                        if (ic.Group != -1) {
                            text = (string)obj["channel"];
                            if (text.IsEmpty ()) {
                                ic = IndividualControl.Empty;
                                enable = false;
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
                            enable,
                            enableHighLevelAlarm,
                            highLevelAlarmSetpoint,
                            enableLowLevelAlarm,
                            lowLevelAlarmSetpoint,
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
                        var analogLevelSensorName = (string)obj["analogLevelSensorName"];

                        AddWaterLevelGroup (name, analogLevelSensorName);
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
                                ic.Group = AquaPicDrivers.DigitalInput.GetCardIndex (text);
                            } catch {
                                //
                            }
                        }

                        if (ic.Group != -1) {
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
            foreach (var analogSensor in analogLevelSensors.Values) {
                analogSensor.UpdateWaterLevel ();
            }

            foreach (var waterLevelGroup in waterLevelGroups.Values) {
                waterLevelGroup.Run ();
            }
        }

        /**************************************************************************************************************/
        /* Water Level Groups                                                                                         */
        /**************************************************************************************************************/
        public static void AddWaterLevelGroup (string name, string analogLevelSensorName) {
            if (!WaterLevelGroupNameOk (name)) {
                throw new Exception (string.Format ("Water Level Group: {0} already exists", name));
            }

            waterLevelGroups.Add (name, new WaterLevelGroup (name, analogLevelSensorName));
        }

        public static void RemoveWaterLevelGroup (string name) {
            CheckWaterLevelGroupKey (name);
            waterLevelGroups[name].DisconnectAnalogSensorAlarmsFromDataLogger ();
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

        /***Analog Sensor Name***/
        public static string GetWaterLevelGroupAnalogSensorName (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].analogLevelSensorName;
        }

        /***High switch alarm index***/
        public static int GetWaterLevelGroupHighSwitchAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].highSwitchAlarmIndex;
        }

        /***Low switch alarm index***/
        public static int GetWaterLevelGroupLowSwitchAlarmIndex (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].lowSwitchAlarmIndex;
        }

        /***Data logger***/
        public static DataLogger GetWaterLevelGroupDataLogger (string name) {
            CheckWaterLevelGroupKey (name);
            return waterLevelGroups[name].dataLogger;
        }

        /***Setters****************************************************************************************************/
        /***Analog Sensor***/
        public static void SetWaterLevelGroupAnalogSensorName (string name, string analogSensorName) {
            CheckWaterLevelGroupKey (name);
            if (analogSensorName.IsNotEmpty ()) {
                CheckAnalogLevelSensorKey (analogSensorName);
            }
            waterLevelGroups[name].DisconnectAnalogSensorAlarmsFromDataLogger ();
            waterLevelGroups[name].analogLevelSensorName = analogSensorName;
            waterLevelGroups[name].ConnectAnalogSensorAlarmsToDataLogger ();
        }

        /**************************************************************************************************************/
        /* Analog Level Sensor                                                                                        */
        /**************************************************************************************************************/
        public static void AddAnalogLevelSensor (
            string name,
            bool enable,
            bool enableHighLevelAlarm,
            float highLevelAlarmSetpoint,
            bool enableLowLevelAlarm,
            float lowLevelAlarmSetpoint,
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
                highLevelAlarmSetpoint,
                lowLevelAlarmSetpoint,
                enable,
                enableHighLevelAlarm,
                enableLowLevelAlarm
            ));

            analogLevelSensors[name].zeroScaleValue = zeroScaleCalibrationValue;
            analogLevelSensors[name].fullScaleActual = fullScaleCalibrationActual;
            analogLevelSensors[name].fullScaleValue = fullScaleCalibrationValue;
        }

        public static void AddAnalogLevelSensor (
            string name,
            bool enable,
            bool enableHighLevelAlarm,
            float highLevelAlarmSetpoint,
            bool enableLowLevelAlarm,
            float lowLevelAlarmSetpoint,
            IndividualControl ic
        ) {
            if (!AnalogLevelSensorNameOk (name)) {
                throw new Exception (string.Format ("Water Level Group: {0} already exists", name));
            }

            analogLevelSensors.Add (name, new WaterLevelSensor (
                name,
                ic,
                highLevelAlarmSetpoint,
                lowLevelAlarmSetpoint,
                enable,
                enableHighLevelAlarm,
                enableLowLevelAlarm
            ));
        }

        public static void RemoveAnalogLevelSensor (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            foreach (var waterGroup in waterLevelGroups.Values) {
                if (waterGroup.analogLevelSensorName == analogLevelSensorName) {
                    waterGroup.analogLevelSensorName = string.Empty;
                }
            }
            analogLevelSensors[analogLevelSensorName].Remove ();
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

        /***Enable***/
        public static bool GetAnalogLevelSensorEnable (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].enable;
        }

        /***High level alarm enable***/
        public static bool GetAnalogLevelSensorHighLevelAlarmEnable (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].enableHighAlarm;
        }

        /***High level alarm setpoint***/
        public static float GetAnalogLevelSensorHighLevelAlarmSetpoint (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].highAlarmSetpoint;
        }

        /***High level alarm index***/
        public static int GetAnalogLevelSensorHighLevelAlarmIndex (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].highAlarmIndex;
        }

        /***Low level alarm enable***/
        public static bool GetAnalogLevelSensorLowLevelAlarmEnable (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].enableLowAlarm;
        }

        /***Low level alarm setpoint***/
        public static float GetAnalogLevelSensorLowLevelAlarmSetpoint (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].lowAlarmSetpoint;
        }

        /***Low level alarm index***/
        public static int GetAnalogLevelSensorLowLevelAlarmIndex (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].lowAlarmIndex;
        }

        /***Connected***/
        public static bool GetAnalogLevelSensorConnected (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].connected;
        }

        /***Disconnected alarm index***/
        public static int GetAnalogLevelSensorDisconnectedAlarmIndex (string analogLevelSensorName) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            return analogLevelSensors[analogLevelSensorName].disconnectedAlarmIndex;
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

        /***Enable***/
        public static void SetAnalogLevelSensorEnable (string analogLevelSensorName, bool enable) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            analogLevelSensors[analogLevelSensorName].enable = enable;
        }

        /***Individual Control***/
        public static void SetAnalogLevelSensorIndividualControl (string analogLevelSensorName, IndividualControl channel) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            analogLevelSensors[analogLevelSensorName].Remove ();
            analogLevelSensors[analogLevelSensorName].Add (channel);
        }

        /***High level alarm enable***/
        public static void SetAnalogLevelSensorHighLevelAlarmEnable (string analogLevelSensorName, bool enableHighLevelAlarm) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            analogLevelSensors[analogLevelSensorName].enableHighAlarm = enableHighLevelAlarm;
        }

        /***High level alarm setpoint***/
        public static void SetAnalogLevelSensorHighLevelAlarmSetpoint (string analogLevelSensorName, float highLevelAlarmSetpoint) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            analogLevelSensors[analogLevelSensorName].highAlarmSetpoint = highLevelAlarmSetpoint;
        }

        /***Low level alarm enable***/
        public static void SetAnalogLevelSensorLowLevelAlarmEnable (string analogLevelSensorName, bool enableLowLevelAlarm) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            analogLevelSensors[analogLevelSensorName].enableLowAlarm = enableLowLevelAlarm;
        }

        /***Low level alarm setpoint***/
        public static void SetAnalogLevelSensorLowLevelAlarmSetpoint (string analogLevelSensorName, float lowLevelAlarmSetpoint) {
            CheckAnalogLevelSensorKey (analogLevelSensorName);
            analogLevelSensors[analogLevelSensorName].lowAlarmSetpoint = lowLevelAlarmSetpoint;
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
