using System;
using System.Collections.Generic;
using AquaPic.AnalogInputDriver;
using AquaPic.Globals;
using AquaPic.AlarmDriver;

namespace AquaPic.TemperatureDriver
{
    public partial class Temperature
    {
        private class ColumnTemperature
        {
            public float temperature;
            public List<IndividualControl> channels;
            public int _highTempAlarmIdx;
            public int _lowTempAlarmIdx;

            public ColumnTemperature (AlarmHandler HighTempHandler) {
                this.channels = new List<IndividualControl> ();
                this.temperature = 0.0f;

                _highTempAlarmIdx = Alarm.Subscribe ("High temperature", "Water column temperature too high");
                _lowTempAlarmIdx = Alarm.Subscribe ("Low temperature", "Water column temperature too low");
                Alarm.AddPostHandler (_highTempAlarmIdx, HighTempHandler);
            }

            public void AddColumnTemperature (int cardID, int channelID) {
                IndividualControl ch = new IndividualControl ();
                ch.Group = (byte)cardID;
                ch.Individual = (byte)channelID;
                channels.Add (ch);
                // the Analog Input Channel is already added by the main temperature class
            }

            public float GetColumnTemperature () {
                /* @test
                for (int i = 0; i < channels.Count; ++i)
                    temperature += AnalogInput.GetAnalogValue (channels [i]);
                temperature /= channels.Count;*/

                if (temperature >= highTempAlarmSetpoint) 
                    Alarm.Post (_highTempAlarmIdx);

                if (temperature <= lowTempAlarmSetpoint)
                    Alarm.Post (_lowTempAlarmIdx);

                return temperature;
            }
        }
    }
}

