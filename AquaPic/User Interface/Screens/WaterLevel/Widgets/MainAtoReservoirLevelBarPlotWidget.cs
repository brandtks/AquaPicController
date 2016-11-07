using System;
using System.Linq;
using Gtk;
using AquaPic.Modules;
using AquaPic.Runtime;
using AquaPic.Utilites;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class AtoReservoirLevelWidget : BarPlotWidget
    {
        private TouchLabel label;

        public AtoReservoirLevelWidget ()
            : base () {
            text = "ATO Reservoir Level";
            unitOfMeasurement = TouchWidgetLibrary.UnitsOfMeasurement.Inches;

            label = new TouchLabel ();
            label.textColor = "compl";
            label.text = "Disconnected";
            label.textRender.orientation = TouchOrientation.Vertical;
            label.WidthRequest = 100;
            Put (label, 60, 9);
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

            OnUpdate ();
        }

        public override void OnUpdate () {
            if (WaterLevel.atoReservoirLevelEnabled) {
                if (WaterLevel.atoReservoirLevel < 0.0f) {
                    textBox.text = "--";
                    label.Visible = true;
                    label.text = "Disconnected";
                } else {
                    currentValue = WaterLevel.atoReservoirLevel;
                    label.Visible = false;
                }
            } else {
                textBox.text = "--";
                label.Visible = true;
                label.text = "Disabled";
            }
        }
    }
}