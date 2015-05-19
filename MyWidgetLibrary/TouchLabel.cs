using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchLabel : EventBox
    {
        public string text;
        public MyColor textColor;
        public int textSize;
        public int areaWidth;
        public Justify textAlignment;

        public TouchLabel () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.text = null;
            this.textColor = new MyColor ("black");
            this.textSize = 11;
            this.textAlignment = Justify.Left;
            areaWidth = 0;
            HeightRequest = 30;
            WidthRequest = 200;

            this.ExposeEvent += OnExpose;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;

                Pango.Layout l = new Pango.Layout (PangoContext);
                l.Wrap = Pango.WrapMode.WordChar;
                if (areaWidth != 0)
                    l.Width = Pango.Units.FromPixels (areaWidth);
                
                if (textAlignment == Justify.Left)
                    l.Alignment = Pango.Alignment.Left;
                else if (textAlignment == Justify.Right)
                    l.Alignment = Pango.Alignment.Right;
                else // center
                    l.Alignment = Pango.Alignment.Center;
                
                l.SetMarkup ("<span color=" + (char)34 + textColor.ToHTML () + (char)34 + ">" + text + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New " + textSize.ToString ());

//                if (Justification == Justify.Right) {
//                    int width, height;
//                    l.GetPixelSize (out width, out height);
//                    left -= width;
//                }

                GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), left, top, l);
                l.Dispose ();
            }
        }
    }
}

