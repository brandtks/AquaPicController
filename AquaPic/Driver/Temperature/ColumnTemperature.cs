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
            private float _temperature;
            private List<IndividualControl> channels; 
            public float temperature {
                get { return _temperature; }
            }

            public ColumnTemperature () {
                this.channels = new List<IndividualControl> ();
                this._temperature = 0.0f;
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
                    _temperature += AnalogInput.GetAnalogValue (channels [i]);
                _temperature /= channels.Count;
                return _temperature;
            }
        }
    }
}

