using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchTextBox : EventBox
    {
        public string Text;
        public MyColor FontColor;
        public int FontSize;
        public Justify Justification;

        public TouchTextBox () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.Text = null;
            this.FontColor = new MyColor ("black");
            this.FontSize = 11;
            this.Justification = Justify.Left;

            this.WidthRequest = 100;
            this.HeightRequest = 30;

            this.ExposeEvent += OnExpose;
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
                if (Justification == Justify.Left) {
                    l.Alignment = Pango.Alignment.Left;
                    left += 5;
                } else if (Justification == Justify.Right) {
                    l.Alignment = Pango.Alignment.Right;
                    l.Width = Pango.Units.FromPixels (width - 5);
                } else if (Justification == Justify.Center)
                    l.Alignment = Pango.Alignment.Center;
                l.SetMarkup ("<span color=\"" + FontColor.ToHTML () + "\">" 
                    + Text 
                    + "</span>");
                l.FontDescription = Pango.FontDescription.FromString ("Courier New " + FontSize.ToString ());
                GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), left, top + 6, l);
                l.Dispose ();
            }
        }
    }
}

