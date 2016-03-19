using System;
using Gtk;

namespace AquaPic.UserInterface
{
    public class TemperatureLinePlot : LinePlotWidget
    {
        public TemperatureLinePlot () : base () {
            text = "Temperature";
            unitOfMeasurement = TouchWidgetLibrary.UnitsOfMeasurement.Degrees;

            var eventbox = new EventBox ();
            eventbox.VisibleWindow = false;
            eventbox.SetSizeRequest (WidthRequest, HeightRequest);
            eventbox.ButtonReleaseEvent += (o, args) => {
                var topWidget = this.Toplevel;
                AquaPicGUI.ChangeScreens ("Temperature", topWidget, AquaPicGUI.currentScreen);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();

            OnUpdate ();
        }

        public override void OnUpdate () {
            currentValue = Modules.Temperature.WaterTemperature;
        }
    }
}

