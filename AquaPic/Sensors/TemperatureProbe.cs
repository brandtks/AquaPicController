using System;
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Sensors
{
    public class TemperatureProbe : ISensor<float>
    {
        protected IndividualControl _channel;
        public IndividualControl channel {
            get {
                return _channel;
            }
        }

        protected string _name;
        public string name {
            get {
                return _name;
            }
        }

        protected float _temperature;
        public float temperature {
            get {
                return _temperature;
            }
        }
        public string temperatureGroupName;

        public float zeroActual;
        public float zeroValue;
        public float fullScaleActual;
        public float fullScaleValue;

        public int probeDisconnectedAlarmIndex;

        public TemperatureProbe (
            string name, 
            int cardId, 
            int channelId,
            float zeroActual,
            float zeroValue,
            float fullScaleActual,
            float fullScaleValue, 
            string temperatureGroupName
        ) {
            _name = name;
            _channel.Group = cardId;
            _channel.Individual = channelId;
            this.zeroActual = zeroActual;
            this.zeroValue = zeroValue;
            this.fullScaleActual = fullScaleActual;
            this.fullScaleValue = fullScaleValue;
            this.temperatureGroupName = temperatureGroupName;
            _temperature = this.zeroActual;

            Add (_channel);

            probeDisconnectedAlarmIndex = Alarm.Subscribe (string.Format ("{0} temperature probe disconnected", name));
        }

        public void Add (IndividualControl channel) {
            _channel = channel;
            AquaPicDrivers.AnalogInput.AddChannel (_channel, this.name);
        }

        public void Remove () {
            AquaPicDrivers.AnalogInput.RemoveChannel (_channel);
        }

        public float Get () {
            _temperature = AquaPicDrivers.AnalogInput.GetChannelValue (_channel);
            _temperature = temperature.Map (zeroValue, fullScaleValue, zeroActual, fullScaleActual);

            if (temperature < zeroActual) {
                if (!Alarm.CheckAlarming (probeDisconnectedAlarmIndex)) {
                    Alarm.Post (probeDisconnectedAlarmIndex);
                }
            } else {
                if (Alarm.CheckAlarming (probeDisconnectedAlarmIndex)) {
                    Alarm.Clear (probeDisconnectedAlarmIndex);
                }
            }

            return _temperature;
        }

        public void SetName (string name) {
            _name = name;
            AquaPicDrivers.AnalogInput.SetChannelName (_channel, _name);
        }
    }
}

