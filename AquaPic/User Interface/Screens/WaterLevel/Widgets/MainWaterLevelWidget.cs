using System;
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

            label = new TouchLabel ();
            label.textColor = "compl";
            label.text = "Probe Dis-\nconnected";
            label.textSize = 13;
            label.WidthRequest = 102;
            Put (label, 3, 147);
            label.Show ();

            fullScale = 15.0f;

            flashUpdate = 0;

            enabled = !WaterLevel.analogSensorEnabled;

            OnUpdate ();
        }

        public override void OnUpdate () {
            if (WaterLevel.analogSensorEnabled) {
                if (!enabled)
                    label.text = "Probe Dis-\nconnected";

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
                    label.text = "Probe\nDisabled";
                    label.Visible = true;
                }
            }
        }
    }
}

