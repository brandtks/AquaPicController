using System;
using AquaPic.Drivers;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Modules
{
    public partial class Temperature
    {
        private class TemperatureProbe
        {
            public IndividualControl channel;
            public float temperature;
            public string name;
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
                this.name = name;
                channel.Group = cardId;
                channel.Individual = channelId;
                this.zeroActual = zeroActual;
                this.zeroValue = zeroValue;
                this.fullScaleActual = fullScaleActual;
                this.fullScaleValue = fullScaleValue;
                this.temperatureGroupName = temperatureGroupName;

                AquaPicDrivers.AnalogInput.AddChannel (channel, this.name);
                temperature = 32.0f;
                
                probeDisconnectedAlarmIndex = Alarm.Subscribe (string.Format ("{0} disconnected", name));
            }

            public bool GetTemperature () {
                temperature = AquaPicDrivers.AnalogInput.GetChannelValue (channel);
                temperature = temperature.Map (zeroValue, fullScaleValue, zeroActual, fullScaleActual);

                if (temperature < zeroActual) {
                    if (!Alarm.CheckAlarming (probeDisconnectedAlarmIndex)) {
                        Alarm.Post (probeDisconnectedAlarmIndex);
                        if (CheckTemperatureGroupKeyNoThrow (temperatureGroupName)) {
                            temperatureGroups[temperatureGroupName].dataLogger.AddEntry ("disconnected alarm");
                        }
                    }
                    return false;
                } else {
                    if (Alarm.CheckAlarming (probeDisconnectedAlarmIndex)) {
                        Alarm.Clear (probeDisconnectedAlarmIndex);
                    }
                }

                return true;
            }
        }
    }
}

