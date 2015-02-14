using System;
using System.Collections.Generic;
using AquaPic.AnalogInputDriver;

namespace AquaPic.TemperatureDriver
{
    public partial class Temperature
    {
        public class columnTemperature
        {
            private float _temperature;
            private List<analogInputCh> channels; 
            public float temperature {
                get { return _temperature; }
            }

            public columnTemperature () {
                this.channels = new List<analogInputCh> ();
                this._temperature = 0.0f;
            }

            public void addChannel (byte cardID, byte channelID) {
                analogInputCh ch = new analogInputCh ();
                ch.cardID = cardID;
                ch.channelID = channelID;
                channels.Add (ch);
            }

            public float getColumnTemperature () {
                for (int i = 0; i < channels.Count; ++i)
                    _temperature += AnalogInput.Main.getAnalog (channels [i]);
                _temperature /= channels.Count;
                return _temperature;
            }
        }
    }
}

