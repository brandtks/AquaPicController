using System;
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
            Put (label, 118, 5);
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

            linePlot.LinkDataLogger (WaterLevel.dataLogger);
            WaterLevel.dataLogger.DataLogEntryAddedEvent -= linePlot.OnDataLogEntryAdded;
            WaterLevel.dataLogger.DataLogEntryAddedEvent += OnWaterLevelDataLogEntryAdded;
            linePlot.rangeMargin = 0.5;

            Destroyed += (obj, args) => {
                linePlot.UnLinkDataLogger (WaterLevel.dataLogger);
                WaterLevel.dataLogger.DataLogEntryAddedEvent -= OnWaterLevelDataLogEntryAdded;
            };

            OnUpdate ();
        }

        protected void OnWaterLevelDataLogEntryAdded (object obj, DataLogEntryAddedEventArgs args) {
            if (args.value < 0.0f) {

            }
            
            if (linePlot.DataLogEntries.buffer.Count > 0) {
                var previous = linePlot.DataLogEntries.buffer[linePlot.DataLogEntries.buffer.Count - 1].dateTime;
                var totalSeconds = args.dateTime.Subtract (previous).TotalSeconds.ToInt ();
                var secondTimeSpan = linePlot.options.TimeSpanToSeconds ();
                if (totalSeconds >= secondTimeSpan) {
                    linePlot.DataLogEntries.Add (new LogEntry (args.dateTime, args.value));
                }
            } else {
                linePlot.DataLogEntries.Add (new LogEntry (args.dateTime, args.value));
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

