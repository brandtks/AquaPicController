using System;
using System.Collections.Generic;
using AquaPic.Alarm;
using AquaPic.AnalogInputDriver;
using AquaPic.Globals;
using AquaPic.PowerDriver;

namespace AquaPic.TemperatureDriver
{
    public partial class Temperature
    {
        public static Temperature Main = new Temperature ();

        private int _highTempAlarmIdx;
        private int _lowTempAlarmIdx;
        public int highTempAlarmIdx {
            get { return _highTempAlarmIdx; }
        }
        public int lowTempAlarmIdx {
            get { return _lowTempAlarmIdx; }
        }
        public float highTempAlarmSetpoint;
        public float lowTempAlarmSetpoint;
        public List<Heater> heaters;
        public columnTemperature columnTemp;

        private Temperature () {
            _highTempAlarmIdx = -1;
            _lowTempAlarmIdx = -1;
            heaters = new List<Heater> ();
        }

        public void init (int columnTempCardID, int columnTempChannelID, string colummTempName) {
            _highTempAlarmIdx = alarm.subscribe ("High temperature", "Water column temperature too high");
            _lowTempAlarmIdx = alarm.subscribe ("Low temperature", "Water column temperature too low");
            alarm.addPostHandler (_highTempAlarmIdx, highTempHandler);
            alarm.addPostHandler (_lowTempAlarmIdx, lowTempHandler);
            addColumnTemperatureChannel (columnTempCardID, columnTempChannelID, colummTempName);
        }

        public void addColumnTemperatureChannel (int cardID, int channelID, string name) {
            AnalogInput.Main.addChannel (cardID, channelID, AnalogType.Temperature, name);
            columnTemp.addChannel ((byte)cardID, (byte)channelID);
        }

        public void addHeater (
            int powerID, 
            int plugID,
            string name,
            bool controlTemp = false, 
            float setpoint = 78.0f, 
            float offset = 0.3f)
        {
            Power.Main.addPlug (powerID, plugID, name, true);
            heaters.Add (new Heater ((byte)powerID, (byte)plugID, controlTemp, setpoint, offset));
        }

        public void run () {
            columnTemp.getColumnTemperature ();

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

        private void highTempHandler (object sender) {
            for (int i = 0; i < heaters.Count; ++i) {
                heaters [i].turnHeaterOff (true);
            }
        }

        private void lowTempHandler (object sender) {
            for (int i = 0; i < heaters.Count; ++i) {
                heaters [i].turnHeaterOn (true);
            }
        }
    }
}

