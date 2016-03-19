using System;
using Gtk;
using AquaPic.Modules;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class WaterLevelWidget : BarPlotWidget 
    {
        private TouchLabel label;
        private int flashUpdate;
        private bool enabled;

        public WaterLevelWidget () : base () {
            text = "Water Level";
            unitOfMeasurement = TouchWidgetLibrary.UnitsOfMeasurement.Inches;

            label = new TouchLabel ();
            label.textColor = "compl";
            label.text = "Probe Disconnected";
            label.WidthRequest = 199;
            label.textAlignment = TouchAlignment.Center;
            Put (label, 3, 55);
            label.Show ();

            var eventbox = new EventBox ();
            eventbox.VisibleWindow = false;
            eventbox.SetSizeRequest (WidthRequest, HeightRequest);
            eventbox.ButtonReleaseEvent += (o, args) => {
                var topWidget = this.Toplevel;
                AquaPicGUI.ChangeScreens ("Water Level", topWidget, AquaPicGUI.currentScreen);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();

            fullScale = 15.0f;

            flashUpdate = 0;

            enabled = !WaterLevel.analogSensorEnabled;

            OnUpdate ();
        }

        public override void OnUpdate () {
            if (WaterLevel.analogSensorEnabled) {
                if (!enabled)
                    label.text = "Probe Disconnected";

                if (WaterLevel.analogWaterLevel < 0.0f) {
                    currentValue = 0.0f;

                    flashUpdate = ++flashUpdate % 2;
                    if (flashUpdate < 1)
                        label.Visible = true;
                    else
                        label.Visible = false;
                } else {
                    currentValue = WaterLevel.analogWaterLevel;
                    label.Visible = false;
                    flashUpdate = 0;
                }
            } else {
                if (enabled) {
                    label.text = "Probe Disabled";
                    label.Visible = true;
                }
            }
        }
    }
}

