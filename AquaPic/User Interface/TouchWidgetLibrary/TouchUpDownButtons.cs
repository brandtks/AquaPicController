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
            SetSizeRequest (44, 61);
            up = new TouchButton ();
            down = new TouchButton ();
            buttonsPlaced = false;

            ExposeEvent += (o, args) => {
                if (!buttonsPlaced)
                    PlaceButtons ();
            };
        }

        void PlaceButtons () {
            int width = Allocation.Width;
            int height = (Allocation.Height - 1) / 2;

            up.SetSizeRequest (width, height);
            up.text = Convert.ToChar (0x22C0).ToString (); // 2191
            Put (up, 0, 0);
            up.QueueDraw ();

            down.SetSizeRequest (width, height);
            down.text = Convert.ToChar (0x22C1).ToString (); // 2193
            Put (down, 0, height + 1);
            down.QueueDraw ();

            buttonsPlaced = true;
        }
    }
}

