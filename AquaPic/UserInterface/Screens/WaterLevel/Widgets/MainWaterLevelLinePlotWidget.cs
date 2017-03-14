using System;
using System.Linq;
using Gtk;
using AquaPic.Modules;
using AquaPic.Runtime;
using AquaPic.Utilites;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class WaterLevelLinePlot : LinePlotWidget
    {
        TouchLabel label;
        
        public WaterLevelLinePlot (params object[] options) 
            : base () 
        {
            text = "Water Level";
            unitOfMeasurement = TouchWidgetLibrary.UnitsOfMeasurement.Inches;

            var eventbox = new EventBox ();
            eventbox.VisibleWindow = false;
            eventbox.SetSizeRequest (WidthRequest, HeightRequest);
            eventbox.ButtonReleaseEvent += (o, args) => {
                var topWidget = this.Toplevel;
                AquaPicGui.AquaPicUserInterface.ChangeScreens ("Water Level", topWidget, AquaPicGui.AquaPicUserInterface.currentScene);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();

            label = new TouchLabel ();
            label.SetSizeRequest (152, 16);
            label.textColor = "compl";
            label.textAlignment = TouchAlignment.Right;
            label.textHorizontallyCentered = true;
            Put (label, 155, 63);

            linePlot.rangeMargin = 1;
            linePlot.LinkDataLogger (WaterLevel.dataLogger);
            linePlot.eventColors.Add ("probe disconnected", new TouchColor ("secb", 0.25));
            linePlot.eventColors.Add ("ato started", new TouchColor ("seca", 0.5));
            linePlot.eventColors.Add ("ato stopped", new TouchColor ("secc", 0.5));
            linePlot.eventColors.Add ("disconnected alarm", new TouchColor ("compl", 0.25));
            linePlot.eventColors.Add ("low alarm", new TouchColor ("compl", 0.25));
            linePlot.eventColors.Add ("high alarm", new TouchColor ("compl", 0.25));

            Destroyed += (obj, args) => {
                linePlot.UnLinkDataLogger (WaterLevel.dataLogger);
            };

            OnUpdate ();
        }

        public override void OnUpdate () {
            if (WaterLevel.analogSensorEnabled) {
                if (WaterLevel.analogWaterLevel < 0.0f) {
                    textBox.text = "--";
                    label.Visible = true;
                    label.text = "Disconnected";
                } else {
                    currentValue = WaterLevel.analogWaterLevel;
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

