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
        private TouchLabel label;
        private int flashUpdate;

        public WaterLevelLinePlot () : base () {
            text = "Water Level";
            unitOfMeasurement = TouchWidgetLibrary.UnitsOfMeasurement.Inches;

            label = new TouchLabel ();
            label.textColor = "compl";
            label.text = "Probe Disconnected";
            label.WidthRequest = 199;
            Put (label, 126, 5);
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

            linePlot.rangeMargin = 1;
            WaterLevel.dataLogger.DataLogEntryAddedEvent += OnWaterLevelDataLogEntryAdded;
            var entries = WaterLevel.dataLogger.GetEntries (linePlot.maxDataPoints, linePlot.TimeSpanToSeconds ());
            var usableEntries = from entry in entries
                                where entry.value >= 0.0
                                select entry;
            linePlot.dataPoints.buffer.AddRange (usableEntries);

            Destroyed += (obj, args) => {
                WaterLevel.dataLogger.DataLogEntryAddedEvent -= OnWaterLevelDataLogEntryAdded;
            };

            OnUpdate ();
        }

        protected void OnWaterLevelDataLogEntryAdded (object obj, DataLogEntryAddedEventArgs args) {
            if (args.value <= 0.0f) {
                return;
            }
            
            if (linePlot.dataPoints.count > 0) {
                var previous = linePlot.dataPoints.buffer[linePlot.dataPoints.count - 1].dateTime;
                var totalSeconds = args.dateTime.Subtract (previous).TotalSeconds.ToInt ();
                var secondTimeSpan = linePlot.TimeSpanToSeconds ();
                if (totalSeconds >= secondTimeSpan) {
                    linePlot.dataPoints.Add (new LogEntry (args.dateTime, args.value));
                }
            } else {
                linePlot.dataPoints.Add (new LogEntry (args.dateTime, args.value));
            }

            QueueDraw ();
        }

        public override void OnUpdate () {
            if (WaterLevel.analogSensorEnabled) {
                if (WaterLevel.analogWaterLevel < 0.0f) {
                    label.text = "Probe Disconnected";
                    currentValue = 0.0f;

                    flashUpdate = ++flashUpdate % 3;
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

