using System;
using System.Linq;
using Cairo;
using Gtk;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace TouchWidgetLibrary
{
    public class TouchLinePlot : EventBox
    {
        private TouchLinePlotOptions _options;
        public TouchLinePlotOptions options {
            get {
                return _options;
            }
        }
        
        private CircularBuffer<LogEntry> rollingStorage;
        public CircularBuffer<LogEntry> DataLogEntries {
            get {
                return rollingStorage;
            }
        }
        public double rangeMargin;

        public TouchLinePlot () {
            Visible = true;
            VisibleWindow = false;
            SetSizeRequest (296, 76);

            _options = new TouchLinePlotOptions ();
            rollingStorage = new CircularBuffer<LogEntry> (288);
            rangeMargin = 3;

            ExposeEvent += OnExpose;
            //ButtonReleaseEvent += OnButtonRelease;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int top = Allocation.Top;
                int left = Allocation.Left;
                int height = Allocation.Height;
                int bottom = Allocation.Bottom;

                cr.Rectangle (left + 8, top, 288, height - 6);
                TouchColor.SetSource (cr, "grey3", 0.15f);
                cr.Fill ();

                if (rollingStorage.buffer.Count > 0) {
                    var workingBuffer = rollingStorage.buffer.ToArray ();

                    var min = workingBuffer.Min (entry => entry.value) - rangeMargin;
                    var max = workingBuffer.Max (entry => entry.value) + rangeMargin;

                    Array.Reverse (workingBuffer);

                    var y = workingBuffer[0].value.Map (min, max, bottom - 6, top);
                    var x = left + 8;
                    cr.MoveTo (x, y);

                    for (int i = 1; i < workingBuffer.Length; ++i) {
                        y = workingBuffer[i].value.Map (min, max, bottom - 6, top);
                        x = left + 8 + i;
                        cr.LineTo (x, y);
                    }

                    TouchColor.SetSource (cr, "pri");
                    cr.Stroke ();

                    var textRender = new TouchText ();
                    textRender.textWrap = TouchTextWrap.Shrink;
                    textRender.alignment = TouchAlignment.Right;
                    textRender.font.color = "white";

                    textRender.text = max.ToInt ().ToString ();
                    textRender.Render (this, left - 9, top - 2, 16);

                    textRender.text = min.ToInt ().ToString ();
                    textRender.Render (this, left - 9, bottom - 22, 16);
                }
            }
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {

        }

        public void LinkDataLogger (DataLogger logger) {
            logger.DataLogEntryAddedEvent += OnDataLogEntryAdded;
            var entries = logger.GetEntries (288, _options.TimeSpanToSeconds ());
            rollingStorage.buffer.AddRange (entries);
        }

        public void UnLinkDataLogger (DataLogger logger) {
            logger.DataLogEntryAddedEvent -= OnDataLogEntryAdded;
        }

        public void OnDataLogEntryAdded (object sender, DataLogEntryAddedEventArgs args) {
            if (rollingStorage.buffer.Count > 0) {
                var previous = rollingStorage.buffer[rollingStorage.buffer.Count - 1].dateTime;
                var totalSeconds = args.dateTime.Subtract (previous).TotalSeconds.ToInt ();
                var secondTimeSpan = _options.TimeSpanToSeconds ();
                if (totalSeconds >= secondTimeSpan) {
                    rollingStorage.Add (new LogEntry (args.dateTime, args.value));
                } 
            } else {
                rollingStorage.Add (new LogEntry (args.dateTime, args.value));
            }

            QueueDraw ();
        }
    }

    public class TouchLinePlotOptions
    {
        TouchLinePlotTimeSpan timeSpan;

        public TouchLinePlotOptions () {
            timeSpan = TouchLinePlotTimeSpan.Seconds1;
        }

        public int TimeSpanToSeconds () {
            switch (timeSpan) {
                case TouchLinePlotTimeSpan.Seconds1:
                    return 1;
                case TouchLinePlotTimeSpan.Seconds5:
                    return 5;
                case TouchLinePlotTimeSpan.Seconds10:
                    return 10;
                case TouchLinePlotTimeSpan.Minute1:
                    return 60;
                case TouchLinePlotTimeSpan.Minute2:
                    return 120;
                case TouchLinePlotTimeSpan.Minute5:
                    return 300;
                case TouchLinePlotTimeSpan.Minute10:
                    return 600;
                default:
                    return 1;
            }
        }
    }

    public enum TouchLinePlotTimeSpan {
        Seconds1,
        Seconds5,
        Seconds10,
        Minute1,
        Minute2,
        Minute5,
        Minute10
    }
}

