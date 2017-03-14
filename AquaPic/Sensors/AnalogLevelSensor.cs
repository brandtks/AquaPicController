using System;
using AquaPic.Drivers;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.Sensors
{
    public class AnalogLevelSensor : ISensor<float>
    {
        protected float _level;
        public float level {
            get {
                return _level;
            }
        }

        protected string _name;
        public string name {
            get {
                return _name;
            }
        }

        public IndividualControl _channel;
        public IndividualControl channel {
            get {
                return _channel;
            }
        }

        public float zeroValue;
        public float fullScaleActual;
        public float fullScaleValue;

        public int sensorDisconnectedAlarmIndex = -1;
        public bool enableDisconnectedAlarm {
            get {
                return sensorDisconnectedAlarmIndex != -1;
            }
            set {
                if (value) {
                    sensorDisconnectedAlarmIndex = Alarm.Subscribe ("Probe disconnected, " + name);
                } else {
                    sensorDisconnectedAlarmIndex = -1;
                }
            }
        }

        public AnalogLevelSensor (string name, IndividualControl ic) {
            _name = name;
            _level = 0.0f;

            zeroValue = 819.2f;
            fullScaleActual = 15.0f;
            fullScaleValue = 4096.0f;

            sensorDisconnectedAlarmIndex = Alarm.Subscribe ("Probe disconnected, " + name);

            if (ic.IsNotEmpty ()) {
                Add (ic);
            }
        }

        public void Add (IndividualControl channel) {
            _channel = channel;
            AquaPicDrivers.AnalogInput.AddChannel (_channel, name);
        }

        public void Remove () {
            AquaPicDrivers.AnalogInput.RemoveChannel (_channel);
        }

        public float Get () {
            _level = AquaPicDrivers.AnalogInput.GetChannelValue (_channel);
            _level = _level.Map (zeroValue, fullScaleValue, 0.0f, fullScaleActual);

            if (_level < 0.0f) {
                if (!Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                    Alarm.Post (sensorDisconnectedAlarmIndex);
                }
            } else {
                if (Alarm.CheckAlarming (sensorDisconnectedAlarmIndex)) {
                    Alarm.Clear (sensorDisconnectedAlarmIndex);
                }
            }

            return level;
        }

        public void ChangeName (string name) {
            _name = name;
            AquaPicDrivers.AnalogInput.SetChannelName (_channel, name);
        }
    }
}

