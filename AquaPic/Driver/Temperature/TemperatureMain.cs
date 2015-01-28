using System;
using System.Collections.Generic;
using AquaPic.Alarm;
using AquaPic.AnalogInput;
using AquaPic.Utilites;
using AquaPic.Power;

namespace AquaPic.Temp
{
    public static class temperature
    {
        private static int _highTempAlarmIdx;
        private static int _lowTempAlarmIdx;
        public static int highTempAlarmIdx {
            get { return _highTempAlarmIdx; }
        }
        public static int lowTempAlarmIdx {
            get { return _lowTempAlarmIdx; }
        }
        public static float highTempAlarmSetpoint;
        public static float lowTempAlarmSetpoint;
        public static List<Heater> heaters;
        public static columnTemperature columnTemp;

        static temperature () {
            _highTempAlarmIdx = -1;
            _lowTempAlarmIdx = -1;
            heaters = new List<Heater> ();
        }

        public static void init (
            int columnTempCardID, 
            int columnTempChannelID,
            string colummTempName,
            int heaterPowerID, 
            int heaterPlugID,
            string heaterName,
            bool heaterControlTemp = false,
            float heaterSetpoint = 78.0f,
            float heaterOffset = 0.3f)
        {
            _highTempAlarmIdx = alarm.subscribe ("High temperature", "Water column temperature too high");
            _lowTempAlarmIdx = alarm.subscribe ("Low temperature", "Water column temperature too low");
            alarm.addPostHandler (_highTempAlarmIdx, new alarmHandler (highTempHandler));
            alarm.addPostHandler (_lowTempAlarmIdx, new alarmHandler (lowTempHandler));
            addColumnTemperatureChannel (columnTempCardID, columnTempChannelID, colummTempName);
            addHeater (heaterPowerID, heaterPlugID, heaterName, heaterControlTemp, heaterSetpoint, heaterOffset);
        }

        public static void addColumnTemperatureChannel (int cardID, int channelID, string name) {
            analogInput.addChannel (cardID, channelID, AnalogType.Temperature, name);
            columnTemp.addChannel ((byte)cardID, (byte)channelID);
        }

        public static void addHeater (
            int powerID, 
            int plugID,
            string name,
            bool controlTemp = false, 
            float setpoint = 78.0f, 
            float offset = 0.3f)
        {
            power.addPlug (powerID, plugID, name, true);
            heaters.Add (new Heater ((byte)powerID, (byte)plugID, controlTemp, setpoint, offset));
        }

        public static void run () {
            if (columnTemp.temperature >= highTempAlarmSetpoint) 
                alarm.post (_highTempAlarmIdx);

            if (columnTemp.temperature <= lowTempAlarmSetpoint)
                alarm.post (_lowTempAlarmIdx);

            for (int i = 0; i < heaters.Count; ++i) {
                if (heaters [i].controlTemp) {
                    if (columnTemp.temperature >= (heaters [i].setpoint + heaters [i].offset))
                        heaters [i].turnHeaterOff ();

                    if (columnTemp.temperature <= (heaters [i].setpoint - heaters [i].offset))
                        heaters [i].turnHeaterOn ();
                }
            }
        }

        private static void highTempHandler (object sender) {
            for (int i = 0; i < heaters.Count; ++i) {
                heaters [i].turnHeaterOff (true);
            }
        }

        private static void lowTempHandler (object sender) {
            for (int i = 0; i < heaters.Count; ++i) {
                heaters [i].turnHeaterOn (true);
            }
        }
    }
}

