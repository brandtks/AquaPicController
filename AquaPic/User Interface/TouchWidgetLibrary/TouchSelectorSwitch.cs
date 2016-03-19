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

    public enum MySliderSize {
        Small = 1,
        Large
    }

    public class TouchSelectorSwitch : EventBox
    {
        private bool clicked;
        private uint clickTimer;
        private int click1, click2;

        private int _selectionCount;
        public int selectionCount {
            get {
                return _selectionCount;
            }
        }
        public int currentSelected;
        public TouchOrientation orientation;
        public MySliderSize sliderSize;
        public TouchColor[] backgoundColorOptions;
        public TouchColor[] textColorOptions;
        public TouchColor[] sliderColorOptions;
        public string[] textOptions;
        public byte id;

        public event SelectorChangedEventHandler SelectorChangedEvent;

        public TouchSelectorSwitch (int id, int selectionCount, int currentSelectedIndex, TouchOrientation orientation) {
            this.Visible = true;
            this.VisibleWindow = false;

            this.id = (byte)id;
            _selectionCount = selectionCount;
            this.currentSelected = currentSelectedIndex;
            this.orientation = orientation;
            this.sliderSize = MySliderSize.Large;

            this.backgoundColorOptions = new TouchColor[_selectionCount];
            for (int i = 0; i < backgoundColorOptions.Length; ++i)
                this.backgoundColorOptions [i] = new TouchColor ("grey0");

            this.textColorOptions = new TouchColor[_selectionCount];
            for (int i = 0; i < backgoundColorOptions.Length; ++i)
                this.textColorOptions [i] = new TouchColor ("white");

            this.sliderColorOptions = new TouchColor[_selectionCount];
            for (int i = 0; i < sliderColorOptions.Length; ++i)
                this.sliderColorOptions [i] = new TouchColor ("grey4");

            this.textOptions = new string[_selectionCount];
            for (int i = 0; i < textOptions.Length; ++i)
                this.textOptions [i] = string.Empty;

            this.clicked = false;
            this.clickTimer = 0;
            this.click1 = 0;
            this.click2 = 0;

            if (this.orientation == TouchOrientation.Horizontal) {
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

        public TouchSelectorSwitch (int selectionCount) : this (0, selectionCount, 0, TouchOrientation.Horizontal) { }

        public TouchSelectorSwitch (int id, int selectionCount) : this (id, selectionCount, 0, TouchOrientation.Horizontal) { }

        public void AddSelectedColorOption (int selectionIndex, string newColor) {
            backgoundColorOptions [selectionIndex] = newColor;
        }

        public void AddSelectedColorOption (int selectionIndex, double R, double G, double B) {
            backgoundColorOptions [selectionIndex] = new TouchColor (R, G, B);
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
                        sliderWidth = width / _selectionCount;
                        sliderWidth += (_selectionCount - 2) * 8;
                    }

                    sliderLength = width - sliderWidth;
                    sliderMax = left + sliderLength;

                    seperation = sliderLength / (_selectionCount - 1);

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
                        sliderWidth = height / _selectionCount;
                        sliderWidth += (_selectionCount - 2) * 8;
                    }

                    sliderLength = height - sliderWidth;
                    sliderMax = top + sliderLength;

                    seperation = sliderLength / (_selectionCount - 1);

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
                if (orientation == TouchOrientation.Horizontal)
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, height / 2);
                else
                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, width / 2);
                backgoundColorOptions [currentSelected].SetSource (cr);
                cr.FillPreserve ();
                cr.LineWidth = 1;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                // Slider
                if (orientation == TouchOrientation.Horizontal)
                    TouchGlobal.DrawRoundedRectangle (cr, x, y, sliderWidth, height, height / 2);
                else
                    TouchGlobal.DrawRoundedRectangle (cr, x, y, width, sliderWidth, width / 2);
                sliderColorOptions [currentSelected].SetSource (cr);
                cr.FillPreserve ();
                cr.LineWidth = 1;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                // Text Labels
                TouchText render = new TouchText ();
                render.textWrap = TouchTextWrap.Shrink;
                render.alignment = TouchAlignment.Center;

                seperation = Allocation.Width / _selectionCount;
                x = Allocation.Left;
                for (int i = 0; i < _selectionCount; ++i) {
                    if (!string.IsNullOrWhiteSpace (textOptions [i])) {
                        render.font.color = textColorOptions [i];
                        render.text = textOptions [i];
                        render.Render (this, x, Allocation.Top + 6, seperation);
                    }

                    x += seperation;
                }
            }
        }

        protected void OnSelectorPress (object o, ButtonPressEventArgs args) {
            clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
            clicked = true;

            if (orientation == TouchOrientation.Horizontal)
                click1 = (int)args.Event.X;
            else
                click1 = (int)args.Event.Y;
        }

        protected void OnSelectorRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;

            if (orientation == TouchOrientation.Horizontal) {
                int x = (int)args.Event.X;
                int sliderLength = Allocation.Width;
                int seperation = sliderLength / _selectionCount;

                if (x < 0) {
                    currentSelected = 0;
                } else if (x > sliderLength) {
                    currentSelected = _selectionCount - 1;
                } else {
                    for (int i = 0; i < _selectionCount; ++i) {
                        int leftBoundery = i * seperation;
                        int rightBoundery = (i + 1) * seperation;
                        if ((x >= leftBoundery) && (x <= rightBoundery)) {
                            currentSelected = i;
                            break;
                        }
                    }
                }
            } else {
                int y = (int)args.Event.Y;
                int sliderSize = Allocation.Width;
                int sliderLength = Allocation.Height - sliderSize;
                int sliderMax = Allocation.Height;
                int seperation = sliderLength / (_selectionCount - 1);

                if (y < 0)
                    currentSelected = 0;
                else if (y > sliderMax)
                    currentSelected = _selectionCount - 1;
                else {
                    for (int i = 0; i < _selectionCount; ++i) {
                        int leftBoundery = i * seperation;
                        int rightBoundery = (i + 1) * seperation;
                        if ((y >= leftBoundery) && (y <= rightBoundery)) {
                            currentSelected = i;
                            break;
                        }
                    }
                }
            }

            QueueDraw ();

            if (SelectorChangedEvent != null)
                SelectorChangedEvent (this, new SelectorChangedEventArgs (currentSelected, id));
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);
                if (orientation == TouchOrientation.Horizontal)
                    click2 = x;
                else
                    click2 = y;

                QueueDraw ();
            }

            return clicked;
        }
    }
}