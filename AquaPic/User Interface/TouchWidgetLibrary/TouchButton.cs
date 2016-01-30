using System;
using Gtk;
using Cairo;

namespace TouchWidgetLibrary  
{
    public enum ButtonClickAction : byte {
        None = 0,
        NoTransparency,
        Brighten,
        Darken
    }

    public class TouchButton : EventBox
    {
        public TouchText render;
        public TouchColor buttonColor;
        public ButtonClickAction clickAction;

        public string text {
            get {
                return render.text;
            }
            set {
                render.text = value;
            }
        }

        public TouchColor textColor {
            get {
                return render.font.color;
            }
            set {
                render.font.color = value;
            }
        }

        public int textSize {
            get {
                return render.font.size;
            }
            set {
                render.font.size = value;
            }
        }

        public TouchAlignment textAlignment {
            get {
                return render.alignment;
            }
            set {
                render.alignment = value;
            }
        }

        public TouchButton () {
            this.Visible = true;
            this.VisibleWindow = false;

            render = new TouchText ();
            this.buttonColor = "pri";
            this.text = "";
            this.textColor = "black";
            this.HeightRequest = 45;
            this.WidthRequest = 45;
            this.textAlignment = TouchAlignment.Center;
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

                TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, 4.0);
                buttonColor.SetSource (cr);
                cr.Fill ();

                render.Render (this, left + 3, top, width - 6, height);
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

