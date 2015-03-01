using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class MyLabel : EventBox
    {
        public string text;
        public string color;
        public int textSize;

        public MyLabel () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.text = null;
            this.color = "black";
            this.textSize = 11;

            this.ExposeEvent += OnExpose;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {

                Pango.Layout l = new Pango.Layout (PangoContext);
                l.Wrap = Pango.WrapMode.WordChar;
                l.Alignment = Pango.Alignment.Left;
                l.SetMarkup ("<span color=" + (char)34 + color + (char)34 + ">" + text + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New " + textSize.ToString ());
                GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), Allocation.Left, Allocation.Top, l);
                l.Dispose ();
            }
        }
    }
}

