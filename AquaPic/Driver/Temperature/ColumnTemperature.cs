using System;
using System.Collections.Generic;
using AquaPic.AnalogInput;

namespace AquaPic.Temp
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
                _temperature += analogInput.getAnalog (channels [i]);
            _temperature /= channels.Count;
            return _temperature;
        }
    }
}

