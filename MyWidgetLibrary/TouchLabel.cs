using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchLabel : EventBox
    {
        public string Text;
        public MyColor FontColor;
        public int FontSize;
        public Justify Justification;

        public TouchLabel () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.Text = null;
            this.FontColor = new MyColor ("black");
            this.FontSize = 11;
            this.Justification = Justify.Left;

            this.ExposeEvent += OnExpose;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;

                Pango.Layout l = new Pango.Layout (PangoContext);
                l.Wrap = Pango.WrapMode.WordChar;
                l.Alignment = Pango.Alignment.Left;
                l.SetMarkup ("<span color=" + (char)34 + FontColor.ToHTML () + (char)34 + ">" + Text + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New " + FontSize.ToString ());

                if (Justification == Justify.Right) {
                    int width, height;
                    l.GetPixelSize (out width, out height);
                    left -= width;
                }

                GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), left, top, l);
                l.Dispose ();
            }
        }
    }
}

