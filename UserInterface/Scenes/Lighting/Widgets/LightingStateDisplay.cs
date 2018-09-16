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
        double minutesPerPixel;

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

            minutesPerPixel = 1440d / (graphRightRelative - graphLeftRelative);

            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                /*
                cr.Rectangle (left, top, width, height);
                TouchColor.SetSource (cr, "grey1");
                cr.Stroke ();
                */

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

                var timeXPos = Time.TimeNow.totalMinutes.Map (0, 1440, graphLeft, graphRight);
                double timeYPos = graphBottom;

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
                            cr.Arc (startXPos, startYPos, 3, 0 , 2 * Math.PI);
                            cr.ClosePath ();
                            TouchColor.SetSource (cr, "secb");
                            cr.Fill ();
                        }

                        cr.MoveTo (startXPos, startYPos);
                        switch (state.type) {
                        case LightingStateType.LinearRamp: {
                                if (state.startTime.Before (state.endTime)) {
                                    cr.LineTo (endXPos, endYPos);

                                    if ((timeXPos > startXPos) && (timeXPos < endXPos)) {
                                        timeYPos = ((timeXPos - startXPos) / period).Map (0, 1, startYPos, endYPos);
                                    }
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
                                        var currentXPos = startXPos + phase;
                                        var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                        interYPos = startYPos - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinRange (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
                                    }
                                } else {
                                    for (var phase = 1; phase <= rightPart; ++phase) {
                                        var currentXPos = startXPos + phase;
                                        var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                        interYPos = startYPos - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinRange (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
                                    }

                                    cr.MoveTo (graphLeft, interYPos);
                                    for (var phase = rightPart; phase <= period; ++phase) {
                                        var currentXPos = interXPos + phase;
                                        var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                        interYPos = startYPos - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinRange (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
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
                                        var currentXPos = startXPos + phase;
                                        var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                        interYPos = basePoint - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinRange (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
                                    }
                                    cr.LineTo (endXPos, endYPos);
                                } else {
                                    for (var phase = 1; phase <= rightPart; ++phase) {
                                        var currentXPos = startXPos + phase;
                                        var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                        interYPos = basePoint - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinRange (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
                                    }

                                    cr.MoveTo (graphLeft, interYPos);
                                    for (var phase = rightPart; phase <= period; ++phase) {
                                        var currentXPos = interXPos + phase;
                                        var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                        interYPos = basePoint - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinRange (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
                                    }
                                }
                                break;
                            }
                        case LightingStateType.On:
                            endYPos = startYPos;
                            if (state.startTime.Before (state.endTime)) {
                                cr.LineTo (endXPos, startYPos);

                                if ((timeXPos > startXPos) && (timeXPos < endXPos)) {
                                    timeYPos = startYPos;
                                }
                            } else {
                                cr.LineTo (graphRight, startYPos);

                                cr.MoveTo (graphLeft, startYPos);
                                cr.LineTo (endXPos, startYPos);

                                if (((timeXPos > startXPos) && (timeXPos < graphLeft)) || (timeXPos < endXPos)) {
                                    timeYPos = startYPos;
                                }
                            }
                            break;
                        }
                        TouchColor.SetSource (cr, "secb");
                        cr.Stroke ();

                        cr.MoveTo (endXPos, endYPos);
                        cr.Arc (endXPos, endYPos, 3, 0, 2 * Math.PI);
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

                            // State start adjustment button
                            TouchGlobal.DrawRoundedRectangle (cr, startButtonX, graphBottom - 10, 80, 20, 8);
                            var color = new TouchColor ("pri");
                            color.ModifyColor (0.5);
                            color.SetSource (cr);
                            cr.StrokePreserve ();

                            color = new TouchColor ("pri");
                            if (startButtonClicked) {
                                color.ModifyColor (0.75);
                            }

                            var highlightColor = new TouchColor (color);
                            highlightColor.ModifyColor (1.25);
                            using (var grad = new LinearGradient (startButtonX, graphBottom - 10, startButtonX, graphBottom + 10)) {
                                grad.AddColorStop (0, highlightColor.ToCairoColor ());
                                grad.AddColorStop (0.2, color.ToCairoColor ());
                                cr.SetSource (grad);
                                cr.Fill ();
                            }

                            // State end adjustment button
                            TouchGlobal.DrawRoundedRectangle (cr, endButtonX, graphBottom - 10, 80, 20, 8);
                            color = new TouchColor ("pri");
                            color.ModifyColor (0.5);
                            color.SetSource (cr);
                            cr.StrokePreserve ();

                            color = new TouchColor ("pri");
                            if (startButtonClicked) {
                                color.ModifyColor (0.75);
                            }

                            highlightColor = new TouchColor (color);
                            highlightColor.ModifyColor (1.25);
                            using (var grad = new LinearGradient (endButtonX, graphBottom - 10, endButtonX, graphBottom + 10)) {
                                grad.AddColorStop (0, highlightColor.ToCairoColor ());
                                grad.AddColorStop (0.2, color.ToCairoColor ());
                                cr.SetSource (grad);
                                cr.Fill ();
                            }

                            cr.Rectangle (startButtonX, graphBottom + 15, 80, 25);
                            TouchColor.SetSource (cr, "grey4");
                            cr.FillPreserve ();
                            TouchColor.SetSource (cr, "black");
                            cr.Stroke ();

                            text = new TouchText (state.startTime.ToShortTimeString ());
                            text.alignment = TouchAlignment.Center;
                            text.font.color = "black";
                            text.Render (this, startButtonX.ToInt (), graphBottom + 18, 80);

                            cr.Rectangle (endButtonX, graphBottom + 15, 80, 25);
                            TouchColor.SetSource (cr, "grey4");
                            cr.FillPreserve ();
                            TouchColor.SetSource (cr, "black");
                            cr.Stroke ();

                            text = new TouchText (state.endTime.ToShortTimeString ());
                            text.alignment = TouchAlignment.Center;
                            text.font.color = "black";
                            text.Render (this, endButtonX.ToInt (), graphBottom + 18, 80);
                        }
                    }
                }

                cr.MoveTo (timeXPos, timeYPos);
                cr.Arc (timeXPos, timeYPos, 6, 0, 2 * Math.PI);
                TouchColor.SetSource (cr, "seca");
                cr.ClosePath ();
                cr.Fill ();
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
                }
            }

            if (selectedState != -1) {
                if ((x > stateInfos[selectedState].startButtonXPos) &&
                    (x < stateInfos[selectedState].startButtonXPos + 80) &&
                    (y > graphBottomRelative + 15) &&
                    (y < graphBottomRelative + 40)) 
                {
                    var parent = Toplevel as Window;
                    var t = new TouchNumberInput (true, parent);
                    t.Title = "Start Time";
                    t.TextSetEvent += (o, a) => {
                        try {
                            var newStartTime = Time.Parse (a.text);
                            var oldEndTime = stateInfos[selectedState].lightingState.endTime;
                            var difference = Math.Abs (newStartTime.ToTimeSpan ().Subtract (oldEndTime.ToTimeSpan ()).TotalMinutes);
                            if (difference < 1) {
                                a.keepText = false;
                                MessageBox.Show ("Invalid start time. Too close to end time");
                            }

                            if (a.keepText) {
                                stateInfos[selectedState].lightingState.startTime = newStartTime;
                                stateInfos[selectedState].previous.lightingState.endTime = newStartTime;
                            }
                        } catch {
                            a.keepText = false;
                        }
                    };

                    t.Run ();
                    t.Destroy ();
                } else if ((x > stateInfos[selectedState].endButtonXPos) &&
                           (x < stateInfos[selectedState].endButtonXPos + 80) &&
                           (y > graphBottomRelative + 15) &&
                           (y < graphBottomRelative + 40)) 
                {
                    var parent = Toplevel as Window;
                    var t = new TouchNumberInput (true, parent);
                    t.Title = "End Time";
                    t.TextSetEvent += (o, a) => {
                        try {
                            stateInfos[selectedState].lightingState.endTime = Time.Parse (a.text);
                            stateInfos[selectedState].next.lightingState.startTime = stateInfos[selectedState].lightingState.endTime;
                        } catch {
                            a.keepText = false;
                        }
                    };

                    t.Run ();
                    t.Destroy ();
                }
            }

            clicked = false;
            startButtonClicked = false;
            endButtonClicked = false;

            QueueDraw ();
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                GetPointer (out int x, out int y);

                var yDelta = clickY - y;
                var xDelta = x- clickX;

                if (startButtonClicked) {
                    var stateInfo = stateInfos[selectedState];

                    var currentState = stateInfo.lightingState;
                    var currentStateStartMinutes = currentState.startTime.totalMinutes;
                    var currentStateEndMinutes = currentState.endTime.totalMinutes;

                    var previousState = stateInfo.previous.lightingState;
                    var previousStateStartMinutes = previousState.startTime.totalMinutes;
                    var previousStateEndMinutes = previousState.endTime.totalMinutes;

                    var newStartMinutes = xDelta * minutesPerPixel + currentStateStartMinutes;
                    bool crossedMidnight = false;
                    if (newStartMinutes < 0) {
                        newStartMinutes = 1439;
                        crossedMidnight = true;
                    } else if (newStartMinutes > 1439) {
                        newStartMinutes = 0;
                        crossedMidnight = true;
                    }

                    if (!crossedMidnight) {
                        double currentTotalMinutes;
                        if (currentStateStartMinutes < currentStateEndMinutes) {
                            currentTotalMinutes = currentStateEndMinutes - newStartMinutes;
                        } else {
                            currentTotalMinutes = (1440 - newStartMinutes) + currentStateEndMinutes;
                        }

                        if (currentTotalMinutes < 1) {
                            newStartMinutes = currentStateStartMinutes;
                        }

                        double previousStateLength;
                        if (previousStateStartMinutes < previousStateEndMinutes) {
                            previousStateLength = newStartMinutes - previousStateStartMinutes;
                        } else {
                            previousStateLength = (1440 - previousStateStartMinutes) + newStartMinutes;
                        }

                        if (previousStateLength < 1) {
                            newStartMinutes = currentStateStartMinutes;
                        }
                    } else {
                        var currentDifference = Math.Abs (currentStateEndMinutes - newStartMinutes);
                        var previousDifference = Math.Abs (previousStateStartMinutes - newStartMinutes);

                        if ((currentDifference < 1) || (previousDifference < 1)) {
                            if (newStartMinutes < 1) {
                                newStartMinutes = 1439;
                            } else if (newStartMinutes > 1438) {
                                newStartMinutes = 0;
                            }
                        }
                    }

                    var startTime = new Time (new TimeSpan (0, newStartMinutes.ToInt (), 0));
                    currentState.startTime = startTime;
                    previousState.endTime = startTime;
                } else if (endButtonClicked) {
                    var stateInfo = stateInfos[selectedState];

                    var currentState = stateInfo.lightingState;
                    var currentStateStartMinutes = currentState.startTime.totalMinutes;
                    var currentStateEndMinutes = currentState.endTime.totalMinutes;

                    var nextState = stateInfo.next.lightingState;
                    var nextStateStartMinutes = nextState.startTime.totalMinutes;
                    var nextStateEndMinutes = nextState.endTime.totalMinutes;

                    var newEndMinutes = xDelta * minutesPerPixel + currentStateEndMinutes;
                    bool crossedMidnight = false;
                    if (newEndMinutes < 0) {
                        newEndMinutes = 1439;
                        crossedMidnight = true;
                    } else if (newEndMinutes > 1439) {
                        newEndMinutes = 0;
                        crossedMidnight = true;
                    }

                    if (!crossedMidnight) {
                        double currentTotalMinutes;
                        if (currentStateStartMinutes < currentStateEndMinutes) {
                            currentTotalMinutes = newEndMinutes - currentStateStartMinutes;
                        } else {
                            currentTotalMinutes = (1440 - currentStateStartMinutes) + newEndMinutes;
                        }

                        if (currentTotalMinutes < 1) {
                            newEndMinutes = currentStateEndMinutes;
                        }

                        double nextStateLength;
                        if (nextStateStartMinutes < nextStateEndMinutes) {
                            nextStateLength = nextStateEndMinutes - newEndMinutes;
                        } else {
                            nextStateLength = (1440 - newEndMinutes) + nextStateEndMinutes;
                        }

                        if (nextStateLength < 1) {
                            newEndMinutes = currentStateEndMinutes;
                        }
                    } else {
                        var currentDifference = Math.Abs (newEndMinutes - currentStateStartMinutes);
                        var previousDifference = Math.Abs (nextStateEndMinutes - newEndMinutes);

                        if ((currentDifference < 1) || (previousDifference < 1)) {
                            if (newEndMinutes < 1) {
                                newEndMinutes = 1439;
                            } else if (newEndMinutes > 1438) {
                                newEndMinutes = 0;
                            }
                        }
                    }

                    var endTime = new Time (new TimeSpan (0, newEndMinutes.ToInt (), 0));
                    currentState.endTime = endTime;
                    nextState.startTime = endTime;
                }

                clickX = x;

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
