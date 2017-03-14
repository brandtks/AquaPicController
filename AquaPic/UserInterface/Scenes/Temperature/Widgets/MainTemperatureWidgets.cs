using System;
using Gtk;
using AquaPic.Modules;
using AquaPic.Utilites;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class TemperatureLinePlot : LinePlotWidget
    {
        string groupName;
        TouchLabel label;

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
                AquaPicGui.AquaPicUserInterface.ChangeScreens ("Temperature", topWidget, AquaPicGui.AquaPicUserInterface.currentScene);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();

            label = new TouchLabel ();
            label.SetSizeRequest (152, 16);
            label.textColor = "compl";
            label.textAlignment = TouchAlignment.Right;
            label.textHorizontallyCentered = true;
            Put (label, 155, 63);

            groupName = string.Empty;
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

                text = string.Format ("{0} Temperature", groupName);
            }

            linePlot.eventColors.Add ("no probes", new TouchColor ("secb", 0.25));
            linePlot.eventColors.Add ("heater on", new TouchColor ("seca", 0.5));
            linePlot.eventColors.Add ("heater off", new TouchColor ("secc", 0.5));
            linePlot.eventColors.Add ("disconnected alarm", new TouchColor ("compl", 0.25));
            linePlot.eventColors.Add ("low alarm", new TouchColor ("compl", 0.5));
            linePlot.eventColors.Add ("high alarm", new TouchColor ("compl", 0.5));

            OnUpdate ();
        }

        public override void OnUpdate () {
            if (groupName.IsNotEmpty ()) {
                if (Temperature.AreTemperatureProbesConnected (groupName)) {
                    currentValue = Temperature.GetTemperatureGroupTemperature (groupName);
                } else {
                    textBox.text = "--";
                    label.Visible = true;
                    label.text = "Disconnected";
                }
            } else {
                currentValue = Temperature.temperature;
            }
        }
    }
}

