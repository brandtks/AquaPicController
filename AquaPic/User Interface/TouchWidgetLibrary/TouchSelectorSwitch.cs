using System;
using Gtk;
using Cairo;

namespace TouchWidgetLibrary
{
    public delegate void SelectorChangedEventHandler (object sender, SelectorChangedEventArgs args);

    public class SelectorChangedEventArgs : EventArgs
    {
        public int currentSelectedIndex;
        public byte id;

        public SelectorChangedEventArgs (int currentSelectedIndex, byte id) {
            this.currentSelectedIndex = currentSelectedIndex;
            this.id = id;
        }
    }

    public enum MySliderSize : byte {
        Small = 1,
        Large
    }

    public class TouchSelectorSwitch : EventBox
    {
        private bool clicked;
        private uint clickTimer;
        private int click1, click2;

        public int SelectionCount;
        public int CurrentSelected;
        public TouchOrientation Orientation;
        public MySliderSize SliderSize;
        public TouchColor[] BkgndColorOptions;
        public TouchColor[] TextColorOptions;
        public TouchColor[] SliderColorOptions;
        public string[] TextOptions;
        public byte Id;

        public event SelectorChangedEventHandler SelectorChangedEvent;

        public TouchSelectorSwitch (int id, int selectionCount, int currentSelectedIndex, TouchOrientation orientation) {
            this.Visible = true;
            this.VisibleWindow = false;

            this.Id = (byte)id;
            this.SelectionCount = selectionCount;
            this.CurrentSelected = currentSelectedIndex;
            this.Orientation = orientation;
            this.SliderSize = MySliderSize.Large;

            this.BkgndColorOptions = new TouchColor[this.SelectionCount];
            for (int i = 0; i < BkgndColorOptions.Length; ++i)
                this.BkgndColorOptions [i] = new TouchColor ("grey0");

            this.TextColorOptions = new TouchColor[this.SelectionCount];
            for (int i = 0; i < BkgndColorOptions.Length; ++i)
                this.TextColorOptions [i] = new TouchColor ("white");

            this.SliderColorOptions = new TouchColor[this.SelectionCount];
            for (int i = 0; i < SliderColorOptions.Length; ++i)
                this.SliderColorOptions [i] = new TouchColor ("grey4");

            this.TextOptions = new string[this.SelectionCount];
            for (int i = 0; i < TextOptions.Length; ++i)
                this.TextOptions [i] = string.Empty;

            this.clicked = false;
            this.clickTimer = 0;
            this.click1 = 0;
            this.click2 = 0;

            if (this.Orientation == TouchOrientation.Horizontal) {
                this.WidthRequest = 80;
                this.HeightRequest = 20;
            } else {
                this.WidthRequest = 20;
                this.HeightRequest = 80;
            }

            this.ExposeEvent += OnExpose;
            this.ButtonPressEvent += OnSelectorPress;
            this.ButtonReleaseEvent += OnSelectorRelease;
        }

        public TouchSelectorSwitch () : this (0, 2, 0, TouchOrientation.Horizontal) { }

        public TouchSelectorSwitch (int id) : this (id, 2, 0, TouchOrientation.Horizontal) { }

        public void AddSelectedColorOption (int selectionIndex, string newColor) {
            BkgndColorOptions [selectionIndex] = newColor;
        }

        public void AddSelectedColorOption (int selectionIndex, double R, double G, double B) {
            BkgndColorOptions [selectionIndex] = new TouchColor (R, G, B);
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int height = Allocation.Height;
                int width = Allocation.Width;
                int top = Allocation.Top;
                int left = Allocation.Left;

                int seperation, sliderSize, sliderLength, sliderMax, x, y;

                if (Orientation == TouchOrientation.Horizontal) {  
                    if (SliderSize == MySliderSize.Small)
                        sliderSize = height;
                    else {
                        sliderSize = width / SelectionCount;
                        sliderSize += (SelectionCount - 2) * 8;
                    }

                    sliderLength = width - sliderSize;
                    sliderMax = left + sliderLength;

                    seperation = sliderLength / (SelectionCount - 1);

                    seperation *= CurrentSelected;

                    if (clicked)
                        seperation += (click2 - click1);

                    x = left + seperation;
                    if (x < left)
                        x = left;
                    if (x > sliderMax)
                        x = sliderMax;
                    y = top;
                } else {
                    if (SliderSize == MySliderSize.Small)
                        sliderSize = width;
                    else {
                        sliderSize = height / SelectionCount;
                        sliderSize += (SelectionCount - 2) * 8;
                    }

                    sliderLength = height - sliderSize;
                    sliderMax = top + sliderLength;

                    seperation = sliderLength / (SelectionCount - 1);

                    seperation *= CurrentSelected;

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
                if (Orientation == TouchOrientation.Horizontal)
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, height / 2);
                else
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, width / 2);
                BkgndColorOptions [CurrentSelected].SetSource (cr);
                cr.FillPreserve ();
                cr.LineWidth = 1;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                // Slider
                if (Orientation == TouchOrientation.Horizontal)
                    TouchGlobal.DrawRoundedRectangle (cr, x, y, sliderSize, height, height / 2);
                else
                    TouchGlobal.DrawRoundedRectangle (cr, x, y, width, sliderSize, width / 2);
                SliderColorOptions [CurrentSelected].SetSource (cr);
                cr.FillPreserve ();
                cr.LineWidth = 1;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                // Text Labels
                TouchText render = new TouchText ();
                render.textWrap = TouchTextWrap.Shrink;
                render.alignment = TouchAlignment.Center;

                seperation = Allocation.Width / SelectionCount;
                x = Allocation.Left;
                for (int i = 0; i < SelectionCount; ++i) {
                    if (!string.IsNullOrWhiteSpace (TextOptions [i])) {
                        render.font.color = TextColorOptions [i];
                        render.text = TextOptions [i];
                        render.Render (this, x, Allocation.Top + 6, seperation);
                    }

                    x += seperation;
                }
            }
        }

        protected void OnSelectorPress (object o, ButtonPressEventArgs args) {
            clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
            clicked = true;

            if (Orientation == TouchOrientation.Horizontal)
                click1 = (int)args.Event.X;
            else
                click1 = (int)args.Event.Y;
        }

        protected void OnSelectorRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;

            if (Orientation == TouchOrientation.Horizontal) {
                int x = (int)args.Event.X;
                int sliderLength = Allocation.Width;
                int seperation = sliderLength / SelectionCount;

                if (x < 0)
                    CurrentSelected = 0;
                else if (x > sliderLength)
                    CurrentSelected = SelectionCount - 1;
                else {
                    for (int i = 0; i < SelectionCount; ++i) {
                        int leftBoundery = i * seperation;
                        int rightBoundery = (i + 1) * seperation;
                        if ((x >= leftBoundery) && (x <= rightBoundery)) {
                            CurrentSelected = i;
                            break;
                        }
                    }
                }
            } else {
                int y = (int)args.Event.Y;
                int sliderSize = Allocation.Width;
                int sliderLength = Allocation.Height - sliderSize;
                int sliderMax = Allocation.Height;
                int seperation = sliderLength / (SelectionCount - 1);

                if (y < 0)
                    CurrentSelected = 0;
                else if (y > sliderMax)
                    CurrentSelected = SelectionCount - 1;
                else {
                    for (int i = 0; i < SelectionCount; ++i) {
                        int leftBoundery = i * seperation;
                        int rightBoundery = (i + 1) * seperation;
                        if ((y >= leftBoundery) && (y <= rightBoundery)) {
                            CurrentSelected = i;
                            break;
                        }
                    }
                }
            }

            QueueDraw ();

            if (SelectorChangedEvent != null)
                SelectorChangedEvent (this, new SelectorChangedEventArgs (CurrentSelected, Id));
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);
                if (Orientation == TouchOrientation.Horizontal)
                    click2 = x;
                else
                    click2 = y;

                QueueDraw ();
            }

            return clicked;
        }
    }
}