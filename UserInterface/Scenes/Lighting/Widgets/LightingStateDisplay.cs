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
    public delegate void LightingStateInfoChangedHandler (object sender, bool hasStateInfoChanged);
    public delegate void LightingStateSelectionChangedHandler (object sender, LightingStateSelectionArgs args);

    public class LightingStateSelectionArgs : EventArgs
    {
        public LightingState lightingState;
        public bool stateSelected;

        public LightingStateSelectionArgs (LightingState lightingState, bool stateSelected) {
            this.lightingState = lightingState;
            this.stateSelected = stateSelected;
        }
    }

    public class LightingStateDisplay : EventBox
    {
        #region Properties

        public event LightingStateSelectionChangedHandler LightingStateSelectionChanged;
        public event LightingStateInfoChangedHandler LightingStateInfoChanged;

        const int graphLeftEdgeWidth = 80;
        const int graphTopEdgeWidth = 50;
        const int graphBottomEdgeWidth = 90;

        int _selectedState;
        public int selectedState {
            get {
                return _selectedState;
            }
            set {
                _selectedState = value;
                LightingStateSelectionArgs args;
                if (_selectedState == -1) {
                    args = new LightingStateSelectionArgs (null, false);
                } else {
                    args = new LightingStateSelectionArgs (stateInfos[_selectedState].lightingState, true);
                }
                LightingStateSelectionChanged?.Invoke (this, args);
            }
        }

        List<StateInfo> stateInfos;
        public LightingState[] lightingStates {
            get {
                var states = new List<LightingState> ();
                foreach (var stateInfo in stateInfos) {
                    states.Add (new LightingState (stateInfo.lightingState));
                }
                return states.ToArray ();
            }
        }
        List<LightingState> savedLightingStates;

        bool _hasStateInfoChanged;
        public bool hasStateInfoChanged {
            get {
                return _hasStateInfoChanged;
            }
            set {
                if (value != _hasStateInfoChanged) {
                    _hasStateInfoChanged = value;
                    LightingStateInfoChanged?.Invoke (this, _hasStateInfoChanged);
                }
            }
        }

        public bool adjustDimmingTogether;

        bool clicked, startButtonClicked, endButtonClicked, linearClicked, halfParabolaClicked, fullParabolaClicked, onClicked, offClicked, entityClicked,
            movedOutOfXRange, movedOutOfYRange, dimmingFixture;
        uint clickTimer;
        int graphLeftRelative, graphRightRelative, graphTopRelative, graphBottomRelative, clickX, clickY, addedStateIndex;
        double minutesPerPixel, dimmingPerPixel;
        LightingStateType newStateType;

        #endregion // Properties

        public LightingStateDisplay () {
            Visible = true;
            VisibleWindow = false;

            graphLeftRelative = graphLeftEdgeWidth;
            graphTopRelative = graphTopEdgeWidth;
            selectedState = -1;
            addedStateIndex = -1;
            savedLightingStates = new List<LightingState> ();
            stateInfos = new List<StateInfo> ();
            adjustDimmingTogether = true;

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

                #region General State Render

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
                            cr.Arc (startXPos, startYPos, 3, 0, 2 * Math.PI);
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
                                        var radian = (phase / period).Map (0, 1, 0, Math.PI).Constrain (0, Math.PI);
                                        interYPos = startYPos - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinSetpoint (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
                                    }
                                } else {
                                    for (var phase = 1; phase <= rightPart; ++phase) {
                                        var currentXPos = startXPos + phase;
                                        var radian = (phase / period).Map (0, 1, 0, Math.PI).Constrain (0, Math.PI);
                                        interYPos = startYPos - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinSetpoint (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
                                    }

                                    cr.MoveTo (graphLeft, interYPos);
                                    for (var phase = rightPart; phase <= period; ++phase) {
                                        var currentXPos = interXPos + phase;
                                        var radian = (phase / period).Map (0, 1, 0, Math.PI).Constrain (0, Math.PI);
                                        interYPos = startYPos - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinSetpoint (timeXPos, 1)) {
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
                                        var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, Math.PI / 2).Constrain (0, Math.PI / 2);
                                        interYPos = basePoint - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinSetpoint (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
                                    }
                                    cr.LineTo (endXPos, endYPos);
                                } else {
                                    for (var phase = 1; phase <= rightPart; ++phase) {
                                        var currentXPos = startXPos + phase;
                                        var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, Math.PI / 2).Constrain (0, Math.PI / 2);
                                        interYPos = basePoint - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinSetpoint (timeXPos, 1)) {
                                            timeYPos = interYPos;
                                        }
                                    }

                                    cr.MoveTo (graphLeft, interYPos);
                                    for (var phase = rightPart; phase <= period; ++phase) {
                                        var currentXPos = interXPos + phase;
                                        var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, Math.PI / 2).Constrain (0, Math.PI / 2);
                                        interYPos = basePoint - delta * Math.Sin (radian);
                                        cr.LineTo (currentXPos, interYPos);

                                        if (currentXPos.WithinSetpoint (timeXPos, 1)) {
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

                        if (i != addedStateIndex) {
                            TouchColor.SetSource (cr, "secb");
                        } else {
                            TouchColor.SetSource (cr, "secc");
                        }
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
                            // Or if the previous state was off the start time needs to be rendered
                            if (firstTimeThrough || stateInfo.previous.lightingState.type == LightingStateType.Off) {
                                // If the start and end of the state are close together draw the end at alterating elevations
                                int startTextYPos;

                                if (firstTimeThrough) {
                                    startTextYPos = graphBottom;
                                } else {
                                    var lastPeriod = stateInfo.previous.lightingState.lengthInMinutes;
                                    if (lastPeriod < 80) {
                                        if (lastOnSecondLine) {
                                            startTextYPos = graphBottom;
                                            lastOnSecondLine = false;
                                        } else {
                                            startTextYPos = graphBottom + 18;
                                            lastOnSecondLine = true;
                                        }
                                    } else {
                                        startTextYPos = graphBottom;
                                        lastOnSecondLine = false;
                                    }
                                }

                                text = new TouchText (state.startTime.ToShortTimeString ());
                                text.alignment = TouchAlignment.Center;
                                text.Render (this, startXPos.ToInt () - 50, startTextYPos, 100);
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

                #endregion

                if (selectedState != -1) {
                    var stateInfo = stateInfos[selectedState];
                    var state = stateInfo.lightingState;

                    #region Render Selected State

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

                    #endregion

                    #region Start End Buttons

                    var startButtonX = startXPosSelected;
                    var endButtonX = endXPosSelected;
                    var startButtonY = startYPosSelected;
                    var endButtonY = endYPosSelected;

                    stateInfo.startButtonXPos = startButtonX - left;
                    stateInfo.startButtonYPos = startButtonY - top;
                    stateInfo.endButtonXPos = endButtonX - left;
                    stateInfo.endButtonYPos = endButtonY - top;

                    // State start adjustment button
                    cr.MoveTo (startButtonX + 15, startButtonY);
                    cr.Arc (startButtonX, startButtonY, 15, 0, 2 * Math.PI);
                    cr.ClosePath ();

                    var color = new TouchColor ("secb");
                    if (startButtonClicked) {
                        color.ModifyColor (0.75);
                    }

                    var outlineColor = new TouchColor (color);
                    outlineColor.ModifyColor (0.5);
                    var highlightColor = new TouchColor (color);
                    highlightColor.ModifyColor (1.2);
                    var lowlightColor = new TouchColor (color);
                    lowlightColor.ModifyColor (0.75);

                    using (var grad = new RadialGradient (startButtonX, startButtonY - 10, 0, startButtonX, startButtonY - 10, 25)) {
                        grad.AddColorStop (0, highlightColor.ToCairoColor ());
                        grad.AddColorStop (0.35, color.ToCairoColor ());
                        grad.AddColorStop (0.75, lowlightColor.ToCairoColor ());
                        cr.SetSource (grad);
                        cr.FillPreserve ();
                    }

                    outlineColor.SetSource (cr);
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    // State end adjustment button
                    cr.MoveTo (endButtonX + 15, endButtonY);
                    cr.Arc (endButtonX, endButtonY, 15, 0, 2 * Math.PI);
                    cr.ClosePath ();

                    color = new TouchColor ("secb");
                    if (endButtonClicked) {
                        color.ModifyColor (0.75);
                    }

                    outlineColor = new TouchColor (color);
                    outlineColor.ModifyColor (0.5);
                    highlightColor = new TouchColor (color);
                    highlightColor.ModifyColor (1.2);
                    lowlightColor = new TouchColor (color);
                    lowlightColor.ModifyColor (0.75);

                    using (var grad = new RadialGradient (endButtonX, endButtonY - 10, 0, endButtonX, endButtonY - 10, 25)) {
                        grad.AddColorStop (0, highlightColor.ToCairoColor ());
                        grad.AddColorStop (0.35, color.ToCairoColor ());
                        grad.AddColorStop (0.75, lowlightColor.ToCairoColor ());
                        cr.SetSource (grad);
                        cr.FillPreserve ();
                    }

                    outlineColor.SetSource (cr);
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    #endregion // Start End Buttons

                    #region Time Textboxes

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
                    TouchGlobal.DrawRoundedRectangle (cr, startTimeTextX + 4, graphBottom + 11, 79, 29, 3);
                    var shadowColor = new TouchColor ("grey4");
                    shadowColor.ModifyColor (0.5);
                    shadowColor.A = 0.4;
                    shadowColor.SetSource (cr);
                    cr.Fill ();

                    TouchGlobal.DrawRoundedRectangle (cr, startTimeTextX, graphBottom + 7, 80, 30, 3);
                    TouchColor.SetSource (cr, "grey4");
                    cr.FillPreserve ();

                    TouchColor.SetSource (cr, "grey0");
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    text = new TouchText (state.startTime.ToShortTimeString ());
                    text.alignment = TouchAlignment.Center;
                    text.font.color = "black";
                    text.Render (this, startTimeTextX.ToInt (), graphBottom + 12, 80);

                    // End time textbox
                    TouchGlobal.DrawRoundedRectangle (cr, endTimeTextX + 4, graphBottom + 11, 79, 29, 3);
                    shadowColor.SetSource (cr);
                    cr.Fill ();

                    TouchGlobal.DrawRoundedRectangle (cr, endTimeTextX, graphBottom + 7, 80, 30, 3);
                    TouchColor.SetSource (cr, "grey4");
                    cr.FillPreserve ();

                    TouchColor.SetSource (cr, "grey0");
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    text = new TouchText (state.endTime.ToShortTimeString ());
                    text.alignment = TouchAlignment.Center;
                    text.font.color = "black";
                    text.Render (this, endTimeTextX.ToInt (), graphBottom + 12, 80);

                    #endregion // Time Textboxes

                    #region Dimming Textboxes

                    // Dimming textboxes
                    if (dimmingFixture) {
                        // All light state types that are some sort of ramp have an integer value of 10 or greater
                        var showEndDimmingLevel = (int)stateInfo.lightingState.type >= 10;

                        double startDimmingTextY = 0, endDimmingTextY = 0;
                        if (showEndDimmingLevel) {
                            if (deltaSelected < 29) {
                                if (startYPosSelected < endYPosSelected) {
                                    startDimmingTextY = startYPosSelected - 25 + (deltaSelected - 2) / 2;
                                    endDimmingTextY = endYPosSelected - (deltaSelected - 2) / 2;
                                } else {
                                    startDimmingTextY = startYPosSelected - (deltaSelected - 2) / 2;
                                    endDimmingTextY = endYPosSelected - 25 + (deltaSelected - 2) / 2;
                                }
                            } else {
                                startDimmingTextY = startYPosSelected - 12.5;
                                endDimmingTextY = endYPosSelected - 12.5;
                            }
                        } else {
                            startDimmingTextY = startYPosSelected - 12.5;
                        }

                        stateInfo.startDimmingTextYPos = startDimmingTextY - top;
                        stateInfo.endDimmingTextYPos = endDimmingTextY - top;

                        // Start dimming textbox
                        TouchGlobal.DrawRoundedRectangle (cr, left + 4, startDimmingTextY + 4, 72, 29, 3);
                        shadowColor.SetSource (cr);
                        cr.Fill ();

                        TouchGlobal.DrawRoundedRectangle (cr, left, startDimmingTextY, 73, 30, 3);
                        TouchColor.SetSource (cr, "grey4");
                        cr.FillPreserve ();

                        TouchColor.SetSource (cr, "grey0");
                        cr.LineWidth = 1;
                        cr.Stroke ();

                        text = new TouchText (state.startingDimmingLevel.ToString ());
                        text.alignment = TouchAlignment.Center;
                        text.font.color = "black";
                        text.Render (this, left, startDimmingTextY.ToInt () + 5, 73);

                        // End dimming textbox
                        if (showEndDimmingLevel) {
                            TouchGlobal.DrawRoundedRectangle (cr, left + 4, endDimmingTextY + 4, 72, 29, 3);
                            shadowColor.SetSource (cr);
                            cr.Fill ();

                            TouchGlobal.DrawRoundedRectangle (cr, left, endDimmingTextY, 73, 30, 3);
                            TouchColor.SetSource (cr, "grey4");
                            cr.FillPreserve ();

                            TouchColor.SetSource (cr, "black");
                            cr.LineWidth = 1;
                            cr.Stroke ();

                            text = new TouchText (state.endingDimmingLevel.ToString ());
                            text.alignment = TouchAlignment.Center;
                            text.font.color = "black";
                            text.Render (this, left, endDimmingTextY.ToInt () + 5, 73);
                        }


                    } else {
                        stateInfo.startDimmingTextYPos = 0;
                        stateInfo.endDimmingTextYPos = 0;
                    }

                    #endregion
                } else {
                    #region Linear Ramp Button

                    var buttonX = left + 90;
                    TouchGlobal.DrawRoundedRectangle (cr, buttonX, top + 1, 80, 40, 3);
                    cr.MoveTo (buttonX + 25, top + 35);
                    cr.LineTo (buttonX + 55, top + 5);

                    TouchColor color = dimmingFixture ? "secb" : "grey1";
                    if (linearClicked) {
                        color.ModifyColor (0.75);
                    }
                    color.SetSource (cr);
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    #endregion

                    #region Half Parabola Ramp Button

                    buttonX += 90;
                    TouchGlobal.DrawRoundedRectangle (cr, buttonX, top + 1, 80, 40, 3);
                    cr.MoveTo (buttonX + 25, top + 35);
                    cr.Arc (buttonX + 55, top + 35, 30, Math.PI, 3 * Math.PI / 2);

                    color = dimmingFixture ? "secb" : "grey1";
                    if (halfParabolaClicked) {
                        color.ModifyColor (0.75);
                    }
                    color.SetSource (cr);
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    #endregion

                    #region Full Parabola Ramp Button

                    buttonX += 90;
                    TouchGlobal.DrawRoundedRectangle (cr, buttonX, top + 1, 80, 40, 3);
                    cr.MoveTo (buttonX + 10, top + 35);
                    cr.Arc (buttonX + 40, top + 35, 30, Math.PI, 0);

                    color = dimmingFixture ? "secb" : "grey1";
                    if (fullParabolaClicked) {
                        color.ModifyColor (0.75);
                    }
                    color.SetSource (cr);
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    #endregion

                    #region On Button

                    buttonX += 90;
                    TouchGlobal.DrawRoundedRectangle (cr, buttonX, top + 1, 80, 40, 3);
                    cr.MoveTo (buttonX + 25, top + 10);
                    cr.LineTo (buttonX + 55, top + 10);

                    color = "secb";
                    if (onClicked) {
                        color.ModifyColor (0.75);
                    }
                    color.SetSource (cr);
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    #endregion

                    #region Off Button

                    buttonX += 90;
                    TouchGlobal.DrawRoundedRectangle (cr, buttonX, top + 1, 80, 40, 3);
                    cr.MoveTo (buttonX + 25, top + 30);
                    cr.LineTo (buttonX + 55, top + 30);

                    color = "secb";
                    if (offClicked) {
                        color.ModifyColor (0.75);
                    }
                    color.SetSource (cr);
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    #endregion
                }

                cr.MoveTo (timeXPos, timeYPos);
                cr.Arc (timeXPos, timeYPos, 6, 0, 2 * Math.PI);
                TouchColor.SetSource (cr, "seca");
                cr.ClosePath ();
                cr.Fill ();
            }
        }

        public void SetStates (LightingState[] states, bool dimmingFixture, bool resetStateInfoChanged = true) {
            stateInfos.Clear ();
            selectedState = -1;
            addedStateIndex = -1;
            hasStateInfoChanged = !resetStateInfoChanged;

            for (int i = 0; i < states.Length; ++i) {
                var stateInfo = new StateInfo ();
                stateInfo.lightingState = states[i];
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
            SaveStateInfos ();

            QueueDraw ();
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
                }

                if ((clickX > stateInfo.endButtonXPos - 12) && (clickX < stateInfo.endButtonXPos + 12) &&
                    (clickY > stateInfo.endButtonYPos - 12) && (clickY < stateInfo.endButtonYPos + 12)) {
                    endButtonClicked = true;
                }
            } else {
                if ((clickY > 0) && (clickY < 40)) {
                    if ((clickX > 90) && (clickX < 170)) {
                        if (dimmingFixture) {
                            linearClicked = true;
                            newStateType = LightingStateType.LinearRamp;
                        }
                    } else if ((clickX > 180) && (clickX < 260)) {
                        if (dimmingFixture) {
                            halfParabolaClicked = true;
                            newStateType = LightingStateType.HalfParabolaRamp;
                        }
                    } else if ((clickX > 270) && (clickX < 350)) {
                        if (dimmingFixture) {
                            fullParabolaClicked = true;
                            newStateType = LightingStateType.ParabolaRamp;
                        }
                    } else if ((clickX > 360) && (clickX < 440)) {
                        onClicked = true;
                        newStateType = LightingStateType.On;
                    } else if ((clickX > 450) && (clickX < 530)) {
                        offClicked = true;
                        newStateType = LightingStateType.Off;
                    }
                }
            }

            entityClicked = startButtonClicked | endButtonClicked | linearClicked | halfParabolaClicked | fullParabolaClicked | onClicked | offClicked;
            if (entityClicked) {
                clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
            }

            QueueDraw ();
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var x = args.Event.X;
            var y = args.Event.Y;

            if (!entityClicked) {
                var releasedHappenedOnSomeEnitity = false;

                if (selectedState != -1) {
                    var stateInfo = stateInfos[selectedState];

                    if ((y > graphBottomRelative + 7) && (y < graphBottomRelative + 32)) {
                        if ((x > stateInfo.startTimeTextXPos) && (x < stateInfo.startTimeTextXPos + 80)) {
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

                            releasedHappenedOnSomeEnitity = true;
                        } else if ((x > stateInfo.endTimeTextXPos) && (x < stateInfo.endTimeTextXPos + 80)) {
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

                            releasedHappenedOnSomeEnitity = true;
                        }
                    } else if ((x > graphLeftRelative - 80) && (x < graphLeftRelative - 7)) {
                        if ((y > stateInfo.startDimmingTextYPos) && (y < stateInfo.startDimmingTextYPos + 35)) {
                            var parent = Toplevel as Window;
                            var t = new TouchNumberInput (true, parent);
                            t.Title = "Start Dimming Level";
                            t.TextSetEvent += (o, a) => {
                                try {
                                    var newDimmingLevel = Convert.ToSingle (a.text);
                                    SetStartDimmingLevel (newDimmingLevel);
                                } catch {
                                    MessageBox.Show ("Invalid Start Time");
                                    a.keepText = false;
                                }
                            };

                            t.Run ();
                            t.Destroy ();

                            releasedHappenedOnSomeEnitity = true;
                        } else if ((y > stateInfo.endDimmingTextYPos) && (y < stateInfo.endDimmingTextYPos + 35)) {
                            var parent = Toplevel as Window;
                            var t = new TouchNumberInput (true, parent);
                            t.Title = "End Dimming Level";
                            t.TextSetEvent += (o, a) => {
                                try {
                                    var newDimmingLevel = Convert.ToSingle (a.text);
                                    SetEndDimmingLevel (newDimmingLevel);
                                } catch {
                                    MessageBox.Show ("Invalid Start Time");
                                    a.keepText = false;
                                }
                            };

                            t.Run ();
                            t.Destroy ();

                            releasedHappenedOnSomeEnitity = true;
                        }
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
                } else if (!releasedHappenedOnSomeEnitity) {
                    selectedState = -1;
                }
            } else {
                if (selectedState == -1) {
                    if ((x > graphLeftRelative) && (x < graphRightRelative) &&
                        (y > graphTopRelative) && (y < graphBottomRelative)) {
                        if (addedStateIndex != -1) {
                            if (stateInfos[addedStateIndex].lightingState.type != LightingStateType.Off) {
                                selectedState = addedStateIndex;
                            }
                            hasStateInfoChanged = true;
                        }
                    } else {
                        RestoreSavedStateInfos ();
                    }
                    addedStateIndex = -1;
                }
            }

            clicked = false;
            startButtonClicked = false;
            endButtonClicked = false;
            linearClicked = false;
            halfParabolaClicked = false;
            fullParabolaClicked = false;
            onClicked = false;
            offClicked = false;
            entityClicked = false;
            movedOutOfXRange = false;
            movedOutOfYRange = false;
            addedStateIndex = -1;
            SaveStateInfos ();

            QueueDraw ();
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                GetPointer (out int x, out int y);

                var yDelta = clickY - y;
                var xDelta = x - clickX;

                if (!movedOutOfXRange && Math.Abs (xDelta) > 6) {
                    movedOutOfXRange = true;
                }

                if (!movedOutOfYRange && Math.Abs (yDelta) > 6) {
                    movedOutOfYRange = true;
                }

                if (startButtonClicked) {
                    if (movedOutOfXRange) {
                        var newStartMinutes = xDelta * minutesPerPixel + stateInfos[selectedState].lightingState.startTime.totalMinutes;
                        if (newStartMinutes < 0) {
                            newStartMinutes = 1439;
                        } else if (newStartMinutes > 1439) {
                            newStartMinutes = 0;
                        }
                        var newStartTime = new Time (new TimeSpan (0, newStartMinutes.ToInt (), 0));
                        ValidateAndSetStartTime (newStartTime);
                    }

                    if (dimmingFixture && movedOutOfYRange) {
                        var newDimmingLevel = yDelta * dimmingPerPixel + stateInfos[selectedState].lightingState.startingDimmingLevel;
                        SetStartDimmingLevel (newDimmingLevel);
                    }
                } else if (endButtonClicked) {
                    if (movedOutOfXRange) {
                        var newEndMinutes = xDelta * minutesPerPixel + stateInfos[selectedState].lightingState.endTime.totalMinutes;
                        if (newEndMinutes < 0) {
                            newEndMinutes = 1439;
                        } else if (newEndMinutes > 1439) {
                            newEndMinutes = 0;
                        }
                        var newEndTime = new Time (new TimeSpan (0, newEndMinutes.ToInt (), 0));
                        ValidateAndSetEndTime (newEndTime);
                    }

                    if (dimmingFixture && movedOutOfYRange) {
                        var newDimmingLevel = yDelta * dimmingPerPixel + stateInfos[selectedState].lightingState.endingDimmingLevel;
                        SetEndDimmingLevel (newDimmingLevel);
                    }
                } else {
                    if ((x > graphLeftRelative) && (x < graphRightRelative) &&
                        (y > graphTopRelative) && (y < graphBottomRelative)) {

                        var newMinutes = (clickX - graphLeftRelative) * minutesPerPixel;
                        var newGeneralTime = new Time (new TimeSpan (0, newMinutes.ToInt (), 0));
                        AddState (newGeneralTime);
                    }
                }

                if (movedOutOfXRange) {
                    clickX = x;
                }

                if (movedOutOfYRange) {
                    clickY = y;
                }

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
                } else {
                    hasStateInfoChanged = true;
                }
            }

            return timeOkay;
        }

        void SetStartDimmingLevel (double newStartDimmingLevel) {
            var newDimmingLevel = (float)newStartDimmingLevel.Constrain (0, 100);
            SetStartDimmingLevel (stateInfos[selectedState], newDimmingLevel);
            hasStateInfoChanged = true;
        }

        void SetStartDimmingLevel (StateInfo stateInfo, float newDimmingLevel) {
            var state = stateInfo.lightingState;
            var type = state.type;

            if (type == LightingStateType.On) {
                if (state.startingDimmingLevel != newDimmingLevel) {
                    state.startingDimmingLevel = newDimmingLevel;

                    if (adjustDimmingTogether) {
                        SetEndDimmingLevel (stateInfo.previous, newDimmingLevel);
                    }
                }

                if (state.endingDimmingLevel != newDimmingLevel) {
                    state.endingDimmingLevel = newDimmingLevel;

                    if (adjustDimmingTogether) {
                        SetStartDimmingLevel (stateInfo.next, newDimmingLevel);
                    }
                }
            } else if (type != LightingStateType.Off) {
                if (state.startingDimmingLevel != newDimmingLevel) {
                    state.startingDimmingLevel = newDimmingLevel;

                    if (adjustDimmingTogether) {
                        SetEndDimmingLevel (stateInfo.previous, newDimmingLevel);
                    }
                }
            }
        }

        public bool ValidateAndSetEndTime (Time newEndTime) {
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
                } else {
                    hasStateInfoChanged = true;
                }
            }

            return timeOkay;
        }

        void SetEndDimmingLevel (double newEndDimmingLevel) {
            var newDimmingLevel = (float)newEndDimmingLevel.Constrain (0, 100);
            SetEndDimmingLevel (stateInfos[selectedState], newDimmingLevel);
            hasStateInfoChanged = true;
        }

        void SetEndDimmingLevel (StateInfo stateInfo, float newDimmingLevel) {
            var state = stateInfo.lightingState;
            var type = state.type;

            if (type == LightingStateType.On) {
                if (state.startingDimmingLevel != newDimmingLevel) {
                    state.startingDimmingLevel = newDimmingLevel;

                    if (adjustDimmingTogether) {
                        SetEndDimmingLevel (stateInfo.previous, newDimmingLevel);
                    }
                }

                if (state.endingDimmingLevel != newDimmingLevel) {
                    state.endingDimmingLevel = newDimmingLevel;

                    if (adjustDimmingTogether) {
                        SetStartDimmingLevel (stateInfo.next, newDimmingLevel);
                    }
                }
            } else if (type != LightingStateType.Off) {
                if (state.endingDimmingLevel != newDimmingLevel) {
                    state.endingDimmingLevel = newDimmingLevel;

                    if (adjustDimmingTogether) {
                        SetStartDimmingLevel (stateInfo.next, newDimmingLevel);
                    }
                }
            }
        }

        public void RemoveSelectedState () {
            RemoveState (selectedState);
            selectedState = -1;
            hasStateInfoChanged = true;
            SaveStateInfos ();
            QueueDraw ();
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
                    var midTime = new Time (new TimeSpan (0, midMinutes.ToInt (), 0));
                    stateInfo.previous.lightingState.endTime = midTime;
                    stateInfo.next.lightingState.startTime = midTime;
                } else if ((stateInfo.previous.lightingState.type == LightingStateType.Off) &&
                    (stateInfo.next.lightingState.type != LightingStateType.Off)) {
                    stateInfo.previous.lightingState.endTime = stateInfo.lightingState.endTime;
                } else if ((stateInfo.previous.lightingState.type != LightingStateType.Off) &&
                    (stateInfo.next.lightingState.type == LightingStateType.Off)) {
                    stateInfo.next.lightingState.startTime = stateInfo.lightingState.startTime;
                } else {
                    var nextStateIndex = stateInfos.IndexOf (stateInfo.next);
                    RemoveState (nextStateIndex);
                }
            }
        }

        void AddState (Time generalTime) {
            if (addedStateIndex != -1) {
                RestoreSavedStateInfos ();
                addedStateIndex = -1;
            }

            if (stateInfos.Count >= 2) {
                foreach (var stateInfo in stateInfos) {
                    var state = stateInfo.lightingState;

                    if (state.startTime.Before (state.endTime)) {
                        if (generalTime.After (state.startTime) && generalTime.Before (state.endTime)) {
                            var timeToEnd = Math.Abs (generalTime.totalMinutes - state.endTime.totalMinutes);
                            var timeToStart = Math.Abs (generalTime.totalMinutes - state.startTime.totalMinutes);

                            PlaceNewState (stateInfo, generalTime, timeToStart, timeToEnd);
                            break;
                        }
                    } else {
                        if (generalTime.After (state.endTime) && generalTime.Before (Time.TimeMidnight)) {
                            var timeToEnd = Math.Abs (generalTime.totalMinutes - Time.TimeMidnight.totalMinutes) + state.endTime.totalMinutes;
                            var timeToStart = Math.Abs (generalTime.totalMinutes - state.startTime.totalMinutes);

                            PlaceNewState (stateInfo, generalTime, timeToStart, timeToEnd);
                            break;
                        } 

                        if (generalTime.After (Time.TimeZero) && generalTime.Before (state.startTime)) {
                            var timeToEnd = Math.Abs (generalTime.totalMinutes - state.endTime.totalMinutes);
                            var timeToStart = Math.Abs (generalTime.totalMinutes - Time.TimeZero.totalMinutes) + state.startTime.totalMinutes;

                            PlaceNewState (stateInfo, generalTime, timeToStart, timeToEnd);
                            break;
                        }
                    }
                }
            } else if (stateInfos.Count == 1) {
                RemoveState (0);
                AddState (generalTime);
            } else {
                var stateTime = new Time (generalTime);
                stateTime.AddHours (-1);
                var endTime = new Time (generalTime);
                endTime.AddHours (1);
                var newState = new LightingState (stateTime, endTime, newStateType);
                var offState = new LightingState (endTime, stateTime, LightingStateType.Off);

                var newStateInfo = new StateInfo ();
                newStateInfo.lightingState = newState;

                var offStateInfo = new StateInfo ();
                offStateInfo.lightingState = offState;

                newStateInfo.next = offStateInfo;
                newStateInfo.previous = offStateInfo;

                offStateInfo.next = newStateInfo;
                offStateInfo.previous = newStateInfo;

                stateInfos.Add (newStateInfo);
                stateInfos.Add (offStateInfo);
                addedStateIndex = 0;
            }
        }

        void PlaceNewState (StateInfo stateInfo, Time generalTime, double timeToStart, double timeToEnd) {
            var state = stateInfo.lightingState;
            var next = stateInfo.next.lightingState;
            var previous = stateInfo.previous.lightingState;

            if (state.type == LightingStateType.Off && newStateType != LightingStateType.Off) {
                if (timeToEnd < timeToStart) {
                    if (timeToEnd < 120) {
                        AddStateAtEnd (stateInfo, timeToEnd);
                    } else {
                        AddStateAtTimePoint (stateInfo, generalTime, timeToStart, timeToEnd);
                    }
                } else {
                    if (timeToStart < 120) {
                        AddStateAtBeginning (stateInfo, timeToStart);
                    } else {
                        AddStateAtTimePoint (stateInfo, generalTime, timeToStart, timeToEnd);
                    }
                }
            } else {
                if (timeToEnd < timeToStart) {
                    if (newStateType != LightingStateType.Off ||
                        (state.type != LightingStateType.Off && next.type != LightingStateType.Off)) {
                        AddStateAtEnd (stateInfo, timeToEnd);
                    }
                } else {
                    if (newStateType != LightingStateType.Off ||
                        (state.type != LightingStateType.Off && previous.type != LightingStateType.Off)) {
                        AddStateAtBeginning (stateInfo, timeToStart);
                    }
                }
            }
        }

        void AddStateAtEnd (StateInfo stateInfo, double timeToEnd) {
            LightingState newState;
            if (timeToEnd < 60) {
                newState = AddStateAtEnd (stateInfo, 60);
                var secondNewState = AddStateToNext (stateInfo, 60);
                newState.endTime = secondNewState.endTime;
                newState.endingDimmingLevel = secondNewState.endingDimmingLevel;
            } else {
                newState = AddStateAtEnd (stateInfo, 120);
            }

            var newStateInfo = new StateInfo ();
            newStateInfo.lightingState = newState;
            newStateInfo.previous = stateInfo;
            newStateInfo.next = stateInfo.next;
            newStateInfo.next.previous = newStateInfo;
            stateInfo.next = newStateInfo;

            addedStateIndex = stateInfos.IndexOf (stateInfo) + 1;
            stateInfos.Insert (addedStateIndex, newStateInfo);
        }

        LightingState AddStateAtEnd (StateInfo stateInfo, int length) {
            var state = stateInfo.lightingState;
            var stateLength = state.lengthInMinutes.ToInt ();

            if (stateLength <= 1) {
                // The state is too short to add a new state
                // Add it to the next state
                return AddStateToNext (stateInfo, length);
            }

            var newState = new LightingState (new Time (state.endTime), new Time (state.endTime), newStateType);

            if (stateLength > (length * 2)) {
                newState.startTime.AddMinutes (-length);
            } else {
                newState.startTime.AddMinutes (-stateLength / 2);
            }

            state.endTime = new Time (newState.startTime);

            if (dimmingFixture) {
                switch (newStateType) {
                case LightingStateType.On:
                case LightingStateType.ParabolaRamp:
                    if (stateInfo.lightingState.type == LightingStateType.Off) {
                        newState.startingDimmingLevel = stateInfo.next.lightingState.startingDimmingLevel;
                        newState.endingDimmingLevel = stateInfo.next.lightingState.startingDimmingLevel;
                    } else {
                        newState.startingDimmingLevel = stateInfo.lightingState.endingDimmingLevel;
                        newState.endingDimmingLevel = stateInfo.lightingState.endingDimmingLevel;
                    }

                    break;
                case LightingStateType.HalfParabolaRamp:
                case LightingStateType.LinearRamp:
                    newState.startingDimmingLevel = stateInfo.lightingState.endingDimmingLevel;
                    newState.endingDimmingLevel = stateInfo.next.lightingState.startingDimmingLevel;
                    break;
                default:
                    newState.startingDimmingLevel = 0;
                    newState.endingDimmingLevel = 0;
                    break;
                }
            } else {
                if (newStateType != LightingStateType.Off) {
                    newState.startingDimmingLevel = 100;
                    newState.endingDimmingLevel = 100;
                } else {
                    newState.startingDimmingLevel = 0;
                    newState.endingDimmingLevel = 0;
                }
            }

            return newState;
        }

        void AddStateAtBeginning (StateInfo stateInfo, double timeToStart) {
            LightingState newState;
            if (timeToStart < 60) {
                newState = AddStateAtBeginning (stateInfo, 60);
                var secondNewState = AddStateToPrevious (stateInfo, 60);
                newState.startTime = secondNewState.startTime;
                newState.startingDimmingLevel = secondNewState.startingDimmingLevel;
            } else {
                newState = AddStateAtBeginning (stateInfo, 120);
            }

            var newStateInfo = new StateInfo ();
            newStateInfo.lightingState = newState;

            newStateInfo.next = stateInfo;
            newStateInfo.previous = stateInfo.previous;
            newStateInfo.previous.next = newStateInfo;
            stateInfo.previous = newStateInfo;

            addedStateIndex = stateInfos.IndexOf (stateInfo);
            stateInfos.Insert (addedStateIndex, newStateInfo);
        }

        LightingState AddStateAtBeginning (StateInfo stateInfo, int length) {
            var state = stateInfo.lightingState;
            var stateLength = state.lengthInMinutes.ToInt ();

            if (stateLength <= 1) {
                // The state is too short to add a new state
                // Add it to the next state
                return AddStateToPrevious (stateInfo, length);
            }

            var newState = new LightingState (new Time (state.startTime), new Time (state.startTime), newStateType);

            if (stateLength > (length * 2)) {
                newState.endTime.AddMinutes (length);
            } else {
                newState.endTime.AddMinutes (stateLength / 2);
            }

            state.startTime = new Time (newState.endTime);

            if (dimmingFixture) {
                switch (newStateType) {
                case LightingStateType.On:
                case LightingStateType.ParabolaRamp:
                    if (stateInfo.previous.lightingState.type == LightingStateType.Off) {
                        newState.startingDimmingLevel = stateInfo.lightingState.startingDimmingLevel;
                        newState.endingDimmingLevel = stateInfo.lightingState.startingDimmingLevel;
                    } else {
                        newState.startingDimmingLevel = stateInfo.previous.lightingState.endingDimmingLevel;
                        newState.endingDimmingLevel = stateInfo.previous.lightingState.endingDimmingLevel;
                    }
                    break;
                case LightingStateType.HalfParabolaRamp:
                case LightingStateType.LinearRamp:
                    newState.startingDimmingLevel = stateInfo.previous.lightingState.endingDimmingLevel;
                    newState.endingDimmingLevel = stateInfo.lightingState.startingDimmingLevel;
                    break;
                default:
                    newState.startingDimmingLevel = 0;
                    newState.endingDimmingLevel = 0;
                    break;
                }
            } else {
                if (newStateType != LightingStateType.Off) {
                    newState.startingDimmingLevel = 100;
                    newState.endingDimmingLevel = 100;
                } else {
                    newState.startingDimmingLevel = 0;
                    newState.endingDimmingLevel = 0;
                }
            }

            return newState;
        }

        void AddStateAtTimePoint (StateInfo stateInfo, Time generalTime, double timeToStart, double timeToEnd) {
            var state = stateInfo.lightingState;

            var startTime = new Time (generalTime);
            var endTime = new Time (generalTime);
            if (timeToStart > 120) {
                startTime.AddMinutes (-60);
            } else {
                startTime.AddMinutes (-timeToStart.ToInt () / 2);
            }

            if (timeToEnd > 120) {
                endTime.AddMinutes (60);
            } else {
                endTime.AddMinutes (timeToEnd.ToInt () / 2);
            }

            var newState = new LightingState (startTime, endTime, newStateType);
            var newNextState = new LightingState (new Time (newState.endTime), new Time (state.endTime), state.type);

            state.endTime = new Time (newState.startTime);

            var newStateInfo = new StateInfo ();
            newStateInfo.lightingState = newState;
            var newNextStateInfo = new StateInfo ();
            newNextStateInfo.lightingState = newNextState;

            newStateInfo.previous = stateInfo;
            newStateInfo.next = newNextStateInfo;

            newNextStateInfo.previous = newStateInfo;
            newNextStateInfo.next = stateInfo.next;

            stateInfo.next = newStateInfo;

            addedStateIndex = stateInfos.IndexOf (stateInfo) + 1;
            stateInfos.Insert (addedStateIndex, newStateInfo);

            stateInfos.Insert (addedStateIndex + 1, newNextStateInfo);
        }

        LightingState AddStateToNext (StateInfo stateInfo, int length) {
            var next = stateInfo.next;
            var nextStateLength = next.lightingState.lengthInMinutes.ToInt ();

            if (nextStateLength <= 1) {
                return AddStateToNext (next.next, length);
            } 
                
            return AddStateAtBeginning (next, length);
        }

        LightingState AddStateToPrevious (StateInfo stateInfo, int length) {
            var previous = stateInfo.previous;
            var previousStateLength = previous.lightingState.lengthInMinutes.ToInt ();

            if (previousStateLength <= 1) {
                AddStateToNext (previous.previous, length);
            }

            return AddStateAtEnd (previous, length);
        }

        public void RestoreSavedStateInfos () {
            SetStates (savedLightingStates.ToArray (), dimmingFixture, !hasStateInfoChanged);
            QueueDraw ();
        }

        public void SaveStateInfos () {
            savedLightingStates.Clear ();
            foreach (var stateInfo in stateInfos) {
                savedLightingStates.Add (new LightingState (stateInfo.lightingState));
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
            public double startDimmingTextYPos, endDimmingTextYPos;

            public override string ToString () {
                return lightingState.ToString ();
            }
        }
    }
}
