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
using System.Collections.Generic;
using Gtk;
using Cairo;
using GoodtimeDevelopment.Utilites;

namespace GoodtimeDevelopment.TouchWidget
{
    public delegate void ComboBoxChangedEventHandler (object sender, ComboBoxChangedEventArgs args);

    public class ComboBoxChangedEventArgs : EventArgs
    {
        public int activeIndex;
        public string activeText;
        public bool keepChange;

        public ComboBoxChangedEventArgs (int activeIndex, string activeText) {
            this.activeIndex = activeIndex;
            this.activeText = activeText;
            keepChange = true;
        }
    }

    public class TouchComboBox : EventBox
    {
        public List<string> comboList;
        public string nonActiveMessage;
        public int activeIndex;
        public string activeText {
            get {
                if (activeIndex != -1) {
                    return comboList[activeIndex];
                } else {
                    return string.Empty;
                }
            }
            set {
                if (comboList.Contains (value)) {
                    for (int i = 0; i < comboList.Count; i++) {
                        if (value == comboList[i]) {
                            activeIndex = i;
                            break;
                        }
                    }
                }
            }
        }
        public int maxListHeight;

        bool listDropdown, secondClick, includeScrollBar, scrollBarClicked,
            scrollBarMoved, scrollBarUpClicked, scrollBarDownClicked;
        int highlighted, height, listOffset, scrollBarHeight, clickY;

        public event ComboBoxChangedEventHandler ComboChangedEvent;

        public TouchComboBox () {
            Visible = true;
            VisibleWindow = false;

            comboList = new List<string> ();
            activeIndex = -1;
            listDropdown = false;
            secondClick = false;
            highlighted = 0;
            height = 30;
            listOffset = 0;
            maxListHeight = 6;

            SetSizeRequest (175, height + 2);

            ExposeEvent += OnExpose;
            ButtonPressEvent += OnComboBoxPressed;
            ButtonReleaseEvent += OnComboBoxReleased;
            ScrollEvent += OnScollEvent;
        }

        public TouchComboBox (string[] names) : this () {
            for (int i = 0; i < names.Length; ++i) {
                comboList.Add (names[i]);
            }
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                int left = Allocation.Left + 1;
                int top = Allocation.Top;
                int width = Allocation.Width - 2;

                if (listDropdown) {
                    int listHeight;

                    if (comboList.Count > maxListHeight) {
                        listHeight = (maxListHeight + 1) * height;
                        includeScrollBar = true;
                    } else if (comboList.Count > 0) {
                        listHeight = (comboList.Count + 1) * height;
                    } else {
                        listHeight = 2 * height;
                    }

                    HeightRequest = listHeight + 2;

                    int radius = height / 2;
                    cr.MoveTo (left, top + radius);
                    cr.Arc (left + radius, top + radius, radius, Math.PI, -Math.PI / 2);
                    cr.LineTo (left + width - radius, top);
                    cr.Arc (left + width - radius, top + radius, radius, -Math.PI / 2, 0);
                    cr.LineTo (left + width, top + listHeight);
                    cr.LineTo (left, top + listHeight);
                    cr.ClosePath ();
                    TouchColor.SetSource (cr, "grey4");
                    cr.FillPreserve ();
                    TouchColor.SetSource (cr, "black");
                    cr.LineWidth = 1;
                    cr.Stroke ();

                    DrawDownButton (cr, left, top, width);

                    if (includeScrollBar) {
                        if (listOffset + maxListHeight > comboList.Count) {
                            listOffset = 0;
                        }

                        int x = left + width - height;
                        listHeight -= height;

                        cr.Rectangle (x, top + height, height, listHeight);
                        TouchColor.SetSource (cr, "grey3");
                        cr.Fill ();

                        cr.Rectangle (x, top + height, height, height);
                        TouchColor buttonColor = "grey1";
                        var outlineColor = new TouchColor (buttonColor);
                        outlineColor.ModifyColor (0.5);
                        var highlightColor = new TouchColor (buttonColor);
                        highlightColor.ModifyColor (1.4);
                        var lowlightColor = new TouchColor (buttonColor);
                        lowlightColor.ModifyColor (0.75);
                        using (var grad = new LinearGradient (x, top + height, x, top + 2 * height)) {
                            grad.AddColorStop (0.0, buttonColor.ToCairoColor ());
                            grad.AddColorStop (0.85, lowlightColor.ToCairoColor ());
                            cr.SetSource (grad);
                            cr.FillPreserve ();
                        }
                        outlineColor.SetSource (cr);
                        cr.LineWidth = 1;
                        cr.Stroke ();

                        var buttonTop = top + listHeight;
                        cr.Rectangle (x, buttonTop, height, height);
                        using (var grad = new LinearGradient (x, buttonTop, x, buttonTop + height)) {
                            grad.AddColorStop (0, highlightColor.ToCairoColor ());
                            grad.AddColorStop (0.2, buttonColor.ToCairoColor ());
                            grad.AddColorStop (0.85, lowlightColor.ToCairoColor ());
                            cr.SetSource (grad);
                            cr.FillPreserve ();
                        }
                        outlineColor.SetSource (cr);
                        cr.LineWidth = 1;
                        cr.Stroke ();

                        scrollBarHeight = (listHeight - 2 * height) / comboList.Count;
                        var scrollBarActualHeight = scrollBarHeight * maxListHeight;
                        var scrollBarTop = top + (2 * height) + (scrollBarHeight * listOffset);

                        TouchGlobal.DrawRoundedRectangle (cr, x, scrollBarTop, height, scrollBarActualHeight, height / 3);
                        buttonColor = "grey2";
                        outlineColor = new TouchColor (buttonColor);
                        outlineColor.ModifyColor (0.5);
                        highlightColor = new TouchColor (buttonColor);
                        highlightColor.ModifyColor (1.3);
                        lowlightColor = new TouchColor (buttonColor);
                        lowlightColor.ModifyColor (0.75);
                        using (var grad = new LinearGradient (x, scrollBarTop, x, scrollBarTop + scrollBarActualHeight)) {
                            grad.AddColorStop (0, highlightColor.ToCairoColor ());
                            grad.AddColorStop (0.1, buttonColor.ToCairoColor ());
                            grad.AddColorStop (0.85, lowlightColor.ToCairoColor ());
                            cr.SetSource (grad);
                            cr.FillPreserve ();
                        }
                        outlineColor.SetSource (cr);
                        cr.LineWidth = 1;
                        cr.Stroke ();

                        int triOffset = 7;
                        int triSize = height - 12;
                        int y = top + height + triOffset + triSize;
                        x += (triOffset - 1);

                        cr.MoveTo (x, y);
                        cr.LineTo (x + triSize, y);
                        cr.LineTo (x + (triSize / 2), y - triSize);
                        cr.ClosePath ();
                        if (scrollBarUpClicked) {
                            TouchColor.SetSource (cr, "pri");
                        } else {
                            TouchColor.SetSource (cr, "grey2");
                        }
                        cr.Fill ();

                        y = top + listHeight + triOffset;
                        cr.MoveTo (x, y);
                        cr.LineTo (x + triSize, y);
                        cr.LineTo (x + triSize / 2, y + triSize);
                        cr.ClosePath ();
                        if (scrollBarDownClicked) {
                            TouchColor.SetSource (cr, "pri");
                        } else {
                            TouchColor.SetSource (cr, "grey2");
                        }
                        cr.Fill ();
                    }

                    if (highlighted != -1) {
                        var highlightedWidth = width - 2;
                        if (includeScrollBar) {
                            highlightedWidth -= height;
                        }
                        var y = top + height + (height * highlighted);
                        cr.Rectangle (left + 1, y + 1, highlightedWidth, height - 2);
                        TouchColor.SetSource (cr, "pri");
                        cr.Fill ();
                    }

                    var textRender = new TouchText ();
                    textRender.font.color = "black";
                    if (includeScrollBar) {
                        for (int i = 0; i < maxListHeight; ++i) {
                            textRender.text = comboList[i + listOffset];
                            int y = top + height + 6 + (height * i);
                            textRender.Render (this, left + 10, y, width - height);
                        }
                    } else {
                        for (int i = 0; i < comboList.Count; ++i) {
                            textRender.text = comboList[i];
                            int y = top + height + 6 + (height * i);
                            textRender.Render (this, left + 10, y, width - height);
                        }
                    }
                } else {
                    HeightRequest = height;

                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width - 2, height, height / 2);
                    TouchColor.SetSource (cr, "grey4");
                    cr.FillPreserve ();
                    cr.LineWidth = 1;
                    TouchColor.SetSource (cr, "black");
                    cr.Stroke ();

                    DrawDownButton (cr, left, top, width);
                }

                bool writeStringCond1 = nonActiveMessage.IsNotEmpty () && (activeIndex == -1);
                bool writeStringCond2 = (comboList.Count > 0) && (activeIndex >= 0);

                if (writeStringCond1 || writeStringCond2) {
                    string text = writeStringCond1 ? nonActiveMessage : comboList[activeIndex];
                    var t = new TouchText (text);
                    t.textWrap = TouchTextWrap.Shrink;
                    t.font.color = "black";
                    var w = width - height - 10;
                    t.Render (this, left + 10, top, w, height);
                }
            }
        }

        void DrawDownButton (Context cr, int left, int top, int width) {
            var x = left + (width - height);
            var radius = height / 2;
            cr.MoveTo (x, top);
            cr.LineTo (x + radius, top);
            if (listDropdown) {
                cr.Arc (x + radius, top + radius, radius, -Math.PI / 2, 0);
                cr.LineTo (x + height, top + height);
            } else {
                cr.Arc (x + radius, top + radius, radius, -Math.PI / 2, Math.PI / 2);
            }
            cr.LineTo (x, top + height);
            cr.ClosePath ();

            //TouchColor backgroundColor = listDropdown ? "grey3" : "grey1";
            TouchColor backgroundColor = "grey1";
            var outlineColor = new TouchColor (backgroundColor);
            outlineColor.ModifyColor (0.5);
            var highlightColor = new TouchColor (backgroundColor);
            highlightColor.ModifyColor (1.4);
            var lowlightColor = new TouchColor (backgroundColor);
            lowlightColor.ModifyColor (0.75);

            using (var grad = new LinearGradient (x, top, x, top + height)) {
                grad.AddColorStop (0, highlightColor.ToCairoColor ());
                grad.AddColorStop (0.2, backgroundColor.ToCairoColor ());
                if (!(listDropdown && includeScrollBar)) {
                    grad.AddColorStop (0.85, lowlightColor.ToCairoColor ());
                }
                cr.SetSource (grad);
                cr.FillPreserve ();
            }

            outlineColor.SetSource (cr);
            cr.LineWidth = 1;
            cr.Stroke ();

            int triOffset = 7;
            int triSize = height - 12;
            int y = top + triOffset;
            if (listDropdown) {
                x += height / 2;
                cr.MoveTo (x, y);
                cr.LineTo (x + triSize / 2, y + triSize);
                cr.LineTo (x - triSize / 2, y + triSize);
            } else {
                x += (triOffset - 3);
                cr.MoveTo (x, y);
                cr.LineTo (x + triSize, y);
                cr.LineTo (x + triSize / 2, y + triSize);
            }

            cr.ClosePath ();
            TouchColor.SetSource (cr, "seca");
            cr.Fill ();
        }

        protected void OnComboBoxPressed (object o, ButtonPressEventArgs args) {
            if (listDropdown) {
                int x = (int)args.Event.X;
                int y = (int)args.Event.Y;
                bool pressWithinScroll = ((x >= (Allocation.Width - height)) && (x <= Allocation.Width));

                if (includeScrollBar && pressWithinScroll) {
                    if ((y >= 0) && (y <= height)) {
                        secondClick = true;
                    } else if ((y >= height) && (y <= (2 * height))) {
                        scrollBarUpClicked = true;
                    } else if ((y >= (Allocation.Height - height)) && (y <= Allocation.Height)) {
                        scrollBarDownClicked = true;
                    } else {
                        scrollBarClicked = true;
                        clickY = y;
#if RPI_BUILD
                        GLib.Timeout.Add (20, OnTimerEvent);
#endif
                    }
                } else {
                    secondClick = true;
                }
            } else {
                secondClick = false;
            }

#if !RPI_BUILD
            GLib.Timeout.Add (20, OnTimerEvent);
#endif
            listDropdown = true;
            highlighted = activeIndex;
            QueueDraw ();
        }

        protected void OnComboBoxReleased (object o, ButtonReleaseEventArgs args) {
            int x = (int)args.Event.X;
            int y = (int)args.Event.Y;
            int rightXBounds;
            int maxLoop;

            if (includeScrollBar) {
                if (scrollBarClicked) {
                    if (!scrollBarMoved) {
                        if ((y >= (2 * height)) && (y <= (2 * height + (listOffset * scrollBarHeight)))) {
                            --listOffset;

                            if (listOffset < 0) {
                                listOffset = 0;
                            }
                        } else if ((y >= (2 * height) + (listOffset * scrollBarHeight) + (maxListHeight * scrollBarHeight)) && (y <= (Allocation.Height - height))) {
                            ++listOffset;

                            if ((listOffset + maxListHeight) > comboList.Count) {
                                listOffset = comboList.Count - maxListHeight;
                            }
                        }
                    }

                    scrollBarMoved = false;
                    scrollBarClicked = false;
                    return;
                }

                rightXBounds = Allocation.Width - height;
                maxLoop = maxListHeight;

                if ((x >= rightXBounds) && (x <= Allocation.Width)) {
                    if ((y >= height) && (y <= (2 * height))) {
                        --listOffset;

                        if (listOffset < 0) {
                            listOffset = 0;
                        }
                    } else if ((y >= (Allocation.Height - height)) && (y <= Allocation.Height)) {
                        ++listOffset;

                        if ((listOffset + maxListHeight) > comboList.Count) {
                            listOffset = comboList.Count - maxListHeight;
                        }
                    }

                    scrollBarUpClicked = false;
                    scrollBarDownClicked = false;
                }
            } else {
                rightXBounds = Allocation.Width;
                maxLoop = comboList.Count;
            }

            if ((x >= 0) && (x <= rightXBounds)) {
                int top = Allocation.Top;

                for (int i = 0; i < maxLoop; ++i) {
                    int topWindow = i * height + 30;
                    int bottomWindow = (i + 1) * height + 30;
                    if ((y >= topWindow) && (y <= bottomWindow)) {
                        var previousIndex = activeIndex;
                        if (includeScrollBar) {
                            activeIndex = i + listOffset;
                        } else {
                            activeIndex = i;
                        }

                        var newIndex = activeIndex;
                        var comboChangedEventArgs = new ComboBoxChangedEventArgs (activeIndex, comboList[activeIndex]);

                        ComboChangedEvent?.Invoke(this, comboChangedEventArgs);

                        if (!comboChangedEventArgs.keepChange || (newIndex != activeIndex)) {
                            activeIndex = previousIndex;
                        }

                        break;
                    }
                }
            }

            if (secondClick) {
                listDropdown = false;
                QueueDraw ();
            }
        }

#if !RPI_BUILD
        protected bool OnTimerEvent () {
            if (listDropdown) {
                int x, y;
                GetPointer (out x, out y);

                if (scrollBarClicked) {
                    HandleScrollBarClick (x, y);
                }

                if ((x >= 0) && (x <= Allocation.Width)) {
                    int maxLoop;
                    if (includeScrollBar) {
                        maxLoop = maxListHeight;
                    } else {
                        maxLoop = comboList.Count;
                    }

                    for (int i = 0; i < maxLoop; ++i) {
                        int topWindow = i * height + 25;
                        int bottomWindow = (i + 1) * height + 25;
                        if ((y >= topWindow) && (y <= bottomWindow)) {
                            highlighted = i;
                            QueueDraw ();
                            break;
                        }
                    }
                }
            }

            return listDropdown;
        }
#endif

#if RPI_BUILD
        protected bool OnTimerEvent () {
            if (scrollBarClicked) {
                int x, y;
                GetPointer (out x, out y);
                HandleScrollBarClick (x, y);
            }

            return scrollBarClicked;
        }
#endif

        protected void HandleScrollBarClick (int x, int y) {
            if (Math.Abs (y - clickY) > scrollBarHeight) {
                scrollBarMoved = true;
                listOffset = (y - clickY) / scrollBarHeight;

                if (listOffset < 0) {
                    listOffset = 0;
                } else if ((listOffset + maxListHeight) > comboList.Count) {
                    listOffset = comboList.Count - maxListHeight;
                }
                QueueDraw ();
            }
        }

        protected void OnScollEvent (object sender, ScrollEventArgs args) {
            if (args.Event.Direction == Gdk.ScrollDirection.Down) {
                ++listOffset;
            } else if (args.Event.Direction == Gdk.ScrollDirection.Up) {
                --listOffset;
            }

            if (listOffset < 0) {
                listOffset = 0;
            } else if ((listOffset + maxListHeight) > comboList.Count) {
                listOffset = comboList.Count - maxListHeight;
            }

            QueueDraw ();
        }
    }
}

