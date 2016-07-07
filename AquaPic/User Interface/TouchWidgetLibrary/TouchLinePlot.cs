using System;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using Gtk;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace TouchWidgetLibrary
{
    public class TouchLinePlot : EventBox
    {
        string name;
        
        private CircularBuffer<LogEntry> _dataPoints;
        public CircularBuffer<LogEntry> dataPoints {
            get {
                return _dataPoints;
            }
        }

        private CircularBuffer<LogEntry> _eventPoints;
        public CircularBuffer<LogEntry> eventPoints {
            get {
                return _eventPoints;
            }
        }

        public Dictionary<string, TouchColor> eventColors;

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

                _dataPoints.maxSize = maxDataPoints;
                _eventPoints.maxSize = maxDataPoints;
            }
        }

        public int maxDataPoints {
            get {
                return 288 / _pointSpacing;
            }
        }

        public double rangeMargin;
        public TouchLinePlotPointTimeDifference timeSpan;
        public TouchLinePlotStartingPoint startingPoint;

        public TouchLinePlot () {
            Visible = true;
            VisibleWindow = false;
            SetSizeRequest (296, 76);

            name = "Unlinked";

            _pointSpacing = 1;
            _dataPoints = new CircularBuffer<LogEntry> (maxDataPoints);
            _eventPoints = new CircularBuffer<LogEntry> (maxDataPoints);
            eventColors = new Dictionary<string, TouchColor> ();
            
            rangeMargin = 3;
            timeSpan = TouchLinePlotPointTimeDifference.Seconds1;
            startingPoint = new TouchLinePlotStartingPoint ();

            ExposeEvent += OnExpose;
            //ButtonReleaseEvent += OnButtonRelease;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int top = Allocation.Top;
                int left = Allocation.Left;
                int height = Allocation.Height;
                int bottom = Allocation.Bottom;
                var now = DateTime.Now;

                cr.Rectangle (left + 8, top, 288, height - 6);
                TouchColor.SetSource (cr, "grey3", 0.15f);
                cr.Fill ();

                //Value points
                if (_dataPoints.count > 0) {
                    var valueBuffer = _dataPoints.ToArray ();

                    var min = valueBuffer.Min (entry => entry.value);
                    var max = valueBuffer.Max (entry => entry.value);

                    if ((max - min) < rangeMargin) {
                        max += (rangeMargin / 2);
                        min -= (rangeMargin / 2);
                    }

                    Array.Reverse (valueBuffer);

                    TouchColor.SetSource (cr, "pri");

                    var y = valueBuffer[0].value.Map (min, max, bottom - 10, top + 4);
                    double x = left + 8;
                    var pointDifference = now.Subtract (valueBuffer[0].dateTime).TotalSeconds / (double)PointTimeDifferenceToSeconds ();
                    if (pointDifference > 2) {
                        x += pointDifference * (double)_pointSpacing;
                    }
                    cr.MoveTo (x, y);

                    for (int i = 1; i < valueBuffer.Length; ++i) {
                        y = valueBuffer[i].value.Map (min, max, bottom - 10, top + 4);
                        x = left + 8;
                        
                        pointDifference = now.Subtract (valueBuffer[i].dateTime).TotalSeconds / (double)PointTimeDifferenceToSeconds ();
                        x += pointDifference * (double)_pointSpacing;

                        var previousDifference = valueBuffer[i - 1].dateTime.Subtract (valueBuffer[i].dateTime).TotalSeconds / (double)PointTimeDifferenceToSeconds ();
                        if (previousDifference > 2) {
                            cr.Stroke ();

                            if (x > (left + 296)) {
                                break;
                            } else {
                                cr.MoveTo (x, y);
                            }
                        } else {
                            if (x > (left + 296)) {
                                x = left + 296;
                                cr.LineTo (x, y);
                                break;
                            } else {
                                cr.LineTo (x, y);
                            }
                        }
                    }

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

                //Event points
                if (_eventPoints.count > 0) {
                    var eventBuffer = _eventPoints.ToArray ();
                    Array.Reverse (eventBuffer);
                    for (int i = 0; i < eventBuffer.Length; i++) {
                        double x = left + 8;
                        x += (now.Subtract (eventBuffer[i].dateTime).TotalSeconds / (double)PointTimeDifferenceToSeconds ()) * (double)_pointSpacing;
                        if (x > (left + 296)) {
                            break;
                        }

                        cr.Rectangle (x, top, _pointSpacing, height - 6);

                        if (eventColors.ContainsKey (eventBuffer[i].eventType)) {
                            eventColors[eventBuffer[i].eventType].SetSource (cr);
                        } else {
                            TouchColor.SetSource (cr, "seca", .375);
                        }

                        cr.Fill ();
                    }
                }
            }
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {

        }

        public void LinkDataLogger (DataLogger logger) {
            name = logger.name;
            
            var endSearchTime = DateTime.Now.Subtract (new TimeSpan (0, 0, maxDataPoints * PointTimeDifferenceToSeconds ()));
            
            logger.ValueLogEntryAddedEvent += OnValueLogEntryAdded;
            var valueEntries = logger.GetValueEntries (maxDataPoints, PointTimeDifferenceToSeconds (), endSearchTime);
            _dataPoints.AddRange (valueEntries);

            logger.EventLogEntryAddedEvent += OnEventLogEntryAdded;
            var eventEntries = logger.GetEventEntries (maxDataPoints, endSearchTime);
            _eventPoints.AddRange (eventEntries);
        }

        public void UnLinkDataLogger (DataLogger logger) {
            logger.ValueLogEntryAddedEvent -= OnValueLogEntryAdded;
            logger.EventLogEntryAddedEvent -= OnEventLogEntryAdded;
        }

        public void OnValueLogEntryAdded (object sender, DataLogEntryAddedEventArgs args) {
            if (_dataPoints.count > 0) {
                var previous = _dataPoints[_dataPoints.count - 1].dateTime;
                var totalSeconds = args.entry.dateTime.Subtract (previous).TotalSeconds.ToInt ();
                var secondTimeSpan = PointTimeDifferenceToSeconds ();
                if (totalSeconds >= secondTimeSpan) {
                    _dataPoints.Add (new LogEntry (args.entry));
                } 
            } else {
                _dataPoints.Add (new LogEntry (args.entry));
            }

            QueueDraw ();
        }

        public void OnEventLogEntryAdded (object sender, DataLogEntryAddedEventArgs args) {
            _eventPoints.Add (new LogEntry (args.entry));
            QueueDraw ();
        }

        public int PointTimeDifferenceToSeconds () {
            switch (timeSpan) {
                case TouchLinePlotPointTimeDifference.Seconds1:
                    return 1;
                case TouchLinePlotPointTimeDifference.Seconds5:
                    return 5;
                case TouchLinePlotPointTimeDifference.Seconds10:
                    return 10;
                case TouchLinePlotPointTimeDifference.Minute1:
                    return 60;
                case TouchLinePlotPointTimeDifference.Minute2:
                    return 120;
                case TouchLinePlotPointTimeDifference.Minute5:
                    return 300;
                case TouchLinePlotPointTimeDifference.Minute10:
                    return 600;
                default:
                    return 1;
            }
        }
    }

    public enum TouchLinePlotPointTimeDifference {
        [Description ("One second")]
        Seconds1,
        
        [Description ("Five seconds")]
        Seconds5,

        [Description ("Ten seconds")]
        Seconds10,

        [Description ("One minute")]
        Minute1,

        [Description ("Two minutes")]
        Minute2,

        [Description ("Five minutes")]
        Minute5,

        [Description ("Ten minutes")]
        Minute10
    }

    public class TouchLinePlotStartingPoint
    {
        public TouchLinePlotStartTime startPoint;
        public DateTime pastTimePoint;

        public TouchLinePlotStartingPoint () {
            startPoint = TouchLinePlotStartTime.Now;
            pastTimePoint = DateTime.MinValue;
        }
    }

    public enum TouchLinePlotStartTime {
        Now,
        PastTimePoint
    }
}

