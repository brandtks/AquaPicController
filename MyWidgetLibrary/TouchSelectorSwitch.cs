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

    public class TouchSelectorSwitch : EventBox
    {
        private bool clicked;
        private uint clickTimer;
        private int click1, click2;

        public int SelectionCount;
        public int CurrentSelected;
        public MyOrientation Orientation;
        public MyColor[] BkgndColorOptions;
        public MyColor[] TextColorOptions;
        public string[] NameOptions;
        public byte Id;

        public event SelectorChangedEventHandler SelectorChanged;

        public TouchSelectorSwitch (int id, int selectionCount, int currentSelectedIndex, MyOrientation orientation) {
            this.Visible = true;
            this.VisibleWindow = false;

            this.Id = (byte)id;
            this.SelectionCount = selectionCount;
            this.CurrentSelected = currentSelectedIndex;
            this.Orientation = orientation;
            this.BkgndColorOptions = new MyColor[this.SelectionCount];
            for (int i = 0; i < BkgndColorOptions.Length; ++i)
                this.BkgndColorOptions [i] = new MyColor (0.15, 0.15, 0.15);

            this.TextColorOptions = new MyColor[this.SelectionCount];
            for (int i = 0; i < BkgndColorOptions.Length; ++i)
                this.TextColorOptions [i] = new MyColor ("black");

            this.NameOptions = new string[this.SelectionCount];
            for (int i = 0; i < NameOptions.Length; ++i)
                this.NameOptions [i] = string.Empty;

            this.clicked = false;
            this.clickTimer = 0;
            this.click1 = 0;
            this.click2 = 0;

            if (this.Orientation == MyOrientation.Horizontal) {
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

        public TouchSelectorSwitch () : this (0, 2, 0, MyOrientation.Horizontal) { }

        public TouchSelectorSwitch (int id) : this (id, 2, 0, MyOrientation.Horizontal) { }

        public void AddSelectedColorOption (int selectionIndex, string newColor) {
            BkgndColorOptions [selectionIndex].ChangeColor (newColor);
        }

        public void AddSelectedColorOption (int selectionIndex, double R, double G, double B) {
            BkgndColorOptions [selectionIndex].ChangeColor (R, G, B);
        }

        public void AddSelectedTextColorOption (int selectionIndex, string colorName) {
            TextColorOptions [selectionIndex].ChangeColor (colorName);
        }

        public void AddSelectedNameOption (int selectionIndex, string name) {
            NameOptions [selectionIndex] = name;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int height = Allocation.Height;
                int width = Allocation.Width;
                int top = Allocation.Top;
                int left = Allocation.Left;

                int seperation, sliderSize, sliderLength, sliderMax, x, y;

                if (Orientation == MyOrientation.Horizontal) {
                    sliderSize = height;
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
                    sliderSize = width;
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
                //cr.Rectangle (left, top, width, height);
                WidgetGlobal.DrawRoundedRectangle (cr, left, top, width, height, 10);
                cr.SetSourceRGB (BkgndColorOptions [CurrentSelected].R, BkgndColorOptions [CurrentSelected].G, BkgndColorOptions [CurrentSelected].B);
                cr.FillPreserve ();
                cr.LineWidth = 1;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                // Slider
                //cr.Rectangle (x, y, sliderSize, sliderSize);
                WidgetGlobal.DrawRoundedRectangle (cr, x, y, sliderSize, sliderSize, 10);
                cr.SetSourceRGB (0.85, 0.85, 0.85);
                cr.FillPreserve ();
                cr.LineWidth = 1;
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.Stroke ();

                if (!clicked) {
                    if ((SelectionCount == 2) && (Orientation == MyOrientation.Horizontal)) {
                        if (!string.IsNullOrWhiteSpace (NameOptions [CurrentSelected])) {
                            Pango.Layout l = new Pango.Layout (PangoContext);
                            l.Width = Pango.Units.FromPixels (sliderLength);
                            l.Wrap = Pango.WrapMode.WordChar;
                            l.Alignment = Pango.Alignment.Center;
                            if (CurrentSelected == 0)
                                x += sliderSize;
                            else
                                x = left;
                            l.SetMarkup ("<span color=" + (char)34 + TextColorOptions [CurrentSelected].ToHTML () + (char)34 + ">" + NameOptions [CurrentSelected] + "</span>"); 
                            l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");
                            GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), x, top + 1, l);
                            l.Dispose ();
                        }
                    }
                }
            }
        }

        protected void onSelectorPress (object o, ButtonPressEventArgs args) {
            clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
            clicked = true;

            if (Orientation == MyOrientation.Horizontal)
                click1 = (int)args.Event.X;
            else
                click1 = (int)args.Event.Y;
        }

        protected void onSelectorRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;

            if (Orientation == MyOrientation.Horizontal) {
                int x = (int)args.Event.X;

                if ((x < (click1 + 25) && (x > (click1 - 25))))
                    CurrentSelected = ++CurrentSelected % SelectionCount;
                else {
                    int sliderSize = Allocation.Height;
                    int sliderLength = Allocation.Width - sliderSize;
                    int sliderMax = Allocation.Left + sliderLength;
                    int seperation = sliderLength / (SelectionCount - 1);

                    if (x < 0)
                        CurrentSelected = 0;
                    else if (x > sliderMax)
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
                }
            } else {
                int y = (int)args.Event.Y;

                if ((y < (click1 + 25) && (y > (click1 - 25))))
                    CurrentSelected = ++CurrentSelected % SelectionCount;
                else {
                    int sliderSize = Allocation.Width;
                    int sliderLength = Allocation.Height - sliderSize;
                    int sliderMax = Allocation.Top + sliderLength;
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
            }

            QueueDraw ();

            if (SelectorChanged != null)
                SelectorChanged (this, new SelectorChangedEventArgs (CurrentSelected, Id));
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);
                if (Orientation == MyOrientation.Horizontal)
                    click2 = x;
                else
                    click2 = y;

                QueueDraw ();
            }

            return clicked;
        }
    }




}

