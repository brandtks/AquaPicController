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
        public MyColor buttonColor;
        public string text;
        public MyColor textColor;
        public ButtonClickAction clickAction;

        public TouchButton () {
            this.Visible = true;
            this.VisibleWindow = false;

            this.buttonColor = "pri";
            this.text = "";
            this.textColor = "black";
            this.HeightRequest = 45;
            this.WidthRequest = 45;
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
                buttonColor.SetSource (cr);
                cr.Fill ();

                Pango.Layout l = new Pango.Layout (this.PangoContext);
                l.Width = Pango.Units.FromPixels (width - 2);
                l.Wrap = Pango.WrapMode.Word;
                l.Alignment = Pango.Alignment.Center;
                //l.SetText (ButtonLabel);
                l.SetMarkup ("<span color=" + (char)34 + textColor.ToHTML () + (char)34 + ">" + text + "</span>"); 
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");
                int y = (top + (height / 2)) - 6;
                y -= ((l.LineCount - 1) * 9);
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), left + 1, y, l);
                //GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), left, top + height, l);
                l.Dispose ();
            }
        }

        protected void onTouchButtonPress (object o, ButtonPressEventArgs args) {
            if (args.Event.Type == Gdk.EventType.ButtonPress) {
                if (clickAction == ButtonClickAction.NoTransparency)
                    buttonColor.ModifyAlpha (1.0f);
                else if (clickAction == ButtonClickAction.Brighten)
                    buttonColor.ModifyColor (1.25);
                else if (clickAction == ButtonClickAction.Darken)
                    buttonColor.ModifyColor (0.75);

                this.QueueDraw ();
            }
        }

        protected void onTouchButtonRelease (object o, ButtonReleaseEventArgs args) {
            if (clickAction == ButtonClickAction.NoTransparency)
                buttonColor.RestoreAlpha ();
            else if ((clickAction == ButtonClickAction.Brighten) || (clickAction == ButtonClickAction.Darken))
                buttonColor.RestoreColor ();

            this.QueueDraw ();
        }
    }
}

