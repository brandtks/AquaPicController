using System;
using Gtk;
using Cairo;

namespace TouchWidgetLibrary  
{
    public class TouchUpDownButtons : Fixed
    {
        public TouchButton up;
        public TouchButton down;
        private bool buttonsPlaced;

        public TouchUpDownButtons () {
            SetSizeRequest (98, 46);
            up = new TouchButton ();
            down = new TouchButton ();
            buttonsPlaced = false;

            ExposeEvent += (o, args) => {
                if (!buttonsPlaced)
                    PlaceButtons ();
            };
        }

        void PlaceButtons () {
            int width = (Allocation.Width - 1) / 2;
            int height = Allocation.Height;

            up.SetSizeRequest (width, height);
            up.text = Convert.ToChar (0x22C0).ToString (); // 2191
            Put (up, 0, 0);
            up.QueueDraw ();

            down.SetSizeRequest (width, height);
            down.text = Convert.ToChar (0x22C1).ToString (); // 2193
            Put (down, width + 1, 0);
            down.QueueDraw ();

            buttonsPlaced = true;
        }
    }
}

