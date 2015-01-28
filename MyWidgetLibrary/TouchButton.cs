using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary  
{
    public enum ButtonClickAction : byte {
        None = 0,
        NoTransparency,
        Brighten,
        Darken
    }

    public class TouchButton : EventBox
    {
        public Colors ButtonColor { get; set; }
        public string Text { get; set; }
        public string TextColor { get; set; }
        public ButtonClickAction clickAction { get; set; }

        public event ButtonReleaseEventHandler TouchButtonReleasedHandler;

        public TouchButton () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.ButtonColor = new Colors ("red", true);
            this.Text = "";
            this.TextColor = "black";
            this.HeightRequest = 115;
            this.WidthRequest = 115;
            this.clickAction = ButtonClickAction.NoTransparency;

            this.ExposeEvent += onExpose;
            this.ButtonPressEvent += onTouchButtonPress;
            this.ButtonReleaseEvent += onTouchButtonRelease;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int height = Allocation.Height;
                int width = Allocation.Width;
                int top = Allocation.Top;
                int left = Allocation.Left;

                cr.Rectangle (left, top, width, height);
                cr.SetSourceRGBA (ButtonColor.R, ButtonColor.G, ButtonColor.B, ButtonColor.A);
                cr.Fill ();

                Pango.Layout l = new Pango.Layout (this.PangoContext);
                l.Width = Pango.Units.FromPixels (width);
                l.Wrap = Pango.WrapMode.Word;
                l.Alignment = Pango.Alignment.Center;
                //l.SetText (ButtonLabel);
                l.SetMarkup ("<span color=" + (char)34 + TextColor + (char)34 + ">" + Text + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 12");
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), left, (top + (height / 2)) - 6, l);
                l.Dispose ();
            }
        }

        protected void onTouchButtonPress (object o, ButtonPressEventArgs args) {
            if (clickAction == ButtonClickAction.NoTransparency)
                ButtonColor.setTemporaryNewAlpha (1.0f);
            else if (clickAction == ButtonClickAction.Brighten)
                ButtonColor.ModifyColor (1.05f);
            else if (clickAction == ButtonClickAction.Darken)
                ButtonColor.ModifyColor (0.75f);

            this.QueueDraw ();
        }

        protected void onTouchButtonRelease (object o, ButtonReleaseEventArgs args) {
            if (clickAction == ButtonClickAction.NoTransparency)
                ButtonColor.restoreAlpha ();
            else if ((clickAction == ButtonClickAction.Brighten) || (clickAction == ButtonClickAction.Darken))
                ButtonColor.restoreColor ();

            this.QueueDraw ();

            if (TouchButtonReleasedHandler != null)
                TouchButtonReleasedHandler (this, args);
        }
    }
}

