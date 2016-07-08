using System;
using Gtk;
using AquaPic.Modules;
using AquaPic.Utilites;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class TemperatureLinePlot : LinePlotWidget
    {
        public TemperatureLinePlot (params object[] options)
            : base () 
        {
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

            string groupName = string.Empty;
            if (options.Length >= 1) {
                groupName = options[0] as string;
                if (groupName != null) {
                    if (!Temperature.CheckTemperatureGroupKeyNoThrow (groupName)) {
                        groupName = Temperature.defaultTemperatureGroup;
                    }
                } else {
                    groupName = Temperature.defaultTemperatureGroup;
                }
            } else {
                groupName = Temperature.defaultTemperatureGroup;
            }

            if (!groupName.IsEmpty ()) {
                linePlot.LinkDataLogger (Temperature.GetTemperatureGroupDataLogger (groupName));

                Destroyed += (obj, args) => {
                    linePlot.UnLinkDataLogger (Temperature.GetTemperatureGroupDataLogger (groupName));
                };

                var label = new TouchLabel ();
                label.text = groupName;
                label.WidthRequest = 112;
                label.textSize = 9;
                label.textColor = "grey3";
                label.textAlignment = TouchAlignment.Center;
                Put (label, 3, 65);
            }

            linePlot.eventColors.Add ("probe disconnected", new TouchColor ("secb", 0.25));
            linePlot.eventColors.Add ("heater on", new TouchColor ("seca", 0.5));
            linePlot.eventColors.Add ("heater off", new TouchColor ("secc", 0.5));
            linePlot.eventColors.Add ("disconnected alarm", new TouchColor ("compl", 0.25));
            linePlot.eventColors.Add ("low alarm", new TouchColor ("compl", 0.5));
            linePlot.eventColors.Add ("high alarm", new TouchColor ("compl", 0.5));

            OnUpdate ();
        }

        public override void OnUpdate () {
            currentValue = Modules.Temperature.temperature;
        }
    }
}

