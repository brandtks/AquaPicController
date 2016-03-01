using System;
using AquaPic.Modules;
using Gtk;

namespace AquaPic.UserInterface
{
    public delegate float GetDimmingLevelHandler ();

    public class DimmingLightBarPlot : BarPlotWidget
    {
        public GetDimmingLevelHandler GetDimmingLevel;

        public DimmingLightBarPlot (string name, GetDimmingLevelHandler GetDimmingLevel) {
            text = name;
            this.GetDimmingLevel = GetDimmingLevel;

            var eventbox = new EventBox ();
            eventbox.VisibleWindow = false;
            eventbox.SetSizeRequest (WidthRequest, HeightRequest);
            eventbox.ButtonReleaseEvent += (o, args) => {
                var topWidget = this.Toplevel;
                AquaPicGUI.ChangeScreens ("Lighting", topWidget, AquaPicGUI.currentScreen, name);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();
        }

        public override void OnUpdate () {
            currentValue = GetDimmingLevel ();
        }
    }
}

