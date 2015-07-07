using System;
using AquaPic.Modules;
using MyWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class WaterLevelWidget : BarPlotWidget 
    {
        private TouchLabel label;
        private int flashUpdate;

        public WaterLevelWidget () : base () {
            text = "Water Level";

            label = new TouchLabel ();
            label.textColor = "compl";
            label.text = "Probe Dis-\nconnected";
            label.textSize = 13;
            label.WidthRequest = 102;
            Put (label, 3, 70);
            label.Show ();

            fullScale = 15.0f;

            flashUpdate = 0;

            OnUpdate ();
        }

        public override void OnUpdate () {
            if (WaterLevel.analogSensorEnabled) {
                if (WaterLevel.analogWaterLevel < 0.0f) {
                    currentValue = 0.0f;
                    if (WaterLevel.analogWaterLevel <= 1.0f) {
                        flashUpdate = ++flashUpdate % 2;
                        if (flashUpdate < 1)
                            label.Visible = true;
                        else
                            label.Visible = false;
                    }
                } else {
                    currentValue = WaterLevel.analogWaterLevel;
                    label.Visible = false;
                    flashUpdate = 0;
                }
            }
        }
    }
}

