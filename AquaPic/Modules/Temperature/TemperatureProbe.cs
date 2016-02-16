using System;
using AquaPic.Drivers;
using AquaPic.Utilites;

namespace AquaPic.Modules
{
    public partial class Temperature
    {
        private class TemperatureProbe
        {
            public IndividualControl channel;
            public float temperature;
            public string name;

            public TemperatureProbe (string name, int cardId, int channelId) {
                this.name = name;
                channel.Group = (byte)cardId;
                channel.Individual = (byte)channelId;
                AquaPicDrivers.AnalogInput.AddChannel (channel, this.name);
                temperature = 0.0f;
            }

            public float GetTemperature () {
                temperature = AquaPicDrivers.AnalogInput.GetChannelValue (channel);
                temperature = temperature.Map (0, 4096, 32.0f, 100.0f);
                return temperature;
            }
        }
    }
}

