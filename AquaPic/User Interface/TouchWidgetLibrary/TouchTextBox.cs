using System;

using Gtk;
using Cairo;

namespace TouchWidgetLibrary
{
    public delegate void TextChangedHandler (object sender, TextChangedEventArgs args);

    public class TextChangedEventArgs : EventArgs {
        public string text;
        public bool keepText;

        public TextChangedEventArgs (string text) {
            this.text = text;
            keepText = true;
        }
    }

    public class TouchTextBox : EventBox
    {
        public string name;
        public string text {
            get {
                return textRender.text;
            }
            set {
                textRender.text = value;
            }
        }

        public TouchColor textColor {
            get {
                return textRender.font.color;
            }
            set {
                textRender.font.color = value;
            }
        }

        public int textSize {
            get {
                return textRender.font.size;
            }
            set {
                textRender.font.size = value;
            }
        }

        public TouchAlignment textAlignment {
            get {
                return textRender.alignment;
            }
            set {
                textRender.alignment = value;
            }
        }
        public TouchColor bkgndColor;
        public bool enableTouch;
        public bool includeTimeFunctions;
        public event TextChangedHandler TextChangedEvent;
        public TouchText textRender;

        public TouchTextBox () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.WidthRequest = 100;
            this.HeightRequest = 30;

            textRender = new TouchText ();
            textRender.textWrap = TouchTextWrap.Shrink;

            this.text = string.Empty;
            name = string.Empty;
            this.textColor = new TouchColor ("black");
            this.textAlignment = TouchAlignment.Left;

            bkgndColor = "grey4";

            enableTouch = false;
            includeTimeFunctions = false;

            this.ExposeEvent += OnExpose;
            ButtonReleaseEvent += OnTouchButtonRelease;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, 3);
                bkgndColor.SetSource (cr);
                cr.FillPreserve ();
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.LineWidth = 0.75;
                cr.Stroke ();

                textRender.Render (this, left + 3, top, width - 6, height);
            }
        }

        protected void OnTouchButtonRelease (object o, ButtonReleaseEventArgs args) {
            if (enableTouch) {
                TouchNumberInput t;

                var parent = this.Toplevel as Gtk.Window;
                if (parent != null) {
                    if (parent.IsTopLevel)
                        t = new TouchNumberInput (includeTimeFunctions, parent);
                    else
                        t = new TouchNumberInput (includeTimeFunctions);
                } else
                    t = new TouchNumberInput (includeTimeFunctions);

                if (!string.IsNullOrWhiteSpace (name))
                    t.Title = name;
                
                t.NumberSetEvent += (value) => {
                    if (!string.IsNullOrWhiteSpace (value)) {
                        TextChangedEventArgs a = new TextChangedEventArgs (value);

                        if (TextChangedEvent != null)
                            TextChangedEvent (this, a);

                        if (a.keepText) 
                            text = a.text;
                    }
                };
                
                t.Run ();
                t.Destroy ();
                QueueDraw ();
            }
        }
    }
}

