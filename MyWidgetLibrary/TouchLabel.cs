using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchLabel : EventBox
    {
        public MyText textRender;

        public string text {
            get {
                return textRender.text;
            }
            set {
                textRender.text = value;
            }
        }

        public MyColor textColor {
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

        public MyAlignment textAlignment {
            get {
                return textRender.alignment;
            }
            set {
                textRender.alignment = value;
            }
        }

        public TouchLabel () {
            this.Visible = true;
            this.VisibleWindow = false;

            textRender = new MyText (string.Empty);

            HeightRequest = 30;
            WidthRequest = 200;

            this.ExposeEvent += OnExpose;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
//                cr.SelectFontFace("Courier New", FontSlant.Normal, FontWeight.Normal);
//                cr.SetFontSize (textSize * 1.4);
//
//                TextExtents te = cr.TextExtents (text);
//                FontExtents fe = cr.FontExtents;
//
//                int x;
//                int y = Allocation.Top + (int)fe.Height - (int)fe.Descent; 
//                if (textAlignment == Justify.Right)
//                    x = Allocation.Left + Allocation.Width - (int)te.Width;
//                else if (textAlignment == Justify.Center)
//                    x = Allocation.Left + (Allocation.Width / 2) - ((int)te.Width / 2);
//                else
//                    x = Allocation.Left;
//
//                textColor.SetSource (cr);
//                cr.MoveTo (x, y);
//                cr.ShowText (text);

                //textRender.Render (cr, Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
                textRender.Render (this, Allocation.Left, Allocation.Top, Allocation.Width);

//                Pango.Layout l = new Pango.Layout (PangoContext);
//                l.Wrap = Pango.WrapMode.WordChar;
//                if (WidthRequest != 200)
//                    l.Width = Pango.Units.FromPixels (WidthRequest);
//
//                if (textAlignment == MyAlignment.Left)
//                    l.Alignment = Pango.Alignment.Left;
//                else if (textAlignment == MyAlignment.Right)
//                    l.Alignment = Pango.Alignment.Right;
//                else // center
//                    l.Alignment = Pango.Alignment.Center;
//                
//                l.SetMarkup ("<span color=" + (char)34 + textColor.ToHTML () + (char)34 + ">" + textRender + "</span>"); 
//                l.FontDescription = Pango.FontDescription.FromString ("Courier New " + textSize.ToString ());
//
//                GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), Allocation.Left, Allocation.Top, l);
//                l.Dispose ();
            }
        }
    }
}

