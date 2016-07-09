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
        //private TouchLabel label;
        //private int flashUpdate;

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
                AquaPicGUI.ChangeScreens ("Water Level", topWidget, AquaPicGUI.currentScreen);
            };
            Put (eventbox, 0, 0);
            eventbox.Show ();

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

        protected void OnWaterLevelDataLogEntryAdded (object obj, DataLogEntryAddedEventArgs args) {
            if (linePlot.dataPoints.count > 0) {
                var previous = linePlot.dataPoints[linePlot.dataPoints.count - 1].dateTime;
                var totalSeconds = args.entry.dateTime.Subtract (previous).TotalSeconds.ToInt ();
                var secondTimeSpan = linePlot.PointTimeDifferenceToSeconds ();
                if (totalSeconds >= secondTimeSpan) {
                    linePlot.dataPoints.Add (new LogEntry (args.entry));
                }
            } else {
                linePlot.dataPoints.Add (new LogEntry (args.entry));
            }

            QueueDraw ();
        }

        public override void OnUpdate () {
            if (WaterLevel.analogSensorEnabled) {
                if (WaterLevel.analogWaterLevel < 0.0f) {
                    OverrideTextBoxValue ("Disconnected");
                } else {
                    currentValue = WaterLevel.analogWaterLevel;
                }
            } else {
                OverrideTextBoxValue ("Disabled");
            }
        }
    }

    public class WaterLevelWidget : BarPlotWidget
    {
        private TouchLabel label;
        private int flashUpdate;

        public WaterLevelWidget ()
            : base () {
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

            OnUpdate ();
        }

        public override void OnUpdate () {
            if (WaterLevel.analogSensorEnabled) {
                if (WaterLevel.analogWaterLevel < 0.0f) {
                    currentValue = 0.0f;

                    flashUpdate = ++flashUpdate % 4;
                    if (flashUpdate <= 1)
                        label.Visible = true;
                    else
                        label.Visible = false;
                } else {
                    currentValue = WaterLevel.analogWaterLevel;
                    label.Visible = false;
                    flashUpdate = 0;
                }
            } else {
                label.text = "Probe Disabled";
                label.Visible = true;
            }
        }
    }
}

