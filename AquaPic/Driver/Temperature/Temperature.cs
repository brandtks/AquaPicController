using System;
using System.Collections.Generic;
using AquaPic.AlarmDriver;
using AquaPic.AnalogInputDriver;
using AquaPic.Globals;
using AquaPic.PowerDriver;

namespace AquaPic.TemperatureDriver
{
    public partial class Temperature
    {
        //public static Temperature Main = new Temperature ();

        public static float highTempAlarmSetpoint = 82;
        public static float lowTempAlarmSetpoint = 75;

        public static int HighTemperatureAlarmIndex {
            get { return columnTemp._highTempAlarmIdx; }
        }
        public static int LowTemperatureAlarmIndex {
            get { return columnTemp._lowTempAlarmIdx; }
        }

        private static List<Heater> heaters = new List<Heater> ();

        private static ColumnTemperature columnTemp = new ColumnTemperature (HighTempHandler);
        public static float WaterColumnTemperature {
            get { return columnTemp.temperature; }
        }

        //private Temperature () {
            //_highTempAlarmIdx = -1;
            //_lowTempAlarmIdx = -1;
            //heaters = new List<Heater> ();
        //}

//        public static void Init () {
//            _highTempAlarmIdx = Alarm.Subscribe ("High temperature", "Water column temperature too high");
//            _lowTempAlarmIdx = Alarm.Subscribe ("Low temperature", "Water column temperature too low");
//            Alarm.AddPostHandler (_highTempAlarmIdx, HighTempHandler);
//            Alarm.AddPostHandler (_lowTempAlarmIdx, LowTempHandler);
//        }

        public static void AddTemperatureProbe (int cardID, int channelID, string name, bool waterColumn = true) {
            AnalogInput.AddChannel (cardID, channelID, AnalogType.Temperature, name);
            if (waterColumn) {
                columnTemp.AddColumnTemperature (cardID, channelID);
            }
        }

        public static void AddHeater (
            int powerID, 
            int plugID,
            string name,
            bool controlTemp = false, 
            float setpoint = 78.0f, 
            float offset = 0.3f)
        {
            heaters.Add (new Heater ((byte)powerID, (byte)plugID, controlTemp, setpoint, offset, name));
        }

        public static void Run () {
            columnTemp.GetColumnTemperature ();

            for (int i = 0; i < heaters.Count; ++i) {
                heaters [i].PlugControl.Execute ();
            }
        }

        private static void HighTempHandler (object sender) {
            for (int i = 0; i < heaters.Count; ++i)
                Power.AlarmShutdownPlug (heaters [i].Plug);
        }
    }
}

