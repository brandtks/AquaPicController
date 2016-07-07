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
        private CircularBuffer<LogEntry> rollingStorage;
        public CircularBuffer<LogEntry> dataPoints {
            get {
                return rollingStorage;
            }
        }

        public double rangeMargin;
        
        public TouchLinePlotTimeSpan timeSpan;
        
        private int _pointSpacing;
        public int pointSpacing {
            get {
                return _pointSpacing;
            }
            set {
                if (value > 288) {
                    _pointSpacing = 288;
                } else if (value < 1) {
                    _pointSpacing = 1;
                } else {
                    _pointSpacing = value;
                }

                rollingStorage.maxSize = maxDataPoints;
            }
        }

        public int maxDataPoints {
            get {
                return 288 / _pointSpacing;
            }
        }

        public TouchLinePlot () {
            Visible = true;
            VisibleWindow = false;
            SetSizeRequest (296, 76);

            timeSpan = TouchLinePlotTimeSpan.Seconds1;
            _pointSpacing = 1;

            rollingStorage = new CircularBuffer<LogEntry> (maxDataPoints);
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

                    var min = workingBuffer.Min (entry => entry.value);
                    var max = workingBuffer.Max (entry => entry.value);

                    if ((max - min) < rangeMargin) {
                        max += (rangeMargin / 2);
                        min -= (rangeMargin / 2);
                    }

                    Array.Reverse (workingBuffer);

                    var y = workingBuffer[0].value.Map (min, max, bottom - 10, top + 4);
                    var x = left + 8;
                    cr.MoveTo (x, y);

                    for (int i = 1; i < workingBuffer.Length; ++i) {
                        y = workingBuffer[i].value.Map (min, max, bottom - 10, top + 4);
                        x = left + 8 + (i * _pointSpacing);
                        cr.LineTo (x, y);
                    }

                    if (workingBuffer.Length == maxDataPoints) {
                        cr.LineTo (left + 296, y);
                    }

                    TouchColor.SetSource (cr, "pri");
                    cr.Stroke ();

                    var textRender = new TouchText ();
                    textRender.textWrap = TouchTextWrap.Shrink;
                    textRender.alignment = TouchAlignment.Right;
                    textRender.font.color = "white";

                    textRender.text = Math.Ceiling (max).ToString ();
                    textRender.Render (this, left - 9, top - 2, 16);

                    textRender.text = Math.Floor (min).ToString ();
                    textRender.Render (this, left - 9, bottom - 22, 16);
                }
            }
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {

        }

        public void LinkDataLogger (DataLogger logger) {
            logger.DataLogEntryAddedEvent += OnDataLogEntryAdded;
            var entries = logger.GetEntries (maxDataPoints, TimeSpanToSeconds ());
            rollingStorage.buffer.AddRange (entries);
        }

        public void UnLinkDataLogger (DataLogger logger) {
            logger.DataLogEntryAddedEvent -= OnDataLogEntryAdded;
        }

        public void OnDataLogEntryAdded (object sender, DataLogEntryAddedEventArgs args) {
            if (rollingStorage.count > 0) {
                var previous = rollingStorage.buffer[rollingStorage.count - 1].dateTime;
                var totalSeconds = args.dateTime.Subtract (previous).TotalSeconds.ToInt ();
                var secondTimeSpan = TimeSpanToSeconds ();
                if (totalSeconds >= secondTimeSpan) {
                    rollingStorage.Add (new LogEntry (args.dateTime, args.value));
                } 
            } else {
                rollingStorage.Add (new LogEntry (args.dateTime, args.value));
            }

            QueueDraw ();
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

