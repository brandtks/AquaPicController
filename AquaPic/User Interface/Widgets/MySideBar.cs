using System;
using Cairo;
using Gtk;
using TouchWidgetLibrary;
using AquaPic.Utilites;

namespace AquaPic
{
    public class MySideBar : EventBox
    {
        private bool clicked, expanded;
        //private uint clickTimer;
        //private int click1, click2;

        public MySideBar () {
            Visible = true;
            VisibleWindow = false;

            SetSizeRequest (25, 461);

            ExposeEvent += onExpose;
            ButtonPressEvent += OnButtonPress;
            ButtonReleaseEvent += OnButtonRelease;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                if (expanded) {
                    cr.Rectangle (left, top, width, height);
                    TouchColor.SetSource (cr, "grey0", 0.80);
                    cr.Fill ();

                    cr.MoveTo (left, top);
                    cr.LineTo (200, top);
                    cr.Arc (-850.105, (double)top + ((double)height / 2), 1075.105, (192.4).ToRadians (), (167.6).ToRadians ());
                    cr.LineTo (200, top + height);
                    cr.LineTo (left, top + height);
                    cr.LineTo (left, top);
                    cr.ClosePath ();
                    TouchColor.SetSource (cr, "grey3", 0.80);
                    cr.Fill ();
                }
            }
        }

        protected void OnButtonPress (object o, ButtonPressEventArgs args) {
            if (expanded) {
                //clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
                //clicked = true;


            } else {

            }
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            clicked = false;
            if (expanded) {
                //clicked = false;

                expanded = false;
                SetSizeRequest (25, 461);
                QueueDraw ();
            } else {
                expanded = true;

                SetSizeRequest (800, 461);
                QueueDraw ();
            }
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);

                QueueDraw ();
            }

            return clicked;
        }
    }
}

