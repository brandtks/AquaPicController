using System;
using System.Collections.Generic;
using AquaPic.AnalogInputDriver;
using AquaPic.Globals;

namespace AquaPic.TemperatureDriver
{
    public partial class Temperature
    {
        private class ColumnTemperature
        {
            public float temperature;
            private List<IndividualControl> channels; 

            public ColumnTemperature () {
                this.channels = new List<IndividualControl> ();
                this.temperature = 0.0f;
            }

            public void AddColumnTemperature (int cardID, int channelID) {
                IndividualControl ch = new IndividualControl ();
                ch.Group = (byte)cardID;
                ch.Individual = (byte)channelID;
                channels.Add (ch);
                // the Analog Input Channel is already added by the main temperature class
            }

            public float GetColumnTemperature () {
                for (int i = 0; i < channels.Count; ++i)
                    temperature += AnalogInput.GetAnalogValue (channels [i]);
                temperature /= channels.Count;
                return temperature;
            }
        }
    }
}

