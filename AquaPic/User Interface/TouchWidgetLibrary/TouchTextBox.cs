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
        public string text;
        public string name;
        public TouchColor textColor;
        public int textSize;
        public TouchAlignment textAlignment;
        public TouchColor bkgndColor;
        public bool enableTouch;
        public bool includeTimeFunctions;
        public event TextChangedHandler TextChangedEvent;

        public TouchTextBox () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.WidthRequest = 100;
            this.HeightRequest = 30;

            this.text = string.Empty;
            name = string.Empty;
            this.textColor = new TouchColor ("black");
            this.textSize = 11;
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

//                Pango.Layout l = new Pango.Layout (PangoContext);
//                l.Width = Pango.Units.FromPixels (width);
//                l.Wrap = Pango.WrapMode.Word;
//                if (textAlignment == MyAlignment.Left) {
//                    l.Alignment = Pango.Alignment.Left;
//                    left += 5;
//                } else if (textAlignment == MyAlignment.Right) {
//                    l.Alignment = Pango.Alignment.Right;
//                    l.Width = Pango.Units.FromPixels (width - 5);
//                } else if (textAlignment == MyAlignment.Center)
//                    l.Alignment = Pango.Alignment.Center;
//                l.SetMarkup ("<span color=\"" + textColor.ToHTML () + "\">" 
//                    + text 
//                    + "</span>");
//                l.FontDescription = Pango.FontDescription.FromString ("Sans " + textSize.ToString ());
//                GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), left, top + 6, l);
//                l.Dispose ();

                TouchText t = text;
                t.font.color = textColor;
                t.font.size = textSize;
                t.alignment = textAlignment;
                t.textWrap = TouchTextWrap.Shrink;
                t.Render (this, left + 3, top, width - 6, height);
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

