using System;

namespace AquaPic.UserInterface
{
    public class TemperatureLinePlot : LinePlotWidget
    {
        public TemperatureLinePlot () : base () {
            text = "Temperature";
            unitOfMeasurement = TouchWidgetLibrary.UnitsOfMeasurement.Degrees;
            OnUpdate ();
        }

        public override void OnUpdate () {
            currentValue = Modules.Temperature.WaterTemperature;
        }
    }
}

