#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

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
using Gtk;
using Cairo;

namespace GoodtimeDevelopment.TouchWidget
{
    public delegate void SelectorChangedEventHandler (object sender, SelectorChangedEventArgs args);

    public class SelectorChangedEventArgs : EventArgs
    {
        public int currentSelectedIndex;
        public int id;

        public SelectorChangedEventArgs (int currentSelectedIndex, int id) {
            this.currentSelectedIndex = currentSelectedIndex;
            this.id = id;
        }
    }

    public enum MySliderSize
    {
        Small = 1,
        Large
    }

    public class TouchSelectorSwitch : EventBox
    {
        bool clicked;
        uint clickTimer;
        int click1, click2;

        public int selectionCount { get; }
        public int currentSelected;
        public TouchOrientation orientation;
        public MySliderSize sliderSize;
        public TouchColor[] backgoundTextColorOptions;
        public TouchColor[] selectedTextColorOptions;
        public TouchColor[] sliderColorOptions;
        public string[] textOptions;
        public int id;

        public event SelectorChangedEventHandler SelectorChangedEvent;

        public TouchSelectorSwitch (int id, int selectionCount, int currentSelectedIndex, TouchOrientation orientation) {
            Visible = true;
            VisibleWindow = false;

            this.id = id;
            this.selectionCount = selectionCount;
            currentSelected = currentSelectedIndex;
            this.orientation = orientation;
            sliderSize = MySliderSize.Large;

            backgoundTextColorOptions = new TouchColor[this.selectionCount];
            for (int i = 0; i < backgoundTextColorOptions.Length; ++i) {
                backgoundTextColorOptions[i] = new TouchColor ("white");
            }

            selectedTextColorOptions = new TouchColor[this.selectionCount];
            for (int i = 0; i < backgoundTextColorOptions.Length; ++i) {
                selectedTextColorOptions[i] = new TouchColor ("white");
            }

            sliderColorOptions = new TouchColor[this.selectionCount];
            for (int i = 0; i < sliderColorOptions.Length; ++i) {
                sliderColorOptions[i] = new TouchColor ("grey2");
            }

            textOptions = new string[this.selectionCount];
            for (int i = 0; i < textOptions.Length; ++i) {
                textOptions[i] = string.Empty;
            }

            clicked = false;
            clickTimer = 0;
            click1 = 0;
            click2 = 0;

            if (this.orientation == TouchOrientation.Horizontal) {
                SetSizeRequest (80, 30);
            } else {
                SetSizeRequest (30, 80);
            }

            ExposeEvent += OnExpose;
            ButtonPressEvent += OnSelectorPress;
            ButtonReleaseEvent += OnSelectorRelease;
        }

        public TouchSelectorSwitch (int id, int selectionCount) : this (id, selectionCount, 0, TouchOrientation.Horizontal) { }

        public TouchSelectorSwitch (int selectionCount) : this (0, selectionCount, 0, TouchOrientation.Horizontal) { }

        public TouchSelectorSwitch () : this (0, 2, 0, TouchOrientation.Horizontal) { }

        public override void Dispose () {
            if (clickTimer != 0) {
                GLib.Source.Remove (clickTimer);
            }
            base.Dispose ();
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int height = Allocation.Height;
                int width = Allocation.Width;
                int top = Allocation.Top;
                int left = Allocation.Left;

                int seperation, sliderWidth, sliderLength, sliderMax, x, y;

                if (orientation == TouchOrientation.Horizontal) {
                    if (sliderSize == MySliderSize.Small)
                        sliderWidth = height;
                    else {
                        sliderWidth = width / selectionCount;
                        sliderWidth += (selectionCount - 2) * 8;
                    }

                    sliderLength = width - sliderWidth;
                    sliderMax = left + sliderLength;

                    seperation = sliderLength / (selectionCount - 1);

                    seperation *= currentSelected;

                    if (clicked)
                        seperation += (click2 - click1);

                    x = left + seperation;
                    if (x < left)
                        x = left;
                    if (x > sliderMax)
                        x = sliderMax;
                    y = top;
                } else {
                    if (sliderSize == MySliderSize.Small)
                        sliderWidth = width;
                    else {
                        sliderWidth = height / selectionCount;
                        sliderWidth += (selectionCount - 2) * 8;
                    }

                    sliderLength = height - sliderWidth;
                    sliderMax = top + sliderLength;

                    seperation = sliderLength / (selectionCount - 1);

                    seperation *= currentSelected;

                    if (clicked)
                        seperation += click2 - click1;

                    y = top + seperation;
                    if (y < top)
                        y = top;
                    if (y > sliderMax)
                        y = sliderMax;
                    x = left;
                }

                // Background 
                if (orientation == TouchOrientation.Horizontal) {
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, height / 2);
                } else {
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, width / 2);
                }
                TouchColor.SetSource (cr, "grey0");
                cr.FillPreserve ();

                TouchColor.SetSource (cr, "black");
                cr.LineWidth = 1;
                cr.Stroke ();

                // Slider
                double sliderLeft, sliderTop, sliderBottom;
                if (orientation == TouchOrientation.Horizontal) {
                    sliderLeft = x;
                    sliderTop = y;
                    sliderBottom = y + height;

                    TouchGlobal.DrawRoundedRectangle (cr, x, y, sliderWidth, height, height / 2);
                } else {
                    sliderLeft = x;
                    sliderTop = y;
                    sliderBottom = y + sliderWidth;

                    TouchGlobal.DrawRoundedRectangle (cr, x, y, width, sliderWidth, width / 2);
                }

                var sliderColor = sliderColorOptions[currentSelected];
                var outlineColor = new TouchColor (sliderColor);
                outlineColor.ModifyColor (0.5);
                var highlightColor = new TouchColor (sliderColor);
                highlightColor.ModifyColor (1.4);
                var lowlightColor = new TouchColor (sliderColor);
                lowlightColor.ModifyColor (0.75);

                using (var grad = new LinearGradient (sliderLeft, sliderTop, sliderLeft, sliderBottom)) {
                    grad.AddColorStop (0, highlightColor.ToCairoColor ());
                    grad.AddColorStop (0.2, sliderColor.ToCairoColor ());
                    grad.AddColorStop (0.85, lowlightColor.ToCairoColor ());
                    cr.SetSource (grad);
                    cr.FillPreserve ();
                }

                outlineColor.SetSource (cr);
                cr.LineWidth = 0.9;
                cr.Stroke ();

                // Text Labels
                var render = new TouchText ();
                render.textWrap = TouchTextWrap.Shrink;
                render.alignment = TouchAlignment.Center;

                seperation = Allocation.Width / selectionCount;
                x = Allocation.Left;
                for (int i = 0; i < selectionCount; ++i) {
                    if (!string.IsNullOrWhiteSpace (textOptions[i])) {
                        render.font.color = i == currentSelected ? selectedTextColorOptions[i] : backgoundTextColorOptions[i];
                        render.text = textOptions[i];
                        render.Render (this, x, Allocation.Top + 4, seperation);
                    }

                    x += seperation;
                }
            }
        }

        protected void OnSelectorPress (object o, ButtonPressEventArgs args) {
            clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
            clicked = true;
            click1 = orientation == TouchOrientation.Horizontal ? (int)args.Event.X : (int)args.Event.Y;
        }

        protected void OnSelectorRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;

            if (orientation == TouchOrientation.Horizontal) {
                int x = (int)args.Event.X;
                int sliderLength = Allocation.Width;
                int seperation = sliderLength / selectionCount;

                if (x < 0) {
                    currentSelected = 0;
                } else if (x > sliderLength) {
                    currentSelected = selectionCount - 1;
                } else {
                    for (int i = 0; i < selectionCount; ++i) {
                        int leftBoundery = i * seperation;
                        int rightBoundery = (i + 1) * seperation;
                        if ((x >= leftBoundery) && (x <= rightBoundery)) {
                            currentSelected = i;
                            break;
                        }
                    }
                }
            } else {
                var y = (int)args.Event.Y;
                var sliderLength = Allocation.Height - Allocation.Width;
                var sliderMax = Allocation.Height;
                var seperation = sliderLength / (selectionCount - 1);

                if (y < 0)
                    currentSelected = 0;
                else if (y > sliderMax)
                    currentSelected = selectionCount - 1;
                else {
                    for (int i = 0; i < selectionCount; ++i) {
                        var leftBoundery = i * seperation;
                        var rightBoundery = (i + 1) * seperation;
                        if ((y >= leftBoundery) && (y <= rightBoundery)) {
                            currentSelected = i;
                            break;
                        }
                    }
                }
            }

            QueueDraw ();

            SelectorChangedEvent?.Invoke (this, new SelectorChangedEventArgs (currentSelected, id));
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);
                if (orientation == TouchOrientation.Horizontal) {
                    click2 = x;
                } else {
                    click2 = y;
                }

                QueueDraw ();
            }

            return clicked;
        }
    }
}

