using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchLabel : EventBox
    {
        public string Text;
        public MyColor TextColor;
        public int TextSize;
        public int AreaWidth;
        public Justify TextAlignment;

        public TouchLabel () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.Text = null;
            this.TextColor = new MyColor ("black");
            this.TextSize = 11;
            this.TextAlignment = Justify.Left;
            AreaWidth = 0;

            this.ExposeEvent += OnExpose;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;

                Pango.Layout l = new Pango.Layout (PangoContext);
                l.Wrap = Pango.WrapMode.WordChar;
                if (AreaWidth != 0)
                    l.Width = Pango.Units.FromPixels (AreaWidth);
                
                if (TextAlignment == Justify.Left)
                    l.Alignment = Pango.Alignment.Left;
                else if (TextAlignment == Justify.Right)
                    l.Alignment = Pango.Alignment.Right;
                else // center
                    l.Alignment = Pango.Alignment.Center;
                
                l.SetMarkup ("<span color=" + (char)34 + TextColor.ToHTML () + (char)34 + ">" + Text + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New " + TextSize.ToString ());

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

