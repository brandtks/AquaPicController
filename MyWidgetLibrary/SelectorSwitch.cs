using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
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

    public enum MyOrientation : byte {
        Vertical = 1,
        Horizontal
    }

    public class SelectorSwitch : EventBox
    {
        private bool clicked;
        private uint clickTimer;
        private int click1, click2;

        public int selectionCount;
        public int currentSelected;
        public MyOrientation orientation;
        public Colors[] colorOptions;
        public string[] textColorOptions;
        public string[] nameOptions;
        public byte id;

        public event SelectorChangedEventHandler SelectorChanged;

        public SelectorSwitch (int id, int selectionCount, int currentSelectedIndex, MyOrientation orientation) {
            this.Visible = true;
            this.VisibleWindow = false;

            this.id = (byte)id;
            this.selectionCount = selectionCount;
            this.currentSelected = currentSelectedIndex;
            this.orientation = orientation;
            this.colorOptions = new Colors[this.selectionCount];
            for (int i = 0; i < colorOptions.Length; ++i)
                this.colorOptions [i] = new Colors (0.15, 0.15, 0.15);

            this.textColorOptions = new string[this.selectionCount];
            for (int i = 0; i < colorOptions.Length; ++i)
                this.textColorOptions [i] = "black";

            this.nameOptions = new string[this.selectionCount];
            for (int i = 0; i < nameOptions.Length; ++i)
                this.nameOptions [i] = null;

            this.clicked = false;
            this.clickTimer = 0;
            this.click1 = 0;
            this.click2 = 0;

            if (this.orientation == MyOrientation.Horizontal) {
                this.WidthRequest = 80;
                this.HeightRequest = 20;
            } else {
                this.WidthRequest = 20;
                this.HeightRequest = 80;
            }

            this.ExposeEvent += onExpose;
            this.ButtonPressEvent += onSelectorPress;
            this.ButtonReleaseEvent += onSelectorRelease;
        }

        public SelectorSwitch () : this (0, 2, 0, MyOrientation.Horizontal) { }

        public SelectorSwitch (int id) : this (id, 2, 0, MyOrientation.Horizontal) { }

        public void AddSelectedColorOption (int selectionIndex, string newColor) {
            colorOptions [selectionIndex].ChangeColor (newColor);
        }

        public void AddSelectedColorOption (int selectionIndex, double R, double G, double B) {
            colorOptions [selectionIndex].ChangeColor (R, G, B);
        }

        public void AddSelectedTextColorOption (int selectionIndex, string colorName) {
            textColorOptions [selectionIndex] = colorName;
        }

        public void AddSelectedNameOption (int selectionIndex, string name) {
            nameOptions [selectionIndex] = name;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int height = Allocation.Height;
                int width = Allocation.Width;
                int top = Allocation.Top;
                int left = Allocation.Left;

                int seperation, sliderSize, sliderLength, sliderMax, x, y;

                if (orientation == MyOrientation.Horizontal) {
                    sliderSize = height;
                    sliderLength = width - sliderSize;
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
                    sliderSize = width;
                    sliderLength = height - sliderSize;
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

                //cr.Rectangle (left, top, width, height);
                WidgetGlobal.DrawRoundedRectangle (cr, left, top, width, height, 10);
                cr.SetSourceRGB (colorOptions [currentSelected].R, colorOptions [currentSelected].G, colorOptions [currentSelected].B);
                cr.FillPreserve ();
                cr.LineWidth = 1;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                //cr.Rectangle (x, y, sliderSize, sliderSize);
                WidgetGlobal.DrawRoundedRectangle (cr, x, y, sliderSize, sliderSize, 10);
                cr.SetSourceRGB (0.85, 0.85, 0.85);
                cr.FillPreserve ();
                cr.LineWidth = 1;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                if (!clicked) {
                    if ((selectionCount == 2) && (orientation == MyOrientation.Horizontal)) {
                        if (!string.IsNullOrWhiteSpace (nameOptions [currentSelected])) {
                            Pango.Layout l = new Pango.Layout (PangoContext);
                            l.Width = Pango.Units.FromPixels (sliderLength);
                            l.Wrap = Pango.WrapMode.WordChar;
                            l.Alignment = Pango.Alignment.Center;
                            if (currentSelected == 0)
                                x += sliderSize;
                            else
                                x = left;
                            l.SetMarkup ("<span color=" + (char)34 + textColorOptions [currentSelected] + (char)34 + ">" + nameOptions [currentSelected] + "</span>"); 
                            l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");
                            GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), x, top + 1, l);
                            l.Dispose ();
                        }
                    }
                }
            }
        }

        protected void onSelectorPress (object o, ButtonPressEventArgs args) {
            this.clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
            clicked = true;

            if (orientation == MyOrientation.Horizontal)
                click1 = (int)args.Event.X;
            else
                click1 = (int)args.Event.Y;
        }

        protected void onSelectorRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;

            if (orientation == MyOrientation.Horizontal) {
                int x = (int)args.Event.X;

                if ((x < (click1 + 25) && (x > (click1 - 25))))
                    currentSelected = ++currentSelected % selectionCount;
                else {
                    int sliderSize = Allocation.Height;
                    int sliderLength = Allocation.Width - sliderSize;
                    int sliderMax = Allocation.Left + sliderLength;
                    int seperation = sliderLength / (selectionCount - 1);

                    if (x < 0)
                        currentSelected = 0;
                    else if (x > sliderMax)
                        currentSelected = selectionCount - 1;
                    else {
                        for (int i = 0; i < selectionCount; ++i) {
                            int leftBoundery = i * seperation;
                            int rightBoundery = (i + 1) * seperation;
                            if ((x >= leftBoundery) && (x <= rightBoundery)) {
                                currentSelected = i;
                                break;
                            }
                        }
                    }
                }
            } else {
                int y = (int)args.Event.Y;

                if ((y < (click1 + 25) && (y > (click1 - 25))))
                    currentSelected = ++currentSelected % selectionCount;
                else {
                    int sliderSize = Allocation.Width;
                    int sliderLength = Allocation.Height - sliderSize;
                    int sliderMax = Allocation.Top + sliderLength;
                    int seperation = sliderLength / (selectionCount - 1);

                    if (y < 0)
                        currentSelected = 0;
                    else if (y > sliderMax)
                        currentSelected = selectionCount - 1;
                    else {
                        for (int i = 0; i < selectionCount; ++i) {
                            int leftBoundery = i * seperation;
                            int rightBoundery = (i + 1) * seperation;
                            if ((y >= leftBoundery) && (y <= rightBoundery)) {
                                currentSelected = i;
                                break;
                            }
                        }
                    }
                }
            }

            QueueDraw ();

            if (SelectorChanged != null)
                SelectorChanged (this, new SelectorChangedEventArgs (currentSelected, id));
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);
                if (orientation == MyOrientation.Horizontal)
                    click2 = x;
                else
                    click2 = y;
            }

            QueueDraw ();
            return clicked;
        }
    }




}

