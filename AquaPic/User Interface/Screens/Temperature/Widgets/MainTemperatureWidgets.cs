using System;

namespace AquaPic
{
    public class TemperatureLinePlot : LinePlotWidget
    {
        public TemperatureLinePlot () : base () {
            text = "Temperature";
            OnUpdate ();
        }

        public override void OnUpdate () {
            currentValue = Modules.Temperature.WaterTemperature;
        }
    }
}

