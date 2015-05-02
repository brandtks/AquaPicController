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
        public MyColor ButtonColor;
        public string Text;
        public MyColor TextColor;
        public ButtonClickAction clickAction;

        public event ButtonReleaseEventHandler TouchButtonReleasedHandler;

        public TouchButton () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.ButtonColor = new MyColor ("pri", 0.9);
            this.Text = "";
            this.TextColor = new MyColor ("black");
            this.HeightRequest = 115;
            this.WidthRequest = 115;
            this.clickAction = ButtonClickAction.Darken;

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

                //cr.Rectangle (left, top, width, height);
                WidgetGlobal.DrawRoundedRectangle (cr, left, top, width, height, 4.0);
                ButtonColor.SetSource (cr);
                cr.Fill ();

                Pango.Layout l = new Pango.Layout (this.PangoContext);
                l.Width = Pango.Units.FromPixels (width);
                l.Wrap = Pango.WrapMode.Word;
                l.Alignment = Pango.Alignment.Center;
                //l.SetText (ButtonLabel);
                l.SetMarkup ("<span color=" + (char)34 + TextColor.ToHTML () + (char)34 + ">" + Text + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");
                //GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), left, (top + (height / 2)) - 6, l);
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), left, top + height, l);
                l.Dispose ();
            }
        }

        protected void onTouchButtonPress (object o, ButtonPressEventArgs args) {
            if (clickAction == ButtonClickAction.NoTransparency)
                ButtonColor.ModifyAlpha (1.0f);
            else if (clickAction == ButtonClickAction.Brighten)
                ButtonColor.ModifyColor (1.25);
            else if (clickAction == ButtonClickAction.Darken)
                ButtonColor.ModifyColor (0.75);

            this.QueueDraw ();
        }

        protected void onTouchButtonRelease (object o, ButtonReleaseEventArgs args) {
            if (clickAction == ButtonClickAction.NoTransparency)
                ButtonColor.RestoreAlpha ();
            else if ((clickAction == ButtonClickAction.Brighten) || (clickAction == ButtonClickAction.Darken))
                ButtonColor.RestoreColor ();

            this.QueueDraw ();

            if (TouchButtonReleasedHandler != null)
                TouchButtonReleasedHandler (this, args);
        }
    }
}

