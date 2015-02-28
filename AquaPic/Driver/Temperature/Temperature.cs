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

        public static float highTempAlarmSetpoint;
        public static float lowTempAlarmSetpoint;
        private static int _highTempAlarmIdx;
        private static int _lowTempAlarmIdx;
        public static int highTempAlarmIdx {
            get { return _highTempAlarmIdx; }
        }
        public static int lowTempAlarmIdx {
            get { return _lowTempAlarmIdx; }
        }

        private static List<Heater> heaters = new List<Heater> ();

        private static ColumnTemperature columnTemp = new ColumnTemperature ();
        public static float columnTemperature {
            get { return columnTemp.temperature; }
        }

        //private Temperature () {
            //_highTempAlarmIdx = -1;
            //_lowTempAlarmIdx = -1;
            //heaters = new List<Heater> ();
        //}

        public static void Init () {
            _highTempAlarmIdx = Alarm.Subscribe ("High temperature", "Water column temperature too high");
            _lowTempAlarmIdx = Alarm.Subscribe ("Low temperature", "Water column temperature too low");
            Alarm.AddPostHandler (_highTempAlarmIdx, HighTempHandler);
            Alarm.AddPostHandler (_lowTempAlarmIdx, LowTempHandler);
        }

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

            if (columnTemp.temperature >= highTempAlarmSetpoint) 
                Alarm.Post (_highTempAlarmIdx);

            if (columnTemp.temperature <= lowTempAlarmSetpoint)
                Alarm.Post (_lowTempAlarmIdx);

            for (int i = 0; i < heaters.Count; ++i) {
                if (heaters [i].controlTemp) {
                    if (columnTemp.temperature >= (heaters [i].setpoint + heaters [i].offset))
                        heaters [i].turnHeaterOff ();

                    if (columnTemp.temperature <= (heaters [i].setpoint - heaters [i].offset))
                        heaters [i].turnHeaterOn ();
                }
            }
        }

        private static void HighTempHandler (object sender) {
            for (int i = 0; i < heaters.Count; ++i) {
                heaters [i].turnHeaterOff (true);
            }
        }

        private static void LowTempHandler (object sender) {
            for (int i = 0; i < heaters.Count; ++i) {
                heaters [i].turnHeaterOn (true);
            }
        }
    }
}

