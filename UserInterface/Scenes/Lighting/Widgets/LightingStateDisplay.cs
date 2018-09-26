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
        bool clicked, startButtonClicked, endButtonClicked, deleteButtonClicked;
        uint clickTimer;
        int clickX, clickY;
        int selectedState;

        const int graphLeftEdgeWidth = 80;
        const int graphTopEdgeWidth = 60;
        const int graphBottomEdgeWidth = 40;
        int graphLeftRelative, graphRightRelative, graphTopRelative, graphBottomRelative;
        double minutesPerPixel, dimmingPerPixel;

        List<StateInfo> stateInfos;
        bool dimmingFixture;

        public LightingStateDisplay () {
            Visible = true;
            VisibleWindow = false;
            SetSizeRequest (540, 360);

            graphLeftRelative = graphLeftEdgeWidth;
            graphTopRelative = graphTopEdgeWidth;
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
            var graphLeft = left + graphLeftEdgeWidth;
            var right = Allocation.Right;
            var graphRight = right;
            var width = Allocation.Width;
            graphRightRelative = width;
            var midX = (graphRightRelative - graphLeftEdgeWidth) / 2 + graphLeft;

            var top = Allocation.Top;
            var graphTop = top + graphTopEdgeWidth;
            var bottom = Allocation.Bottom;
            var graphBottom = bottom - graphBottomEdgeWidth;
            var height = Allocation.Height;
            graphBottomRelative = height - graphBottomEdgeWidth;
            var midY = (graphBottomRelative - graphTopEdgeWidth) / 2 + graphTop;

            minutesPerPixel = 1440d / (graphRightRelative - graphLeftRelative);
            dimmingPerPixel = 100d / (graphBottomRelative - graphTopRelative);

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
                TouchText text;
                if (selectedState == -1 || !dimmingFixture) {
                    var textWidth = graphLeftEdgeWidth - 7;
                    text = new TouchText ("100%");
                    text.alignment = TouchAlignment.Right;
                    text.Render (this, left, graphTop - 12, textWidth);

                    text = new TouchText ("50%");
                    text.alignment = TouchAlignment.Right;
                    text.Render (this, left, midY - 12, textWidth);

                    text = new TouchText ("0%");
                    text.alignment = TouchAlignment.Right;
                    text.Render (this, left, graphBottom - 13, textWidth);
                }

                var timeXPos = Time.TimeNow.totalMinutes.Map (0, 1440, graphLeft, graphRight);
                double timeYPos = graphBottom;

                double startYPosSelected = 0, endYPosSelected = 0, startXPosSelected = 0, endXPosSelected = 0;
                double rightPartSelected = 0, periodSelected = 0, deltaSelected = 0, interXPosSelected = 0;

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

                        if (selectedState != i) {
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

                        if (selectedState != i) {
                            cr.MoveTo (endXPos, endYPos);
                            cr.Arc (endXPos, endYPos, 3, 0, 2 * Math.PI);
                            cr.ClosePath ();
                            cr.Fill ();
                        }
             
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
                            startYPosSelected = startYPos;
                            endYPosSelected = endYPos;
                            startXPosSelected = startXPos;
                            endXPosSelected = endXPos;
                            rightPartSelected = rightPart;
                            periodSelected = period;
                            deltaSelected = delta;
                            interXPosSelected = interXPos;
                        }
                    }
                }

                if (selectedState != -1) {
                    var stateInfo = stateInfos[selectedState];
                    var state = stateInfo.lightingState;

                    cr.MoveTo (startXPosSelected, graphBottom);
                    cr.LineTo (startXPosSelected, startYPosSelected);
                    switch (state.type) {
                    case LightingStateType.LinearRamp: {
                            if (state.startTime.Before (state.endTime)) {
                                cr.LineTo (endXPosSelected, endYPosSelected);
                            } else {
                                var rightRatio = rightPartSelected / periodSelected;
                                var rightYPos = startYPosSelected + (rightRatio * deltaSelected);

                                cr.LineTo (graphRight, rightYPos);
                                cr.LineTo (graphRight, graphBottom);
                                cr.ClosePath ();

                                cr.MoveTo (graphLeft, graphBottom);
                                cr.LineTo (graphLeft, rightYPos);
                                cr.LineTo (endXPosSelected, endYPosSelected);
                            }
                            break;
                        }
                    case LightingStateType.ParabolaRamp: {
                            double interYPos = graphBottom;

                            if (state.startTime.Before (state.endTime)) {
                                for (var phase = 1; phase <= periodSelected; ++phase) {
                                    var radian = (phase / periodSelected).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                    interYPos = startYPosSelected - deltaSelected * Math.Sin (radian);
                                    cr.LineTo (startXPosSelected + phase, interYPos);
                                }
                            } else {
                                for (var phase = 1; phase <= rightPartSelected; ++phase) {
                                    var radian = (phase / periodSelected).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                    interYPos = startYPosSelected - deltaSelected * Math.Sin (radian);
                                    cr.LineTo (startXPosSelected + phase, interYPos);
                                }
                                cr.LineTo (graphRight, graphBottom);
                                cr.ClosePath ();

                                cr.MoveTo (graphLeft, graphBottom);
                                cr.LineTo (graphLeft, interYPos);
                                for (var phase = rightPartSelected; phase <= periodSelected; ++phase) {
                                    var radian = (phase / periodSelected).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
                                    interYPos = startYPosSelected - deltaSelected * Math.Sin (radian);
                                    cr.LineTo (interXPosSelected + phase, interYPos);
                                }
                            }
                            endYPosSelected = (float)interYPos;
                            break;
                        }
                    case LightingStateType.HalfParabolaRamp: {
                            double mapFrom1, mapFrom2, basePoint;
                            if (startYPosSelected <= endYPosSelected) {
                                mapFrom1 = 1d;
                                mapFrom2 = 0d;
                                basePoint = endYPosSelected;
                            } else {
                                mapFrom1 = 0d;
                                mapFrom2 = 1d;
                                basePoint = startYPosSelected;
                            }

                            double interYPos = graphBottom;
                            if (state.startTime.Before (state.endTime)) {
                                for (var phase = 1; phase <= periodSelected; ++phase) {
                                    var radian = (phase / periodSelected).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                    interYPos = basePoint - deltaSelected * Math.Sin (radian);
                                    cr.LineTo (startXPosSelected + phase, interYPos);
                                }
                                cr.LineTo (endXPosSelected, endYPosSelected);
                            } else {
                                for (var phase = 1; phase <= rightPartSelected; ++phase) {
                                    var radian = (phase / periodSelected).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                    interYPos = basePoint - deltaSelected * Math.Sin (radian);
                                    cr.LineTo (startXPosSelected + phase, interYPos);
                                }
                                cr.LineTo (graphRight, graphBottom);
                                cr.ClosePath ();

                                cr.MoveTo (graphLeft, graphBottom);
                                cr.LineTo (graphLeft, interYPos);
                                for (var phase = rightPartSelected; phase <= periodSelected; ++phase) {
                                    var radian = (phase / periodSelected).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
                                    interYPos = basePoint - deltaSelected * Math.Sin (radian);
                                    cr.LineTo (interXPosSelected + phase, interYPos);
                                }
                            }

                            break;
                        }
                    case LightingStateType.On:
                        endYPosSelected = startYPosSelected;
                        if (state.startTime.Before (state.endTime)) {
                            cr.LineTo (endXPosSelected, startYPosSelected);
                        } else {
                            cr.LineTo (graphRight, startYPosSelected);
                            cr.LineTo (graphRight, graphBottom);
                            cr.ClosePath ();

                            cr.MoveTo (graphLeft, graphBottom);
                            cr.LineTo (graphLeft, startYPosSelected);
                            cr.LineTo (endXPosSelected, startYPosSelected);
                        }
                        break;
                    }
                    cr.LineTo (endXPosSelected, graphBottom);
                    cr.ClosePath ();
                    TouchColor.SetSource (cr, "grey2", 0.5);
                    cr.Fill ();

                    double startButtonX, endButtonX, startButtonY, endButtonY;
                    if (periodSelected < 32) {
                        startButtonX = startXPosSelected - 16 + periodSelected / 2;
                        endButtonX = endXPosSelected + 16 - periodSelected / 2;
                    } else {
                        startButtonX = startXPosSelected;
                        endButtonX = endXPosSelected;
                    }

                    startButtonY = startYPosSelected;
                    endButtonY = endYPosSelected;

                    stateInfo.startButtonXPos = startButtonX - left;
                    stateInfo.startButtonYPos = startButtonY - top;
                    stateInfo.endButtonXPos = endButtonX - left;
                    stateInfo.endButtonYPos = endButtonY - top;

                    // State start adjustment button
                    cr.MoveTo (startButtonX, startButtonY);
                    cr.Arc (startButtonX, startButtonY, 15, 0, 2 * Math.PI);
                    cr.ClosePath ();
                    
                    var color = new TouchColor ("secb");
                    if (startButtonClicked) {
                        color.ModifyColor (0.75);
                    }
                    color.SetSource (cr);
                    cr.Fill ();

                    // State end adjustment button
                    cr.MoveTo (endButtonX, endButtonY);
                    cr.Arc (endButtonX, endButtonY, 15, 0, 2 * Math.PI);
                    cr.ClosePath ();

                    color = new TouchColor ("secb");
                    if (endButtonClicked) {
                        color.ModifyColor (0.75);
                    }
                    color.SetSource (cr);
                    cr.Fill ();

                    double startTimeTextX, endTimeTextX;
                    if (periodSelected < 84) {
                        startTimeTextX = startXPosSelected - 80 + (periodSelected - 2) / 2;
                        endTimeTextX = endXPosSelected - (periodSelected - 2) / 2;
                    } else {
                        startTimeTextX = startXPosSelected - 40;
                        endTimeTextX = endXPosSelected - 40;
                    }

                    stateInfo.startTimeTextXPos = startTimeTextX - left;
                    stateInfo.endTimeTextXPos = endTimeTextX - left;

                    // Start time textbox
                    cr.Rectangle (startTimeTextX, graphBottom + 10, 80, 25);
                    TouchColor.SetSource (cr, "grey4");
                    cr.FillPreserve ();
                    TouchColor.SetSource (cr, "black");
                    cr.Stroke ();

                    text = new TouchText (state.startTime.ToShortTimeString ());
                    text.alignment = TouchAlignment.Center;
                    text.font.color = "black";
                    text.Render (this, startTimeTextX.ToInt (), graphBottom + 13, 80);

                    // End time textbox
                    cr.Rectangle (endTimeTextX, graphBottom + 10, 80, 25);
                    TouchColor.SetSource (cr, "grey4");
                    cr.FillPreserve ();
                    TouchColor.SetSource (cr, "black");
                    cr.Stroke ();

                    text = new TouchText (state.endTime.ToShortTimeString ());
                    text.alignment = TouchAlignment.Center;
                    text.font.color = "black";
                    text.Render (this, endTimeTextX.ToInt (), graphBottom + 13, 80);

                    double startDimmingTextY, endDimmingTextY;
                    if (deltaSelected < 29) {
                        startDimmingTextY = startYPosSelected - 12.5 + (deltaSelected - 2) / 2;
                        endDimmingTextY = endYPosSelected - (deltaSelected - 2) / 2;
                    } else {
                        startDimmingTextY = startYPosSelected - 12.5;
                        endDimmingTextY = endYPosSelected - 12.5;
                    }

                    stateInfo.startTimeTextXPos = startTimeTextX - left;
                    stateInfo.endTimeTextXPos = endTimeTextX - left;

                    // Delete button
                    double deleteButtonX, deleteButtonY;
                    if (state.startTime.Before (state.endTime)) {
                        deleteButtonX = periodSelected / 2 + startXPosSelected;
                    } else {
                        if (rightPartSelected > (periodSelected / 2)) {
                            deleteButtonX = rightPartSelected / 2 + startXPosSelected;
                        } else {
                            deleteButtonX = (periodSelected - rightPartSelected) / 2 + graphLeft;
                        }
                    }

                    if (startYPosSelected > endYPosSelected) {
                        deleteButtonY = (graphBottom - endYPosSelected) / 2 + endYPosSelected;
                    } else {
                        deleteButtonY = (graphBottom - startYPosSelected) / 2 + startYPosSelected;
                    }

                    stateInfo.deleteButtonXPos = deleteButtonX - left;
                    stateInfo.deleteButtonYPos = deleteButtonY - top;

                    cr.MoveTo (deleteButtonX, deleteButtonY);
                    cr.Arc (deleteButtonX, deleteButtonY, 15, 0, 2 * Math.PI);
                    TouchColor.SetSource (cr, "compl");
                    cr.Fill ();

                    cr.MoveTo (deleteButtonX - 6, deleteButtonY - 6);
                    cr.LineTo (deleteButtonX + 6, deleteButtonY + 6);
                    cr.MoveTo (deleteButtonX + 6, deleteButtonY - 6);
                    cr.LineTo (deleteButtonX - 6, deleteButtonY + 6);
                    TouchColor.SetSource (cr, "grey2");
                    cr.Stroke ();
                }

                cr.MoveTo (timeXPos, timeYPos);
                cr.Arc (timeXPos, timeYPos, 6, 0, 2 * Math.PI);
                TouchColor.SetSource (cr, "seca");
                cr.ClosePath ();
                cr.Fill ();
            }
        }

        public void SetStates (LightingState[] lightingStates, bool dimmingFixture) {
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
            } else if (stateInfos.Count == 1) {
                stateInfos[0].next = stateInfos[0];
                stateInfos[1].previous = stateInfos[0];
            }

            this.dimmingFixture = dimmingFixture;
        }

        protected void OnButtonPress (object sender, ButtonPressEventArgs args) {
            clicked = true;
            clickX = args.Event.X.ToInt ();
            clickY = args.Event.Y.ToInt ();

            if (selectedState != -1) {
                var stateInfo = stateInfos[selectedState];

                if ((clickX > stateInfo.startButtonXPos - 12) && (clickX < stateInfo.startButtonXPos + 12) && 
                    (clickY > stateInfo.startButtonYPos - 12) && (clickY < stateInfo.startButtonYPos + 12)) {
                    startButtonClicked = true;
                    clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
                }

                if ((clickX > stateInfo.endButtonXPos - 12) && (clickX < stateInfo.endButtonXPos + 12) &&
                    (clickY > stateInfo.endButtonYPos - 12) && (clickY < stateInfo.endButtonYPos + 12)) {
                    endButtonClicked = true;
                    clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
                }

                if ((clickX > stateInfo.deleteButtonXPos - 12) && (clickX < stateInfo.deleteButtonXPos + 12) &&
                    (clickY > stateInfo.deleteButtonYPos - 12) && (clickY < stateInfo.deleteButtonYPos + 12)) {
                    deleteButtonClicked = true;
                }
            }
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var x = args.Event.X;
            var y = args.Event.Y;

            var clickHappenedOnEntity = startButtonClicked | endButtonClicked | deleteButtonClicked;
            if (!clickHappenedOnEntity) {
                if (selectedState != -1) {
                    var stateInfo = stateInfos[selectedState];

                    if ((x > stateInfo.startTimeTextXPos) && (x < stateInfo.startTimeTextXPos + 80) &&
                    (y > graphBottomRelative + 10) && (y < graphBottomRelative + 35)) {
                        var parent = Toplevel as Window;
                        var t = new TouchNumberInput (true, parent);
                        t.Title = "Start Time";
                        t.TextSetEvent += (o, a) => {
                            try {
                                if (!ValidateAndSetStartTime (Time.Parse (a.text))) {
                                    a.keepText = false;
                                    MessageBox.Show ("Invalid Start Time");
                                }
                            } catch {
                                a.keepText = false;
                            }
                        };

                        t.Run ();
                        t.Destroy ();
                    } else if ((x > stateInfo.endTimeTextXPos) && (x < stateInfo.endTimeTextXPos + 80) &&
                        (y > graphBottomRelative + 10) && (y < graphBottomRelative + 35)) {
                        var parent = Toplevel as Window;
                        var t = new TouchNumberInput (true, parent);
                        t.Title = "End Time";
                        t.TextSetEvent += (o, a) => {
                            try {
                                if (!ValidateAndSetEndTime (Time.Parse (a.text))) {
                                    a.keepText = false;
                                    MessageBox.Show ("Invalid End Time");
                                }
                            } catch {
                                a.keepText = false;
                            }
                        };

                        t.Run ();
                        t.Destroy ();
                    }
                }

                // The release happened on the graph
                if ((x > graphLeftRelative) && (x < graphRightRelative) &&
                    (y > graphTopRelative) && (y < graphBottomRelative)) {
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
                                ((x > graphLeftEdgeWidth) && (x < stateInfo.endStateXPos))) {
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


            } else {
                var stateInfo = stateInfos[selectedState];

                if ((x > stateInfo.deleteButtonXPos - 12) && (x < stateInfo.deleteButtonXPos + 12) &&
                    (y > stateInfo.deleteButtonYPos - 12) && (y < stateInfo.deleteButtonYPos + 12)) {
                    var parent = Toplevel as Window;
                    var ms = new TouchDialog ("Are you sure you want to delete the state", parent);
                    ms.Response += (o, a) => {
                        if (a.ResponseId == ResponseType.Yes) {
                            RemoveState (selectedState);
                            selectedState = -1;
                        }
                    };

                    ms.Run ();
                    ms.Destroy ();
                }
            }

            clicked = false;
            startButtonClicked = false;
            endButtonClicked = false;
            deleteButtonClicked = false;

            QueueDraw ();
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                GetPointer (out int x, out int y);

                var yDelta = clickY - y;
                var xDelta = x- clickX;

                if (startButtonClicked) {
                    var newStartMinutes = xDelta * minutesPerPixel + stateInfos[selectedState].lightingState.startTime.totalMinutes;
                    if (newStartMinutes < 0) {
                        newStartMinutes = 1439;
                    } else if (newStartMinutes > 1439) {
                        newStartMinutes = 0;
                    }
                    var newStartTime = new Time (new TimeSpan (0, newStartMinutes.ToInt (), 0));
                    ValidateAndSetStartTime (newStartTime);

                    if (dimmingFixture) {
                        var newDimmingLevel = yDelta * dimmingPerPixel + stateInfos[selectedState].lightingState.startingDimmingLevel;
                        SetStartDimmingLevel (newDimmingLevel);
                    }
                } else if (endButtonClicked) {
                    var newEndMinutes = xDelta * minutesPerPixel + stateInfos[selectedState].lightingState.endTime.totalMinutes;
                    if (newEndMinutes < 0) {
                        newEndMinutes = 1439;
                    } else if (newEndMinutes > 1439) {
                        newEndMinutes = 0;
                    }
                    var newEndTime = new Time (new TimeSpan (0, newEndMinutes.ToInt (), 0));
                    ValidateAndSetEndTime (newEndTime);

                    if (dimmingFixture) {
                        var newDimmingLevel = yDelta * dimmingPerPixel + stateInfos[selectedState].lightingState.endingDimmingLevel;
                        SetEndDimmingLevel (newDimmingLevel);
                    }
                }

                clickX = x;
                clickY = y;

                QueueDraw ();
            }

            return clicked;
        }

        bool ValidateAndSetStartTime (Time newStartTime) {
            var timeOkay = true;

            var oldStartTime = stateInfos[selectedState].lightingState.startTime;
            var endTime = stateInfos[selectedState].lightingState.endTime;
            var previousStartTime = stateInfos[selectedState].previous.lightingState.startTime;

            var difference = Math.Abs (newStartTime.ToTimeSpan ().Subtract (endTime.ToTimeSpan ()).TotalMinutes);
            timeOkay = (difference >= 1 && difference <= 1439);

            difference = Math.Abs (newStartTime.ToTimeSpan ().Subtract (previousStartTime.ToTimeSpan ()).TotalMinutes);
            timeOkay &= (difference >= 1 && difference <= 1439);

            if (timeOkay) {
                stateInfos[selectedState].lightingState.startTime = newStartTime;
                stateInfos[selectedState].previous.lightingState.endTime = newStartTime;

                var totalLength = 0d;
                foreach (var stateInfo in stateInfos) {
                    totalLength += stateInfo.lightingState.lengthInMinutes;
                }

                timeOkay = totalLength <= 1440;

                if (!timeOkay) {
                    stateInfos[selectedState].lightingState.startTime = oldStartTime;
                    stateInfos[selectedState].previous.lightingState.endTime = oldStartTime;
                }
            }

            return timeOkay;
        }

        void SetStartDimmingLevel (double newStartDimmingLevel) {
            var stateInfo = stateInfos[selectedState];
            var state = stateInfo.lightingState;
            var type = state.type;
            var newDimmingLevel = (float)newStartDimmingLevel.Constrain (0, 100);

            if (type == LightingStateType.On) {
                state.startingDimmingLevel = newDimmingLevel;
                state.endingDimmingLevel = newDimmingLevel;
            } else {
                state.startingDimmingLevel = newDimmingLevel;
            }
        }

        bool ValidateAndSetEndTime (Time newEndTime) {
            var timeOkay = true;

            var oldEndTime = stateInfos[selectedState].lightingState.endTime;
            var startTime = stateInfos[selectedState].lightingState.startTime;
            var nextEndTime = stateInfos[selectedState].next.lightingState.endTime;

            var difference = Math.Abs (newEndTime.ToTimeSpan ().Subtract (startTime.ToTimeSpan ()).TotalMinutes);
            timeOkay = (difference >= 1 && difference <= 1439);

            difference = Math.Abs (newEndTime.ToTimeSpan ().Subtract (nextEndTime.ToTimeSpan ()).TotalMinutes);
            timeOkay &= (difference >= 1 && difference <= 1439);

            if (timeOkay) {
                stateInfos[selectedState].lightingState.endTime = newEndTime;
                stateInfos[selectedState].next.lightingState.startTime = newEndTime;

                var totalLength = 0d;
                foreach (var stateInfo in stateInfos) {
                    totalLength += stateInfo.lightingState.lengthInMinutes;
                }

                timeOkay = totalLength <= 1440;

                if (!timeOkay) {
                    stateInfos[selectedState].lightingState.endTime = oldEndTime;
                    stateInfos[selectedState].next.lightingState.startTime = oldEndTime;
                }
            }

            return timeOkay;
        }

        void SetEndDimmingLevel (double newStartDimmingLevel) {
            var stateInfo = stateInfos[selectedState];
            var state = stateInfo.lightingState;
            var type = state.type;
            var newDimmingLevel = (float)newStartDimmingLevel.Constrain (0, 100);

            if (type == LightingStateType.On) {
                state.startingDimmingLevel = newDimmingLevel;
                state.endingDimmingLevel = newDimmingLevel;
            } else {
                state.endingDimmingLevel = newDimmingLevel;
            }
        }

        void RemoveState (int state) {
            if (stateInfos.Count > 0) {
                var stateInfo = stateInfos[state];
                stateInfos.RemoveAt (state);
                stateInfo.previous.next = stateInfo.next;
                stateInfo.next.previous = stateInfo.previous;

                if ((stateInfo.previous.lightingState.type != LightingStateType.Off) &&
                    (stateInfo.next.lightingState.type != LightingStateType.Off)) {
                    var period = stateInfo.lightingState.lengthInMinutes;
                    var startMinutes = stateInfo.lightingState.startTime.totalMinutes;
                    var midMinutes = period / 2 + startMinutes;
                    if (startMinutes > 1439) {
                        startMinutes -= 1439;
                    }
                    var midTime = new Time (new TimeSpan (0, midMinutes.ToInt (), 0));
                    stateInfo.previous.lightingState.endTime = midTime;
                    stateInfo.next.lightingState.startTime = midTime;
                } else if ((stateInfo.previous.lightingState.type == LightingStateType.Off) &&
                    (stateInfo.next.lightingState.type != LightingStateType.Off)) {
                    stateInfo.next.lightingState.startTime = stateInfo.lightingState.startTime;
                } else if ((stateInfo.previous.lightingState.type != LightingStateType.Off) &&
                    (stateInfo.next.lightingState.type == LightingStateType.Off)) {
                    stateInfo.previous.lightingState.endTime = stateInfo.lightingState.endTime;
                } else {
                    var nextStateIndex = stateInfos.IndexOf (stateInfo.next);
                    RemoveState (nextStateIndex);
                }
            }
        }

        class StateInfo
        {
            public StateInfo previous;
            public StateInfo next;
            public LightingState lightingState;
            public double startStateXPos, endStateXPos;
            public double startButtonXPos, startButtonYPos;
            public double endButtonXPos, endButtonYPos;
            public double startTimeTextXPos, endTimeTextXPos;
            public double startDimmingTextXPos, endDimmingTextXPos;
            public double deleteButtonXPos, deleteButtonYPos;
        }
    }
}
