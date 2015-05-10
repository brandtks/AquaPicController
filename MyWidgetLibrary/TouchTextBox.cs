using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public delegate void TextChangedHandler (object sender, TextChangedEventArgs args);

    public class TextChangedEventArgs : EventArgs {
        public string text;

        public TextChangedEventArgs (string text) {
            this.text = text;
        }
    }

    public class TouchTextBox : EventBox
    {
        public string text;
        public MyColor textColor;
        public int textSize;
        public Justify textAlignment;
        public bool enableTouch;
        public TextChangedHandler TextChangedEvent;

        public TouchTextBox () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.text = null;
            this.textColor = new MyColor ("black");
            this.textSize = 11;
            this.textAlignment = Justify.Left;

            this.WidthRequest = 100;
            this.HeightRequest = 30;

            this.ExposeEvent += OnExpose;
            ButtonReleaseEvent += OnTouchButtonRelease;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                WidgetGlobal.DrawRoundedRectangle (cr, left, top, width, height, 3);
                cr.SetSourceRGB (0.85, 0.85, 0.85);
                cr.FillPreserve ();
                cr.SetSourceRGB (0.0, 0.0, 0.0);
                cr.LineWidth = 0.75;
                cr.Stroke ();

                Pango.Layout l = new Pango.Layout (PangoContext);
                l.Width = Pango.Units.FromPixels (width);
                l.Wrap = Pango.WrapMode.Word;
                if (textAlignment == Justify.Left) {
                    l.Alignment = Pango.Alignment.Left;
                    left += 5;
                } else if (textAlignment == Justify.Right) {
                    l.Alignment = Pango.Alignment.Right;
                    l.Width = Pango.Units.FromPixels (width - 5);
                } else if (textAlignment == Justify.Center)
                    l.Alignment = Pango.Alignment.Center;
                l.SetMarkup ("<span color=\"" + textColor.ToHTML () + "\">" 
                    + text 
                    + "</span>");
                l.FontDescription = Pango.FontDescription.FromString ("Courier New " + textSize.ToString ());
                GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), left, top + 6, l);
                l.Dispose ();
            }
        }

        protected void OnTouchButtonRelease (object o, ButtonReleaseEventArgs args) {
            if (enableTouch) {
                TouchNumberInput t = new TouchNumberInput ();
                t.NumberSetEvent += (value) => {
                        text = value;

                        if (TextChangedEvent != null)
                            TextChangedEvent (this, new TextChangedEventArgs (text));
                    };
                
                t.Show ();
            }
        }
    }
}

