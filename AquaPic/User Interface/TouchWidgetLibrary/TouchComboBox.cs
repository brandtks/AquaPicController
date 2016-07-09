using System;
using System.Collections.Generic;
using Gtk;
using Cairo;

namespace TouchWidgetLibrary
{
    public delegate void ComboBoxChangedEventHandler (object sender, ComboBoxChangedEventArgs args);

    public class ComboBoxChangedEventArgs : EventArgs
    {
        public int Active;
        public string ActiveText;

        public ComboBoxChangedEventArgs (int active, string activeText) {
            this.Active = active;
            this.ActiveText = activeText;
        }
    }

    public class TouchComboBox : EventBox
    {
        public List<string> comboList;
        public string nonActiveMessage;
        public int active;
        public string activeText {
            get {
                if (active != -1) {
                    return comboList[active];
                } else {
                    return string.Empty;
                }
            }
            set {
                if (comboList.Contains (value)) {
                    for (int i = 0; i < comboList.Count; i++) {
                        if (value == comboList[i]) {
                            active = i;
                            break;
                        }
                    }
                }
            }
        }

        private bool listDropdown;
        private bool secondClick;
        private int highlighted;
        private int height;

        public event ComboBoxChangedEventHandler ChangedEvent;

        public TouchComboBox () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.comboList = new List<string> ();
            this.active = -1;
            this.listDropdown = false;
            secondClick = false;
            this.highlighted = 0;
            this.height = 30;

            this.WidthRequest = 175;
            this.HeightRequest = height + 2;

            this.ExposeEvent += OnExpose;
            this.ButtonPressEvent += OnComboBoxPressed;
            this.ButtonReleaseEvent += OnComboBoxReleased;
        }

        public TouchComboBox (string[] names) : this () {
            for (int i = 0; i < names.Length; ++i)
                comboList.Add (names [i]);
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left + 1;
                int top = Allocation.Top;
                int width = Allocation.Width - 2;

                if (listDropdown) {
                    int listHeight;
                    if (comboList.Count > 0)
                        listHeight = comboList.Count * 30 + height;
                    else
                        listHeight = 30 + height;

                    this.HeightRequest = listHeight + 2;

                    int radius = height / 2;
                    cr.MoveTo (left, top + radius);
                    cr.Arc (left + radius, top + radius, radius, Math.PI, -Math.PI / 2);
                    cr.LineTo (left + width - radius, top);
                    cr.Arc (left + width - radius, top + radius, radius, -Math.PI / 2, 0);
                    cr.LineTo (left + width, top + listHeight);
                    cr.LineTo (left, top + listHeight);
                    cr.ClosePath ();
                    cr.SetSourceRGB (0.85, 0.85, 0.85);
                    cr.FillPreserve ();
                    cr.LineWidth = 0.85;
                    cr.SetSourceRGB (0.0, 0.0, 0.0);
                    cr.Stroke ();

                    DrawDownButton (cr, left, top, width);

                    if (highlighted != -1) {
                        int y = top + height + (height * highlighted);
                        cr.Rectangle (left + 1, y + 1, width - 2, height - 2);
                        TouchColor.SetSource (cr, "pri");
                        cr.Fill ();
                    }

                    TouchText textRender = new TouchText ();
                    textRender.font.color = "black";
                    for (int i = 0; i < comboList.Count; ++i) {
                        textRender.text = comboList [i];
                        int y = top + height + 6 + (height * i);
                        textRender.Render (this, left + 10, y, width - height);
                    }
                } else {
                    this.HeightRequest = 30;

                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width - 2, height, height / 2);
                    cr.SetSourceRGB (0.85, 0.85, 0.85);
                    cr.FillPreserve ();
                    cr.LineWidth = 0.85;
                    cr.SetSourceRGB (0.0, 0.0, 0.0);
                    cr.Stroke ();

                    DrawDownButton (cr, left, top, width);
                }

                bool writeStringCond1 = !string.IsNullOrWhiteSpace (nonActiveMessage) && (active == -1);
                bool writeStringCond2 = (comboList.Count > 0) && (active >= 0) ;

                if (writeStringCond1 || writeStringCond2) {
                    string text;
                    if (writeStringCond1)
                        text = nonActiveMessage;
                    else
                        text = comboList[active];

                    TouchText t = new TouchText (text);
                    t.textWrap = TouchTextWrap.Shrink;
                    t.font.color = "black";
                    int w = width - height - 10;
                    t.Render (this, left + 10, top, w, height);
                }
            }
        }

        private void DrawDownButton (Context cr, int left, int top, int width) {
            int x = left + (width - height);
            int radius = height / 2;
            cr.MoveTo (x, top);
            cr.LineTo (x + radius, top);
            if (listDropdown) {
                cr.Arc (x + radius, top + radius, radius, -Math.PI / 2, 0);
                cr.LineTo (x + height, top + height);
            } else
                cr.Arc (x + radius, top + radius, radius, -Math.PI / 2, Math.PI / 2);
            cr.LineTo (x, top + height);
            cr.ClosePath ();

            if (listDropdown)
                TouchColor.SetSource (cr, "grey2");
            else
                TouchColor.SetSource (cr, "grey1");
            cr.FillPreserve ();
            cr.LineWidth = 0.85;
            cr.SetSourceRGB (0.0, 0.0, 0.0);
            cr.Stroke ();

            int triOffset = 7;
            int triSize = height - 12;
            int y = top + triOffset;
            x += (triOffset - 3);
            cr.MoveTo (x, y);
            cr.LineTo (x + triSize, y);
            cr.LineTo (x + triSize / 2, y + triSize);
            cr.ClosePath ();
            TouchColor.SetSource (cr, "seca");
            cr.Fill ();
        }

        protected void OnComboBoxPressed (object o, ButtonPressEventArgs args) {
            if (listDropdown)
                secondClick = true;
            else
                secondClick = false;
            GLib.Timeout.Add (20, OnTimerEvent);
            listDropdown = true;
            highlighted = active;
            QueueDraw ();
        }

        protected void OnComboBoxReleased (object o, ButtonReleaseEventArgs args) {
            int x = (int)args.Event.X;
            int y = (int)args.Event.Y;
            
            if ((x >= 0) && (x <= Allocation.Width)) {
                int top = Allocation.Top;

                for (int i = 0; i < comboList.Count; ++i) {
                    int topWindow = i * height + 30;
                    int bottomWindow = (i + 1) * height + 30;
                    if ((y >= topWindow) && (y <= bottomWindow)) {
                        active = i;
                        listDropdown = false;
                        if (ChangedEvent != null)
                            ChangedEvent (this, new ComboBoxChangedEventArgs (active, comboList [active]));
                        QueueDraw ();
                        break;
                    }
                }
            }

            if (secondClick) {
                listDropdown = false;
                QueueDraw ();
            }
        }

        protected bool OnTimerEvent () {
            if (listDropdown) {
                int x, y;
                GetPointer (out x, out y);

                if ((x >= 0) && (x <= Allocation.Width)) {
                    //int top = Allocation.Top + height;

                    for (int i = 0; i < comboList.Count; ++i) {
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
    }
}

