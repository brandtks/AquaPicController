#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2018 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.Collections.Generic;
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    public class LightingStateDisplay : EventBox
    {
        bool clicked, startButtonClicked, endButtonClicked;
        uint clickTimer;
        int clickX, clickY;
        int selectedState;

        const int graphVericalEdgeWidth = 40;
        const int graphHorizontalEdgeWidth = 60;
        int graphLeftRelative, graphRightRelative, graphTopRelative, graphBottomRelative;

        List<StateInfo> stateInfos;

        public LightingStateDisplay () {
            Visible = true;
            VisibleWindow = false;
            SetSizeRequest (540, 360);

            graphLeftRelative = graphVericalEdgeWidth;
            graphTopRelative = graphHorizontalEdgeWidth;
            selectedState = -1;
            stateInfos = new List<StateInfo> ();

            ExposeEvent += onExpose;
            ButtonPressEvent += OnButtonPress;
            ButtonReleaseEvent += OnButtonRelease;
        }

        public override void Dispose () {
            GLib.Source.Remove (clickTimer);
            base.Dispose ();
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            var left = Allocation.Left;
            var graphLeft = left + graphVericalEdgeWidth;
            var right = Allocation.Right;
            var graphRight = right - graphVericalEdgeWidth;
            var width = Allocation.Width;
            graphRightRelative = width - graphVericalEdgeWidth;
            var midX = (graphRightRelative - graphVericalEdgeWidth) / 2 + graphLeft;

            var top = Allocation.Top;
            var graphTop = top + graphHorizontalEdgeWidth;
            var bottom = Allocation.Bottom;
            var graphBottom = bottom - graphHorizontalEdgeWidth;
            var height = Allocation.Height;
            graphBottomRelative = height - graphHorizontalEdgeWidth;
            var midY = (graphBottomRelative - graphHorizontalEdgeWidth) / 2 + graphTop;

            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                cr.Rectangle (left, top, width, height);
                TouchColor.SetSource (cr, "grey1");
                cr.Stroke ();

                // Draw the graph outline
                cr.MoveTo (graphLeft - 5, graphTop);
                cr.LineTo (graphLeft, graphTop);
                cr.LineTo (graphLeft, midY);
                cr.LineTo (graphLeft - 5, midY);
                cr.LineTo (graphLeft, midY);
                cr.LineTo (graphLeft, graphBottom);
                cr.LineTo (graphLeft - 5, graphBottom);
                cr.LineTo (graphLeft, graphBottom);
                cr.LineTo (graphLeft, graphBottom + 5);
                cr.LineTo (graphLeft, graphBottom);
                cr.LineTo (midX, graphBottom);
                cr.LineTo (midX, graphBottom + 5);
                cr.LineTo (midX, graphBottom);
                cr.LineTo (graphRight, graphBottom);
                cr.LineTo (graphRight, graphBottom);
                cr.LineTo (graphRight, graphBottom + 5);
                TouchColor.SetSource (cr, "grey3");
                cr.Stroke ();

                // Draw the y axis labels
                var textWidth = graphVericalEdgeWidth - 7;
                var text = new TouchText ("100%");
                text.alignment = TouchAlignment.Right;
                text.Render (this, left, graphTop - 12, textWidth);

                text = new TouchText ("50%");
                text.alignment = TouchAlignment.Right;
                text.Render (this, left, midY - 12, textWidth);

                text = new TouchText ("0%");
                text.alignment = TouchAlignment.Right;
                text.Render (this, left, graphBottom - 13, textWidth);

                // Draw the states
                bool firstTimeThrough = true, lastOnSecondLine = false;
                for (var i = 0; i < stateInfos.Count; ++i) {
                    var stateInfo = stateInfos[i];
                    var state = stateInfo.lightingState;

                    var startXPos = state.startTime.totalMinutes.Map (0, 1440, graphLeft, graphRight);
                    stateInfo.startStateXPos = startXPos - left;
                    var endXPos = state.endTime.totalMinutes.Map (0, 1440, graphLeft, graphRight);
                    stateInfo.endStateXPos = endXPos - left;

                    if (state.type != LightingStateType.Off) {
                        var startYPos = state.startingDimmingLevel.Map (0, 100, graphBottom, graphTop);
                        var endYPos = state.endingDimmingLevel.Map (0, 100, graphBottom, graphTop);

                        var rightPart = graphRight - startXPos;
                        double period;
                        if (state.startTime.Before (state.endTime)) {
                            period = endXPos - startXPos;
                        } else {
                            period = rightPart + (endXPos - graphLeft);
                        }
                        var delta = endYPos - startYPos;
                        var interXPos = graphLeft - rightPart;

                        if (firstTimeThrough) {
                            cr.MoveTo (startXPos, startYPos);
                            cr.Arc (startXPos, startYPos, 3, -Math.PI, Math.PI);
                            cr.ClosePath ();
                            TouchColor.SetSource (cr, "secb");
                            cr.Fill ();
                        }

                        cr.MoveTo (startXPos, startYPos);
                        switch (state.type) {
                        case LightingStateType.LinearRamp: {
                                if (state.startTime.Before (state.endTime)) {
                                    cr.LineTo (endXPos, endYPos);
                                } else {
                                    var rightRatio = rightPart / period;
                                    var rightYPos = startYPos + (rightRatio * delta);

                                    cr.LineTo (graphRight, rightYPos);

                                    cr.MoveTo (graphLeft, rightYPos);
                                    cr.LineTo (endXPos, endYPos);
                                }
                                break;
                            }
                        case LightingStateType.ParabolaRamp: {
                                delta = Math.Abs (delta);
                                double interYPos = graphBottom;

                                if (state.startTime.Before (state.endTime)) {
                                    for (var phase = 1; phase <= period; ++phase) {
                                        var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                        interYPos = startYPos - delta * Math.Sin (radian);
                                        cr.LineTo (startXPos + phase, interYPos);
                                    }
                                } else {
                                    for (var phase = 1; phase <= rightPart; ++phase) {
                                        var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                        interYPos = startYPos - delta * Math.Sin (radian);
                                        cr.LineTo (startXPos + phase, interYPos);
                                    }

                                    cr.MoveTo (graphLeft, interYPos);
                                    for (var phase = rightPart; phase <= period; ++phase) {
                                        var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                        interYPos = startYPos - delta * Math.Sin (radian);
                                        cr.LineTo (interXPos + phase, interYPos);
                                    }
                                }
                                endYPos = (float)interYPos;
                                break;
                            }
                        case LightingStateType.HalfParabolaRamp: {
                                delta = Math.Abs (delta);
                                double mapFrom1, mapFrom2, basePoint;
                                if (startYPos <= endYPos) {
                                    mapFrom1 = 1d;
                                    mapFrom2 = 0d;
                                    basePoint = endYPos;
                                } else {
                                    mapFrom1 = 0d;
                                    mapFrom2 = 1d;
                                    basePoint = startYPos;
                                }

                                double interYPos = graphBottom;
                                if (state.startTime.Before (state.endTime)) {
                                    for (var phase = 1; phase <= period; ++phase) {
                                        var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                        interYPos = basePoint - delta * Math.Sin (radian);
                                        cr.LineTo (startXPos + phase, interYPos);
                                    }
                                    cr.LineTo (endXPos, endYPos);
                                } else {
                                    for (var phase = 1; phase <= rightPart; ++phase) {
                                        var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                        interYPos = basePoint - delta * Math.Sin (radian);
                                        cr.LineTo (startXPos + phase, interYPos);
                                    }

                                    cr.MoveTo (graphLeft, interYPos);
                                    for (var phase = rightPart; phase <= period; ++phase) {
                                        var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                        interYPos = basePoint - delta * Math.Sin (radian);
                                        cr.LineTo (interXPos + phase, interYPos);
                                    }
                                }
                                break;
                            }
                        case LightingStateType.On:
                            endYPos = startYPos;
                            if (state.startTime.Before (state.endTime)) {
                                cr.LineTo (endXPos, startYPos);
                            } else {
                                cr.LineTo (graphRight, startYPos);

                                cr.MoveTo (graphLeft, startYPos);
                                cr.LineTo (endXPos, startYPos);
                            }
                            break;
                        }
                        TouchColor.SetSource (cr, "secb", 0.5);
                        cr.Stroke ();

                        cr.MoveTo (endXPos, endYPos);
                        cr.Arc (endXPos, endYPos, 3, -Math.PI, Math.PI);
                        cr.ClosePath ();
                        cr.Fill ();

                        if (selectedState == -1) {
                            // Only the first state needs the starting time drawn. All other states the start time 
                            // is the same as the last end time.
                            if (firstTimeThrough) {
                                text = new TouchText (state.startTime.ToShortTimeString ());
                                text.alignment = TouchAlignment.Center;
                                text.Render (this, startXPos.ToInt () - 50, graphBottom, 100);
                                firstTimeThrough = false;
                            }

                            // If the start and end of the state are close together draw the end at alterating elevations
                            int textYPos;
                            if (period < 80) {
                                if (lastOnSecondLine) {
                                    textYPos = graphBottom;
                                    lastOnSecondLine = false;
                                } else {
                                    textYPos = graphBottom + 18;
                                    lastOnSecondLine = true;
                                }
                            } else {
                                textYPos = graphBottom;
                                lastOnSecondLine = false;
                            }

                            text = new TouchText (state.endTime.ToShortTimeString ());
                            text.alignment = TouchAlignment.Center;
                            text.Render (this, endXPos.ToInt () - 50, textYPos, 100);
                        }

                        if (i == selectedState) {
                            cr.MoveTo (startXPos, graphBottom);
                            cr.LineTo (startXPos, startYPos);
                            switch (state.type) {
                            case LightingStateType.LinearRamp: {
                                    if (state.startTime.Before (state.endTime)) {
                                        cr.LineTo (endXPos, endYPos);
                                    } else {
                                        var rightRatio = rightPart / period;
                                        var rightYPos = startYPos + (rightRatio * delta);

                                        cr.LineTo (graphRight, rightYPos);
                                        cr.LineTo (graphRight, graphBottom);
                                        cr.ClosePath ();

                                        cr.MoveTo (graphLeft, graphBottom);
                                        cr.LineTo (graphLeft, rightYPos);
                                        cr.LineTo (endXPos, endYPos);
                                    }
                                    break;
                                }
                            case LightingStateType.ParabolaRamp: {
                                    delta = Math.Abs (delta);
                                    double interYPos = graphBottom;

                                    if (state.startTime.Before (state.endTime)) {
                                        for (var phase = 1; phase <= period; ++phase) {
                                            var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                            interYPos = startYPos - delta * Math.Sin (radian);
                                            cr.LineTo (startXPos + phase, interYPos);
                                        }
                                    } else {
                                        for (var phase = 1; phase <= rightPart; ++phase) {
                                            var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                            interYPos = startYPos - delta * Math.Sin (radian);
                                            cr.LineTo (startXPos + phase, interYPos);
                                        }
                                        cr.LineTo (graphRight, graphBottom);
                                        cr.ClosePath ();

                                        cr.MoveTo (graphLeft, graphBottom);
                                        cr.LineTo (graphLeft, interYPos);
                                        for (var phase = rightPart; phase <= period; ++phase) {
                                            var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                            interYPos = startYPos - delta * Math.Sin (radian);
                                            cr.LineTo (interXPos + phase, interYPos);
                                        }
                                    }
                                    endYPos = (float)interYPos;
                                    break;
                                }
                            case LightingStateType.HalfParabolaRamp: {
                                    delta = Math.Abs (delta);
                                    double mapFrom1, mapFrom2, basePoint;
                                    if (startYPos <= endYPos) {
                                        mapFrom1 = 1d;
                                        mapFrom2 = 0d;
                                        basePoint = endYPos;
                                    } else {
                                        mapFrom1 = 0d;
                                        mapFrom2 = 1d;
                                        basePoint = startYPos;
                                    }

                                    double interYPos = graphBottom;
                                    if (state.startTime.Before (state.endTime)) {
                                        for (var phase = 1; phase <= period; ++phase) {
                                            var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                            interYPos = basePoint - delta * Math.Sin (radian);
                                            cr.LineTo (startXPos + phase, interYPos);
                                        }
                                        cr.LineTo (endXPos, endYPos);
                                    } else {
                                        for (var phase = 1; phase <= rightPart; ++phase) {
                                            var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                            interYPos = basePoint - delta * Math.Sin (radian);
                                            cr.LineTo (startXPos + phase, interYPos);
                                        }
                                        cr.LineTo (graphRight, graphBottom);
                                        cr.ClosePath ();

                                        cr.MoveTo (graphLeft, graphBottom);
                                        cr.LineTo (graphLeft, interYPos);
                                        for (var phase = rightPart; phase <= period; ++phase) {
                                            var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                            interYPos = basePoint - delta * Math.Sin (radian);
                                            cr.LineTo (interXPos + phase, interYPos);
                                        }
                                    }

                                    break;
                                }
                            case LightingStateType.On:
                                endYPos = startYPos;
                                if (state.startTime.Before (state.endTime)) {
                                    cr.LineTo (endXPos, startYPos);
                                } else {
                                    cr.LineTo (graphRight, startYPos);
                                    cr.LineTo (graphRight, graphBottom);
                                    cr.ClosePath ();

                                    cr.MoveTo (graphLeft, graphBottom);
                                    cr.LineTo (graphLeft, startYPos);
                                    cr.LineTo (endXPos, startYPos);
                                }
                                break;
                            }
                            cr.LineTo (endXPos, graphBottom);
                            cr.ClosePath ();
                            TouchColor.SetSource (cr, "grey2", 0.5);
                            cr.Fill ();

                            double startButtonX, endButtonX;
                            if (period < 84) {
                                startButtonX = startXPos - 80 + (period - 2) / 2;
                                endButtonX = endXPos - (period - 2) / 2;
                            } else {
                                startButtonX = startXPos - 40;
                                endButtonX = endXPos - 40;
                            }
                            stateInfo.startButtonXPos = startButtonX - left;
                            stateInfo.endButtonXPos = endButtonX - left;

                            TouchGlobal.DrawRoundedRectangle (cr, startButtonX, graphBottom - 10, 80, 20, 8);
                            var color = new TouchColor ("pri");
                            if (startButtonClicked) {
                                color.ModifyColor (1.25);
                            }
                            color.SetSource (cr);
                            cr.Fill ();

                            TouchGlobal.DrawRoundedRectangle (cr, endButtonX, graphBottom - 10, 80, 20, 8);
                            color = new TouchColor ("pri");
                            if (endButtonClicked) {
                                color.ModifyColor (1.25);
                            }
                            color.SetSource (cr);
                            cr.Fill ();

                            cr.Rectangle (startXPos - 80, graphBottom + 15, 80, 25);
                            TouchColor.SetSource (cr, "grey4");
                            cr.FillPreserve ();
                            TouchColor.SetSource (cr, "black");
                            cr.Stroke ();

                            text = new TouchText (state.startTime.ToShortTimeString ());
                            text.alignment = TouchAlignment.Center;
                            text.font.color = "black";
                            text.Render (this, startXPos.ToInt () - 80, graphBottom + 18, 80);

                            cr.Rectangle (endXPos, graphBottom + 15, 80, 25);
                            TouchColor.SetSource (cr, "grey4");
                            cr.FillPreserve ();
                            TouchColor.SetSource (cr, "black");
                            cr.Stroke ();

                            text = new TouchText (state.endTime.ToShortTimeString ());
                            text.alignment = TouchAlignment.Center;
                            text.font.color = "black";
                            text.Render (this, endXPos.ToInt (), graphBottom + 18, 80);
                        }
                    }
                }

                /*
                var xPos = Time.TimeNow.ToTimeSpan ().TotalMinutes.Map (0, 1440, graphLeft, graphRight);
                cr.MoveTo (xPos, graphBottom);
                cr.LineTo (xPos, graphTop);
                TouchColor.SetSource (cr, "seca");
                cr.Stroke ();
                */
            }
        }

        public void SetStates (LightingState[] lightingStates) {
            stateInfos.Clear ();
            selectedState = -1;
            for (int i = 0; i < lightingStates.Length; ++i) {
                var stateInfo = new StateInfo ();
                stateInfo.lightingState = lightingStates[i];
                stateInfos.Add (stateInfo);
            }

            if (stateInfos.Count >= 2) {
                var last = stateInfos.Count - 1;
                for (int i = 0; i < stateInfos.Count; ++i) {
                    var next = i + 1;
                    var previous = i - 1;
                    if (i == 0) {
                        stateInfos[i].previous = stateInfos[last];
                        stateInfos[i].next = stateInfos[next];
                    } else if (i == last) {
                        stateInfos[i].previous = stateInfos[previous];
                        stateInfos[i].next = stateInfos[0];
                    } else {
                        stateInfos[i].previous = stateInfos[previous];
                        stateInfos[i].next = stateInfos[next];
                    }
                }
            }
        }

        protected void OnButtonPress (object sender, ButtonPressEventArgs args) {
            clicked = true;
            clickX = args.Event.X.ToInt ();
            clickY = args.Event.Y.ToInt ();

            if (selectedState != -1) {
                if ((clickY > graphBottomRelative - 10) && (clickY < graphBottomRelative + 10)) {
                    var stateInfo = stateInfos[selectedState];
                    if ((clickX > stateInfo.startButtonXPos) && (clickX < stateInfo.startButtonXPos + 80)) {
                        startButtonClicked = true;
                        clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
                    }

                    if ((clickX > stateInfo.endButtonXPos) && (clickX < stateInfo.endButtonXPos + 80)) {
                        endButtonClicked = true;
                        clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
                    }
                }
            }
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var x = args.Event.X;
            var y = args.Event.Y;

            var clickHappenedOnEntity = startButtonClicked | endButtonClicked;
            if (!clickHappenedOnEntity) {
                // The release happened on the graph
                if ((x > graphLeftRelative) && 
                    (x < graphRightRelative) &&
                    (y > graphTopRelative) &&
                    (y < graphBottomRelative)) {
                    for (var i = 0; i < stateInfos.Count; ++i) {
                        var stateInfo = stateInfos[i];
                        var state = stateInfo.lightingState;
                        if (state.startTime.Before (state.endTime)) {
                            if ((x > stateInfo.startStateXPos) && (x < stateInfo.endStateXPos)) {
                                if (state.type != LightingStateType.Off) {
                                    if (selectedState == i) {
                                        selectedState = -1;
                                    } else {
                                        selectedState = i;
                                    }
                                } else {
                                    selectedState = -1;
                                }
                                break;
                            }
                        } else {
                            if (((x > stateInfo.startStateXPos) && (x < graphRightRelative)) ||
                                ((x > graphVericalEdgeWidth) && (x < stateInfo.endStateXPos))) {
                                if (state.type != LightingStateType.Off) {
                                    if (selectedState == i) {
                                        selectedState = -1;
                                    } else {
                                        selectedState = i;
                                    }
                                } else {
                                    selectedState = -1;
                                }
                                break;
                            }
                        }
                    }

                    QueueDraw ();
                }
            }

            clicked = false;
            clickHappenedOnEntity = false;
            startButtonClicked = false;
            endButtonClicked = false;
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);

                double yDelta = clickY - y;
                double xDelta = clickX - x;

                if (startButtonClicked) {
                    var newStartMinutes = x.Map (graphLeftRelative, graphRightRelative, 0, 1440);
                    if (newStartMinutes < 0) {
                        newStartMinutes = 1440;
                    } else if (newStartMinutes > 1440) {
                        newStartMinutes = 0;
                    }

                    var stateInfo = stateInfos[selectedState];
                    var currentState = stateInfo.lightingState;
                    var currentStateStartMinutes = currentState.startTime.totalMinutes;
                    var currentStateEndMinutes = currentState.endTime.totalMinutes;
                    double currentTotalMinutes;
                    if (currentStateStartMinutes < currentStateEndMinutes) {
                        currentTotalMinutes = currentStateEndMinutes - newStartMinutes;
                    } else {
                        currentTotalMinutes = (1440 - newStartMinutes) + currentStateEndMinutes;
                    }

                    if (currentTotalMinutes < 15) {
                        newStartMinutes = currentStateStartMinutes.ToInt ();
                    }

                    var previousState = stateInfo.previous.lightingState;
                    var previousStateStartMinutes = previousState.startTime.totalMinutes;
                    var previousStateEndMinutes = previousState.endTime.totalMinutes;

                    double previousStateLength;
                    if (previousStateStartMinutes < previousStateEndMinutes) {
                        previousStateLength = newStartMinutes - previousStateStartMinutes;
                    } else {
                        previousStateLength = (1440 - previousStateStartMinutes) + newStartMinutes;
                    }

                    if (previousState.type != LightingStateType.Off) {
                        if (previousStateLength < 15) {
                            newStartMinutes = currentStateStartMinutes.ToInt ();
                        }
                    } else {
                        if (previousStateLength < 0) {
                            newStartMinutes = currentStateStartMinutes.ToInt ();
                        }
                    }

                    var startTime = new Time (new TimeSpan (0, newStartMinutes, 0));
                    Console.WriteLine ("Start time: {0}", startTime.ToShortTimeString ());

                    currentState.startTime = startTime;
                    previousState.endTime = startTime;
                }

                QueueDraw ();
            }

            return clicked;
        }

        class StateInfo
        {
            public StateInfo previous;
            public StateInfo next;
            public LightingState lightingState;
            public double startStateXPos, endStateXPos;
            public double startButtonXPos, endButtonXPos;
        }
    }
}
